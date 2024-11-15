using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using HarmonyLib;
using System.Reflection;

namespace ShowCombatEncounterDetail;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class InfarctusPluginCombatEncounterInfo : BaseUnityPlugin
{
    public static string ToolTip_CardName = "";
    public static bool isPVEEncounter = false;
    public static GameObject canvasObjectBoard=null;
    public static GameObject imageObjectBoard=null;
    public static GameObject canvasObjectWeapon=null;
    public static GameObject imageObjectWeapon=null;
    internal static new ManualLogSource Logger;
    
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        
    }

    
    public static void initalization(){
        canvasObjectBoard = new GameObject("ImageCanvasBoard");
        canvasObjectWeapon = new GameObject("ImageCanvasWeapon");
        imageObjectBoard = new GameObject("DisplayedImageBoard");
        imageObjectWeapon = new GameObject("DisplayedImageWeapon");
    }
    public static void clean_destroy(){
        if(canvasObjectBoard!=null){
            Destroy(canvasObjectBoard);
            Destroy(canvasObjectWeapon);
            Destroy(imageObjectBoard);
            Destroy(imageObjectWeapon);
            canvasObjectBoard=null;
            imageObjectBoard=null;
            canvasObjectWeapon=null;
            imageObjectWeapon=null;
        }
    }
    

    public static void CreateImageDisplayFromCardName()
    {
        if(isPVEEncounter && ToolTip_CardName!=""){
            Logger.LogDebug("Creating Image Display for "+ToolTip_CardName);
            initalization();
            CreateImageDisplay(ToolTip_CardName +"/board",-0.25f,-0.25f,canvasObjectBoard,imageObjectBoard);
            CreateImageDisplay(ToolTip_CardName +"/items",0.25f,-0.25f,canvasObjectWeapon,imageObjectWeapon);
        }
    }



    private static void CreateImageDisplay(string filename,float relativeposX,float relativeposY,GameObject canvasObject,GameObject imageObject,string extension = ".png")
    {
        // Get the camera size to set the image size proportionally
        var (width, height) = GetMainCameraSize();
        if (width == 0 || height == 0)
        {
            Logger.LogWarning("Main Camera not found, unable to create image display.");
            return;
        }

        // Set up the canvas
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // Add a CanvasScaler to handle scaling
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2((float)width, (float)height);

        // Initialize the imageObject as a child of canvasObject
        imageObject.transform.SetParent(canvasObject.transform);

        Image imageComponent = imageObject.AddComponent<Image>();

        Texture2D image_texture = null;

        // Load the image as a Texture2D
        try{
            image_texture = LoadTextureFromFile("BepInEx/plugins/ShowCombatEncounterDetail/Assets/" + filename + extension);
        }catch{}

        if (image_texture != null)
        {
            // Convert Texture2D to a Sprite
            Rect rect = new Rect(0, 0, image_texture.width, image_texture.height);
            Sprite sprite = Sprite.Create(image_texture, rect, new Vector2(0.5f, 0.5f));
            imageComponent.sprite = sprite;

            // Calculate the aspect ratio of the original image
            float imageAspectRatio = (float)image_texture.width / image_texture.height;

            // Set the size of the Image while preserving aspect ratio
            float scaleFactor = 0.5f; // Adjust this scale factor to fit your needs
            float displayWidth = (float)width * scaleFactor;
            float displayHeight = displayWidth / imageAspectRatio; // Calculate height based on aspect ratio

            if(displayHeight> (float)height/2){
                float coeff_reduce = (float)height/2.0f/displayHeight;
                displayHeight*=coeff_reduce;
                displayWidth*=coeff_reduce;
            }

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);

            // Center the image on screen
            rectTransform.anchoredPosition = new Vector2((float)(width * relativeposX), (float)(height * relativeposY));
        }
        else
        {
            Logger.LogError("Failed to load image texture.");
        }

    }



    private static Texture2D LoadTextureFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2); // Create a small texture to be replaced
            if (texture.LoadImage(fileData)) // Load the image file data
            {
                return texture;
            }
        }
        return null;
    }

    private static (double,double) GetMainCameraSize()
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
                return (0,0);
            }
        }
        return (0,0);
    }
}
