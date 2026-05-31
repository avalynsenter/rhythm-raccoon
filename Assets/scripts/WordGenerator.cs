using UnityEngine;
using UnityEngine.InputSystem; 

public class WordSpawner : MonoBehaviour
{
    [Header("Spawn Zone (Anchor Points)")]
    [Tooltip("Drag the LeftBound empty GameObject here.")]
    public Transform leftSpawnBound;
    [Tooltip("Drag the RightBound empty GameObject here.")]
    public Transform rightSpawnBound;

    [Header("Prefabs")]
    public GameObject[] spawnablePrefabs; 

    [Header("Difficulty: Timing & Speed")]
    public float initialSpawnDelay = 4f; // Increased to slow down initial spawning
    public float minimumSpawnDelay = 1.5f; 
    public float delayDecreaseRate = 0.02f; // Slowed down the difficulty ramp

    public float initialFallSpeed = 2f;
    public float maxFallSpeed = 7f;
    public float speedIncreaseRate = 0.05f;

    [Header("Difficulty: Amount")]
    public int minLettersPerWave = 1;
    public int maxLettersLimit = 5;
    public float timeToReachMaxLetters = 60f;

    private float currentSpawnDelay;
    private float currentFallSpeed;
    private float spawnTimer;
    private float gameTimer;

    void Start()
    {
        currentSpawnDelay = initialSpawnDelay;
        currentFallSpeed = initialFallSpeed;

        if (leftSpawnBound == null || rightSpawnBound == null)
        {
            Debug.LogError("WordSpawner is missing its Left or Right spawn bounds!");
        }
    }

    void Update()
    {
        if (leftSpawnBound == null || rightSpawnBound == null) return;

        gameTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        currentSpawnDelay = Mathf.Max(minimumSpawnDelay, initialSpawnDelay - (gameTimer * delayDecreaseRate));
        currentFallSpeed = Mathf.Min(maxFallSpeed, initialFallSpeed + (gameTimer * speedIncreaseRate));

        if (spawnTimer >= currentSpawnDelay)
        {
            SpawnWave();
            spawnTimer = 0f;
        }
    }

    private void SpawnWave()
    {
        float progress = Mathf.Clamp01(gameTimer / timeToReachMaxLetters);
        int lettersToSpawn = Mathf.RoundToInt(Mathf.Lerp(minLettersPerWave, maxLettersLimit, progress));

        // Use the exact X positions of your anchor points
        float leftEdge = leftSpawnBound.position.x;
        float rightEdge = rightSpawnBound.position.x;
        float availableWidth = rightEdge - leftEdge;
        float spacing = availableWidth / (lettersToSpawn + 1);
        
        // Use the exact Y position of your Left anchor point
        float spawnY = leftSpawnBound.position.y; 

        for (int i = 0; i < lettersToSpawn; i++)
        {
            GameObject prefabToSpawn = spawnablePrefabs[Random.Range(0, spawnablePrefabs.Length)];

            float spawnX = leftEdge + (spacing * (i + 1));
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

            GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

            FallingLetter letterScript = spawnedObj.GetComponent<FallingLetter>();
            if (letterScript != null)
            {
                letterScript.SetFallSpeed(currentFallSpeed);
                
                Key randomKey = (Key)Random.Range((int)Key.A, (int)Key.Z + 1);
                letterScript.SetupRandomLetter(randomKey);
            }
        }
    }
}