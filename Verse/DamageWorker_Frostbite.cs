using System;

namespace Verse
{
	public class DamageWorker_Frostbite : DamageWorker_AddInjury
	{
		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, ref DamageWorker.DamageResult result)
		{
			base.FinalizeAndAddInjury(pawn, totalDamage, dinfo, ref result);
		}
	}
}
