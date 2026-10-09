using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace EveOPreview.Configuration.Implementation;

internal sealed class ThumbnailConfiguration : IThumbnailConfiguration
{
	private bool _enablePerClientThumbnailLayouts;

	private bool _enableClientLayoutTracking;

	[JsonProperty("ConfigVersion")]
	public int ConfigVersion { get; set; }

	[JsonProperty("CycleGroup1ForwardHotkeys")]
	public List<string> CycleGroup1ForwardHotkeys { get; set; }

	[JsonProperty("CycleGroup1BackwardHotkeys")]
	public List<string> CycleGroup1BackwardHotkeys { get; set; }

	[JsonProperty("CycleGroup1ClientsOrder")]
	public Dictionary<string, int> CycleGroup1ClientsOrder { get; set; }

	[JsonProperty("CycleGroup2ForwardHotkeys")]
	public List<string> CycleGroup2ForwardHotkeys { get; set; }

	[JsonProperty("CycleGroup2BackwardHotkeys")]
	public List<string> CycleGroup2BackwardHotkeys { get; set; }

	[JsonProperty("CycleGroup2ClientsOrder")]
	public Dictionary<string, int> CycleGroup2ClientsOrder { get; set; }

	[JsonProperty("CycleGroup3ForwardHotkeys")]
	public List<string> CycleGroup3ForwardHotkeys { get; set; }

	[JsonProperty("CycleGroup3BackwardHotkeys")]
	public List<string> CycleGroup3BackwardHotkeys { get; set; }

	[JsonProperty("CycleGroup3ClientsOrder")]
	public Dictionary<string, int> CycleGroup3ClientsOrder { get; set; }

	[JsonProperty("CycleGroup4ForwardHotkeys")]
	public List<string> CycleGroup4ForwardHotkeys { get; set; }

	[JsonProperty("CycleGroup4BackwardHotkeys")]
	public List<string> CycleGroup4BackwardHotkeys { get; set; }

	[JsonProperty("CycleGroup4ClientsOrder")]
	public Dictionary<string, int> CycleGroup4ClientsOrder { get; set; }

	[JsonProperty("CycleGroup5ForwardHotkeys")]
	public List<string> CycleGroup5ForwardHotkeys { get; set; }

	[JsonProperty("CycleGroup5BackwardHotkeys")]
	public List<string> CycleGroup5BackwardHotkeys { get; set; }

	[JsonProperty("CycleGroup5ClientsOrder")]
	public Dictionary<string, int> CycleGroup5ClientsOrder { get; set; }

	[JsonProperty("CharSelectCycleForwardHotkeys")]
	public List<string> CharSelectCycleForwardHotkeys { get; set; }

	[JsonProperty("CharSelectCycleBackwardHotkeys")]
	public List<string> CharSelectCycleBackwardHotkeys { get; set; }

	[JsonProperty("PerClientActiveClientHighlightColor")]
	public Dictionary<string, Color> PerClientActiveClientHighlightColor { get; set; }

	public bool MinimizeToTray { get; set; }

	public int ThumbnailRefreshPeriod { get; set; }

	[JsonProperty("CompatibilityMode")]
	public bool EnableCompatibilityMode { get; set; }

	[JsonProperty("ThumbnailsOpacity")]
	public double ThumbnailOpacity { get; set; }

	public bool EnableClientLayoutTracking
	{
		get
		{
			return _enableClientLayoutTracking;
		}
		set
		{
			if (!value)
			{
				ClientLayout.Clear();
			}
			_enableClientLayoutTracking = value;
		}
	}

	public bool HideActiveClientThumbnail { get; set; }

	public bool MinimizeInactiveClients { get; set; }

	public bool ShowThumbnailsAlwaysOnTop { get; set; }

	public bool EnablePerClientThumbnailLayouts
	{
		get
		{
			return _enablePerClientThumbnailLayouts;
		}
		set
		{
			if (!value)
			{
				PerClientLayout.Clear();
			}
			_enablePerClientThumbnailLayouts = value;
		}
	}

	public bool HideThumbnailsOnLostFocus { get; set; }

	public bool HotkeysOnlyWhenClientActive { get; set; }

	public int HideThumbnailsDelay { get; set; }

	public Size ThumbnailSize { get; set; }

	public Size ThumbnailMaximumSize { get; set; }

	public Size ThumbnailMinimumSize { get; set; }

	public bool EnableThumbnailSnap { get; set; }

	[JsonProperty("EnableThumbnailZoom")]
	public bool ThumbnailZoomEnabled { get; set; }

	public int ThumbnailZoomFactor { get; set; }

	public ZoomAnchor ThumbnailZoomAnchor { get; set; }

	public bool ShowThumbnailOverlays { get; set; }

	public bool ShowThumbnailFrames { get; set; }

	public bool EnableActiveClientHighlight { get; set; }

	public Color ActiveClientHighlightColor { get; set; }

	public int ActiveClientHighlightThickness { get; set; }

	[JsonProperty("LoginThumbnailLocation")]
	public Point LoginThumbnailLocation { get; set; }

	[JsonProperty]
	private Dictionary<string, Dictionary<string, Point>> PerClientLayout { get; set; }

	[JsonProperty]
	private Dictionary<string, Point> FlatLayout { get; set; }

	[JsonProperty]
	private Dictionary<string, ClientLayout> ClientLayout { get; set; }

	[JsonProperty]
	private Dictionary<string, string> ClientHotkey { get; set; }

	[JsonProperty]
	private Dictionary<string, bool> DisableThumbnail { get; set; }

	[JsonProperty]
	private List<string> PriorityClients { get; set; }

	public ThumbnailConfiguration()
	{
		ConfigVersion = 1;
		CycleGroup1ForwardHotkeys = new List<string> { "F13" };
		CycleGroup1BackwardHotkeys = new List<string> { "Control+F13" };
		CycleGroup1ClientsOrder = new Dictionary<string, int>
		{
			{ "EVE - Example DPS Toon 1", 1 },
			{ "EVE - Example DPS Toon 2", 2 }
		};
		CycleGroup2ForwardHotkeys = new List<string> { "F14" };
		CycleGroup2BackwardHotkeys = new List<string> { "Control+F14" };
		CycleGroup2ClientsOrder = new Dictionary<string, int>
		{
			{ "EVE - Example Logi Toon 1", 1 },
			{ "EVE - Example Logi Toon 2", 2 }
		};
		CycleGroup3ForwardHotkeys = new List<string> { "F15" };
		CycleGroup3BackwardHotkeys = new List<string> { "Control+F15" };
		CycleGroup3ClientsOrder = new Dictionary<string, int>
		{
			{ "EVE - Example Tackle Toon 1", 1 },
			{ "EVE - Example Tackle Toon 2", 2 }
		};
		CycleGroup4ForwardHotkeys = new List<string> { "F16" };
		CycleGroup4BackwardHotkeys = new List<string> { "Control+F16" };
		CycleGroup4ClientsOrder = new Dictionary<string, int>
		{
			{ "EVE - Example Scout Toon 1", 1 },
			{ "EVE - Example Scout Toon 2", 2 }
		};
		CharSelectCycleForwardHotkeys = new List<string>();
		CharSelectCycleBackwardHotkeys = new List<string>();
		CycleGroup5ForwardHotkeys = new List<string> { "F17" };
		CycleGroup5BackwardHotkeys = new List<string> { "Control+F17" };
		CycleGroup5ClientsOrder = new Dictionary<string, int>
		{
			{ "EVE - Example Hauler Toon 1", 1 },
			{ "EVE - Example Hauler Toon 2", 2 }
		};
		PerClientActiveClientHighlightColor = new Dictionary<string, Color>
		{
			{
				"EVE - Example Toon 1",
				Color.Red
			},
			{
				"EVE - Example Toon 2",
				Color.Green
			}
		};
		PerClientLayout = new Dictionary<string, Dictionary<string, Point>>();
		FlatLayout = new Dictionary<string, Point>();
		ClientLayout = new Dictionary<string, ClientLayout>();
		ClientHotkey = new Dictionary<string, string>();
		DisableThumbnail = new Dictionary<string, bool>();
		PriorityClients = new List<string>();
		MinimizeToTray = false;
		ThumbnailRefreshPeriod = 500;
		EnableCompatibilityMode = false;
		ThumbnailOpacity = 0.5;
		EnableClientLayoutTracking = false;
		HideActiveClientThumbnail = false;
		MinimizeInactiveClients = false;
		ShowThumbnailsAlwaysOnTop = true;
		EnablePerClientThumbnailLayouts = false;
		HideThumbnailsOnLostFocus = false;
		HotkeysOnlyWhenClientActive = true;
		HideThumbnailsDelay = 2;
		ThumbnailSize = new Size(384, 216);
		ThumbnailMinimumSize = new Size(192, 108);
		ThumbnailMaximumSize = new Size(960, 540);
		EnableThumbnailSnap = true;
		ThumbnailZoomEnabled = false;
		ThumbnailZoomFactor = 2;
		ThumbnailZoomAnchor = ZoomAnchor.NW;
		ShowThumbnailOverlays = true;
		ShowThumbnailFrames = false;
		EnableActiveClientHighlight = false;
		ActiveClientHighlightColor = Color.GreenYellow;
		ActiveClientHighlightThickness = 3;
		LoginThumbnailLocation = new Point(5, 5);
	}

	public Point GetThumbnailLocation(string currentClient, string activeClient, Point defaultLocation)
	{
		if (EnablePerClientThumbnailLayouts && !string.IsNullOrEmpty(activeClient) && PerClientLayout.TryGetValue(activeClient, out var value) && value.TryGetValue(currentClient, out var value2))
		{
			return value2;
		}
		return FlatLayout.TryGetValue(currentClient, out value2) ? value2 : defaultLocation;
	}

	public void SetThumbnailLocation(string currentClient, string activeClient, Point location)
	{
		Dictionary<string, Point> value;
		if (EnablePerClientThumbnailLayouts)
		{
			if (string.IsNullOrEmpty(activeClient))
			{
				return;
			}
			if (!PerClientLayout.TryGetValue(activeClient, out value))
			{
				value = new Dictionary<string, Point>();
				PerClientLayout[activeClient] = value;
			}
		}
		else
		{
			value = FlatLayout;
		}
		value[currentClient] = location;
	}

	public ClientLayout GetClientLayout(string currentClient)
	{
		ClientLayout.TryGetValue(currentClient, out var value);
		return value;
	}

	public void SetClientLayout(string currentClient, ClientLayout layout)
	{
		ClientLayout[currentClient] = layout;
	}

	public Keys GetClientHotkey(string currentClient)
	{
		if (ClientHotkey.TryGetValue(currentClient, out var value))
		{
			object obj = new KeysConverter().ConvertFromInvariantString(value);
			return (obj != null) ? ((Keys)obj) : Keys.None;
		}
		return Keys.None;
	}

	public void SetClientHotkey(string currentClient, Keys hotkey)
	{
		ClientHotkey[currentClient] = new KeysConverter().ConvertToInvariantString(hotkey);
	}

	public Keys StringToKey(string hotkey)
	{
		object obj = new KeysConverter().ConvertFromInvariantString(hotkey);
		return (obj != null) ? ((Keys)obj) : Keys.None;
	}

	public bool IsPriorityClient(string currentClient)
	{
		return PriorityClients.Contains(currentClient);
	}

	public bool IsThumbnailDisabled(string currentClient)
	{
		bool value;
		return DisableThumbnail.TryGetValue(currentClient, out value) & value;
	}

	public void ToggleThumbnail(string currentClient, bool isDisabled)
	{
		DisableThumbnail[currentClient] = isDisabled;
	}

	public void ApplyRestrictions()
	{
		ThumbnailRefreshPeriod = ApplyRestrictions(ThumbnailRefreshPeriod, 300, 1000);
		ThumbnailSize = new Size(ApplyRestrictions(ThumbnailSize.Width, ThumbnailMinimumSize.Width, ThumbnailMaximumSize.Width), ApplyRestrictions(ThumbnailSize.Height, ThumbnailMinimumSize.Height, ThumbnailMaximumSize.Height));
		ThumbnailOpacity = (double)ApplyRestrictions((int)(ThumbnailOpacity * 100.0), 20, 100) / 100.0;
		ThumbnailZoomFactor = ApplyRestrictions(ThumbnailZoomFactor, 2, 10);
		ActiveClientHighlightThickness = ApplyRestrictions(ActiveClientHighlightThickness, 1, 6);
	}

	private static int ApplyRestrictions(int value, int minimum, int maximum)
	{
		if (value <= minimum)
		{
			return minimum;
		}
		if (value >= maximum)
		{
			return maximum;
		}
		return value;
	}
}
