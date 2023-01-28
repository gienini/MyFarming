using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{

    private Canvas canvas;
    private Camera mainCamera;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite transparentCursorSprite = null;
    [SerializeField] private GridCursor gridCursor = null;

    private bool _cursorIsEnabled = false;
    private bool _cursorPositionIsValid = false;
    private ItemType _selectedItemType;
    private float _itemUseRadius = 0f;

    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }
    public float ItemUseRadius { get => _itemUseRadius; set => _itemUseRadius = value; }


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

    private void DisplayCursor()
    {
        Vector3 cursorWorldPosition = GetWorldPositionForCursor();
        SetCursorValidity(cursorWorldPosition, Player.Instance.GetPlayerCentrePosition());
        cursorRectTransform.position = GetRectTransformPositionForCursor();
    }

    private void SetCursorValidity(Vector3 cursorPosition, Vector3 playerPosition)
    {
        SetCursorToValid();
        //Primera comprobacion, miramos los 4 cuadrantes que parten de las esquinas de el itemUseRadius
        //Tener en cuenta que quedan casos de cursor invalido por mirar
        if (
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)
            ||
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)
           )
        {
            SetCursorToInvalid();
            return;
        }

        //Segunda comprobacion, Miramos si el punto queda dentro del itemUseRadius
        if (
            Mathf.Abs(cursorPosition.x - playerPosition.x) > ItemUseRadius
            ||
            Mathf.Abs(cursorPosition.y - playerPosition.y) > ItemUseRadius
            )
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

        switch (itemDetails.tipo)
        {
            case ItemType.Herramienta_Regar:
            case ItemType.Herramienta_Labrar:
            case ItemType.Herramienta_Cortar:
            case ItemType.Herramienta_Romper:
            case ItemType.Herramienta_Segar:
            case ItemType.Herramienta_Recoger:
                if (!SetCurrentValidityTool(cursorPosition, playerPosition, itemDetails))
                {
                    SetCursorToInvalid();
                    return;
                }
                break;
            case ItemType.Escenario_Segable:
            case ItemType.mueble:
            case ItemType.none:
            case ItemType.count:
                break;
        }


    }

    private bool SetCurrentValidityTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        bool isValid = false;
        switch (itemDetails.tipo)
        {
            case ItemType.Herramienta_Segar:
                return SetCursorValidityHerramientaSegar(cursorPosition, playerPosition, itemDetails);
            default:
                break;
        }
        return isValid;
    }

    private bool SetCursorValidityHerramientaSegar(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        List<Item> itemList = new List<Item>();
        if (HelperMethods.GetComponentsAtCursorLocation<Item>(out itemList, cursorPosition))
        {
            if (itemList.Count > 0)
            {
                foreach(Item item in itemList)
                {
                    if (InventoryManager.Instance.GetItemDetails(item.ItemCode).tipo == ItemType.Escenario_Segable)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void SetCursorToInvalid()
    {
        cursorImage.sprite = transparentCursorSprite;
        CursorPositionIsValid = false;
        gridCursor.EnableCursor();
    }

    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
        gridCursor.DisableCursor();
    }

    public Vector3 GetWorldPositionForCursor()
    {
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
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

    public Vector2 GetRectTransformPositionForCursor()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        return RectTransformUtility.PixelAdjustPoint(screenPosition, cursorRectTransform, canvas);

    }
}
