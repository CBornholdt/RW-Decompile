using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Designator_Build : Designator_Place
	{
		private const float DragPriceDrawNumberX = 29f;

		protected BuildableDef entDef;

		private ThingDef stuffDef;

		private bool writeStuff;

		private static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);

		private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);

		public override BuildableDef PlacingDef
		{
			get
			{
				return this.entDef;
			}
		}

		public override string Label
		{
			get
			{
				ThingDef thingDef = this.entDef as ThingDef;
				if (thingDef != null && this.writeStuff)
				{
					return GenLabel.ThingLabel(thingDef, this.stuffDef, 1);
				}
				return this.entDef.label;
			}
		}

		public override string Desc
		{
			get
			{
				return this.entDef.description;
			}
		}

		public override Color IconDrawColor
		{
			get
			{
				if (this.stuffDef != null)
				{
					return this.stuffDef.stuffProps.color;
				}
				return this.entDef.IconDrawColor;
			}
		}

		public override bool Visible
		{
			get
			{
				if (DebugSettings.godMode)
				{
					return true;
				}
				if (this.entDef.researchPrerequisites != null)
				{
					for (int i = 0; i < this.entDef.researchPrerequisites.Count; i++)
					{
						if (!this.entDef.researchPrerequisites[i].IsFinished)
						{
							return false;
						}
					}
				}
				if (this.entDef.buildingPrerequisites != null)
				{
					for (int j = 0; j < this.entDef.buildingPrerequisites.Count; j++)
					{
						if (!Find.ListerBuildings.ColonistsHaveBuilding(this.entDef.buildingPrerequisites[j]))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public override int DraggableDimensions
		{
			get
			{
				return this.entDef.placingDraggableDimensions;
			}
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public override string HighlightTag
		{
			get
			{
				if (this.cachedHighlightTag == null && this.tutorTag != null)
				{
					this.cachedHighlightTag = "Designator-Build-" + this.tutorTag;
				}
				return this.cachedHighlightTag;
			}
		}

		public Designator_Build(BuildableDef entDef)
		{
			this.entDef = entDef;
			this.icon = entDef.uiIcon;
			this.hotKey = entDef.designationHotKey;
			this.tutorTag = entDef.defName;
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.uiIconPath.NullOrEmpty())
			{
				this.iconProportions = thingDef.graphicData.drawSize;
				this.iconDrawScale = GenUI.IconDrawScale(thingDef);
			}
			else
			{
				this.iconProportions = new Vector2(1f, 1f);
				this.iconDrawScale = 1f;
			}
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null)
			{
				this.iconTexCoords = new Rect(0f, 0f, Designator_Build.TerrainTextureCroppedSize.x / (float)this.icon.width, Designator_Build.TerrainTextureCroppedSize.y / (float)this.icon.height);
			}
			this.ResetStuffToDefault();
		}

		public void ResetStuffToDefault()
		{
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null && thingDef.MadeFromStuff)
			{
				this.stuffDef = GenStuff.DefaultStuffFor(thingDef);
			}
		}

		public override void DrawMouseAttachments()
		{
			base.DrawMouseAttachments();
			if (!ArchitectCategoryTab.InfoRect.Contains(GenUI.AbsMousePosition()))
			{
				DesignationDragger dragger = DesignatorManager.Dragger;
				int num;
				if (dragger.Dragging)
				{
					num = dragger.DragCells.Count<IntVec3>();
				}
				else
				{
					num = 1;
				}
				float num2 = 0f;
				Vector2 vector = Event.current.mousePosition + Designator_Build.DragPriceDrawOffset;
				List<ThingCount> list = this.entDef.CostListAdjusted(this.stuffDef, true);
				for (int i = 0; i < list.Count; i++)
				{
					ThingCount thingCount = list[i];
					float y = vector.y + num2;
					Rect position = new Rect(vector.x, y, 27f, 27f);
					GUI.DrawTexture(position, thingCount.thingDef.uiIcon);
					Rect rect = new Rect(vector.x + 29f, y, 999f, 29f);
					int num3 = num * thingCount.count;
					string text = num3.ToString();
					if (Find.ResourceCounter.GetCount(thingCount.thingDef) < num3)
					{
						GUI.color = Color.red;
						text = text + " (" + "NotEnoughStoredLower".Translate() + ")";
					}
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect, text);
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = Color.white;
					num2 += 29f;
				}
			}
		}

		public override void ProcessInput(Event ev)
		{
			if (!base.CheckCanInteract())
			{
				return;
			}
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef == null || !thingDef.MadeFromStuff)
			{
				base.ProcessInput(ev);
			}
			else
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef current in Find.ResourceCounter.AllCountedAmounts.Keys)
				{
					if (current.IsStuff && current.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || Find.ListerThings.ThingsOfDef(current).Count > 0))
					{
						ThingDef localStuffDef = current;
						string labelCap = localStuffDef.LabelCap;
						list.Add(new FloatMenuOption(labelCap, delegate
						{
							this.ProcessInput(ev);
							DesignatorManager.Select(this);
							this.stuffDef = localStuffDef;
							this.writeStuff = true;
						}, MenuOptionPriority.Medium, null, null, 0f, null)
						{
							tutorTag = "SelectStuff-" + thingDef.defName + "-" + localStuffDef.defName
						});
					}
				}
				if (list.Count == 0)
				{
					Messages.Message("NoStuffsToBuildWith".Translate(), MessageSound.RejectInput);
				}
				else
				{
					FloatMenu floatMenu = new FloatMenu(list);
					floatMenu.vanishIfMouseDistant = true;
					Find.WindowStack.Add(floatMenu);
					DesignatorManager.Select(this);
				}
			}
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			return GenConstruct.CanPlaceBlueprintAt(this.entDef, c, this.placingRot, DebugSettings.godMode, null);
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
			{
				return;
			}
			if (DebugSettings.godMode || this.entDef.GetStatValueAbstract(StatDefOf.WorkToMake, this.stuffDef) == 0f)
			{
				if (this.entDef is TerrainDef)
				{
					Find.TerrainGrid.SetTerrain(c, (TerrainDef)this.entDef);
				}
				else
				{
					Thing thing = ThingMaker.MakeThing((ThingDef)this.entDef, this.stuffDef);
					thing.SetFactionDirect(Faction.OfPlayer);
					GenSpawn.Spawn(thing, c, this.placingRot);
				}
			}
			else
			{
				GenSpawn.WipeExistingThings(c, this.placingRot, this.entDef.blueprintDef, true);
				GenConstruct.PlaceBlueprintForBuild(this.entDef, c, this.placingRot, Faction.OfPlayer, this.stuffDef);
			}
			MoteMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, this.placingRot, this.entDef.Size));
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null && thingDef.IsOrbitalTradeBeacon)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);
			}
			if (TutorSystem.TutorialMode)
			{
				TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));
			}
		}

		public override void SelectedUpdate()
		{
			base.SelectedUpdate();
			IntVec3 intVec = Gen.MouseCell();
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null && (thingDef.EverTransmitsPower || thingDef.ConnectToPower))
			{
				OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
				if (thingDef.ConnectToPower)
				{
					CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(intVec, null);
					if (compPower != null)
					{
						PowerNetGraphics.RenderAnticipatedWirePieceConnecting(intVec, compPower.parent);
					}
				}
			}
		}

		public override void DrawPanelReadout(ref float curY, float width)
		{
			if (this.entDef.costStuffCount <= 0 && this.stuffDef != null)
			{
				this.stuffDef = null;
			}
			Text.Font = GameFont.Tiny;
			List<ThingCount> list = this.entDef.CostListAdjusted(this.stuffDef, false);
			for (int i = 0; i < list.Count; i++)
			{
				ThingCount thingCount = list[i];
				Texture2D image;
				if (thingCount.thingDef == null)
				{
					image = BaseContent.BadTex;
				}
				else
				{
					image = thingCount.thingDef.uiIcon;
				}
				GUI.DrawTexture(new Rect(0f, curY, 20f, 20f), image);
				if (thingCount.thingDef != null && thingCount.thingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted && Find.ResourceCounter.GetCount(thingCount.thingDef) < thingCount.count)
				{
					GUI.color = Color.red;
				}
				Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingCount.count.ToString());
				GUI.color = Color.white;
				string text;
				if (thingCount.thingDef == null)
				{
					text = "(" + "UnchosenStuff".Translate() + ")";
				}
				else
				{
					text = thingCount.thingDef.LabelCap;
				}
				float width2 = width - 60f;
				float num = Text.CalcHeight(text, width2) - 2f;
				Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
				curY += num;
			}
			ThingDef thingDef = this.entDef as ThingDef;
			if (thingDef != null)
			{
				Widgets.InfoCardButton(0f, curY, thingDef, this.stuffDef);
			}
			else
			{
				Widgets.InfoCardButton(0f, curY, this.entDef);
			}
			curY += 24f;
		}

		public void SetStuffDef(ThingDef stuffDef)
		{
			this.stuffDef = stuffDef;
		}
	}
}
