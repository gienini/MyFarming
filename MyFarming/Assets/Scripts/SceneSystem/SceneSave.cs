﻿
using System.Collections.Generic;
[System.Serializable]

public class SceneSave
{
    public Dictionary<string, int> intDictionary;
    public Dictionary<string, bool> boolDictionary;
    public List<SceneItem> listSceneItem;
    public Dictionary<string, string> stringDictionary;
    public Dictionary<string, Vector3Serializable> vector3Dictionary;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;
    public List<InventoryItem>[] listInvItemArray;
    public Dictionary<string, int[]> intArrayDictionary;
}
