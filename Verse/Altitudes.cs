using System;
using UnityEngine;

namespace Verse
{
	public static class Altitudes
	{
		private const int NumAltitudeLayers = 31;

		private const float LayerSpacing = 0.46875f;

		public const float AltInc = 0.046875f;

		private static readonly float[] Alts;

		public static readonly Vector3 AltIncVect;

		static Altitudes()
		{
			Altitudes.Alts = new float[31];
			Altitudes.AltIncVect = new Vector3(0f, 0.046875f, 0f);
			for (int i = 0; i < 31; i++)
			{
				Altitudes.Alts[i] = (float)i * 0.46875f;
			}
		}

		public static float AltitudeFor(AltitudeLayer alt)
		{
			return Altitudes.Alts[(int)alt];
		}
	}
}
