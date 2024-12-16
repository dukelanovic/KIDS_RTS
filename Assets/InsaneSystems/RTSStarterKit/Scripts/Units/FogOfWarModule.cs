using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[DisallowMultipleComponent]
	public class FogOfWarModule : Module
	{
		const float UpdateRate = 0.25f;

		[Tooltip("List of all unit Renderer components which should be hidden when enemy unit enters FOW and shown when exits FOW.")]
		[SerializeField] Renderer[] renderersToHide = Array.Empty<Renderer>();
		[Tooltip("Can be empty. List of all unit child game objects which should be hidden when enemy unit enters FOW and shown when exits FOW.")]
		[SerializeField] GameObject[] gameObjectsToDisable = Array.Empty<GameObject>();
		
		public bool IsVisibleInFOW { get; protected set; }
		
		float updateVisibilityTimer;

		void Start()
		{
			if (!GameController.Instance.MainStorage.isFogOfWarOn)
			{
				enabled = false;
				return;
			}
			
			for (var i = 0; i < gameObjectsToDisable.Length; i++)
				if (gameObjectsToDisable[i] == gameObject)
				{
					Debug.LogWarning("Fog of war module of unit " + name + " setted up incorrectly. You shouldn't add self unit object to the Game Objects To Disable field! Add models here.");
					enabled = false;
					return;
				}
			
			
			CheckVisibleState();
			
			if (IsPlayerTeamUnit())
				IsVisibleInFOW = true;
		}
		
		void Update()
		{
			if (updateVisibilityTimer >= 0)
			{
				updateVisibilityTimer -= Time.deltaTime;
				return;
			}

			CheckVisibleState();

			updateVisibilityTimer = UpdateRate;
		}

		void CheckVisibleState()
		{
			if (IsPlayerTeamUnit())
				return;

			var allLocalPlayerTeamUnits = GetAllLocalPlayerTeamUnits();

			bool isVisible = false;
			var selfPosition = transform.position;
			
			for (int i = 0; i < allLocalPlayerTeamUnits.Count; i++)
			{
				if (!allLocalPlayerTeamUnits[i])
					continue;

				var otherUnitSqrVisibility = Mathf.Pow(allLocalPlayerTeamUnits[i].Data.visionDistance, 2f);
				var otherPosition = allLocalPlayerTeamUnits[i].transform.position;
				if ((selfPosition - otherPosition).sqrMagnitude <= otherUnitSqrVisibility)
				{
					isVisible = true;
					break;
				}
			}

			SetShownState(isVisible);
		}

		List<Unit> GetAllLocalPlayerTeamUnits()
		{
			var allUnits = Unit.AllUnits;
			var allUnitsCount = allUnits.Count;

			var resultUnits = new List<Unit>();
			
			for (int i = 0; i < allUnitsCount; i++)
				if (allUnits[i].IsOwnedByPlayer(Player.LocalPlayerId) || allUnits[i].IsInMyTeam(Player.LocalPlayerId))
					resultUnits.Add(allUnits[i]);

			return resultUnits;
		}

		bool IsPlayerTeamUnit()
		{
			return SelfUnit.IsOwnedByPlayer(Player.LocalPlayerId) || SelfUnit.IsInMyTeam(Player.LocalPlayerId);
		}

		void SetShownState(bool visibility)
		{
			for (int i = 0; i < renderersToHide.Length; i++)
				renderersToHide[i].enabled = visibility;

			for (int i = 0; i < gameObjectsToDisable.Length; i++)
			{
				if (gameObjectsToDisable[i] == SelfUnit.gameObject)
				{
					Debug.LogWarning("In FOW unit module of unit " + name +
					                 " in hideable game object set its self object. It is wrong. Ignoring.");
					continue;
				}

				gameObjectsToDisable[i].SetActive(visibility);
			}

			IsVisibleInFOW = visibility;
		}

		public void AutoSetup()
		{
			renderersToHide = GetComponentsInChildren<Renderer>();
		}
	}
}