using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System;

public class GameManager : Manager<GameManager>
{
    public const string Tag = "GameManager";
    
    [SerializeField]
    private string initialLevelName = "Main";
    private const string GameOverLevelName = "GameOver";
    private const string RecapLevelName = "Recap";

    public enum GameState
    {
        Pregame,
        Running,
        Paused,
        Postgame
    }

    public GameObject[] systemPrefabs;
    public Events.EventGameState onGameStateChanged;

    private List<GameObject> instancedSystemPrefabs;

    private GameState currentGameState = GameState.Pregame;

    private string currentLevelName = string.Empty;

    private static SessionStats _currentSession;

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
        get { return currentGameState; }
        set { currentGameState = value; }
    }

    private void Start()
    {
        instancedSystemPrefabs = new List<GameObject>();
        InstantiateSystemPrefabs();
        UIManager.Instance.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
    }

    private void Update()
    {
        if (currentGameState == GameState.Pregame)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void OnLoadOperationComplete(AsyncOperation ao)
    {

        if (currentLevelName == initialLevelName)
        {
            UpdateState(GameState.Running);
            InitSessions();
        }

        Debug.Log("Load Complete.");
    }

    private static void HandleMainMenuFadeComplete(bool fadeOut)
    {
        if (!fadeOut)
        {
            // UnloadLevel(_currentLevelName);
        }
    }

    void UpdateState(GameState state)
    {
        GameState previousGameState = currentGameState;
        currentGameState = state;

        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (currentGameState)
        {
            case GameState.Pregame:
                Time.timeScale = 1.0f;
                break;
            case GameState.Running:
                Time.timeScale = 1.0f;
                break;
            case GameState.Paused:
                Time.timeScale = 0.0f;
                break;
        }

        onGameStateChanged.Invoke(currentGameState, previousGameState);
    }

    private void InstantiateSystemPrefabs()
    {
        foreach (var fab in systemPrefabs)
        {
            instancedSystemPrefabs.Add(Instantiate(fab));
        }
    }

    private void LoadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Single);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }
        ao.completed += OnLoadOperationComplete;

        currentLevelName = levelName;
    }

    protected void OnDestroy()
    {
        if (instancedSystemPrefabs == null)
            return;

        foreach (var fab in instancedSystemPrefabs)
        {
            Destroy(fab);
        }
        instancedSystemPrefabs.Clear();
    }

    public void StartGame()
    {
        LoadLevel(initialLevelName);
    }

    public void TogglePause()
    {
        UpdateState(currentGameState == GameState.Running ? GameState.Paused : GameState.Running);
    }

    public void RestartGame()
    {
        UpdateState(GameState.Pregame);
    }

    public static void QuitGame()
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
        _currentSession.WavesCompleted += 1;
        SaveSession(EndGameState.Win);
        UIManager.Instance.PlayYouWin();
    }

    public void OnNextWave()
    {
        _currentSession.WavesCompleted += 1;
        UIManager.Instance.PlayNextWave();
    }


    public void OnHeroInit()
    {
        UIManager.Instance.InitUnitFrame();
        Debug.LogWarning("Hero Initialized");
    }

    public void OnMobDied()
    {
        _currentSession.MobsKilled += 1;
    }

    #endregion

    public IEnumerator EndGame()
    {
        UpdateState(GameState.Postgame);
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.HideUI();
        SceneManager.LoadScene(GameOverLevelName);
    }

    public void RestartFromEndGame()
    {
        SceneManager.LoadScene(initialLevelName);
        InitSessions();
        UIManager.Instance.ShowUI();
        RestartGame();
    }

    public void ShowRecap()
    {
        SceneManager.LoadScene(RecapLevelName);
    }

    #region Stats

    private static void InitSessions()
    {
        StatsManager.SaveFilePath = Path.Combine(Application.persistentDataPath, "saveGame.json");
        StatsManager.LoadSessions();
        _currentSession = new SessionStats();
    }

    public void SaveSession(EndGameState endGameState)
    {
        _currentSession.SessionDate = DateTime.Now.ToLongDateString();
        _currentSession.HighestLevel = hero.GetCurrentLevel();
        _currentSession.WinOrLoss = endGameState;
        _currentSession.ExperienceGained = hero.GetCurrentXp();

        StatsManager.sessionKeeper.Sessions.Add(_currentSession);
        StatsManager.SaveSessions();
    }

    #endregion
}
