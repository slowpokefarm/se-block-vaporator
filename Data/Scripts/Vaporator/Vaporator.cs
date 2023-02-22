using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using System.Diagnostics;

namespace Slowpokefarm.Vaporator {
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_Refinery), true, "LargeBlockVaporator")]
	public class Vaporator : MyGameLogicComponent {

		private static Config configuration;
		private static Config Configuration {
			get {
				if (configuration == null) {
					configuration = ConfigAccess.ReadFileFromWorldStorage<Config>("MoistureVaporator.cfg", typeof(Config)) ?? new Config();
					ConfigAccess.WriteFileToWorldStorage<Config>("MoistureVaporator.cfg", typeof(Config), configuration);
				}
				return configuration;
			}
		}

		private IMyInventory inventory;
		public IMyInventory Inventory {
			get {
				if (inventory == null) {
					inventory = (Entity as IMyTerminalBlock).GetInventory(1) as IMyInventory;
				}
				return inventory;
			}
		}

		private MyResourceSinkComponent sink;
		public MyResourceSinkComponent Sink {
			get {
				if (sink == null) {
					sink = Entity.Components.Get<MyResourceSinkComponent>();
				}
				return sink;
			}
		}

		private MyObjectBuilder_EntityBase builder;
		private IMyRefinery vaporatorInstance;
		private bool checkedAirDensity = false;
		private float airDensity = 0f;
		private bool isStationary = false;

		IMyTerminalBlock terminalBlock;

		public override void Init(MyObjectBuilder_EntityBase objectBuilder) {
			vaporatorInstance = Entity as IMyRefinery;

			builder = objectBuilder;

			terminalBlock = Entity as IMyTerminalBlock;
			terminalBlock.AppendingCustomInfo += appendCustomInfo;

			isStationary = vaporatorInstance.CubeGrid.IsStatic;

			Entity.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
		}

		public override void UpdateBeforeSimulation100() {
			try {
				if (vaporatorInstance.Enabled) {
					if (inValidSituation()) {
						float poweruse = getPowerUsage();
						Sink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, poweruse);
						Inventory.AddItems(getProducedIceAmount(), new MyObjectBuilder_Ore() { SubtypeName = "Ice" });
					}

					terminalBlock.RefreshCustomInfo();
					base.UpdateBeforeSimulation100();
				}
			} catch (Exception e) {
				MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
				MyLog.Default.WriteLine(e);
			}
		}

		private bool inValidSituation() {
			this.isStationary = vaporatorInstance.CubeGrid.IsStatic;
			if (isStationary) {
				if (!checkedAirDensity) {
					checkedAirDensity = true;
					this.airDensity = getAirDensityAtMyLocation();
				}
				if (this.airDensity >= Configuration.AirDensityMin) {
					return true;
				}
			} else {
				this.checkedAirDensity = false;
				this.airDensity = 0.0f;
			}

			this.vaporatorInstance.Enabled = false;
			return false;
		}

		public void appendCustomInfo(IMyTerminalBlock block, StringBuilder info) {
			info.Clear();
			info.AppendFormat("\n");
			if (this.isStationary) {
				if (this.airDensity >= Configuration.AirDensityMin) {
					info.AppendLine("Atmosphere density: " + this.airDensity.ToString("N") + " p");
				} else {
					info.AppendLine("Insufficient atmosphere density");
				}
			} else {
				info.AppendLine("Block is not on a stationary grid!");
			}
		}

		public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) {
			return builder;
		}
		private VRage.MyFixedPoint getProducedIceAmount() {
			return (VRage.MyFixedPoint)(Configuration.IceAmountBase * (this.airDensity * vaporatorInstance.UpgradeValues["Effectiveness"]) * (1 + vaporatorInstance.UpgradeValues["Productivity"]));
		}

		private float getPowerUsage() {
			return Configuration.PowerUsageBase * (1f + vaporatorInstance.UpgradeValues["Productivity"]) * (1f / vaporatorInstance.UpgradeValues["PowerEfficiency"]);
		}

		private bool isWithinAthmosphereOf(MyPlanet planet) {
			return Vector3D.DistanceSquared(vaporatorInstance.GetPosition(), planet.WorldMatrix.Translation) < (planet.AtmosphereRadius * planet.AtmosphereRadius);
		}

		private float getAirDensityAtMyLocation() {
			foreach (MyPlanet planet in Util.getAllBreathablePlanets()) {
				if (isWithinAthmosphereOf(planet)) {
					return planet.GetAirDensity(this.vaporatorInstance.GetPosition());
				}
			}
			return 0.0f;
		}
	}
}