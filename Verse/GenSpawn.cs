using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenSpawn
	{
		public static Thing Spawn(ThingDef def, IntVec3 loc, Map map)
		{
			return GenSpawn.Spawn(ThingMaker.MakeThing(def, null), loc, map);
		}

		public static Thing Spawn(Thing newThing, IntVec3 loc, Map map)
		{
			return GenSpawn.Spawn(newThing, loc, map, Rot4.North, false);
		}

		public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, bool respawningAfterLoad = false)
		{
			if (map == null)
			{
				Log.Error("Tried to spawn " + newThing + " in a null map.");
				return null;
			}
			if (!loc.InBounds(map))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to spawn ",
					newThing,
					" out of bounds at ",
					loc,
					"."
				}));
				return null;
			}
			if (newThing.Spawned)
			{
				Log.Error("Tried to spawn " + newThing + " but it's already spawned.");
				return newThing;
			}
			GenSpawn.WipeExistingThings(loc, rot, newThing.def, map, DestroyMode.Vanish);
			if (newThing.def.randomizeRotationOnSpawn)
			{
				newThing.Rotation = Rot4.Random;
			}
			else
			{
				newThing.Rotation = rot;
			}
			newThing.Position = loc;
			if (newThing.holdingOwner != null)
			{
				newThing.holdingOwner.Remove(newThing);
			}
			newThing.SpawnSetup(map, respawningAfterLoad);
			if (newThing.Spawned && newThing.stackCount == 0)
			{
				Log.Error("Spawned thing with 0 stackCount: " + newThing);
				newThing.Destroy(DestroyMode.Vanish);
				return null;
			}
			return newThing;
		}

		public static void SpawnBuildingAsPossible(Building building, Map map, bool respawningAfterLoad = false)
		{
			bool flag = false;
			foreach (IntVec3 current in building.OccupiedRect())
			{
				List<Thing> thingList = current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] is Pawn && building.def.passability == Traversability.Impassable)
					{
						flag = true;
						break;
					}
					if ((thingList[i].def.category == ThingCategory.Building || thingList[i].def.category == ThingCategory.Item) && GenSpawn.SpawningWipes(building.def, thingList[i].def))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				bool flag2 = false;
				if (building.def.Minifiable)
				{
					MinifiedThing minifiedThing = building.MakeMinified();
					if (GenPlace.TryPlaceThing(minifiedThing, building.Position, map, ThingPlaceMode.Near, null))
					{
						flag2 = true;
					}
					else
					{
						minifiedThing.GetDirectlyHeldThings().Clear();
						minifiedThing.Destroy(DestroyMode.Vanish);
					}
				}
				if (!flag2)
				{
					GenLeaving.DoLeavingsFor(building, map, DestroyMode.Refund, building.OccupiedRect());
				}
			}
			else
			{
				GenSpawn.Spawn(building, building.Position, map, building.Rotation, respawningAfterLoad);
			}
		}

		public static void WipeExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map, DestroyMode mode)
		{
			foreach (IntVec3 current in GenAdj.CellsOccupiedBy(thingPos, thingRot, thingDef.Size))
			{
				foreach (Thing current2 in map.thingGrid.ThingsAt(current).ToList<Thing>())
				{
					if (GenSpawn.SpawningWipes(thingDef, current2.def))
					{
						current2.Destroy(mode);
					}
				}
			}
		}

		public static bool WouldWipeAnythingWith(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map, Predicate<Thing> predicate)
		{
			return GenSpawn.WouldWipeAnythingWith(GenAdj.OccupiedRect(thingPos, thingRot, thingDef.Size), thingDef, map, predicate);
		}

		public static bool WouldWipeAnythingWith(CellRect cellRect, BuildableDef thingDef, Map map, Predicate<Thing> predicate)
		{
			foreach (IntVec3 current in cellRect)
			{
				foreach (Thing current2 in map.thingGrid.ThingsAt(current).ToList<Thing>())
				{
					if (GenSpawn.SpawningWipes(thingDef, current2.def) && predicate(current2))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
		{
			ThingDef thingDef = newEntDef as ThingDef;
			ThingDef thingDef2 = oldEntDef as ThingDef;
			if (thingDef == null || thingDef2 == null)
			{
				return false;
			}
			if (thingDef.category == ThingCategory.Attachment || thingDef.category == ThingCategory.Mote || thingDef.category == ThingCategory.Filth || thingDef.category == ThingCategory.Projectile)
			{
				return false;
			}
			if (!thingDef2.destroyable)
			{
				return false;
			}
			if (thingDef.category == ThingCategory.Plant)
			{
				return false;
			}
			if (thingDef2.category == ThingCategory.Filth && thingDef.passability != Traversability.Standable)
			{
				return true;
			}
			if (thingDef2.category == ThingCategory.Item && thingDef.passability == Traversability.Impassable && thingDef.surfaceType == SurfaceType.None)
			{
				return true;
			}
			if (thingDef.EverTransmitsPower && thingDef2 == ThingDefOf.PowerConduit)
			{
				return true;
			}
			if (thingDef.IsFrame && GenSpawn.SpawningWipes(thingDef.entityDefToBuild, oldEntDef))
			{
				return true;
			}
			BuildableDef buildableDef = GenConstruct.BuiltDefOf(thingDef);
			BuildableDef buildableDef2 = GenConstruct.BuiltDefOf(thingDef2);
			if (buildableDef == null || buildableDef2 == null)
			{
				return false;
			}
			ThingDef thingDef3 = thingDef.entityDefToBuild as ThingDef;
			if (thingDef2.IsBlueprint)
			{
				if (thingDef.IsBlueprint)
				{
					if (thingDef3 != null && thingDef3.building != null && thingDef3.building.canPlaceOverWall && thingDef2.entityDefToBuild is ThingDef && (ThingDef)thingDef2.entityDefToBuild == ThingDefOf.Wall)
					{
						return true;
					}
					if (thingDef2.entityDefToBuild is TerrainDef)
					{
						if (thingDef.entityDefToBuild is ThingDef && ((ThingDef)thingDef.entityDefToBuild).coversFloor)
						{
							return true;
						}
						if (thingDef.entityDefToBuild is TerrainDef)
						{
							return true;
						}
					}
				}
				return thingDef2.entityDefToBuild == ThingDefOf.PowerConduit && thingDef.entityDefToBuild is ThingDef && (thingDef.entityDefToBuild as ThingDef).EverTransmitsPower;
			}
			if ((thingDef2.IsFrame || thingDef2.IsBlueprint) && thingDef2.entityDefToBuild is TerrainDef)
			{
				ThingDef thingDef4 = buildableDef as ThingDef;
				if (thingDef4 != null && !thingDef4.CoexistsWithFloors)
				{
					return true;
				}
			}
			if (thingDef2 == ThingDefOf.ActiveDropPod)
			{
				return false;
			}
			if (thingDef == ThingDefOf.ActiveDropPod)
			{
				return thingDef2 != ThingDefOf.ActiveDropPod && (thingDef2.category == ThingCategory.Building && thingDef2.passability == Traversability.Impassable);
			}
			if (thingDef.IsEdifice())
			{
				if (thingDef.BlockPlanting && thingDef2.category == ThingCategory.Plant)
				{
					return true;
				}
				if (!(buildableDef is TerrainDef) && buildableDef2.IsEdifice())
				{
					return true;
				}
			}
			return false;
		}
	}
}
