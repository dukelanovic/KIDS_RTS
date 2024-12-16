using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class Globals
	{
		public static readonly int LayerIgnoreRaycast;
			
		public static readonly int LayermaskTerrain;
		public static readonly LayerMask LayermaskObstaclesToShoot;
		public static readonly LayerMask LayermaskObstaclesToShootNoUnit;
		public static readonly LayerMask LayermaskUnit;

		static Globals()
		{
			LayermaskTerrain = 1 << LayerMask.NameToLayer("Terrain");
			LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

			var storage = GameController.Instance.MainStorage;
			
			LayermaskUnit = storage.unitLayerMask;
			LayermaskObstaclesToShoot = storage.obstaclesToUnitShoots;
			LayermaskObstaclesToShootNoUnit = storage.obstaclesToUnitShootsWithoutUnitLayer;
		}
	}
}