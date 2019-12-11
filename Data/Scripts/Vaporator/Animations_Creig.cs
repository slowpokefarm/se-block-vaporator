using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using System.Collections.Generic;
using VRage.ModAPI;
using Sandbox.ModAPI;
using VRageMath;
using System;
using System.Linq;
using Sandbox.Game;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.Game.Entities;
using VRageRender;
using Sandbox.Game.Lights;

namespace Slowpokefarm.Vaporator
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Refinery), false, "LargeBlockVaporator")]
    public class MoistureEvaporatorAnimation : MyGameLogicComponent
    {
        /* Thanks to CreigWarfare from Official Discord for helping with the Animation code <3 */

        //configs
        private float TopMaxHeight = 1f; //meters       
        private float TopCurrHeight = 0f;
        private float incrementAmount = 0.003f;

        private int AnimationLoop = 0;
        private bool playAnimation = true;
        public Dictionary<string, MyEntitySubpart> subparts;

        MyObjectBuilder_EntityBase objectBuilder = null;
        IMyCubeBlock waterEvaporator = null;
        private bool subPartEventregistered;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                base.Init(objectBuilder);
                this.objectBuilder = objectBuilder;
                waterEvaporator = Entity as IMyCubeBlock;
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Init Error" + e, 10000, "Red");
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return objectBuilder;
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

                subparts = (waterEvaporator as MyEntity).Subparts;
                //((waterEvaporator as MyEntity).Subparts.FirstPair().Value as MyEntitySubpart).OnClose += SubpartClosed;

                if (waterEvaporator.IsWorking)
                {
                    RotationAnimation(true);
                }
                else if (TopCurrHeight > 0)
                {
                    RotationAnimation(false);
                }
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        private void RotationAnimation(bool on)
        {
            try
            {
                if (subparts != null)
                {
                    foreach (var subpart in subparts)
                    {
                        if (AnimationLoop == 1500)
                        {
                            AnimationLoop = 0;
                        }

                        if (subpart.Key == "MoistureVaporator_Top")
                        {
                            float rotation = -0.005f;
                            MyEntitySubpart subPart = subpart.Value as MyEntitySubpart;

                            if (!subPartEventregistered)
                            {
                                subPart.OnClose += SubpartClosed;
                                subPartEventregistered = true;
                            }

                            Matrix matrix = subpart.Value.PositionComp.LocalMatrix * MatrixD.CreateRotationY(rotation);

                            if (on && TopCurrHeight < TopMaxHeight)
                            {
                                var increaseAmount = TopCurrHeight + incrementAmount <= TopMaxHeight ? incrementAmount : TopMaxHeight - TopCurrHeight;
                                matrix.Translation += new Vector3(0, increaseAmount, 0);
                                TopCurrHeight += increaseAmount;
                                
                            }
                            else if (!on && TopCurrHeight > 0)
                            {
                                var decreaseAmount = TopCurrHeight - incrementAmount >= 0 ? incrementAmount : TopCurrHeight;
                                matrix.Translation -= new Vector3(0, decreaseAmount, 0);
                                TopCurrHeight -= decreaseAmount;
                            }

                            subpart.Value.PositionComp.LocalMatrix = matrix;
                            AnimationLoop++;
                        }

                        if (subpart.Key == "MoistureVaporator_TopSpinner")
                        {
                            var rotation = -0.005f;
                            var initialMatrix = subpart.Value.PositionComp.LocalMatrix;
                            var rotationMatrix = MatrixD.CreateRotationY(rotation);
                            var matrix = rotationMatrix * initialMatrix;
                            subpart.Value.PositionComp.LocalMatrix = matrix;
                            AnimationLoop++;
                        }
                        if (subpart.Key == "MoistureVaporator_Middle1" || subpart.Key == "MoistureVaporator_Middle3")
                        {
                            var rotation = 0.005f;
                            var initialMatrix = subpart.Value.PositionComp.LocalMatrix;
                            var rotationMatrix = MatrixD.CreateRotationY(rotation);
                            var matrix = rotationMatrix * initialMatrix;
                            subpart.Value.PositionComp.LocalMatrix = matrix;
                            AnimationLoop++;
                        }
                        if (subpart.Key == "MoistureVaporator_Middle2")
                        {
                            var rotation = -0.005f;
                            var initialMatrix = subpart.Value.PositionComp.LocalMatrix;
                            var rotationMatrix = MatrixD.CreateRotationY(rotation);
                            var matrix = rotationMatrix * initialMatrix;
                            subpart.Value.PositionComp.LocalMatrix = matrix;
                            AnimationLoop++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Animation Error" + e, 2500, "Red");
            }
        }

        internal void SubpartClosed(MyEntity subPartEntity)
        {
            if (subPartEntity != null && waterEvaporator != null && !waterEvaporator.MarkedForClose)
            {
                TopCurrHeight = 0;
                subPartEventregistered = false;
                subPartEntity.OnClose -= SubpartClosed;
            }
        }

        public override void Close()
        {
        }
    }
}
