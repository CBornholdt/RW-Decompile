using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public class HediffComp_HealOldWounds : HediffComp
	{
		private int ticksToHeal;

		public HediffCompProperties_HealOldWounds Props
		{
			get
			{
				return (HediffCompProperties_HealOldWounds)this.props;
			}
		}

		public override void CompPostMake()
		{
			base.CompPostMake();
			this.ResetTicksToHeal();
		}

		private void ResetTicksToHeal()
		{
			this.ticksToHeal = Rand.Range(15, 30) * 60000;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			this.ticksToHeal--;
			if (this.ticksToHeal <= 0)
			{
				this.TryHealRandomOldWound();
				this.ResetTicksToHeal();
			}
		}

		private void TryHealRandomOldWound()
		{
			Hediff hediff;
			if (!base.Pawn.health.hediffSet.hediffs.Where(new Func<Hediff, bool>(HediffUtility.IsOld)).TryRandomElement(out hediff))
			{
				return;
			}
			hediff.Severity = 0f;
			if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				Messages.Message("MessageOldWoundHealed".Translate(new object[]
				{
					this.parent.Label,
					base.Pawn.LabelShort,
					hediff.Label
				}), MessageTypeDefOf.PositiveEvent);
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksToHeal, "ticksToHeal", 0, false);
		}

		public override string CompDebugString()
		{
			return "ticksToHeal: " + this.ticksToHeal;
		}
	}
}
