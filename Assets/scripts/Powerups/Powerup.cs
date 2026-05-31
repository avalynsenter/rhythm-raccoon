using UnityEngine;

public abstract class Powerup : MonoBehaviour
{
    [Header("Powerup Settings")]
    [Tooltip("A brief description for debugging purposes.")]
    public string powerupName = "Unknown Powerup";

    // This is the abstract method that every specific powerup MUST implement.
    // It will be called by the ScoreManager when the player successfully types the letter.
    public abstract void ApplyEffect();
}