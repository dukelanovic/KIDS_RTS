using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class Cursors
	{
		public static bool lockCursorChange;
		
		public static void SetDefaultCursor()
		{
			SetCursor(GameController.Instance.MainStorage.defaultCursour);
		}
		
		public static void SetAttackCursor()
		{
			SetCursor(GameController.Instance.MainStorage.attackCursour, new Vector2(0.5f, 0.5f));
		}
		
		public static void SetRestrictCursor()
		{
			SetCursor(GameController.Instance.MainStorage.restrictCursour, new Vector2(0.5f, 0.5f));
		}

		public static void SetResourcesCursor()
		{
			SetCursor(GameController.Instance.MainStorage.gatherResourcesCursour, new Vector2(0.5f, 0.5f));
		}
		
		public static void SetGiveResourcesCursor()
		{
			SetCursor(GameController.Instance.MainStorage.giveResourcesCursour, new Vector2(0.5f, 0.5f));
		}
		
		public static void SetSellCursor()
		{
			SetCursor(GameController.Instance.MainStorage.sellCursor, new Vector2(0.5f, 0.5f));
			lockCursorChange = true;
		}
		
		public static void SetRepairCursor()
		{
			SetCursor(GameController.Instance.MainStorage.repairCursor, new Vector2(0.5f, 0.5f));
			lockCursorChange = true;
		}

		public static void SetMapOrderCursor()
		{
			SetCursor(GameController.Instance.MainStorage.mapOrderCursor);
		}
		
		public static void SetCursor(Texture2D cursorTexture)
		{
			SetCursor(cursorTexture, Vector2.up);
		}
		
		public static void SetCursor(Texture2D cursorTexture, Vector2 hotSpot)
		{
			if (lockCursorChange)
				return;
			
			Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
		}
	}
}