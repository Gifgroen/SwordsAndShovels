using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterStats_SO characterDefinitionTemplate;
    public CharacterStats_SO characterDefinition;
    public CharacterInventory charInv;
    public GameObject characterWeaponSlot;

    #region Initializations
    private void Awake()
    {
        if (characterDefinitionTemplate != null)
        {
            characterDefinition = Instantiate(characterDefinitionTemplate);
        }
    }

    private void Start()
    {
        if (characterDefinition.isHero)
        {
            characterDefinition.SetCharacterLevel(0);
        }
    }
    #endregion

    #region Stat Increasers
    public void ApplyHealth(int amount)
    {
        characterDefinition.ApplyHealth(amount);
    }

    public void ApplyMana(int amount)
    {
        characterDefinition.ApplyMana(amount);
    }

    public void GiveWealth(int amount)
    {
        characterDefinition.GiveWealth(amount);
    }

    public void IncreaseXp(int xp)
    {
        characterDefinition.GiveXp(xp);
    }

    #endregion

    #region Stat Reducers
    public void TakeDamage(int amount)
    {
        characterDefinition.TakeDamage(amount);
    }

    public void TakeMana(int amount)
    {
        characterDefinition.TakeMana(amount);
    }
    #endregion

    #region Weapon and Armor Change
    public void ChangeWeapon(ItemPickUp weaponPickUp)
    {
        if (!characterDefinition.UnEquipWeapon(weaponPickUp, charInv, characterWeaponSlot))
        {
            characterDefinition.EquipWeapon(weaponPickUp, charInv, characterWeaponSlot);
        }
    }

    public void ChangeArmor(ItemPickUp armorPickUp)
    {
        if (!characterDefinition.UnEquipArmor(armorPickUp, charInv))
        {
            characterDefinition.EquipArmor(armorPickUp, charInv);
        }
    }
    #endregion

    #region Reporters
    public int GetHealth()
    {
        return characterDefinition.currentHealth;
    }

    public Weapon GetCurrentWeapon()
    {
        return characterDefinition.CurrentWeapon(); 
    }

    public int GetDamage()
    {
        return characterDefinition.currentDamage;
    }

    public float GetResistance()
    {
        return characterDefinition.currentResistance;
    }

    public bool IsOverEncumbered(float itemWeight)
    {
        return (characterDefinition.currentEncumbrance + itemWeight) > characterDefinition.maxEncumbrance;
    }
    #endregion

    #region Stat Initializers

    public void SetInitialHealth(int health)
    {
        characterDefinition.maxHealth = health;
        characterDefinition.currentHealth = health;
    }

    public void SetInitialResistance(int resistance)
    {
        characterDefinition.baseResistance = resistance;
        characterDefinition.currentResistance = resistance;
    }

    public void SetInitialDamage(int damage)
    {
        characterDefinition.baseDamage = damage;
        characterDefinition.currentDamage = damage;
    }

    #endregion
}
