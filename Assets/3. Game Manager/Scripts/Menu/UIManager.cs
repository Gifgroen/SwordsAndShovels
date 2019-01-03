using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Manager<UIManager>
{
    [SerializeField] private MainMenu _mainMenu;
    [SerializeField] private PauseMenu _pauseMenu;

    [SerializeField] private Camera _dummyCamera;
    [SerializeField] private GameObject unitFrame;

    [SerializeField] private Image healthBar;
    [SerializeField] private Text levelText;

    [SerializeField] private Image NextWave;
    [SerializeField] private Image GameOver;
    [SerializeField] private Image YouWin;

    [SerializeField] private Text TitleText;
    [SerializeField] private Text TagLine;



    public Events.EventFadeComplete OnMainMenuFadeComplete;

    private void Start()
    {
        _mainMenu.OnMainMenuFadeComplete.AddListener(HandleMainMenuFadeComplete);
        GameManager.Instance.OnGameStateChanged.AddListener(HandleGameStateChanged);
    }

    void HandleMainMenuFadeComplete(bool fadeOut)
    {
        TagLine.gameObject.SetActive(!fadeOut);
        TitleText.gameObject.SetActive(!fadeOut);

        unitFrame.SetActive(fadeOut);
        OnMainMenuFadeComplete.Invoke(fadeOut);
    }

    void HandleGameStateChanged(GameManager.GameState currentState, GameManager.GameState previousState)
    {
        _pauseMenu.gameObject.SetActive(currentState == GameManager.GameState.PAUSED);
        bool showUnitFrame = currentState == GameManager.GameState.RUNNING ||
                                                        currentState == GameManager.GameState.PAUSED;
        unitFrame.SetActive(showUnitFrame);
    } 
    
    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.PREGAME)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.StartGame();
        }
    }

    public void SetDummyCameraActive(bool active)
    {
        _dummyCamera.gameObject.SetActive(active);
    }

    public void InitUnitFrame()
    {
        levelText.text = "1";
        healthBar.fillAmount = 1;
    }

    public void UpdateUnitFrame(HeroController hero)
    {
        int curHealth = hero.GetCurrentHealth();
        int maxHealth = hero.GetMaxHealth();

        healthBar.fillAmount = (float)curHealth / maxHealth;
        levelText.text = hero.GetCurrentLevel().ToString();
    }

    public void PlayNextWave()
    {
        NextWave.gameObject.SetActive(true);
    }

    public void PlayGameOver()
    {
        GameOver.gameObject.SetActive(true);
    }

    public void PlayYouWin()
    {
        YouWin.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        unitFrame.SetActive(false);
        SetDummyCameraActive(false);
        _mainMenu.gameObject.SetActive(false);
        _pauseMenu.gameObject.SetActive(false);
        TagLine.gameObject.SetActive(false);
        TitleText.gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        _mainMenu.gameObject.SetActive(true);
        _mainMenu.FadeOut();
        GameManager.Instance.CurrentGameState = GameManager.GameState.RUNNING;
    }


}
