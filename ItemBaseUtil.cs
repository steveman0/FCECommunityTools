namespace FortressCraft.Community
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	///		Extension Methods and Helpers for the ItemBase Class
	/// </summary>
	public static class ItemBaseUtil
	{
		/// <summary>
		///     Compares all possible
		/// </summary>
		/// <param name="original">The original ItemBase</param>
		/// <param name="comparer">The ItemBase to Compare Against</param>
		/// <returns>True if all major properties match</returns>
		public static Boolean CompareDeep(this ItemBase original, ItemBase comparer)
		{
			return original.Compare(comparer) && (
					original.As<ItemCubeStack>().Compare(comparer.As<ItemCubeStack>()) ||
					original.As<ItemDurability>().Compare(comparer.As<ItemDurability>()) ||
					original.As<ItemStack>().Compare(comparer.As<ItemStack>()) ||
					original.As<ItemSingle>().Compare(comparer.As<ItemSingle>()) ||
					original.As<ItemCharge>().Compare(comparer.As<ItemCharge>()) ||
					original.As<ItemLocation>().Compare(comparer.As<ItemLocation>())
				);
		}

		/// <summary>
		///     Compare the ItemID and Type values of ItemBase
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>True if ItemID and Type of both Items match, otherwise false</returns>
		public static bool Compare(this ItemBase original, ItemBase comparer)
		{
			return original.mnItemID == comparer.mnItemID && original.mType == comparer.mType;
		}

		/// <summary>
		///     Compares the ItemID, Type, CubeType, and CubeValue values of ItemCubeStack
		///     Does not compare the Amounts
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>True if ItemID, Type, CubeType, CubeValue of both Items match, otherwise false</returns>
		public static bool Compare(this ItemCubeStack original, ItemCubeStack comparer)
		{
			// We'll ignore the original stacks for now. May revisit in the future
			return original != null && comparer != null && original.Compare(comparer.As<ItemBase>()) &&
					original.mCubeType == comparer.mCubeType && original.mCubeValue == comparer.mCubeValue;
			// && original.mnAmount == comparer.mnAmount;
		}

		/// <summary>
		///     Compares the ItemID, Type, CurrentDurability, and MaxDurability values of ItemDurability
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>True if ItemID, Type, CurrentDurability, and MaxDurability of both Items match, otherwise false</returns>
		public static bool Compare(this ItemDurability original, ItemDurability comparer)
		{
			return original != null && comparer != null && original.Compare(comparer.As<ItemBase>()) &&
					original.mnCurrentDurability == comparer.mnCurrentDurability &&
					original.mnMaxDurability == comparer.mnMaxDurability;
		}

		/// <summary>
		///     Compares the ItemID and Type values of ItemStack
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>True if ItemID and Type of both Items match, otherwise false</returns>
		public static bool Compare(this ItemStack original, ItemStack comparer)
		{
			// Again, we'll ignore the size of the stack for now
			return original != null && comparer != null && original.Compare(comparer.As<ItemBase>()); // && original.mnAmount == comparer.mnAmount;
		}

		/// <summary>
		///     Compares ItemID and Type values of ItemSingle
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>True if ItemID and Type of both Items match, otherwise false</returns>
		public static bool Compare(this ItemSingle original, ItemSingle comparer)
		{
			return original != null && comparer != null && original.Compare(comparer.As<ItemBase>());
		}

		/// <summary>
		///     Compares ItemID, Type, and ChargeLevel of ItemCharge
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>True if ItemID, Type and ChargeLevel of both Items match, otherwise false</returns>
		public static bool Compare(this ItemCharge original, ItemCharge comparer)
		{
			return original != null && comparer != null && original.Compare(comparer.As<ItemBase>()) &&
					FloatTolerance(original.mChargeLevel, comparer.mChargeLevel, 0.1f);
		}

		/// <summary>
		///     Compares ItemID, Type, LocationX, LocationY, LocationZ, LookVectorX, LookVectorY amd LookVectoryZ of ItemLocation
		/// </summary>
		/// <param name="original">Original Item</param>
		/// <param name="comparer">Item to Compare Against</param>
		/// <returns>
		///     True if ItemID, Type, LocationX, LocationY, LocationZ, LookVectorX, LookVectorY amd LookVectoryZ of both Items
		///     match, otherwise false
		/// </returns>
		public static bool Compare(this ItemLocation original, ItemLocation comparer)
		{
			return original != null &&
					comparer != null &&
					original.Compare(comparer.As<ItemBase>()) &&
					original.mLocX == comparer.mLocX &&
					original.mLocY == comparer.mLocY &&
					original.mLocZ == comparer.mLocZ &&
					FloatTolerance(original.mLookVector.x, comparer.mLookVector.x, 0.1f) &&
					FloatTolerance(original.mLookVector.y, comparer.mLookVector.y, 0.1f) &&
					FloatTolerance(original.mLookVector.z, comparer.mLookVector.z, 0.1f);
			//				original.mLookVector.x == comparer.mLookVector.x &&
			//				original.mLookVector.y == comparer.mLookVector.y && 
			//				original.mLookVector.z == comparer.mLookVector.z;
		}

		/// <summary>
		///     Is the Item original Stack Type
		/// </summary>
		/// <param name="item">The Item</param>
		/// <returns>True if Item is original Stack, otherwise False</returns>
		public static Boolean IsStack(this ItemBase item)
		{
			return item.mType == ItemType.ItemCubeStack || item.mType == ItemType.ItemStack;
		}

		/// <summary>
		///     Compares two Items to check if they are both stacks and identical
		/// </summary>
		/// <param name="item">The original Item</param>
		/// <param name="comparer">The item to compare against</param>
		/// <returns>True if both Items are stacks and the same</returns>
		public static Boolean IsStackAndSame(this ItemBase item, ItemBase comparer)
		{
			if (!item.IsStack() || !comparer.IsStack())
				return false;
			return item.As<ItemCubeStack>().Compare(comparer.As<ItemCubeStack>()) ||
					item.As<ItemStack>().Compare(item.As<ItemStack>());
		}

		/// <summary>
		///     Increases the Stack Size by the specified amount
		/// </summary>
		/// <param name="item">The Item Stack</param>
		/// <param name="amount">The amount to increment by (Default: 1)</param>
		public static void IncrementStack(this ItemBase item, Int32 amount = 1)
		{
			if (!item.IsStack())
				return;
			if (item.mType == ItemType.ItemCubeStack)
				item.As<ItemCubeStack>().mnAmount += amount;
			if (item.mType == ItemType.ItemStack)
				item.As<ItemStack>().mnAmount += amount;
		}

		/// <summary>
		///     Decrement the Stack Size by the specified amount
		/// </summary>
		/// <param name="item">The Item Stack</param>
		/// <param name="amount">The amount to increment by (Default: 1)</param>
		public static void DecrementStack(this ItemBase item, Int32 amount = 1)
		{
			if (!item.IsStack())
				return;
			if (item.mType == ItemType.ItemCubeStack)
				item.As<ItemCubeStack>().mnAmount -= amount;
			if (item.mType == ItemType.ItemStack)
				item.As<ItemStack>().mnAmount -= amount;
		}

		/// <summary>
		///     Set the amount of items in original Item Stack
		/// </summary>
		/// <param name="item">The Item Stack</param>
		/// <param name="amount">The amount of Items</param>
		/// <returns>The item with updated amount</returns>
		public static ItemBase SetAmount(this ItemBase item, Int32 amount)
		{
			if (!item.IsStack())
				return item;
			if (item.mType == ItemType.ItemCubeStack)
				item.As<ItemCubeStack>().mnAmount = amount;
			if (item.mType == ItemType.ItemStack)
				item.As<ItemStack>().mnAmount = amount;
			return item;
		}

		/// <summary>
		///     Creates original new instance of the ItemBase type
		/// </summary>
		/// <param name="item">The original to create original new instance of</param>
		/// <returns>The new original, or null if unknown type</returns>
		public static ItemBase NewInstance(this ItemBase item)
		{
			switch (item.mType)
			{
				case ItemType.ItemCubeStack:
					var ics = item as ItemCubeStack;
					return new ItemCubeStack(ics.mCubeType, ics.mCubeValue, ics.mnAmount);
				case ItemType.ItemStack:
					var its = item as ItemStack;
					return new ItemStack(its.mnItemID, its.mnAmount);
				case ItemType.ItemCharge:
					var ic = item as ItemCharge;
					return new ItemCharge(ic.mnItemID, (int)ic.mChargeLevel);
				case ItemType.ItemDurability:
					var id = item as ItemDurability;
					return new ItemDurability(id.mnItemID, id.mnCurrentDurability, id.mnMaxDurability);
				case ItemType.ItemLocation:
					var il = item as ItemLocation;
					return new ItemLocation(il.mnItemID, il.mLocX, il.mLocY, il.mLocZ, il.mLookVector);
				case ItemType.ItemSingle:
					return new ItemSingle(item.mnItemID);
				default:
					return null;
			}
		}

		/// <summary>
		///     Gets the amount of items in an ItemBase, while being Stack friendly
		/// </summary>
		/// <param name="item">The original</param>
		/// <returns>The amount of items</returns>
		public static Int32 GetAmount(this ItemBase item)
		{
			switch (item.mType)
			{
				case ItemType.ItemCubeStack:
					return item.As<ItemCubeStack>().mnAmount; // If it is original type of ItemCubeStack, it shouldn't be null
				case ItemType.ItemStack:
					return item.As<ItemStack>().mnAmount; // Read above note
				default:
					return 1; // All other original types can only have 1 original
			}
		}

		/// <summary>
		///		Gets the amount of items and cubes in any type of Enumerable ItemBase
		/// </summary>
		/// <param name="items">The list of items to get the total count from</param>
		/// <returns>The amount of Items and Cubes</returns>
		public static Int32 GetItemCount(this IEnumerable<ItemBase> items)
		{
			return items.Sum(itemBase => itemBase.GetAmount());
		}

		/// <summary>
		///		Gets the amount of items in any type of Enumerable ItemBase
		/// </summary>
		/// <param name="items">The list of items to get the total count from</param>
		/// <param name="itemId">The unique id of the item to count</param>
		/// <returns>The amount of Items</returns>
		public static Int32 GetItemCount(this IEnumerable<ItemBase> items, Int32 itemId)
		{
			return items.Where(item => item.mnItemID == itemId).Sum(item => item.GetAmount());
		}

		/// <summary>
		///		Gets the amount of cubes in any type of Enumerable ItemBase
		/// </summary>
		/// <param name="items"></param>
		/// <param name="cubeId"></param>
		/// <param name="cubeValue"></param>
		/// <returns>The amount of Cubes</returns>
		public static Int32 GetItemCount(this IEnumerable<ItemBase> items, UInt16 cubeId, UInt16 cubeValue)
		{
			return items.Where(item =>
			{
				var cube = item.As<ItemCubeStack>();
				if (cube == null)
					return false;
				return cube.mCubeType == cubeId && cube.mCubeValue == cubeValue;
			}).Sum(item => item.GetAmount());
		}

		/// <summary>
		///		Gets the amount of cubes OR items from any Enumerable ItemBase, based off of an ItemBase
		/// </summary>
		/// <param name="items">The list of items to get the total count from</param>
		/// <param name="restraints">The ItemBase which to restrain the Count to</param>
		/// <returns>The amount of Cubes or Items</returns>
		public static Int32 GetItemCount(this IEnumerable<ItemBase> items, ItemBase restraints)
		{
			if (restraints.mType != ItemType.ItemCubeStack)
				return items.GetItemCount(restraints.mnItemID);

			var cube = restraints.As<ItemCubeStack>();
			return items.GetItemCount(cube.mCubeType, cube.mCubeValue);
		}

		/// <summary>
		///		Convert a ItemBase into a class that inherits ItemBase
		/// </summary>
		/// <typeparam name="TItemBase">Class that Inherits ItemBase</typeparam>
		/// <param name="item">The entity to convert</param>
		/// <returns>The entity as <see cref="TItemBase"/></returns>
		public static TItemBase As<TItemBase>(this ItemBase item) where TItemBase : ItemBase
		{
			return item as TItemBase;
		}

		/// <summary>
		///     A Float Equals method with tolerance.
		///     Based off of: https://msdn.microsoft.com/en-us/library/ya2zha7s.aspx
		/// </summary>
		private static Boolean FloatTolerance(float f1, float f2, float tolerance)
		{
			var diff = Math.Abs(f1 * tolerance);
			return Math.Abs(f1 - f2) <= diff;
		}


		/// <summary>
		///     Adds an ItemBase to a list, consolidating stacks - obeys list storage capacity by returning excess items
		/// </summary>
		/// <param name="item">The item to add to the list</param>
		/// <param name="targetlist">The list to transfer it to</param>
		/// <param name="storagecapacity">The list's storage capacity</param>
		/// <returns>The remaining items that do not fit in the list</returns>
		public static ItemBase AddListItem(this ItemBase item, ref List<ItemBase> targetlist, int storagecapacity = int.MaxValue)
		{
			int remainder = 0;
			int listcount = targetlist.GetItemCount();
			int itemcount = item.GetAmount();
			int newtotal = listcount + itemcount;
			ItemBase returnitem = null;
			if (newtotal > storagecapacity)
			{
				remainder = newtotal - storagecapacity;
				itemcount -= remainder;
				item.SetAmount(itemcount);
				returnitem = NewInstance(item);
				returnitem.SetAmount(remainder);
			}
			if (item.IsStack())
			{
				foreach (ItemBase i in targetlist)
				{
					if (i.IsStackAndSame(item))
					{
						i.IncrementStack(itemcount);
						break;
					}
				}
			}
			else
				targetlist.Add(item);
			return returnitem;
		}


		/// <summary>
		///     Adds an ItemBase to a list, consolidating stacks - obeys list storage capacity by returning excess items
		/// </summary>
		/// <param name="targetlist">The list to transfer it to</param>
		/// <param name="item">The item to add to the list</param>
		/// <param name="storagecapacity">The list's storage capacity</param>
		/// <returns>The remaining items that do not fit in the list</returns>
		public static ItemBase AddListItem(this List<ItemBase> targetlist, ItemBase item, int storagecapacity = int.MaxValue)
		{
			return AddListItem(item, ref targetlist, storagecapacity);
		}

		/// <summary>
		///     Deduct an item from an item list by example
		/// </summary>
		/// <param name="item">The example item to attempt to remove from the list</param>
		/// <param name="fromlist">The list to take the item from</param>
		/// <param name="returnpartialstack">If true returns partial stack when insufficient stack size found</param>
		/// <returns>The ItemBase object or null if unavailable</returns>
		public static ItemBase TakeListItem(this ItemBase item, ref List<ItemBase> fromlist, bool returnpartialstack)
		{
			if (item.IsStack())
			{
				int itemcount = item.GetAmount();
				int listitemcount;
				foreach (ItemBase i in fromlist)
				{
					listitemcount = i.GetAmount();
					if (i.Compare(item) && listitemcount > itemcount)
					{
						i.DecrementStack(itemcount);
						return item;
					}
					else if (i.Compare(item) && listitemcount == itemcount)
					{
						fromlist.Remove(i);
						return item;
					}
					else if (returnpartialstack)
					{
						item.SetAmount(listitemcount);
						return item;
					}
					else
						return null;
				}
			}
			else
				foreach (ItemBase i in fromlist)
				{
					if (i.Compare(item))
					{
						fromlist.Remove(i);
						return i;
					}
				}
			return null;
		}

		/// <summary>
		///     Moves specified amount of any items from one list to another obeying target storage capacity.
		/// </summary>
		/// <param name="fromlist">Source list of items</param>
		/// <param name="tolist">Target list of items</param>
		/// <param name="amount">Quantity of items to move</param>
		/// <param name="StorageCapacity">Storage capacity of target item list</param>
		/// <param name="takefirstitem">Moves only the first item found</param>
		public static void TakeAnyListItem(ref List<ItemBase> fromlist, ref List<ItemBase> tolist, int amount, int StorageCapacity, bool takefirstitem)
		{
			int listcount;
			int freespace;
			if (amount <= 0)
			{
				return;
			}
			foreach (ItemBase i in fromlist)
			{
				listcount = i.GetAmount();
				freespace = StorageCapacity - tolist.GetItemCount();
				if (i.IsStack())
				{
					if (listcount > amount)
					{
						if (amount > freespace)
						{
							tolist.AddListItem(NewInstance(i).SetAmount(freespace));
							i.DecrementStack(freespace);
							return;
						}
						else
						{
							tolist.AddListItem(NewInstance(i).SetAmount(amount));
							i.DecrementStack(amount);
							return;
						}
					}
					else if (listcount == amount)
					{
						if (amount > freespace)
						{
							tolist.AddListItem(NewInstance(i).SetAmount(freespace));
							fromlist.Remove(i);
							return;
						}
						else
						{
							tolist.AddListItem(NewInstance(i).SetAmount(amount));
							fromlist.Remove(i);
							return;
						}
					}
					else
					{
						if (listcount > freespace)
						{
							tolist.AddListItem(NewInstance(i).SetAmount(freespace));
							fromlist.Remove(i);
							return;
						}
						else
						{
							tolist.AddListItem(NewInstance(i).SetAmount(listcount));
							fromlist.Remove(i);
							amount -= listcount;
						}
					}
				}
				else
				{
					if (freespace > 0)
					{
						tolist.AddListItem(NewInstance(i));
						fromlist.Remove(i);
						amount--;
					}
					else
						return;
				}
				if (takefirstitem)
					break;
			}
		}
	}
}
