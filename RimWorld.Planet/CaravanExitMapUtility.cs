using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanExitMapUtility
	{
		private static List<int> tmpNeighbors = new List<int>();

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<int> retTiles = new List<int>();

		private static List<int> tileCandidates = new List<int>();

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, Direction8Way dir)
		{
			int directionTile = CaravanExitMapUtility.FindRandomStartingTileBasedOnExitDir(exitFromTile, dir);
			return CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, exitFromTile, directionTile);
		}

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile)
		{
			if (Find.World.Impassable(directionTile))
			{
				directionTile = exitFromTile;
			}
			if (Find.World.Impassable(exitFromTile))
			{
				exitFromTile = directionTile;
			}
			Caravan caravan = CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, faction, exitFromTile);
			if (caravan.Tile != directionTile)
			{
				caravan.pather.StartPath(directionTile, null, true);
				caravan.pather.nextTileCostLeft /= 2f;
				caravan.tweener.ResetToPosition();
			}
			return caravan;
		}

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int startingTile)
		{
			if (!GenWorldClosest.TryFindClosestPassableTile(startingTile, out startingTile))
			{
				Log.Error("Could not find any passable tile for a new caravan.");
				return null;
			}
			CaravanExitMapUtility.tmpPawns.Clear();
			CaravanExitMapUtility.tmpPawns.AddRange(pawns);
			Map map = null;
			for (int i = 0; i < CaravanExitMapUtility.tmpPawns.Count; i++)
			{
				map = CaravanExitMapUtility.tmpPawns[i].MapHeld;
				if (map != null)
				{
					break;
				}
			}
			Caravan caravan = CaravanMaker.MakeCaravan(CaravanExitMapUtility.tmpPawns, faction, startingTile, false);
			for (int j = 0; j < CaravanExitMapUtility.tmpPawns.Count; j++)
			{
				CaravanExitMapUtility.tmpPawns[j].ExitMap(false);
			}
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int k = 0; k < pawnsListForReading.Count; k++)
			{
				if (!pawnsListForReading[k].IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawnsListForReading[k], PawnDiscardDecideMode.Decide);
				}
			}
			if (map != null)
			{
				map.info.parent.Notify_CaravanFormed(caravan);
			}
			return caravan;
		}

		public static void ExitMapAndJoinOrCreateCaravan(Pawn pawn)
		{
			CaravanExitMapUtility.GenerateCaravanExitTale(pawn);
			Caravan caravan = CaravanExitMapUtility.FindCaravanToJoinFor(pawn);
			if (caravan != null)
			{
				pawn.DeSpawn();
				caravan.AddPawn(pawn, true);
				pawn.ExitMap(false);
			}
			else if (pawn.IsColonist)
			{
				List<int> list = CaravanExitMapUtility.AvailableExitTilesAt(pawn.Map);
				Caravan caravan2 = CaravanExitMapUtility.ExitMapAndCreateCaravan(Gen.YieldSingle<Pawn>(pawn), pawn.Faction, pawn.Map.Tile, (!list.Any<int>()) ? pawn.Map.Tile : list.RandomElement<int>());
				caravan2.autoJoinable = true;
				if (pawn.Faction == Faction.OfPlayer)
				{
					Messages.Message("MessagePawnLeftMapAndCreatedCaravan".Translate(new object[]
					{
						pawn.LabelShort
					}).CapitalizeFirst(), caravan2, MessageTypeDefOf.TaskCompletion);
				}
			}
			else
			{
				Log.Error("Pawn " + pawn + " didn't find any caravan to join, and he can't create one.");
			}
		}

		public static bool CanExitMapAndJoinOrCreateCaravanNow(Pawn pawn)
		{
			return pawn.Spawned && pawn.Map.exitMapGrid.MapUsesExitGrid && (pawn.IsColonist || CaravanExitMapUtility.FindCaravanToJoinFor(pawn) != null);
		}

		public static List<int> AvailableExitTilesAt(Map map)
		{
			CaravanExitMapUtility.retTiles.Clear();
			int currentTileID = map.Tile;
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(currentTileID, CaravanExitMapUtility.tmpNeighbors);
			for (int i = 0; i < CaravanExitMapUtility.tmpNeighbors.Count; i++)
			{
				int num = CaravanExitMapUtility.tmpNeighbors[i];
				if (CaravanExitMapUtility.IsGoodCaravanStartingTile(num))
				{
					Rot4 rotFromTo = grid.GetRotFromTo(currentTileID, num);
					IntVec3 intVec;
					if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Walkable(map) && !x.Fogged(map), map, rotFromTo, CellFinder.EdgeRoadChance_Ignore, out intVec))
					{
						CaravanExitMapUtility.retTiles.Add(num);
					}
				}
			}
			CaravanExitMapUtility.retTiles.SortBy((int x) => grid.GetHeadingFromTo(currentTileID, x));
			return CaravanExitMapUtility.retTiles;
		}

		public static int RandomBestExitTileFrom(Map map)
		{
			Tile tile = map.TileInfo;
			List<int> options = CaravanExitMapUtility.AvailableExitTilesAt(map);
			if (!options.Any<int>())
			{
				return -1;
			}
			if (tile.roads == null)
			{
				return options.RandomElement<int>();
			}
			int bestRoadIndex = -1;
			for (int i = 0; i < tile.roads.Count; i++)
			{
				if (options.Contains(tile.roads[i].neighbor))
				{
					if (bestRoadIndex == -1 || tile.roads[i].road.priority > tile.roads[bestRoadIndex].road.priority)
					{
						bestRoadIndex = i;
					}
				}
			}
			if (bestRoadIndex == -1)
			{
				return options.RandomElement<int>();
			}
			return (from rl in tile.roads
			where options.Contains(rl.neighbor) && rl.road == tile.roads[bestRoadIndex].road
			select rl).RandomElement<Tile.RoadLink>().neighbor;
		}

		private static int FindRandomStartingTileBasedOnExitDir(int tileID, Direction8Way exitDir)
		{
			CaravanExitMapUtility.tileCandidates.Clear();
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(tileID, CaravanExitMapUtility.tmpNeighbors);
			for (int i = 0; i < CaravanExitMapUtility.tmpNeighbors.Count; i++)
			{
				int num = CaravanExitMapUtility.tmpNeighbors[i];
				if (grid.GetDirection8WayFromTo(tileID, num) == exitDir)
				{
					CaravanExitMapUtility.tileCandidates.Add(num);
				}
			}
			if (CaravanExitMapUtility.tileCandidates.Count == 0)
			{
				return tileID;
			}
			int result;
			if (CaravanExitMapUtility.tileCandidates.Where(new Func<int, bool>(CaravanExitMapUtility.IsGoodCaravanStartingTile)).TryRandomElement(out result))
			{
				return result;
			}
			return CaravanExitMapUtility.tileCandidates.RandomElement<int>();
		}

		private static bool IsGoodCaravanStartingTile(int tile)
		{
			return !Find.World.Impassable(tile);
		}

		public static Caravan FindCaravanToJoinFor(Pawn pawn)
		{
			if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
			{
				return null;
			}
			int tile = pawn.Map.Tile;
			Find.WorldGrid.GetTileNeighbors(tile, CaravanExitMapUtility.tmpNeighbors);
			CaravanExitMapUtility.tmpNeighbors.Add(tile);
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (CaravanExitMapUtility.tmpNeighbors.Contains(caravan.Tile))
				{
					if (caravan.autoJoinable)
					{
						if (pawn.HostFaction == null)
						{
							if (caravan.Faction == pawn.Faction)
							{
								return caravan;
							}
						}
						else if (caravan.Faction == pawn.HostFaction)
						{
							return caravan;
						}
					}
				}
			}
			return null;
		}

		public static bool IsTheOnlyJoinableCaravanForAnyPrisonerOrAnimal(Caravan c)
		{
			if (!c.autoJoinable)
			{
				Log.Warning("This caravan is not auto joinable.");
				return false;
			}
			if (c.Faction != Faction.OfPlayer)
			{
				Log.Warning("Only player caravans supported.");
				return false;
			}
			List<Map> maps = Find.Maps;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			int i = 0;
			while (i < maps.Count)
			{
				Map map = maps[i];
				if (!map.IsPlayerHome && Find.WorldGrid.IsNeighborOrSame(c.Tile, map.Tile) && map.mapPawns.FreeColonistsCount == 0)
				{
					if (map.mapPawns.AllPawns.Any((Pawn x) => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer))
					{
						bool flag = false;
						for (int j = 0; j < caravans.Count; j++)
						{
							Caravan caravan = caravans[j];
							if (c != caravan && caravan.autoJoinable && caravan.Faction == c.Faction && Find.WorldGrid.IsNeighborOrSame(caravan.Tile, map.Tile))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							return true;
						}
					}
				}
				IL_145:
				i++;
				continue;
				goto IL_145;
			}
			return false;
		}

		public static void OpenTheOnlyJoinableCaravanForPrisonerOrAnimalDialog(Caravan c, Action confirmAction)
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmLeavePrisonersOrAnimalsBehind".Translate(), confirmAction, false, null));
		}

		public static void GenerateCaravanExitTale(Pawn pawn)
		{
			if (pawn.Spawned && pawn.IsFreeColonist)
			{
				if (pawn.Map.IsPlayerHome)
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanFormed, new object[]
					{
						pawn
					});
				}
				else if (GenHostility.AnyHostileActiveThreatToPlayer(pawn.Map))
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanFled, new object[]
					{
						pawn
					});
				}
			}
		}
	}
}
