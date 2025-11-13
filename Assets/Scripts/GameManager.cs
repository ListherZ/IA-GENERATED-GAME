using UnityEngine;
using TMPro;
using UnityEngine.UI; // Para botones / imágenes si los necesitas

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

        [Header("Referencias UI (HUD)")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI lossesText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Pantalla final")]
        [Tooltip("CanvasGroup en una imagen negra a pantalla completa (alpha=0 de inicio).")]
        [SerializeField] private CanvasGroup fadeGroup;              // ← negro a pantalla completa
        [Tooltip("Panel con el texto y los botones (inactivo al inicio).")]
        [SerializeField] private GameObject endPanel;                // ← panel con texto + botones
        [SerializeField] private TextMeshProUGUI endTitleText;       // ← texto blanco central
        [SerializeField] private Button restartButton;               // ← botón 'Volver a empezar'
        [SerializeField] private Button quitButton;                  // ← botón 'Salir'
        [SerializeField] private float fadeDuration = 1.0f;          // ← duración del fundido

        private MapManager _mapManager;

        // Protección contra múltiples muertes/acciones
        private bool isProcessingDeath = false;
        public bool IsProcessingDeath => isProcessingDeath;
        private bool isRunCompleted = false;

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
            // Botones (si están asignados) → conectar acciones
            if (restartButton != null) restartButton.onClick.AddListener(RestartRun);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            // Asegurar estados UI iniciales
            if (endPanel != null) endPanel.SetActive(false);
            SetFadeAlpha(0f);

            // Buscar un MapManager activo si no se ha registrado aún
            if (_mapManager == null)
                _mapManager = FindFirstObjectByType<MapManager>();

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
            if (isProcessingDeath || isRunCompleted) return;

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
            if (isProcessingDeath || isRunCompleted) return;

            isProcessingDeath = true;
            lossesCount++;
            currentHealth = 0;

            ShowMessage("Has muerto. Reiniciando nivel...");
            UpdateUI();

            // Reiniciar nivel actual
            if (_mapManager != null)
            {
                _mapManager.ReloadCurrentLevel(currentLevelIndex);
                score = 0;
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
            if (isRunCompleted) return;

            ShowMessage("🎉 ¡Nivel completado!");
            currentLevelIndex++;

            if (_mapManager == null) return;

            if (currentLevelIndex >= _mapManager.LevelCount)
            {
                // TODOS LOS NIVELES COMPLETOS → secuencia final
                StartCoroutine(ShowEndSequence());
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
        // UI (HUD)
        // -----------------------------------------------------
        private void UpdateUI()
        {
            if (healthText != null)
                healthText.text = $"Health: {currentHealth} of {maxHealth}";

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

        // -----------------------------------------------------
        // SECUENCIA FINAL: Fade a negro + panel con botones
        // -----------------------------------------------------
        private System.Collections.IEnumerator ShowEndSequence()
        {
            isRunCompleted = true;      // bloquea daño/muerte y nuevas entradas
            ClearMessage();

            // 1) Fade a negro
            yield return FadeTo(1f, fadeDuration);

            // 2) Mostrar panel final encima del negro
            if (endPanel != null) endPanel.SetActive(true);
            if (endTitleText != null) endTitleText.text = "¡Has completado todos los niveles!";

            // (El jugador verá: fondo negro, texto blanco, dos botones)
        }

        // -----------------------------------------------------
        // Controles de fade
        // -----------------------------------------------------
        private void SetFadeAlpha(float a)
        {
            if (fadeGroup == null) return;
            fadeGroup.alpha = Mathf.Clamp01(a);
            fadeGroup.blocksRaycasts = a > 0.001f; // bloquea clics cuando está visible
            fadeGroup.interactable = a > 0.001f;   // útil si el panel final está dentro
        }

        private System.Collections.IEnumerator FadeTo(float target, float duration)
        {
            if (fadeGroup == null || duration <= 0f)
            {
                SetFadeAlpha(target);
                yield break;
            }

            float start = fadeGroup.alpha;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                SetFadeAlpha(Mathf.Lerp(start, target, k));
                yield return null;
            }

            SetFadeAlpha(target);
        }

        // -----------------------------------------------------
        // Botones del final
        // -----------------------------------------------------
        public void RestartRun()
        {
            // Oculta panel final y hace fade out
            if (endPanel != null) endPanel.SetActive(false);
            StartCoroutine(RestartSequenceCo());
        }

        private System.Collections.IEnumerator RestartSequenceCo()
        {
            // 1) Asegurar negro (por si venimos del panel)
            yield return FadeTo(1f, 0.1f);

            // 2) Resetear progreso
            isRunCompleted = false;
            isProcessingDeath = false;
            lossesCount = 0;
            score = 0;
            currentLevelIndex = 0;

            // 3) Cargar primer nivel
            StartLevel(currentLevelIndex);

            // 4) Fade desde negro a juego
            yield return FadeTo(0f, fadeDuration);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
