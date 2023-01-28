using UnityEngine;

public static class Settings
{
    public static float pickAnimationPause = 1f;
    public static float afterPickAnimationPause = 0.2f;
    public static Vector2 guadañaEfectoSize = new Vector2(3f, 3f);
    public static int limiteObjetosCheckSiega = 15;
    public static int limiteObjetosDestroySiega = 2;
    public static Vector2 cursorSize = Vector2.one;
    // margen para conocer el centro del monigote PLAYER (1f = 16px)
    public const float playerCentreYOffset = 0.875f;
    public const float useToolAnimationPause = 0.25f;
    public const float afterUseToolAnimationPause = 0.4f;
    public const float liftToolAnimationPause = 0.2f;
    public const float afterLiftToolAnimationPause = 0.4f;

    public const int playerInitialInventoryCapacity = 24;
    public const int playerMaximumInventoryCapacity = 24;
    // Oscurecimiento de escenario
    public const float fadeInSeconds = 0.25f;
    public const float fadeOutSeconds = 0.35f;
    // Minimo de opacidad para un elemento oscurecido
    public const float targetAlpha = 0.45f;

    // Player movement
    public const float runningSpeed = 5.333f;
    public const float walkingSpeed = 2.555f;

    //Tilemap
    public const float gridCellSize = 1f;

    // Player animation parameters
    public static int xInput;
    public static int yInput;
    public static int isWalking;
    public static int isRunning;
    public static int toolEffect;
    public static int isUsingToolRight;
    public static int isUsingToolLeft;
    public static int isUsingToolUp;
    public static int isUsingToolDown;
    public static int isLiftingToolRight;
    public static int isLiftingToolLeft;
    public static int isLiftingToolUp;
    public static int isLiftingToolDown;
    public static int isSwingingToolRight;
    public static int isSwingingToolLeft;
    public static int isSwingingToolUp;
    public static int isSwingingToolDown;
    public static int isPickingRight;
    public static int isPickingLeft;
    public static int isPickingUp;
    public static int isPickingDown;

    // Shared Animation Parameters

    public static int idleUp;
    public static int idleDown;
    public static int idleLeft;
    public static int idleRight;

    //Time system
    public const float secondsPerGameSecond = 0.012f;


    //Tools
    public const string HerramientaLabrar = "Azada"; 
    public const string HerramientaCortar = "Hacha"; 
    public const string HerramientaRomper = "Mai"; 
    public const string HerramientaSegar = "Guadaña"; 
    public const string HerramientaRecoger = "Cesta";
    public const string HerramientaRegar = "Regadora";

    //SaveGame
    public const string RutaRelativaSaveGame = "/WildHopeCreek.dat";
    public const string PersistentScene = "PersistentScene";
    public const string PlayerPositionKey = "playerPosition";
    public const string PlayerDirectionKey = "playerDirection";
    public const string CurrentSceneKey = "currentScene";
    public const string InventoryListCapacityArray = "inventoryListCapacityArray";

    // Static constructor
    static Settings()
    {
        // Player
        xInput = Animator.StringToHash("xInput");
        yInput = Animator.StringToHash("yInput");
        isWalking = Animator.StringToHash("isWalking");
        isRunning = Animator.StringToHash("isRunning");
        toolEffect = Animator.StringToHash("toolEffect");
        isUsingToolRight = Animator.StringToHash("isUsingToolRight");
        isUsingToolLeft = Animator.StringToHash("isUsingToolLeft");
        isUsingToolUp = Animator.StringToHash("isUsingToolUp");
        isUsingToolDown = Animator.StringToHash("isUsingToolDown");
        isLiftingToolRight = Animator.StringToHash("isLiftingToolRight");
        isLiftingToolLeft = Animator.StringToHash("isLiftingToolLeft");
        isLiftingToolUp = Animator.StringToHash("isLiftingToolUp");
        isLiftingToolDown = Animator.StringToHash("isLiftingToolDown");
        isSwingingToolRight = Animator.StringToHash("isSwingingToolRight");
        isSwingingToolLeft = Animator.StringToHash("isSwingingToolLeft");
        isSwingingToolUp = Animator.StringToHash("isSwingingToolUp");
        isSwingingToolDown = Animator.StringToHash("isSwingingToolDown");
        isPickingRight = Animator.StringToHash("isPickingRight");
        isPickingLeft = Animator.StringToHash("isPickingLeft");
        isPickingUp = Animator.StringToHash("isPickingUp");
        isPickingDown = Animator.StringToHash("isPickingDown");
        // Shared
        idleUp = Animator.StringToHash("idleUp");
        idleDown = Animator.StringToHash("idleDown");
        idleLeft = Animator.StringToHash("idleLeft");
        idleRight = Animator.StringToHash("idleRight");
    }
}
