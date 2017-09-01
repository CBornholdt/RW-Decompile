using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		private const float HivePoints = 400f;

		protected override bool CanFireNowSub(IIncidentTarget target)
		{
			Map map = (Map)target;
			IntVec3 intVec;
			return base.CanFireNowSub(target) && HivesUtility.TotalSpawnedHivesCount(map) < 30 && InfestationCellFinder.TryFindCell(out intVec, map);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Hive t = null;
			int num;
			for (int i = Mathf.Max(GenMath.RoundRandom(parms.points / 400f), 1); i > 0; i -= num)
			{
				num = Mathf.Min(3, i);
				t = this.SpawnHiveCluster(num, map);
			}
			base.SendStandardLetter(t, new string[0]);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}

		private Hive SpawnHiveCluster(int hiveCount, Map map)
		{
			IntVec3 loc;
			if (!InfestationCellFinder.TryFindCell(out loc, map))
			{
				return null;
			}
			Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Hive, null), loc, map);
			hive.SetFaction(Faction.OfInsects, null);
			IncidentWorker_Infestation.SpawnInsectJellyInstantly(hive);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				Hive hive2;
				if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(false, out hive2))
				{
					IncidentWorker_Infestation.SpawnInsectJellyInstantly(hive2);
					hive = hive2;
				}
			}
			return hive;
		}

		private static void SpawnInsectJellyInstantly(Hive hive)
		{
			CompSpawner compSpawner = (CompSpawner)hive.AllComps.Find(delegate(ThingComp x)
			{
				CompSpawner compSpawner2 = x as CompSpawner;
				return compSpawner2 != null && compSpawner2.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly;
			});
			if (compSpawner != null)
			{
				compSpawner.TryDoSpawn();
			}
		}
	}
}
