using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{

    public List<ISaveable> iSaveableObjectList;
    public GameSave gameSave;

    protected override void Awake()
    {
        base.Awake();
        iSaveableObjectList = new List<ISaveable>();
    }

    public void LoadDataFromFile()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if(File.Exists(Application.persistentDataPath + Settings.RutaRelativaSaveGame))
        {
            gameSave = new GameSave();
            FileStream file = File.Open(Application.persistentDataPath + Settings.RutaRelativaSaveGame, FileMode.Open);

            gameSave = (GameSave)bf.Deserialize(file);

            for (int i = iSaveableObjectList.Count -1 ; i > -1 ; i--)
            {
                if (gameSave.gameObjectData.ContainsKey(iSaveableObjectList[i].ISaveableUniqueID))
                {
                    iSaveableObjectList[i].IsaveableLoad(gameSave);
                }
                else
                {
                    Component component = (Component)iSaveableObjectList[i];
                    Destroy(component.gameObject);
                }
            }
            file.Close();
        }
        UIManager.Instance.DisablePausemenu();
    }

    public void SaveDataToFile()
    {
        gameSave = new GameSave();

        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID, iSaveableObject.IsaveableSave());
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + Settings.RutaRelativaSaveGame, FileMode.Create);

        bf.Serialize(file, gameSave);
        file.Close();
        UIManager.Instance.DisablePausemenu();
    }

    public void StoreCurrentSceneData()
    {
        foreach(ISaveable reg in iSaveableObjectList)
        {
            reg.IsaveableStoreScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RestoreCurrentSceneData()
    {
        foreach (ISaveable reg in iSaveableObjectList)
        {
            reg.IsaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

}
