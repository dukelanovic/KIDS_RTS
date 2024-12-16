using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Abilities;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "UnitData", menuName = Storage.AssetName + "/Unit Data")]
	public class UnitData : ScriptableObject
	{
		const string BonePrefix = "rts_";
		
		public enum UsedDamageType
		{
			UseDamageSettingFromShell,
			UseCustomDamageValue
		}

		public enum AttackType { Simple, Shell }
		public enum MoveType { Ground, Flying }
		public enum AttackPossibility { Land, Air, LandAndAir }

		public string textId;
		[Tooltip("This is icon of unit, which will be shown on build panel and when you select this unit ingame.")]
		public Sprite icon;
		[Tooltip("Usually, you place there imported unit model, and asset code will make ready unit prefab using it.")]
		public GameObject unitModel;
		
		[Header("Base parameters")]
		[Tooltip("If true, this unit will be able to be killed, otherwise it will be invulnerable.")]
		public bool canBeDestroyed = true;
		[Range(1f, 10000f)] public float maxHealth = 100;
		[Tooltip("Check this true if your unit is infantry soldier and should have animation module support.")]
		public bool isInfantry;
		[Tooltip("Vision distance in Fog of War. Used ONLY if Fog of War is ON.")]
		[Range(0f, 512f)] public float visionDistance = 10f;

		[Header("Build parameters")]
		[Range(0, 10000)] public int price = 100;
		[Range(0f, 360f)] public float buildTime = 3f;
		[Tooltip("If you have tiers or something like this in your game, pass here Data of building, which should be created before unlocking possibility to build this unit/building.")]
		public UnitData requiredBuildingToUnlock;
		
		[Header("Movement parameters")]
		[Tooltip("Check this, if your unit can move (non-building units usually).")]
		public bool hasMoveModule = true;
		[Tooltip("Use Ground for all ground units. Flying is used to Helicopter/Drone like flying movement.")]
		public MoveType moveType = MoveType.Ground;
		[Range(0f, 100f)] public float moveSpeed = 2;
		[Range(0f, 720f)] public float rotationSpeed = 360;
		[Range(2f, 10f)] public float flyingFlyHeight = 6f;
		public bool canMoveWhileAttackingTarget = true;

		[Header("Attack parameters")]
		public bool hasAttackModule;
		[Tooltip("Attack type. Use Simple when unit should just hit target when it in attack distance without any conditions. Use Shell if unit should shoot in direction of target using Shell prefab.")]
		public AttackType attackType = AttackType.Shell;
		public AttackPossibility attackPossibility = AttackPossibility.Land;
		[Range(0f, 1000f)] public float attackDistance = 5;
		[Tooltip("Note that this value will be used only if Used Damage Type set to Use Custom Damage Value setting, otherwise will be used damage from Shell settings.")]
		public float attackDamage = 15f;
		public UsedDamageType usedDamageType = UsedDamageType.UseDamageSettingFromShell;
		[Tooltip("Here you should place shell/bullet/rocket object, which will be spawned when unit shoot.")]
		public GameObject attackShell;
		[Tooltip("If true, an unit obstacles on fireline will be ignored, otherwise attacker unit will search better shoot position. Good for games with big count of units on battlefield.")]
		public bool allowShootThroughUnitObstacles = true;
		[Tooltip("If true, unit will be able shoot through other units and walls. Previous setting will be ignored.")]
		public bool allowShootThroughAnyObstacles = true;
		public bool needAimToTargetToShoot = true;
		[Tooltip("If you disabled previous toggle, look at this. If it true, units will still try to aim enemies (rotate to them), but of course will be able to shoot without aim. If you NOT need rotation to target, uncheck this toggle.")]
		public bool stillTryRotateToTargetWhenNoAimNeeded = true;
		[Tooltip("Unit reload time in seconds.")]
		[Range(0f, 360f)] public float reloadTime = 1;
		[Space(5)] public bool hasTurret;
		[Range(0f, 360f)] public float turretRotationSpeed = 1;
		public bool limitTurretRotationAngle;
		[Tooltip("This parameter works only if Limit Turret Rotation Angle toggle is on.")]
		[Range(0f, 179f)] public float maximumTurretRotationAngle = 15;
		[Space(5)]
		[Tooltip("Is unit have splash damage on its attack? Splash damage applies to all enemy units in splash radius.")]
		public bool hasSplashDamage; 
		[Tooltip("Radius of splash damage. It calculated from this unit shell explosion position.")]
		[Range(0.25f, 30f)] public float splashDamageRadius;
		[Tooltip("Splash damage which will be applied to enemies in splash radius.")]
		[Range(0.5f, 1000)] public float splashDamageValue;

		[Header("Electricity parameters")]
		[Range(0, 40)] public int addsElectricity;
		[Range(0, 40)] public int usesElectricity;

		[Header("Carry parameters")]
		[Tooltip("Is this unit can be carried on board of carrier units?")]
		public bool canBeCarried;
		[Tooltip("How much units can carry this unit on his board? 0 means he can't carry any units.")]
		[Range(0, 40)] public int canCarryUnitsCount;
		
		[Header("Harvester parameters")]
		[Tooltip("Is this unit harvest resources?")]
		public bool isHarvester;
		public int harvestMaxResources = 600;
		public float harvestTime = 5f;
		public float harvestCarryOutTime = 3f;
		
		[Header("Effects")]
		[Tooltip("Object, added to this field, will be spawned after unit death. You can place here explosion effect or any other, what you want. Particles effect should be Play on Awake.")]
		public GameObject explosionEffect;
		[Tooltip("Effect, which will be spawned on unit Shoot Point when unit attack target. Particles effect should be Play on Awake.")]
		public GameObject shootEffect;

		[Tooltip("Check this toggle if your unit have animations like move, attack, etc. It usually used for infantry, soldiers etc. Because it is humanuid unit and always have animations.")]
		public bool useAnimations;
		[Tooltip("If you have ready animator controller for your unit, place it here, and next time automatic prefab generation will add it to the unit prefab Animator settings.")]
		public RuntimeAnimatorController animatorController;
		
		[Tooltip("Should this unit be rotated to a ground normal below him? Useful for vehicles. Don't recommended for infantry.")]
		public bool UseGroundAngle;

		[Tooltip("If unit has wheels, you need to setup its rotation speed in degrees/sec.")]
		public float WheelsRotationSpeed;
		
		[Header("Audio Settings")]
		[Tooltip("Add in this array all unit shoot sound variations, one of them will be selected randomly every shoot.")]
		[Sound] public AudioClip[] shootSoundVariations;
		[Tooltip("Add in this array all unit selection sound variations (for example, voices or sound effects), one of them will be selected randomly every unit selection.")]
		[Sound] public AudioClip[] selectionSoundVariations;
		[Tooltip("Add in this array all unit order sound variations (for example, voices or sound effects), one of them will be selected randomly and played every order for unit.")]
		[Sound] public AudioClip[] orderSoundVariations;
		[Tooltip("Add in this array all unit ready sound variations (for example, voices or sound effects), one of them will be selected randomly and played on unit spawn.")]
		[Sound] public AudioClip[] readySoundVariations;
		[Tooltip("Increasing this value make shoot audio effect sounds little different every time.")]
		[Range(0, 0.5f)] public float shootSoundPitchRandomization = 0.05f;

		[Header("Misc")]
		[Tooltip("Add to this field prefab of Unit, for which you done this settings. Object from this field will be spawned, when player buy this unit ingame.")]
		public GameObject selfPrefab;

		[Header("Building settings")]
		public bool isBuilding;
		[Tooltip("Refinery is building, where your harvesters will bring resources. Check this true, if this building is refinery.")]
		public bool isRefinery;
		[Tooltip("Harvester unit data which will be spawned on this refinery at start. If not set, no harvester will be spawned")]
		public UnitData RefineryHarversterUnitData;
		[Tooltip("Check this toggle if your building should produce something. For example, if this building allows to build another buildings or units.")]
		public bool isProduction;
		[Tooltip("List of production categories of this unit. Left empty, if it has no production module attached to. Note that usually there will be only one category.")]
		public List<ProductionCategory> productionCategories = new List<ProductionCategory>();
		[Tooltip("This GameObject used to draw building when Build mode is active. Usually transparent and colored. Btw, you can add to it custom material, etc.")]
		public GameObject drawerObject;

		[Header("Abilities")]
		[Tooltip("List of unit abilities. Drag here data files of needed abilities and it will appear ingame.")]
		public List<Ability> unitAbilities;
		
		/// <summary> This method used by RTS Kit to generate ready unit prefab. You can override it and add your code, if you need to extend UnitData with your own settings. </summary>
		public virtual void SetupPrefab(GameObject unitGameObject)
		{
			if (unitGameObject.GetComponent<Unit>())
			{
				Debug.LogWarning($"Unit prefab for {unitGameObject} is already set up!");
				return;
			}
			
			var collectedBones = new List<Transform>();
			
			var unitData = this;
			
			var unitModule = unitGameObject.AddComponent<Unit>();
			unitModule.SetUnitData(unitData);

			var model = new GameObject("Model");
			model.transform.SetParent(unitGameObject.transform);
			model.transform.localPosition = Vector3.zero;

			var realModel = Instantiate(unitData.unitModel, model.transform);
			realModel.name = realModel.name.Replace("(Clone)", "");
			realModel.transform.localPosition = Vector3.zero;

			unitGameObject.CheckComponent<ElectricityModule>(unitData.addsElectricity > 0 || unitData.usesElectricity > 0);

			unitGameObject.CheckComponent<BoxCollider>(true);
			unitGameObject.CheckComponent<EffectsModule>(true);
			unitGameObject.CheckComponent<Damageable>(unitData.canBeDestroyed);
			unitGameObject.CheckComponent<Movable>(unitData.hasMoveModule);
			
			var attackable = unitGameObject.CheckComponent<Attackable>(unitData.hasAttackModule);
			unitGameObject.CheckComponent<EnemiesSearch>(unitData.hasAttackModule);
			
			if (unitData.hasAttackModule)
			{
				GetBones(model.transform, "shoot", ref collectedBones);
				attackable.SetShootPoints(collectedBones);
			}
			
			var turret = unitGameObject.CheckComponent<Tower>(unitData.hasTurret);
			
			if (unitData.hasTurret)
			{
				GetBones(model.transform, "turret", ref collectedBones);
		
				if (collectedBones.Count > 0)
					turret.SetTurretTransform(collectedBones[0]);
			}

			unitGameObject.CheckComponent<Harvester>(unitData.isHarvester);
			unitGameObject.CheckComponent<Refinery>(unitData.isRefinery);

			if (unitData.isProduction)
			{
				for (var i = 0; i < unitData.productionCategories.Count; i++)
				{
					var production = unitGameObject.AddComponent<Production>();
					production.SetCategoryId(i);
					
					GetBones(model.transform, "spawn_point", ref collectedBones);

					if (collectedBones.Count > 0)
						production.SpawnPoint = collectedBones[0];
					
					GetBones(model.transform, "move_point", ref collectedBones);

					if (collectedBones.Count > 0)
						production.SpawnWaypoint = collectedBones[0];
				}
			}

			unitGameObject.CheckComponent<Infantry>(unitData.isInfantry);
			
			unitGameObject.CheckComponent<AnimationsModule>(unitData.useAnimations);
			
			var animator = unitGameObject.CheckComponent<Animator>(unitData.useAnimations);

			if (unitData.useAnimations && unitData.animatorController)
				animator.runtimeAnimatorController = unitData.animatorController;

			var fowModule = unitGameObject.CheckComponent<FogOfWarModule>(true);
			fowModule.AutoSetup();
			
			unitGameObject.CheckComponent<CarryModule>(unitData.canCarryUnitsCount > 0);
			
			GetBones(model.transform, "wheel", ref collectedBones);

			var wheelsComponent = unitGameObject.CheckComponent<Wheels>(collectedBones.Count > 0);
			if (wheelsComponent)
				wheelsComponent.SetupWheels(collectedBones);

			unitGameObject.CheckComponent<AudioSource>(true);
			
			Debug.Log($"Unit prefab for {name} was generated! Now you can setup this prefab visually and with features you need.");
		}

		void GetBones(Transform model, string boneName, ref List<Transform> output)
		{
			output.Clear();
			
			var childs = model.transform.GetAllChilds(true);
				
			foreach (var child in childs)
				if (child.name.ToLower().StartsWith(BonePrefix + boneName))
					output.Add(child);
		}
	}
}