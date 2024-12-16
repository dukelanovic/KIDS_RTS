using System;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
    /// <summary> This module searches an enemies targets for Attackable module. Something like basic AI for Unit. </summary>
    [DisallowMultipleComponent]
    public class EnemiesSearch : Module
    {
        const float EnemySearchDelayTime = 0.2f, MaxEnemySearchRadius = 18f;
        
        public event Action<Unit> TargetFound;
        public event Action<Unit> BaitTargetFound;
        
        public Unit BaitTarget { get; protected set; }
        
        protected float enemySearchDelay, targetSearchDelay;
        		
        protected readonly Collider[] objectsInSearchRadius = new Collider[100];
        protected Unit[] unitComponentsFound = new Unit[1];
        
        protected int unitLayermask;

        protected Attackable attackable;
        
        protected virtual void Start()
        {
            unitLayermask = Globals.LayermaskUnit;
            attackable = SelfUnit.GetModule<Attackable>();
        }

        protected virtual void Update()
        {
            if (enemySearchDelay > 0)
                enemySearchDelay -= Time.deltaTime;
            else
                SearchForEnemy();

            if (targetSearchDelay > 0)
                targetSearchDelay -= Time.deltaTime;
            else
                SearchBaitTarget();
        }
        
        protected virtual void SearchBaitTarget()
        {
            if (SelfUnit.HasOrders() || attackable.AttackTarget != null || BaitTarget != null)
                return;
			
            float objectsCount = Physics.OverlapSphereNonAlloc(transform.position, MaxEnemySearchRadius, objectsInSearchRadius, unitLayermask);

            for (int i = 0; i < objectsCount; i++)
            {
                unitComponentsFound = objectsInSearchRadius[i].GetComponents<Unit>();

                if (unitComponentsFound.Length == 0)
                    continue;

                var unit = unitComponentsFound[0];

                if (!unit || SelfUnit.IsInMyTeam(unit) || unit == SelfUnit || !unit.GetModule<FogOfWarModule>().IsVisibleInFOW || !attackable.CanAttackTargetByMoveType(unit))
                    continue;

				SetBaitTarget(unit);
                
                break;
            }

            targetSearchDelay = EnemySearchDelayTime;
        }
		
        protected virtual void SearchForEnemy()
        {
            if (attackable.AttackTarget != null) 
                return;

            float objectsCount = Physics.OverlapSphereNonAlloc(transform.position, attackable.GetAttackDistance(), objectsInSearchRadius, unitLayermask);

            for (int i = 0; i < objectsCount; i++)
            {
                unitComponentsFound = objectsInSearchRadius[i].GetComponents<Unit>();

                if (unitComponentsFound.Length == 0)
                    continue;

                var unit = unitComponentsFound[0];

                if (SelfUnit.IsInMyTeam(unit) || unit == SelfUnit || !attackable.CanAttackTargetByMovePossibility(unit.transform) || !attackable.CanAttackTargetByMoveType(unit))
                    continue;

                TargetFound?.Invoke(unit);

                break;
            }

            enemySearchDelay = EnemySearchDelayTime;
        }

        protected void SetBaitTarget(Unit baitTarget)
        {
            BaitTarget = baitTarget;

            BaitTargetFound?.Invoke(baitTarget);
        }
    }
}