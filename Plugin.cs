using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using HarmonyLib;
using System.Reflection;
using UnityEngine.UI;

namespace ShowCombatEncounterDetail;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class InfarctusPluginCombatEncounterInfo : BaseUnityPlugin
{
    public static string ToolTipCardName = "";
    public static bool IsPveEncounter = false;
    public static GameObject CanvasObjectBoard = null;
    public static GameObject ImageObjectBoard = null;
    public static GameObject CanvasObjectWeapon = null;
    public static GameObject ImageObjectWeapon = null;
    public static Canvas CanvasBoard;
    public static Canvas CanvasItems;
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }


    public static void Initialization()
    {
        clean_destroy();
        CanvasObjectBoard = new GameObject("ImageCanvasBoard");
        CanvasObjectWeapon = new GameObject("ImageCanvasWeapon");
        ImageObjectBoard = new GameObject("DisplayedImageBoard");
        ImageObjectWeapon = new GameObject("DisplayedImageWeapon");
    }

    public static void clean_destroy()
    {
        if (CanvasObjectBoard != null)
        {
            Destroy(CanvasObjectBoard);
            CanvasObjectBoard = null;
        }

        if (CanvasObjectWeapon != null)
        {
            Destroy(CanvasObjectWeapon);
            CanvasObjectWeapon = null;
        }

        if (ImageObjectBoard != null)
        {
            Destroy(ImageObjectBoard);
            ImageObjectBoard = null;
        }

        if (ImageObjectWeapon != null)
        {
            Destroy(ImageObjectWeapon);
            ImageObjectWeapon = null;
        }

        if (CanvasBoard != null)
        {
            Destroy(CanvasBoard);
            CanvasBoard = null;
        }

        if (CanvasItems != null)
        {
            Destroy(CanvasItems);
            CanvasItems = null;
        }
    }


    public static void CreateImageDisplayFromCardName()
    {
        if (!IsPveEncounter || ToolTipCardName == "") return;
        clean_destroy();
        Logger.LogDebug("Creating Image Display for " + ToolTipCardName);
        Initialization();
        CreateImageDisplay(ToolTipCardName + "/board", -0.25f, -0.25f, CanvasObjectBoard, ImageObjectBoard,
            CanvasBoard);
        CreateImageDisplay(ToolTipCardName + "/items", 0.25f, -0.25f, CanvasObjectWeapon, ImageObjectWeapon,
            CanvasItems);
    }


    private static void CreateImageDisplay(string filename, float relativeposX, float relativeposY,
        GameObject canvasObject, GameObject imageObject, Canvas canvas, string extension = ".png")
    {
        if (!File.Exists("BepInEx/plugins/ShowCombatEncounterDetail/Assets/" + filename + extension))
        {
            return;
        }

        // Get the camera size to set the image size proportionally
        var (width, height) = GetMainCameraSize();
        if (width == 0 || height == 0)
        {
            Logger.LogWarning("Main Camera not found, unable to create image display.");
            return;
        }

        // Set up the canvas
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // Add a CanvasScaler to handle scaling
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2((float) width, (float) height);

        // Initialize the imageObject as a child of canvasObject
        imageObject.transform.SetParent(canvasObject.transform);

        Image imageComponent = imageObject.AddComponent<Image>();

        Texture2D image_texture = null;

        // Load the image as a Texture2D

        image_texture =
            LoadTextureFromFile("BepInEx/plugins/ShowCombatEncounterDetail/Assets/" + filename + extension);

        if (image_texture != null)
        {
            // Convert Texture2D to a Sprite
            Rect rect = new Rect(0, 0, image_texture.width, image_texture.height);
            Sprite sprite = Sprite.Create(image_texture, rect, new Vector2(0.5f, 0.5f));
            imageComponent.sprite = sprite;

            // Calculate the aspect ratio of the original image
            float imageAspectRatio = (float) image_texture.width / image_texture.height;

            // Set the size of the Image while preserving aspect ratio
            float scaleFactor = 0.5f; // Adjust this scale factor to fit your needs
            float displayWidth = (float) width * scaleFactor;
            float displayHeight = displayWidth / imageAspectRatio; // Calculate height based on aspect ratio

            if (displayHeight > (float) height / 2)
            {
                float coeff_reduce = (float) height / 2.0f / displayHeight;
                displayHeight *= coeff_reduce;
                displayWidth *= coeff_reduce;
            }

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);

            // Center the image on screen
            rectTransform.anchoredPosition =
                new Vector2((float) (width * relativeposX), (float) (height * relativeposY));
        }
        else
        {
            Logger.LogError("Failed to load image texture.");
        }
    }


    private static Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Create a small texture to be replaced
        if (texture.LoadImage(fileData)) // Load the image file data
        {
            return texture;
        }

        return null;
    }

    private static (double, double) GetMainCameraSize()
    {
        // Attempt to find the camera at the specified path
        GameObject cameraObject = GameObject.Find("DefaultSceneCameras/MainCamera");
        if (cameraObject != null)
        {
            Camera mainCamera = cameraObject.GetComponent<Camera>();
            if (mainCamera != null)
            {
                // Log the scaledPixelWidth and scaledPixelHeight
                int width = mainCamera.scaledPixelWidth;
                int height = mainCamera.scaledPixelHeight;
                return (width, height);
            }
            else
            {
                Logger.LogWarning("Camera component not found on MainCamera object.");
                return (0, 0);
            }
        }

        return (0, 0);
    }
}