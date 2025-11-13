using UnityEngine;

namespace SphereTrials
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private GameObject pickupEffect; // 👈 Prefab del efecto de partículas

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (pickupEffect != null)
            {
                GameObject efecto = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(efecto, 2f); // se destruye tras 2 segundos
            }


            // Suma el puntaje
            GameManager.Instance.AddScore(scoreValue);

            // Destruye el objeto
            Destroy(gameObject);
        }
    }
}
