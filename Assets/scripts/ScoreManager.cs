using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; } = 0;

    // --- NEW: Multiplier Tracking ---
    private int currentMultiplier = 1;
    private float multiplierTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        // --- NEW: Countdown the multiplier timer ---
        if (multiplierTimer > 0)
        {
            multiplierTimer -= Time.deltaTime;
            
            // When time runs out, reset the multiplier back to normal
            if (multiplierTimer <= 0)
            {
                currentMultiplier = 1;
                multiplierTimer = 0f;
                Debug.Log("Score multiplier has ended! Back to 1x.");
            }
        }
    }

    public void AddScore(int pointsToAdd)
    {
        // Multiply the incoming points by our current multiplier
        int calculatedPoints = pointsToAdd * currentMultiplier;
        Score += calculatedPoints;
        
        Debug.Log($"Word cleared! +{calculatedPoints} points (Multiplier: {currentMultiplier}x). Total Score: {Score}");
    }

    // --- NEW: Method for powerups to call ---
    public void ActivateMultiplier(int multiplier, float duration)
    {
        currentMultiplier = multiplier;
        
        // If the player collects two double-score powerups in a row, 
        // this simply resets the clock back to the full duration!
        multiplierTimer = duration; 
        
        Debug.Log($"Multiplier Activated! {currentMultiplier}x score for {duration} seconds!");
    }
}