using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Misc
{
	/// <summary> Use this class to play music on your map. It is very simple, randomizes music from Map Settings.</summary>
	public sealed class MapMusicPlayer : MonoBehaviour
	{
		[SerializeField] AudioSource ambienceAudioSource;

		void Start()
		{
			if (!ambienceAudioSource)
			{
				enabled = false;
				Debug.LogWarning("No ambience audio source added to Map Music Player component!");
			}

			StartCoroutine(CheckIsPlaying());
		}

		IEnumerator CheckIsPlaying()
		{
			while (true)
			{
				if (!ambienceAudioSource.isPlaying)
					StartPlay();

				yield return new WaitForSeconds(0.5f);
			}
		}

		void StartPlay()
		{
			if (!ambienceAudioSource || MatchSettings.CurrentMatchSettings == null || MatchSettings.CurrentMatchSettings.SelectedMap == null)
				return;

			var sounds = MatchSettings.CurrentMatchSettings.SelectedMap.ambientSoundsTracks;

			if (sounds.Length > 0)
			{
				ambienceAudioSource.clip = sounds[Random.Range(0, sounds.Length)];
				ambienceAudioSource.Play();
			}
		}
	}
}