using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Profile
{
	public static class CollectionsTracker
	{
		private static Dictionary<WeakReference, int> collections = new Dictionary<WeakReference, int>();

		public static void RememberCollections()
		{
			if (!MemoryTracker.AnythingTracked)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.");
				return;
			}
			CollectionsTracker.collections.Clear();
			foreach (WeakReference current in MemoryTracker.FoundCollections)
			{
				if (current.IsAlive)
				{
					ICollection collection = current.Target as ICollection;
					CollectionsTracker.collections[current] = collection.Count;
				}
			}
			Log.Message("Tracking " + CollectionsTracker.collections.Count + " collections.");
		}

		public static void LogGrownCollections()
		{
			if (!MemoryTracker.AnythingTracked)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.");
				return;
			}
			CollectionsTracker.collections.RemoveAll((KeyValuePair<WeakReference, int> kvp) => !kvp.Key.IsAlive || ((ICollection)kvp.Key.Target).Count <= kvp.Value);
			MemoryTracker.LogObjectHoldPathsFor(from kvp in CollectionsTracker.collections
			select kvp.Key, delegate(WeakReference elem)
			{
				ICollection collection = elem.Target as ICollection;
				return collection.Count - CollectionsTracker.collections[elem];
			});
			CollectionsTracker.collections.Clear();
		}
	}
}
