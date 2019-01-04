using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public HeroController hero;
    
    // Start is called before the first frame update
    private void Start()
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            throw new Exception("Expected a GameManager in the active game! Is null reference!");
        }
        gameManager.SetupHeroEventListeners(hero);
    }
}
