using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.Menus
{
	public sealed class FactionDropdown : MonoBehaviour
	{
		PlayerEntry selfPlayerEntry;

		public void SetupWithData(Lobby lobby, PlayerEntry playerEntry)
		{
			selfPlayerEntry = playerEntry;

			var factions = lobby.GetStorage.availableFactions;
			var dataList = new Dropdown.OptionDataList();

			for (int i = 0; i < factions.Count; i++)
				dataList.options.Add(new Dropdown.OptionData(factions[i].textId));

			dataList.options.Add(new Dropdown.OptionData("Random"));

			GetComponent<Dropdown>().options = dataList.options;

			selfPlayerEntry.OnFactionDropdownChanged();
		}
	}
}