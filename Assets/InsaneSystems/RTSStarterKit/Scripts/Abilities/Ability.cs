using InsaneSystems.RTSStarterKit.UI;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	/// <summary> Base class for any ability. Derive from it. </summary>
	public abstract class Ability : ScriptableObject
	{
		[Tooltip("Ability name. Shown in game interface.")]
		public string abilityName;
		[Tooltip("Ability icon image. Shown in game interface.")]
		public Sprite icon;
		[Sound] public AudioClip soundToPlayOnUse;
		
		[Tooltip("Is ability can be used by default? If false, it can be enabled only from other code or ability (upgrades for example).")]
		public bool isActiveByDefault = true;
		
		// realtime gameplay parameters, changes every run
		public Unit UnitOwner { get; protected set; }
		public bool IsActive { get; set; }
		protected AbilitiesModule UnitOwnerAbilities { get; private set; }
		
		public void Init(Unit unitOwner)
		{
			UnitOwner = unitOwner;
			UnitOwnerAbilities = unitOwner.GetModule<AbilitiesModule>();
			
			IsActive = isActiveByDefault;
			
			InitAction();
		}

		/// <summary> Override it for you ability initialization. It being called once on unit spawn. </summary>
		protected virtual void InitAction() { }

		public void Update()
		{
			UpdateAction();
		}
		
		/// <summary> Override it for here your ability action. </summary>
		protected virtual void UpdateAction() { }

		public void StartUse()
		{
			if (!CanUse()) 
				return;

			UnitOwnerAbilities.PlayAbilityAudio(soundToPlayOnUse);
			StartUseAction();
			
			UIController.Instance.UnitAbilities.Redraw();
		}
		
		protected virtual void StartUseAction() { }
		
		/// <summary> Override it to make custom usage condition. </summary>
		public virtual bool CanUse() => true;
	}
}