using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class DebugTools_Health
	{
		public static List<DebugMenuOption> Options_RestorePart(Pawn p)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (BodyPartRecord current in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
			{
				BodyPartRecord localPart = current;
				list.Add(new DebugMenuOption(localPart.def.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					p.health.RestorePart(localPart, null, true);
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_ApplyDamage()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (DamageDef current in DefDatabase<DamageDef>.AllDefs)
			{
				DamageDef localDef = current;
				list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault<Pawn>();
					if (pawn != null)
					{
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_Damage_BodyParts(pawn, localDef)));
					}
				}));
			}
			return list;
		}

		private static List<DebugMenuOption> Options_Damage_BodyParts(Pawn p, DamageDef def)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("(no body part)", DebugMenuOptionMode.Action, delegate
			{
				p.TakeDamage(new DamageInfo(def, 5, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
			}));
			foreach (BodyPartRecord current in p.RaceProps.body.AllParts)
			{
				BodyPartRecord localPart = current;
				list.Add(new DebugMenuOption(localPart.def.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					Thing arg_30_0 = p;
					DamageDef def2 = def;
					int amount = 5;
					BodyPartRecord localPart = localPart;
					arg_30_0.TakeDamage(new DamageInfo(def2, amount, -1f, null, localPart, null, DamageInfo.SourceCategory.ThingOrUnknown));
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_AddHediff()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type current in (from t in typeof(Hediff).AllSubclasses()
			where !t.IsAbstract
			select t).Concat(Gen.YieldSingle<Type>(typeof(Hediff))))
			{
				Type localDiffType = current;
				if (localDiffType != typeof(Hediff_Injury))
				{
					list.Add(new DebugMenuOption(localDiffType.ToString(), DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_HediffsDefs(localDiffType)));
					}));
				}
			}
			return list;
		}

		private static List<DebugMenuOption> Options_HediffsDefs(Type diffType)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (HediffDef current in from d in DefDatabase<HediffDef>.AllDefs
			where d.hediffClass == diffType
			select d)
			{
				HediffDef localDef = current;
				list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					Pawn pawn = Find.VisibleMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>().FirstOrDefault<Pawn>();
					if (pawn != null)
					{
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_Hediff_BodyParts(pawn, localDef)));
						DebugTools.curTool = null;
					}
				}));
			}
			return list;
		}

		private static List<DebugMenuOption> Options_Hediff_BodyParts(Pawn p, HediffDef def)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("(no body part)", DebugMenuOptionMode.Action, delegate
			{
				p.health.AddHediff(def, null, null);
			}));
			foreach (BodyPartRecord current in p.RaceProps.body.AllParts)
			{
				BodyPartRecord localPart = current;
				list.Add(new DebugMenuOption(localPart.def.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					p.health.AddHediff(def, localPart, null);
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_RemoveHediff(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Hediff current in pawn.health.hediffSet.hediffs)
			{
				Hediff localH = current;
				list.Add(new DebugMenuOption(localH.LabelCap + ((localH.Part == null) ? string.Empty : (" (" + localH.Part.def + ")")), DebugMenuOptionMode.Action, delegate
				{
					pawn.health.RemoveHediff(localH);
				}));
			}
			return list;
		}
	}
}
