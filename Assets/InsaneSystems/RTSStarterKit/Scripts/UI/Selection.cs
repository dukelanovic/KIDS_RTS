using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public sealed class Selection : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] RectTransform selectionPanel;

		Vector2 startPoint;
		Vector2 midPoint;

		bool isSelectionStarted;

		void Start()
		{
			Hide();

			Controls.Selection.SelectionStarted += OnStartSelection;
			Controls.Selection.SelectionEnded += OnEndSelection;
		}

		void Update()
		{
			if (isSelectionStarted)
				SelectionWorkAction();
		}

		void OnDestroy()
		{
			Controls.Selection.SelectionStarted -= OnStartSelection;
			Controls.Selection.SelectionEnded -= OnEndSelection;
		}
		
		void OnStartSelection()
		{
			startPoint = Input.mousePosition;
			Show();

			isSelectionStarted = true;
		}

		void SelectionWorkAction()
		{
			midPoint = Vector2.Lerp(startPoint, Input.mousePosition, 0.5f);

			selectionPanel.transform.position = midPoint;
			selectionPanel.sizeDelta = new Vector2(Mathf.Abs(Input.mousePosition.x - startPoint.x), Mathf.Abs(Input.mousePosition.y - startPoint.y));
		}

		void OnEndSelection()
		{
			selectionPanel.transform.position = Input.mousePosition;
			selectionPanel.sizeDelta = Vector2.zero;

			Hide();
			isSelectionStarted = false;
		}

		void Show()
		{
			selfObject.SetActive(true);
			isSelectionStarted = true;
		}

		void Hide()
		{
			selfObject.SetActive(false);
			isSelectionStarted = false;
		}
	}
}