using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class Hediff : IExposable
	{
		public HediffDef def;

		public int ageTicks;

		private int partIndex = -1;

		public ThingDef source;

		public BodyPartGroupDef sourceBodyPartGroup;

		public HediffDef sourceHediffDef;

		protected float severityInt;

		private bool recordedTale;

		protected bool causesNoPain;

		[Unsaved]
		public Pawn pawn;

		[Unsaved]
		private BodyPartRecord cachedPart;

		public virtual string LabelBase
		{
			get
			{
				return this.def.label;
			}
		}

		public string Label
		{
			get
			{
				string labelInBrackets = this.LabelInBrackets;
				return this.LabelBase + ((!labelInBrackets.NullOrEmpty()) ? (" (" + labelInBrackets + ")") : string.Empty);
			}
		}

		public string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public virtual Color LabelColor
		{
			get
			{
				return this.def.defaultLabelColor;
			}
		}

		public virtual string LabelInBrackets
		{
			get
			{
				return (this.CurStage != null && !this.CurStage.label.NullOrEmpty()) ? this.CurStage.label : null;
			}
		}

		public virtual string SeverityLabel
		{
			get
			{
				return (this.def.lethalSeverity > 0f) ? (this.Severity / this.def.lethalSeverity).ToStringPercent() : null;
			}
		}

		public virtual int UIGroupKey
		{
			get
			{
				return this.Label.GetHashCode();
			}
		}

		public virtual string TipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (StatDrawEntry current in HediffStatsUtility.SpecialDisplayStats(this.CurStage, this))
				{
					if (current.ShouldDisplay)
					{
						stringBuilder.AppendLine(current.LabelCap + ": " + current.ValueString);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public virtual HediffStage CurStage
		{
			get
			{
				return (!this.def.stages.NullOrEmpty<HediffStage>()) ? this.def.stages[this.CurStageIndex] : null;
			}
		}

		public virtual bool ShouldRemove
		{
			get
			{
				return this.Severity <= 0f;
			}
		}

		public virtual bool Visible
		{
			get
			{
				return this.CurStage == null || this.CurStage.everVisible;
			}
		}

		public virtual float BleedRate
		{
			get
			{
				return 0f;
			}
		}

		public bool Bleeding
		{
			get
			{
				return this.BleedRate > 1E-05f;
			}
		}

		public virtual float PainOffset
		{
			get
			{
				return (this.CurStage != null && !this.causesNoPain) ? this.CurStage.painOffset : 0f;
			}
		}

		public virtual float PainFactor
		{
			get
			{
				return (this.CurStage != null) ? this.CurStage.painFactor : 1f;
			}
		}

		public List<PawnCapacityModifier> CapMods
		{
			get
			{
				return (this.CurStage != null) ? this.CurStage.capMods : null;
			}
		}

		public virtual float SummaryHealthPercentImpact
		{
			get
			{
				return 0f;
			}
		}

		public virtual float TendPriority
		{
			get
			{
				float num = 0f;
				HediffStage curStage = this.CurStage;
				if (curStage != null && curStage.lifeThreatening)
				{
					num = Mathf.Max(num, 1f);
				}
				num = Mathf.Max(num, this.BleedRate * 1.5f);
				HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
				if (hediffComp_TendDuration != null && hediffComp_TendDuration.TProps.severityPerDayTended < 0f)
				{
					num = Mathf.Max(num, 0.025f);
				}
				return num;
			}
		}

		public virtual TextureAndColor StateIcon
		{
			get
			{
				return TextureAndColor.None;
			}
		}

		public virtual int CurStageIndex
		{
			get
			{
				if (this.def.stages == null)
				{
					return 0;
				}
				List<HediffStage> stages = this.def.stages;
				float severity = this.Severity;
				for (int i = stages.Count - 1; i >= 0; i--)
				{
					if (severity >= stages[i].minSeverity)
					{
						return i;
					}
				}
				return 0;
			}
		}

		public virtual float Severity
		{
			get
			{
				return this.severityInt;
			}
			set
			{
				bool flag = false;
				if (this.def.lethalSeverity > 0f && value >= this.def.lethalSeverity)
				{
					value = this.def.lethalSeverity;
					flag = true;
				}
				int curStageIndex = this.CurStageIndex;
				this.severityInt = Mathf.Clamp(value, this.def.minSeverity, this.def.maxSeverity);
				if (this.CurStageIndex != curStageIndex || flag)
				{
					this.pawn.health.Notify_HediffChanged(this);
					if (!this.pawn.Dead && this.pawn.needs.mood != null)
					{
						this.pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
					}
				}
			}
		}

		public BodyPartRecord Part
		{
			get
			{
				if (this.cachedPart == null && this.partIndex >= 0)
				{
					this.cachedPart = this.pawn.RaceProps.body.GetPartAtIndex(this.partIndex);
				}
				return this.cachedPart;
			}
			set
			{
				if (this.pawn == null)
				{
					Log.Error("Hediff: Cannot set Part without setting pawn first.");
					return;
				}
				if (value != null)
				{
					this.partIndex = this.pawn.RaceProps.body.GetIndexOfPart(value);
				}
				else
				{
					this.partIndex = -1;
				}
				this.cachedPart = value;
			}
		}

		public virtual bool TendableNow
		{
			get
			{
				return this.def.tendable && this.Severity > 0f && this.Visible && !this.FullyImmune() && !this.IsTended() && !this.IsOld();
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look<HediffDef>(ref this.def, "def");
			Scribe_Values.Look<int>(ref this.ageTicks, "ageTicks", 0, false);
			Scribe_Defs.Look<ThingDef>(ref this.source, "source");
			Scribe_Defs.Look<BodyPartGroupDef>(ref this.sourceBodyPartGroup, "sourceBodyPartGroup");
			Scribe_Defs.Look<HediffDef>(ref this.sourceHediffDef, "sourceHediffDef");
			Scribe_Values.Look<int>(ref this.partIndex, "partIndex", -1, false);
			Scribe_Values.Look<float>(ref this.severityInt, "severity", 0f, false);
			Scribe_Values.Look<bool>(ref this.recordedTale, "recordedTale", false, false);
			Scribe_Values.Look<bool>(ref this.causesNoPain, "causesNoPain", false, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.partIndex >= 0)
			{
				this.cachedPart = this.pawn.RaceProps.body.GetPartAtIndex(this.partIndex);
			}
		}

		public virtual void Tick()
		{
			this.ageTicks++;
			if (this.def.hediffGivers != null && this.pawn.IsHashIntervalTick(60))
			{
				for (int i = 0; i < this.def.hediffGivers.Count; i++)
				{
					this.def.hediffGivers[i].OnIntervalPassed(this.pawn, this);
				}
			}
			HediffStage curStage = this.CurStage;
			if (curStage != null)
			{
				if (curStage.hediffGivers != null && this.pawn.IsHashIntervalTick(60))
				{
					for (int j = 0; j < curStage.hediffGivers.Count; j++)
					{
						curStage.hediffGivers[j].OnIntervalPassed(this.pawn, this);
					}
				}
				if (curStage.mentalStateGivers != null && this.pawn.IsHashIntervalTick(60) && !this.pawn.InMentalState)
				{
					for (int k = 0; k < curStage.mentalStateGivers.Count; k++)
					{
						MentalStateGiver mentalStateGiver = curStage.mentalStateGivers[k];
						if (Rand.MTBEventOccurs(mentalStateGiver.mtbDays, 60000f, 60f))
						{
							this.pawn.mindState.mentalStateHandler.TryStartMentalState(mentalStateGiver.mentalState, null, false, false, null);
						}
					}
				}
				if (curStage.vomitMtbDays > 0f && this.pawn.IsHashIntervalTick(600) && Rand.MTBEventOccurs(curStage.vomitMtbDays, 60000f, 600f) && this.pawn.Spawned && this.pawn.Awake())
				{
					this.pawn.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, true, true, null, null);
				}
				Thought_Memory th;
				if (curStage.forgetMemoryThoughtMtbDays > 0f && this.pawn.needs.mood != null && this.pawn.IsHashIntervalTick(400) && Rand.MTBEventOccurs(curStage.forgetMemoryThoughtMtbDays, 60000f, 400f) && this.pawn.needs.mood.thoughts.memories.Memories.TryRandomElement(out th))
				{
					this.pawn.needs.mood.thoughts.memories.RemoveMemory(th);
				}
				if (!this.recordedTale && curStage.tale != null)
				{
					TaleRecorder.RecordTale(curStage.tale, new object[]
					{
						this.pawn
					});
					this.recordedTale = true;
				}
				if (curStage.destroyPart && this.Part != null && this.Part != this.pawn.RaceProps.body.corePart)
				{
					this.pawn.health.AddHediff(HediffDefOf.MissingBodyPart, this.Part, null);
				}
				if (curStage.deathMtbDays > 0f && this.pawn.IsHashIntervalTick(200) && Rand.MTBEventOccurs(curStage.deathMtbDays, 60000f, 200f))
				{
					bool flag = PawnUtility.ShouldSendNotificationAbout(this.pawn);
					Caravan caravan = this.pawn.GetCaravan();
					this.pawn.Kill(null);
					if (flag)
					{
						this.pawn.health.NotifyPlayerOfKilled(null, this, caravan);
					}
					return;
				}
			}
		}

		public virtual void PostMake()
		{
			this.Severity = Mathf.Max(this.Severity, this.def.initialSeverity);
			this.causesNoPain = (Rand.Value < this.def.chanceToCauseNoPain);
		}

		public virtual void PostAdd(DamageInfo? dinfo)
		{
		}

		public virtual void PostRemoved()
		{
			if (this.def.causesNeed != null && !this.pawn.Dead)
			{
				this.pawn.needs.AddOrRemoveNeedsAsAppropriate();
			}
		}

		public virtual void PostTick()
		{
		}

		public virtual void Tended(float quality, int batchPosition = 0)
		{
		}

		public virtual void Heal(float amount)
		{
			if (amount <= 0f)
			{
				return;
			}
			this.Severity -= amount;
			this.pawn.health.Notify_HediffChanged(this);
		}

		public virtual void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
		}

		public virtual bool TryMergeWith(Hediff other)
		{
			if (other == null || other.def != this.def || other.Part != this.Part)
			{
				return false;
			}
			this.Severity += other.Severity;
			this.ageTicks = 0;
			return true;
		}

		public virtual bool CauseDeathNow()
		{
			return this.def.lethalSeverity >= 0f && this.Severity >= this.def.lethalSeverity;
		}

		public virtual void Notify_PawnDied()
		{
		}

		public virtual string DebugString()
		{
			string text = "severity: " + this.Severity.ToString("F3") + ((this.Severity < this.def.maxSeverity) ? string.Empty : " (reached max)");
			if (this.TendableNow)
			{
				text = text + "\ntend priority: " + this.TendPriority;
			}
			return text.Indented();
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.def.defName,
				(this.cachedPart == null) ? string.Empty : (" " + this.cachedPart.def.label),
				" ticksSinceCreation=",
				this.ageTicks,
				")"
			});
		}

		public override int GetHashCode()
		{
			return this.def.GetHashCode();
		}
	}
}
