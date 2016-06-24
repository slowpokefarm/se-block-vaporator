using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using Sandbox.Game.EntityComponents;
using System.Collections.Generic;
using VRage.ModAPI;
using System.Text;
using System;
using VRageMath;
using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Utils;
using Sandbox.Game.Entities;

namespace Slowpokefarm.Vaporator
{
	[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
	public class WorldPlanets : MySessionComponentBase
	{
			public static Dictionary<long, MyPlanet> planets = new Dictionary<long, MyPlanet>();
			public static List<long> removePlanets = new List<long>();
			private int update = 0;
			private const int Update_Ticks = 120;

			private static HashSet<IMyEntity> ents = new HashSet<IMyEntity>(); // this is always empty

			public override void UpdateAfterSimulation()
			{
			if(++update >= Update_Ticks)
				{
					update = 0;
					MyAPIGateway.Entities.GetEntities(ents, delegate(IMyEntity e)
					{
						if(e is MyPlanet)
						{
							if (!WorldPlanets.planets.ContainsKey(e.EntityId))
								WorldPlanets.planets.Add(e.EntityId, e as MyPlanet);
						}

							return false; // no reason to add to the list
					}
					);
				}
			}

			protected override void UnloadData()
			{
				planets.Clear();
				removePlanets.Clear();
				ents.Clear();
			}
	}
}