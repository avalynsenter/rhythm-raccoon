using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a UI Slider to visually represent the score difference and trigger the multiplayer game over condition.
/// </summary>
public class TugOfWarUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider tugOfWarSlider;

    [Header("Settings")]
    [SerializeField] private int maxScoreDifference = 25;

    private bool isGameOver = false;

    void Start()
    {
        if (tugOfWarSlider != null)
        {
            // Configure the slider's range based on the max score difference.
            tugOfWarSlider.minValue = -maxScoreDifference;
            tugOfWarSlider.maxValue = maxScoreDifference;
            tugOfWarSlider.value = 0;
        }
        else
        {
            Debug.LogError("TugOfWarSlider is not assigned in the Inspector!", this.gameObject);
        }
    }

    void Update()
    {
        // If the game is over or the manager doesn't exist, do nothing.
        if (isGameOver || MultiplayerMatchManager.Instance == null)
        {
            return;
        }

        // Calculate the current difference in scores.
        int myScore = MultiplayerMatchManager.Instance.GetMyScore();
        int opponentScore = MultiplayerMatchManager.Instance.GetOpponentScore();
        int scoreDifference = myScore - opponentScore;

        // Apply the difference to the slider's value.
        if (tugOfWarSlider != null)
        {
            tugOfWarSlider.value = scoreDifference;
        }

        // Check if one player has reached the max score difference.
        if (scoreDifference >= maxScoreDifference || scoreDifference <= -maxScoreDifference)
        {
            isGameOver = true;
            Debug.Log("Tug of War game over condition met!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGameMultiplayer();
            }
        }
    }
}