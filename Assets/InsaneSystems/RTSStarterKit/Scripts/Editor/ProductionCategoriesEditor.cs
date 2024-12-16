using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public sealed class ProductionCategoriesEditor : DataListEditor
	{	
		[MenuItem("RTS Starter Kit/Production Categories Editor", priority = 2)]
		static void Init()
		{
			var window = (ProductionCategoriesEditor)GetWindow(typeof(ProductionCategoriesEditor));
			window.InitLoad();

			window.titleContent = new GUIContent(window.editorName);
			window.minSize = new Vector2(1024f, 600f);
			window.maxSize = new Vector2(1024f, 2048f);
			
			window.Show();
		}
		
		public override void InitLoad()
		{
			InitialSetup("RTS Production Categories Editor", "ProductionCategories", "productioncategory", "Category");
			StylesSetup(InsaneEditorStyles.Headers["ProductionCatEditor"]);
		}
		
		protected override Sprite GetButtonIcon<T>(T data)
		{
			return data == null ? null : (data as ProductionCategory).icon;
		}
		
		protected override void DrawCreateButton()
		{
			if (GUILayout.Button("Create new category"))
				CreateNewData<ProductionCategory>();
		}
	}
}