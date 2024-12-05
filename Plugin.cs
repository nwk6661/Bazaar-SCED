using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ShowCombatEncounterDetail;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class ShowCombatEncounterDetail : BaseUnityPlugin
{
    private static Harmony _harmony;
    private static ConfigEntry<float> _configRelativePosX;
    private static ConfigEntry<float> _configRelativePosY;
    
    public static string ToolTipCardName = "";
    public static bool IsPveEncounter = false;
    private static GameObject _canvasObjectBoard;
    private static GameObject _imageObjectBoard;
    private static GameObject _canvasObjectWeapon;
    private static GameObject _imageObjectWeapon;
    private static Canvas _canvasBoard;
    private static Canvas _canvasItems;
    private static ManualLogSource _logger;

    private void Awake()
    {
        // Plugin startup logic
        _logger = base.Logger;
        _logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        _configRelativePosX = Config.Bind(
            "General",
            "relativePosX",
            0.0f,
            new ConfigDescription(
                "Relative X position of the encounter card as a ratio between [-1, 1]",
                 new AcceptableValueRange<float>(-1f, 1f)
                )
            );


        _configRelativePosY = Config.Bind(
            "General",
            "relativePosY",
            -.125f,
            new ConfigDescription(
                "Relative Y position of the encounter card as a ratio between [-1, 1]",
                new AcceptableValueRange<float>(-1f, 1f)
            )
        );
        
        _harmony = Harmony.CreateAndPatchAll(typeof(Patches.Patches));
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        CleanDestroy();
    }


    private static void Initialization()
    {
        CleanDestroy();
        _canvasObjectBoard = new GameObject("ImageCanvasBoard");
        _canvasObjectWeapon = new GameObject("ImageCanvasWeapon");
        _imageObjectBoard = new GameObject("DisplayedImageBoard");
        _imageObjectWeapon = new GameObject("DisplayedImageWeapon");
    }

    public static void CleanDestroy()
    {
        if (_canvasObjectBoard != null)
        {
            Destroy(_canvasObjectBoard);
            _canvasObjectBoard = null;
        }

        if (_canvasObjectWeapon != null)
        {
            Destroy(_canvasObjectWeapon);
            _canvasObjectWeapon = null;
        }

        if (_imageObjectBoard != null)
        {
            Destroy(_imageObjectBoard);
            _imageObjectBoard = null;
        }

        if (_imageObjectWeapon != null)
        {
            Destroy(_imageObjectWeapon);
            _imageObjectWeapon = null;
        }

        if (_canvasBoard != null)
        {
            Destroy(_canvasBoard);
            _canvasBoard = null;
        }

        if (_canvasItems == null) return;
        
        Destroy(_canvasItems);
        _canvasItems = null;
    }


    public static void CreateImageDisplayFromCardName()
    {
        if (!IsPveEncounter || ToolTipCardName == "") return;
        _logger.LogDebug("Creating Image Display for " + ToolTipCardName);
        Initialization();
        CreateImageDisplay($"BepInEx/plugins/ShowCombatEncounterDetail/Assets/{ToolTipCardName}.png",
            _configRelativePosX.Value, _configRelativePosY.Value, _canvasObjectBoard, _imageObjectBoard,
            _canvasBoard);
    }


    private static void CreateImageDisplay(string detailsImagePath, float relativePosX, float relativePosY,
        GameObject canvasObject, GameObject imageObject, Canvas canvas)
    {
        if (!File.Exists(detailsImagePath)) return;

        // Get the camera size to set the image size proportionally
        var (width, height) = GetMainCameraSize();
        if (width == 0 || height == 0)
        {
            _logger.LogWarning("Main Camera not found, unable to create image display.");
            return;
        }

        // Set up the canvas
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // Add a CanvasScaler to handle scaling
        var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2((float) width, (float) height);

        // Initialize the imageObject as a child of canvasObject
        imageObject.transform.SetParent(canvasObject.transform);

        var imageComponent = imageObject.AddComponent<Image>();

        Texture2D imageTexture = null;

        // Load the image as a Texture2D

        imageTexture =
            LoadTextureFromFile(detailsImagePath);

        if (imageTexture != null)
        {
            // Convert Texture2D to a Sprite
            var rect = new Rect(0, 0, imageTexture.width, imageTexture.height);
            var sprite = Sprite.Create(imageTexture, rect, new Vector2(0.5f, 0.5f));
            imageComponent.sprite = sprite;

            // Calculate the aspect ratio of the original image
            var imageAspectRatio = (float) imageTexture.width / imageTexture.height;

            // Set the size of the Image while preserving aspect ratio
            const float scaleFactor = 0.5f; // Adjust this scale factor to fit your needs
            var displayWidth = (float) width * scaleFactor;
            var displayHeight = displayWidth / imageAspectRatio; // Calculate height based on aspect ratio

            if (displayHeight > (float) height / 2)
            {
                var coeffReduce = (float) height / 2.0f / displayHeight;
                displayHeight *= coeffReduce;
                displayWidth *= coeffReduce;
            }

            var rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);

            // Center the image on screen
            rectTransform.anchoredPosition =
                new Vector2((float) (width * relativePosX), (float) (height * relativePosY));
        }
        else
        {
            _logger.LogError("Failed to load image texture.");
        }
    }


    private static Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var fileData = File.ReadAllBytes(filePath);
        var texture = new Texture2D(2, 2); // Create a small texture to be replaced
        return texture.LoadImage(fileData) ? // Load the image file data
            texture : null;
    }

    private static (double, double) GetMainCameraSize()
    {
        // Attempt to find the camera at the specified path
        var cameraObject = GameObject.Find("DefaultSceneCameras/MainCamera");
        if (cameraObject == null) return (0, 0);
        var mainCamera = cameraObject.GetComponent<Camera>();
        if (mainCamera != null)
        {
            // Log the scaledPixelWidth and scaledPixelHeight
            var width = mainCamera.scaledPixelWidth;
            var height = mainCamera.scaledPixelHeight;
            return (width, height);
        }

        _logger.LogWarning("Camera component not found on MainCamera object.");
        return (0, 0);

    }
}