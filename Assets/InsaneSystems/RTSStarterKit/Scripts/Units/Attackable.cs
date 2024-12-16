using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[DisallowMultipleComponent]
	public class Attackable : Module
	{
		public delegate void AttackAction();
		
		static readonly Collider[] splashDamageColliders = new Collider[30];
		
		public event AttackAction StartedAttack, StoppedAttack, Shooted;
		public event Action<Unit> NewTargetWasSet;

		[SerializeField] protected Transform[] shootPoints = new Transform[1];

		public Unit AttackTarget { get; protected set; }
		public Transform[] ShootPoints => shootPoints;
		public Transform CurrentShootPoint => shootPoints[currentShootPoint];

		public float CustomDamage { get; set; }
		public float CustomAttackDistance { get; set; }
		public float CustomReloadTime { get; set; }
		public GameObject CustomShell { get; set; }

		float currentReloadTime;
		/// <summary> Is target being attacked continious time? </summary>
		bool attackingTarget;
		int currentShootPoint;
		
		float squaredAttackDistance;
		
		EnemiesSearch enemiesSearch;

		void Start()
		{
			if (!SelfUnit.Data.hasAttackModule)
				Debug.LogWarning("[Attack module] Unit " + name + " has disabled Has Attack Module toggle, but Attack Module still added to prefab. Fix this.");
			
			squaredAttackDistance = SelfUnit.Data.attackDistance * SelfUnit.Data.attackDistance;
			
			enemiesSearch = gameObject.CheckComponent<EnemiesSearch>(true);
			enemiesSearch.TargetFound += SetTarget;
			enemiesSearch.BaitTargetFound += OnBaitTargetFound;
		}
		
		protected void SetTarget(Unit target)
		{
			AttackTarget = target;

			NewTargetWasSet?.Invoke(target);
		}

		protected virtual void OnBaitTargetFound(Unit baitTarget)
		{
			var order = new AttackOrder { AttackTarget = baitTarget };
			SelfUnit.AddOrder(order, false, false);
		}
		
		protected virtual void Update() => HandleAttack(Time.deltaTime);

		protected virtual void HandleAttack(float deltaTime)
		{
			if (!AttackTarget)
			{
				StopAttack(false);
				return;
			}

			if (!CanAttackTargetByMoveState())
			{
				StopAttack(false);
				return;
			}

			if (SelfUnit.HasOrders() && SelfUnit.Orders[0] is AttackOrder)
			{
				var orderTarget = ((AttackOrder) SelfUnit.Orders[0]).AttackTarget;

				if (orderTarget != AttackTarget && IsTargetInAttackRange(orderTarget) && CanAttackTargetByMoveType(orderTarget))
					AttackTarget = orderTarget;
			}

			if (!IsTargetInAttackRange(AttackTarget))
			{
				StopAttack(true);
				return;
			}

			if ((!SelfUnit.Tower || !SelfUnit.Tower.CanRotateToTarget(AttackTarget.transform)) && SelfUnit.Movable && SelfUnit.Data.stillTryRotateToTargetWhenNoAimNeeded)
			{
				var targetSameYPosition = AttackTarget.transform.position;
				targetSameYPosition.y = transform.position.y;

				var rotationToTarget = Quaternion.LookRotation(targetSameYPosition - transform.position);

				transform.rotation = Quaternion.Lerp(transform.rotation, rotationToTarget, deltaTime * 5f);
			}

			if (currentReloadTime > 0)
			{
				currentReloadTime -= deltaTime;
				return;
			}

			if (IsFireLineFree(AttackTarget) && IsTurretAimedToTarget(AttackTarget))
				DoShoot();
		}

		void StopAttack(bool removeTarget = false)
		{
			if (removeTarget)
				AttackTarget = null;

			attackingTarget = false;

			StoppedAttack?.Invoke();
		}

		protected virtual void DoShoot()
		{
			var curShootPoint = GetCurrentShootPoint();
	
			if (SelfUnit.Data.attackType == UnitData.AttackType.Simple)
			{
				var damageable = AttackTarget.GetModule<Damageable>();

				if (damageable)
					damageable.TakeDamage(CustomDamage > 0 ? CustomDamage : SelfUnit.Data.attackDamage);
				
				if (SelfUnit.Data.hasSplashDamage)
					DoSplashDamage(transform.position, SelfUnit.Data.splashDamageRadius, SelfUnit.Data.splashDamageValue, SelfUnit);
			}
			else if (SelfUnit.Data.attackType == UnitData.AttackType.Shell)
			{
				var spawnedObject = Instantiate(GetShellTemplate(), curShootPoint.position, curShootPoint.rotation);

				var shell = spawnedObject.GetComponent<Shell>();

				if (shell)
				{
					shell.SetUnitOwner(SelfUnit);
					shell.SetTarget(AttackTarget);

					if (SelfUnit.Data.usedDamageType == UnitData.UsedDamageType.UseCustomDamageValue)
						shell.SetCustomDamage(SelfUnit.Data.attackDamage);

					if (CustomDamage > 0)
						shell.SetCustomDamage(CustomDamage);
				}
			}
			
			StartReload();
			UpdateShootPoint();

			if (!attackingTarget)
				StartedAttack?.Invoke();

			Shooted?.Invoke();

			attackingTarget = true;
		}
		
		protected virtual void StartReload()
		{
			currentReloadTime = SelfUnit.Data.reloadTime;

			if (CustomReloadTime > 0)
				currentReloadTime = CustomReloadTime;
		}

		void UpdateShootPoint()
		{
			if (currentShootPoint < shootPoints.Length - 1)
				currentShootPoint++;
			else
				currentShootPoint = 0;
		}
		
		/// <summary> Requires unit to shoot now in current target without reload and attack conditions check. Use only if you know why you need it. </summary>
		public void DoCustomShoot(GameObject newCustomShell = null)
		{
			if (newCustomShell)
				CustomShell = newCustomShell;
			
			DoShoot();

			if (newCustomShell)
				CustomShell = null;
		}

		public Transform GetCurrentShootPoint() => shootPoints[currentShootPoint];

		public virtual bool IsTargetInAttackRange(Unit target)
		{
			if (!target)
				return false;

			return (transform.position - target.transform.position).sqrMagnitude <= GetSquaredAttackDistance();
		}

		public virtual bool IsFireLineFree(Unit target)
		{
			if (!target)
				return false;

			if (SelfUnit.Data.allowShootThroughAnyObstacles)
				return true;

			var targetCenter = target.GetCenterPoint();

			var ray = new Ray(CurrentShootPoint.position, targetCenter - CurrentShootPoint.position);

			// in first case will check only static obstacles, otherwise will check unit obstacles too. If no obstacles returns true. If obstacle is target also returns true.
			var layerMask = SelfUnit.Data.allowShootThroughUnitObstacles ? Globals.LayermaskObstaclesToShootNoUnit : Globals.LayermaskObstaclesToShoot;

			if (Physics.Raycast(ray, out var hit, GetAttackDistance(), layerMask))
				return hit.collider.gameObject == target.gameObject;

			return true;
		}

		public virtual bool IsTurretAimedToTarget(Unit target)
		{
			if (!SelfUnit.Data.needAimToTargetToShoot)
				return true;

			if (!SelfUnit.Tower)
			{
				var otherSameToSelfYPosition = target.transform.position;
				otherSameToSelfYPosition.y = transform.position.y;
				var selfForwardNoY = transform.forward;
				selfForwardNoY.y = 0;

				var toOther = (otherSameToSelfYPosition - transform.position).normalized;
				return Vector3.Angle(selfForwardNoY, toOther) < 3f;
			}

			return SelfUnit.Tower.IsTurretAimedToTarget(target.GetComponent<BoxCollider>());
		}

		public virtual bool CanAttackTargetByMoveState()
		{
			if (!SelfUnit.Tower && SelfUnit.Movable && SelfUnit.Movable.IsMoving)
				return false;
			
			if (SelfUnit.Tower && SelfUnit.Movable && SelfUnit.Movable.IsMoving && !SelfUnit.Data.canMoveWhileAttackingTarget)
				return false;

			return true;
		}

		public bool CanAttackTargetByMoveType(Unit target)
		{
			var attackPossibility = SelfUnit.Data.attackPossibility;
			
			if ((attackPossibility == UnitData.AttackPossibility.Land || attackPossibility == UnitData.AttackPossibility.LandAndAir) && target.Data.moveType == UnitData.MoveType.Ground)
				return true;
			
			if ((attackPossibility == UnitData.AttackPossibility.Air || attackPossibility == UnitData.AttackPossibility.LandAndAir) && target.Data.moveType == UnitData.MoveType.Flying)
				return true;

			return false;
		}

		public bool CanAttackTargetByMovePossibility(Transform target)
		{
			return SelfUnit.Tower && SelfUnit.Tower.CanRotateToTarget(target) || SelfUnit.Movable;
		}
		
		public void SetShootPoints(List<Transform> shootPointsTransforms) => shootPoints = shootPointsTransforms.ToArray();

		public float GetAttackDistance() => CustomAttackDistance > 0 ? CustomAttackDistance : SelfUnit.Data.attackDistance;

		public float GetSquaredAttackDistance()
		{
			if (CustomAttackDistance > 0)
				return CustomAttackDistance * CustomAttackDistance;

			return squaredAttackDistance;
		}

		protected GameObject GetShellTemplate() => CustomShell ? CustomShell : SelfUnit.Data.attackShell;

		public static void DoSplashDamage(Vector3 atPoint, float radius, float value, Unit unitShooted = null)
		{
			byte shooterPlayer = 0; 
			var excludeTeamDamage = false;

			if (unitShooted)
			{
				shooterPlayer = unitShooted.OwnerPlayerId;
				excludeTeamDamage = true;
			}

			var unitLayerMask = Globals.LayermaskUnit; // debug; todo move somewhere
			var colsCount = Physics.OverlapSphereNonAlloc(atPoint, radius, splashDamageColliders, unitLayerMask);

			for (int i = 0; i < colsCount; i++)
			{
				var damageable = splashDamageColliders[i].GetComponent<Damageable>();

				if (!damageable || damageable.SelfUnit == unitShooted)
					continue;
				
				if (excludeTeamDamage && damageable.SelfUnit.IsInMyTeam(shooterPlayer))
					continue;
				
				damageable.TakeDamage(value);
			}
		}

		void OnDrawGizmosSelected()
		{
			var selfUnitTemporary = GetComponent<Unit>();
			UnitData selfData = null;

			if (selfUnitTemporary)
				selfData = selfUnitTemporary.Data;

			if (selfData != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position, selfData.attackDistance);
			}
		}

		void OnDrawGizmos()
		{
			if (shootPoints.Length == 0)
				return;

			for (int i = 0; i < shootPoints.Length; i++)
			{
				if (shootPoints[i] == null)
					continue;

				var endPoint = shootPoints[i].transform.position + shootPoints[i].transform.forward;

				Gizmos.color = Color.yellow;

				Gizmos.DrawLine(shootPoints[i].transform.position, endPoint);
				Gizmos.DrawLine(endPoint, endPoint - shootPoints[i].transform.forward / 3f - shootPoints[i].transform.right / 5f);
				Gizmos.DrawLine(endPoint, endPoint - shootPoints[i].transform.forward / 3f + shootPoints[i].transform.right / 5f);
			}
		}
	}
}