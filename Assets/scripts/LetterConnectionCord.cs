using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LetterConnectionCord : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<FallingLetter> connectedLetters;

    [Header("Rope Physics")]
    public int pointsPerSegment = 10; // How smooth the curve is (adds extra points between letters)
    public float sagAmount = 0.5f;    // How far the rope droops
    public float swayAmount = 0.2f;   // How strongly the wind blows the rope
    public float swaySpeed = 2f;      // How fast the wind blows
    
    private float noiseOffset;

    public void Setup(List<FallingLetter> letters)
    {
        lineRenderer = GetComponent<LineRenderer>();
        connectedLetters = letters;

        // Sort letters by X position so the line draws cleanly left-to-right
        connectedLetters.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Calculate total points needed: (Number of gaps * points per gap) + the final anchor point
        lineRenderer.positionCount = ((connectedLetters.Count - 1) * pointsPerSegment) + 1;
        
        // Give this specific rope a unique wind pattern
        noiseOffset = Random.Range(0f, 1000f); 
    }

    void Update()
    {
        if (connectedLetters == null || connectedLetters.Count < 2) return;

        // Check if ANY letter in the cluster was destroyed/popped
        for (int i = 0; i < connectedLetters.Count; i++)
        {
            if (connectedLetters[i] == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        int currentPointIndex = 0;

        // Loop through each segment (the gap between Letter A and Letter B)
        for (int i = 0; i < connectedLetters.Count - 1; i++)
        {
            Vector3 startPoint = connectedLetters[i].transform.position;
            Vector3 endPoint = connectedLetters[i + 1].transform.position;
            
            // Adjust sag dynamically so letters that are closer together sag slightly less
            float distance = Vector3.Distance(startPoint, endPoint);
            float dynamicSag = sagAmount * distance; 

            // Draw the sub-points for this segment
            for (int j = 0; j < pointsPerSegment; j++)
            {
                // 't' is a percentage from 0.0 to 0.99 for where we are on the line
                float t = j / (float)pointsPerSegment;

                // 1. Get the perfectly straight linear position
                Vector3 pointPosition = Vector3.Lerp(startPoint, endPoint, t);

                // 2. Add the Sag
                // Mathf.Sin(t * PI) creates an arc that is 0 at the ends and 1 in the exact middle
                float arcShape = Mathf.Sin(t * Mathf.PI);
                pointPosition.y -= arcShape * dynamicSag;

                // 3. Add organic wind sway
                // We use 't' in the noise calculation so the wind ripples across the cord
                float windRipple = Mathf.PerlinNoise(Time.time * swaySpeed, noiseOffset + (t * 5f));
                // Multiply by arcShape so the anchor points don't move, only the middle of the rope!
                float windOffset = (windRipple * 2f - 1f) * swayAmount * arcShape; 
                pointPosition.x += windOffset;

                // Push behind the letters
                pointPosition.z = 1f; 

                lineRenderer.SetPosition(currentPointIndex, pointPosition);
                currentPointIndex++;
            }
        }

        // Lock the very last point of the LineRenderer to the exact center of the final letter
        Vector3 finalPoint = connectedLetters[connectedLetters.Count - 1].transform.position;
        finalPoint.z = 1f;
        lineRenderer.SetPosition(currentPointIndex, finalPoint);
    }
}