using System;
using UnityEngine;
[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription]
    public int semillaCodigo; //Codigo de la semilla correspondiente
    public int[] diasCrecimiento; //Dias de crecimiento por fase
    public GameObject[] crecimientoPrefab; //Prefab para cada fase de crecimiento
    public Sprite[] crecimientoSprite;
    public Season[] season; //Estaciones en las que puede plantarse
    public Sprite cosechaSprite; // Sprite de la cosecha
    [ItemCodeDescription]
    public int cosechaTransformCodigo; //Si al cosecharse se transforma en otro objeto se especifica aqui (ejemplo un arbol cuando se tala se convierte en tocon)
    public bool hideCultivoPreCosecha; // Si hay que deshabilitar el cultivo antes de la animacion de cosecha
    public bool disableCultivoPreCosecha; //Si los colliders del cultivo se tienen que deshabilitar
    public bool esCosechaAnimacion; //Si existe una animacion de cosecha para la ultima fase
    public bool esCosechaEfecto; //Si existe un efecto al cosechar
    public bool spawnCultivoEnPlayer; //Para añadirlo al inventario automaticamente
    public HarvestActionEffect cosechaEfecto; //El efecto del cultivo al ser cosechado

    [ItemCodeDescription]
    public int[] cosechaHerramientaCodigo; //Array de codigos de item capaces de cosechar este cultivo. Vacio para cualquiera
    public int[] cosechaHerramientaAcciones; //Array con el numero de acciones a realizar con cada herramienta del array cosechaHerramientaCodigo
    [ItemCodeDescription]
    public int[] cultivoProducidoCodigo; //Array de codigos de item producidos por este cultivo
    public int[] cultivoProducidoCantidadMinima; //Array con el minimo de items producidos por cosecha
    public int[] cultivoProducidoCantidadMaxima; //Array con el maximo. Si max>min se produce un numero random entre max y min
    public int diasRecrecimiento; //Dias que tarda en volver a ser cosechable. -1 si solo se cosecha una vez

    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if (RequiredHarvestActionsForTool(toolItemCode) == -1)
        {
            return false;
        }else
        {
            return true;
        }
    }

    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        for (int i = 0; i < cosechaHerramientaCodigo.Length; i++)
        {
            if (cosechaHerramientaCodigo[i] == toolItemCode)
            {
                return cosechaHerramientaAcciones[i];
            }
        }
        return -1;
    }
}


