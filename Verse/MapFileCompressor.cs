using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Verse
{
	public class MapFileCompressor : IExposable
	{
		private string compressedString;

		public void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.compressedString, "compressedThingMap", null, false);
		}

		public void ReadDataFromMap()
		{
			CompressibilityDecider.DetermineReferences();
			this.compressedString = GridSaveUtility.CompressedStringForShortGrid(new Func<IntVec3, ushort>(this.HashValueForSquare));
		}

		private ushort HashValueForSquare(IntVec3 curSq)
		{
			ushort num = 0;
			foreach (Thing current in Find.ThingGrid.ThingsAt(curSq))
			{
				if (current.IsSaveCompressible())
				{
					if (num != 0)
					{
						Log.Error(string.Concat(new object[]
						{
							"Found two compressible things in ",
							curSq,
							". The last was ",
							current
						}));
					}
					num = current.def.shortHash;
				}
			}
			return num;
		}

		[DebuggerHidden]
		public IEnumerable<Thing> ThingsToSpawnAfterLoad()
		{
			Dictionary<ushort, ThingDef> thingDefsByShortHash = new Dictionary<ushort, ThingDef>();
			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
			{
				if (thingDefsByShortHash.ContainsKey(def.shortHash))
				{
					Log.Error(string.Concat(new object[]
					{
						"Hash collision between ",
						def,
						" and  ",
						thingDefsByShortHash[def.shortHash],
						": both have short hash ",
						def.shortHash
					}));
				}
				else
				{
					thingDefsByShortHash.Add(def.shortHash, def);
				}
			}
			foreach (GridSaveUtility.LoadedGridShort gridThing in GridSaveUtility.LoadedUShortGrid(this.compressedString))
			{
				if (gridThing.val != 0)
				{
					ThingDef def2 = null;
					try
					{
						def2 = thingDefsByShortHash[gridThing.val];
					}
					catch (KeyNotFoundException)
					{
						Log.Error("Map compressor decompression error: No thingDef with short hash " + gridThing.val + ". Adding as null to dictionary.");
						thingDefsByShortHash.Add(gridThing.val, null);
					}
					Thing th = ThingMaker.MakeThing(def2, null);
					th.SetPositionDirect(gridThing.cell);
					yield return th;
				}
			}
		}
	}
}