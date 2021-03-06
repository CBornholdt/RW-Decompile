using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ApparelUtility
	{
		public struct LayerGroupPair
		{
			private readonly ApparelLayer layer;

			private readonly BodyPartGroupDef group;

			public LayerGroupPair(ApparelLayer layer, BodyPartGroupDef group)
			{
				this.layer = layer;
				this.group = group;
			}

			public override bool Equals(object rhs)
			{
				if (!(rhs is ApparelUtility.LayerGroupPair))
				{
					return false;
				}
				ApparelUtility.LayerGroupPair layerGroupPair = (ApparelUtility.LayerGroupPair)rhs;
				return layerGroupPair.layer == this.layer && layerGroupPair.group == this.group;
			}

			public override int GetHashCode()
			{
				int num = 17;
				num = num * 23 + this.layer.GetHashCode();
				return num * 23 + this.group.GetHashCode();
			}
		}

		public static bool CanWearTogether(ThingDef A, ThingDef B, BodyDef body)
		{
			bool flag = false;
			for (int i = 0; i < A.apparel.layers.Count; i++)
			{
				for (int j = 0; j < B.apparel.layers.Count; j++)
				{
					if (A.apparel.layers[i] == B.apparel.layers[j])
					{
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
			BodyPartGroupDef[] interferingBodyPartGroups = A.apparel.GetInterferingBodyPartGroups(body);
			BodyPartGroupDef[] interferingBodyPartGroups2 = B.apparel.GetInterferingBodyPartGroups(body);
			for (int k = 0; k < interferingBodyPartGroups.Length; k++)
			{
				if (interferingBodyPartGroups2.Contains(interferingBodyPartGroups[k]))
				{
					return false;
				}
			}
			return true;
		}

		public static void GenerateLayerGroupPairs(BodyDef body, ThingDef td, Action<ApparelUtility.LayerGroupPair> callback)
		{
			for (int i = 0; i < td.apparel.layers.Count; i++)
			{
				ApparelLayer layer = td.apparel.layers[i];
				BodyPartGroupDef[] interferingBodyPartGroups = td.apparel.GetInterferingBodyPartGroups(body);
				for (int j = 0; j < interferingBodyPartGroups.Length; j++)
				{
					callback(new ApparelUtility.LayerGroupPair(layer, interferingBodyPartGroups[j]));
				}
			}
		}

		public static bool HasPartsToWear(Pawn p, ThingDef apparel)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			bool flag = false;
			for (int j = 0; j < hediffs.Count; j++)
			{
				if (hediffs[j] is Hediff_MissingPart)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
			IEnumerable<BodyPartRecord> notMissingParts = p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined);
			List<BodyPartGroupDef> groups = apparel.apparel.bodyPartGroups;
			int i;
			for (i = 0; i < groups.Count; i++)
			{
				if (notMissingParts.Any((BodyPartRecord x) => x.IsInGroup(groups[i])))
				{
					return true;
				}
			}
			return false;
		}
	}
}
