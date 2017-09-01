using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_WandererJoin : IncidentWorker
	{
		private const float RelationWithColonistWeight = 20f;

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 loc;
			if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out loc))
			{
				return false;
			}
			PawnKindDef pawnKindDef = new List<PawnKindDef>
			{
				PawnKindDefOf.Villager
			}.RandomElement<PawnKindDef>();
			PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, false, false, null, null, null, null, null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			GenSpawn.Spawn(pawn, loc, map);
			string text = "WandererJoin".Translate(new object[]
			{
				pawnKindDef.label,
				pawn.story.Title.ToLower()
			});
			text = text.AdjustedFor(pawn);
			string label = "LetterLabelWandererJoin".Translate();
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Good, pawn, null);
			return true;
		}
	}
}
