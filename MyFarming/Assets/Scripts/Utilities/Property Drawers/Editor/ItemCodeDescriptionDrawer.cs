using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
public class ItemCodeDescriptionDrawer : PropertyDrawer
{
    /// <summary>
    /// Se redibuja la propiedad itemCode para añadir una linea mas que incluya el itemDescription
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //Tiene que devolver el doble del height para pintar tanto el codigo como el nombre en el editor
        return EditorGUI.GetPropertyHeight(property) * 2;
    }
    /// <summary>
    /// se dispara cuando se muestra en el editor
    /// </summary>
    /// <param name="position"></param>
    /// <param name="property"></param>
    /// <param name="label"></param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Se pone el codigo entre BeginProperty y EndProperty para asegurar que se sobreescribe en objetos prefab
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.BeginChangeCheck(); // Mira si los valores cambiaron

            //Dibujamos el itemCode (reescribimos el itemCode como estaba de serie)
            var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2), label, property.intValue);

            //Dibujamos el itemDescription debajo del codigo
            EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2), "Nombre del item", 
                GetItemDescription(property.intValue));


            //Si el codigo cambia se tiene que cambiar la descripcion
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = newValue;
            }
        }

        EditorGUI.EndProperty();
    }

    private string GetItemDescription(int itemCode)
    {
        //Se obtiene el itemList de manera dinamica
        SO_ItemList so_itemList = AssetDatabase.LoadAssetAtPath("Assets/Scriptable Objects Assets/Item/so_ItemList.asset",
            typeof(SO_ItemList)) as SO_ItemList;

        List<ItemDetails> itemDetailsList = so_itemList.itemDetails;
        //Find en un lambda
        ItemDetails itemDetail = itemDetailsList.Find(x => x.codigo == itemCode);

        if (itemDetail != null)
        {
            return itemDetail.descripcion;
        }else
        {
            return "";
        }
    }
}
