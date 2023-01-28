using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    //EJEMPLO slider en el editor [Range(10000, 10100)]
    //Este decorador es personalizado, hace que salga la descripcion del item cuando se le selecciona el itemcode
    // en el IDE
    [ItemCodeDescription]
    private int _itemCode;
    private SpriteRenderer spriteRenderer;
    public int ItemCode { get { return _itemCode; } set { _itemCode = value; } }


    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (_itemCode != 0)
        {
            Init(_itemCode);
        }
    }

    public void Init (int itemCodeParam)
    {
        if (itemCodeParam != 0)
        {
            ItemCode = itemCodeParam;

            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);
            spriteRenderer.sprite = itemDetails.sprite;

            // Cuando este item es "Escenario_Segable" le añadimos el componente de nudge para
            // que se menee cuando lo toquemos con el player
            if (itemDetails.tipo == ItemType.Escenario_Segable)
            {
                //gameObject es heredado de Component, se refiere a este objeto
                gameObject.AddComponent<ItemNudge>();
            }
        }

    }
     
}
