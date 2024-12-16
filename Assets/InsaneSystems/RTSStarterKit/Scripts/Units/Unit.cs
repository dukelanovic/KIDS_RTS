using System;
using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> Core unit class. To extend it, use modules. This class itself is sealed. </summary>
	[RequireComponent(typeof(BoxCollider))]
	[DisallowMultipleComponent]
	public sealed class Unit : MonoBehaviour
	{
		public static List<Unit> AllUnits { get; } = new List<Unit>();

		public static event UnitAction UnitSpawned, UnitHovered, UnitUnhovered, UnitDestroyed;
		public static event UnitChangedOwnerAction UnitChangedOwner;
		
		public event UnitAction Selected, Unselected;
		public event UnitOrderReceived ReceivedOrder;

		public delegate void UnitAction(Unit unit);
		public delegate void UnitChangedOwnerAction(Unit unit, int newOwner, int previousOwner);
		public delegate void UnitOrderReceived(Unit unit, Order order);

		[Tooltip("Unit will get parameters from Unit Data file, so move data of this unit to this field.")]
		[SerializeField] UnitData unitData;
		[Tooltip("List of renderers, which should be colored in team colors. You also can select number of material to colorize, if mesh have several materials.")]
		[SerializeField] List<ColoredRenderer> coloredRenderers;
		[SerializeField] [Range(0, 255)] byte ownerPlayerId;

		public UnitData Data => unitData;
		public Damageable Damageable { get; private set; }
		public Movable Movable { get; private set; }
		public Attackable Attackable { get; private set; }
		public Tower Tower { get; private set; }
		public Production Production { get; private set; }
		public EffectsModule Effects { get; private set; }
		public int UnitSelectionGroup { get; private set; }

		public readonly List<Order> Orders = new List<Order>();

		public byte OwnerPlayerId => ownerPlayerId;

		public bool IsSelected { get; private set; }
		public bool IsHovered { get; private set; }
		public bool IsBeingCarried { get; private set; }
		public bool IsMovementLockedByHotkey { get; set; }
		
		public GameObject Model { get; private set; }
		
		readonly List<Module> modules = new List<Module>();

		new Collider collider;

		void Awake()
		{
			AllUnits.Add(this);

			Damageable = GetComponent<Damageable>();
			Movable = GetComponent<Movable>();
			Attackable = GetComponent<Attackable>();
			Tower = GetComponent<Tower>();
			Production = GetComponent<Production>();
			
			UnitSelectionGroup = -1;

			if (Data.moveType == UnitData.MoveType.Flying)
			{
				GetComponent<BoxCollider>().isTrigger = true;
				
				var newRigidbody = gameObject.CheckComponent<Rigidbody>(true);

				newRigidbody.isKinematic = true;
			}
			
			gameObject.CheckComponent<AbilitiesModule>(Data.unitAbilities.Count > 0);
			gameObject.CheckComponent<AnimationsModule>(Data.useAnimations);
			Effects = gameObject.CheckComponent<EffectsModule>(true);
			
			var transformModel = transform.Find("Model");

			if (transformModel)
				Model = transformModel.gameObject;
			else
				Debug.LogWarning("Unit " + name + " doesn't have Model child object, which should contain his mesh etc.");

			collider = GetComponent<Collider>();
		}

		void Start()
		{
			if (Damageable)
				Damageable.GlobalDied += UnitDiedAction;

			UpdateColorByOwner();

			SetupBuilding();
			
			gameObject.CheckComponent<ElectricityModule>((Data.addsElectricity > 0 || Data.usesElectricity > 0) && GameController.Instance.MainStorage.isElectricityUsedInGame);
			gameObject.CheckComponent<FogOfWarModule>(GameController.Instance.MainStorage.isFogOfWarOn);

			UnitSpawned?.Invoke(this);
		}

		void SetupBuilding()
		{
			if (!Data.isBuilding) 
				return;
			
			if (GameController.Instance.MainStorage.addNavMeshObstacleToBuildings && !GetComponent<NavMeshObstacle>())
			{
				var navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();

				navMeshObstacle.shape = NavMeshObstacleShape.Box;
				var boxCollider = GetComponent<BoxCollider>();
				if (boxCollider)
				{
					navMeshObstacle.center = boxCollider.center;
					navMeshObstacle.size = boxCollider.size;
				}

				navMeshObstacle.carving = true;
			}
		}
		
		void Update()
		{
			if (HasOrders())
				Orders[0].Execute();
		}

		void OnDestroy()
		{
			Damageable.GlobalDied -= UnitDiedAction;
			
			if (IsHovered)
				Cursors.SetDefaultCursor();
		}

		/// <summary> Call this on game match start. </summary>
		public static void Init()
		{
			AllUnits.Clear();
		}
		
		public void AddOrder(Order order, bool isAdditive, bool isSoundNeeded = true, bool isReceivedEventNeeded = true)
		{
			if (!isAdditive)
				Orders.Clear();

			var personalOrder = order.Clone();
			personalOrder.Executor = this;

			Orders.Add(personalOrder);

			if (isReceivedEventNeeded)
				ReceivedOrder?.Invoke(this, order);

			if (isSoundNeeded)
				Effects.PlayOrderSound();
		}

		public void SetOwner(byte playerId)
		{
			var previousOwner = ownerPlayerId;
			
			ownerPlayerId = playerId;
			UpdateColorByOwner();
			Unselect();
			Selection.UnselectUnit(this);

			UnitChangedOwner?.Invoke(this, playerId, previousOwner);
		}

		void UpdateColorByOwner() // todo: InsaneSystems - move to EffectsModule?
		{
			var material = GameController.Instance.PlayersController.PlayersIngame[ownerPlayerId].PlayerUnitMaterial;

			for (int i = 0; i < coloredRenderers.Count; i++)
			{
				if (coloredRenderers[i].usesHouseColorShader)
					coloredRenderers[i].SetColor(GameController.Instance.PlayersController.PlayersIngame[ownerPlayerId].Color);
				else
					coloredRenderers[i].SetMaterial(material);
			}
		}

		public void EndCurrentOrder()
		{
			if (HasOrders())
				Orders.RemoveAt(0);
			
			if (Movable)
				Movable.Stop();
		}

		public void EndAllOrders()
		{
			if (HasOrders())
				Orders.Clear();

			if (Movable)
				Movable.Stop();
		}

		public void Select(bool isSoundNeeded = true)
		{
			IsSelected = true;

			if (isSoundNeeded)
				Effects.PlaySelectionSound();

			Selected?.Invoke(this);
		}

		public void Unselect()
		{
			IsSelected = false;
			Unselected?.Invoke(this);
		}

		void UnitDiedAction(Unit unit)
		{
			if (unit != this) 
				return;

			AllUnits.Remove(unit);
			UnitDestroyed?.Invoke(this);
		}

		public bool HasOrders() => Orders != null && Orders.Count > 0;
		public bool IsOwnedByPlayer(int playerId) => ownerPlayerId == playerId;
		public Player GetOwnerPlayer() => Player.GetPlayerById(ownerPlayerId);

		public bool IsInMyTeam(Unit other)
		{
			return GameController.Instance.PlayersController.IsPlayersInOneTeam(ownerPlayerId, other.ownerPlayerId);
		}

		public bool IsInMyTeam(byte otherPlayerId)
		{
			return GameController.Instance.PlayersController.IsPlayersInOneTeam(ownerPlayerId, otherPlayerId);
		}

		public Vector3 GetSize()
		{
			return collider switch
			{
				BoxCollider boxCollider => boxCollider.size,
				SphereCollider sphereCollider => sphereCollider.radius * Vector3.one,
				_ => Vector3.zero
			};
		}

		public Vector3 GetCenterPoint() => transform.position + transform.up * GetSize().y / 2f;

		public void RegisterModule(Module module)
		{
			if (!modules.Contains(module))
				modules.Add(module);
		}

		public T GetModule<T>() where T : Module
		{
			/* todo rts kit use dictionaries and allow only one module type (2 productions now is ok, idk is it good idea
			Module result;
			modulesNew.TryGetValue(typeof(T), out result);

			return result as T;
			*/
			
			for (int i = 0; i < modules.Count; i++)
				if (modules[i].GetType() == typeof(T) || modules[i].GetType().IsSubclassOf(typeof(T)))
					return modules[i] as T;

			return default;
		}
		
		public bool TryGetModule<T>(out T module) where T : Module
		{
			module = GetModule<T>();

			if (module)
				return true;

			module = default;
			return false;
		}

		public void SetUnitSelectionGroup(int value)
		{
			if (UnitSelectionGroup == value)
				UnitSelectionGroup = -1;
			else
				UnitSelectionGroup = value;
		}

		public void SetUnitData(UnitData unitData) => this.unitData = unitData;

		public void OnMouseEnter()
		{
			IsHovered = true;
			UnitHovered?.Invoke(this);
		}
		
		public void OnMouseExit()
		{
			IsHovered = false;
			UnitUnhovered?.Invoke(this);
		}
		
		public bool IsUnitVisibleOnScreen()
		{
			if (coloredRenderers.Count == 0 || coloredRenderers[0].renderer == null) // todo RTS Kit - replace colored renderer check for main renderer check
				return false;
			
			return coloredRenderers[0].renderer.isVisible;
		}
		
		public bool IsVisibleInViewport()
		{
			var coords = GetViewportPosition(GameController.CachedMainCamera);
			return coords.x > 0 && coords.x < 1 && coords.y > 0 && coords.y < 1;
		}

		public Vector2 GetViewportPosition(Camera forCamera)
		{
			return forCamera.WorldToViewportPoint(transform.position);
		}
		
		/// <summary> Returns best point near unit using its size. </summary>
		public Vector3 GetNearPoint(Vector3 toPoint, bool getOnlyOffset = false)
		{
			var initPoint = transform.position;
			var size3D = GetSize();
			var size = Mathf.Max(size3D.x, size3D.z) / 2f; // divide by 2 because its radius, not diameter

			var direction = (toPoint - transform.position).normalized;
			var offset = direction * size;

			if (getOnlyOffset)
				return offset;
			
			return initPoint + offset;
		}
		
		/// <summary> Returns random point near unit using its size.</summary>
		public Vector3 GetRandomNearPoint(bool getOnlyOffset = false)
		{
			var initPoint = transform.position;
			var size3D = GetSize();
			var radius = Mathf.Max(size3D.x, size3D.z) / 2f;

			var offset = new Vector3(Mathf.Sin(Random.Range(-Mathf.PI, (float)Math.PI)) * radius, 0, Mathf.Cos(Random.Range(-Mathf.PI, (float)Math.PI)) * radius);

			if (getOnlyOffset)
				return offset;
			
			return initPoint + offset;
		}
		
		public void SetCarryState(bool isCarried) // todo Insane Systems - move to separated module?
		{
			IsBeingCarried = isCarried;

			if (isCarried)
				Selection.UnselectUnit(this);

			//gameObject.SetActive(!isCarried);  // todo Insane Systems - change to false visiblity because unit should shoot

			var active = !isCarried;

			if (Model)
				Model.SetActive(active);

			GetModule<Movable>().enabled = active;
			GetComponent<NavMeshAgent>().enabled = active;
			GetComponent<Collider>().enabled = active;
		}
	}
}