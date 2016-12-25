using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompMannable : ThingComp
	{
		private int lastManTick = -1;

		private Pawn lastManPawn;

		public bool MannedNow
		{
			get
			{
				return Find.TickManager.TicksGame - this.lastManTick <= 1;
			}
		}

		public Pawn ManningPawn
		{
			get
			{
				if (!this.MannedNow)
				{
					return null;
				}
				return this.lastManPawn;
			}
		}

		public CompProperties_Mannable Props
		{
			get
			{
				return (CompProperties_Mannable)this.props;
			}
		}

		public void ManForATick(Pawn pawn)
		{
			this.lastManTick = Find.TickManager.TicksGame;
			this.lastManPawn = pawn;
			pawn.mindState.lastMannedThing = this.parent;
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (pawn.RaceProps.ToolUser)
			{
				if (pawn.CanReserveAndReach(this.parent, PathEndMode.InteractionCell, Danger.Deadly, 1))
				{
					if (this.Props.manWorkType == WorkTags.None || pawn.story == null || !pawn.story.WorkTagIsDisabled(this.Props.manWorkType))
					{
						FloatMenuOption opt = new FloatMenuOption("OrderManThing".Translate(new object[]
						{
							this.parent.LabelShort
						}), delegate
						{
							Job newJob = new Job(JobDefOf.ManTurret, this.<>f__this.parent);
							this.pawn.drafter.TakeOrderedJob(newJob);
						}, MenuOptionPriority.Medium, null, null, 0f, null);
						yield return opt;
					}
				}
			}
		}
	}
}
