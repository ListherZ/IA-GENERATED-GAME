using UnityEngine;

namespace SphereTrials
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveForce = 10f;          // Fuerza moderada
        [SerializeField] private float maxHorizontalSpeed = 8f;  // Máx velocidad en plano

        [Header("Salto")]
        [SerializeField] private float jumpForce = 4.5f;         // Altura de salto
        [SerializeField] private float groundCheckRadius = 0.35f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Límites de velocidad")]
        [SerializeField] private float maxUpwardSpeed = 6f;      // Máx hacia arriba
        [SerializeField] private float maxFallSpeed = 20f;       // Máx caída

        [Header("Caída fuera del mapa")]
        [SerializeField] private float fallThresholdY = -20f;

        private Rigidbody _rb;
        private Camera _mainCam;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            _rb.useGravity = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

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
            ClampVelocity();
        }

        // ================== MOVIMIENTO ==================
        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f)
                return;

            // Dirección relativa a la cámara (solo plano XZ)
            Vector3 camForward = _mainCam.transform.forward;
            Vector3 camRight = _mainCam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 dir = (camForward * v + camRight * h).normalized;

            // Fuerza suave (no explosiva)
            _rb.AddForce(dir * moveForce, ForceMode.Force);
        }

        // ================== SALTO ==================
        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                // Limpia la componente vertical para que el salto sea consistente
                Vector3 vel = _rb.linearVelocity;
                vel.y = 0f;
                _rb.linearVelocity = vel;

                // Impulso hacia arriba (salto físico)
                _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        private bool IsGrounded()
        {
            // Detección simple: esfera alrededor del centro del jugador
            return Physics.CheckSphere(transform.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }

        // ================== CAÍDA FUERA DEL MAPA ==================
        private void CheckFallDeath()
        {
            // Si no hay GameManager, no hacemos nada
            if (GameManager.Instance == null)
                return;

            // Si ya se está procesando una muerte, NO vuelvas a llamar
            if (GameManager.Instance.IsProcessingDeath)
                return;

            // Solo cuando cruza el umbral por primera vez
            if (transform.position.y < fallThresholdY)
            {
                GameManager.Instance.PlayerDied();
            }
        }

        // ================== CONTROL DE VELOCIDAD ==================
        private void ClampVelocity()
        {
            Vector3 vel = _rb.linearVelocity;

            // Limitar horizontal
            Vector3 horizontal = new Vector3(vel.x, 0f, vel.z);
            if (horizontal.magnitude > maxHorizontalSpeed)
            {
                horizontal = horizontal.normalized * maxHorizontalSpeed;
            }

            // Limitar vertical hacia arriba
            if (vel.y > maxUpwardSpeed)
            {
                vel.y = maxUpwardSpeed;
            }

            // Limitar caída
            if (vel.y < -maxFallSpeed)
            {
                vel.y = -maxFallSpeed;
            }

            _rb.linearVelocity = new Vector3(horizontal.x, vel.y, horizontal.z);
        }

        // ================== DEBUG VISUAL ==================
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        }
    }
}
