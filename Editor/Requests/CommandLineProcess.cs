using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace WhiteSparrow.PackageRepoEditor
{
	public enum ProcessFile
	{
		GIT = 0,
	}
	public class CommandLineProcess : IDisposable
	{
		private event Action m_OnComplete;
		public event Action OnComplete
		{
			add
			{
				if (IsCompleted)
					value?.Invoke();
				else
					m_OnComplete += value;
			}
			remove => m_OnComplete -= value;
		}
		
		
		
		private ProcessStartInfo m_StartInfo;
		private Process m_Process;

		private StringBuilder m_OutputStringBuilder;
		private StringBuilder m_ErrorStringBuilder;

		private string m_ErrorString;
		private string m_OutputString;
		
		public string Error
		{
			get
			{
				if (m_ErrorString == null)
					m_ErrorString = m_ErrorStringBuilder?.ToString() ?? string.Empty;
				return m_ErrorString;
			}
		}
		public string Output
		{
			get
			{
				if (m_OutputString == null)
					m_OutputString = m_OutputStringBuilder?.ToString() ?? string.Empty;
				return m_OutputString;
			}
		}

		private bool m_Terminated = false;
		public bool HasError => !string.IsNullOrWhiteSpace(Error);
		public bool IsCompleted => m_Terminated || (m_Process?.HasExited ?? false);

		public int ExitCode => m_Process?.ExitCode ?? -1;

		public CommandLineProcess(ProcessFile file, string command, string workingDirectory)
		{
			string fileName = null;
			switch (file)
			{
				case ProcessFile.GIT: fileName = "git.exe";
					break;
			}
			
			Init(fileName, command, workingDirectory);
		}
		public CommandLineProcess(string filename, string command, string workingDirectory)
		{
			Init(filename, command, workingDirectory);
		}

		private void Init(string filename, string command, string workingDirectory)
		{
			m_StartInfo = new ProcessStartInfo();
			m_StartInfo.UseShellExecute = false;
			m_StartInfo.CreateNoWindow = true;
			m_StartInfo.FileName = filename;
			m_StartInfo.Arguments = command;
			m_StartInfo.WorkingDirectory = workingDirectory;
			m_StartInfo.RedirectStandardOutput = true;
			m_StartInfo.RedirectStandardError = true;
		}

		public void Start()
		{
			try
			{
				m_OutputStringBuilder = new StringBuilder();
				m_ErrorStringBuilder = new StringBuilder();
				
				m_Process = new Process();
				m_Process.EnableRaisingEvents = true;
				m_Process.StartInfo = m_StartInfo;
				m_Process.Exited += OnProcessExited;
				m_Process.OutputDataReceived += OnProcessOutput;
				m_Process.ErrorDataReceived += OnErrorOutput;

				
				m_Process.Start();
				m_Process.BeginOutputReadLine();
				m_Process.BeginErrorReadLine();
				
				if(m_Process.HasExited)
					Complete();
			}
			catch (Exception e)
			{
				m_ErrorStringBuilder.AppendLine(e.Message);
				Complete();
			}
		}

		public void WaitTillExit()
		{
			if (m_Terminated)
				return;
			if (m_Process is { HasExited: true })
				return;
			m_Process.WaitForExit();
			return;
		}
		
		private void OnProcessExited(object sender, EventArgs e)
		{
			Complete();
		}

		private void OnProcessOutput(object sender, DataReceivedEventArgs e)
		{
			m_OutputStringBuilder.AppendLine(e.Data);
			m_OutputString = null;
		}

		private void OnErrorOutput(object sender, DataReceivedEventArgs e)
		{
			m_ErrorStringBuilder.AppendLine(e.Data);
			m_ErrorString = null;
		}


		public void Dispose()
		{
			m_Process?.Dispose();

			m_OutputStringBuilder?.Clear();
			m_OutputStringBuilder = null;
			
			m_ErrorStringBuilder?.Clear();
			m_ErrorStringBuilder = null;
		}

		private void Complete()
		{
			m_Terminated = true;
			m_OnComplete?.Invoke();
			m_OnComplete = null;
		}
	}
}