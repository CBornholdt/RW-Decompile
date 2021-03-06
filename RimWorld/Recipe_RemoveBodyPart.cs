using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	internal class Recipe_RemoveBodyPart : Recipe_Surgery
	{
		private const float ViolationGoodwillImpact = 20f;

		[DebuggerHidden]
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			IEnumerable<BodyPartRecord> parts = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined);
			foreach (BodyPartRecord part in parts)
			{
				if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
				{
					yield return part;
				}
				if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
				{
					yield return part;
				}
				if (part != pawn.RaceProps.body.corePart && !part.def.dontSuggestAmputation && pawn.health.hediffSet.hediffs.Any((Hediff d) => !(d is Hediff_Injury) && d.def.isBad && d.Visible && d.Part == part))
				{
					yield return part;
				}
			}
		}

		public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
		{
			return pawn.Faction != billDoerFaction && HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
		{
			bool flag = MedicalRecipesUtility.IsClean(pawn, part);
			bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
			if (billDoer != null)
			{
				if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
				{
					billDoer,
					pawn
				});
				MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
				MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
			}
			DamageDef surgicalCut = DamageDefOf.SurgicalCut;
			int amount = 99999;
			pawn.TakeDamage(new DamageInfo(surgicalCut, amount, -1f, null, part, null, DamageInfo.SourceCategory.ThingOrUnknown));
			if (flag)
			{
				if (pawn.Dead)
				{
					ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.OrganHarvesting);
				}
				else
				{
					ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
				}
			}
			if (flag2)
			{
				pawn.Faction.AffectGoodwillWith(billDoer.Faction, -20f);
			}
		}

		public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.RaceProps.IsMechanoid || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
			{
				return RecipeDefOf.RemoveBodyPart.LabelCap;
			}
			BodyPartRemovalIntent bodyPartRemovalIntent = HealthUtility.PartRemovalIntent(pawn, part);
			if (bodyPartRemovalIntent != BodyPartRemovalIntent.Amputate)
			{
				if (bodyPartRemovalIntent != BodyPartRemovalIntent.Harvest)
				{
					throw new InvalidOperationException();
				}
				return "Harvest".Translate();
			}
			else
			{
				if (part.depth == BodyPartDepth.Inside || part.def.useDestroyedOutLabel)
				{
					return "RemoveOrgan".Translate();
				}
				return "Amputate".Translate();
			}
		}
	}
}
