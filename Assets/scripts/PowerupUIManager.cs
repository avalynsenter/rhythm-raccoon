using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupUIManager : MonoBehaviour
{
    public static PowerupUIManager Instance { get; private set; }

    [Header("UI Setup")]
    public Transform container;        // Drag your Horizontal Layout Group here
    public GameObject iconPrefab;      // Drag your new Prefab here

    // This struct lets you assign Sprites to specific powerup names in the Inspector!
    [System.Serializable]
    public struct PowerupIconConfig
    {
        public string powerupName;
        public Sprite iconSprite;
    }
    public List<PowerupIconConfig> iconConfigs;

    // A private class to track the math for each active icon
    private class ActiveIcon
    {
        public GameObject uiObject;
        public Image fillImage;
        public float maxDuration;
        public float currentTimer;
    }

    private Dictionary<string, ActiveIcon> activeIcons = new Dictionary<string, ActiveIcon>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ActivateOrRefreshIcon(string pName, float duration)
    {
        if (activeIcons.ContainsKey(pName))
        {
            // REFILL: The player collected it again! Reset the clock to max.
            activeIcons[pName].maxDuration = duration;
            activeIcons[pName].currentTimer = duration;
        }
        else
        {
            // NEW: Instantiate a fresh icon in the container
            GameObject newObj = Instantiate(iconPrefab, container);
            
            // Find the child image named "Fill"
            Image fillImg = newObj.transform.Find("Fill").GetComponent<Image>();
            
            // Look through our configs to find the matching Sprite
            Sprite foundSprite = null;
            foreach (var config in iconConfigs)
            {
                if (config.powerupName == pName) foundSprite = config.iconSprite;
            }

            // Apply the sprite (You can apply it to a background image too if you want!)
            if (foundSprite != null && fillImg != null)
            {
                fillImg.sprite = foundSprite;
            }

            // Track it
            ActiveIcon newIcon = new ActiveIcon 
            { 
                uiObject = newObj, 
                fillImage = fillImg, 
                maxDuration = duration, 
                currentTimer = duration 
            };
            
            activeIcons.Add(pName, newIcon);
        }
    }

    private void Update()
    {
        // We use a list to track which ones need to be deleted to avoid breaking the dictionary loop
        List<string> expiredPowerups = new List<string>();

        foreach (var kvp in activeIcons)
        {
            ActiveIcon icon = kvp.Value;
            icon.currentTimer -= Time.deltaTime;
            
            // Calculate the percentage of time left (1.0 to 0.0) and apply the clock wipe!
            if (icon.fillImage != null)
            {
                icon.fillImage.fillAmount = icon.currentTimer / icon.maxDuration;
            }

            // If time runs out, mark it for destruction
            if (icon.currentTimer <= 0)
            {
                Destroy(icon.uiObject);
                expiredPowerups.Add(kvp.Key);
            }
        }

        // Clean up the dictionary
        foreach (string key in expiredPowerups)
        {
            activeIcons.Remove(key);
        }
    }
}