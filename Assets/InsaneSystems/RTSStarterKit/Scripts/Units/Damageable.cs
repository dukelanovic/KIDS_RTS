using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[DisallowMultipleComponent]
	public class Damageable : Module
	{
		public delegate void OnDamageableDied(Unit unitOwner);
		public delegate void DamageableTakeDamage(Unit damagedUnit, float damageValue);
		
		public static event OnDamageableDied GlobalDied;
		public static event DamageableTakeDamage GlobalTakeDamage, GlobalHealed;

		public event OnDamageableDied Died;

		public float Health { get; protected set; }

		protected void Start()
		{
			Health = SelfUnit.Data.maxHealth;

			OnStart();
		}

		protected virtual void OnStart() { }

		public virtual void TakeDamage(float value)
		{
			if (Mathf.Approximately(value, 0) || value < 0)
				return;
			
			Health = Mathf.Clamp(Health - value, 0, SelfUnit.Data.maxHealth);

			GlobalTakeDamage?.Invoke(SelfUnit, value);

			if (SelfUnit.OwnerPlayerId == Player.LocalPlayerId)
				UI.UIController.Instance.MinimapSignal.ShowFor(SelfUnit);

			if (Health <= 0)
				Die();
		}
		
		public virtual void AddHealth(float value)
		{
			if (value <= 0)
				return;
			
			Health = Mathf.Clamp(Health + value, 0, SelfUnit.Data.maxHealth);

			GlobalHealed?.Invoke(SelfUnit, value);
		}

		public float GetHealthPercents() => Health / SelfUnit.Data.maxHealth;

		public virtual void Die()
		{
			PlayDeathEffect();
			
			GlobalDied?.Invoke(SelfUnit);
			Died?.Invoke(SelfUnit);
			
			Destroy(gameObject);
		}

		protected virtual void PlayDeathEffect()
		{
			if (SelfUnit.Data.explosionEffect)
				Instantiate(SelfUnit.Data.explosionEffect, transform.position, transform.rotation);
		}
	}
}