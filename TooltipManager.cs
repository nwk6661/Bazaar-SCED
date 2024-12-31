using UnityEngine;
using UnityEngine.UI;
using System.IO;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ShowCombatEncounterDetail;

public class TooltipManager
{
    private GameObject _tooltipCanvas;
    private Image _tooltipImage;
    
    public string ToolTipCardName { get; set; } = "";
    public bool IsPveEncounter { get; set; } = false;

    private readonly ConfigEntry<float> _configRelativePosX;
    private readonly ConfigEntry<float> _configRelativePosY;
    private readonly ManualLogSource _logger;

    public TooltipManager(ConfigEntry<float> configRelativePosX, ConfigEntry<float> configRelativePosY, ManualLogSource logger)
    {
        _configRelativePosX = configRelativePosX;
        _configRelativePosY = configRelativePosY;
        _logger = logger;
    }

    public void CleanDestroy()
    {
        if (!_tooltipCanvas) return;
        Object.Destroy(_tooltipCanvas);
        _tooltipCanvas = null;
        _tooltipImage = null;
    }

    public void CreateImageDisplayFromCardName()
    {
        if (!IsPveEncounter || string.IsNullOrEmpty(ToolTipCardName) || TheBazaar.Data.IsInCombat)
        {
            return; 
        }

        _logger.LogDebug($"Creating Image Display for {ToolTipCardName}");
        string imagePath = $"{BepInEx.Paths.PluginPath}/ShowCombatEncounterDetail/Assets/{ToolTipCardName}.png";
        
        if (!File.Exists(imagePath))
        {
            return; 
        }

        CleanDestroy();
        CreateTooltipCanvas(imagePath);
    }

    private void CreateTooltipCanvas(string imagePath)
    {
        _tooltipCanvas = new GameObject("TooltipCanvas");
        var canvas = _tooltipCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = _tooltipCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(Screen.width, Screen.height);

        var imageObj = new GameObject("TooltipImage");
        imageObj.transform.SetParent(_tooltipCanvas.transform, false);
        _tooltipImage = imageObj.AddComponent<Image>();

        var texture = LoadTextureFromFile(imagePath);
        if (!texture)
        {
            _logger.LogError("Failed to load image texture.");
            CleanDestroy();
            return;
        }

        _tooltipImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        SetImageSize(texture.width, texture.height);
    }

    private void SetImageSize(float textureWidth, float textureHeight)
    {
        var screenHeight = Screen.height;
        var screenWidth = Screen.width;
        
        var imageAspectRatio = textureWidth / textureHeight;
        var targetWidth = screenWidth * 0.5f;
        var targetHeight = targetWidth / imageAspectRatio;

        // Adjust size if height exceeds half the screen height
        if (targetHeight > screenHeight * 0.5f)
        {
            var scale = (screenHeight * 0.5f) / targetHeight;
            targetWidth *= scale;
            targetHeight *= scale;
        }

        var rectTransform = _tooltipImage.rectTransform;
        rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        rectTransform.anchoredPosition = new Vector2(
            screenWidth * _configRelativePosX.Value,
            screenHeight * _configRelativePosY.Value
        );
    }

    private static Texture2D LoadTextureFromFile(string filePath)
    {
        var fileData = File.ReadAllBytes(filePath);
        var texture = new Texture2D(2, 2);
        return texture.LoadImage(fileData) ? texture : null;
    }
} 