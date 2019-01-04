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

    public GameState CurrentGameState { get; set; } = GameState.Pregame;

    private void Start()
    {
        instancedSystemPrefabs = new List<GameObject>();
        InstantiateSystemPrefabs();
        UIManager.Instance.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
    }

    public void SetupHeroEventListeners(HeroController foundHero)
    {
        foundHero.RegisterOnHeroInitialised(OnHeroInit);
        foundHero.RegisterOnLevelUpListener(OnHeroLeveledUp);
        foundHero.RegisterOnDamagedListener(OnHeroDamaged);
        foundHero.RegisterOnGainedHealthListener(OnHeroGainedHealth);
        foundHero.RegisterOnHeroDeathListener(OnHeroDied);        
    }

    private void Update()
    {
        if (CurrentGameState == GameState.Pregame)
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

    private void UpdateState(GameState state)
    {
        GameState previousGameState = CurrentGameState;
        CurrentGameState = state;

        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (CurrentGameState)
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

        onGameStateChanged.Invoke(CurrentGameState, previousGameState);
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
        UpdateState(CurrentGameState == GameState.Running ? GameState.Paused : GameState.Running);
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

    public void OnHeroLeveledUp(int level)
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
        UIManager.Instance.HideUi();
        SceneManager.LoadScene(GameOverLevelName);
    }

    public void RestartFromEndGame()
    {
        SceneManager.LoadScene(initialLevelName);
        InitSessions();
        UIManager.Instance.ShowUi();
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
