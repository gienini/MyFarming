using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Crop : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int cropGridPosition;
    private int harvestActionCount = 0;
    [Tooltip("Este debe ser publicado desde el transform del gameObject hijo, mostrando el punto en que se genera el efecto")]
    [SerializeField] private Transform harvestActionEffectTransform = null;

    [Tooltip("Este debe ser publicado desde el gameObject hijo")]
    [SerializeField] private SpriteRenderer cropHarvestedSpriteRenderer = null;

    internal void ProcessToolAction(ItemDetails equippedItemDetails, bool isToolRight, bool isToolLeft, bool isToolDown, bool isToolUp)
    {
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

        if (gridPropertyDetails == null)
            return;

        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null)
            return;

        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.codigo);
        if (cropDetails == null)
            return;

        Animator animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            if (isToolRight || isToolUp)
            {
                //Lanzamos animacion
                animator.SetTrigger("usetoolright");
            } else if (isToolLeft || isToolDown)
            {
                animator.SetTrigger("usetoolleft");
            }
        }

        //Disparamos el efecto de particulas
        if (cropDetails.esCosechaEfecto)
        {
            EventHandler.CallHarvestActionEffectEvent(harvestActionEffectTransform.position, cropDetails.cosechaEfecto);
        }

        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.codigo);
        if (requiredHarvestActions == -1)
            return; //No se puede usar la herramienta para recoger este cultivo

        harvestActionCount += 1;

        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(isToolRight, isToolUp, cropDetails, gridPropertyDetails, animator);
    }

    private void HarvestCrop(bool isUsingToolRight, bool isUsingToolup, CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        if (animator != null && cropDetails.esCosechaAnimacion)
        {
            if (cropDetails.cosechaSprite != null)
            {
                if (cropHarvestedSpriteRenderer != null)
                {
                    cropHarvestedSpriteRenderer.sprite = cropDetails.cosechaSprite;
                }
            }

            if ( isUsingToolRight || isUsingToolup)
            {
                animator.SetTrigger("harvestright");
            }else
            {
                animator.SetTrigger("harvestleft");
            }
        }

        //Reseteamos la casilla
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        if (cropDetails.hideCultivoPreCosecha)
        {
            GetComponentInChildren<SpriteRenderer>().enabled = false;
        }

        if (cropDetails.disableCultivoPreCosecha)
        {
            //Deshabilitamos los colliders
            Collider2D[] collider2Ds = GetComponentsInChildren<Collider2D>();
            foreach(Collider2D collider2D in collider2Ds)
            {
                collider2D.enabled = false;
            }
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        if (cropDetails.esCosechaAnimacion && animator != null)
        {
            StartCoroutine(ProcessHarvestActionAfterAnimation(cropDetails, gridPropertyDetails, animator));
        }else
        {
            HarvestAction(cropDetails, gridPropertyDetails);
        }
    }

    private IEnumerator ProcessHarvestActionAfterAnimation(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails, Animator animator)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Harvested"))
        {
            yield return null;
        }
        HarvestAction(cropDetails, gridPropertyDetails);
    }

    private void HarvestAction(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        SpawnHarvestedItems(cropDetails);
        if (cropDetails.cosechaTransformCodigo > 0)
        {
            CreateHarvestedTransformCrop(cropDetails, gridPropertyDetails);
        }
        Destroy(gameObject);
    }

    private void CreateHarvestedTransformCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        //Cambiamos las propiedades del gridPropertyDetails para que represente el nuevo crop que se deberia crear en lugar del primero
        gridPropertyDetails.seedItemCode = cropDetails.cosechaTransformCodigo;
        gridPropertyDetails.growthDays = 0;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);
        //Esta instruccion muestra la cosecha en el grid
        GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);
    }

    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        for (int i = 0; i < cropDetails.cultivoProducidoCodigo.Length; i++)
        {
            int cropsToProduce;
            if (cropDetails.cultivoProducidoCantidadMinima[i] == cropDetails.cultivoProducidoCantidadMaxima[i]
                    || cropDetails.cultivoProducidoCantidadMinima[i] < cropDetails.cultivoProducidoCantidadMaxima[i])
            {
                ///Caso en que solo hay un numero de cultivos producidos
                cropsToProduce = cropDetails.cultivoProducidoCantidadMinima[i];
            }
            else
            {
                //Caso en que hay un rango de numero de cultivos producidos
                cropsToProduce = Random.Range(cropDetails.cultivoProducidoCantidadMinima[i], cropDetails.cultivoProducidoCantidadMaxima[i]);
            }

            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCultivoEnPlayer)
                {
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cultivoProducidoCodigo[i]);
                }else
                {
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), 0f);
                    SceneItemsManager.Instance.InstantiateSceneItems(cropDetails.cultivoProducidoCodigo[i], spawnPosition);
                }
            }
        }
    }
}
