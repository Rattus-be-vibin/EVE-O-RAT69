using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EveOPreview.Services.Implementation;

internal sealed class ProcessMonitor : IProcessMonitor
{
	private const string DEFAULT_PROCESS_NAME = "ExeFile";

	private const string CURRENT_PROCESS_NAME = "EVE-O Preview";

	private readonly IDictionary<IntPtr, string> _processCache;

	private IProcessInfo _currentProcessInfo;

	public ProcessMonitor()
	{
		_processCache = new Dictionary<IntPtr, string>(512);
		_currentProcessInfo = new ProcessInfo(IntPtr.Zero, "");
	}

	private bool IsMonitoredProcess(string processName)
	{
		return string.Equals(processName, "ExeFile", StringComparison.OrdinalIgnoreCase);
	}

	private IProcessInfo GetCurrentProcessInfo()
	{
		Process currentProcess = Process.GetCurrentProcess();
		return new ProcessInfo(currentProcess.MainWindowHandle, currentProcess.MainWindowTitle);
	}

	public IProcessInfo GetMainProcess()
	{
		if (_currentProcessInfo.Handle == IntPtr.Zero)
		{
			IProcessInfo currentProcessInfo = GetCurrentProcessInfo();
			if (currentProcessInfo.Title != "")
			{
				_currentProcessInfo = currentProcessInfo;
			}
		}
		return _currentProcessInfo;
	}

	public ICollection<IProcessInfo> GetAllProcesses()
	{
		ICollection<IProcessInfo> collection = new List<IProcessInfo>(_processCache.Count);
		foreach (KeyValuePair<IntPtr, string> item in _processCache)
		{
			collection.Add(new ProcessInfo(item.Key, item.Value));
		}
		return collection;
	}

	public void GetUpdatedProcesses(out ICollection<IProcessInfo> addedProcesses, out ICollection<IProcessInfo> updatedProcesses, out ICollection<IProcessInfo> removedProcesses)
	{
		addedProcesses = new List<IProcessInfo>(16);
		updatedProcesses = new List<IProcessInfo>(16);
		removedProcesses = new List<IProcessInfo>(16);
		IList<IntPtr> list = new List<IntPtr>(_processCache.Keys);
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			string processName = process.ProcessName;
			if (!IsMonitoredProcess(processName))
			{
				continue;
			}
			IntPtr mainWindowHandle = process.MainWindowHandle;
			if (mainWindowHandle == IntPtr.Zero)
			{
				continue;
			}
			string mainWindowTitle = process.MainWindowTitle;
			_processCache.TryGetValue(mainWindowHandle, out var value);
			if (value == null)
			{
				_processCache.Add(mainWindowHandle, mainWindowTitle);
				addedProcesses.Add(new ProcessInfo(mainWindowHandle, mainWindowTitle));
				continue;
			}
			if (value != mainWindowTitle)
			{
				_processCache[mainWindowHandle] = mainWindowTitle;
				updatedProcesses.Add(new ProcessInfo(mainWindowHandle, mainWindowTitle));
			}
			list.Remove(mainWindowHandle);
		}
		foreach (IntPtr item in list)
		{
			string title = _processCache[item];
			removedProcesses.Add(new ProcessInfo(item, title));
			_processCache.Remove(item);
		}
	}
}
