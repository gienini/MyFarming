using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Linq;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISaveable
{
    private Transform cropParentTransform;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;
    private Grid grid;
    private Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;
    [SerializeField] private SO_GridProperties[] so_GridPropertiesArray = null;
    [SerializeField] private Tile[] dugGround = null;
    [SerializeField] private Tile[] wateredGround = null;
    private bool isFirstTimeSceneLoaded = true;
    private const string tituloFirstTimeLoaded = "isFirstTimeSceneLoaded";

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }
    private void Start()
    {
        InitialiseGridProperties();
    }

    private void InitialiseGridProperties()
    {
        foreach(SO_GridProperties sO_GridProperties in so_GridPropertiesArray)
        {
            Dictionary<string, GridPropertyDetails> gridPropertiesDictionary = new Dictionary<string, GridPropertyDetails>();

            foreach(GridProperty gridProperty in sO_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertiesDictionary);
                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }

                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.esExcabable:
                        gridPropertyDetails.esExcavable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.esCamino:
                        gridPropertyDetails.esCamino = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.esNPCObstaculo:
                        gridPropertyDetails.esNPCObstaculo = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.esSoltarItem:
                        gridPropertyDetails.esSoltarItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.esSoltarMueble:
                        gridPropertyDetails.esSoltarMueble = gridProperty.gridBoolValue;
                        break;
                    default:
                        break;
                }
                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertiesDictionary);
            }
            SceneSave sceneSave = new SceneSave();

            sceneSave.gridPropertyDetailsDictionary = gridPropertiesDictionary;
            //Caso de escena inicial
            if (sO_GridProperties.sceneName.ToString() == SceneControlerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDetailsDictionary = gridPropertiesDictionary;
            }

            sceneSave.boolDictionary = new Dictionary<string, bool>();
            sceneSave.boolDictionary.Add(tituloFirstTimeLoaded, true);

            //Guardamos la escena en nuestro gameObject que contiene los datos de la escena
            GameObjectSave.sceneData.Add(sO_GridProperties.sceneName.ToString(), sceneSave);
        }
    }

    public void SetGridPropertyDetails(int x, int y, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(x, y, gridPropertyDetails, gridPropertyDetailsDictionary);
    }

    public void SetGridPropertyDetails(int x, int y, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertiesDictionary)
    {
        string key = "x" + x + "y" + y;

        gridPropertyDetails.gridX = x;
        gridPropertyDetails.gridY = y;

        gridPropertiesDictionary[key] = gridPropertyDetails;
    }

    /// <summary>
    /// Retorna las propiedades de la casilla en la coordenada dada
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="gridPropertiesDictionary"></param>
    /// <returns></returns>
    public GridPropertyDetails GetGridPropertyDetails(int x, int y, Dictionary<string, GridPropertyDetails> gridPropertiesDictionary)
    {
        string key = "x" + x + "y" + y;
        GridPropertyDetails gridPropertyDetails;

        if (!gridPropertiesDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            return null;
        }else
        {
            return gridPropertyDetails;
        }
    }

    public GridPropertyDetails GetGridPropertyDetails(int x, int y)
    {
        return GetGridPropertyDetails(x, y, gridPropertyDetailsDictionary);
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }
    private void OnDisable()
    {
        ISaveableDeregister();

        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }

    private void AfterSceneLoaded()
    {
        GameObject cropParentTransoformGO = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform);
        if (cropParentTransoformGO != null)
        {
            cropParentTransform = cropParentTransoformGO.transform;
        }
        else
        {
            cropParentTransform = null;
        }
        
        grid = GameObject.FindObjectOfType<Grid>();
        //Recogemos toda la info de las capas groundDecoration de la escena
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void IsaveableStoreScene(string sceneName)
    {
        GameObjectSave.sceneData.Remove(sceneName);
        SceneSave sceneSave = new SceneSave();
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;

        sceneSave.boolDictionary = new Dictionary<string, bool>();
        sceneSave.boolDictionary.Add(tituloFirstTimeLoaded, isFirstTimeSceneLoaded);

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void IsaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDetailsDictionary = sceneSave.gridPropertyDetailsDictionary;
            }

            if (sceneSave.boolDictionary != null && sceneSave.boolDictionary.TryGetValue(tituloFirstTimeLoaded, out bool storedIsFirstTimeLoaded))
            {
                isFirstTimeSceneLoaded = storedIsFirstTimeLoaded;
            }
            if (isFirstTimeSceneLoaded)
                EventHandler.CallInstantiateCropPrefabsEvent();

            //Seteamos el estado de los tiles de la escena
            if (gridPropertyDetailsDictionary.Count > 0)
            {
                //Destruimos las capas groundDecoration1 y 2 (tenemos el estado guardado en gridPropertyDictionary)
                ClearDisplayGridPropertyDetails();
                //Seteamos las capas groundDecoration1 y 2 a partir del dictionary
                DisplayGridPropertyDetails();
            }
            if (isFirstTimeSceneLoaded) { isFirstTimeSceneLoaded = false; }
        }
    }

    public GameObjectSave IsaveableSave()
    {
        IsaveableStoreScene(SceneManager.GetActiveScene().name);
        return GameObjectSave;
    }

    public void IsaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;
            IsaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
    private void ClearDisplayGroundDecorations()
    {
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }


    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();

        ClearDisplayAllPlantedCrops();
    }

    private void ClearDisplayAllPlantedCrops()
    {
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();
        foreach(Crop crop in cropArray)
        {
            Destroy(crop.gameObject);
        }
    }

    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);
        }
    }

   

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        if ( gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }

    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        //Seteamos la casilla arada en las coordenadas de gridPropDetails, en la capa de Decoration2
        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);
        GridPropertyDetails adjacentGridPropertyDetails;
        //Hay que seleccionar el tile basandonos en las casillas adyacentes ortogonalmente
        //ARRIBA
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }
        //ABAJO
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }
        //IZQUIERDA
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }
        //DERECHA
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }

    
    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //Seteamos la casilla arada en las coordenadas de gridPropDetails, en la capa de Decoration1
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        GridPropertyDetails adjacentGridPropertyDetails;
        //Hay que seleccionar el tile basandonos en las casillas adyacentes ortogonalmente
        //ARRIBA
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if ( adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY +1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY +1, 0), dugTile1);
        }
        //ABAJO
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }
        //IZQUIERDA
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile3);
        }
        //DERECHA
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
    }

    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        bool upWatered = IsGridSquareDug(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightWatered  = IsGridSquareDug(xGrid + 1, yGrid);

        #region Retornamos el tile apropiado en funcion del estado de los tiles adyacentes
        if (!upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[0];
        }
        else if (!upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[1];
        }
        else if (!upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[2];
        }
        else if (!upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[3];
        }
        else if (!upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[4];
        }
        else if (!upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[5];
        }
        else if (!upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[6];
        }
        else if (!upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[7];
        }
        else if (upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[8];
        }
        else if (upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[9];
        }
        else if (upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[10];
        }
        else if (upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[11];
        }
        else if (upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[12];
        }
        else if (upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[13];
        }
        else if (upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[14];
        }
        else if (upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[15];
        }
        return null;
        #endregion
    }


    private Tile SetDugTile(int xGrid, int yGrid)
    {
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid -1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid +1, yGrid);

        #region Retornamos el tile apropiado en funcion del estado de los tiles adyacentes
        if (!upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        else if (!upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[1];
        }
        else if (!upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[2];
        }
        else if (!upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[3];
        }
        else if (!upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        else if (!upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[5];
        }
        else if (!upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[6];
        }
        else if (!upDug && downDug && rightDug && leftDug)
        {
            return dugGround[7];
        }
        else if (upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        else if (upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[9];
        }
        else if (upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[10];
        }
        else if (upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[11];
        }
        else if (upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        else if (upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[13];
        }
        else if (upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[14];
        }
        else if (upDug && downDug && rightDug && leftDug)
        {
            return dugGround[15];
        }
        return null;
        #endregion
    }

    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceWatered > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if ( gridPropertyDetails == null)
        {
            return false;
        }else if ( gridPropertyDetails.daysSinceDug > -1)
        {
            return true;
        }else
        {
            return false;
        }
    }

    private void DisplayGridPropertyDetails()
    {
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDetailsDictionary)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;
            DisplayDugGround(gridPropertyDetails);
            DisplayWateredGround(gridPropertyDetails);
            DisplayPlantedCrop(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.seedItemCode > -1)
        {
            CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);
            if (cropDetails != null)
            {
                GameObject cropPrefab;
                int fasesCrecimiento = cropDetails.diasCrecimiento.Length;

                int faseCrecimientoActual = 0;
                //Obtenemos la fase de crecimiento actual
                for (int i = fasesCrecimiento - 1; i >= 0; i--)
                {
                    if (gridPropertyDetails.growthDays >= cropDetails.diasCrecimiento[i])
                    {
                        faseCrecimientoActual = i;
                        break;
                    }
                }
                cropPrefab = cropDetails.crecimientoPrefab[faseCrecimientoActual];

                Sprite growthSprite = cropDetails.crecimientoSprite[faseCrecimientoActual];

                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
                //Ajustamos la posicion. Actualmente apunta a la esquina superior izquierda de la casilla, lo ajustamos para que apunte al centro
                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z);

                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);
                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                cropInstance.transform.SetParent(cropParentTransform);
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }
            else
            {
                Debug.Log("FALTA LA ENTRADA PARA LA SEMILLA " + gridPropertyDetails.seedItemCode+ "en so_CropDetailsList");
            }
        }
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        ClearDisplayGridPropertyDetails();
        //Iteramos las propiedades del grid de todas las escenas
        foreach (SO_GridProperties sO_GridProperties in so_GridPropertiesArray)
        {
            //Obtenemos el gridPropertyDetailsDictionary para la escena
            if (GameObjectSave.sceneData.TryGetValue(sO_GridProperties.sceneName.ToString(), out SceneSave sceneSave)){
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    //EJEMPLO Iterando un diccionario
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count -1; i >= 0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);
                        GridPropertyDetails gridPropertyDetails = item.Value;
                        #region Se actualizan las propiedades para hacer efectivo el paso del dia
                        //Modificamos el estado de las plantaciones
                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays += 1;
                        }
                        //Reseteamos las casillas regadas
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);
                        #endregion Se actualizan las propiedades para hacer efectivo el paso del dia
                    }
                }
            }
        }
        //Actualizamos el gridPropertyDetails que se muestra en la escena para reflejar los cambios
        DisplayGridPropertyDetails();
    }

    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);
        Crop crop = null;

        for (int i  = 0; i < collider2DArray.Length; i++)
        {
            //Nos quedamos con el gameobject padre o hijo que sea de tipo Crop y este en el grid "gridPropertyDetails"
            crop = collider2DArray[i].GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
            {
                break;
            }
            crop = collider2DArray[i].GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
            {
                break;
            }
        }
        return crop;
    }

    public CropDetails GetCropDetails(int itemCode)
    {
        return so_CropDetailsList.GetCropDetails(itemCode);
    }

    
}
