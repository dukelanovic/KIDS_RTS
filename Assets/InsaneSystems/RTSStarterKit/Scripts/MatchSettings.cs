using System.Collections.Generic;

namespace InsaneSystems.RTSStarterKit
{
	public class MatchSettings
	{
		public static MatchSettings CurrentMatchSettings { get; set; }

		public List<PlayerSettings> PlayersSettings { get; set; }
		public MapSettings SelectedMap { get; protected set; }

		public MatchSettings() => PlayersSettings = new List<PlayerSettings>();

		public void AddPlayerSettings(PlayerSettings playerSettings) => PlayersSettings.Add(playerSettings);
		public void RemovePlayerSettingsById(byte id) => PlayersSettings.RemoveAt(id);

		public void RemovePlayerSettings(PlayerSettings playerSettings) => PlayersSettings.Remove(playerSettings);
		public void SelectMap(MapSettings selectedMap) => SelectedMap = selectedMap;
	}
}