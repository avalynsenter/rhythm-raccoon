using UnityEngine;

public class DoubleScorePowerup : Powerup
{
    [Header("Double Score Settings")]
    public int scoreMultiplier = 2;
    public float durationInSeconds = 10f;

    public override void ApplyEffect()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ActivateMultiplier(scoreMultiplier, durationInSeconds);
            Debug.Log($"applied {powerupName}!");
        }
    }
}