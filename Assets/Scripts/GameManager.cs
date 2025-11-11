using UnityEngine;
using TMPro;

namespace SphereTrials
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Salud del Jugador")]
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private int currentHealth;

        [Header("Progreso del Juego")]
        [SerializeField] private int currentLevelIndex = 0;
        [SerializeField] private int lossesCount = 0;
        [SerializeField] private int score = 0;

        [Header("Referencias de UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI lossesText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI scoreText;

        private MapManager _mapManager;

        // -----------------------------------------------------------
        // Ciclo de vida
        // -----------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Buscar MapManager si aún no se registró
            if (_mapManager == null)
                _mapManager = FindFirstObjectByType<MapManager>();

            if (_mapManager != null)
                StartLevel(currentLevelIndex);
            else
                Debug.LogWarning("GameManager: No se encontró un MapManager en la escena.");
        }

        // -----------------------------------------------------------
        // Registro del gestor de mapas
        // -----------------------------------------------------------
        public void RegisterMapManager(MapManager manager)
        {
            _mapManager = manager;
        }

        // -----------------------------------------------------------
        // Control de niveles
        // -----------------------------------------------------------
        private void StartLevel(int levelIndex)
        {
            if (_mapManager == null) return;

            currentHealth = maxHealth;
            ClearMessage();
            _mapManager.LoadLevel(levelIndex);
            UpdateUI();
        }

        // -----------------------------------------------------------
        // Sistema de daño y muerte
        // -----------------------------------------------------------
        public void PlayerHit(int damage)
        {
            currentHealth -= Mathf.Abs(damage);

            if (currentHealth <= 0)
            {
                PlayerDied();
            }
            else
            {
                UpdateUI();
            }
        }

        public void PlayerDied()
        {
            lossesCount++;
            currentHealth = 0;
            ShowMessage("💀 Has muerto. Reiniciando nivel...");

            UpdateUI();

            // Reinicia el nivel actual
            if (_mapManager != null)
            {
                _mapManager.ReloadCurrentLevel(currentLevelIndex);
            }

            // Restaura vida y limpia mensaje
            currentHealth = maxHealth;
            UpdateUI();
            ClearMessageDelayed(1.5f);
        }

        // -----------------------------------------------------------
        // Ganar nivel / objetivo
        // -----------------------------------------------------------
        public void PlayerReachedGoal()
        {
            ShowMessage("🎉 ¡Nivel completado!");
            currentLevelIndex++;

            if (_mapManager == null) return;

            if (currentLevelIndex >= _mapManager.LevelCount)
            {
                ShowMessage("🏁 ¡Has completado todos los niveles!");
            }
            else
            {
                StartLevel(currentLevelIndex);
            }
        }

        // -----------------------------------------------------------
        // Sistema de puntuación
        // -----------------------------------------------------------
        public void AddScore(int value)
        {
            score += Mathf.Max(0, value);
            UpdateUI();
        }

        // -----------------------------------------------------------
        // UI
        // -----------------------------------------------------------
        private void UpdateUI()
        {
            if (healthText != null)
                healthText.text = $"Health: {currentHealth}/{maxHealth}";

            if (levelText != null)
                levelText.text = $"Level: {currentLevelIndex + 1}";

            if (lossesText != null)
                lossesText.text = $"Losses: {lossesCount}";

            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        private void ShowMessage(string msg)
        {
            if (messageText != null)
            {
                messageText.gameObject.SetActive(true);
                messageText.text = msg;
            }
        }

        private void ClearMessage()
        {
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
                messageText.text = string.Empty;
            }
        }

        private void ClearMessageDelayed(float delay)
        {
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), delay);
        }
    }
}
