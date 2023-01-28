using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public bool esDebugActivado = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();

        if (item != null)
        {
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);
            if (esDebugActivado == true)
            {
                Debug.Log(itemDetails.descripcion);
            }

            if (itemDetails.esRecogible == true)
            {
                InventoryManager.Instance.AddItem(InventoryLocation.player, item, collision.gameObject);
            }
        }
    }
}
