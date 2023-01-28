public interface ISaveable
{
    string ISaveableUniqueID { get; set; }

    GameObjectSave GameObjectSave { get; set; }

    void ISaveableRegister();

    void ISaveableDeregister();

    GameObjectSave IsaveableSave();

    void IsaveableLoad(GameSave gameSave);

    void IsaveableStoreScene(string sceneName);

    void IsaveableRestoreScene(string sceneName);
}
