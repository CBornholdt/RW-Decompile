using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ExecutionUtility
	{
		public static void DoExecutionByCut(Pawn executioner, Pawn victim)
		{
			Map map = victim.Map;
			IntVec3 position = victim.Position;
			int num = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 8f), 1);
			for (int i = 0; i < num; i++)
			{
				victim.health.DropBloodFilth();
			}
			BodyPartRecord bodyPartRecord = ExecutionUtility.ExecuteCutPart(victim);
			int amount = Mathf.Clamp((int)victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20);
			DamageInfo damageInfo = new DamageInfo(DamageDefOf.ExecutionCut, amount, -1f, executioner, bodyPartRecord, null, DamageInfo.SourceCategory.ThingOrUnknown);
			victim.TakeDamage(damageInfo);
			if (!victim.Dead)
			{
				victim.Kill(new DamageInfo?(damageInfo));
			}
			Thing thing = position.GetThingList(map).FirstOrDefault((Thing t) => t is Corpse && ((Corpse)t).InnerPawn == victim);
			if (thing != null)
			{
				thing.SetForbiddenIfOutsideHomeArea();
			}
		}

		private static BodyPartRecord ExecuteCutPart(Pawn pawn)
		{
			BodyPartRecord bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Head);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.InsectHead);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Body);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			bodyPartRecord = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Torso);
			if (bodyPartRecord != null)
			{
				return bodyPartRecord;
			}
			Log.Error("No good slaughter cut part found for " + pawn);
			return pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).RandomElementByWeight((BodyPartRecord x) => x.coverageAbsWithChildren);
		}
	}
}
