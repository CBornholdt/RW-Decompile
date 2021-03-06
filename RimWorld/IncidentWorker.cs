using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker
	{
		public IncidentDef def;

		public virtual float AdjustedChance
		{
			get
			{
				return this.def.baseChance;
			}
		}

		public bool CanFireNow(IIncidentTarget target)
		{
			if (!this.def.TargetAllowed(target))
			{
				return false;
			}
			if (GenDate.DaysPassed < this.def.earliestDay)
			{
				return false;
			}
			if (this.def.minPopulation > 0 && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists.Count<Pawn>() < this.def.minPopulation)
			{
				return false;
			}
			if (Find.Storyteller.difficulty.difficulty < this.def.minDifficulty)
			{
				return false;
			}
			if (this.def.allowedBiomes != null)
			{
				BiomeDef biome = Find.WorldGrid[target.Tile].biome;
				if (!this.def.allowedBiomes.Contains(biome))
				{
					return false;
				}
			}
			for (int i = 0; i < Find.Scenario.parts.Count; i++)
			{
				ScenPart_DisableIncident scenPart_DisableIncident = Find.Scenario.parts[i] as ScenPart_DisableIncident;
				if (scenPart_DisableIncident != null && scenPart_DisableIncident.Incident == this.def)
				{
					return false;
				}
			}
			Dictionary<IncidentDef, int> lastFireTicks = target.StoryState.lastFireTicks;
			int ticksGame = Find.TickManager.TicksGame;
			int num;
			if (lastFireTicks.TryGetValue(this.def, out num))
			{
				float num2 = (float)(ticksGame - num) / 60000f;
				if (num2 < this.def.minRefireDays)
				{
					return false;
				}
			}
			List<IncidentDef> refireCheckIncidents = this.def.RefireCheckIncidents;
			if (refireCheckIncidents != null)
			{
				for (int j = 0; j < refireCheckIncidents.Count; j++)
				{
					if (lastFireTicks.TryGetValue(refireCheckIncidents[j], out num))
					{
						float num3 = (float)(ticksGame - num) / 60000f;
						if (num3 < this.def.minRefireDays)
						{
							return false;
						}
					}
				}
			}
			return this.CanFireNowSub(target);
		}

		protected virtual bool CanFireNowSub(IIncidentTarget target)
		{
			return true;
		}

		public bool TryExecute(IncidentParms parms)
		{
			bool flag = this.TryExecuteWorker(parms);
			if (flag && this.def.tale != null)
			{
				Pawn pawn = null;
				if (parms.target is Caravan)
				{
					pawn = (parms.target as Caravan).RandomOwner();
				}
				else if (parms.target is Map)
				{
					pawn = (parms.target as Map).mapPawns.FreeColonistsSpawned.RandomElementWithFallback(null);
				}
				else if (parms.target is World)
				{
					pawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists.RandomElementWithFallback(null);
				}
				if (pawn != null)
				{
					TaleRecorder.RecordTale(this.def.tale, new object[]
					{
						pawn
					});
				}
			}
			return flag;
		}

		protected virtual bool TryExecuteWorker(IncidentParms parms)
		{
			Log.Error("Unimplemented incident " + this);
			return false;
		}

		protected void SendStandardLetter()
		{
			if (this.def.letterLabel.NullOrEmpty() || this.def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.");
			}
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, null);
		}

		protected void SendStandardLetter(GlobalTargetInfo target, params string[] textArgs)
		{
			if (this.def.letterLabel.NullOrEmpty() || this.def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.");
			}
			string text = string.Format(this.def.letterText, textArgs).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, target, null);
		}
	}
}
