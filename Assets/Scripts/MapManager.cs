using System.Collections.Generic;
using UnityEngine;

namespace SphereTrials
{
    public class MapManager : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField] private List<GameObject> levelPrefabs = new List<GameObject>();

        [Header("Player")]
        [SerializeField] private PlayerController playerPrefab;
        [SerializeField] private CameraFollow cameraFollow;

        private GameObject _currentLevelInstance;
        private PlayerController _currentPlayer;

        public int LevelCount => levelPrefabs.Count;

        private void Awake()
        {
            // Si el GameManager ya existe, nos registramos.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterMapManager(this);
            }
        }

        // Llamado por GameManager cuando quiera cargar un nivel
        public void LoadLevel(int index)
        {
            if (index < 0 || index >= levelPrefabs.Count)
            {
                Debug.LogWarning($"MapManager: Level index {index} fuera de rango.");
                return;
            }

            if (_currentLevelInstance != null)
            {
                Destroy(_currentLevelInstance);
            }

            _currentLevelInstance = Instantiate(levelPrefabs[index], Vector3.zero, Quaternion.identity);

            // Buscar punto de spawn
            Transform spawnPoint = _currentLevelInstance.transform.Find("SpawnPoint");
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(0f, 2f, 0f);

            // Crear o resetear jugador
            if (_currentPlayer == null)
            {
                _currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(_currentPlayer.transform);
                }
                else
                {
                    Debug.LogWarning("MapManager: CameraFollow no asignado.");
                }
            }
            else
            {
                _currentPlayer.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

                var rb = _currentPlayer.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        public void ReloadCurrentLevel(int index)
        {
            LoadLevel(index);
        }
    }
}
