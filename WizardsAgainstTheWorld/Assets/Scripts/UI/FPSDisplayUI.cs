using System.Collections.Generic;
using UnityEngine;
using TMPro; // Add TextMeshPro namespace

public class FPSDisplayUI : MonoBehaviour
{
    public TMP_Text displayText; // Reference to TextMeshPro component

    private List<float> frameTimes = new List<float>(); // Stores frame times for 1 second
    private float timer = 0f; // Tracks time elapsed

    private float minFps = 0f; // Add minFps variable
    private float averageFps = 0f; // Add minFps variable

    void Update()
    {
        // Get current FPS
        float currentFps = 1f / Time.unscaledDeltaTime;

        // Add the delta time for this frame to the list
        frameTimes.Add(Time.unscaledDeltaTime);

        // Update the timer
        timer += Time.unscaledDeltaTime;

        if (timer >= 1f) // Every 1 second
        {
            // Calculate average FPS
            float totalFrameTime = 0f;
            foreach (float frameTime in frameTimes)
            {
                totalFrameTime += frameTime;
            }

            averageFps = frameTimes.Count / totalFrameTime;

            // Calculate minimum FPS
            float maxFrameTime = Mathf.Max(frameTimes.ToArray());
            minFps = 1f / maxFrameTime;

            // Reset timer and clear frame times
            timer = 0f;
            frameTimes.Clear();
        }

        // Update the display text
        displayText.text = $"Average FPS: {averageFps:F2}\nMin FPS: {minFps:F2}\nCurrent FPS: {currentFps:F2}";
    }
}