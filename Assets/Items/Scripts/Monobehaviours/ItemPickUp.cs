using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public ItemPickUps_SO itemDefinition;

    private void StoreItem(CharacterInventory charInventory, CharacterStats stats)
    {
        charInventory.StoreItem(this, stats);
    }

    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    public void UseItem(CharacterStats charStats)
    {
        switch (itemDefinition.itemType)
        {
            case ItemTypeDefinitions.HEALTH:
                charStats.ApplyHealth(itemDefinition.amount);
                Debug.Log(charStats.GetHealth());
                break;
            case ItemTypeDefinitions.MANA:
                charStats.ApplyMana(itemDefinition.amount);
                break;
            case ItemTypeDefinitions.WEALTH:
                charStats.GiveWealth(itemDefinition.amount);
                break;
            case ItemTypeDefinitions.WEAPON:
                charStats.ChangeWeapon(this);
                break;
            case ItemTypeDefinitions.ARMOR:
                charStats.ChangeArmor(this);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var stats = other.GetComponent<CharacterStats>();
        if (itemDefinition.isStorable)
        {
            StoreItem(FindObjectOfType<CharacterInventory>(), stats);
        }
        else
        {
            UseItem(stats);
        }
    }
}
