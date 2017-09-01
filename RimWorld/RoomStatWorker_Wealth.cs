using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomStatWorker_Wealth : RoomStatWorker
	{
		public override float GetScore(Room room)
		{
			float num = 0f;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				if (thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Plant)
				{
					num += (float)thing.stackCount * thing.GetInnerIfMinified().MarketValue;
				}
			}
			foreach (IntVec3 current in room.Cells)
			{
				num += current.GetTerrain(room.Map).GetStatValueAbstract(StatDefOf.MarketValue, null);
			}
			return num;
		}
	}
}
