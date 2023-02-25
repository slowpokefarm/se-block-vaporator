using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowpokefarm.Vaporator {
	public class Config {
		public float AirDensityMin;
		public double IceAmountBase;
		public float PowerUsageBase;

		public Config() {
			AirDensityMin = 0.01f;
			IceAmountBase = 0.2;
			PowerUsageBase = 0.4f;
		}
	}
}
