using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewStats", menuName = "Character/Stats", order = 1)]
public class CharacterStats_SO : ScriptableObject
{
    public Events.EventIntegerEvent onLevelUp;
    public Events.EventIntegerEvent onHeroDamaged;
    public Events.EventIntegerEvent onHeroGainedHealth;
    public UnityEvent onHeroDeath;
    public UnityEvent onHeroInitialized;

    [System.Serializable]
    public class CharLevel
    {
        public int maxHealth;
        public int maxMana;
        public int maxWealth;
        public int baseDamage;
        public float baseResistance;
        public float maxEncumbrance;
        public int requiredXp;
    }

    #region Fields
    public bool isHero = false;

    public ItemPickUp misc1 { get; private set; }
    public ItemPickUp misc2 { get; private set; }

    public int maxHealth = 0;
    public int currentHealth = 0;

    public int maxWealth = 0;
    public int currentWealth = 0;

    public int maxMana = 0;
    public int currentMana = 0;

    public int baseDamage = 0;
    public int currentDamage = 0;

    public float baseResistance = 0;
    public float currentResistance = 0f;

    public float maxEncumbrance = 0f;
    public float currentEncumbrance = 0f;

    public int charExperience = 0;
    public int charLevel = 0;

    public CharLevel[] charLevels;
    
    private ItemPickUp weapon { get; set; }
    private ItemPickUp headArmor { get; set; }
    private ItemPickUp chestArmor { get; set; }
    private ItemPickUp handArmor { get; set; }
    private ItemPickUp legArmor { get; set; }
    private ItemPickUp footArmor { get; set; }
    
    public Weapon CurrentWeapon()
    {
        return weapon != null ? weapon.itemDefinition.weaponSlotObject: null;
    }
    #endregion

    #region Stat Increasers
    public void ApplyHealth(int amount)
    {
        if ((currentHealth + amount) > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += amount;
        }
        if (isHero) {
            onHeroGainedHealth.Invoke(amount);
        }
    }

    public void ApplyMana(int amount)
    {
        if ((currentMana + amount) > maxMana)
        {
            currentMana = maxMana;
        }
        else
        {
            currentMana += amount;
        }
    }

    public void GiveWealth(int amount)
    {
        if ((currentWealth + amount) > maxWealth)
        {
            currentWealth = maxWealth;
        }
        else
        {
            currentWealth += amount;
        }
    }

    public void GiveXp(int xp)
    {
        charExperience += xp;
        if (charLevel >= charLevels.Length)
        {
            return;
        }
        if (charExperience >= charLevels[charLevel].requiredXp)
        {
            SetCharacterLevel(charLevel);
        }
    }

    public void EquipWeapon(ItemPickUp weaponPickUp, CharacterInventory charInventory, GameObject weaponSlot)
    {
        weapon = weaponPickUp;
        charInventory.inventoryDisplaySlots[2].sprite = weaponPickUp.itemDefinition.itemIcon;
        Instantiate(weaponPickUp.itemDefinition.weaponSlotObject.weaponPreb, weaponSlot.transform);
        currentDamage = baseDamage + weapon.itemDefinition.amount;
    }

    public void EquipArmor(ItemPickUp armorPickUp, CharacterInventory charInventory)
    {
        switch (armorPickUp.itemDefinition.itemArmorSubType)
        {
            case ItemArmorSubType.Head:
                charInventory.inventoryDisplaySlots[3].sprite = armorPickUp.itemDefinition.itemIcon;
                headArmor = armorPickUp;
                currentResistance += armorPickUp.itemDefinition.amount;
                break;
            case ItemArmorSubType.Chest:
                charInventory.inventoryDisplaySlots[4].sprite = armorPickUp.itemDefinition.itemIcon;
                chestArmor = armorPickUp;
                currentResistance += armorPickUp.itemDefinition.amount;
                break;
            case ItemArmorSubType.Hands:
                charInventory.inventoryDisplaySlots[5].sprite = armorPickUp.itemDefinition.itemIcon;
                handArmor = armorPickUp;
                currentResistance += armorPickUp.itemDefinition.amount;
                break;
            case ItemArmorSubType.Legs:
                charInventory.inventoryDisplaySlots[6].sprite = armorPickUp.itemDefinition.itemIcon;
                legArmor = armorPickUp;
                currentResistance += armorPickUp.itemDefinition.amount;
                break;
            case ItemArmorSubType.Boots:
                charInventory.inventoryDisplaySlots[7].sprite = armorPickUp.itemDefinition.itemIcon;
                footArmor = armorPickUp;
                currentResistance += armorPickUp.itemDefinition.amount;
                break;
        }
    }
    #endregion

    #region Stat Reducers
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (isHero)
        {
            onHeroDamaged.Invoke(amount);
        }

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    public void TakeMana(int amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
        {
            currentMana = 0;
        }
    }

    public bool UnEquipWeapon(ItemPickUp weaponPickUp, CharacterInventory charInventory, GameObject weaponSlot)
    {
        if (weapon == null)
        {
            return false;
        }
        
        bool previousWeaponSame = weapon == weaponPickUp;
        
        charInventory.inventoryDisplaySlots[2].sprite = null;
        Destroy(weaponSlot.transform.GetChild(0).gameObject);
        weapon = null;
        currentDamage = baseDamage;

        return previousWeaponSame;
    }

    public bool UnEquipArmor(ItemPickUp armorPickUp, CharacterInventory charInventory)
    {
        bool previousArmorSame = false;

        switch (armorPickUp.itemDefinition.itemArmorSubType)
        {
            case ItemArmorSubType.Head:
                if (headArmor != null)
                {
                    if (headArmor == armorPickUp)
                    {
                        previousArmorSame = true;
                    }
                    charInventory.inventoryDisplaySlots[3].sprite = null;
                    currentResistance -= armorPickUp.itemDefinition.amount;
                    headArmor = null;
                }
                break;
            case ItemArmorSubType.Chest:
                if (chestArmor != null)
                { 
                    if (chestArmor == armorPickUp)
                    {
                        previousArmorSame = true;
                    }
                    charInventory.inventoryDisplaySlots[4].sprite = null;
                    currentResistance -= armorPickUp.itemDefinition.amount;
                    chestArmor = null;
                }
                break;
            case ItemArmorSubType.Hands:
                if (handArmor != null)
                {
                    if (handArmor == armorPickUp)
                    {
                        previousArmorSame = true;
                    }
                    charInventory.inventoryDisplaySlots[5].sprite = null;
                    currentResistance -= armorPickUp.itemDefinition.amount;
                    handArmor = null;
                }
                break;
            case ItemArmorSubType.Legs:
                if (legArmor != null)
                {
                    if (legArmor == armorPickUp)
                    {
                        previousArmorSame = true;
                    }
                    charInventory.inventoryDisplaySlots[6].sprite = null;
                    currentResistance -= armorPickUp.itemDefinition.amount;
                    legArmor = null;
                }
                break;
            case ItemArmorSubType.Boots:
                if (footArmor != null)
                {
                    if (footArmor == armorPickUp)
                    {
                        previousArmorSame = true;
                    }
                    charInventory.inventoryDisplaySlots[7].sprite = null;
                    currentResistance -= armorPickUp.itemDefinition.amount;
                    footArmor = null;
                }
                break;
        }
        return previousArmorSame;
    }
    #endregion

    #region Character Level Up and Death
    private void Death()
    {
        if (isHero)
        {
            onHeroDeath.Invoke();
        }
    }

    public void SetCharacterLevel(int newLevel)
    {
        charLevel = newLevel + 1;

        maxHealth = charLevels[newLevel].maxHealth;
        currentHealth = charLevels[newLevel].maxHealth;

        maxMana = charLevels[newLevel].maxMana;
        currentMana = charLevels[newLevel].maxMana;

        maxWealth = charLevels[newLevel].maxWealth;

        baseDamage = charLevels[newLevel].baseDamage;

        currentDamage = charLevels[newLevel].baseDamage;
        if (weapon != null)
        {
            currentDamage += weapon.itemDefinition.amount;
        }

        baseResistance = charLevels[newLevel].baseResistance;
        currentResistance = charLevels[newLevel].baseResistance;

        maxEncumbrance = charLevels[newLevel].maxEncumbrance;

        if (charLevel > 1)
        {
            onLevelUp.Invoke(charLevel);
        }
    }
    #endregion
}
