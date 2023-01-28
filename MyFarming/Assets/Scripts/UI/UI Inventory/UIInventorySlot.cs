using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
/// <summary>
/// IBeginDragHandler, IDragHandler, IEndDragHandler son eventos del sistema para arrastrar el raton
/// </summary>
public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Camera mainCamera;
    private Canvas parentCanvas;
    private Transform parentItem;
    private GridCursor gridCursor;
    public GameObject draggedItem;
    private Cursor cursor;

    public Image inventorySlotHighLight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [SerializeField]public UIInventoryBar inventoryBar = null;
    [SerializeField] public GameObject itemPrefab = null;
    [SerializeField] public GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int cantidad;
    [SerializeField] public int slotNumber = 0;


    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();

    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.DropSelectedItemEvent -= DragSelectedItemAtMousePosition;
        EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventoryEvent;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.DropSelectedItemEvent += DragSelectedItemAtMousePosition;
        EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventoryEvent;
    }

    private void RemoveSelectedItemFromInventoryEvent()
    {
        if (itemDetails != null && isSelected)
        {
            int itemCode = itemDetails.codigo;

            InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode);
            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, itemCode) == -1)
            {
                ClearSelectedItem();
            }
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
    }

    private void ClearCursors()
    {
        gridCursor.DisableCursor();
        gridCursor.SelectedItemType = ItemType.none;
        cursor.DisableCursor();
        cursor.SelectedItemType = ItemType.none;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            Player.Instance.DisablePlayerInputAndResetMovement();
            //Creamos un nuevo objeto a partir del objeto presente en el
            //  "slot donde empezamos el arrastre" = inventoryBar.inventoryBarDraggedItem
            //  Este objeto solo sirve para mostrar el drag
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            //Buscamos el componente Image del gameObject
            Image draggedItemImage = draggedItem.gameObject.GetComponentInChildren<Image>();
            draggedItemImage.sprite = itemDetails.sprite;
            //Seteamos el tamaño de la imagen en pixels
            draggedItemImage.rectTransform.sizeDelta = new Vector2(16f, 16f);
            
            SetSelectedItem();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            //El GameObject que sirve para mostrar el drag sigue el raton mientras se mantiene pulsado
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            //Eliminamos le gameObject que muestra el drag
            Destroy(draggedItem);

            //Caso en que el drag acaba dentro del inventario
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                DestroyInventoryTextBox();
                ClearSelectedItem();

            }
            //En otro caso se intenta soltar el item si esSoltable
            else
            {
                if (itemDetails.esSoltable)
                {
                    DragSelectedItemAtMousePosition();
                }
            }
        }
        Player.Instance.EnablePlayerInput();

        ClearCursors();
    }
    /// <summary>
    /// Suelta el item en la posicion actual del raton
    /// </summary>
    private void DragSelectedItemAtMousePosition()
    {
        if (itemDetails != null && isSelected)
        {

            //Hay que mirar si se puede realizar la accion
            //=====VERSION ANTIGUA=====
            //Vector3Int gridPosition = GridPropertiesManager.Instance.grid.WorldToCell(worldPosition);
            //GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(gridPosition.x, gridPosition.y);
            //if (gridPropertyDetails != null && gridPropertyDetails.esSoltarItem)


            // =====VERSION NUEVA=====
            // ahora la comprovacion de validez la hace el cursor
            if (gridCursor.CursorPositionIsValid)
            {
                //Calculamos el lugar donde se suelta el item a partir de la posicion del raton
                // La coordenada Z debe ser el valor en negativo de la Z de la camara

                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
                //Creamos el item a partir del prefab asignado al script (En este caso Item)
                worldPosition.y = worldPosition.y - Settings.gridCellSize/2f;
                GameObject nuevoObjeto = Instantiate(itemPrefab, worldPosition, Quaternion.identity, parentItem);

                Item item = nuevoObjeto.gameObject.GetComponent<Item>();
                item.ItemCode = itemDetails.codigo;

                //Eliminamos el item del inventario del jugador
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);
                //Se trata del ultimo elemento de la pila
                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(cantidad != 0)
        {
            inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.tipo);

            inventoryTextBox.SetTextboxText(itemDetails.descripcion, itemTypeDescription, "", itemDetails.descripcionLarga, "", "");

            if (inventoryBar.EsInventarioPosicionAbajo)
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBox();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isSelected)
            {
                ClearSelectedItem();
            }
            else
            {
                SetSelectedItem();
            }
        }
    }

    public void ClearSelectedItem()
    {
        ClearCursors();

        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);

        Player.Instance.ClearCarriedItem();
    }


    private void SetSelectedItem()
    {
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = true;

        inventoryBar.SetHighlightedInventorySlots();

        //Se establece el radio de uso del item a la instancia del cursor
        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;
        cursor.ItemUseRadius = itemDetails.itemUseRadius;

        if ( itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }else
        {
            gridCursor.DisableCursor();
        }

        if (itemDetails.itemUseRadius > 0f)
        {
            cursor.EnableCursor();
        }
        else
        {
            cursor.DisableCursor();
        }

        gridCursor.SelectedItemType = itemDetails.tipo;
        cursor.SelectedItemType = itemDetails.tipo;
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.codigo);

        if (itemDetails.esSostenible == true)
        {
            Player.Instance.ShowCarriedItem(itemDetails.codigo);
        }
        else
        {
            Player.Instance.ClearCarriedItem();
        }
    }

    private void DestroyInventoryTextBox()
    {
        if (inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    public void SceneLoaded()
    {
        //Buscamos el UIInventory cuando la escena ya se ha cargado, no antes
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemParentTransform).transform;
    }
}
