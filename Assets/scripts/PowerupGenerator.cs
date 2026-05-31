using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowerupGenerator : MonoBehaviour
{
    [Header("Spawn Zone (Anchor Points)")]
    public Transform leftSpawnBound;
    public Transform rightSpawnBound;

    [Header("Powerup Settings")]
    public GameObject[] powerupPrefabs; 
    public float spawnInterval = 15f; // Spawns a powerup every 15 seconds
    public float fallSpeed = 3f;

    private float spawnTimer;

    // Track the active powerups currently falling on screen
    private List<FallingLetter> activePowerups = new List<FallingLetter>();

    void Update()
    {
        if (leftSpawnBound == null || rightSpawnBound == null) return;

        spawnTimer += Time.deltaTime;

        // 1. Spawn a powerup when the timer hits
        if (spawnTimer >= spawnInterval)
        {
            SpawnPowerup();
            spawnTimer = 0f;
        }

        // 2. Check if the player collected any powerups
        CheckActivePowerups();
    }

    private void SpawnPowerup()
    {
        if (powerupPrefabs.Length == 0) return;

        // Pick a random X position somewhere between the left and right bounds
        float randomX = Random.Range(leftSpawnBound.position.x, rightSpawnBound.position.x);
        Vector3 spawnPosition = new Vector3(randomX, leftSpawnBound.position.y, 0f);

        GameObject prefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];
        GameObject spawnedObj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        FallingLetter letterScript = spawnedObj.GetComponent<FallingLetter>();
        if (letterScript != null)
        {
            letterScript.SetFallSpeed(fallSpeed);
            
            // Assign a random letter to the powerup
            Key randomKey = (Key)Random.Range((int)Key.A, (int)Key.Z + 1);
            letterScript.SetupRandomLetter(randomKey);

            activePowerups.Add(letterScript);
        }
    }

    private void CheckActivePowerups()
    {
        // Loop backwards safely to check and remove items
        for (int i = activePowerups.Count - 1; i >= 0; i--)
        {
            FallingLetter powerupLetter = activePowerups[i];

            // If it fell to the bottom and was destroyed, remove it from tracking
            if (powerupLetter == null)
            {
                activePowerups.RemoveAt(i);
                continue;
            }

            // If it is in the zone AND its individual key is pressed!
            if (powerupLetter.inZone && powerupLetter.isPressed)
            {
                // Trigger the abstract powerup effect
                if (powerupLetter.TryGetComponent<Powerup>(out Powerup powerupComponent))
                {
                    powerupComponent.ApplyEffect();
                }

                // Destroy the object and remove it from the list
                Destroy(powerupLetter.gameObject);
                activePowerups.RemoveAt(i);
            }
        }
    }
}