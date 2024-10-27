using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

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
			if (m_IsComplete)
				return;
			
			m_IsComplete = true;
			m_OnComplete?.Invoke();
			m_OnComplete = null;
		}

		protected void CompleteError(string error)
		{
			Error = error;
			Debug.LogError(Error);
			Complete();
		}
	}

	public static class PackageManagerRequest
	{
		public static PackageManagerRequest<T> Wrap<T>(T request, Action<T> completeCallback = null)
			where T : Request
		{
			PackageManagerRequest<T> output = new PackageManagerRequest<T>(request);
			if(completeCallback != null)
				output.OnComplete += completeCallback;
			return output;
		}
	}

	public class PackageManagerRequest<T> : IDisposable
		where T : Request
	{
		private T m_Request;
		public T Request => m_Request;

		private Action<T> m_OnComplete;
		public event Action<T> OnComplete
		{
			add
			{
				if (m_Request == null)
					return;
				
				if (m_Request.IsCompleted)
				{
					value.Invoke(m_Request);
				}
				else
				{
					m_OnComplete += value;
				}
			}
			remove => m_OnComplete -= value;
		}
		
		
		public PackageManagerRequest(T request)
		{
			m_Request = request;
			if (m_Request.IsCompleted)
			{
				Complete();
			}
			else
			{
				EditorApplication.update += OnEditorUpdate;
			}
			
		}

		private void OnEditorUpdate()
		{
			if (m_Request == null || m_Request.IsCompleted)
			{
				Complete();
			}
		}

		private void Complete()
		{
			EditorApplication.update -= OnEditorUpdate;

			m_OnComplete?.Invoke(m_Request);
			m_OnComplete = null;
		}

		public void Dispose()
		{
			EditorApplication.update -= OnEditorUpdate;
			m_OnComplete = null;
			m_Request = null;
		}

		
	}
	
}