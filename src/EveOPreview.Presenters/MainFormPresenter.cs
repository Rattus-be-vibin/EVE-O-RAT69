using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using EveOPreview.Configuration;
using EveOPreview.Mediator.Messages;
using EveOPreview.View;
using MediatR;

namespace EveOPreview.Presenters;

public class MainFormPresenter : Presenter<IMainFormView>, IMainFormPresenter
{
	private const string FORUM_URL = "https://github.com/Rattus-be-vibin/EVE-O-RAT69";

	private readonly IMediator _mediator;

	private readonly IThumbnailConfiguration _configuration;

	private readonly IConfigurationStorage _configurationStorage;

	private readonly IDictionary<string, IThumbnailDescription> _descriptionsCache;

	private bool _suppressSizeNotifications;

	private bool _exitApplication;

	public MainFormPresenter(IApplicationController controller, IMainFormView view, IMediator mediator, IThumbnailConfiguration configuration, IConfigurationStorage configurationStorage)
		: base(controller, view)
	{
		_mediator = mediator;
		_configuration = configuration;
		_configurationStorage = configurationStorage;
		_descriptionsCache = new Dictionary<string, IThumbnailDescription>();
		_suppressSizeNotifications = false;
		_exitApplication = false;
		View.FormActivated = Activate;
		View.FormMinimized = Minimize;
		View.FormCloseRequested = Close;
		View.ApplicationSettingsChanged = SaveApplicationSettings;
		View.ThumbnailsSizeChanged = UpdateThumbnailsSize;
		View.ThumbnailStateChanged = UpdateThumbnailState;
		View.DocumentationLinkActivated = OpenDocumentationLink;
		View.ApplicationExitRequested = ExitApplication;
	}

	private void Activate()
	{
		_suppressSizeNotifications = true;
		LoadApplicationSettings();
		View.SetDocumentationUrl("github.com/Rattus-be-vibin/EVE-O-RAT69");
		View.SetVersionInfo(GetApplicationVersion());
		if (_configuration.MinimizeToTray)
		{
			View.Minimize();
		}
		_mediator.Send(new StartService());
		_suppressSizeNotifications = false;
	}

	private void Minimize()
	{
		if (_configuration.MinimizeToTray)
		{
			View.Hide();
		}
	}

	private void Close(ViewCloseRequest request)
	{
		if (_exitApplication || !View.MinimizeToTray)
		{
			_mediator.Send(new StopService()).Wait();
			_configurationStorage.Save();
			request.Allow = true;
		}
		else
		{
			request.Allow = false;
			View.Minimize();
		}
	}

	private async void UpdateThumbnailsSize()
	{
		if (!_suppressSizeNotifications)
		{
			SaveApplicationSettings();
			await _mediator.Publish(new ThumbnailConfiguredSizeUpdated());
		}
	}

	private void LoadApplicationSettings()
	{
		_configurationStorage.Load();
		View.MinimizeToTray = _configuration.MinimizeToTray;
		View.ThumbnailOpacity = _configuration.ThumbnailOpacity;
		View.EnableClientLayoutTracking = _configuration.EnableClientLayoutTracking;
		View.HideActiveClientThumbnail = _configuration.HideActiveClientThumbnail;
		View.MinimizeInactiveClients = _configuration.MinimizeInactiveClients;
		View.ShowThumbnailsAlwaysOnTop = _configuration.ShowThumbnailsAlwaysOnTop;
		View.HideThumbnailsOnLostFocus = _configuration.HideThumbnailsOnLostFocus;
		View.EnablePerClientThumbnailLayouts = _configuration.EnablePerClientThumbnailLayouts;
		View.SetThumbnailSizeLimitations(_configuration.ThumbnailMinimumSize, _configuration.ThumbnailMaximumSize);
		View.ThumbnailSize = _configuration.ThumbnailSize;
		View.EnableThumbnailZoom = _configuration.ThumbnailZoomEnabled;
		View.ThumbnailZoomFactor = _configuration.ThumbnailZoomFactor;
		View.ThumbnailZoomAnchor = ViewZoomAnchorConverter.Convert(_configuration.ThumbnailZoomAnchor);
		View.ShowThumbnailOverlays = _configuration.ShowThumbnailOverlays;
		View.ShowThumbnailFrames = _configuration.ShowThumbnailFrames;
		View.EnableActiveClientHighlight = _configuration.EnableActiveClientHighlight;
		View.ActiveClientHighlightColor = _configuration.ActiveClientHighlightColor;
	}

	private async void SaveApplicationSettings()
	{
		_configuration.MinimizeToTray = View.MinimizeToTray;
		_configuration.ThumbnailOpacity = (float)View.ThumbnailOpacity;
		_configuration.EnableClientLayoutTracking = View.EnableClientLayoutTracking;
		_configuration.HideActiveClientThumbnail = View.HideActiveClientThumbnail;
		_configuration.MinimizeInactiveClients = View.MinimizeInactiveClients;
		_configuration.ShowThumbnailsAlwaysOnTop = View.ShowThumbnailsAlwaysOnTop;
		_configuration.HideThumbnailsOnLostFocus = View.HideThumbnailsOnLostFocus;
		_configuration.EnablePerClientThumbnailLayouts = View.EnablePerClientThumbnailLayouts;
		_configuration.ThumbnailSize = View.ThumbnailSize;
		_configuration.ThumbnailZoomEnabled = View.EnableThumbnailZoom;
		_configuration.ThumbnailZoomFactor = View.ThumbnailZoomFactor;
		_configuration.ThumbnailZoomAnchor = ViewZoomAnchorConverter.Convert(View.ThumbnailZoomAnchor);
		_configuration.ShowThumbnailOverlays = View.ShowThumbnailOverlays;
		if (_configuration.ShowThumbnailFrames != View.ShowThumbnailFrames)
		{
			_configuration.ShowThumbnailFrames = View.ShowThumbnailFrames;
			await _mediator.Publish(new ThumbnailFrameSettingsUpdated());
		}
		_configuration.EnableActiveClientHighlight = View.EnableActiveClientHighlight;
		_configuration.ActiveClientHighlightColor = View.ActiveClientHighlightColor;
		_configurationStorage.Save();
		View.RefreshZoomSettings();
		await _mediator.Send(new SaveConfiguration());
	}

	public void AddThumbnails(IList<string> thumbnailTitles)
	{
		IList<IThumbnailDescription> list = new List<IThumbnailDescription>(thumbnailTitles.Count);
		lock (_descriptionsCache)
		{
			foreach (string thumbnailTitle in thumbnailTitles)
			{
				IThumbnailDescription thumbnailDescription = CreateThumbnailDescription(thumbnailTitle);
				_descriptionsCache[thumbnailTitle] = thumbnailDescription;
				list.Add(thumbnailDescription);
			}
		}
		View.AddThumbnails(list);
	}

	public void RemoveThumbnails(IList<string> thumbnailTitles)
	{
		IList<IThumbnailDescription> list = new List<IThumbnailDescription>(thumbnailTitles.Count);
		lock (_descriptionsCache)
		{
			foreach (string thumbnailTitle in thumbnailTitles)
			{
				if (_descriptionsCache.TryGetValue(thumbnailTitle, out var value))
				{
					_descriptionsCache.Remove(thumbnailTitle);
					list.Add(value);
				}
			}
		}
		View.RemoveThumbnails(list);
	}

	private IThumbnailDescription CreateThumbnailDescription(string title)
	{
		bool isDisabled = _configuration.IsThumbnailDisabled(title);
		return new ThumbnailDescription(title, isDisabled);
	}

	private async void UpdateThumbnailState(string title)
	{
		if (_descriptionsCache.TryGetValue(title, out var description))
		{
			_configuration.ToggleThumbnail(title, description.IsDisabled);
		}
		await _mediator.Send(new SaveConfiguration());
	}

	public void UpdateThumbnailSize(Size size)
	{
		_suppressSizeNotifications = true;
		View.ThumbnailSize = size;
		_suppressSizeNotifications = false;
	}

	private void OpenDocumentationLink()
	{
		ProcessStartInfo startInfo = new ProcessStartInfo(new Uri(FORUM_URL).AbsoluteUri);
		Process.Start(startInfo);
	}

	private string GetApplicationVersion()
	{
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		return $"{version.Major}.{version.Minor}.{version.Build}";
	}

	private void ExitApplication()
	{
		_exitApplication = true;
		View.Close();
	}
}
