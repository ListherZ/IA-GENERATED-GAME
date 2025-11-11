using UnityEngine;

namespace SphereTrials
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveForce = 15f;
        [SerializeField] private float maxHorizontalSpeed = 12f;

        [Header("Salto")]
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float groundCheckRadius = 0.35f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Control de rebote")]
        [Tooltip("Límite de velocidad vertical hacia arriba para evitar rebotes excesivos.")]
        [SerializeField] private float maxUpwardSpeed = 8f;

        [Header("Caída")]
        [SerializeField] private float fallThresholdY = -20f;

        private Rigidbody _rb;
        private Camera _mainCam;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            // Ajustes recomendados para sentirlo más estable
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.useGravity = true;

            _mainCam = Camera.main;
        }

        private void Update()
        {
            HandleJumpInput();
            CheckFallDeath();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            LimitHorizontalSpeed();
            LimitUpwardBounce();
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
                return;

            // Movimiento relativo a la cámara para mejor control
            Vector3 camForward = _mainCam.transform.forward;
            Vector3 camRight = _mainCam.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h).normalized;
            Vector3 force = moveDir * moveForce;

            _rb.AddForce(force, ForceMode.Acceleration);
        }

        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                // Resetear componente vertical antes de saltar
                Vector3 vel = _rb.linearVelocity;
                vel.y = 0f;
                _rb.linearVelocity = vel;

                _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }

        private bool IsGrounded()
        {
            // Chequeo simple de suelo alrededor del centro de la esfera
            return Physics.CheckSphere(transform.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }

        private void CheckFallDeath()
        {
            if (transform.position.y < fallThresholdY)
            {
                GameManager.Instance.PlayerDied();
            }
        }

        /// <summary>
        /// Limita solo la velocidad horizontal para que no se vuelva incontrolable.
        /// </summary>
        private void LimitHorizontalSpeed()
        {
            Vector3 vel = _rb.linearVelocity;
            Vector3 horizontal = new Vector3(vel.x, 0f, vel.z);

            if (horizontal.magnitude > maxHorizontalSpeed)
            {
                horizontal = horizontal.normalized * maxHorizontalSpeed;
                _rb.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
            }
        }

        /// <summary>
        /// Evita rebotes muy altos limitando la velocidad vertical hacia arriba.
        /// No afecta la caída normal.
        /// </summary>
        private void LimitUpwardBounce()
        {
            Vector3 vel = _rb.linearVelocity;

            if (vel.y > maxUpwardSpeed)
            {
                vel.y = maxUpwardSpeed;
                _rb.linearVelocity = vel;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        }
    }
}
