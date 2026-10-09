using System;

namespace EveOPreview.Services.Implementation;

internal sealed class ProcessInfo : IProcessInfo
{
	public IntPtr Handle { get; }

	public string Title { get; }

	public ProcessInfo(IntPtr handle, string title)
	{
		Handle = handle;
		Title = title;
	}
}
