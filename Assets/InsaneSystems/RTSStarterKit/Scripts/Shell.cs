using System;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[RequireComponent(typeof(Rigidbody))]
	public class Shell : MonoBehaviour
	{
		public event Action<Unit> TargetWasSet;
		
		[Comment("Shell is a component, adds to the object game logic of bullet, rocket or some other projectile. It is used by a shooting units, so you need to add it to the prefab of your projectile and set up needed parameters.")]
		[Tooltip("Damage, which will receive target when hitted by this shell.")]
		[SerializeField] [Range(0, 10000)] protected float damage = 50f;
		[Tooltip("Shell fly speed in meters per second. Meters is default Unity coordinates unit, so position 1 0 0 it is in 1 meter from 0 0 0.")]
		[SerializeField] [Range(0, 1000)] protected float flySpeed = 3f;
		[SerializeField] [Range(0, 30)] protected float lifeTime = 5f;
		[Tooltip("If true, this shell will fly like auto-aiming missile, following attack target. Otherwise it can miss target.")]
		[SerializeField] protected bool autoAim;
		[SerializeField] protected bool destroyIfTargetDied = true;
		
		[Tooltip("Effect of explosion which will be spawned when this shell hits ground or enemy unit.")]
		[SerializeField] protected GameObject explosionEffect;
		
		[Header("Artillery settings")]
		[Tooltip("Check this true, if this shell used by artillery unit. So it will fly using sinus for trajectory.")]
		[SerializeField] protected bool isArtilleryShell;
		[Tooltip("Max fly height of artillery shell.")]
		[SerializeField] protected float maxHeight = 10f;

		/// <summary> Unit, which shooted this shell. Note that if this unit dead, this field will be null. </summary>
		public Unit OwnerUnit { get; private set; }
		
		protected Vector3 StartPosition { get; private set; }
	
		protected byte PlayerOwner { get; private set; }
		protected Transform Target { get; private set; }
		protected Unit TargetUnitComponent { get; private set; }

		protected UnitData SelfUnitData { get; private set; }
		
		protected Vector3 FirstTargetPosition { get; private set; }
		
		float currentFlyTime;
		float currentHeight;
		float artilleryFlyTime = 1f;
		
		void Awake()
		{
			if (!GetComponent<Collider>())
			{
				var sphereCollider = gameObject.AddComponent<SphereCollider>();
				sphereCollider.radius = 0.25f;
			}
		}

		protected virtual void Start() => StartPosition = transform.position;

		protected virtual void Update()
		{
			var deltaTime = Time.deltaTime;
			
			Fly(deltaTime);
			lifeTime -= deltaTime;

			if (destroyIfTargetDied && Target == null)
				DestroyAction();
			else if (lifeTime <= 0)
				Destroy(gameObject);
		}

		protected virtual void Fly(float deltaTime)
		{
			if (isArtilleryShell)
			{
				transform.position = Vector3.Lerp(StartPosition, FirstTargetPosition, currentFlyTime / artilleryFlyTime);

				currentHeight = Mathf.Sin((currentFlyTime / artilleryFlyTime) * Mathf.PI) * maxHeight;
				transform.position = new Vector3(transform.position.x, StartPosition.y + currentHeight, transform.position.z);

				currentFlyTime += deltaTime;
			}
			else
			{
				transform.position += transform.forward * (flySpeed * deltaTime);
			}

			if (autoAim && Target)
			{
				var targetPosition = Target.position;

				if (TargetUnitComponent)
					targetPosition.y += TargetUnitComponent.GetSize().y / 2f;

				transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			var otherShell = other.GetComponent<Shell>();

			if (otherShell)
				return;

			var unit = other.GetComponent<Unit>();
		
			if (unit)
			{
				if (SelfUnitData.allowShootThroughUnitObstacles && unit != TargetUnitComponent)
					return;

				var damageable = unit.GetModule<Damageable>();

				if (!damageable || unit.IsInMyTeam(PlayerOwner))
					return;

				damageable.TakeDamage(damage);
				DestroyAction();
			}
			else if (isArtilleryShell)
			{
				DestroyAction();
			}
		}

		void DestroyAction()
		{
			if (explosionEffect)
				Instantiate(explosionEffect, transform.position, Quaternion.identity);

			if (SelfUnitData.hasSplashDamage)
				Attackable.DoSplashDamage(transform.position, SelfUnitData.splashDamageRadius, SelfUnitData.splashDamageValue, OwnerUnit);
			
			Destroy(gameObject);
		}

		public void SetUnitOwner(Unit owner)
		{
			OwnerUnit = owner;
			PlayerOwner = owner.OwnerPlayerId;
			SelfUnitData = owner.Data;
		}

		public void SetCustomDamage(float damageValue) => damage = damageValue;

		public void SetTarget(Unit target)
		{
			Target = target.transform;
			TargetUnitComponent = target;

			FirstTargetPosition = target.transform.position;

			TargetWasSet?.Invoke(target);
		}
	}
}