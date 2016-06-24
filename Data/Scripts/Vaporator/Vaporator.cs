using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Drill), "LargeBlockVaporator", "SmallBlockVaporator")]
    public class Vaporator : MyGameLogicComponent
    {
        // Builder is nessassary for GetObjectBuilder method as far as I know.
        private MyObjectBuilder_EntityBase builder;
		private Sandbox.ModAPI.IMyShipDrill m_drill;
		private IMyCubeBlock m_parent;
		private int myupdatecheck;
		private float w_density;

		Sandbox.ModAPI.IMyTerminalBlock terminalBlock;
		AtmosphereDetector atmoDet = new AtmosphereDetector();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_drill = Entity as Sandbox.ModAPI.IMyShipDrill;
			m_parent = Entity as IMyCubeBlock;
			myupdatecheck = 0;
			// MUST set the object to update atleast ONCE because you can not modify the inventory inside Init method!!!
            Entity.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
            builder = objectBuilder;

            terminalBlock = Entity as Sandbox.ModAPI.IMyTerminalBlock;
			terminalBlock.AppendingCustomInfo += appendCustomInfo;
        }

        public override void UpdateBeforeSimulation100()
        {
			w_density = atmoDet.AtmosphereDetectionVaporator (this.Entity);

			// Get the first Inventory, drills only have one
			IMyInventory inventory = ((Sandbox.ModAPI.IMyTerminalBlock)Entity).GetInventory(0) as IMyInventory;
			//check to make sure we just affect the blocks we want to
			if (m_drill.BlockDefinition.SubtypeName.Contains("SmallBlockVaporator"))
			{
				//I really want to pull this value from the cubeblocks.sbc but don't know how.
				m_drill.PowerConsumptionMultiplier = 50;
				//check to see if the block is on
				if (m_drill.Enabled)
				{
				VRage.MyFixedPoint amount = (VRage.MyFixedPoint)(0.01 * w_density);
				//add the items
				//don't need to check if there's room, AddItems does that for us
				inventory.AddItems(amount, new MyObjectBuilder_Ore() { SubtypeName = "Ice" });
				}
			}
			if (m_drill.BlockDefinition.SubtypeName.Contains("LargeBlockVaporator"))
			{
				m_drill.PowerConsumptionMultiplier = 500;
				if (m_drill.Enabled)
				{
				VRage.MyFixedPoint amount = (VRage.MyFixedPoint)(0.04 * w_density);
				inventory.AddItems(amount, new MyObjectBuilder_Ore() { SubtypeName = "Ice" });
				}
			}
			terminalBlock.RefreshCustomInfo();

            base.UpdateBeforeSimulation100();
        }

		//Info handler
		public void appendCustomInfo(Sandbox.ModAPI.IMyTerminalBlock block, StringBuilder info)
		{
			info.Clear ();
			info.AppendLine ("Atmosphere density: " + w_density.ToString("N") + " p");
		}


        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return builder;
        }
    }
}