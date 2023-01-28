using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>, ISaveable
{

    public bool esDebugActivado = false;
    private Dictionary<int, ItemDetails> itemDetailsDictionary;
    /// <summary>
    /// Contiene la lista de todos los inventarios del juego (coincide con el enum inventoryLocation)
    ///     [0] el player
    ///     [1] los chest
    /// </summary>
    public List<InventoryItem>[] inventoryLists;
    /// <summary>
    /// Contiene la capacidad de los inventarios de jugador y chests. Coincide por indice con inventoryLists
    /// </summary>
    [HideInInspector] public int[] inventoryListsCapacityInArray;
    [SerializeField] private SO_ItemList itemList = null;

    private int[] selectedInventoryItem; //Indica el objeto activo. El indice indica el inventario y el valor es el codigo
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }
    public GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    private UIInventoryBar uIInventoryBar;

    //private void Start()
    //{
    //    //Se inicializa el diccionario
    //    CreateItemDetailsDictionary();
    //}

    protected override void Awake()
    {
        base.Awake();

        CreateInventoryLists();

        //Se inicializa el diccionario
        CreateItemDetailsDictionary();
        selectedInventoryItem = new int[(int)InventoryLocation.count];
        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }
    private void Start()
    {
        uIInventoryBar = FindObjectOfType<UIInventoryBar>();
    }
    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];
        for (int i  = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }
        inventoryListsCapacityInArray = new int[(int)InventoryLocation.count];
        inventoryListsCapacityInArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    /// <summary>
    /// Publica el itemDetailsDIctionary desde el ScriptableObject ItemList
    /// </summary>
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();
        foreach(ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.codigo, itemDetails);
        }
    }
    /// <summary>
    /// obtenemos la informacion de un item dado el codigo
    /// </summary>
    /// <param name="codigo">el codigo del item</param>
    /// <returns>la informacion del item o null</returns>
    public ItemDetails GetItemDetails(int codigo)
    {
        ItemDetails itemDetails;
        if (itemDetailsDictionary.TryGetValue(codigo, out itemDetails) )
        {
            return itemDetails;
        }else
        {
            return null;
        }
    }

    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObject)
    {
        AddItem(inventoryLocation, item);
        //Metodo heredado para eliminar gameObjects
        Destroy(gameObject);
    }

    internal void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count
                && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];

            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;

            //IMPORTANTE llamar evento de refresco de inventario
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }
    }

    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventarioSeleccionado = inventoryLists[(int)inventoryLocation];
        //Buscamos el item en el inventario para ver si ya existia
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventarioSeleccionado, itemCode, itemPosition);
        } else
        {
            AddItemAtPosition(inventarioSeleccionado, itemCode);
        }
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);
        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    private void AddItemAtPosition(List<InventoryItem> inventarioSeleccionado, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();
        inventoryItem.codigo = itemCode;
        inventoryItem.cantidad = 1;

        inventarioSeleccionado.Add(inventoryItem);
        DebugPrintInventoryList(inventarioSeleccionado);
    }

    internal void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryList); 
        }
    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();
        //Tomamos la cantidad de Item que quedaran despues de borrar
        int quantity = inventoryList[itemPosition].cantidad -1;
        if (quantity > 0)
        {
            //Quedan objetos en la pila
            inventoryItem.cantidad = quantity;
            inventoryItem.codigo = itemCode;
            inventoryList[itemPosition] = inventoryItem;
        }else
        {
            inventoryList.RemoveAt(itemPosition);
        }
    }

    private void AddItemAtPosition(List<InventoryItem> inventarioSeleccionado, int itemCode, int itemPosition)
    {
        
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.codigo = itemCode;
        inventoryItem.cantidad = inventarioSeleccionado[itemPosition].cantidad + 1;

        inventarioSeleccionado[itemPosition] = inventoryItem;
        DebugPrintInventoryList(inventarioSeleccionado);
    }

    private void DebugPrintInventoryList(List<InventoryItem> inventarioSeleccionado)
    {
        if (esDebugActivado == true)
        {
            foreach (InventoryItem inventoryItem in inventarioSeleccionado)
            {
                Debug.Log("** " + InventoryManager.Instance.GetItemDetails(inventoryItem.codigo).descripcion + " cantidad = " + inventoryItem.cantidad);
            }
            Debug.Log("*****");
        }
        
    }

    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventarioSeleccionado = inventoryLists[(int)inventoryLocation];
        for (int i = 0; i < inventarioSeleccionado.Count; i++)
        {
            if (inventarioSeleccionado[i].codigo == itemCode)
            {
                return i;
            }
        }
        return -1;
    }

    public string GetItemTypeDescription(ItemType itemType) { 
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Herramienta_Romper:
                itemTypeDescription = Settings.HerramientaRomper;
                break;
            case ItemType.Herramienta_Cortar:
                itemTypeDescription = Settings.HerramientaCortar;
                break;
            case ItemType.Herramienta_Labrar:
                itemTypeDescription = Settings.HerramientaLabrar;
                break;
            case ItemType.Herramienta_Recoger:
                itemTypeDescription = Settings.HerramientaRecoger;
                break;
            case ItemType.Herramienta_Segar:
                itemTypeDescription = Settings.HerramientaSegar;
                break;
            case ItemType.Herramienta_Regar:
                itemTypeDescription = Settings.HerramientaRegar;
                break;
            default:
                itemTypeDescription = itemType.ToString();
                break;

        }

        return itemTypeDescription;
    }

    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }


    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }


    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1)
        {
            return null;
        }else
        {
            return GetItemDetails(itemCode);
        }
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
        SceneSave sceneSave = new SceneSave();
        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();

        GameObjectSave.sceneData.Remove(Settings.PersistentScene);
        //Añadimos todos los inventarios al save de la persistent scene
        sceneSave.listInvItemArray = inventoryLists;
        //Añadimos la capacidad de todos los inventarios al save de la persistent scene
        sceneSave.intArrayDictionary.Add(Settings.InventoryListCapacityArray, inventoryListsCapacityInArray);
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;

    }

    public void IsaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.listInvItemArray != null)
                {
                    inventoryLists = sceneSave.listInvItemArray;
                    for (int i  = 0; i < (int)InventoryLocation.count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
                    }

                    Player.Instance.ClearCarriedItem();
                    uIInventoryBar.ClearHighlightOnInventorySlots();
                }

                if (sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue(Settings.InventoryListCapacityArray, out int[] inventoryCapacityArray))
                {
                    inventoryListsCapacityInArray = inventoryCapacityArray;
                }
            }
        }
    }

    public void IsaveableStoreScene(string sceneName)
    {
        //NADA
    }

    public void IsaveableRestoreScene(string sceneName)
    {
        //NADA
    }
}
