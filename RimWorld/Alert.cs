using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Alert
	{
		public const float Width = 154f;

		private const float TextWidth = 148f;

		public const float Height = 28f;

		private const float ItemPeekWidth = 30f;

		public const float InfoRectWidth = 330f;

		protected AlertPriority defaultPriority;

		protected string defaultLabel;

		protected string defaultExplanation;

		protected float lastBellTime = -1000f;

		private AlertBounce alertBounce;

		private static readonly Texture2D AlertBGTex = SolidColorMaterials.NewSolidColorTexture(Color.white);

		private static readonly Texture2D AlertBGTexHighlight = TexUI.HighlightTex;

		public virtual AlertPriority Priority
		{
			get
			{
				return this.defaultPriority;
			}
		}

		protected virtual Color BGColor
		{
			get
			{
				return Color.clear;
			}
		}

		public virtual bool Active
		{
			get
			{
				return this.GetReport().active;
			}
		}

		public abstract AlertReport GetReport();

		public virtual string GetExplanation()
		{
			return this.defaultExplanation;
		}

		public virtual string GetLabel()
		{
			return this.defaultLabel;
		}

		public void Notify_Started()
		{
			if (this.Priority >= AlertPriority.High)
			{
				if (this.alertBounce == null)
				{
					this.alertBounce = new AlertBounce();
				}
				this.alertBounce.DoAlertStartEffect();
				if (Time.timeSinceLevelLoad > 1f && Time.realtimeSinceStartup > this.lastBellTime + 0.5f)
				{
					SoundDefOf.TinyBell.PlayOneShotOnCamera(null);
					this.lastBellTime = Time.realtimeSinceStartup;
				}
			}
		}

		public virtual void AlertActiveUpdate()
		{
		}

		public virtual Rect DrawAt(float topY, bool minimized)
		{
			Text.Font = GameFont.Small;
			string label = this.GetLabel();
			float height = Text.CalcHeight(label, 148f);
			Rect rect = new Rect((float)UI.screenWidth - 154f, topY, 154f, height);
			if (this.alertBounce != null)
			{
				rect.x -= this.alertBounce.CalculateHorizontalOffset();
			}
			GUI.color = this.BGColor;
			GUI.DrawTexture(rect, Alert.AlertBGTex);
			GUI.color = Color.white;
			GUI.BeginGroup(rect);
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(new Rect(0f, 0f, 148f, height), label);
			GUI.EndGroup();
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, Alert.AlertBGTexHighlight);
			}
			if (Widgets.ButtonInvisible(rect, false) && this.GetReport().culprit.IsValid)
			{
				CameraJumper.TryJumpAndSelect(this.GetReport().culprit);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			return rect;
		}

		public void DrawInfoPane()
		{
			Alert.<DrawInfoPane>c__AnonStorey3E9 <DrawInfoPane>c__AnonStorey3E = new Alert.<DrawInfoPane>c__AnonStorey3E9();
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			<DrawInfoPane>c__AnonStorey3E.expString = this.GetExplanation();
			if (this.GetReport().culprit.IsValid)
			{
				<DrawInfoPane>c__AnonStorey3E.expString = <DrawInfoPane>c__AnonStorey3E.expString + "\n\n(" + "ClickToJumpToProblem".Translate() + ")";
			}
			float num = Text.CalcHeight(<DrawInfoPane>c__AnonStorey3E.expString, 310f);
			num += 20f;
			<DrawInfoPane>c__AnonStorey3E.infoRect = new Rect((float)UI.screenWidth - 154f - 330f - 8f, Mathf.Max(Mathf.Min(Event.current.mousePosition.y, (float)UI.screenHeight - num), 0f), 330f, num);
			if (<DrawInfoPane>c__AnonStorey3E.infoRect.yMax > (float)UI.screenHeight)
			{
				Alert.<DrawInfoPane>c__AnonStorey3E9 expr_E2_cp_0 = <DrawInfoPane>c__AnonStorey3E;
				expr_E2_cp_0.infoRect.y = expr_E2_cp_0.infoRect.y - ((float)UI.screenHeight - <DrawInfoPane>c__AnonStorey3E.infoRect.yMax);
			}
			if (<DrawInfoPane>c__AnonStorey3E.infoRect.y < 0f)
			{
				<DrawInfoPane>c__AnonStorey3E.infoRect.y = 0f;
			}
			Find.WindowStack.ImmediateWindow(138956, <DrawInfoPane>c__AnonStorey3E.infoRect, WindowLayer.GameUI, delegate
			{
				Text.Font = GameFont.Small;
				Rect rect = <DrawInfoPane>c__AnonStorey3E.infoRect.AtZero();
				Widgets.DrawWindowBackground(rect);
				Rect position = rect.ContractedBy(10f);
				GUI.BeginGroup(position);
				Widgets.Label(new Rect(0f, 0f, position.width, position.height), <DrawInfoPane>c__AnonStorey3E.expString);
				GUI.EndGroup();
			}, false, false, 1f);
		}
	}
}
