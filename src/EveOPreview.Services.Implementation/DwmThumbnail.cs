using System;
using System.Runtime.InteropServices;
using EveOPreview.Services.Interop;

namespace EveOPreview.Services.Implementation;

internal class DwmThumbnail : IDwmThumbnail
{
	private readonly IWindowManager _windowManager;

	private IntPtr _handle;

	private DWM_THUMBNAIL_PROPERTIES _properties;

	public DwmThumbnail(IWindowManager windowManager)
	{
		_windowManager = windowManager;
		_handle = IntPtr.Zero;
	}

	public void Register(IntPtr destination, IntPtr source)
	{
		_properties = new DWM_THUMBNAIL_PROPERTIES();
		_properties.dwFlags = 29u;
		_properties.opacity = byte.MaxValue;
		_properties.fVisible = true;
		_properties.fSourceClientAreaOnly = true;
		if (!_windowManager.IsCompositionEnabled)
		{
			return;
		}
		try
		{
			_handle = DwmNativeMethods.DwmRegisterThumbnail(destination, source);
		}
		catch (ArgumentException)
		{
			_handle = IntPtr.Zero;
		}
		catch (COMException)
		{
			_handle = IntPtr.Zero;
		}
	}

	public void Unregister()
	{
		if (!_windowManager.IsCompositionEnabled || _handle == IntPtr.Zero)
		{
			return;
		}
		try
		{
			DwmNativeMethods.DwmUnregisterThumbnail(_handle);
		}
		catch (ArgumentException)
		{
		}
		catch (COMException)
		{
		}
	}

	public void Move(int left, int top, int right, int bottom)
	{
		_properties.rcDestination = new RECT(left, top, right, bottom);
	}

	public void Update()
	{
		if (!_windowManager.IsCompositionEnabled || _handle == IntPtr.Zero)
		{
			return;
		}
		try
		{
			DwmNativeMethods.DwmUpdateThumbnailProperties(_handle, _properties);
		}
		catch (ArgumentException)
		{
		}
		catch (COMException)
		{
		}
	}
}
