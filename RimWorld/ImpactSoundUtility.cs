using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class ImpactSoundUtility
	{
		public static void PlayImpactSound(Thing hitThing, ImpactSoundTypeDef ist, Map map)
		{
			if (ist == null)
			{
				return;
			}
			if (ist.playOnlyIfHitPawn && !(hitThing is Pawn))
			{
				return;
			}
			if (map == null)
			{
				Log.Warning("Can't play impact sound because map is null.");
				return;
			}
			SoundDef soundDef;
			if (hitThing.Stuff != null)
			{
				soundDef = hitThing.Stuff.stuffProps.soundImpactStuff;
			}
			else
			{
				soundDef = hitThing.def.soundImpactDefault;
			}
			if (soundDef.NullOrUndefined())
			{
				soundDef = SoundDefOf.BulletImpactGround;
			}
			soundDef.PlayOneShot(new TargetInfo(hitThing.PositionHeld, map, false));
		}
	}
}
