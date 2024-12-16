using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneSystems.RTSStarterKit
{
	[DisallowMultipleComponent]
	public class Tower : Module
	{
		[SerializeField] Transform turretTransform;
		[SerializeField] Transform secondAxisGun;

		float timerToNextRandom;
		float randomRotationTime;
		int randomRotateDirection;

		Quaternion secondAxisGunDefaultLocalRotation;

		void Start()
		{
			if (!SelfUnit.Data.hasTurret)
				Debug.LogWarning("[Tower module] Unit " + name + " has disabled Has Turret toggle, but Tower module still added to prefab. Fix this.");

			if (secondAxisGun)
				secondAxisGunDefaultLocalRotation = secondAxisGun.localRotation;
		}

		void Update() => RotateTower();

		public bool IsTurretAimedToTarget(Collider target)
		{
			var otherSameToTowerYPosition = target.transform.position;
			otherSameToTowerYPosition.y = turretTransform.position.y;
			var turretForwardNoY = turretTransform.forward;
			turretForwardNoY.y = 0;

			var toOther = (otherSameToTowerYPosition - turretTransform.position).normalized;
			return Vector3.Angle(turretForwardNoY, toOther) < 3f;
		}

		void RotateTower()
		{
			if (SelfUnit.Attackable.AttackTarget != null)
			{
				if (!CanRotateToTarget(SelfUnit.Attackable.AttackTarget.transform))
					return;

				var target = SelfUnit.Attackable.AttackTarget.transform;

				var targetPositionSameY = target.position;
				targetPositionSameY.y = turretTransform.position.y;

				var newRotation = Quaternion.LookRotation(targetPositionSameY - turretTransform.position);

				turretTransform.rotation = Quaternion.RotateTowards(turretTransform.rotation, newRotation, SelfUnit.Data.turretRotationSpeed);

				if (secondAxisGun)
				{
					var newGunRotation = Quaternion.LookRotation(target.position - secondAxisGun.position);

					secondAxisGun.localRotation = Quaternion.RotateTowards(secondAxisGun.localRotation, newGunRotation, SelfUnit.Data.turretRotationSpeed);
					secondAxisGun.localEulerAngles = new Vector3(secondAxisGun.localEulerAngles.x, 0f, 0f);
				}
			}
			else if (SelfUnit.HasOrders())
			{
				var newRotation = Quaternion.RotateTowards(turretTransform.rotation, transform.rotation, SelfUnit.Data.turretRotationSpeed);
				turretTransform.rotation = newRotation;

				RotateSecondAxisGunToDefault();
			}
			else if (!SelfUnit.Data.limitTurretRotationAngle)
			{
				if (timerToNextRandom <= 0)
				{
					randomRotationTime = Random.Range(0.2f, 1f);
					randomRotateDirection = Random.Range(0, 1);
					timerToNextRandom = 10f;
				}
				else
				{
					timerToNextRandom -= Time.deltaTime;
				}

				if (randomRotationTime > 0)
				{
					randomRotationTime -= Time.deltaTime;
					turretTransform.Rotate(Vector3.up, randomRotateDirection == 0 ? -1f : 1f);
				}

				RotateSecondAxisGunToDefault();
			}
		}

		void RotateSecondAxisGunToDefault()
		{
			if (!secondAxisGun)
				return;

			secondAxisGun.localRotation = Quaternion.RotateTowards(secondAxisGun.localRotation, secondAxisGunDefaultLocalRotation, SelfUnit.Data.turretRotationSpeed);
			secondAxisGun.localEulerAngles = new Vector3(secondAxisGun.localEulerAngles.x, 0f, 0f);
		}

		public bool CanRotateToTarget(Transform target)
		{
			if (!SelfUnit.Data.limitTurretRotationAngle)
				return true;

			var targetDirection = (target.position - transform.position).normalized;
			var angleBetween = Mathf.Abs(Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up));

			return angleBetween <= SelfUnit.Data.maximumTurretRotationAngle;
		}

		public void SetTurretTransform(Transform turretTransform) => this.turretTransform = turretTransform;
		public void SetSecondAxisGunTransform(Transform secondAxisGunTransform) => secondAxisGun = secondAxisGunTransform;

		public Transform GetTurretTransform() => turretTransform;
		public Transform GetSecondAxisGunTransform() => secondAxisGun;
	}
}