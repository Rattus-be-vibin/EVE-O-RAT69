using System.Drawing;
using EveOPreview.Configuration;
using EveOPreview.Services;

namespace EveOPreview.View;

internal sealed class LiveThumbnailView : ThumbnailView
{
	private IDwmThumbnail _thumbnail;

	private Point _startLocation;

	private Point _endLocation;

	private IThumbnailConfiguration _config;

	public LiveThumbnailView(IWindowManager windowManager, IThumbnailConfiguration config, IThumbnailManager thumbnailManager)
		: base(windowManager, config, thumbnailManager)
	{
		_startLocation = new Point(0, 0);
		_endLocation = new Point(ClientSize);
		_config = config;
	}

	protected override void RefreshThumbnail(bool forceRefresh)
	{
		IDwmThumbnail dwmThumbnail = (forceRefresh ? _thumbnail : null);
		if ((_thumbnail == null) | forceRefresh)
		{
			RegisterThumbnail();
		}
		dwmThumbnail?.Unregister();
	}

	protected override void ResizeThumbnail(int baseWidth, int baseHeight, int highlightWidthTop, int highlightWidthRight, int highlightWidthBottom, int highlightWidthLeft)
	{
		int num = baseWidth - highlightWidthRight;
		int num2 = baseHeight - highlightWidthBottom;
		if (_startLocation.X != highlightWidthLeft || _startLocation.Y != highlightWidthTop || _endLocation.X != num || _endLocation.Y != num2)
		{
			_startLocation = new Point(highlightWidthLeft, highlightWidthTop);
			_endLocation = new Point(num, num2);
			_thumbnail.Move(highlightWidthLeft, highlightWidthTop, num, num2);
			_thumbnail.Update();
		}
	}

	private void RegisterThumbnail()
	{
		_thumbnail = WindowManager.GetLiveThumbnail(Handle, Id);
		_thumbnail.Move(_startLocation.X, _startLocation.Y, _endLocation.X, _endLocation.Y);
		_thumbnail.Update();
	}
}
