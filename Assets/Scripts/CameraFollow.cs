using UnityEngine;

namespace SphereTrials
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Seguimiento")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private string playerTag = "Player";

        private Transform target;

        private void Start()
        {
            // Busca automáticamente el jugador por tag
            FindTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                // Si el jugador reaparece (por respawn o cambio de nivel)
                FindTarget();
                return;
            }

            // Posición deseada (centro del jugador + offset fijo)
            Vector3 desiredPosition = target.position + offset;

            // Movimiento suave, sin afectar la física del jugador
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Siempre mirar al jugador
            Vector3 direction = target.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, smoothSpeed * Time.deltaTime);
            }
        }

        private void FindTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
                target = player.transform;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
