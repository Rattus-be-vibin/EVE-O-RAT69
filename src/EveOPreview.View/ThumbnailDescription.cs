namespace EveOPreview.View;

internal sealed class ThumbnailDescription : IThumbnailDescription
{
	public string Title { get; set; }

	public bool IsDisabled { get; set; }

	public ThumbnailDescription(string title, bool isDisabled)
	{
		Title = title;
		IsDisabled = isDisabled;
	}
}
