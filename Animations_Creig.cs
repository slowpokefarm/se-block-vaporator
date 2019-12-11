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
    public class MoistureEvaporatorAnimation
    {
        /* Thanks to CreigWarfare from Official Discord for helping with the Animation code <3 */

        // Config values
        public float TopCurrHeight = 0f;
        private float TopMaxHeight = 1f; //meters       
        private float incrementAmount = 0.003f;

        private int AnimationLoop = 0;
        private bool playAnimation = true;
        public Dictionary<string, MyEntitySubpart> subparts;

        public IMyRefinery waterEvaporator;

        private bool subPartEventregistered;

        public void RotationAnimation(bool on)
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
    }
}
