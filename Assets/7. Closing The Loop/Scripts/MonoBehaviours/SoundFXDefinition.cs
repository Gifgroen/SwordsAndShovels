using UnityEngine;

[System.Serializable]
public struct SoundFXDefinition 
{
    public SoundEffect Effect;
    public AudioClip Clip;
}

[System.Serializable]
public enum SoundEffect
{
    HeroHit,
    LevelUp,
    MobDamage,
    MobDeath,
    NextWave
}
