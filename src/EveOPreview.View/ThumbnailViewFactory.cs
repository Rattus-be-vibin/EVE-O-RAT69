using System;
using System.Drawing;
using EveOPreview.Configuration;

namespace EveOPreview.View;

internal sealed class ThumbnailViewFactory : IThumbnailViewFactory
{
	private readonly IApplicationController _controller;

	private readonly bool _isCompatibilityModeEnabled;

	public ThumbnailViewFactory(IApplicationController controller, IThumbnailConfiguration configuration)
	{
		_controller = controller;
		_isCompatibilityModeEnabled = configuration.EnableCompatibilityMode;
	}

	public IThumbnailView Create(IntPtr id, string title, Size size)
	{
		IThumbnailView thumbnailView2;
		if (!_isCompatibilityModeEnabled)
		{
			IThumbnailView thumbnailView = _controller.Create<LiveThumbnailView>();
			thumbnailView2 = thumbnailView;
		}
		else
		{
			IThumbnailView thumbnailView = _controller.Create<StaticThumbnailView>();
			thumbnailView2 = thumbnailView;
		}
		IThumbnailView thumbnailView3 = thumbnailView2;
		thumbnailView3.Id = id;
		thumbnailView3.Title = title;
		thumbnailView3.ThumbnailSize = size;
		return thumbnailView3;
	}
}
