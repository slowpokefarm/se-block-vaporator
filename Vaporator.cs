using System;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Slowpokefarm.Vaporator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Refinery), true, "LargeBlockVaporator")]
    public class Vaporator : MyGameLogicComponent
    {
        // Builder is nessassary for GetObjectBuilder method as far as I know.
        private MyObjectBuilder_EntityBase builder;
		private Sandbox.ModAPI.IMyRefinery m_vaporator;
		private IMyCubeBlock m_parent;
        private float w_density;
		private float poweruse;

        Sandbox.ModAPI.IMyTerminalBlock terminalBlock;
        Vape_AtmosphereDetector atmoDet = new Vape_AtmosphereDetector();
        MoistureEvaporatorAnimation animation = new MoistureEvaporatorAnimation();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
			m_vaporator = Entity as Sandbox.ModAPI.IMyRefinery;
            m_parent = Entity as IMyCubeBlock;

            //m_vaporator.UpgradeValues.Add("Productivity", 0f);
            //m_vaporator.UpgradeValues.Add("Effectiveness", 1f);
            //m_vaporator.UpgradeValues.Add("PowerEfficiency", 1f);
			
            builder = objectBuilder;

            // MUST set the object to update atleast ONCE because you can not modify the inventory inside Init method!!!
            Entity.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
			
            terminalBlock = Entity as Sandbox.ModAPI.IMyTerminalBlock;
            terminalBlock.AppendingCustomInfo += appendCustomInfo;
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                var isHost = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE ||
                             MyAPIGateway.Multiplayer.IsServer;

                var isDedicatedHost = isHost && MyAPIGateway.Utilities.IsDedicated;

                if (isDedicatedHost)
                    return;

                animation.subparts = (m_vaporator as MyEntity).Subparts;
                animation.waterEvaporator = m_vaporator;
                //((waterEvaporator as MyEntity).Subparts.FirstPair().Value as MyEntitySubpart).OnClose += SubpartClosed;

                if (m_vaporator.Enabled && m_vaporator.IsWorking && m_vaporator.IsFunctional)
                {
                    animation.RotationAnimation(true);
                }
                else if (animation.TopCurrHeight > 0)
                {
                    animation.RotationAnimation(false);
                }
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            w_density = atmoDet.AtmosphereDetectionVaporator (this.Entity);
            IMyInventory inventory = ((Sandbox.ModAPI.IMyTerminalBlock)Entity).GetInventory(1) as IMyInventory;
			
			//check to see if the block is on
			if (m_vaporator.Enabled && m_vaporator.IsWorking && m_vaporator.IsFunctional)
			{    

				try
				{
					var sink = Entity.Components.Get<MyResourceSinkComponent>();

					poweruse = 0.4f * (1f + m_vaporator.UpgradeValues["Productivity"]) * (1f / m_vaporator.UpgradeValues["PowerEfficiency"]);
					
					if(sink != null)
					{
						sink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, poweruse);
					}
				}
				catch(Exception e)
				{
					MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
					MyLog.Default.WriteLine(e);
				}		
				//m_vaporator.PowerConsumptionMultiplier =  10 * (1f + m_vaporator.UpgradeValues["Productivity"]) * (1f / m_vaporator.UpgradeValues["PowerEfficiency"]);
			
				VRage.MyFixedPoint amount = (VRage.MyFixedPoint)(0.2 * (w_density * m_vaporator.UpgradeValues["Effectiveness"]) * (1 + m_vaporator.UpgradeValues["Productivity"]));
                inventory.AddItems(amount, new MyObjectBuilder_Ore() { SubtypeName = "Ice" });
			
			}
				
            terminalBlock.RefreshCustomInfo();
            base.UpdateBeforeSimulation100();
        }

        //Info handler
        public void appendCustomInfo(Sandbox.ModAPI.IMyTerminalBlock block, StringBuilder info)
        {
            info.Clear ();
            //info.Append("Current Required Input: ");
            //info.Append((m_vaporator.Enabled && m_vaporator.IsWorking && m_vaporator.IsFunctional) ? (100f * (1f + m_vaporator.UpgradeValues["Productivity"]) * (1f / m_vaporator.UpgradeValues["PowerEfficiency"])).ToString("N") : 0f.ToString("N"));
            //info.Append(" kW \n");
            info.AppendFormat("\n");
            //info.AppendFormat("\n");
            //info.Append("Productivity: ");
            //info.Append(((m_vaporator.UpgradeValues["Productivity"] + 1f) * 100f).ToString("F0"));
            //info.Append("%\n");
            //info.Append("Effectiveness: ");
            //info.Append(((m_vaporator.UpgradeValues["Effectiveness"]) * 100f).ToString("F0"));
            //info.Append("%\n");
            //info.Append("Power Efficiency: ");
            //info.Append(((m_vaporator.UpgradeValues["PowerEfficiency"]) * 100f).ToString("F0"));
            //info.Append("%\n");
			
            info.AppendLine ("Atmosphere density: " + w_density.ToString("N") + " p");
            // info.AppendLine ("OwnerId: " + ((MyCubeBlock)Entity).OwnerId );
        }


        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return builder;
        }
    }
}