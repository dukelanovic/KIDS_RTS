using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This class contains all game texts. You can extend it and override GetTextFromListById method, to add support of custom languages, for example. </summary>
	[CreateAssetMenu(fileName = "TextsLibrary", menuName = Storage.AssetName + "/Texts Library")]
	public class TextsLibrary : ScriptableObject
	{
		public List<TextData> uiTextDatas = new List<TextData>();

		public string GetUITextById(string textId)
		{
			return GetTextFromListById(uiTextDatas, textId);
		}

		public virtual string GetTextFromListById(List<TextData> textDatas, string textId)
		{
			for (int i = 0; i < textDatas.Count; i++)
				if (textDatas[i].textId == textId)
					return textDatas[i].englishText;

			return "No text added";
		}
	}

	[System.Serializable]
	public class TextData
	{
		public string textId = "textId";
		public string englishText = "English Text";
	}
}