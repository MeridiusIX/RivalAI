using Sandbox.Definitions;
using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.ObjectBuilders;

namespace RivalAI.Helpers {
	public static class InventoryHelper {

		private static ListReader<MyPhysicalItemDefinition> _allItems = new List<MyPhysicalItemDefinition>();
		private static Dictionary<MyDefinitionId, MyPhysicalItemDefinition> _cachedItemDefinitions = new Dictionary<MyDefinitionId, MyPhysicalItemDefinition>();

		public static void AddItemsToInventory(MyInventory inventory, MyDefinitionId itemId, float amount = -1) {

			var itemDef = GetItemDefinition(itemId);

			if (itemDef == null)
				return;

			float freeSpace = (float)(inventory.MaxVolume - inventory.CurrentVolume);
			var amountToAdd = Math.Floor(freeSpace / itemDef.Volume);

			if (amountToAdd > amount && amount > -1) {

				var adjustedAmt = amountToAdd - amount;
				amountToAdd = adjustedAmt;

			}

			if (amountToAdd > 0 && inventory.CanItemsBeAdded((MyFixedPoint)amountToAdd, itemId) == true) {

				inventory.AddItems((MyFixedPoint)amountToAdd, MyObjectBuilderSerializer.CreateNewObject(itemId));

			}
			
		}

		public static MyPhysicalItemDefinition GetItemDefinition(MyDefinitionId itemId) {

			if (_allItems.Count == 0) {

				_allItems = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
			
			}

			MyPhysicalItemDefinition item = null;

			if (_cachedItemDefinitions.TryGetValue(itemId, out item))
				return item;

			foreach (var itemdef in _allItems) {

				if (itemdef.Id == itemId) {

					item = itemdef;
					_cachedItemDefinitions.Add(itemId, item);
					break;

				}
			
			}

			return item;
		
		}

	}
}
