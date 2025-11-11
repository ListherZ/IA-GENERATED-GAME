using UnityEngine;

namespace SphereTrials
{
    public class Hazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private bool destroyOnHit = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.PlayerHit(damage);
                else
                    Debug.LogWarning("GameManager.Instance es null al aplicar daño.");

                if (destroyOnHit)
                    Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.PlayerHit(damage);
                else
                    Debug.LogWarning("GameManager.Instance es null al aplicar daño.");

                if (destroyOnHit)
                    Destroy(gameObject);
            }
        }
    }
}
