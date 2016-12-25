using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Toils_Tend
	{
		public static Toil PickupMedicine(TargetIndex ind, Pawn injured)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(ind).Thing;
				int medicineCountToFullyHeal = Medicine.GetMedicineCountToFullyHeal(injured);
				curJob.maxNumToCarry = medicineCountToFullyHeal;
				int count = Mathf.Min(thing.stackCount, medicineCountToFullyHeal);
				actor.carrier.TryStartCarry(thing, count);
				if (thing.Spawned)
				{
					Find.Reservations.Release(thing, actor);
				}
				curJob.SetTarget(ind, actor.carrier.CarriedThing);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		public static Toil FinalizeTend(Pawn patient)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Medicine medicine = (Medicine)actor.jobs.curJob.targetB.Thing;
				float num = (!patient.RaceProps.Animal) ? 500f : 175f;
				float num2 = (medicine != null) ? medicine.def.MedicineTendXpGainFactor : 0.5f;
				actor.skills.Learn(SkillDefOf.Medicine, num * num2);
				TendUtility.DoTend(actor, patient, medicine);
				if (medicine != null && medicine.Destroyed)
				{
					actor.CurJob.SetTarget(TargetIndex.B, TargetInfo.Invalid);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}
