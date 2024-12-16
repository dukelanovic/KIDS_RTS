using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This class describes order to the unit. There some already implemented inherited classes like AttackOrder and MovePositionOrder. You can create your own if you want. </summary>
	[System.Serializable]
	public abstract class Order
	{
		public Unit Executor { get; set; }
		bool isStarted;
		
		public void Execute()
		{
			if (!isStarted)
			{
				isStarted = true;
				StartAction();
			}

			ExecuteAction();
		}
		
		public virtual void StartAction() { }

		protected virtual void ExecuteAction() => End();

		public virtual void End() => Executor.EndCurrentOrder();
		public abstract Order Clone();

		protected virtual Vector3 GetActualMovePosition() => Vector3.zero;
	}

	[System.Serializable]
	public class AttackOrder : Order
	{
		public Unit AttackTarget { get; set; }
		
		Vector3 moveOffset;

		public override void StartAction()
		{
			if (AttackTarget)
				moveOffset = AttackTarget.GetNearPoint(Executor.transform.position, true);
		}
		
		protected override void ExecuteAction()
		{
			if (!Executor || !Executor.Attackable || !AttackTarget)
			{
				End();
				return;
			}

			if (Executor.Movable)
			{
				if (!Executor.Attackable.IsFireLineFree(AttackTarget) ||
				    !Executor.Attackable.IsTargetInAttackRange(AttackTarget))
					Executor.Movable.MoveToPosition(GetActualMovePosition());
				else
					Executor.Movable.Stop();
			}

			// todo RTS Kit - extend attackTarget
		}

		public override Order Clone()
		{
			var order = new AttackOrder
			{
				AttackTarget = AttackTarget
			};

			return order;
		}

		protected override Vector3 GetActualMovePosition() => AttackTarget.transform.position + moveOffset;
	}

	[System.Serializable]
	public class MovePositionOrder : Order
	{
		public Vector3 MovePosition { get; set; }

		protected override void ExecuteAction()
		{
			if (!Executor || !Executor.Movable)
			{
				if (Executor && !Executor.Movable)
					Debug.LogWarning($"Movable order given to wrong unit {Executor}.");

				End();
				return;
			}

			Executor.Movable.MoveToPosition(MovePosition); // todo do it once at start

			var movePosWithSameY = MovePosition;
			movePosWithSameY.y = Executor.transform.position.y;
			
			if ((Executor.transform.position - movePosWithSameY).sqrMagnitude <= Executor.Movable.SqrDistanceFineToStop)
				End();
		}

		public override Order Clone()
		{
			var order = new MovePositionOrder
			{
				MovePosition = MovePosition
			};

			return order;
		}

		protected override Vector3 GetActualMovePosition() => MovePosition;
	}

	[System.Serializable]
	public class FollowOrder : Order
	{
		public Unit FollowTarget { get; set; }

		Vector3 moveOffset;

		public override void StartAction()
		{
			if (FollowTarget)
				moveOffset = FollowTarget.GetNearPoint(Executor.transform.position, true);
		}

		protected override void ExecuteAction()
		{
			if (!Executor || !Executor.Movable || !FollowTarget)
			{
				End();
				return;
			}

			Executor.Movable.MoveToPosition(GetActualMovePosition());

			// todo RTS Kit - change it to make set movable target to follow target
		}

		public override Order Clone()
		{
			var order = new FollowOrder
			{
				FollowTarget = FollowTarget
			};

			return order;
		}

		protected override Vector3 GetActualMovePosition() => FollowTarget.transform.position  + moveOffset;
	}
}