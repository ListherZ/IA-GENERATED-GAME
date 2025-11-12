using UnityEngine;
using TMPro;

namespace SphereTrials
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Salud del jugador")]
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private int currentHealth;

        [Header("Progreso")]
        [SerializeField] private int currentLevelIndex = 0;
        [SerializeField] private int lossesCount = 0;
        [SerializeField] private int score = 0;

        [Header("Referencias UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI lossesText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI scoreText;

        private MapManager _mapManager;

        // --- Protección contra múltiples muertes ---
        private bool isProcessingDeath = false;
        public bool IsProcessingDeath => isProcessingDeath;

        // -----------------------------------------------------
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
            // Busca un MapManager activo si no se ha registrado aún
            if (_mapManager == null)
                _mapManager = FindObjectOfType<MapManager>();

            if (_mapManager != null)
                StartLevel(currentLevelIndex);
            else
                Debug.LogWarning("GameManager: No se encontró MapManager en la escena.");
        }

        public void RegisterMapManager(MapManager manager)
        {
            _mapManager = manager;
        }

        // -----------------------------------------------------
        // Inicio de nivel
        // -----------------------------------------------------
        private void StartLevel(int levelIndex)
        {
            if (_mapManager == null) return;

            currentHealth = maxHealth;
            ClearMessage();
            _mapManager.LoadLevel(levelIndex);
            UpdateUI();
        }

        // -----------------------------------------------------
        // Daño, muerte y reinicio
        // -----------------------------------------------------
        public void PlayerHit(int damage)
        {
            if (isProcessingDeath)
                return;

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
            if (isProcessingDeath)
                return;

            isProcessingDeath = true;
            lossesCount++;
            currentHealth = 0;

            ShowMessage("💀 Has muerto. Reiniciando nivel...");
            UpdateUI();

            // Reiniciar nivel actual
            if (_mapManager != null)
            {
                _mapManager.ReloadCurrentLevel(currentLevelIndex);
            }

            // Restaurar vida
            currentHealth = maxHealth;
            UpdateUI();

            // Ocultar mensaje después de un rato
            ClearMessageDelayed(1.5f);

            // Liberar flag de protección
            Invoke(nameof(ResetDeathFlag), 0.1f);
        }

        private void ResetDeathFlag()
        {
            isProcessingDeath = false;
        }

        // -----------------------------------------------------
        // Meta / siguiente nivel
        // -----------------------------------------------------
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

        // -----------------------------------------------------
        // Puntaje
        // -----------------------------------------------------
        public void AddScore(int value)
        {
            score += Mathf.Max(0, value);
            UpdateUI();
        }

        // -----------------------------------------------------
        // UI
        // -----------------------------------------------------
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
