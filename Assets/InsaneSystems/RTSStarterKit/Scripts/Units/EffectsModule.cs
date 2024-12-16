using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module used to handle all unit effects. You can extend it with your own effects. </summary>
	[DisallowMultipleComponent]
	public class EffectsModule : Module
	{
		[Tooltip("This sound source will be used to play unit sounds like shoot effects, etc.")]
		[SerializeField] AudioSource unitSoundSource;
		
		Attackable attackable;
		Movable movable;
		Production production;

		GameObject selectionIndicator;

		Quaternion targetModelRotation;
		bool isGroundWasHit;
		RaycastHit groundHit;

		Vector3 previousPos;
		
		protected virtual void Start()
		{
			selectionIndicator = Instantiate(GameController.Instance.MainStorage.selectionIndicatorTemplate, transform);
			
			var boxColliderSize = GetComponent<BoxCollider>().size;
			selectionIndicator.transform.localScale = Vector3.one * (Mathf.Max(boxColliderSize.x, boxColliderSize.z) + 0.1f);
			selectionIndicator.SetActive(false);

			SoundLibrary.MakeUnitAudio(SelfUnit, true);

			SelfUnit.Selected += OnSelected;
			SelfUnit.Unselected += OnUnselected;
			
			Unit.UnitHovered += OnHovered;
			Unit.UnitUnhovered += OnUnhovered;
			
			if (SelfUnit.TryGetModule(out attackable))
				attackable.Shooted += OnShooted;

			if (SelfUnit.TryGetModule(out movable))
			{
				movable.StartedMove += OnStartedMove;
				movable.StoppedMove += OnStoppedMove;
			}

			if (SelfUnit.TryGetModule(out production))
			{
				production.StartedProduce += OnStartedProduce;
				production.EndedProduce += OnEndedProduce;
			}
			
			PlayUnitSound(SelfUnit.Data.readySoundVariations);
		}

		void Update()
		{
			UpdateGroundHit();
			HandleGroundRotation();
		}
		
		protected virtual void OnDestroy()
		{
			if (SelfUnit)
			{
				SelfUnit.Selected -= OnSelected;
				SelfUnit.Unselected -= OnUnselected;
			}
			
			Unit.UnitHovered -= OnHovered;
			Unit.UnitUnhovered -= OnUnhovered;

			if (attackable)
				attackable.Shooted -= OnShooted;

			if (movable)
			{
				movable.StartedMove -= OnStartedMove;
				movable.StoppedMove -= OnStoppedMove;
			}

			if (production)
			{
				production.StartedProduce -= OnStartedProduce;
				production.EndedProduce -= OnEndedProduce;
			}
		}
		
		public void UpdateGroundHit()
		{
			var isNeedUpdateGroundHit = (previousPos - transform.position).sqrMagnitude > 0;
			previousPos = transform.position;
			
			if (!isNeedUpdateGroundHit || !SelfUnit.Data.UseGroundAngle)
				return;
			
			var ray = new Ray(transform.position + Vector3.up, -Vector3.up);

			isGroundWasHit = Physics.Raycast(ray, out groundHit, 10, Globals.LayermaskTerrain);
		}

		protected virtual void HandleGroundRotation()
		{
			if (!SelfUnit.Data.UseGroundAngle || !SelfUnit.Model)
				return;

			var model = SelfUnit.Model.transform;
			bool doInstantRotation;
			
			if (isGroundWasHit)
			{
				var rotationToNormal = Quaternion.FromToRotation(Vector3.up, groundHit.normal);
				targetModelRotation = rotationToNormal * transform.rotation;
				doInstantRotation = Vector3.Angle(groundHit.normal, model.up) < 1f;
			}
			else
			{
				doInstantRotation = true;
			}
			
			if (doInstantRotation)
				model.rotation = targetModelRotation;
			else
				model.rotation = Quaternion.Lerp(model.rotation, targetModelRotation, Time.deltaTime * GameController.Instance.MainStorage.UnitsGroundRotationSpeed);
		}

		protected virtual void OnUnhovered(Unit unit)
		{
			if (unit != SelfUnit)
				return;

			Cursors.SetDefaultCursor();
		}

		protected virtual void OnHovered(Unit unit)
		{
			if (unit != SelfUnit)
				return;
			
			if (Selection.SelectedUnits.Count == 0 || !Selection.SelectedUnits[0])
				return;
			
			var mainUnit = Selection.SelectedUnits[0];

			if (!SelfUnit.IsInMyTeam(Player.LocalPlayer.TeamIndex) && mainUnit.Data.hasAttackModule)
			{
				var fowModule = SelfUnit.GetModule<FogOfWarModule>();

				if (!fowModule || fowModule.IsVisibleInFOW)
					Cursors.SetAttackCursor();
			}
			
			if (SelfUnit.IsInMyTeam(Player.LocalPlayer.TeamIndex) && mainUnit.Data.isHarvester && SelfUnit.Data.isRefinery)
				Cursors.SetGiveResourcesCursor();

			if (SelfUnit.Data.canCarryUnitsCount > 0 && SelfUnit.IsInMyTeam(Player.LocalPlayer.TeamIndex))
			{
				var anyCanBeCarried = false;

				for (int i = 0; i < Selection.SelectedUnits.Count; i++)
				{
					if (Selection.SelectedUnits[i] && Selection.SelectedUnits[i].Data.canBeCarried)
					{
						anyCanBeCarried = true;
						break;
					}
				}

				if (anyCanBeCarried)
				{
					var carryModule = SelfUnit.GetModule<CarryModule>();

					if (carryModule && carryModule.CanCarryOneMoreUnit())
						Cursors.SetGiveResourcesCursor();
				}
			}
		}

		protected virtual void OnSelected(Unit unit)
		{
			// todo Insane systems - move sound play here
			
			if (selectionIndicator)
				selectionIndicator.SetActive(true);
		}
		
		protected virtual void OnUnselected(Unit unit)
		{
			if (selectionIndicator)
				selectionIndicator.SetActive(false);
		}
		
		protected virtual void OnShooted()
		{
			PlayShootSound();
      
			var curShootPoint = attackable.GetCurrentShootPoint();

			if (SelfUnit.Data.shootEffect)
				Instantiate(SelfUnit.Data.shootEffect, curShootPoint.position, curShootPoint.rotation);
		}
		
		protected virtual void OnStartedMove()
		{ }
		
		protected virtual void OnStoppedMove()
		{ }
		
		protected virtual void OnEndedProduce()
		{ }

		protected virtual void OnStartedProduce()
		{ }
		
		public void PlaySelectionSound() => PlayUnitSound(SelfUnit.Data.selectionSoundVariations);

		public void PlayOrderSound()
		{
			if (SelfUnit.OwnerPlayerId == Player.LocalPlayerId)
				PlayUnitSound(SelfUnit.Data.orderSoundVariations);
		}

		public void PlayCustomSound(AudioClip audioClip) => PlayUnitSound(new AudioClip[1] { audioClip });

		public void PlayShootSound()
		{
			var pitchRandom = SelfUnit.Data.shootSoundPitchRandomization;
			PlayUnitSound(SelfUnit.Data.shootSoundVariations, Random.Range(-pitchRandom, pitchRandom));
		}

		void PlayUnitSound(AudioClip[] clipVariations, float randomedPitch = 0f)
		{
			if (!unitSoundSource || clipVariations.Length == 0 || unitSoundSource.isPlaying)
				return;

			unitSoundSource.pitch = 1f + randomedPitch;

			unitSoundSource.clip = clipVariations[Random.Range(0, clipVariations.Length)];
			unitSoundSource.Play();
		}
	}
}