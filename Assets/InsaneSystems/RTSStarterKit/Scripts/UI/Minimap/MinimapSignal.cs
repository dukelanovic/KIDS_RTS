using UnityEngine;
using System.Collections.Generic;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class MinimapSignal : MonoBehaviour
	{
		const float SoundWaitTime = 7f;

		public bool Enabled { get; set; } = true;

		Minimap minimap;
		AudioSource audioSource;

		float soundTimer;

		readonly List<UnitTimer> attackedUnits = new List<UnitTimer>();

		void Start()
		{
			minimap = FindObjectOfType<Minimap>();

			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = GameController.Instance.MainStorage.soundLibrary.GetSoundByPath("UI/MinimapAttackSound");
			audioSource.spatialBlend = 0f;
		}

		void Update()
		{
			if (!Enabled)
				return;
			
			if (soundTimer > 0)
				soundTimer -= Time.deltaTime;

			for (int i = attackedUnits.Count -1; i >= 0; i--)
			{
				attackedUnits[i].Tick();

				if (attackedUnits[i].IsFinished())
					attackedUnits.RemoveAt(i);
			}
		}

		public void ShowFor(Unit unit)
		{
			if (attackedUnits.Find(ut => ut.Unit == unit) != null)
				return;

			attackedUnits.Add(new UnitTimer(unit, 5f));

			var positionOnMap = minimap.GetUnitOnMapPoint(unit, true);
			var spawnedSignal = Instantiate(GameController.Instance.MainStorage.minimapSignalTemplate, minimap.IconsPanel);
			spawnedSignal.GetComponent<RectTransform>().anchoredPosition = positionOnMap;

			if (soundTimer <= 0)
			{
				soundTimer = SoundWaitTime;
				audioSource.Play();
			}
		}
	}

	/// <summary> This timer class needed to count, which unit was attacked N seconds before. It prevents minimap signal every time unit being damaged.</summary>
	public class UnitTimer
	{
		public Unit Unit { get; }
		public float Timer { get; private set; }

		public UnitTimer(Unit unit, float time)
		{
			Unit = unit;
			Timer = time;
		}
		
		public void Tick() => Timer -= Time.deltaTime;
		public bool IsFinished() => Timer <= 0;
	}
}