using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CableHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text cableStateText;
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Image powerIndicator;

    [Header("Animacion")]
    [SerializeField] private float scorePopDuration = 0.3f;
    [SerializeField] private float damageOverlayDuration = 0.3f;

    private PlayerHealth playerHealth;
    private SubLevelPlayerController playerController;
    private float scorePopTimer = 0f;
    private float damageOverlayTimer = 0f;
    private int lastScore = 0;
    private CableGroup[] cableGroups;

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        playerController = FindAnyObjectByType<SubLevelPlayerController>();

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.GetCurrentLives(), playerHealth.GetMaxLives());
        }

        if (playerController != null)
        {
            playerController.OnScoreChanged += UpdateScore;
            UpdateScore(playerController.GetScore());
        }

        cableGroups = FindObjectsByType<CableGroup>(FindObjectsInactive.Exclude);

        if (damageOverlay != null)
            damageOverlay.enabled = false;

        if (powerIndicator != null)
            powerIndicator.enabled = false;
    }

    private void Update()
    {
        UpdateCableStates();
        UpdateComboDisplay();
        UpdatePowerIndicator();
        UpdateDamageOverlay();
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }

        if (healthText != null)
        {
            healthText.text = current + "/" + max;
        }

        if (damageOverlay != null)
        {
            damageOverlay.enabled = true;
            damageOverlayTimer = damageOverlayDuration;
        }
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score;
        }
        lastScore = score;
        scorePopTimer = scorePopDuration;
    }

    private void UpdateComboDisplay()
    {
        if (comboText == null || playerController == null) return;

        int combo = playerController.GetCombo();
        if (combo > 1)
        {
            comboText.text = "x" + combo + " COMBO";
            comboText.gameObject.SetActive(true);
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private void UpdateCableStates()
    {
        if (cableStateText == null || cableGroups == null) return;

        string states = "";
        for (int i = 0; i < cableGroups.Length; i++)
        {
            CableGroup.GroupState state = cableGroups[i].GetCurrentState();
            string color = state switch
            {
                CableGroup.GroupState.Idle => "<color=green>OFF</color>",
                CableGroup.GroupState.Warning => "<color=yellow>ADVERTENCIA</color>",
                CableGroup.GroupState.Active => "<color=red>Electrificado</color>",
                CableGroup.GroupState.Cooldown => "<color=gray>Cooldown</color>",
                _ => "???"
            };
            states += "Cable " + (i + 1) + ": " + color + "\n";
        }
        cableStateText.text = states;
    }

    private void UpdatePowerIndicator()
    {
        if (powerIndicator == null || playerController == null) return;
        powerIndicator.enabled = playerController.TienePoder();
    }

    private void UpdateDamageOverlay()
    {
        if (damageOverlay == null) return;

        if (damageOverlayTimer > 0f)
        {
            damageOverlayTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(damageOverlayTimer / damageOverlayDuration) * 0.3f;
            Color c = damageOverlay.color;
            c.a = alpha;
            damageOverlay.color = c;
            damageOverlay.enabled = true;
        }
        else
        {
            damageOverlay.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealth;

        if (playerController != null)
            playerController.OnScoreChanged -= UpdateScore;
    }
}
