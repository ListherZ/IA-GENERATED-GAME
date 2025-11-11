using UnityEngine;

namespace SphereTrials
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private float rotationSpeed = 90f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            GameManager.Instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
