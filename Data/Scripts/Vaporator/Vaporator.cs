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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), "LargeBlockVaporator")]
    public class Vaporator : MyGameLogicComponent
    {
        // Builder is nessassary for GetObjectBuilder method as far as I know.
        private MyObjectBuilder_EntityBase builder;
        private Sandbox.ModAPI.IMyOxygenGenerator m_generator;
        private IMyCubeBlock m_parent;
        private float w_density;

        Sandbox.ModAPI.IMyTerminalBlock terminalBlock;
        AtmosphereDetector atmoDet = new AtmosphereDetector();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_generator = Entity as Sandbox.ModAPI.IMyOxygenGenerator;
            m_parent = Entity as IMyCubeBlock;

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
            
            if (m_generator.IsWorking)
            {
                m_generator.PowerConsumptionMultiplier = 1 * (w_density >= 0.1f ? w_density : 0.1f);

                VRage.MyFixedPoint amount = (VRage.MyFixedPoint)(0.04 * w_density);
                inventory.AddItems(amount, new MyObjectBuilder_Ore() { SubtypeName = "Ice" });
            }
            else {
                m_generator.PowerConsumptionMultiplier = 1;
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