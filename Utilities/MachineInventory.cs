using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FortressCraft.Community.Utilities
{
    /// <summary>
    ///     Generic machine inventory class with list inventory support
    /// </summary>
    /// <remarks>Origianl code by steveman0</remarks>
    public class MachineInventory
    {
        /// <summary>
        ///     For associating the machine owner of the inventory
        /// </summary>
        public MachineEntity Machine;

        /// <summary>
        ///     The total item capacity of the storage
        /// </summary>
        public int StorageCapacity;

        /// <summary>
        ///     The list of items in the inventory
        /// </summary>
        public List<ItemBase> Inventory;

        /// <summary>
        ///     Generic machine inventory class with list inventory support
        /// </summary>
        /// <param name="machineentity">For associating the owner machine</param>
        /// <param name="storagecapacity">The storage capacity of the inventory</param>
        public MachineInventory(MachineEntity machineentity, int storagecapacity)
        {
            this.Machine = machineentity;
            this.StorageCapacity = storagecapacity;
            this.Inventory = new List<ItemBase>();
        }

        /// <summary>
        ///     Add a single item type to the inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="amount">Amount of items added if given a stack</param>
        /// <returns>Returns the remainder that doesn't fit or null if successful</returns>
        public ItemBase AddItem(ItemBase item, int amount = 1)
        {
            return ItemBaseUtil.AddListItem(item, ref this.Inventory, true, this.StorageCapacity);
        }

        /// <summary>
        ///     Add items from a source inventory or item list
        /// </summary>
        /// <param name="items">The source inventory or list of items</param>
        /// <param name="amount">The number of items to transfer</param>
        public void AddItem(ref List<ItemBase> items, int amount = 1)
        {
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, amount, this.StorageCapacity, false);
        }

        /// <summary>
        ///     Transfers items to machine inventory if they are on the provided whitelist
        /// </summary>
        /// <param name="items">Source inventory or list of items</param>
        /// <param name="whitelist">List of items types allowed in the transfer</param>
        /// <param name="amount">Number of items to add</param>
        public void AddWhiteList(ref List<ItemBase> items, IEnumerable<ItemBase> whitelist, int amount = 1)
        {
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, amount, this.StorageCapacity, false, whitelist, true);
        }

        /// <summary>
        ///     Transfers items to machine inventory if they are on the provided whitelist
        /// </summary>
        /// <param name="items">Source inventory or list of items</param>
        /// <param name="whitelist">Item type allowed in the transfer</param>
        /// <param name="amount">Number of items to add</param>
        public void AddWhiteList(ref List<ItemBase> items, ItemBase whitelist, int amount = 1)
        {
            ItemBase[] WhiteList = new[] { whitelist };
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, amount, this.StorageCapacity, false, WhiteList, true);
        }

        /// <summary>
        ///     Transfers items to machine inventory if they are not on the provided blacklist
        /// </summary>
        /// <param name="items">Source inventory or list of items</param>
        /// <param name="blacklist">List of items forbidden from transfer</param>
        /// <param name="amount">Number of items to add</param>
        public void AddBlackList(ref List<ItemBase> items, IEnumerable<ItemBase> blacklist, int amount = 1)
        {
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, amount, this.StorageCapacity, false, blacklist, false);
        }

        

        /// <summary>
        ///     Transfers items to machine inventory if they are not on the provided blacklist
        /// </summary>
        /// <param name="items">Source inventory or list of items</param>
        /// <param name="blacklist">Item forbidden from transfer</param>
        /// <param name="amount">Number of items to add</param>
        public void AddBlackList(ref List<ItemBase> items, ItemBase blacklist, int amount = 1)
        {
            ItemBase[] BlackList = new[] { blacklist };
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, amount, this.StorageCapacity, false, BlackList, false);
        }

        /// <summary>
        ///     Fills the inventory to capacity with source items
        /// </summary>
        /// <param name="items">Source items to fill the inventory</param>
        public void Fill(ref List<ItemBase> items)
        {
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, this.SpareCapacity(), this.StorageCapacity);
        }

        /// <summary>
        ///     Fills the inventory to capacity with source items
        /// </summary>
        /// <param name="items">Source items to fill the inventory</param>
        /// <param name="whitelist">Item type allowed in the transfer</param>
        public void FillWhiteList(ref List<ItemBase> items, IEnumerable<ItemBase> whitelist)
        {
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, this.SpareCapacity(), this.StorageCapacity, false, whitelist, true);
        }

        /// <summary>
        ///     Fills the inventory to capacity with source items
        /// </summary>
        /// <param name="items">Source items to fill the inventory</param>
        /// <param name="whitelist">Item type allowed in the transfer</param>
        public void FillWhiteList(ref List<ItemBase> items, ItemBase whitelist)
        {
            ItemBase[] WhiteList = new[] { whitelist };
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, this.SpareCapacity(), this.StorageCapacity, false, WhiteList, true);
        }

        /// <summary>
        ///     Fills the inventory to capacity with source items
        /// </summary>
        /// <param name="items">Source items to fill the inventory</param>
        /// <param name="blacklist">Item forbidden from transfer</param>
        public void FillBlackList(ref List<ItemBase> items, IEnumerable<ItemBase> blacklist)
        {
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, this.SpareCapacity(), this.StorageCapacity, false, blacklist, false);
        }

        /// <summary>
        ///     Fills the inventory to capacity with source items
        /// </summary>
        /// <param name="items">Source items to fill the inventory</param>
        /// <param name="blacklist">Item forbidden from transfer</param>
        public void FillBlackList(ref List<ItemBase> items, ItemBase blacklist)
        {
            ItemBase[] BlackList = new[] { blacklist };
            ItemBaseUtil.MoveItems(ref items, ref this.Inventory, this.SpareCapacity(), this.StorageCapacity, false, BlackList, false);
        }

        /// <summary>
        ///     Empty the inventory of items
        /// </summary>
        /// <param name="items">Target inventory or list</param>
        /// <param name="amount">Maximum number of items to take</param>
        public void Empty(ref List<ItemBase> items, int amount)
        {
            ItemBaseUtil.MoveItems(ref this.Inventory, ref items, amount);
        }

        /// <summary>
        ///     Return item from inventory by example (obeys stack size)
        /// </summary>
        /// <param name="item">Example item to find in inventory</param>
        /// <returns>Returns the item or null if unavailable or insufficient stack size</returns>
        public ItemBase RemoveItem(ItemBase item)
        {
            return ItemBaseUtil.RemoveListItem(item, ref this.Inventory, false);
        }

        /// <summary>
        ///     Return item from inventory by example including partial item stack
        /// </summary>
        /// <param name="item">Example item to find in inventory</param>
        /// <returns>Returns the item or partial stack (null if item not found)</returns>
        public ItemBase RemovePartialStack(ItemBase item)
        {
            return ItemBaseUtil.RemoveListItem(item, ref this.Inventory, true);
        }

        /// <summary>
        ///     Remove any single item type from the inventory
        /// </summary>
        /// <param name="amount">Amount to remove (for stacks)</param>
        /// <returns>The ItemBase removed from inventory</returns>
        public ItemBase RemoveAnySingle(int amount = 1)
        {
            List<ItemBase> output = new List<ItemBase>();
            ItemBaseUtil.MoveItems(ref this.Inventory, ref output, amount, amount, true);
            if (output.Count == 0)
                return null;
            return output[0];
        }

        /// <summary>
        ///     Remove items from inventory if items are on the whitelist
        /// </summary>
        /// <param name="items">The target inventory or list to store the items</param>
        /// <param name="whitelist">The list of items allowed to transfer</param>
        /// <param name="storagecapacity">Storage capacity of target inventory</param>
        /// <param name="amount">Amount of items to move in this transfer</param>
        public void RemoveWhiteList(ref List<ItemBase> items, IEnumerable<ItemBase> whitelist, int storagecapacity = int.MaxValue, int amount = 1)
        {
            ItemBaseUtil.MoveItems(ref this.Inventory, ref items, amount, storagecapacity, false, whitelist, true);
        }

        /// <summary>
        ///     Remove items from inventory if items are on the whitelist
        /// </summary>
        /// <param name="items">The target inventory or list to store the items</param>
        /// <param name="whitelist">Item allowed to transfer</param>
        /// <param name="storagecapacity">Storage capacity of target inventory</param>
        /// <param name="amount">Amount of items to move in this transfer</param>
        public void RemoveWhiteList(ref List<ItemBase> items, ItemBase whitelist, int storagecapacity = int.MaxValue, int amount = 1)
        {
            ItemBase[] WhiteList = new [] { whitelist };
            ItemBaseUtil.MoveItems(ref this.Inventory, ref items, amount, storagecapacity, false, WhiteList, true);
        }

        /// <summary>
        ///     Remove items from inventory if items are not on the blacklist
        /// </summary>
        /// <param name="items">The target inventory or list to store the items</param>
        /// <param name="blacklist">The list of items forbidden from transfer</param>
        /// <param name="storagecapacity">Storage capacity of target inventory</param>
        /// <param name="amount">Amount of items to move in this transfer</param>
        public void RemoveBlackList(ref List<ItemBase> items, IEnumerable<ItemBase> blacklist, int storagecapacity = int.MaxValue, int amount = 1)
        {
            ItemBaseUtil.MoveItems(ref this.Inventory, ref items, amount, storagecapacity, false, blacklist, false);
        }

        /// <summary>
        ///     Remove items from inventory if items are not on the blacklist
        /// </summary>
        /// <param name="items">The target inventory or list to store the items</param>
        /// <param name="blacklist">Item forbidden from transfer</param>
        /// <param name="storagecapacity">Storage capacity of target inventory</param>
        /// <param name="amount">Amount of items to move in this transfer</param>
        public void RemoveBlackList(ref List<ItemBase> items, ItemBase blacklist, int storagecapacity = int.MaxValue, int amount = 1)
        {
            ItemBase[] BlackList = new[] { blacklist };
            ItemBaseUtil.MoveItems(ref this.Inventory, ref items, amount, storagecapacity, false, BlackList, false);
        }

        /// <summary>
        ///     Returns the spare capacity of the inventory
        /// </summary>
        /// <returns>The spare capacity of the inventory</returns>
        public int SpareCapacity()
        {
            return this.StorageCapacity - this.Inventory.GetItemCount();
        }

        /// <summary>
        ///     Returns the current number of items in the inventory
        /// </summary>
        /// <returns>Current number of items in inventory</returns>
        public int ItemCount()
        {
            return this.Inventory.GetItemCount();
        }

        /// <summary>
        ///     Helper logic for checking if the inventory has space
        /// </summary>
        /// <returns></returns>
        public bool HasSpareCapcity()
        {
            return this.SpareCapacity() > 0;
        }

        /// <summary>
        ///     Helper logic for checking if the inventory is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return this.ItemCount() == 0;
        }

        /// <summary>
        ///     Helper logic for checking if the inventory is full
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return this.ItemCount() >= this.StorageCapacity;
        }

        /// <summary>
        ///     Item drop code for emptying the inventory on the ground on machine delete
        /// </summary>
        public void DropOnDelete()
        {
            if (!WorldScript.mbIsServer)
                return;
            System.Random random = new System.Random();
            for (int index = 0; index < this.Inventory.Count; ++index)
            {
                if (this.Inventory[index] != null)
                {
                    Vector3 velocity = new Vector3((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f);
                    ItemManager.instance.DropItem(this.Inventory[index], this.Machine.mnX, this.Machine.mnY, this.Machine.mnZ, velocity);
                }
            }
            this.Inventory = null;
        }

        /// <summary>
        ///     Generic serialization function for writing the inventory to disk
        /// </summary>
        /// <param name="writer"></param>
        public void WriteInventory(BinaryWriter writer)
        {
            int listcount = this.Inventory.Count;
            int version = 0;

            writer.Write(version);
            writer.Write(listcount);
            for (int index = 0; index < listcount; ++index)
            {
                ItemFile.SerialiseItem(this.Inventory[index], writer);
            }
        }

        /// <summary>
        ///     Generic serialization function for reading the inventory from disk
        /// </summary>
        /// <param name="reader"></param>
        public void ReadInventory(BinaryReader reader)
        {
            int listcount;
            int version = reader.ReadInt32();

            switch (version)
            {
                case 0:
                    listcount = reader.ReadInt32();
                    for (int index = 0; index < listcount; ++index)
                    {
                        ItemBase item = ItemFile.DeserialiseItem(reader);
                        if (item != null)
                            this.Inventory.Add(item);
                        else
                            Debug.LogError("Machine inventory tried to read in a null item!  Corrupt save?");
                    }
                    break;
                default:
                    Debug.LogError("Attempted to read Machine inventory version that does not exist!");
                    break;
            }
        }
    }
}
