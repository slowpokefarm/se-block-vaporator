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
	public class Vape_AtmosphereDetector
	{
		public float AtmosphereDetectionVaporator( IMyEntity ent)
		{
			foreach (var pl in Vape_WorldPlanets.planets)
			{
				var planet = pl.Value;

				if (planet.Closed || planet.MarkedForClose)
				{
					Vape_WorldPlanets.removePlanets.Add(pl.Key);
					continue;
				}
				if (planet.HasAtmosphere && Vector3D.DistanceSquared(ent.GetPosition(), planet.WorldMatrix.Translation) < (planet.AtmosphereRadius * planet.AtmosphereRadius))
				{
					return planet.GetAirDensity(ent.GetPosition());
				}
			}
			if (Vape_WorldPlanets.removePlanets.Count > 0)
			{
				foreach (var id in Vape_WorldPlanets.removePlanets)
					Vape_WorldPlanets.planets.Remove(id);

				Vape_WorldPlanets.removePlanets.Clear();
			}
			return 0;
		}
	}
}