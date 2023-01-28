using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;

    private bool _cursorPositionIsValid = false;
    private int _itemUseGridRadius = 0;
    private ItemType _selectedItemType;
    private bool _cursorIsEnabled = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }
    public int ItemUseGridRadius { get => _itemUseGridRadius; set => _itemUseGridRadius = value; }
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }
    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            Vector3Int cursorGridPosition = GetGridPositionForCursor();

            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            SetCursorValidity(cursorGridPosition, playerGridPosition);

            //Seteamos el valor pero no lo usamos aun

            cursorRectTransform.position = GetRectTransformPositionForCursor(cursorGridPosition);

            return cursorGridPosition;
        }
        else
        {
            return Vector3Int.zero;
        }
    }
    /// <summary>
    /// Vuelve el cursor verde o rojo en funcion de las comprovaciones
    /// </summary>
    /// <param name="cursorGridPosition"></param>
    /// <param name="playerGridPosition"></param>
    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        SetCursorToValid();
        if ( Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius 
            || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInvalid();
            return;
        }

        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)
        {
            SetCursorToInvalid();
            return;
        }

        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if ( gridPropertyDetails != null)
        {
            switch (itemDetails.tipo)
            {
                case ItemType.Semilla:
                    if (!IsCursorValidForSeed(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Recurso:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.mueble:
                    if (!IsCursorValidForFurniture(gridPropertyDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Herramienta_Regar:
                case ItemType.Herramienta_Labrar:
                case ItemType.Herramienta_Cortar:
                case ItemType.Herramienta_Romper:
                case ItemType.Herramienta_Segar:
                case ItemType.Herramienta_Recoger:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemDetails))
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Escenario_Segable:
                    break;
                case ItemType.none:
                    break;
                case ItemType.count:
                    break;
                default:
                    break;
            }
        }
        else
        {
            SetCursorToInvalid();
            return;
        }
    }

    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        switch (itemDetails.tipo)
        {
            case ItemType.Herramienta_Labrar:
                if (gridPropertyDetails.esExcavable && gridPropertyDetails.daysSinceDug == -1)
                {
                    #region Tomamos todos los items en la casilla y miramos si son segables
                    Vector3 cursorWorldPosition = new Vector3(GridWorldPositionForCursor().x + 0.5f, GridWorldPositionForCursor().y + 0.5f, 0f);

                    List<Item> itemList = new List<Item>();
                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);
                    #endregion 

                    bool foundReapable = false;

                    foreach(Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).tipo == ItemType.Escenario_Segable)
                        {
                            foundReapable = true;
                            break;
                        }
                    }
                    if (foundReapable)
                    {
                        return false;
                    } else
                    {
                        return true;
                    }
                }else
                {
                    return false;
                }
            case ItemType.Recurso:
                break;
            case ItemType.Semilla:
                break;
            case ItemType.Herramienta_Regar:
                if (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1)
                {
                    return true;
                }else
                {
                    return false;
                }
            
            case ItemType.Herramienta_Segar:
                //Se gestiona desde la clase Cursor (sin grid)
                break;
            case ItemType.Herramienta_Cortar:
            case ItemType.Herramienta_Romper:
            case ItemType.Herramienta_Recoger:
                if ( gridPropertyDetails.seedItemCode != -1)
                {
                    CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);
                    if (cropDetails != null)
                    {
                        if (gridPropertyDetails.growthDays >= cropDetails.diasCrecimiento[cropDetails.diasCrecimiento.Length - 1])
                        {
                            if (cropDetails.CanUseToolToHarvestCrop(itemDetails.codigo))
                            {
                                return true;
                            } else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                break;
            case ItemType.Escenario_Segable:
                break;
            case ItemType.mueble:
                break;
            case ItemType.none:
                break;
            case ItemType.count:
                break;
        }
        return false;
    }

    private Vector3 GridWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }

    private bool IsCursorValidForFurniture(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.esSoltarItem;
    }

    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.esSoltarItem;
    }

    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.esSoltarItem;
    }

    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    private void SetCursorToInvalid()
    {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
    }



    /// <summary>
    /// Toma la posicion del cursor en la camera, lo transforma a WorldPosition y este lo devuelve como GridPosition (worldToCell) que es un Vector3Int
    /// </summary>
    /// <returns></returns>
    public Vector3Int GetGridPositionForCursor()
    {
        //La Z la calculamos asi:
        //Camara esta a z = -10, de manera que los items estan enfrente (+10)
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        return grid.WorldToCell(worldPosition);
    }
    /// <summary>
    /// Toma la posicion del player y lo retorna en forma de GridPosition
    /// </summary>
    /// <returns></returns>
    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }

    public void DisableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 0f);
        CursorIsEnabled = false;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        //Representa un punto en la escena completa
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        //Representa un punto dentro de la pantalla del jugador
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);

        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }
}
