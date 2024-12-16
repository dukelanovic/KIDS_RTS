using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	[CreateAssetMenu(fileName = "ChangeWeapon", menuName = Storage.AssetName + "/Abilities/Change Weapon")]
	public class ChangeWeapon : Ability
	{
		[Header("Custom weapon ability")]
		[Tooltip("Attack distance of this weapon. If you set 0, it will be default unit attack distance. Other for next same parameters.")]
		public float newAttackRange;
		public float newAttackReloadTime;
		public float newAttackDamage;
		[Tooltip("Put here second weapon change ability, which should became active after using this - to change weapon to previous.")]
		public Ability customWeaponAbilityToEnable;
		
		protected override void StartUseAction()
		{
			IsActive = false;

			var attackable = UnitOwner.GetModule<Attackable>();

			if (attackable)
			{
				attackable.CustomAttackDistance = newAttackRange;
				attackable.CustomDamage = newAttackDamage;
				attackable.CustomReloadTime = newAttackReloadTime;
			}

			var anotherAbility = UnitOwnerAbilities.GetAbility(customWeaponAbilityToEnable);
			
			if (anotherAbility)
				anotherAbility.IsActive = true;
		}
	}
}