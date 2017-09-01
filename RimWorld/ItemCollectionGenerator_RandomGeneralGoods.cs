using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_RandomGeneralGoods : ItemCollectionGenerator
	{
		private enum GoodsType
		{
			None,
			Meals,
			RawFood,
			Medicine,
			Drugs,
			Resources
		}

		private static Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>[] GoodsWeights = new Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>[]
		{
			new Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>(ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Meals, 1f),
			new Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>(ItemCollectionGenerator_RandomGeneralGoods.GoodsType.RawFood, 0.75f),
			new Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>(ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Medicine, 0.234f),
			new Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>(ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Drugs, 0.234f),
			new Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float>(ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Resources, 0.234f)
		};

		protected override ItemCollectionGeneratorParams RandomTestParams
		{
			get
			{
				ItemCollectionGeneratorParams randomTestParams = base.RandomTestParams;
				randomTestParams.count = Rand.RangeInclusive(10, 20);
				randomTestParams.techLevel = TechLevel.Transcendent;
				return randomTestParams;
			}
		}

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int count = parms.count;
			TechLevel techLevel = parms.techLevel;
			for (int i = 0; i < count; i++)
			{
				outThings.Add(this.GenerateSingle(techLevel));
			}
		}

		private Thing GenerateSingle(TechLevel techLevel)
		{
			ItemCollectionGenerator_RandomGeneralGoods.GoodsType first = ItemCollectionGenerator_RandomGeneralGoods.GoodsWeights.RandomElementByWeight((Pair<ItemCollectionGenerator_RandomGeneralGoods.GoodsType, float> x) => x.Second).First;
			switch (first)
			{
			case ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Meals:
				return this.RandomMeals(techLevel);
			case ItemCollectionGenerator_RandomGeneralGoods.GoodsType.RawFood:
				return this.RandomRawFood(techLevel);
			case ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Medicine:
				return this.RandomMedicine(techLevel);
			case ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Drugs:
				return this.RandomDrugs(techLevel);
			case ItemCollectionGenerator_RandomGeneralGoods.GoodsType.Resources:
				return this.RandomResources(techLevel);
			default:
				Log.Error("Goods type not handled: " + first);
				return null;
			}
		}

		private Thing RandomMeals(TechLevel techLevel)
		{
			ThingDef thingDef;
			if (techLevel.IsNeolithicOrWorse())
			{
				thingDef = ThingDefOf.Pemmican;
			}
			else
			{
				float value = Rand.Value;
				if (value < 0.5f)
				{
					thingDef = ThingDefOf.MealSimple;
				}
				else if ((double)value < 0.75)
				{
					thingDef = ThingDefOf.MealFine;
				}
				else
				{
					thingDef = ThingDefOf.MealSurvivalPack;
				}
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int num = Mathf.Min(thingDef.stackLimit, 10);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}

		private Thing RandomRawFood(TechLevel techLevel)
		{
			ThingDef thingDef;
			if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsNutritionGivingIngestible && !x.IsCorpse && ThingDefOf.Human.race.CanEverEat(x) && !x.HasComp(typeof(CompHatcher)) && x.techLevel <= techLevel && x.ingestible.preferability < FoodPreferability.MealAwful
			select x).TryRandomElement(out thingDef))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int max = Mathf.Min(thingDef.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomMedicine(TechLevel techLevel)
		{
			ThingDef herbalMedicine;
			if (techLevel.IsNeolithicOrWorse())
			{
				herbalMedicine = ThingDefOf.HerbalMedicine;
			}
			else if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsMedicine && x.techLevel <= techLevel
			select x).TryRandomElement(out herbalMedicine))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(herbalMedicine, null);
			int max = Mathf.Min(herbalMedicine.stackLimit, 20);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomDrugs(TechLevel techLevel)
		{
			ThingDef thingDef;
			if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsDrug && x.techLevel <= techLevel
			select x).TryRandomElement(out thingDef))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int max = Mathf.Min(thingDef.stackLimit, 25);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomResources(TechLevel techLevel)
		{
			ThingDef thingDef = BaseGenUtility.RandomCheapWallStuff(techLevel, false);
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int num = Mathf.Min(thingDef.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}
	}
}
