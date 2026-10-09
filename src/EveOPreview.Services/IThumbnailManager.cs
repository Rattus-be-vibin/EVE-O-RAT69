using System;
using EveOPreview.View;

namespace EveOPreview.Services;

public interface IThumbnailManager
{
	void Start();

	void Stop();

	void UpdateThumbnailsSize();

	void UpdateThumbnailFrames();

	IThumbnailView GetClientByTitle(string title);

	IThumbnailView GetClientByPointer(IntPtr ptr);

	IThumbnailView GetActiveClient();
}
