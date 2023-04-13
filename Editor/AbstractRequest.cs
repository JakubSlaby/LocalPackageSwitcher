using System;

namespace WhiteSparrow.PackageRepoEditor
{
	public abstract class AbstractRequest
	{
		public string Error { get; private set; }
		
		private bool m_IsStarted;
		private bool m_IsComplete = false;
		public bool IsComplete => m_IsComplete;

		private Action m_OnComplete;
		public event Action OnComplete
		{
			add
			{
				if (m_IsComplete)
				{
					value?.Invoke();
					return;
				}
				
				m_OnComplete += value;
			}
			remove => m_OnComplete -= value;
		}
		
		public void Start()
		{
			if (m_IsStarted)
				return;
			
			m_IsStarted = true;
			StartRequest();
		}

		protected abstract void StartRequest();
		
		protected void Complete()
		{
			m_IsComplete = true;
			m_OnComplete?.Invoke();
			m_OnComplete = null;
		}

		protected void CompleteError(string error)
		{
			Error = error;
			Complete();
		}
	}
}