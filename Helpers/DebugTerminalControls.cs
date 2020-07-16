using RivalAI.Behavior;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRage.Utils;

namespace RivalAI.Helpers {

	public class DebugCockpitControls {

		public string SelectedGridName;
		public long SelectedGridId;
		public bool CurrentGridVulnerable;
		public bool CurrentGridEditable;



	}

	public static class DebugTerminalControls {

		private static Dictionary<IMyTerminalBlock, DebugCockpitControls> CockpitSettings = new Dictionary<IMyTerminalBlock, DebugCockpitControls>();

		public static void DisplayIndustrialCockpitControls(IMyTerminalBlock block, List<IMyTerminalControl> controls) {

			if (block.SlimBlock.BlockDefinition.Id.SubtypeName != "SmallBlockCockpitIndustrial" || !Logger.CurrentDebugTypeList.Contains(DebugTypeEnum.Terminal))
				return;

			//Select Block - Listbox
			var listBox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyCockpit>("RAI-Debug-SelectTargetGrid");
			listBox.Enabled = (b) => { return true; };
			listBox.Visible = (b) => { return true; };
			listBox.VisibleRowsCount = 6;
			listBox.Multiselect = false;
			listBox.ListContent = (b, items, selectedItems) => {

				var settings = GetCockpitControls(b);

				var defaultItem = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("None"), MyStringId.GetOrCompute("None"), (long)0);
				items.Add(defaultItem);

				if (settings.SelectedGridId == 0)
					selectedItems.Add(defaultItem);

				for (int i = BehaviorManager.Behaviors.Count - 1; i >= 0; i--) {

					var behavior = BehaviorManager.Behaviors[i];

					if (!behavior.IsAIReady())
						continue;

					var name = behavior.GridName;
					var id = behavior.GridId;

					if (name == "N/A")
						continue;

					var stringId = MyStringId.GetOrCompute(string.Format("{0} - {1}", name, id));
					var newItem = new MyTerminalControlListBoxItem(stringId, stringId, id);
					items.Add(newItem);

					if (id == settings.SelectedGridId && selectedItems.Count == 0)
						selectedItems.Add(newItem);

				}
			
			};
			listBox.ItemSelected = (b, selected) => {

				if (selected.Count == 0)
					return;

				var settings = GetCockpitControls(b);

				if (settings.SelectedGridId != (long)selected[0].UserData) {

					bool setNew = false;

					for (int i = BehaviorManager.Behaviors.Count - 1; i >= 0; i--) {

						var behavior = BehaviorManager.Behaviors[i];

						if (behavior.GridId == settings.SelectedGridId)
							behavior.SetDebugCockpit(b as IMyCockpit, false);

						if (behavior.GridId == (long)selected[0].UserData) {

							behavior.SetDebugCockpit(b as IMyCockpit, true);
							setNew = true;

						}
						
					}

					if (setNew)
						settings.SelectedGridId = (long)selected[0].UserData;
					else
						settings.SelectedGridId = 0;

					SetCockpitControls(b, settings);

				}

			};
			controls.Add(listBox);

			//Switch - Current Grid Invul
			var invulSwitch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyCockpit>("RAI-Debug-GridInvul");
			invulSwitch.Enabled = (b) => { return true; };
			invulSwitch.Visible = (b) => { return true; };
			invulSwitch.Getter = (b) => {

				var settings = GetCockpitControls(b);
				return !settings.CurrentGridVulnerable;

			};

			invulSwitch.Setter = (b, mode) => {

				var settings = GetCockpitControls(b);
				var grid = b.SlimBlock.CubeGrid as MyCubeGrid;
				grid.DestructibleBlocks = !mode;
				settings.CurrentGridVulnerable = !mode;
				SetCockpitControls(b, settings);

			};
			invulSwitch.Title = MyStringId.GetOrCompute("Grid Invulnerability");
			invulSwitch.OffText = MyStringId.GetOrCompute("Off");
			invulSwitch.OnText = MyStringId.GetOrCompute("On");
			controls.Add(invulSwitch);

			//Switch - Current Grid Uneditable
			var editSwitch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyCockpit>("RAI-Debug-GridEdit");
			editSwitch.Enabled = (b) => { return true; };
			editSwitch.Visible = (b) => { return true; };
			editSwitch.Getter = (b) => {

				var settings = GetCockpitControls(b);
				return !settings.CurrentGridEditable;

			};

			editSwitch.Setter = (b, mode) => {

				var settings = GetCockpitControls(b);
				var grid = b.SlimBlock.CubeGrid as MyCubeGrid;
				grid.Editable = !mode;
				settings.CurrentGridEditable = !mode;
				SetCockpitControls(b, settings);

			};
			editSwitch.Title = MyStringId.GetOrCompute("Grid Editable");
			editSwitch.OffText = MyStringId.GetOrCompute("Off");
			editSwitch.OnText = MyStringId.GetOrCompute("On");
			controls.Add(editSwitch);

		}

		public static DebugCockpitControls GetCockpitControls(IMyTerminalBlock block) {

			DebugCockpitControls controls = null;

			if (!CockpitSettings.TryGetValue(block, out controls)) {

				controls = new DebugCockpitControls();
				var grid = block.SlimBlock.CubeGrid as MyCubeGrid;
				controls.CurrentGridVulnerable = grid.DestructibleBlocks;
				controls.CurrentGridEditable = grid.Editable;
				SetCockpitControls(block, controls);

			}

			return controls;

		}

		public static void SetCockpitControls(IMyTerminalBlock block, DebugCockpitControls controls = null) {

			if (CockpitSettings.ContainsKey(block))
				CockpitSettings[block] = controls;
			else
				CockpitSettings.Add(block, controls);

		}

		public static void RegisterControls(bool enable = true) {

			if (enable) {

				MyAPIGateway.TerminalControls.CustomControlGetter += DisplayIndustrialCockpitControls;
			
			} else {

				MyAPIGateway.TerminalControls.CustomControlGetter -= DisplayIndustrialCockpitControls;

			}
		
		}

	}
}
