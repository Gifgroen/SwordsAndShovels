using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecapController : MonoBehaviour 
{
    [SerializeField] private Text WinOrLoss;
    [SerializeField] private Text SessionDate;
    [SerializeField] private Text HighestLevel;
    [SerializeField] private Text MobsKilled;
    [SerializeField] private Text ExperienceGained;
    [SerializeField] private Text WavesCompleted;

    [SerializeField] private Slider SessionSlider;

    private SessionStats DisplayStats;

    // Use this for initialization
    void Start () 
    {
        int count = StatsManager.sessionKeeper.Sessions.Count - 1;
        SessionSlider.maxValue = count;
        SessionSlider.value = count;

        DisplaySessionStats(count);
	}
	
    public void DisplaySessionStats(float Session)
    {
        DisplayStats = StatsManager.sessionKeeper.Sessions[(int)Session];

        WinOrLoss.text = DisplayStats.WinOrLoss.ToString();
        SessionDate.text = DisplayStats.SessionDate;
        HighestLevel.text = DisplayStats.HighestLevel.ToString();
        MobsKilled.text = DisplayStats.MobsKilled.ToString();
        ExperienceGained.text = DisplayStats.ExperienceGained.ToString();
        WavesCompleted.text = DisplayStats.WavesCompleted.ToString();
    }
	
}
