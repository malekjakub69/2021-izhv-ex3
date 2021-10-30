using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Util;

// ReSharper disable InvalidXmlDocComment

/// <summary>
/// Behavior for the root health UI entity.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("UI Settings")]
    /// <summary>
    /// Color used for high health.
    /// </summary>
    public Color healthHigh = new Color { r = 0.0f, g = 0.7f, b = 0.0f, a = 1.0f };
    
    /// <summary>
    /// Percentage for medium health (0.0 - 1.0).
    /// </summary>
    public float healthMediumPtg = 0.5f;
    
    /// <summary>
    /// Color used for medium health.
    /// </summary>
    public Color healthMedium = new Color { r = 0.7f, g = 0.7f, b = 0.0f, a = 1.0f };
    
    /// <summary>
    /// Percentage for low health (0.0 - 1.0).
    /// </summary>
    public float healthLowPtg = 0.25f;
    
    /// <summary>
    /// Color used for low health.
    /// </summary>
    public Color healthLow = new Color { r = 0.7f, g = 0.0f, b = 0.0f, a = 1.0f };
    
    /// <summary>
    /// Canvas grouping component for this whole UI.
    /// </summary>
    public CanvasGroup mCanvasGroup;
    
    /// <summary>
    /// Health bar used to display the current health.
    /// </summary>
    public Image mHealthBar;
    
    /// <summary>
    /// Health text used to display the current health.
    /// </summary>
    public Text mHealthText;

    /// <summary>
    /// Called when the script instance is first loaded.
    /// </summary>
    private void Awake()
    {
        // Retrieve the UI elements.
        mCanvasGroup = GetComponent<CanvasGroup>();
        mHealthBar = Common.GetChildByName(gameObject, "HealthBar")?.GetComponent<Image>();
        mHealthText = Common.GetChildByName(gameObject, "HealthText")?.GetComponent<Text>();
        
        // Initialize and display the default value.
        DisplayHealth(0.0f, 0.0f, 0.0f);
        // Hide the UI.
        SetVisible(false);
    }
    
    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    { }
    
    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    { }
    
    /// <summary>
    /// Display the given health value.
    /// </summary>
    /// <param name="current">Current health level.</param>
    /// <param name="min">Minimal health level.</param>
    /// <param name="max">Maximal health level.</param>
    public void DisplayHealth(float current, float min, float max)
    {
        // Validate the inputs.
        current = Math.Clamp(current, min, max);
        
        // Determine the current health level.
        var s = Settings.Instance;
        var total = max - min;
        var currentPtg = Math.Clamp(current / total, 0.0f, 1.0f);

        // Fill the bar.
        mHealthBar.fillAmount = currentPtg;
        
        // Set the text.
        mHealthText.text = $"{current} / {max}";
        
        // Color the bar.
        if (currentPtg > healthMediumPtg)
        { mHealthBar.color = healthHigh; }
        else if (currentPtg > healthLowPtg)
        { mHealthBar.color = healthMedium; }
        else
        { mHealthBar.color = healthLow; }
    }

    /// <summary>
    /// Set this UI to visible (true) or invisible (false).
    /// </summary>
    /// <param name="visible">Visibility.</param>
    public void SetVisible(bool visible)
    {
        if (visible)
        { mCanvasGroup.alpha = 1.0f; mCanvasGroup.blocksRaycasts = true; }
        else
        { mCanvasGroup.alpha = 0.0f; mCanvasGroup.blocksRaycasts = false; }
    }
}
