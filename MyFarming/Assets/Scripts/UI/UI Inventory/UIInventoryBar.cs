using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryBar : MonoBehaviour
{
    [SerializeField] private Sprite blank16x16Sprite = null;
    [SerializeField] private UIInventorySlot[] inventorySlots = null;
    [HideInInspector]public GameObject inventoryTextBoxGameobject;
    public GameObject inventoryBarDraggedItem;

    private RectTransform rectTransform;

    private bool _esInventarioPosicionAbajo = true;

    public bool EsInventarioPosicionAbajo { get => _esInventarioPosicionAbajo; set => _esInventarioPosicionAbajo = value; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        CambiarPosicionDeInventario();
    }

    private void CambiarPosicionDeInventario()
    {
        Vector3 posicionPlayer = Player.Instance.GetPlayerViewPortPosition();

        if (posicionPlayer.y > 0.3f && EsInventarioPosicionAbajo == false)
        {
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            EsInventarioPosicionAbajo = true;
        }else if (posicionPlayer.y < 0.3f && EsInventarioPosicionAbajo == true)
        {
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            EsInventarioPosicionAbajo = false;
        }
    }

    internal void ClearCurrentlySelectedItem()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].ClearSelectedItem();
        }
    }

    internal void DestroyCurrentLyDraggedItem()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].draggedItem != null)
            {
                Destroy(inventorySlots[i].draggedItem);
            }
        }
    }

    /// <summary>
    /// Lugar indicado para subscribirse a eventos
    /// </summary>
    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryItem> inventoryItems)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            ClearInventorySlots();

            if (inventorySlots.Length > 0 && inventoryItems.Count > 0)
            {
                for (int i = 0; i < inventorySlots.Length && i < inventoryItems.Count; i++)
                {
                    int itemCode = inventoryItems[i].codigo;

                    ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                    if (itemDetails != null)
                    {
                        inventorySlots[i].itemDetails = itemDetails;
                        inventorySlots[i].cantidad = inventoryItems[i].cantidad;
                        inventorySlots[i].textMeshProUGUI.text = inventoryItems[i].cantidad.ToString();
                        inventorySlots[i].inventorySlotImage.sprite = itemDetails.sprite;
                        SetHighlightedInventorySlots(i);
                    }
                }
            }
        }
    }

    private void ClearInventorySlots()
    {
        if (inventorySlots.Length > 0) 
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i].inventorySlotImage.sprite = blank16x16Sprite;
                inventorySlots[i].textMeshProUGUI.text = "";
                inventorySlots[i].itemDetails = null;
                inventorySlots[i].cantidad = 0;
                SetHighlightedInventorySlots(i);
            }
        }
    }

    internal void ClearHighlightOnInventorySlots()
    {
        if (inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].isSelected)
                {
                    inventorySlots[i].isSelected = false;
                    inventorySlots[i].inventorySlotHighLight.color = new Color(0f, 0f, 0f, 0f);
                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
                }
            }
        }
    }

    internal void SetHighlightedInventorySlots()
    {
        if (inventorySlots.Length> 0)
        {
            for (int i = 0; i<inventorySlots.Length; i++)
            {
                SetHighlightedInventorySlots(i);
            }
        }
    }

    private void SetHighlightedInventorySlots(int itemPosition)
    {
        if (inventorySlots.Length> 0 && inventorySlots[itemPosition].itemDetails != null)
        {
            if (inventorySlots[itemPosition].isSelected)
            {
                inventorySlots[itemPosition].inventorySlotHighLight.color = new Color(1f, 1f, 1f, 1f);
                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, inventorySlots[itemPosition].itemDetails.codigo);
            }
        }
    }
}
