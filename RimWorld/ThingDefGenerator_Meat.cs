using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Meat
	{
		[DebuggerHidden]
		public static IEnumerable<ThingDef> ImpliedMeatDefs()
		{
			foreach (ThingDef sourceDef in DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>())
			{
				if (sourceDef.category == ThingCategory.Pawn)
				{
					if (sourceDef.race.useMeatFrom == null)
					{
						if (!sourceDef.race.IsFlesh)
						{
							DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(sourceDef.race, "meatDef", "Steel");
						}
						else
						{
							ThingDef d = new ThingDef();
							d.resourceReadoutPriority = ResourceCountPriority.Middle;
							d.category = ThingCategory.Item;
							d.thingClass = typeof(ThingWithComps);
							d.graphicData = new GraphicData();
							d.graphicData.graphicClass = typeof(Graphic_Single);
							d.useHitPoints = true;
							d.selectable = true;
							d.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
							d.altitudeLayer = AltitudeLayer.Item;
							d.stackLimit = 75;
							d.comps.Add(new CompProperties_Forbiddable());
							CompProperties_Rottable rotProps = new CompProperties_Rottable();
							rotProps.daysToRotStart = 2f;
							rotProps.rotDestroys = true;
							d.comps.Add(rotProps);
							d.comps.Add(new CompProperties_FoodPoisoningChance());
							d.tickerType = TickerType.Rare;
							d.SetStatBaseValue(StatDefOf.Beauty, -20f);
							d.alwaysHaulable = true;
							d.rotatable = false;
							d.pathCost = 15;
							d.drawGUIOverlay = true;
							d.socialPropernessMatters = true;
							d.category = ThingCategory.Item;
							d.description = "MeatDesc".Translate(new object[]
							{
								sourceDef.label
							});
							d.useHitPoints = true;
							d.SetStatBaseValue(StatDefOf.MaxHitPoints, 60f);
							d.SetStatBaseValue(StatDefOf.DeteriorationRate, 6f);
							d.SetStatBaseValue(StatDefOf.Mass, 0.03f);
							d.SetStatBaseValue(StatDefOf.Flammability, 0.5f);
							d.BaseMarketValue = ThingDefGenerator_Meat.GetMeatMarketValue(sourceDef);
							if (d.thingCategories == null)
							{
								d.thingCategories = new List<ThingCategoryDef>();
							}
							DirectXmlCrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(d.thingCategories, "MeatRaw");
							d.ingestible = new IngestibleProperties();
							d.ingestible.foodType = FoodTypeFlags.Meat;
							d.ingestible.preferability = FoodPreferability.RawBad;
							DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(d.ingestible, "tasteThought", ThoughtDefOf.AteRawFood.defName);
							d.ingestible.nutrition = 0.05f;
							d.ingestible.ingestEffect = EffecterDefOf.EatMeat;
							d.ingestible.ingestSound = SoundDef.Named("RawMeat_Eat");
							d.ingestible.specialThoughtDirect = sourceDef.race.FleshType.ateDirect;
							d.ingestible.specialThoughtAsIngredient = sourceDef.race.FleshType.ateAsIngredient;
							if (sourceDef.race.Humanlike)
							{
								d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/MeatHuman";
							}
							else
							{
								if (sourceDef.race.baseBodySize < 0.7f)
								{
									d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/MeatSmall";
								}
								else
								{
									d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/MeatBig";
								}
								d.graphicData.color = sourceDef.race.meatColor;
							}
							d.defName = sourceDef.defName + "_Meat";
							if (sourceDef.race.meatLabel.NullOrEmpty())
							{
								d.label = "MeatLabel".Translate(new object[]
								{
									sourceDef.label
								});
							}
							else
							{
								d.label = sourceDef.race.meatLabel;
							}
							d.ingestible.sourceDef = sourceDef;
							sourceDef.race.meatDef = d;
							yield return d;
						}
					}
				}
			}
		}

		private static float GetMeatMarketValue(ThingDef sourceDef)
		{
			if (sourceDef.race.Humanlike)
			{
				return 0.8f;
			}
			return 2f;
		}
	}
}
