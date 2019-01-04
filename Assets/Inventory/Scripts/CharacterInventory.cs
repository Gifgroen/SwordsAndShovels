using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInventory : MonoBehaviour
{
    #region Variable Declarations
    public CharacterStats charStats;

    public Image[] hotBarDisplayHolders = new Image[4];
    public GameObject inventoryDisplayHolder;
    public Image[] inventoryDisplaySlots = new Image[30];

    private const int InventoryItemCap = 20;
    
    private int idCount = 1;
    private bool addedItem = true;

    public Dictionary<int, InventoryEntry> itemsInInventory = new Dictionary<int, InventoryEntry>();
    public InventoryEntry itemEntry;
    #endregion

    #region Initializations

    private void Start()
    {
        itemEntry = new InventoryEntry(0, null, null);
        itemsInInventory.Clear();

        inventoryDisplaySlots = inventoryDisplayHolder.GetComponentsInChildren<Image>();
    }
    #endregion

    private void Update()
    {
        #region Watch for Hotbar Keypresses - Called by Character Controller Later
        //Checking for a hotbar key to be pressed
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerItemUse(101);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TriggerItemUse(102);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TriggerItemUse(103);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TriggerItemUse(104);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayInventory();
        }
        #endregion

        //Check to see if the item has already been added - Prevent duplicate adds for 1 item
        if (!addedItem)
        {
            TryPickUp();
        }
    }

    public void StoreItem(ItemPickUp itemToStore, CharacterStats stats)
    {
        addedItem = false;

        if (stats.IsOverEncumbered(itemToStore.itemDefinition.itemWeight))
        {
            return;
        }
        itemEntry.invEntry = itemToStore;
        itemEntry.stackSize = 1;
        itemEntry.hbSprite = itemToStore.itemDefinition.itemIcon;

        //addedItem = false;
        itemToStore.gameObject.SetActive(false);
    }

    void TryPickUp()
    {
        bool itsInInv = true;

        //Check to see if the item to be stored was properly submitted to the inventory - Continue if Yes otherwise do nothing
        if (itemEntry.invEntry)
        {
            //Check to see if any items exist in the inventory already - if not, add this item
            if (itemsInInventory.Count == 0)
            {
                addedItem = AddItemToInv();
            }
            //If items exist in inventory
            else
            {
                //Check to see if the item is stackable - Continue if stackable
                if (itemEntry.invEntry.itemDefinition.isStackable)
                {
                    foreach (KeyValuePair<int, InventoryEntry> ie in itemsInInventory)
                    {
                        //Does this item already exist in inventory? - Continue if Yes
                        if (itemEntry.invEntry.itemDefinition == ie.Value.invEntry.itemDefinition)
                        {
                            //Add 1 to stack and destroy the new instance
                            ie.Value.stackSize += 1;
                            AddItemToHotBar(ie.Value);
                            itsInInv = true;
                            Destroy(itemEntry.invEntry.gameObject);
                            break;
                        }
                        //If item does not exist already in inventory then continue here
                        else
                        {
                            itsInInv = false;
                        }
                    }
                }
                //If Item is not stackable then continue here
                else
                {
                    itsInInv = false;

                    //If no space and item is not stackable - say inventory full
                    if (itemsInInventory.Count == InventoryItemCap)
                    {
                        itemEntry.invEntry.gameObject.SetActive(true);
                        Debug.Log("Inventory is Full");
                    }
                }

                //Check if there is space in inventory - if yes, continue here
                if (!itsInInv)
                {
                    addedItem = AddItemToInv();
                }
            }
        }
    }

    private bool AddItemToInv()
    {
        itemsInInventory.Add(idCount, new InventoryEntry(itemEntry.stackSize, Instantiate(itemEntry.invEntry), itemEntry.hbSprite));

        Destroy(itemEntry.invEntry.gameObject);

        FillInventoryDisplay();
        AddItemToHotBar(itemsInInventory[idCount]);

        idCount = IncreaseId();

        #region Reset itemEntry
        itemEntry.invEntry = null;
        itemEntry.stackSize = 0;
        itemEntry.hbSprite = null;
        #endregion

        return true;
    }

    private int IncreaseId()
    {
        int newId = 1;

        for (int itemCount = 1; itemCount <= itemsInInventory.Count; itemCount++)
        {
            if (itemsInInventory.ContainsKey(newId))
            {
                newId += 1;
            }
            else return newId;
        }

        return newId;
    }

    private void AddItemToHotBar(InventoryEntry itemForHotBar)
    {
        int hotBarCounter = 0;
        bool increaseCount = false;
        foreach (Image images in hotBarDisplayHolders)
        {
            hotBarCounter += 1;
            if (itemForHotBar.hotBarSlot == 0)
            {
                if (images.sprite == null)
                {
                    //Add item to open hotbar slot
                    itemForHotBar.hotBarSlot = hotBarCounter;
                    //Change hotbar sprite to show item
                    images.sprite = itemForHotBar.hbSprite;
                    increaseCount = true;
                    break;
                }
            }
            else if (itemForHotBar.invEntry.itemDefinition.isStackable)
            {
                increaseCount = true;
            }
        }
        if (increaseCount)
        {
            hotBarDisplayHolders[itemForHotBar.hotBarSlot - 1].GetComponentInChildren<Text>().text = itemForHotBar.stackSize.ToString();
        }
    }

    private void DisplayInventory()
    {
        inventoryDisplayHolder.SetActive(inventoryDisplayHolder.activeSelf != true);
    }

    private void FillInventoryDisplay()
    {
        int slotCounter = 9;

        foreach (KeyValuePair<int, InventoryEntry> ie in itemsInInventory)
        {
            slotCounter += 1;
            inventoryDisplaySlots[slotCounter].sprite = ie.Value.hbSprite;
            ie.Value.inventorySlot = slotCounter - 9;
        }

        while (slotCounter < 29)
        {
            slotCounter++;
            inventoryDisplaySlots[slotCounter].sprite = null;
        }
    }

    private void TriggerItemUse(int itemToUseId)
    {
        bool triggerItem = false;

        foreach (KeyValuePair<int, InventoryEntry> ie in itemsInInventory)
        {
            if (itemToUseId > 100)
            {
                itemToUseId -= 100;

                if (ie.Value.hotBarSlot == itemToUseId)
                {
                    triggerItem = true;
                }
            }
            else
            {
                if (ie.Value.inventorySlot == itemToUseId)
                {
                    triggerItem = true;
                }
            }

            if (!triggerItem) continue;
            
            if (ie.Value.stackSize == 1)
            {
                if (ie.Value.invEntry.itemDefinition.isStackable)
                {
                    if (ie.Value.hotBarSlot != 0)
                    {
                        hotBarDisplayHolders[ie.Value.hotBarSlot - 1].sprite = null;
                        hotBarDisplayHolders[ie.Value.hotBarSlot - 1].GetComponentInChildren<Text>().text = "0";
                    }

                    ie.Value.invEntry.UseItem(charStats);
                    itemsInInventory.Remove(ie.Key);
                    break;
                }
                else
                {
                    ie.Value.invEntry.UseItem(charStats);
                    if (!ie.Value.invEntry.itemDefinition.indestructable)
                    {
                        itemsInInventory.Remove(ie.Key);
                        break;
                    }
                }
            }
            else
            {
                ie.Value.invEntry.UseItem(charStats);
                ie.Value.stackSize -= 1;
                hotBarDisplayHolders[ie.Value.hotBarSlot - 1].GetComponentInChildren<Text>().text = ie.Value.stackSize.ToString();
                break;
            }
        }
        FillInventoryDisplay();
    }
}
