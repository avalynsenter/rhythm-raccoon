using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneUIRefs : MonoBehaviour
{
    public static SceneUIRefs Instance { get; private set; }

    [Header("Inspector References")]
    [SerializeField] private Image inspectorStaminaBarFill;
    [SerializeField] private TMP_Text inspectorScoreText;
    [SerializeField] private GameObject inspectorGameOverUI;
    // --- NEW: Inspector fields for Win/Loss screens ---
    [SerializeField] private GameObject inspectorWinUI;
    [SerializeField] private GameObject inspectorLoseUI;

    // --- Static properties for easy access ---
    public static Image staminaBarFill { get; private set; }
    public static TMP_Text scoreText { get; private set; }
    public static GameObject gameOverUI { get; private set; }
    // --- NEW: Static properties for Win/Loss screens ---
    public static GameObject winUI { get; private set; }
    public static GameObject loseUI { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // Map the inspector fields to the static properties
        staminaBarFill = inspectorStaminaBarFill;
        scoreText = inspectorScoreText;
        gameOverUI = inspectorGameOverUI;
        winUI = inspectorWinUI;
        loseUI = inspectorLoseUI;
    }
}