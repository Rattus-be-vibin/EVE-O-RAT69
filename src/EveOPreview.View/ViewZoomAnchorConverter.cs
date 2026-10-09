using EveOPreview.Configuration;

namespace EveOPreview.View;

internal static class ViewZoomAnchorConverter
{
	public static ZoomAnchor Convert(ViewZoomAnchor value)
	{
		return (ZoomAnchor)value;
	}

	public static ViewZoomAnchor Convert(ZoomAnchor value)
	{
		return (ViewZoomAnchor)value;
	}
}
