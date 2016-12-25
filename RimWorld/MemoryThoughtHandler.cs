using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class MemoryThoughtHandler : IExposable
	{
		public Pawn pawn;

		private List<Thought_Memory> memories = new List<Thought_Memory>();

		public List<Thought_Memory> Memories
		{
			get
			{
				return this.memories;
			}
		}

		public MemoryThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Thought_Memory>(ref this.memories, "memories", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = this.memories.Count - 1; i > 0; i--)
				{
					if (this.memories[i].def == null)
					{
						this.memories.RemoveAt(i);
					}
					else
					{
						this.memories[i].pawn = this.pawn;
					}
				}
			}
		}

		public void MemoryThoughtInterval()
		{
			for (int i = 0; i < this.memories.Count; i++)
			{
				this.memories[i].ThoughtInterval();
			}
			this.RemoveExpiredMemoryThoughts();
		}

		private void RemoveExpiredMemoryThoughts()
		{
			for (int i = this.memories.Count - 1; i >= 0; i--)
			{
				Thought_Memory thought_Memory = this.memories[i];
				if (thought_Memory.ShouldDiscard)
				{
					this.RemoveMemoryThought(thought_Memory);
					if (thought_Memory.def.nextThought != null)
					{
						this.TryGainMemoryThought(thought_Memory.def.nextThought, null);
					}
				}
			}
		}

		public void TryGainMemoryThought(ThoughtDef def, Pawn otherPawn = null)
		{
			if (!def.IsMemory)
			{
				Log.Warning(def + " is not a memory thought.");
				return;
			}
			this.TryGainMemoryThought((Thought_Memory)ThoughtMaker.MakeThought(def), otherPawn);
		}

		public void TryGainMemoryThought(Thought_Memory newThought, Pawn otherPawn = null)
		{
			if (!this.pawn.needs.mood.thoughts.CanGetThought(newThought.def))
			{
				return;
			}
			newThought.pawn = this.pawn;
			if (otherPawn != null)
			{
				Thought_MemorySocial thought_MemorySocial = newThought as Thought_MemorySocial;
				if (thought_MemorySocial != null)
				{
					thought_MemorySocial.SetOtherPawn(otherPawn);
				}
				newThought.subject = otherPawn.LabelShort;
			}
			if (!newThought.TryMergeWithExistingThought())
			{
				this.memories.Add(newThought);
			}
			if (newThought.def.stackLimitPerPawn >= 0)
			{
				Thought_MemorySocial thought_MemorySocial2 = (Thought_MemorySocial)newThought;
				while (this.NumSocialMemoryThoughtsInGroup(thought_MemorySocial2, thought_MemorySocial2.otherPawnID) > newThought.def.stackLimitPerPawn)
				{
					this.RemoveMemoryThought(this.OldestSocialMemoryThoughtInGroup(newThought, thought_MemorySocial2.otherPawnID));
				}
			}
			if (newThought.def.stackLimit >= 0)
			{
				while (this.NumMemoryThoughtsInGroup(newThought) > newThought.def.stackLimit)
				{
					this.RemoveMemoryThought(this.OldestMemoryThoughtInGroup(newThought));
				}
			}
			if (newThought.def.thoughtToMake != null)
			{
				this.TryGainMemoryThought(newThought.def.thoughtToMake, otherPawn);
			}
		}

		public int NumSocialMemoryThoughtsInGroup(Thought_MemorySocial group, int otherPawnID)
		{
			int num = 0;
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].GroupsWith(group))
				{
					Thought_MemorySocial thought_MemorySocial = this.memories[i] as Thought_MemorySocial;
					if (thought_MemorySocial != null && thought_MemorySocial.otherPawnID == otherPawnID)
					{
						num++;
					}
				}
			}
			return num;
		}

		public Thought_Memory OldestMemoryThoughtInGroup(Thought_Memory group)
		{
			Thought_Memory result = null;
			int num = -9999;
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].GroupsWith(group))
				{
					Thought_Memory thought_Memory = this.memories[i];
					if (thought_Memory != null && thought_Memory.age > num)
					{
						result = thought_Memory;
						num = thought_Memory.age;
					}
				}
			}
			return result;
		}

		public Thought_MemorySocial OldestSocialMemoryThoughtInGroup(Thought_Memory group, int otherPawnID)
		{
			Thought_MemorySocial result = null;
			int num = -9999;
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].GroupsWith(group))
				{
					Thought_MemorySocial thought_MemorySocial = this.memories[i] as Thought_MemorySocial;
					if (thought_MemorySocial != null && thought_MemorySocial.otherPawnID == otherPawnID && thought_MemorySocial.age > num)
					{
						result = thought_MemorySocial;
						num = thought_MemorySocial.age;
					}
				}
			}
			return result;
		}

		public void RemoveMemoryThought(Thought_Memory th)
		{
			if (!this.memories.Remove(th))
			{
				Log.Warning("Tried to remove memory thought of def " + th.def.defName + " but it's not here.");
				return;
			}
		}

		public int NumMemoryThoughtsInGroup(Thought_Memory group)
		{
			int num = 0;
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].GroupsWith(group))
				{
					num++;
				}
			}
			return num;
		}

		public void RemoveSocialMemoryThoughts(ThoughtDef def, Pawn otherPawn)
		{
			if (!typeof(Thought_MemorySocial).IsAssignableFrom(def.ThoughtClass))
			{
				Log.Warning(def + " is not a memory social thought.");
				return;
			}
			while (true)
			{
				Thought_Memory thought_Memory = this.memories.Find(delegate(Thought_Memory x)
				{
					if (x.def != def)
					{
						return false;
					}
					Thought_MemorySocial thought_MemorySocial = (Thought_MemorySocial)x;
					return thought_MemorySocial.otherPawnID == otherPawn.thingIDNumber;
				});
				if (thought_Memory == null)
				{
					break;
				}
				this.RemoveMemoryThought(thought_Memory);
			}
		}

		public void RemoveMemoryThoughtsOfDef(ThoughtDef def)
		{
			if (!def.IsMemory)
			{
				Log.Warning(def + " is not a memory thought.");
				return;
			}
			while (true)
			{
				Thought_Memory thought_Memory = this.memories.Find((Thought_Memory x) => x.def == def);
				if (thought_Memory == null)
				{
					break;
				}
				this.RemoveMemoryThought(thought_Memory);
			}
		}
	}
}
