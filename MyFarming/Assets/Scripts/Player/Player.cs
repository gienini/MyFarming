using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Player : SingletonMonobehaviour<Player>, ISaveable
{
    private WaitForSeconds pickAnimationPause;
    private WaitForSeconds afterPickAnimationPause;
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private bool playerUseToolDisabled = false;
    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;
    private Cursor cursor;
    // Movement parameters
    private float xInput;
    private float yInput;
    private bool isWalking;
    private bool isRunning;
    private bool isIdle;
#pragma warning disable IDE0044
#pragma warning disable 649// Agregar modificador de solo lectura
    private bool isCarrying;
#pragma warning restore IDE0044 // Agregar modificador de solo lectura
#pragma warning restore 649
    private ToolEffect toolEffect = ToolEffect.none;
    private bool isUsingToolRight;
    private bool isUsingToolLeft;
    private bool isUsingToolUp;
    private bool isUsingToolDown;
    private bool isLiftingToolRight;
    private bool isLiftingToolLeft;
    private bool isLiftingToolUp;
    private bool isLiftingToolDown;
    private bool isPickingRight;
    private bool isPickingLeft;
    private bool isPickingUp;
    private bool isPickingDown;
    private bool isSwingingToolRight;
    private bool isSwingingToolLeft;
    private bool isSwingingToolUp;
    private bool isSwingingToolDown;

    private Camera mainCamera;

    

    private Rigidbody2D rigidbody2D;
    private Vector3Int playerDirectionV3;
    private Direction playerDirection;

    private List<CharacterAttribute> characterAttributeCustomisationList;

    [Tooltip("Se debe publicar en el prefab con el spriterenderer del item equipado")]
    [SerializeField] private SpriteRenderer equipedItemSpriteRenderer = null;
    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;

    private float movementSpeed;

    private bool _playerInputIsDisabled = false;

    public bool PlayerInputIsDisabled { get => _playerInputIsDisabled; set => _playerInputIsDisabled = value; }
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    protected override void Awake()
    {
        base.Awake();

        rigidbody2D = GetComponent<Rigidbody2D>();
        animationOverrides = GetComponentInChildren<AnimationOverrides>();
        //Se inicializan las animaciones intercambiables del monigote
        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColor.none, PartVariantType.none);
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColor.none, PartVariantType.azada);


        characterAttributeCustomisationList = new List<CharacterAttribute>();
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();

        //Toma la camara principal. Es una operacion pesada
        mainCamera = Camera.main;
    }
    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }
    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }
    private void Update()
    {
        #region Player Input
        if (!PlayerInputIsDisabled)
        {
            ResetAnimationTriggers();

            PlayerMovementInput();

            playerWalkInput();

            PlayerClickInput();

            PlayerTestInput();


            EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
                isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown, isSwingingToolRight,
                isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
                false, false, false, false);
        }
        #endregion
    }

    private void PlayerClickInput()
    {
        if (!playerUseToolDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)
                {
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();
                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement();

        Vector3Int playerClickDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails != null)
        {
            switch (itemDetails.tipo)
            {
                case ItemType.Semilla:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSemilla(gridPropertyDetails, itemDetails);
                    }
                    break;
                case ItemType.Recurso:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputRecurso(itemDetails);
                    }
                    break;
                case ItemType.none:
                    break;
                case ItemType.count:
                    break;
                case ItemType.Herramienta_Regar:
                case ItemType.Herramienta_Labrar:
                case ItemType.Herramienta_Segar:
                case ItemType.Herramienta_Recoger:
                case ItemType.Herramienta_Cortar:
                case ItemType.Herramienta_Romper:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerClickDirection);
                    break;
                
                case ItemType.Escenario_Segable:
                    break;
                case ItemType.mueble:
                    break;
                default:
                    break;
            }
        }

    }

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        switch (itemDetails.tipo)
        {
            case ItemType.Herramienta_Regar:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;
            case ItemType.Herramienta_Labrar:
                if (gridCursor.CursorPositionIsValid)
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;
            case ItemType.Herramienta_Segar:
                if (cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, this.playerDirectionV3);
                }
                break;
            case ItemType.Herramienta_Recoger:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            case ItemType.Herramienta_Cortar:
                if (gridCursor.CursorPositionIsValid)
                {
                    ChopInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            case ItemType.Herramienta_Romper:
                if (gridCursor.CursorPositionIsValid)
                {
                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            default:
                break;
        }
    }

    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(BreakInPlayerDirectionCoroutine(gridPropertyDetails, itemDetails, playerDirection));
    }

    private void ChopInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ChopInPlayerDirectionCoroutine(gridPropertyDetails, itemDetails, playerDirection));
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(CollectInPlayerDirectionCoroutine(gridPropertyDetails, itemDetails, playerDirection));
    }

    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerClickDirection)
    {
        StartCoroutine(WaterGroundAtCursorRoutine(playerClickDirection, gridPropertyDetails));
    }
    private IEnumerator BreakInPlayerDirectionCoroutine(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerUseToolDisabled = true;

        toolCharacterAttribute.partVariantType = PartVariantType.mai;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);

        yield return useToolAnimationPause;

        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerUseToolDisabled = false;
    }

    private IEnumerator ChopInPlayerDirectionCoroutine(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerUseToolDisabled = true;

        toolCharacterAttribute.partVariantType = PartVariantType.hacha;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);

        yield return useToolAnimationPause;

        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerUseToolDisabled = false;
    }

    private IEnumerator CollectInPlayerDirectionCoroutine(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerUseToolDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
        yield return pickAnimationPause;
        yield return afterPickAnimationPause;

        PlayerInputIsDisabled = false;
        playerUseToolDisabled = false;
    }

    private void ProcessCropWithEquippedItemInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        switch (equippedItemDetails.tipo)
        {
            case ItemType.Herramienta_Recoger:
                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;
                }else if (playerDirection == Vector3Int.left)
                {
                    isPickingLeft = true;
                } else if (playerDirection == Vector3Int.up)
                {
                    isPickingUp = true;
                } else if (playerDirection == Vector3Int.down)
                {
                    isPickingDown = true;
                }
                break;
            case ItemType.Herramienta_Cortar:
            case ItemType.Herramienta_Romper:
                if (playerDirection == Vector3Int.right)
                {
                    isUsingToolRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isUsingToolLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isUsingToolUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isUsingToolDown = true;
                }
                break;
            default:
                break;
        }

        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        if (crop != null)
        {
            switch (equippedItemDetails.tipo)
            {
                case ItemType.Herramienta_Recoger:
                    crop.ProcessToolAction(equippedItemDetails, isPickingRight, isPickingLeft, isPickingDown, isPickingUp);
                    break;
                case ItemType.Herramienta_Romper:
                case ItemType.Herramienta_Cortar:
                    crop.ProcessToolAction(equippedItemDetails, isUsingToolRight, isUsingToolLeft, isUsingToolDown, isUsingToolUp);
                    break;
                default:
                    break;
            }

        }
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerUseToolDisabled = true;
        //Animacion
        toolCharacterAttribute.partVariantType = PartVariantType.guadaña;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return liftToolAnimationPause;

        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerUseToolDisabled = false;
    }

    private void UseToolInPlayerDirection(ItemDetails equipedItemDetails, Vector3Int playerDirection)
    {
        if (Input.GetMouseButton(0))
        {
            switch (equipedItemDetails.tipo)
            {
                case ItemType.Herramienta_Segar:
                    if (playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight = true;
                    } else if (playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft = true;
                    } else if (playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp = true;
                    }else
                    {
                        isSwingingToolDown = true;
                    }
                    break;
                default:
                    break;
            }
            //Definimos el punto central del cuadrado en el que buscaremos colliders. Cae a la mitad del itemUseRadius en la direccion a la que mire el player
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equipedItemDetails.itemUseRadius / 2f)), GetPlayerCentrePosition().y +
                playerDirection.y * (equipedItemDetails.itemUseRadius / 2f));

            //Definimos el tamaño del cuadrado
            Vector2 size = new Vector2(equipedItemDetails.itemUseRadius, equipedItemDetails.itemUseRadius);

            //Usamos el metodo para buscar colliders en un cuadrado
            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocation<Item>(Settings.limiteObjetosCheckSiega, point, size, 0f);

            int reapableItemCount = 0;

            for (int i = 0; i < itemArray.Length && reapableItemCount < Settings.limiteObjetosDestroySiega; i++)
            {
                if (itemArray[i] != null)
                {
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).tipo == ItemType.Escenario_Segable)
                    {
                        //Posicion del efecto visual
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, itemArray[i].transform.position.y + Settings.gridCellSize / 2f, itemArray[i].transform.position.z);

                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                        Destroy(itemArray[i].gameObject);
                        reapableItemCount++;
                    }
                }
            }
        }
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerUseToolDisabled = true;
        //Animacion
        toolCharacterAttribute.partVariantType = PartVariantType.regadora;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);
        //Usamos el efecto de herramienta, animacion que va a parte de la herramienta y el character
        toolEffect = ToolEffect.watering;

        if (playerDirection == Vector3Int.right)
        {
            isLiftingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isLiftingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isLiftingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isLiftingToolDown = true;
        }

        yield return liftToolAnimationPause;

        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }
        //Seteamos la info en el gridPropertyManager
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);
        //Mostramos la info del gridPropertyManager en la escena
        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerUseToolDisabled = false;
    }

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerUseToolDisabled = true;
        //Animacion
        toolCharacterAttribute.partVariantType = PartVariantType.azada;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        if (playerDirection == Vector3Int.right)
        {
            isUsingToolRight = true;
        }else if (playerDirection == Vector3Int.left)
        {
            isUsingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isUsingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isUsingToolDown = true;
        }
        //Esto pausa la ejecucion por el tiempo determinado en el valor de retorno, sirve para que se vea la animacion por pantalla
        yield return useToolAnimationPause;


        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }
        //Seteamos el tile con metadatos indicando que es una tile arada
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);
        //Llamamos al metodo que efectua el cambio visual en los tiles segun metadatos
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);
        //Esto pausa la ejecucion por el tiempo determinado en el valor de retorno
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerUseToolDisabled = false;



    }
    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        if (
            cursorPosition.x > playerPosition.x
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.right;
        }else if (
            cursorPosition.x < playerPosition.x
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.left;
        }else if (cursorPosition.y > playerPosition.y)
        {
            return Vector3Int.up;
        }else
        {
            return Vector3Int.down;
        }

    }
    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if ( cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }else if (cursorGridPosition.x < playerGridPosition.x)
        {
            return Vector3Int.left;
        }
        else if (cursorGridPosition.y > playerGridPosition.y)
        {
            return Vector3Int.up;
        }else
        {
            return Vector3Int.down;
        }
    }

    private void ProcessPlayerClickInputRecurso(ItemDetails itemDetails)
    {
        if (itemDetails.esSoltable && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }
    private void ProcessPlayerClickInputSemilla(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //Condiciones plantar semilla
        if (itemDetails.esSoltable && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug>-1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }
        //Condiciones soltar semilla
        else if (itemDetails.esSoltable && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.codigo) != null)
        //{
            gridPropertyDetails.seedItemCode = itemDetails.codigo;
            gridPropertyDetails.growthDays = 0;

            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            EventHandler.CallRemoveSelectedItemFromInventoryEvent();
        //}else
        //{
        //    Debug.Log("FALTA LA ENTRADA PARA LA SEMILLA " + itemDetails.codigo);
        //}

    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        // Se calcula el movimiento, deltaTime es el tiempo que tardara en ejecutarse el ciclo de update
        Vector2 move = new Vector2(xInput * movementSpeed * Time.deltaTime, yInput * movementSpeed * Time.deltaTime);
        // Se aplica el movimiento desde posicion actual, se le suma el recorrido calculado
        rigidbody2D.MovePosition(rigidbody2D.position + move);
    }

    private void ResetAnimationTriggers()
    {
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;
        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;
    }

    private void PlayerMovementInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (xInput != 0 && yInput != 0)
        {
            yInput = yInput * 0.71f;
            xInput = xInput * 0.71f;
        }

        if (xInput != 0 || yInput != 0)
        {
            isWalking = false;
            isRunning = true;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;
            // Captures player direction for save game;

            if (xInput < 0)
            {
                playerDirectionV3 = Vector3Int.left;
            }
            else if (xInput > 0)
            {
                playerDirectionV3 = Vector3Int.right;
            }
            else if (yInput < 0)
            {
                playerDirectionV3 = Vector3Int.down;
            }
            else if (yInput > 0)
            {
                playerDirectionV3 = Vector3Int.up;
            }
        }else if (xInput == 0 && yInput == 0)
        {
            isWalking = false;
            isRunning = false;
            isIdle = true;
        }

        
    }

    private void playerWalkInput()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isWalking = true;
            isRunning = false;
            isIdle = false;
            movementSpeed = Settings.walkingSpeed;
        }else
        {
            isWalking = false;
            isRunning = true;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;
        }
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }

    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();

        EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying, toolEffect,
                isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown, isSwingingToolRight,
                isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
                false, false, false, false);

    }

    private void ResetMovement()
    {
        xInput = 0f;
        yInput = 0f;
        isWalking = false;
        isRunning = false;
        isIdle = true;
    }

    public Vector3 GetPlayerViewPortPosition()
    {
        //transform es el elemento del componente que contiene info de su posicion y tamaño
        // Vector3 contiene la posicion del player. (0,0) es abajo izquierda, (1, 1) arriba derecha
        return mainCamera.WorldToViewportPoint(transform.position);
    }


    public void ClearCarriedItem()
    {
        equipedItemSpriteRenderer.sprite = null;
        equipedItemSpriteRenderer.color = new Color(0f, 0f, 0f, 0f);

        armsCharacterAttribute.partVariantType = PartVariantType.none;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        isCarrying = false;
    }

    public void ShowCarriedItem(int itemCode)
    {
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        if ( itemDetails != null)
        {
            equipedItemSpriteRenderer.sprite = itemDetails.sprite;
            equipedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            //Aplicamos los brazos en estado "carry"
            armsCharacterAttribute.partVariantType = PartVariantType.sostener;
            characterAttributeCustomisationList.Clear();
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

            isCarrying = true;
        }
    }

    private void PlayerTestInput()
    {
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }
        if (Input.GetKey(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            SceneControlerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }
        //TEST OBJECT POOL
        //if (Input.GetMouseButtonDown(1))
        //{
        //    GameObject tree = PoolManager.Instance.ReuseObect(canyonOakTreePrefab, mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
        //        -mainCamera.transform.position.z)), Quaternion.identity);
        //    tree.SetActive(true);
        //}
    }
    //Retorna el punto en el centro del monigote player usando el setting "playerCentreYOffset"
    public Vector3 GetPlayerCentrePosition()
    {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave IsaveableSave()
    {
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        SceneSave sceneSave = new SceneSave();
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();
        Vector3Serializable vector3Serializable = new Vector3Serializable(transform.position.x, transform.position.y, transform.position.z);
        sceneSave.vector3Dictionary.Add(Settings.PlayerPositionKey, vector3Serializable);
        sceneSave.stringDictionary.Add(Settings.CurrentSceneKey, SceneManager.GetActiveScene().name);
        sceneSave.stringDictionary.Add(Settings.PlayerDirectionKey, playerDirectionV3.ToString());

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void IsaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                //Cargamos la posicion del player
                if (sceneSave.vector3Dictionary != null && sceneSave.vector3Dictionary.TryGetValue(Settings.PlayerPositionKey, out Vector3Serializable playerPosition))
                {
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }
                if (sceneSave.stringDictionary != null)
                {
                    //Cargamos la escena
                    if (sceneSave.stringDictionary.TryGetValue(Settings.CurrentSceneKey, out string currentScene))
                    {
                        SceneControlerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }
                    //Cargamos la direccion del player
                    if (sceneSave.stringDictionary.TryGetValue(Settings.PlayerDirectionKey, out string playerDir))
                    {
                        bool playerDirFound = Enum.TryParse<Direction>(playerDir, true, out Direction direction);
                        if (playerDirFound)
                        {
                            playerDirection = direction;
                            SetPlayerDirection(playerDirection);
                        }
                    }
                }
            }
        }
    }

    public void IsaveableStoreScene(string sceneName)
    {
        //NO HACE NADA
    }

    public void IsaveableRestoreScene(string sceneName)
    {
        //NO HACE NADA
    }

    private void SetPlayerDirection(Direction playerDir)
    {
        switch (playerDir)
        {
            case Direction.up:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none,
                false, false, false, false,
                false, false, false, false,
                false, false, false, false,false,
                false, false, false,
                true, false, false, false);
                break;
            
            case Direction.left:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none,
                false, false, false, false,
                false, false, false, false,
                false, false, false, false, false,
                false, false, false,
                false, false, true, false);
                break;
            case Direction.right:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none,
                false, false, false, false,
                false, false, false, false,
                false, false, false, false, false,
                false, false, false,
                false, false, false, true);
                break;
            case Direction.down:
            default:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none,
                false, false, false, false,
                false, false, false, false,
                false, false, false, false, false,
                false, false, false,
                false, true, false, false);
                break;
        }
    }
}
