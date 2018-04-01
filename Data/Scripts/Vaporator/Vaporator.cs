using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.GameSystems.Conveyors;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;


namespace Slowpokefarm.Vaporator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), true, "LargeBlockVaporator")]
    public class Vaporator : MyGameLogicComponent
    {
        // Builder is nessassary for GetObjectBuilder method as far as I know.
        private MyObjectBuilder_EntityBase builder;
		private Sandbox.ModAPI.IMyOxygenGenerator m_vaporator;
		private IMyCubeBlock m_parent;
        private float w_density;

        Sandbox.ModAPI.IMyTerminalBlock terminalBlock;
        Vape_AtmosphereDetector atmoDet = new Vape_AtmosphereDetector();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
			m_vaporator = Entity as Sandbox.ModAPI.IMyOxygenGenerator;
            m_parent = Entity as IMyCubeBlock;

            m_vaporator.UpgradeValues.Add("Productivity", 0f);
            m_vaporator.UpgradeValues.Add("Effectiveness", 1f);
            m_vaporator.UpgradeValues.Add("PowerEfficiency", 1f);
			
            builder = objectBuilder;

            // MUST set the object to update atleast ONCE because you can not modify the inventory inside Init method!!!
            Entity.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;

            terminalBlock = Entity as Sandbox.ModAPI.IMyTerminalBlock;
            terminalBlock.AppendingCustomInfo += appendCustomInfo;
        }

        public override void UpdateBeforeSimulation100()
        {
            w_density = atmoDet.AtmosphereDetectionVaporator (this.Entity);
            IMyInventory inventory = ((Sandbox.ModAPI.IMyTerminalBlock)Entity).GetInventory(0) as IMyInventory;
			
			//check to see if the block is on
			if (m_vaporator.Enabled && m_vaporator.IsWorking && m_vaporator.IsFunctional)
			{                        
				m_vaporator.PowerConsumptionMultiplier =  10 * (1f + m_vaporator.UpgradeValues["Productivity"]) * (1f / m_vaporator.UpgradeValues["PowerEfficiency"]);
			
				VRage.MyFixedPoint amount = (VRage.MyFixedPoint)(0.04 * (w_density * m_vaporator.UpgradeValues["Effectiveness"]) * (1 + m_vaporator.UpgradeValues["Productivity"]));
                inventory.AddItems(amount, new MyObjectBuilder_Ore() { SubtypeName = "Ice" });
			}

				

            terminalBlock.RefreshCustomInfo();

            base.UpdateBeforeSimulation100();
        }

        //Info handler
        public void appendCustomInfo(Sandbox.ModAPI.IMyTerminalBlock block, StringBuilder info)
        {
            info.Clear ();
			
            info.Append("Current Required Input: ");
            info.Append((m_vaporator.Enabled && m_vaporator.IsWorking && m_vaporator.IsFunctional) ? (100f * (1f + m_vaporator.UpgradeValues["Productivity"]) * (1f / m_vaporator.UpgradeValues["PowerEfficiency"])).ToString("N") : 0f.ToString("N"));
            info.Append(" kW \n");
            info.AppendFormat("\n\n");
            info.Append("Productivity: ");
            info.Append(((m_vaporator.UpgradeValues["Productivity"] + 1f) * 100f).ToString("F0"));
            info.Append("%\n");
            info.Append("Effectiveness: ");
            info.Append(((m_vaporator.UpgradeValues["Effectiveness"]) * 100f).ToString("F0"));
            info.Append("%\n");
            info.Append("Power Efficiency: ");
            info.Append(((m_vaporator.UpgradeValues["PowerEfficiency"]) * 100f).ToString("F0"));
            info.Append("%\n");
			
            info.AppendLine ("Atmosphere density: " + w_density.ToString("N") + " p");
            // info.AppendLine ("OwnerId: " + ((MyCubeBlock)Entity).OwnerId );
        }


        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return builder;
        }
    }
}