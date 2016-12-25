using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class TendUtility
	{
		public const float NoMedicinePotency = 0.5f;

		private const float ChanceToDevelopBondRelationOnTended = 0.01f;

		private static List<Hediff_MissingPart> bleedingStumps = new List<Hediff_MissingPart>();

		private static List<Hediff> otherHediffs = new List<Hediff>();

		public static void DoTend(Pawn doctor, Pawn patient, Medicine medicine)
		{
			if (!patient.health.HasHediffsNeedingTend(false))
			{
				return;
			}
			if (medicine != null && medicine.Destroyed)
			{
				Log.Warning("Tried to use destroyed medicine.");
				medicine = null;
			}
			float num = (medicine == null) ? 0f : medicine.def.GetStatValueAbstract(StatDefOf.MedicalPotency, null);
			float num2 = (medicine == null) ? 0.5f : num;
			if (doctor != null)
			{
				num2 *= doctor.GetStatValue(StatDefOf.BaseHealingQuality, true);
			}
			if (patient.InBed())
			{
				num2 *= patient.CurrentBed().GetStatValue(StatDefOf.MedicalTreatmentQualityFactor, true);
			}
			if (patient.health.hediffSet.GetInjuriesTendable().Any<Hediff_Injury>())
			{
				float num3 = 0f;
				int num4 = 0;
				foreach (Hediff_Injury current in from x in patient.health.hediffSet.GetInjuriesTendable()
				orderby x.Severity descending
				select x)
				{
					float num5 = Mathf.Min(current.Severity, 20f);
					if (num3 + num5 > 20f)
					{
						break;
					}
					num3 += num5;
					current.Tended(num2, num4);
					if (medicine == null)
					{
						break;
					}
					num4++;
				}
			}
			else
			{
				TendUtility.bleedingStumps.Clear();
				List<Hediff_MissingPart> missingPartsCommonAncestors = patient.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
				{
					if (missingPartsCommonAncestors[i].IsFresh)
					{
						TendUtility.bleedingStumps.Add(missingPartsCommonAncestors[i]);
					}
				}
				if (TendUtility.bleedingStumps.Count > 0)
				{
					TendUtility.bleedingStumps.RandomElement<Hediff_MissingPart>().IsFresh = false;
					TendUtility.bleedingStumps.Clear();
				}
				else
				{
					TendUtility.otherHediffs.Clear();
					TendUtility.otherHediffs.AddRange(patient.health.hediffSet.GetTendableNonInjuryNonMissingPartHediffs());
					Hediff hediff;
					if (TendUtility.otherHediffs.TryRandomElement(out hediff))
					{
						HediffCompProperties hediffCompProperties = hediff.def.CompPropsFor(typeof(HediffComp_Tendable));
						if (hediffCompProperties != null && hediffCompProperties.tendAllAtOnce)
						{
							int num6 = 0;
							for (int j = 0; j < TendUtility.otherHediffs.Count; j++)
							{
								if (TendUtility.otherHediffs[j].def == hediff.def)
								{
									TendUtility.otherHediffs[j].Tended(num2, num6);
									num6++;
								}
							}
						}
						else
						{
							hediff.Tended(num2, 0);
						}
					}
					TendUtility.otherHediffs.Clear();
				}
			}
			if (doctor != null && patient.HostFaction == null && patient.Faction != null && patient.Faction != doctor.Faction)
			{
				patient.Faction.AffectGoodwillWith(doctor.Faction, 0.3f);
			}
			if (doctor != null && doctor.RaceProps.Humanlike && patient.RaceProps.Animal && RelationsUtility.TryDevelopBondRelation(doctor, patient, 0.01f) && doctor.Faction != null && doctor.Faction != patient.Faction)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(doctor, patient, 1f, false);
			}
			patient.records.Increment(RecordDefOf.TimesTendedTo);
			if (doctor != null)
			{
				doctor.records.Increment(RecordDefOf.TimesTendedOther);
			}
			if (medicine != null)
			{
				if ((patient.Spawned || (doctor != null && doctor.Spawned)) && num > 0.9f)
				{
					SoundDef.Named("TechMedicineUsed").PlayOneShot(patient.Position);
				}
				if (medicine.stackCount > 1)
				{
					medicine.stackCount--;
				}
				else if (!medicine.Destroyed)
				{
					medicine.Destroy(DestroyMode.Vanish);
				}
			}
		}
	}
}
