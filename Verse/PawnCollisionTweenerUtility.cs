using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class PawnCollisionTweenerUtility
	{
		private const float Radius = 0.32f;

		public static Vector3 PawnCollisionPosOffsetFor(Pawn pawn)
		{
			if (pawn.GetPosture() != PawnPosture.Standing)
			{
				return Vector3.zero;
			}
			bool flag = pawn.Spawned && pawn.pather.MovingNow;
			if (!flag || pawn.pather.nextCell == pawn.pather.Destination.Cell)
			{
				if (!flag && pawn.Drawer.leaner.ShouldLean())
				{
					return Vector3.zero;
				}
				IntVec3 at;
				if (flag)
				{
					at = pawn.pather.nextCell;
				}
				else
				{
					at = pawn.Position;
				}
				int num = 0;
				int vertexIndex = 0;
				PawnCollisionTweenerUtility.GetPawnsStandingAtOrAboutToStandAt(at, out num, out vertexIndex, pawn);
				if (num == 0)
				{
					return Vector3.zero;
				}
				return GenGeo.RegularPolygonVertexPosition(num, vertexIndex) * 0.32f;
			}
			else
			{
				IntVec3 nextCell = pawn.pather.nextCell;
				if (PawnCollisionTweenerUtility.CanGoDirectlyToNextCell(pawn))
				{
					return Vector3.zero;
				}
				int num2 = pawn.thingIDNumber % 2;
				if (nextCell.x != pawn.Position.x)
				{
					if (num2 == 0)
					{
						return new Vector3(0f, 0f, 0.32f);
					}
					return new Vector3(0f, 0f, -0.32f);
				}
				else
				{
					if (num2 == 0)
					{
						return new Vector3(0.32f, 0f, 0f);
					}
					return new Vector3(-0.32f, 0f, 0f);
				}
			}
		}

		private static void GetPawnsStandingAtOrAboutToStandAt(IntVec3 at, out int pawnsCount, out int pawnsWithLowerIdCount, Pawn forPawn)
		{
			pawnsCount = 0;
			pawnsWithLowerIdCount = 0;
			foreach (IntVec3 current in CellRect.SingleCell(at).ExpandedBy(1))
			{
				if (current.InBounds())
				{
					List<Thing> thingList = current.GetThingList();
					for (int i = 0; i < thingList.Count; i++)
					{
						Pawn pawn = thingList[i] as Pawn;
						if (pawn != null)
						{
							if (pawn.GetPosture() == PawnPosture.Standing)
							{
								if (current != at)
								{
									if (!pawn.pather.MovingNow || pawn.pather.nextCell != pawn.pather.Destination.Cell || pawn.pather.Destination.Cell != at)
									{
										goto IL_11B;
									}
								}
								else if (pawn.pather.MovingNow)
								{
									goto IL_11B;
								}
								pawnsCount++;
								if (pawn.thingIDNumber < forPawn.thingIDNumber)
								{
									pawnsWithLowerIdCount++;
								}
							}
						}
						IL_11B:;
					}
				}
			}
		}

		private static bool CanGoDirectlyToNextCell(Pawn pawn)
		{
			IntVec3 nextCell = pawn.pather.nextCell;
			foreach (IntVec3 current in CellRect.FromLimits(nextCell, pawn.Position).ExpandedBy(1))
			{
				if (current.InBounds())
				{
					List<Thing> thingList = current.GetThingList();
					for (int i = 0; i < thingList.Count; i++)
					{
						Pawn pawn2 = thingList[i] as Pawn;
						if (pawn2 != null && pawn2 != pawn)
						{
							if (pawn2.GetPosture() == PawnPosture.Standing)
							{
								if (pawn2.pather.MovingNow)
								{
									if (((pawn2.Position == nextCell && PawnCollisionTweenerUtility.WillBeFasterOnNextCell(pawn, pawn2)) || pawn2.pather.nextCell == nextCell || pawn2.Position == pawn.Position || (pawn2.pather.nextCell == pawn.Position && PawnCollisionTweenerUtility.WillBeFasterOnNextCell(pawn2, pawn))) && pawn2.thingIDNumber < pawn.thingIDNumber)
									{
										bool result = false;
										return result;
									}
								}
								else if (pawn2.Position == pawn.Position || pawn2.Position == nextCell)
								{
									bool result = false;
									return result;
								}
							}
						}
					}
				}
			}
			return true;
		}

		private static bool WillBeFasterOnNextCell(Pawn p1, Pawn p2)
		{
			if (p1.pather.nextCellCostLeft == p2.pather.nextCellCostLeft)
			{
				return p1.thingIDNumber < p2.thingIDNumber;
			}
			return p1.pather.nextCellCostLeft < p2.pather.nextCellCostLeft;
		}
	}
}
