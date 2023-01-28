using System.Collections.Generic;
[System.Serializable]
public class GameSave
{
    //key = GUID
    public Dictionary<string, GameObjectSave> gameObjectData;

    public GameSave()
    {
        gameObjectData = new Dictionary<string, GameObjectSave>();
    }
}
