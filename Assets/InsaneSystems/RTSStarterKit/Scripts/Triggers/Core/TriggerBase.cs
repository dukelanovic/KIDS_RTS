using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public abstract class TriggerBase : MonoBehaviour
	{
		[Tooltip("How much times trigger will be repeated after execution?")]
		[SerializeField] int repeatTimes;
		[Tooltip("Delay between every repeats in seconds. Will be used only if Repeat Times is greater than 0.")]
		[SerializeField] float repeatDelay = 1;

		bool isExecutionStarted;
		int repeatTimesLeft;
		float repeatDelayLeft;

		public void Execute()
		{
			ExecuteAction();
			repeatTimesLeft = repeatTimes;
			repeatDelayLeft = repeatDelay;
			isExecutionStarted = true;
		}

		protected abstract void ExecuteAction();

		void Update()
		{
			if (isExecutionStarted && repeatTimesLeft > 0)
			{
				repeatDelayLeft -= Time.deltaTime;

				if (repeatDelayLeft <= 0)
				{
					ExecuteAction();
					repeatTimesLeft--;
					repeatDelayLeft = repeatDelay;

					if (repeatTimesLeft == 0)
						isExecutionStarted = false;
				}
			}
		}
	}
}