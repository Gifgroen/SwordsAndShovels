using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System;

public class GameManager : Manager<GameManager>
{
    public enum GameState
    {
        PREGAME,
        RUNNING,
        PAUSED,
        POSTGAME
    }

    public GameObject[] SystemPrefabs;
    public Events.EventGameState OnGameStateChanged;

    List<GameObject> _instancedSystemPrefabs;

    GameState _currentGameState = GameState.PREGAME;

    string _currentLevelName = string.Empty;

    private static SessionStats CurrentSession;

    private HeroController heroController;

    private HeroController hero
    {
        get
        {
            if (null == heroController)
            {
                heroController = FindObjectOfType<HeroController>();
            }

            return heroController;
        }
    }

    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        set { _currentGameState = value; }
    }

    private void Start()
    {
        _instancedSystemPrefabs = new List<GameObject>();

        InstantiateSystemPrefabs();

        UIManager.Instance.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
    }

    private void Update()
    {
        if (_currentGameState == GameState.PREGAME)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void OnLoadOperationComplete(AsyncOperation ao)
    {

        if (_currentLevelName == "Main")
        {
            UpdateState(GameState.RUNNING);
            Instance.InitSessions();
        }

        Debug.Log("Load Complete.");
    }

    void HandleMainMenuFadeComplete(bool fadeOut)
    {
        if (!fadeOut)
        {
            // UnloadLevel(_currentLevelName);
        }
    }

    void UpdateState(GameState state)
    {
        GameState previousGameState = _currentGameState;
        _currentGameState = state;

        switch (_currentGameState)
        {
            case GameState.PREGAME:
                Time.timeScale = 1.0f;
                break;

            case GameState.RUNNING:
                Time.timeScale = 1.0f;
                break;

            case GameState.PAUSED:
                Time.timeScale = 0.0f;
                break;

            default:
                break;
        }

        OnGameStateChanged.Invoke(_currentGameState, previousGameState);
    }

    void InstantiateSystemPrefabs()
    {
        GameObject prefabInstance;
        for (int i = 0; i < SystemPrefabs.Length; ++i)
        {
            prefabInstance = Instantiate(SystemPrefabs[i]);
            _instancedSystemPrefabs.Add(prefabInstance);
        }
    }

    public void LoadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }

        ao.completed += OnLoadOperationComplete;

        _currentLevelName = levelName;
    }

    protected void OnDestroy()
    {
        if (_instancedSystemPrefabs == null)
            return;

        for (int i = 0; i < _instancedSystemPrefabs.Count; ++i)
        {
            Destroy(_instancedSystemPrefabs[i]);
        }
        _instancedSystemPrefabs.Clear();
    }

    public void StartGame()
    {
        LoadLevel("Main");
    }

    public void TogglePause()
    {
        UpdateState(_currentGameState == GameState.RUNNING ? GameState.PAUSED : GameState.RUNNING);
    }

    public void RestartGame()
    {
        UpdateState(GameState.PREGAME);
    }

    public void QuitGame()
    {
        // implement features for quitting
        Application.Quit();
    }

    #region CallBacks

    public void OnHeroLeveledUp(int newLevel)
    {
        UIManager.Instance.UpdateUnitFrame(hero);
        SoundManager.Instance.PlaySoundEffect(SoundEffect.LevelUp);
    }

    public void OnHeroDamaged(int damage)
    {
        UIManager.Instance.UpdateUnitFrame(hero);
        SoundManager.Instance.PlaySoundEffect(SoundEffect.HeroHit);
    }

    public void OnHeroGainedHealth(int health)
    {
        UIManager.Instance.UpdateUnitFrame(hero);
        Debug.LogWarningFormat("Hero gained {0} health", health);
    }

    public void OnHeroDied()
    {
        UIManager.Instance.UpdateUnitFrame(hero);
        UIManager.Instance.PlayGameOver();
        SaveSession(EndGameState.Loss);
        StartCoroutine(EndGame());
    }

    public void OnOutOfWaves()
    {
        CurrentSession.WavesCompleted += 1;
        SaveSession(EndGameState.Win);
        UIManager.Instance.PlayYouWin();
    }

    public void OnNextWave()
    {
        CurrentSession.WavesCompleted += 1;
        UIManager.Instance.PlayNextWave();
    }


    public void OnHeroInit()
    {
        UIManager.Instance.InitUnitFrame();
        Debug.LogWarning("Hero Initialized");
    }

    public void OnMobDied()
    {
        CurrentSession.MobsKilled += 1;
    }

    #endregion

    public IEnumerator EndGame()
    {
        UpdateState(GameState.POSTGAME);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideUI();
        SceneManager.LoadScene("GameOver");
    }

    public void RestartFromEndGame()
    {
        SceneManager.LoadScene("Main");
        Instance.InitSessions();
        UIManager.Instance.ShowUI();
        RestartGame();
    }

    public void ShowRecap()
    {
        SceneManager.LoadScene("Recap");
    }

    #region Stats

    private void InitSessions()
    {
        StatsManager.SaveFilePath = Path.Combine(Application.persistentDataPath, "saveGame.json");
        StatsManager.LoadSessions();
        CurrentSession = new SessionStats();
    }

    public void SaveSession(EndGameState endGameState)
    {
        CurrentSession.SessionDate = DateTime.Now.ToLongDateString();
        CurrentSession.HighestLevel = hero.GetCurrentLevel();
        CurrentSession.WinOrLoss = endGameState;
        CurrentSession.ExperienceGained = hero.GetCurrentXP();

        StatsManager.sessionKeeper.Sessions.Add(CurrentSession);
        StatsManager.SaveSessions();
    }

    #endregion
}
