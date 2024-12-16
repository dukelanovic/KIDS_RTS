using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.Menus
{
	public sealed class PlayerEntry : MonoBehaviour
	{
		public PlayerSettings SelfPlayerSettings { get; private set; }

		[SerializeField] Text nickNameText;
		[SerializeField] ColorDropdown colorDropdown;
		[SerializeField] FactionDropdown factionDropdown;
		[SerializeField] Dropdown teamDropdown;
		[SerializeField] Button removeButton;

		Lobby parentLobby;

		public void SetupWithPlayerSettings(PlayerSettings playerSettings, Lobby fromLobby)
		{
			SelfPlayerSettings = playerSettings;

			colorDropdown.SetupWithData(fromLobby, this);
			colorDropdown.SetColorValue(playerSettings.color);

			teamDropdown.value = playerSettings.team;

			parentLobby = fromLobby;

			removeButton.interactable = playerSettings.isAI;
			nickNameText.text = playerSettings.nickName;

			factionDropdown.SetupWithData(fromLobby, this);
		}

		public void OnColorDropdownChanged(Color value) => parentLobby.PlayerChangeColor(SelfPlayerSettings, value);

		public void OnTeamDropdownChanged() => SelfPlayerSettings.team = (byte)teamDropdown.value;

		public void OnFactionDropdownChanged()
		{
			var selectedId = factionDropdown.GetComponent<Dropdown>().value;
			var factions = parentLobby.GetStorage.availableFactions;
			
			SelfPlayerSettings.selectedFaction = selectedId < factions.Count ? factions[selectedId] : factions[Random.Range(0, factions.Count - 1)];
		}

		public void OnRemoveButton()
		{
			Destroy(gameObject);
			parentLobby.RemovePlayer(SelfPlayerSettings);
		}
	}
}