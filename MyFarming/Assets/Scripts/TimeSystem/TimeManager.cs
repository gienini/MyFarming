using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>, ISaveable
{
    private int gameYear = 1;
    private Season gameSeason = Season.Primavera;
    private int gameDay = 1;
    private string gameDayOfWeek = "Lun";
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;

    private bool gameClockPaused = false;
    private float gameTick = 0f;
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get => _iSaveableUniqueID; set => _iSaveableUniqueID = value; }
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoadFadeIn;
    }

    private void AfterSceneLoadFadeIn()
    {
        gameClockPaused = false;
    }

    private void BeforeSceneUnloadFadeOut()
    {
        gameClockPaused = true;
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoadFadeIn;
    }
    protected override void Awake()
    {
        base.Awake();
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }
    private void Start()
    {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        }
    }
    //Asi sabemos como pasa un segundo del juego
    private void GameTick()
    {
        //Tiempo de refresco de cada frame
        gameTick += Time.deltaTime;
        if (gameTick >= Settings.secondsPerGameSecond)
        {
            gameTick -= Settings.secondsPerGameSecond;
            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;
        if (gameSecond > 59)
        {
            gameSecond = 0;
            gameMinute++;
            if (gameMinute > 59)
            {
                gameMinute = 0;
                gameHour++;
                if (gameHour > 23)
                {
                    gameHour = 0;
                    gameDay++;
                    if (gameDay > 30)
                    {
                        gameDay = 1;
                        int gs = (int)gameSeason;
                        gs++;

                        gameSeason = (Season)gs;
                        if (gs > 3)
                        {
                            gs = 0;
                            gameSeason = (Season)gs;

                            gameYear++;
                            if (gameYear > 9999)
                            {
                                gameYear = 1;
                            }
                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }
                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }
                    gameDayOfWeek = GetDayOfWeek();
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }
                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }
            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            //Debug.Log("Segundo: " + gameSecond + " Minuto: " + gameMinute + " Hora: " + gameHour + " Dia del mes: " + gameDay + " Dia de la semana :" + gameDayOfWeek + " Season: " + gameSeason + " Año: " + gameYear);
        }
        //Aqui se podria llamar al evento de avanzar game second
    }

    private string GetDayOfWeek()
    {
        int totalDays = (((int)gameSeason) * 30 ) + gameDay;
        int dayOfWeek = totalDays % 7;
        switch (dayOfWeek)
        {
            case 1:
                return "Lun";
            case 2:
                return "Mar";
            case 3:
                return "Mie";
            case 4:
                return "Jue";
            case 5:
                return "Vie";
            case 6:
                return "Sab";
            case 0:
                return "Dom";
            default:
                return null;

        }

    }

    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++ )
        {
            UpdateGameSecond();
        }
    }

    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave IsaveableSave()
    {
        SceneSave sceneSave = new SceneSave();

        GameObjectSave.sceneData.Remove(Settings.PersistentScene);
        sceneSave.intDictionary = new Dictionary<string, int>();
        sceneSave.stringDictionary = new Dictionary<string, string>();

        sceneSave.intDictionary.Add("gameYear", gameYear);
        sceneSave.intDictionary.Add("gameDay", gameDay);
        sceneSave.intDictionary.Add("gameHour", gameHour);
        sceneSave.intDictionary.Add("gameMinute", gameMinute);
        sceneSave.intDictionary.Add("gameSecond", gameSecond);

        sceneSave.stringDictionary.Add("gameDayOfWeek", gameDayOfWeek);
        sceneSave.stringDictionary.Add("gameSeason", gameSeason.ToString());

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void IsaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;
            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.intDictionary != null && sceneSave.stringDictionary != null)
                {
                    if (sceneSave.intDictionary.TryGetValue("gameYear", out int saveGameYear))
                    {
                        gameYear = saveGameYear;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameDay", out int saveGameDay))
                    {
                        gameDay = saveGameDay;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameHour", out int saveGameHour))
                    {
                        gameHour = saveGameHour;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameMinute", out int saveGameMinute))
                    {
                        gameMinute = saveGameMinute;
                    }
                    if (sceneSave.intDictionary.TryGetValue("gameSecond", out int saveGameSecond))
                    {
                        gameSecond = saveGameSecond;
                    }

                    if (sceneSave.stringDictionary.TryGetValue("gameDayOfWeek", out string saveGameDayOfWeek))
                    {
                        gameDayOfWeek = saveGameDayOfWeek;
                    }
                    if (sceneSave.stringDictionary.TryGetValue("gameSeason", out string saveGameSeason))
                    {
                        if (Enum.TryParse<Season>(saveGameSeason, out Season season)){
                            gameSeason = season;
                        }
                    }

                    gameTick = 0f;

                    EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }
            }
        }
        
    }

    public void IsaveableStoreScene(string sceneName)
    {
        //NADA
    }

    public void IsaveableRestoreScene(string sceneName)
    {
        //NADA
    }
}
