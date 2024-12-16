using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Abilities;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[DisallowMultipleComponent]
	public sealed class AbilitiesModule : Module
	{
		public List<Ability> Abilities { get; } = new List<Ability>();

		new AudioSource audio;

		void Start()
		{
			for (var i = 0; i < SelfUnit.Data.unitAbilities.Count; i++)
				CheckForAbility(SelfUnit.Data.unitAbilities[i]);

			audio = SoundLibrary.MakeUnitAudio(SelfUnit);
		}

		void Update()
		{
			for (var i = 0; i < Abilities.Count; i++)
				Abilities[i].Update();
		}
		
		/// <summary> Checks for added ability of unit by ability data template. If there no ability, adds it. </summary>
		public Ability CheckForAbility(Ability template)
		{
			var ability = GetAbility(template);

			if (!ability)
				ability = AddAbility(template);
			
			return ability;
		}

		Ability AddAbility(Ability template)
		{
			var abilityInstance = Instantiate(template);
			abilityInstance.Init(SelfUnit);
			Abilities.Add(abilityInstance);

			return abilityInstance;
		}

		/// <summary> Gets ability of unit by ability data template. </summary>
		public Ability GetAbility(Ability abilityTemplate)
		{
			if (!abilityTemplate)
				return null;
			
			for (int i = 0; i < Abilities.Count; i++)
				if (Abilities[i].abilityName == abilityTemplate.abilityName)
					return Abilities[i];

			return null;
		}

		public Ability GetAbilityById(int id) => Abilities.Count > id ? Abilities[id] : null;
		
		public void PlayAbilityAudio(AudioClip clip)
		{
			if (!clip)
				return;
			
			audio.clip = clip;
			audio.Play();
		}
	}
}