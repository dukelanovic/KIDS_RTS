using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public sealed class Build : MonoBehaviour
	{
		public static bool IsBuildMode { get; private set; }

		public static event Action<Unit> BuildingWasCreated;
		
		static int allExcludeTerrainLayer;
		static int sqrBuildDistance;

		static GameObject buildingToCreate;
		static Vector3 buildingSize;

		static GameObject drawer;
		static BuildingDrawer buildingDrawer;

		static Action onLocalPlayerBuildCallback;

		static bool startedRotation;
		static bool canBuild;

		static readonly Collider[] colliders = new Collider[5];
		
		Vector3 finalHitPoint = Vector3.zero;

		Camera mainCamera;
		bool useGridForBuildMode;

		void Start()
		{
			allExcludeTerrainLayer = ~Globals.LayermaskTerrain;

			sqrBuildDistance = (int)Mathf.Pow(GameController.Instance.MainStorage.maxBuildDistance, 2);

			mainCamera = Camera.main;
			useGridForBuildMode = GameController.Instance.MainStorage.useGridForBuildingMode;
		}

		void Update()
		{
			if (IsBuildMode)
			{
				if (Input.GetMouseButtonDown(1))
					DisableBuildMode();

				HandleBuilding(buildingSize);
			}
		}

		void HandleBuilding(Vector3 buildingSize)
		{
			var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out var hit, 1000, Globals.LayermaskTerrain))
			{
				if (canBuild && Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
				{
					CreateBuilding(buildingToCreate, finalHitPoint, drawer.transform.rotation);
					return;
				}

				if (Input.GetMouseButton(0) && GameController.Instance.MainStorage.allowBuildingsRotation)
				{
					if (startedRotation)
						drawer.transform.rotation = Quaternion.LookRotation(hit.point - drawer.transform.position);
					else if (Vector3.Distance(drawer.transform.position, hit.point) > 1.5f) // todo RTS Kit - optimize
						startedRotation = true;
				}
				else
				{
					finalHitPoint = hit.point;

					if (useGridForBuildMode)
						finalHitPoint = new Vector3((int)hit.point.x, hit.point.y, (int)hit.point.z);

					drawer.transform.position = finalHitPoint;
				}

				canBuild = CheckZoneToBuild(finalHitPoint, buildingSize, drawer.transform.rotation, Player.LocalPlayerId);

				buildingDrawer.SetBuildingAllowedState(canBuild);
			}
		}

		public static bool CheckZoneToBuild(Vector3 atPoint, Vector3 buildingSize, Quaternion rotation, byte playerToCheck)
		{
			var halfSize = buildingSize / 2f;

			float startHeight = 0;

			var size = Physics.OverlapBoxNonAlloc(atPoint, halfSize, colliders, Quaternion.identity, allExcludeTerrainLayer);

			if (size > 0)
				return false;

			for (int x = 0; x <= buildingSize.x; x++)
				for (int z = 0; z <= buildingSize.z; z++)
				{
					var xCheckPosition = Mathf.Clamp(x - halfSize.x, -halfSize.x, halfSize.x);
					var zCheckPosition = Mathf.Clamp(z - halfSize.z, -halfSize.z, halfSize.z);
					var checkPosition = new Vector3(xCheckPosition, 0, zCheckPosition);

					var ray = new Ray(atPoint + Vector3.up * 10f + checkPosition, -Vector3.up);

					if (Physics.Raycast(ray, out var hit, 1000, Globals.LayermaskTerrain))
					{
						if (x == 0 && z == 0)
						{
							startHeight = hit.point.y;
						}
						else
						{
							if (Mathf.Abs(startHeight - hit.point.y) > 0.15f)
								return false;
						}
					}
					else
					{
						return false;
					}
				}

			var playerCommCenter = Player.GetPlayerById(playerToCheck).OwnedProductionBuildings[0];
			return (playerCommCenter.transform.position - atPoint).sqrMagnitude < sqrBuildDistance;
		}

		public static GameObject CreateBuilding(GameObject buildingToSpawn, Vector3 atPoint, Quaternion rotation, byte playerOwner = 0)
		{
			var spawnedBuilding = Instantiate(buildingToSpawn, atPoint, rotation);
			var buildingUnit = spawnedBuilding.GetComponent<Unit>();
			buildingUnit.SetOwner(playerOwner);
			
			BuildingWasCreated?.Invoke(buildingUnit);

			if (playerOwner == Player.LocalPlayerId && onLocalPlayerBuildCallback != null)
				onLocalPlayerBuildCallback.Invoke();
			
			if (playerOwner == Player.LocalPlayerId)
				DisableBuildMode();

			return spawnedBuilding;
		}

		public void EnableBuildMode(GameObject buildingObject, Action newOnLocalPlayerBuildCallback = null)
		{
			if (IsBuildMode)
				DisableBuildMode();

			onLocalPlayerBuildCallback = newOnLocalPlayerBuildCallback;
			
			buildingToCreate = buildingObject;
			var buildingScript = buildingToCreate.GetComponent<Unit>();

			if (buildingScript)
				drawer = Instantiate(buildingScript.Data.drawerObject, Vector3.zero, Quaternion.Euler(0, 180, 0));

			buildingDrawer = drawer.GetComponent<BuildingDrawer>();
			buildingSize = GetBuildingSize(buildingScript.Data); 
	
			IsBuildMode = true;
		}

		public static Vector3 GetBuildingSize(UnitData buildingData)
		{ 
			return buildingData.selfPrefab.GetComponent<BoxCollider>().size;
		}

		public static void DisableBuildMode()
		{
			Destroy(drawer);
			IsBuildMode = false;
			startedRotation = false;
			canBuild = false;
		}
	}
}