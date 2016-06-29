using UnityEngine;
using System.Collections.Generic;
using FortressCraft.Community.Utilities;

public class FreightCartWindow : BaseMachineWindow
{
    public const string InterfaceName = "FreightCartStation";

    private bool dirty;
    private bool ChooseLowStock = false;

    public override void SpawnWindow(SegmentEntity targetEntity)
    {
        FreightCartStation station = targetEntity as FreightCartStation;
        Debug.Log("Before close?");
        //Catch for when the window is called on an inappropriate machine
        if (station == null)
        {
            GenericMachinePanelScript.instance.Hide();
            UIManager.RemoveUIRules("Machine");
            return;
        }
        //station.UIdelay = 0;
        //station.UILock = true;
        UIUtil.UIdelay = 0;
        UIUtil.UILock = true;
        Debug.Log("After close?");

        this.manager.SetTitle("Freight Cart Station - Register Freight");

        this.manager.AddButton("switchlowstock", "Edit Low Stock", 25, 0);
        this.manager.AddButton("switchhighstock", "Edit High Stock", 175, 0);

        int spacing = 175;
        int count = 0;
        int offset = 50;
        if (station.massStorageCrate != null)
            count = FreightCartManager.instance.GetFreightEntries(station.massStorageCrate).Count;
        for (int n = 0; n < count + 1; n++)
        {
            int suffix = n;
            if (n == count)
                suffix = -1;
            this.manager.AddIcon("registry" + suffix, "empty", Color.white, 0, offset + (spacing * n));
            this.manager.AddBigLabel("registrytitle" + n, "Add New Freight", Color.white, 60, offset + (spacing * n));
            if (suffix != -1)
            {
                this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "lowstocktitle" + n, "Low Stock Limit", this.ChooseLowStock == true ? Color.white : Color.gray, false, 0, offset + (spacing * n + 40));
                this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "highstocktitle" + n, "High Stock Limit", this.ChooseLowStock == false ? Color.white : Color.gray, false, 150, offset + (spacing * n + 40));
                this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "lowstock" + n, "Low Stock Limit", this.ChooseLowStock == true ? Color.white : Color.gray, false, 0, offset + (spacing * n + 60));
                this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "highstock" + n, "High Stock Limit", this.ChooseLowStock == false ? Color.white : Color.gray, false, 150, offset + (spacing * n + 60));
                this.manager.AddButton("decreasestock" + n, "Decrease Stock", 25, offset + (spacing * n + 100));
                this.manager.AddButton("increasestock" + n, "Increase Stock", 175, offset + (spacing * n + 100));
            }
        }
        this.dirty = true;
    }

    public override void UpdateMachine(SegmentEntity targetEntity)
    {
        FreightCartStation station = targetEntity as FreightCartStation;
        //Catch for when the window is called on an inappropriate machine
        if (station == null)
        {
            GenericMachinePanelScript.instance.Hide();
            UIManager.RemoveUIRules("Machine");
            return;
        }
        //station.UIdelay = 0;
        UIUtil.UIdelay = 0;
        List<FreightRegistry> registries = new List<FreightRegistry>();
        if (station.massStorageCrate != null)
            registries = FreightCartManager.instance.GetFreightEntries(station.massStorageCrate);
        else
            return;

        for (int index = 0; index < registries.Count; index++)
        {
            ItemBase item = registries[index].FreightItem;
            int lowstock = registries[index].LowStock;
            int highstock = registries[index].HighStock;

            string itemname = ItemManager.GetItemName(item);
            string iconname = ItemManager.GetItemIcon(item);

            this.manager.UpdateIcon("registry" + index, iconname, Color.white);
            this.manager.UpdateLabel("registrytitle" + index, itemname, Color.white);
            this.manager.UpdateLabel("lowstock" + index, registries[index].LowStock.ToString(), this.ChooseLowStock == true ? Color.white : Color.gray);
            this.manager.UpdateLabel("highstock" + index, registries[index].HighStock.ToString(), this.ChooseLowStock == false ? Color.white : Color.gray);
            this.manager.UpdateLabel("lowstocktitle" + index, "Low Stock Limit", this.ChooseLowStock == true ? Color.white : Color.gray);
            this.manager.UpdateLabel("highstocktitle" + index, "High Stock Limit", this.ChooseLowStock == false ? Color.white : Color.gray);
        }
        if (this.dirty == true)
        {
            this.UpdateState(station);
            this.dirty = false;
        }
    }

    private void UpdateState(FreightCartStation machine)
    {
        return;
    }

    public override bool ButtonClicked(string name, SegmentEntity targetEntity)
    {
        FreightCartStation station = targetEntity as FreightCartStation;

        if (name.Contains("registry")) // drag drop to a slot
        {
            int slotNum = -1;
            int.TryParse(name.Replace("registry", ""), out slotNum); //Get slot name as number
            List<FreightRegistry> registries = FreightCartManager.instance.GetFreightEntries(station.massStorageCrate);

            if (slotNum > -1) // valid slot
            {
                //clear registry
                FreightCartManager.instance.RemoveRegistry(station.massStorageCrate, registries[slotNum].FreightItem);
                this.manager.RedrawWindow();
            }

            return true;
        }
        else if (name.Contains("switchlowstock"))
        {
            this.ChooseLowStock = true;
            this.manager.RedrawWindow();
        }
        else if (name.Contains("switchhighstock"))
        {
            this.ChooseLowStock = false;
            this.manager.RedrawWindow();
        }
        else if (name.Contains("decreasestock"))
        {
            int slotNum = -1;
            int.TryParse(name.Replace("decreasestock", ""), out slotNum); //Get slot name as number
            List<FreightRegistry> registries = FreightCartManager.instance.GetFreightEntries(station.massStorageCrate);

            if (slotNum > -1) // valid slot
            {
                int amount = 100;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    amount = 10;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    amount = 1;
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    amount = 1000;

                int stock;
                if (this.ChooseLowStock)
                {
                    stock = registries[slotNum].LowStock - amount;
                    if (stock < 0)
                        stock = 0;
                    FreightCartManager.instance.UpdateRegistry(station.massStorageCrate, registries[slotNum].FreightItem, stock, registries[slotNum].HighStock);
                    this.manager.UpdateLabel("lowstock" + slotNum, stock.ToString(), Color.white);
                }
                else
                {
                    stock = registries[slotNum].HighStock - amount;
                    if (stock < 0)
                        stock = 0;
                    FreightCartManager.instance.UpdateRegistry(station.massStorageCrate, registries[slotNum].FreightItem, registries[slotNum].LowStock, stock);
                    this.manager.UpdateLabel("highstock" + slotNum, stock.ToString(), Color.white);
                }
            }
        }
        else if (name.Contains("increasestock"))
        {
            int slotNum = -1;
            int.TryParse(name.Replace("increasestock", ""), out slotNum); //Get slot name as number
            List<FreightRegistry> registries = FreightCartManager.instance.GetFreightEntries(station.massStorageCrate);

            if (slotNum > -1) // valid slot
            {
                int amount = 100;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    amount = 10;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    amount = 1;
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    amount = 1000;

                int stock;
                if (this.ChooseLowStock)
                {
                    stock = registries[slotNum].LowStock + amount;
                    FreightCartManager.instance.UpdateRegistry(station.massStorageCrate, registries[slotNum].FreightItem, stock, registries[slotNum].HighStock);
                    this.manager.UpdateLabel("lowstock" + slotNum, stock.ToString(), Color.white);
                }
                else
                {
                    stock = registries[slotNum].HighStock + amount;
                    FreightCartManager.instance.UpdateRegistry(station.massStorageCrate, registries[slotNum].FreightItem, registries[slotNum].LowStock, stock);
                    this.manager.UpdateLabel("highstock" + slotNum, stock.ToString(), Color.white);
                }
            }
        }

        return false;
    }

    public override ItemBase GetDragItem(string name, SegmentEntity targetEntity)
    {
        FreightCartStation machine = targetEntity as FreightCartStation;

        return null;
    }

    public override bool RemoveItem(string name, ItemBase originalItem, ItemBase swapitem, SegmentEntity targetEntity)
    {
        FreightCartStation machine = targetEntity as FreightCartStation;

        return false;
    }

    public override void HandleItemDrag(string name, ItemBase draggedItem, DragAndDropManager.DragRemoveItem dragDelegate, SegmentEntity targetEntity)
    {
        FreightCartStation station = targetEntity as FreightCartStation;
        if (station.massStorageCrate == null)
            return;

        if (name.Contains("registry")) // drag drop to a slot
        {
            int slotNum = -1;
            int.TryParse(name.Replace("registry", ""), out slotNum); //Get slot name as number

            if (slotNum == -1) // valid slot
            {
                if (this.manager.mWindowLookup[name + "_icon"].GetComponent<UISprite>().spriteName == "empty")
                {
                    FreightCartManager.instance.AddRegistry(station.massStorageCrate, draggedItem, 0, 0);
                    this.manager.RedrawWindow();
                }
                //machine.filterCubeId[slotNum] = cubeId;
                //machine.filterCubeValue[slotNum] = cubeValue;
                //machine.filterItem[slotNum] = draggedItem.mnItemID;
                //UnityEngine.Debug.LogError("SET: " + cubeId + " : " + cubeValue + " : " + draggedItem.mnItemID + " : " + draggedItem.mType);

                //UnityEngine.Debug.LogError("item set");
            }
        }

        return;
    }

    public static NetworkInterfaceResponse HandleNetworkCommand(Player player, NetworkInterfaceCommand nic)
    {
        FreightCartStation station = nic.target as FreightCartStation;

        string command = nic.command;
        if (command != null)
        {
            if (command == "test")
            {
                // do whatever
            }
        }

        return new NetworkInterfaceResponse
        {
            entity = station,
            inventory = player.mInventory
        };
    }
}

