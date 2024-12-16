using UnityEngine;
using UnityEngine.AI;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows unit to move. Do not add it to the buildings. </summary>
	[DisallowMultipleComponent]
	public class Movable : Module
	{
	
		public event MoveAction StartedMove, StoppedMove;

		public bool IsMoving { get; protected set; }
		public float SqrDistanceFineToStop { get; protected set; }
		public float CustomSpeed { get; protected set; }
		public bool UseCustomSpeed { get; protected set; }
		
		protected NavMeshAgent navMeshAgent;

		
		Wheels wheelsModule;

		Vector3 lastMovePosition, destination;
		
		float airUnitMoveProblemTime;
		
		public delegate void MoveAction();

		protected override void AwakeAction()
		{
			if (!GetComponent<NavMeshAgent>())
				navMeshAgent = gameObject.AddComponent<NavMeshAgent>();

			navMeshAgent.speed = SelfUnit.Data.moveSpeed;
			navMeshAgent.angularSpeed = SelfUnit.Data.rotationSpeed;
			navMeshAgent.acceleration = GameController.Instance.MainStorage.UnitsAcceleration;

			var boxCollider = GetComponent<BoxCollider>();

			if (boxCollider)
			{
				navMeshAgent.radius = ((boxCollider.size.x + boxCollider.size.z) / 2f) / 2f;
			}
			else
			{
				var sphereCollider = GetComponent<SphereCollider>();

				if (sphereCollider)
					navMeshAgent.radius = sphereCollider.radius;
			}

			SqrDistanceFineToStop = 1.5f;

			Stop();
		}

		void Start()
		{
			if (!SelfUnit.Data.hasMoveModule)
				Debug.LogWarning("[Movable module] Unit " + name + " has disabled Has Move module toggle, but Movable module still added to prefab. Fix this.");

			wheelsModule = SelfUnit.GetModule<Wheels>();

			if (SelfUnit.Data.moveType == UnitData.MoveType.Flying)
				navMeshAgent.enabled = false;
		}

		void Update()
		{
			if (destination != transform.position)
			{
				if (wheelsModule && IsMoving)
					wheelsModule.RotateWheelsForward();

				// todo RTS Kit - check is target right and rotate wheels to this target
				
				MoveToPosition(lastMovePosition);

				if ((transform.position - destination).sqrMagnitude <= SqrDistanceFineToStop)// reached destination
					Stop();
			}
			else if (IsMoving)
			{
				SetMoveAsStopped();
			}

			if (SelfUnit.Data.moveType == UnitData.MoveType.Flying)
			{
				var selfPosition = transform.position;
				selfPosition.y = SelfUnit.Data.flyingFlyHeight;
				transform.position = selfPosition;

				if (!IsMoving)
					airUnitMoveProblemTime = 0;
			}
		}

		public void SetCustomSpeed(float speed, bool useSpeed)
		{
			UseCustomSpeed = useSpeed;
			CustomSpeed = speed;
			
			if (navMeshAgent)
				navMeshAgent.speed = UseCustomSpeed ? CustomSpeed : SelfUnit.Data.moveSpeed;
		}

		public void MoveToPosition(Vector3 position)
		{
			if (destination == position || position == transform.position)
				return;

			if (SelfUnit.IsBeingCarried || SelfUnit.IsMovementLockedByHotkey)
				return;
			
			destination = position;

			if (navMeshAgent.enabled)
				navMeshAgent.destination = destination;

			if (SelfUnit.Data.moveType == UnitData.MoveType.Flying && destination != transform.position)
			{
				destination.y = transform.position.y;

				var direction = (destination - transform.position).normalized;
				
				transform.position += direction * ((UseCustomSpeed ? CustomSpeed : SelfUnit.Data.moveSpeed) * Time.deltaTime);

				airUnitMoveProblemTime = Mathf.Clamp(airUnitMoveProblemTime - Time.deltaTime, 0f, 2f);
				
				if (direction != Vector3.zero)
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * SelfUnit.Data.rotationSpeed / 360f);
			}

			lastMovePosition = position;

			IsMoving = true;

			StartedMove?.Invoke();
		}

		public void Stop()
		{
			destination = transform.position;

			if (navMeshAgent.enabled)
				navMeshAgent.destination = destination;

			SetMoveAsStopped();
		}

		void SetMoveAsStopped()
		{
			IsMoving = false;
			StoppedMove?.Invoke();
		}

		void OnTriggerStay(Collider other) => PushUnitFromCollider(other);

		void PushUnitFromCollider(Collider other)
		{
			if (SelfUnit.Data.moveType != UnitData.MoveType.Flying || IsMoving)
				return;

			var otherUnit = other.GetComponent<Unit>();

			if (otherUnit)
			{
				var otherDirection = (other.transform.position - transform.position).normalized;

				transform.position -= otherDirection * Time.deltaTime;

				if (!IsMoving)
					destination = transform.position;
			}
		}
	}
}