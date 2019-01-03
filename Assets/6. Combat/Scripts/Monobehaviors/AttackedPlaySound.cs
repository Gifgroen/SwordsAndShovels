using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedPlaySound : MonoBehaviour, IAttackable
{
    public void OnAttack(GameObject attacker, Attack attack)
    {
        SoundManager.Instance.PlaySoundEffect(SoundEffect.MobDamage);
    }
}
