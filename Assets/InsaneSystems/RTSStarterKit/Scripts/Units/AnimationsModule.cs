using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows to use animations on units. To enable it work, you need to set checkbox Use Animations in UnitData of your unit to checked. </summary>
	[DisallowMultipleComponent]
	public sealed class AnimationsModule : Module
	{
		[SerializeField] Animator animator;

		static readonly int attackId = Animator.StringToHash("Attack");
		static readonly int moveId = Animator.StringToHash("Move");
		static readonly int dieId = Animator.StringToHash("Die");
		static readonly int harvestId = Animator.StringToHash("Harvest");

		Attackable attackable;
		Movable movable;
		Harvester harvester;
		Damageable damageable;
		
		void Start()
		{
			if (!SelfUnit.Data.useAnimations)
			{
				enabled = false;
				return;
			}
			
			if (!animator)
				animator = GetComponent<Animator>();

			if (!animator)
			{
				Debug.LogWarning("Unit " + name + " does not have Animator component! It will have NO animations, if you're not add it.");
				return;
			}

			if (!animator.runtimeAnimatorController && SelfUnit.Data.animatorController)
				animator.runtimeAnimatorController = SelfUnit.Data.animatorController;

			if (SelfUnit.TryGetModule(out attackable))
			{
				attackable.StartedAttack += OnStartAttack;
				attackable.StoppedAttack += OnStopAttack;
			}

			if (SelfUnit.TryGetModule(out movable))
			{
				movable.StartedMove += OnStartMove;
				movable.StoppedMove += OnStopMove;
			}

			if (SelfUnit.TryGetModule(out damageable))
				damageable.Died += OnDie;
			
			if (SelfUnit.TryGetModule(out harvester))
			{
				harvester.StartedHarvest += OnStartHarvest;
				harvester.StoppedHarvest += OnStopHarvest;
			}
		}

		void OnDestroy()
		{
			if (attackable)
			{
				attackable.StartedAttack -= OnStartAttack;
				attackable.StoppedAttack -= OnStopAttack;
			}

			if (movable)
			{
				movable.StartedMove -= OnStartMove;
				movable.StoppedMove -= OnStopMove;
			}

			if (damageable)
				damageable.Died -= OnDie;
			
			if (harvester)
			{
				harvester.StartedHarvest -= OnStartHarvest;
				harvester.StoppedHarvest -= OnStopHarvest;
			}
		}

		void OnStartAttack() => SetAnimatorBool(attackId, true);
		void OnStopAttack() => SetAnimatorBool(attackId, false);

		void OnStartMove() => SetAnimatorBool(moveId, true);
		void OnStopMove() => SetAnimatorBool(moveId, false);

		void OnStartHarvest() => SetAnimatorBool(harvestId, true);
		void OnStopHarvest() => SetAnimatorBool(harvestId, false);

		void OnDie(Unit unit) => SetAnimatorBool(dieId, true);

		void SetAnimatorBool(int valueId, bool value)
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool(valueId, value);
		}
		
		public Animator GetAnimator() => animator;
	}
}