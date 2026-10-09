using System.Drawing;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Services;

namespace EveOPreview.View;

internal sealed class StaticThumbnailView : ThumbnailView
{
	private readonly PictureBox _thumbnail;

	private IThumbnailConfiguration _config;

	public StaticThumbnailView(IWindowManager windowManager, IThumbnailConfiguration config, IThumbnailManager thumbnailManager)
		: base(windowManager, config, thumbnailManager)
	{
		_thumbnail = new StaticThumbnailImage
		{
			TabStop = false,
			SizeMode = PictureBoxSizeMode.StretchImage,
			Location = new Point(0, 0),
			Size = new Size(ClientSize.Width, ClientSize.Height)
		};
		Controls.Add(_thumbnail);
		_config = config;
	}

	protected override void RefreshThumbnail(bool forceRefresh)
	{
		if (forceRefresh)
		{
			Image staticThumbnail = WindowManager.GetStaticThumbnail(Id);
			if (staticThumbnail != null)
			{
				Image image = _thumbnail.Image;
				_thumbnail.Image = staticThumbnail;
				image?.Dispose();
			}
		}
	}

	protected override void ResizeThumbnail(int baseWidth, int baseHeight, int highlightWidthTop, int highlightWidthRight, int highlightWidthBottom, int highlightWidthLeft)
	{
		if (IsLocationUpdateRequired(_thumbnail.Location, highlightWidthLeft, highlightWidthTop))
		{
			_thumbnail.Location = new Point(highlightWidthLeft, highlightWidthTop);
		}
		int num = baseWidth - highlightWidthLeft - highlightWidthRight;
		int num2 = baseHeight - highlightWidthTop - highlightWidthBottom;
		if (IsSizeUpdateRequired(_thumbnail.Size, num, num2))
		{
			_thumbnail.Size = new Size(num, num2);
		}
	}

	private bool IsLocationUpdateRequired(Point currentLocation, int left, int top)
	{
		return currentLocation.X != left || currentLocation.Y != top;
	}

	private bool IsSizeUpdateRequired(Size currentSize, int width, int height)
	{
		return currentSize.Width != width || currentSize.Height != height;
	}
}
