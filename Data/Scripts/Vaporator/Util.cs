using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.ModAPI;
using System.Collections.Generic;

namespace Slowpokefarm.Vaporator {
	public class Util {
		public static bool isGameSimulating() {
			return (MyAPIGateway.Session != null);
		}

		public static bool isDedicatedServer() {
			return (MyAPIGateway.Session != null) && (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer) && MyAPIGateway.Utilities.IsDedicated;
		}

		private static HashSet<MyPlanet> ALL_PLANETS = null;
		public static HashSet<MyPlanet> getAllBreathablePlanets() {
			if (ALL_PLANETS == null) {
				HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
				MyAPIGateway.Entities.GetEntities(entities, (entity) => entity is MyPlanet && !entity.Closed && !entity.MarkedForClose && (entity as MyPlanet).HasAtmosphere);
				HashSet<MyPlanet> planets = new HashSet<MyPlanet>();
				foreach(IMyEntity entity in entities) {
					planets.Add(entity as MyPlanet);
				}
				ALL_PLANETS = planets;
			}
			return ALL_PLANETS;
		}
	}
}