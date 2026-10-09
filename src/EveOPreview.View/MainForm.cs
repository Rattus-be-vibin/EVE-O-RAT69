using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EveOPreview.View;

public class MainForm : Form, IMainFormView, IView
{
	private readonly ApplicationContext _context;

	private readonly Dictionary<ViewZoomAnchor, RadioButton> _zoomAnchorMap;

	private ViewZoomAnchor _cachedThumbnailZoomAnchor;

	private bool _suppressEvents;

	private Size _minimumSize;

	private Size _maximumSize;

	private Color _activeClientHighlightColor;

	private IContainer components = null;

	private NotifyIcon NotifyIcon;

	private ContextMenuStrip TrayMenu;

	private TabPage ZoomTabPage;

	private CheckBox EnableClientLayoutTrackingCheckBox;

	private CheckBox HideActiveClientThumbnailCheckBox;

	private CheckBox ShowThumbnailsAlwaysOnTopCheckBox;

	private CheckBox HideThumbnailsOnLostFocusCheckBox;

	private CheckBox EnablePerClientThumbnailsLayoutsCheckBox;

	private CheckBox MinimizeToTrayCheckBox;

	private NumericUpDown ThumbnailsWidthNumericEdit;

	private NumericUpDown ThumbnailsHeightNumericEdit;

	private TrackBar ThumbnailOpacityTrackBar;

	private Panel ZoomAnchorPanel;

	private RadioButton ZoomAanchorNWRadioButton;

	private RadioButton ZoomAanchorNRadioButton;

	private RadioButton ZoomAanchorNERadioButton;

	private RadioButton ZoomAanchorWRadioButton;

	private RadioButton ZoomAanchorSERadioButton;

	private RadioButton ZoomAanchorCRadioButton;

	private RadioButton ZoomAanchorSRadioButton;

	private RadioButton ZoomAanchorERadioButton;

	private RadioButton ZoomAanchorSWRadioButton;

	private CheckBox EnableThumbnailZoomCheckBox;

	private NumericUpDown ThumbnailZoomFactorNumericEdit;

	private Label HighlightColorLabel;

	private Panel ActiveClientHighlightColorButton;

	private CheckBox EnableActiveClientHighlightCheckBox;

	private CheckBox ShowThumbnailOverlaysCheckBox;

	private CheckBox ShowThumbnailFramesCheckBox;

	private CheckedListBox ThumbnailsList;

	private LinkLabel DocumentationLink;

	private Label VersionLabel;

	private CheckBox MinimizeInactiveClientsCheckBox;

	public bool MinimizeToTray
	{
		get
		{
			return MinimizeToTrayCheckBox.Checked;
		}
		set
		{
			MinimizeToTrayCheckBox.Checked = value;
		}
	}

	public double ThumbnailOpacity
	{
		get
		{
			return Math.Min((double)ThumbnailOpacityTrackBar.Value / 100.0, 1.0);
		}
		set
		{
			int num = (int)(100.0 * value);
			if (num > 100)
			{
				num = 100;
			}
			else if (num < 10)
			{
				num = 10;
			}
			ThumbnailOpacityTrackBar.Value = num;
		}
	}

	public bool EnableClientLayoutTracking
	{
		get
		{
			return EnableClientLayoutTrackingCheckBox.Checked;
		}
		set
		{
			EnableClientLayoutTrackingCheckBox.Checked = value;
		}
	}

	public bool HideActiveClientThumbnail
	{
		get
		{
			return HideActiveClientThumbnailCheckBox.Checked;
		}
		set
		{
			HideActiveClientThumbnailCheckBox.Checked = value;
		}
	}

	public bool MinimizeInactiveClients
	{
		get
		{
			return MinimizeInactiveClientsCheckBox.Checked;
		}
		set
		{
			MinimizeInactiveClientsCheckBox.Checked = value;
		}
	}

	public bool ShowThumbnailsAlwaysOnTop
	{
		get
		{
			return ShowThumbnailsAlwaysOnTopCheckBox.Checked;
		}
		set
		{
			ShowThumbnailsAlwaysOnTopCheckBox.Checked = value;
		}
	}

	public bool HideThumbnailsOnLostFocus
	{
		get
		{
			return HideThumbnailsOnLostFocusCheckBox.Checked;
		}
		set
		{
			HideThumbnailsOnLostFocusCheckBox.Checked = value;
		}
	}

	public bool EnablePerClientThumbnailLayouts
	{
		get
		{
			return EnablePerClientThumbnailsLayoutsCheckBox.Checked;
		}
		set
		{
			EnablePerClientThumbnailsLayoutsCheckBox.Checked = value;
		}
	}

	public Size ThumbnailSize
	{
		get
		{
			return new Size((int)ThumbnailsWidthNumericEdit.Value, (int)ThumbnailsHeightNumericEdit.Value);
		}
		set
		{
			ThumbnailsWidthNumericEdit.Value = value.Width;
			ThumbnailsHeightNumericEdit.Value = value.Height;
		}
	}

	public bool EnableThumbnailZoom
	{
		get
		{
			return EnableThumbnailZoomCheckBox.Checked;
		}
		set
		{
			EnableThumbnailZoomCheckBox.Checked = value;
			RefreshZoomSettings();
		}
	}

	public int ThumbnailZoomFactor
	{
		get
		{
			return (int)ThumbnailZoomFactorNumericEdit.Value;
		}
		set
		{
			ThumbnailZoomFactorNumericEdit.Value = value;
		}
	}

	public ViewZoomAnchor ThumbnailZoomAnchor
	{
		get
		{
			if (_zoomAnchorMap[_cachedThumbnailZoomAnchor].Checked)
			{
				return _cachedThumbnailZoomAnchor;
			}
			foreach (KeyValuePair<ViewZoomAnchor, RadioButton> item in _zoomAnchorMap)
			{
				if (!item.Value.Checked)
				{
					continue;
				}
				_cachedThumbnailZoomAnchor = item.Key;
				return _cachedThumbnailZoomAnchor;
			}
			return ViewZoomAnchor.NW;
		}
		set
		{
			_cachedThumbnailZoomAnchor = value;
			_zoomAnchorMap[_cachedThumbnailZoomAnchor].Checked = true;
		}
	}

	public bool ShowThumbnailOverlays
	{
		get
		{
			return ShowThumbnailOverlaysCheckBox.Checked;
		}
		set
		{
			ShowThumbnailOverlaysCheckBox.Checked = value;
		}
	}

	public bool ShowThumbnailFrames
	{
		get
		{
			return ShowThumbnailFramesCheckBox.Checked;
		}
		set
		{
			ShowThumbnailFramesCheckBox.Checked = value;
		}
	}

	public bool EnableActiveClientHighlight
	{
		get
		{
			return EnableActiveClientHighlightCheckBox.Checked;
		}
		set
		{
			EnableActiveClientHighlightCheckBox.Checked = value;
		}
	}

	public Color ActiveClientHighlightColor
	{
		get
		{
			return _activeClientHighlightColor;
		}
		set
		{
			_activeClientHighlightColor = value;
			ActiveClientHighlightColorButton.BackColor = value;
		}
	}

	public Action ApplicationExitRequested { get; set; }

	public Action FormActivated { get; set; }

	public Action FormMinimized { get; set; }

	public Action<ViewCloseRequest> FormCloseRequested { get; set; }

	public Action ApplicationSettingsChanged { get; set; }

	public Action ThumbnailsSizeChanged { get; set; }

	public Action<string> ThumbnailStateChanged { get; set; }

	public Action DocumentationLinkActivated { get; set; }

	public MainForm(ApplicationContext context)
	{
		_context = context;
		_zoomAnchorMap = new Dictionary<ViewZoomAnchor, RadioButton>();
		_cachedThumbnailZoomAnchor = ViewZoomAnchor.NW;
		_suppressEvents = false;
		_minimumSize = new Size(80, 60);
		_maximumSize = new Size(80, 60);
		InitializeComponent();
		ThumbnailsList.DisplayMember = "Title";
		InitZoomAnchorMap();
	}

	public new void Show()
	{
		_context.MainForm = this;
		_suppressEvents = true;
		FormActivated?.Invoke();
		_suppressEvents = false;
		Application.Run(_context);
	}

	public void SetThumbnailSizeLimitations(Size minimumSize, Size maximumSize)
	{
		_minimumSize = minimumSize;
		_maximumSize = maximumSize;
	}

	public void Minimize()
	{
		WindowState = FormWindowState.Minimized;
	}

	public void SetVersionInfo(string version)
	{
		VersionLabel.Text = version;
	}

	public void SetDocumentationUrl(string url)
	{
		DocumentationLink.Text = url;
	}

	public void AddThumbnails(IList<IThumbnailDescription> thumbnails)
	{
		ThumbnailsList.BeginUpdate();
		foreach (IThumbnailDescription thumbnail in thumbnails)
		{
			ThumbnailsList.SetItemChecked(ThumbnailsList.Items.Add(thumbnail), thumbnail.IsDisabled);
		}
		ThumbnailsList.EndUpdate();
	}

	public void RemoveThumbnails(IList<IThumbnailDescription> thumbnails)
	{
		ThumbnailsList.BeginUpdate();
		foreach (IThumbnailDescription thumbnail in thumbnails)
		{
			ThumbnailsList.Items.Remove(thumbnail);
		}
		ThumbnailsList.EndUpdate();
	}

	public void RefreshZoomSettings()
	{
		bool enableThumbnailZoom = EnableThumbnailZoom;
		ThumbnailZoomFactorNumericEdit.Enabled = enableThumbnailZoom;
		ZoomAnchorPanel.Enabled = enableThumbnailZoom;
	}

	private void ContentTabControl_DrawItem(object sender, DrawItemEventArgs e)
	{
		TabControl tabControl = (TabControl)sender;
		TabPage tabPage = tabControl.TabPages[e.Index];
		Rectangle tabRect = tabControl.GetTabRect(e.Index);
		Graphics graphics = e.Graphics;
		Brush brush = new SolidBrush(SystemColors.ActiveCaptionText);
		Brush brush2 = ((e.State == DrawItemState.Selected) ? new SolidBrush(SystemColors.Control) : new SolidBrush(SystemColors.ControlDark));
		graphics.FillRectangle(brush2, e.Bounds);
		Font font = new Font("Arial", Font.Size * 1.5f, FontStyle.Bold, GraphicsUnit.Pixel);
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Center;
		graphics.DrawString(tabPage.Text, font, brush, tabRect, stringFormat);
	}

	private void OptionChanged_Handler(object sender, EventArgs e)
	{
		if (!_suppressEvents)
		{
			ApplicationSettingsChanged?.Invoke();
		}
	}

	private void ThumbnailSizeChanged_Handler(object sender, EventArgs e)
	{
		if (!_suppressEvents)
		{
			_suppressEvents = true;
			Size thumbnailSize = ThumbnailSize;
			thumbnailSize.Width = Math.Min(Math.Max(thumbnailSize.Width, _minimumSize.Width), _maximumSize.Width);
			thumbnailSize.Height = Math.Min(Math.Max(thumbnailSize.Height, _minimumSize.Height), _maximumSize.Height);
			ThumbnailSize = thumbnailSize;
			_suppressEvents = false;
			ThumbnailsSizeChanged?.Invoke();
		}
	}

	private void ActiveClientHighlightColorButton_Click(object sender, EventArgs e)
	{
		using (ColorDialog colorDialog = new ColorDialog())
		{
			colorDialog.Color = ActiveClientHighlightColor;
			if (colorDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			ActiveClientHighlightColor = colorDialog.Color;
		}
		OptionChanged_Handler(sender, e);
	}

	private void ThumbnailsList_ItemCheck_Handler(object sender, ItemCheckEventArgs e)
	{
		if (ThumbnailsList.Items[e.Index] is IThumbnailDescription thumbnailDescription)
		{
			thumbnailDescription.IsDisabled = e.NewValue == CheckState.Checked;
			ThumbnailStateChanged?.Invoke(thumbnailDescription.Title);
		}
	}

	private void DocumentationLinkClicked_Handler(object sender, LinkLabelLinkClickedEventArgs e)
	{
		DocumentationLinkActivated?.Invoke();
	}

	private void MainFormResize_Handler(object sender, EventArgs e)
	{
		if (WindowState == FormWindowState.Minimized)
		{
			FormMinimized?.Invoke();
		}
	}

	private void MainFormClosing_Handler(object sender, FormClosingEventArgs e)
	{
		ViewCloseRequest viewCloseRequest = new ViewCloseRequest();
		FormCloseRequested?.Invoke(viewCloseRequest);
		e.Cancel = !viewCloseRequest.Allow;
	}

	private void RestoreMainForm_Handler(object sender, EventArgs e)
	{
		base.Show();
		WindowState = FormWindowState.Normal;
		BringToFront();
	}

	private void ExitMenuItemClick_Handler(object sender, EventArgs e)
	{
		ApplicationExitRequested?.Invoke();
	}

	private void InitZoomAnchorMap()
	{
		_zoomAnchorMap[ViewZoomAnchor.NW] = ZoomAanchorNWRadioButton;
		_zoomAnchorMap[ViewZoomAnchor.N] = ZoomAanchorNRadioButton;
		_zoomAnchorMap[ViewZoomAnchor.NE] = ZoomAanchorNERadioButton;
		_zoomAnchorMap[ViewZoomAnchor.W] = ZoomAanchorWRadioButton;
		_zoomAnchorMap[ViewZoomAnchor.C] = ZoomAanchorCRadioButton;
		_zoomAnchorMap[ViewZoomAnchor.E] = ZoomAanchorERadioButton;
		_zoomAnchorMap[ViewZoomAnchor.SW] = ZoomAanchorSWRadioButton;
		_zoomAnchorMap[ViewZoomAnchor.S] = ZoomAanchorSRadioButton;
		_zoomAnchorMap[ViewZoomAnchor.SE] = ZoomAanchorSERadioButton;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EveOPreview.View.MainForm));
		this.MinimizeInactiveClientsCheckBox = new System.Windows.Forms.CheckBox();
		this.EnableClientLayoutTrackingCheckBox = new System.Windows.Forms.CheckBox();
		this.HideActiveClientThumbnailCheckBox = new System.Windows.Forms.CheckBox();
		this.ShowThumbnailsAlwaysOnTopCheckBox = new System.Windows.Forms.CheckBox();
		this.HideThumbnailsOnLostFocusCheckBox = new System.Windows.Forms.CheckBox();
		this.EnablePerClientThumbnailsLayoutsCheckBox = new System.Windows.Forms.CheckBox();
		this.MinimizeToTrayCheckBox = new System.Windows.Forms.CheckBox();
		this.ThumbnailsWidthNumericEdit = new System.Windows.Forms.NumericUpDown();
		this.ThumbnailsHeightNumericEdit = new System.Windows.Forms.NumericUpDown();
		this.ThumbnailOpacityTrackBar = new System.Windows.Forms.TrackBar();
		this.ZoomTabPage = new System.Windows.Forms.TabPage();
		this.ZoomAnchorPanel = new System.Windows.Forms.Panel();
		this.ZoomAanchorNWRadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorNRadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorNERadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorWRadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorSERadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorCRadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorSRadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorERadioButton = new System.Windows.Forms.RadioButton();
		this.ZoomAanchorSWRadioButton = new System.Windows.Forms.RadioButton();
		this.EnableThumbnailZoomCheckBox = new System.Windows.Forms.CheckBox();
		this.ThumbnailZoomFactorNumericEdit = new System.Windows.Forms.NumericUpDown();
		this.HighlightColorLabel = new System.Windows.Forms.Label();
		this.ActiveClientHighlightColorButton = new System.Windows.Forms.Panel();
		this.EnableActiveClientHighlightCheckBox = new System.Windows.Forms.CheckBox();
		this.ShowThumbnailOverlaysCheckBox = new System.Windows.Forms.CheckBox();
		this.ShowThumbnailFramesCheckBox = new System.Windows.Forms.CheckBox();
		this.ThumbnailsList = new System.Windows.Forms.CheckedListBox();
		this.VersionLabel = new System.Windows.Forms.Label();
		this.DocumentationLink = new System.Windows.Forms.LinkLabel();
		this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
		this.TrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
		System.Windows.Forms.ToolStripMenuItem toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
		System.Windows.Forms.ToolStripSeparator toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
		System.Windows.Forms.TabControl tabControl = new System.Windows.Forms.TabControl();
		System.Windows.Forms.TabPage tabPage = new System.Windows.Forms.TabPage();
		System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
		System.Windows.Forms.TabPage tabPage2 = new System.Windows.Forms.TabPage();
		System.Windows.Forms.Panel panel2 = new System.Windows.Forms.Panel();
		System.Windows.Forms.Label label = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label3 = new System.Windows.Forms.Label();
		System.Windows.Forms.Panel panel3 = new System.Windows.Forms.Panel();
		System.Windows.Forms.Label label4 = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label5 = new System.Windows.Forms.Label();
		System.Windows.Forms.TabPage tabPage3 = new System.Windows.Forms.TabPage();
		System.Windows.Forms.Panel panel4 = new System.Windows.Forms.Panel();
		System.Windows.Forms.TabPage tabPage4 = new System.Windows.Forms.TabPage();
		System.Windows.Forms.Panel panel5 = new System.Windows.Forms.Panel();
		System.Windows.Forms.Label label6 = new System.Windows.Forms.Label();
		System.Windows.Forms.TabPage tabPage5 = new System.Windows.Forms.TabPage();
		System.Windows.Forms.Panel panel6 = new System.Windows.Forms.Panel();
		System.Windows.Forms.Label label7 = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label8 = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label9 = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label10 = new System.Windows.Forms.Label();
		tabControl.SuspendLayout();
		tabPage.SuspendLayout();
		panel.SuspendLayout();
		tabPage2.SuspendLayout();
		panel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailsWidthNumericEdit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailsHeightNumericEdit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailOpacityTrackBar).BeginInit();
		this.ZoomTabPage.SuspendLayout();
		panel3.SuspendLayout();
		this.ZoomAnchorPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailZoomFactorNumericEdit).BeginInit();
		tabPage3.SuspendLayout();
		panel4.SuspendLayout();
		tabPage4.SuspendLayout();
		panel5.SuspendLayout();
		tabPage5.SuspendLayout();
		panel6.SuspendLayout();
		this.TrayMenu.SuspendLayout();
		base.SuspendLayout();
		toolStripMenuItem.Name = "RestoreWindowMenuItem";
		toolStripMenuItem.Size = new System.Drawing.Size(151, 22);
		toolStripMenuItem.Text = "Restore";
		toolStripMenuItem.Click += new System.EventHandler(this.RestoreMainForm_Handler);
		toolStripMenuItem2.Name = "ExitMenuItem";
		toolStripMenuItem2.Size = new System.Drawing.Size(151, 22);
		toolStripMenuItem2.Text = "Exit";
		toolStripMenuItem2.Click += new System.EventHandler(this.ExitMenuItemClick_Handler);
		toolStripMenuItem3.Enabled = false;
		toolStripMenuItem3.Name = "TitleMenuItem";
		toolStripMenuItem3.Size = new System.Drawing.Size(151, 22);
		toolStripMenuItem3.Text = "EVE-O Preview";
		toolStripSeparator.Name = "SeparatorMenuItem";
		toolStripSeparator.Size = new System.Drawing.Size(148, 6);
		tabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
		tabControl.Controls.Add(tabPage);
		tabControl.Controls.Add(tabPage2);
		tabControl.Controls.Add(this.ZoomTabPage);
		tabControl.Controls.Add(tabPage3);
		tabControl.Controls.Add(tabPage4);
		tabControl.Controls.Add(tabPage5);
		tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
		tabControl.ItemSize = new System.Drawing.Size(35, 120);
		tabControl.Location = new System.Drawing.Point(0, 0);
		tabControl.Multiline = true;
		tabControl.Name = "ContentTabControl";
		tabControl.SelectedIndex = 0;
		tabControl.Size = new System.Drawing.Size(390, 218);
		tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
		tabControl.TabIndex = 6;
		tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ContentTabControl_DrawItem);
		tabPage.BackColor = System.Drawing.SystemColors.Control;
		tabPage.Controls.Add(panel);
		tabPage.Location = new System.Drawing.Point(124, 4);
		tabPage.Name = "GeneralTabPage";
		tabPage.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		tabPage.Size = new System.Drawing.Size(262, 210);
		tabPage.TabIndex = 0;
		tabPage.Text = "General";
		panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel.Controls.Add(this.MinimizeInactiveClientsCheckBox);
		panel.Controls.Add(this.EnableClientLayoutTrackingCheckBox);
		panel.Controls.Add(this.HideActiveClientThumbnailCheckBox);
		panel.Controls.Add(this.ShowThumbnailsAlwaysOnTopCheckBox);
		panel.Controls.Add(this.HideThumbnailsOnLostFocusCheckBox);
		panel.Controls.Add(this.EnablePerClientThumbnailsLayoutsCheckBox);
		panel.Controls.Add(this.MinimizeToTrayCheckBox);
		panel.Dock = System.Windows.Forms.DockStyle.Fill;
		panel.Location = new System.Drawing.Point(3, 3);
		panel.Name = "GeneralSettingsPanel";
		panel.Size = new System.Drawing.Size(256, 204);
		panel.TabIndex = 18;
		this.MinimizeInactiveClientsCheckBox.AutoSize = true;
		this.MinimizeInactiveClientsCheckBox.Location = new System.Drawing.Point(8, 79);
		this.MinimizeInactiveClientsCheckBox.Name = "MinimizeInactiveClientsCheckBox";
		this.MinimizeInactiveClientsCheckBox.Size = new System.Drawing.Size(163, 17);
		this.MinimizeInactiveClientsCheckBox.TabIndex = 24;
		this.MinimizeInactiveClientsCheckBox.Text = "Minimize inactive EVE clients";
		this.MinimizeInactiveClientsCheckBox.UseVisualStyleBackColor = true;
		this.MinimizeInactiveClientsCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.EnableClientLayoutTrackingCheckBox.AutoSize = true;
		this.EnableClientLayoutTrackingCheckBox.Location = new System.Drawing.Point(8, 31);
		this.EnableClientLayoutTrackingCheckBox.Name = "EnableClientLayoutTrackingCheckBox";
		this.EnableClientLayoutTrackingCheckBox.Size = new System.Drawing.Size(127, 17);
		this.EnableClientLayoutTrackingCheckBox.TabIndex = 19;
		this.EnableClientLayoutTrackingCheckBox.Text = "Track client locations";
		this.EnableClientLayoutTrackingCheckBox.UseVisualStyleBackColor = true;
		this.EnableClientLayoutTrackingCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.HideActiveClientThumbnailCheckBox.AutoSize = true;
		this.HideActiveClientThumbnailCheckBox.Checked = true;
		this.HideActiveClientThumbnailCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.HideActiveClientThumbnailCheckBox.Location = new System.Drawing.Point(8, 55);
		this.HideActiveClientThumbnailCheckBox.Name = "HideActiveClientThumbnailCheckBox";
		this.HideActiveClientThumbnailCheckBox.Size = new System.Drawing.Size(184, 17);
		this.HideActiveClientThumbnailCheckBox.TabIndex = 20;
		this.HideActiveClientThumbnailCheckBox.Text = "Hide preview of active EVE client";
		this.HideActiveClientThumbnailCheckBox.UseVisualStyleBackColor = true;
		this.HideActiveClientThumbnailCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ShowThumbnailsAlwaysOnTopCheckBox.AutoSize = true;
		this.ShowThumbnailsAlwaysOnTopCheckBox.Checked = true;
		this.ShowThumbnailsAlwaysOnTopCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.ShowThumbnailsAlwaysOnTopCheckBox.Location = new System.Drawing.Point(8, 103);
		this.ShowThumbnailsAlwaysOnTopCheckBox.Name = "ShowThumbnailsAlwaysOnTopCheckBox";
		this.ShowThumbnailsAlwaysOnTopCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.ShowThumbnailsAlwaysOnTopCheckBox.Size = new System.Drawing.Size(137, 17);
		this.ShowThumbnailsAlwaysOnTopCheckBox.TabIndex = 21;
		this.ShowThumbnailsAlwaysOnTopCheckBox.Text = "Previews always on top";
		this.ShowThumbnailsAlwaysOnTopCheckBox.UseVisualStyleBackColor = true;
		this.ShowThumbnailsAlwaysOnTopCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.HideThumbnailsOnLostFocusCheckBox.AutoSize = true;
		this.HideThumbnailsOnLostFocusCheckBox.Checked = true;
		this.HideThumbnailsOnLostFocusCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.HideThumbnailsOnLostFocusCheckBox.Location = new System.Drawing.Point(8, 127);
		this.HideThumbnailsOnLostFocusCheckBox.Name = "HideThumbnailsOnLostFocusCheckBox";
		this.HideThumbnailsOnLostFocusCheckBox.Size = new System.Drawing.Size(234, 17);
		this.HideThumbnailsOnLostFocusCheckBox.TabIndex = 22;
		this.HideThumbnailsOnLostFocusCheckBox.Text = "Hide previews when EVE client is not active";
		this.HideThumbnailsOnLostFocusCheckBox.UseVisualStyleBackColor = true;
		this.HideThumbnailsOnLostFocusCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.EnablePerClientThumbnailsLayoutsCheckBox.AutoSize = true;
		this.EnablePerClientThumbnailsLayoutsCheckBox.Checked = true;
		this.EnablePerClientThumbnailsLayoutsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.EnablePerClientThumbnailsLayoutsCheckBox.Location = new System.Drawing.Point(8, 151);
		this.EnablePerClientThumbnailsLayoutsCheckBox.Name = "EnablePerClientThumbnailsLayoutsCheckBox";
		this.EnablePerClientThumbnailsLayoutsCheckBox.Size = new System.Drawing.Size(185, 17);
		this.EnablePerClientThumbnailsLayoutsCheckBox.TabIndex = 23;
		this.EnablePerClientThumbnailsLayoutsCheckBox.Text = "Unique layout for each EVE client";
		this.EnablePerClientThumbnailsLayoutsCheckBox.UseVisualStyleBackColor = true;
		this.EnablePerClientThumbnailsLayoutsCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.MinimizeToTrayCheckBox.AutoSize = true;
		this.MinimizeToTrayCheckBox.Location = new System.Drawing.Point(8, 7);
		this.MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
		this.MinimizeToTrayCheckBox.Size = new System.Drawing.Size(139, 17);
		this.MinimizeToTrayCheckBox.TabIndex = 18;
		this.MinimizeToTrayCheckBox.Text = "Minimize to System Tray";
		this.MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;
		this.MinimizeToTrayCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		tabPage2.BackColor = System.Drawing.SystemColors.Control;
		tabPage2.Controls.Add(panel2);
		tabPage2.Location = new System.Drawing.Point(124, 4);
		tabPage2.Name = "ThumbnailTabPage";
		tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
		tabPage2.Size = new System.Drawing.Size(262, 210);
		tabPage2.TabIndex = 1;
		tabPage2.Text = "Thumbnail";
		panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel2.Controls.Add(label);
		panel2.Controls.Add(label2);
		panel2.Controls.Add(this.ThumbnailsWidthNumericEdit);
		panel2.Controls.Add(this.ThumbnailsHeightNumericEdit);
		panel2.Controls.Add(this.ThumbnailOpacityTrackBar);
		panel2.Controls.Add(label3);
		panel2.Dock = System.Windows.Forms.DockStyle.Fill;
		panel2.Location = new System.Drawing.Point(3, 3);
		panel2.Name = "ThumbnailSettingsPanel";
		panel2.Size = new System.Drawing.Size(256, 204);
		panel2.TabIndex = 19;
		label.AutoSize = true;
		label.Location = new System.Drawing.Point(8, 57);
		label.Name = "HeigthLabel";
		label.Size = new System.Drawing.Size(90, 13);
		label.TabIndex = 24;
		label.Text = "Thumbnail Heigth";
		label2.AutoSize = true;
		label2.Location = new System.Drawing.Point(8, 33);
		label2.Name = "WidthLabel";
		label2.Size = new System.Drawing.Size(87, 13);
		label2.TabIndex = 23;
		label2.Text = "Thumbnail Width";
		this.ThumbnailsWidthNumericEdit.BackColor = System.Drawing.SystemColors.Window;
		this.ThumbnailsWidthNumericEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.ThumbnailsWidthNumericEdit.CausesValidation = false;
		this.ThumbnailsWidthNumericEdit.Increment = new decimal(new int[4] { 10, 0, 0, 0 });
		this.ThumbnailsWidthNumericEdit.Location = new System.Drawing.Point(105, 31);
		this.ThumbnailsWidthNumericEdit.Maximum = new decimal(new int[4] { 999999, 0, 0, 0 });
		this.ThumbnailsWidthNumericEdit.Name = "ThumbnailsWidthNumericEdit";
		this.ThumbnailsWidthNumericEdit.Size = new System.Drawing.Size(48, 20);
		this.ThumbnailsWidthNumericEdit.TabIndex = 21;
		this.ThumbnailsWidthNumericEdit.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.ThumbnailsWidthNumericEdit.ValueChanged += new System.EventHandler(this.ThumbnailSizeChanged_Handler);
		this.ThumbnailsHeightNumericEdit.BackColor = System.Drawing.SystemColors.Window;
		this.ThumbnailsHeightNumericEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.ThumbnailsHeightNumericEdit.CausesValidation = false;
		this.ThumbnailsHeightNumericEdit.Increment = new decimal(new int[4] { 10, 0, 0, 0 });
		this.ThumbnailsHeightNumericEdit.Location = new System.Drawing.Point(105, 55);
		this.ThumbnailsHeightNumericEdit.Maximum = new decimal(new int[4] { 99999999, 0, 0, 0 });
		this.ThumbnailsHeightNumericEdit.Name = "ThumbnailsHeightNumericEdit";
		this.ThumbnailsHeightNumericEdit.Size = new System.Drawing.Size(48, 20);
		this.ThumbnailsHeightNumericEdit.TabIndex = 22;
		this.ThumbnailsHeightNumericEdit.Value = new decimal(new int[4] { 70, 0, 0, 0 });
		this.ThumbnailsHeightNumericEdit.ValueChanged += new System.EventHandler(this.ThumbnailSizeChanged_Handler);
		this.ThumbnailOpacityTrackBar.AutoSize = false;
		this.ThumbnailOpacityTrackBar.LargeChange = 10;
		this.ThumbnailOpacityTrackBar.Location = new System.Drawing.Point(61, 6);
		this.ThumbnailOpacityTrackBar.Maximum = 100;
		this.ThumbnailOpacityTrackBar.Minimum = 20;
		this.ThumbnailOpacityTrackBar.Name = "ThumbnailOpacityTrackBar";
		this.ThumbnailOpacityTrackBar.Size = new System.Drawing.Size(191, 22);
		this.ThumbnailOpacityTrackBar.TabIndex = 20;
		this.ThumbnailOpacityTrackBar.TickFrequency = 10;
		this.ThumbnailOpacityTrackBar.Value = 20;
		this.ThumbnailOpacityTrackBar.ValueChanged += new System.EventHandler(this.OptionChanged_Handler);
		label3.AutoSize = true;
		label3.Location = new System.Drawing.Point(8, 9);
		label3.Name = "OpacityLabel";
		label3.Size = new System.Drawing.Size(43, 13);
		label3.TabIndex = 19;
		label3.Text = "Opacity";
		this.ZoomTabPage.BackColor = System.Drawing.SystemColors.Control;
		this.ZoomTabPage.Controls.Add(panel3);
		this.ZoomTabPage.Location = new System.Drawing.Point(124, 4);
		this.ZoomTabPage.Name = "ZoomTabPage";
		this.ZoomTabPage.Size = new System.Drawing.Size(262, 210);
		this.ZoomTabPage.TabIndex = 2;
		this.ZoomTabPage.Text = "Zoom";
		panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel3.Controls.Add(label4);
		panel3.Controls.Add(this.ZoomAnchorPanel);
		panel3.Controls.Add(label5);
		panel3.Controls.Add(this.EnableThumbnailZoomCheckBox);
		panel3.Controls.Add(this.ThumbnailZoomFactorNumericEdit);
		panel3.Dock = System.Windows.Forms.DockStyle.Fill;
		panel3.Location = new System.Drawing.Point(0, 0);
		panel3.Name = "ZoomSettingsPanel";
		panel3.Size = new System.Drawing.Size(262, 210);
		panel3.TabIndex = 36;
		label4.AutoSize = true;
		label4.Location = new System.Drawing.Point(8, 33);
		label4.Name = "ZoomFactorLabel";
		label4.Size = new System.Drawing.Size(67, 13);
		label4.TabIndex = 39;
		label4.Text = "Zoom Factor";
		this.ZoomAnchorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorNWRadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorNRadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorNERadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorWRadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorSERadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorCRadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorSRadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorERadioButton);
		this.ZoomAnchorPanel.Controls.Add(this.ZoomAanchorSWRadioButton);
		this.ZoomAnchorPanel.Location = new System.Drawing.Point(81, 54);
		this.ZoomAnchorPanel.Name = "ZoomAnchorPanel";
		this.ZoomAnchorPanel.Size = new System.Drawing.Size(77, 73);
		this.ZoomAnchorPanel.TabIndex = 38;
		this.ZoomAanchorNWRadioButton.AutoSize = true;
		this.ZoomAanchorNWRadioButton.Location = new System.Drawing.Point(3, 3);
		this.ZoomAanchorNWRadioButton.Name = "ZoomAanchorNWRadioButton";
		this.ZoomAanchorNWRadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorNWRadioButton.TabIndex = 0;
		this.ZoomAanchorNWRadioButton.TabStop = true;
		this.ZoomAanchorNWRadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorNWRadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorNRadioButton.AutoSize = true;
		this.ZoomAanchorNRadioButton.Location = new System.Drawing.Point(31, 3);
		this.ZoomAanchorNRadioButton.Name = "ZoomAanchorNRadioButton";
		this.ZoomAanchorNRadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorNRadioButton.TabIndex = 1;
		this.ZoomAanchorNRadioButton.TabStop = true;
		this.ZoomAanchorNRadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorNRadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorNERadioButton.AutoSize = true;
		this.ZoomAanchorNERadioButton.Location = new System.Drawing.Point(59, 3);
		this.ZoomAanchorNERadioButton.Name = "ZoomAanchorNERadioButton";
		this.ZoomAanchorNERadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorNERadioButton.TabIndex = 2;
		this.ZoomAanchorNERadioButton.TabStop = true;
		this.ZoomAanchorNERadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorNERadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorWRadioButton.AutoSize = true;
		this.ZoomAanchorWRadioButton.Location = new System.Drawing.Point(3, 29);
		this.ZoomAanchorWRadioButton.Name = "ZoomAanchorWRadioButton";
		this.ZoomAanchorWRadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorWRadioButton.TabIndex = 3;
		this.ZoomAanchorWRadioButton.TabStop = true;
		this.ZoomAanchorWRadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorWRadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorSERadioButton.AutoSize = true;
		this.ZoomAanchorSERadioButton.Location = new System.Drawing.Point(59, 55);
		this.ZoomAanchorSERadioButton.Name = "ZoomAanchorSERadioButton";
		this.ZoomAanchorSERadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorSERadioButton.TabIndex = 8;
		this.ZoomAanchorSERadioButton.TabStop = true;
		this.ZoomAanchorSERadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorSERadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorCRadioButton.AutoSize = true;
		this.ZoomAanchorCRadioButton.Location = new System.Drawing.Point(31, 29);
		this.ZoomAanchorCRadioButton.Name = "ZoomAanchorCRadioButton";
		this.ZoomAanchorCRadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorCRadioButton.TabIndex = 4;
		this.ZoomAanchorCRadioButton.TabStop = true;
		this.ZoomAanchorCRadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorCRadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorSRadioButton.AutoSize = true;
		this.ZoomAanchorSRadioButton.Location = new System.Drawing.Point(31, 55);
		this.ZoomAanchorSRadioButton.Name = "ZoomAanchorSRadioButton";
		this.ZoomAanchorSRadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorSRadioButton.TabIndex = 7;
		this.ZoomAanchorSRadioButton.TabStop = true;
		this.ZoomAanchorSRadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorSRadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorERadioButton.AutoSize = true;
		this.ZoomAanchorERadioButton.Location = new System.Drawing.Point(59, 29);
		this.ZoomAanchorERadioButton.Name = "ZoomAanchorERadioButton";
		this.ZoomAanchorERadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorERadioButton.TabIndex = 5;
		this.ZoomAanchorERadioButton.TabStop = true;
		this.ZoomAanchorERadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorERadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ZoomAanchorSWRadioButton.AutoSize = true;
		this.ZoomAanchorSWRadioButton.Location = new System.Drawing.Point(3, 55);
		this.ZoomAanchorSWRadioButton.Name = "ZoomAanchorSWRadioButton";
		this.ZoomAanchorSWRadioButton.Size = new System.Drawing.Size(14, 13);
		this.ZoomAanchorSWRadioButton.TabIndex = 6;
		this.ZoomAanchorSWRadioButton.TabStop = true;
		this.ZoomAanchorSWRadioButton.UseVisualStyleBackColor = true;
		this.ZoomAanchorSWRadioButton.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		label5.AutoSize = true;
		label5.Location = new System.Drawing.Point(8, 57);
		label5.Name = "ZoomAnchorLabel";
		label5.Size = new System.Drawing.Size(41, 13);
		label5.TabIndex = 40;
		label5.Text = "Anchor";
		this.EnableThumbnailZoomCheckBox.AutoSize = true;
		this.EnableThumbnailZoomCheckBox.Checked = true;
		this.EnableThumbnailZoomCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.EnableThumbnailZoomCheckBox.Location = new System.Drawing.Point(8, 7);
		this.EnableThumbnailZoomCheckBox.Name = "EnableThumbnailZoomCheckBox";
		this.EnableThumbnailZoomCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.EnableThumbnailZoomCheckBox.Size = new System.Drawing.Size(98, 17);
		this.EnableThumbnailZoomCheckBox.TabIndex = 36;
		this.EnableThumbnailZoomCheckBox.Text = "Zoom on hover";
		this.EnableThumbnailZoomCheckBox.UseVisualStyleBackColor = true;
		this.EnableThumbnailZoomCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ThumbnailZoomFactorNumericEdit.BackColor = System.Drawing.SystemColors.Window;
		this.ThumbnailZoomFactorNumericEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.ThumbnailZoomFactorNumericEdit.Location = new System.Drawing.Point(81, 31);
		this.ThumbnailZoomFactorNumericEdit.Maximum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.ThumbnailZoomFactorNumericEdit.Minimum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.ThumbnailZoomFactorNumericEdit.Name = "ThumbnailZoomFactorNumericEdit";
		this.ThumbnailZoomFactorNumericEdit.Size = new System.Drawing.Size(38, 20);
		this.ThumbnailZoomFactorNumericEdit.TabIndex = 37;
		this.ThumbnailZoomFactorNumericEdit.Value = new decimal(new int[4] { 2, 0, 0, 0 });
		this.ThumbnailZoomFactorNumericEdit.ValueChanged += new System.EventHandler(this.OptionChanged_Handler);
		tabPage3.BackColor = System.Drawing.SystemColors.Control;
		tabPage3.Controls.Add(panel4);
		tabPage3.Location = new System.Drawing.Point(124, 4);
		tabPage3.Name = "OverlayTabPage";
		tabPage3.Size = new System.Drawing.Size(262, 210);
		tabPage3.TabIndex = 3;
		tabPage3.Text = "Overlay";
		panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel4.Controls.Add(this.HighlightColorLabel);
		panel4.Controls.Add(this.ActiveClientHighlightColorButton);
		panel4.Controls.Add(this.EnableActiveClientHighlightCheckBox);
		panel4.Controls.Add(this.ShowThumbnailOverlaysCheckBox);
		panel4.Controls.Add(this.ShowThumbnailFramesCheckBox);
		panel4.Dock = System.Windows.Forms.DockStyle.Fill;
		panel4.Location = new System.Drawing.Point(0, 0);
		panel4.Name = "OverlaySettingsPanel";
		panel4.Size = new System.Drawing.Size(262, 210);
		panel4.TabIndex = 25;
		this.HighlightColorLabel.AutoSize = true;
		this.HighlightColorLabel.Location = new System.Drawing.Point(5, 78);
		this.HighlightColorLabel.Name = "HighlightColorLabel";
		this.HighlightColorLabel.Size = new System.Drawing.Size(31, 13);
		this.HighlightColorLabel.TabIndex = 29;
		this.HighlightColorLabel.Text = "Color";
		this.ActiveClientHighlightColorButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.ActiveClientHighlightColorButton.Location = new System.Drawing.Point(42, 77);
		this.ActiveClientHighlightColorButton.Name = "ActiveClientHighlightColorButton";
		this.ActiveClientHighlightColorButton.Size = new System.Drawing.Size(93, 17);
		this.ActiveClientHighlightColorButton.TabIndex = 28;
		this.ActiveClientHighlightColorButton.Click += new System.EventHandler(this.ActiveClientHighlightColorButton_Click);
		this.EnableActiveClientHighlightCheckBox.AutoSize = true;
		this.EnableActiveClientHighlightCheckBox.Checked = true;
		this.EnableActiveClientHighlightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.EnableActiveClientHighlightCheckBox.Location = new System.Drawing.Point(8, 55);
		this.EnableActiveClientHighlightCheckBox.Name = "EnableActiveClientHighlightCheckBox";
		this.EnableActiveClientHighlightCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.EnableActiveClientHighlightCheckBox.Size = new System.Drawing.Size(127, 17);
		this.EnableActiveClientHighlightCheckBox.TabIndex = 27;
		this.EnableActiveClientHighlightCheckBox.Text = "Highlight active client";
		this.EnableActiveClientHighlightCheckBox.UseVisualStyleBackColor = true;
		this.EnableActiveClientHighlightCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ShowThumbnailOverlaysCheckBox.AutoSize = true;
		this.ShowThumbnailOverlaysCheckBox.Checked = true;
		this.ShowThumbnailOverlaysCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.ShowThumbnailOverlaysCheckBox.Location = new System.Drawing.Point(8, 7);
		this.ShowThumbnailOverlaysCheckBox.Name = "ShowThumbnailOverlaysCheckBox";
		this.ShowThumbnailOverlaysCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.ShowThumbnailOverlaysCheckBox.Size = new System.Drawing.Size(90, 17);
		this.ShowThumbnailOverlaysCheckBox.TabIndex = 25;
		this.ShowThumbnailOverlaysCheckBox.Text = "Show overlay";
		this.ShowThumbnailOverlaysCheckBox.UseVisualStyleBackColor = true;
		this.ShowThumbnailOverlaysCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		this.ShowThumbnailFramesCheckBox.AutoSize = true;
		this.ShowThumbnailFramesCheckBox.Checked = true;
		this.ShowThumbnailFramesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.ShowThumbnailFramesCheckBox.Location = new System.Drawing.Point(8, 31);
		this.ShowThumbnailFramesCheckBox.Name = "ShowThumbnailFramesCheckBox";
		this.ShowThumbnailFramesCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
		this.ShowThumbnailFramesCheckBox.Size = new System.Drawing.Size(87, 17);
		this.ShowThumbnailFramesCheckBox.TabIndex = 26;
		this.ShowThumbnailFramesCheckBox.Text = "Show frames";
		this.ShowThumbnailFramesCheckBox.UseVisualStyleBackColor = true;
		this.ShowThumbnailFramesCheckBox.CheckedChanged += new System.EventHandler(this.OptionChanged_Handler);
		tabPage4.BackColor = System.Drawing.SystemColors.Control;
		tabPage4.Controls.Add(panel5);
		tabPage4.Location = new System.Drawing.Point(124, 4);
		tabPage4.Name = "ClientsTabPage";
		tabPage4.Size = new System.Drawing.Size(262, 210);
		tabPage4.TabIndex = 4;
		tabPage4.Text = "Active Clients";
		panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel5.Controls.Add(this.ThumbnailsList);
		panel5.Controls.Add(label6);
		panel5.Dock = System.Windows.Forms.DockStyle.Fill;
		panel5.Location = new System.Drawing.Point(0, 0);
		panel5.Name = "ClientsPanel";
		panel5.Size = new System.Drawing.Size(262, 210);
		panel5.TabIndex = 32;
		this.ThumbnailsList.BackColor = System.Drawing.SystemColors.Window;
		this.ThumbnailsList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.ThumbnailsList.CheckOnClick = true;
		this.ThumbnailsList.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.ThumbnailsList.FormattingEnabled = true;
		this.ThumbnailsList.IntegralHeight = false;
		this.ThumbnailsList.Location = new System.Drawing.Point(0, 28);
		this.ThumbnailsList.Name = "ThumbnailsList";
		this.ThumbnailsList.Size = new System.Drawing.Size(260, 180);
		this.ThumbnailsList.TabIndex = 34;
		this.ThumbnailsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ThumbnailsList_ItemCheck_Handler);
		label6.AutoSize = true;
		label6.Location = new System.Drawing.Point(8, 9);
		label6.Name = "ThumbnailsListLabel";
		label6.Size = new System.Drawing.Size(162, 13);
		label6.TabIndex = 33;
		label6.Text = "Thumbnails (check to force hide)";
		tabPage5.BackColor = System.Drawing.SystemColors.Control;
		tabPage5.Controls.Add(panel6);
		tabPage5.Location = new System.Drawing.Point(124, 4);
		tabPage5.Name = "AboutTabPage";
		tabPage5.Size = new System.Drawing.Size(262, 210);
		tabPage5.TabIndex = 5;
		tabPage5.Text = "About";
		panel6.BackColor = System.Drawing.Color.Transparent;
		panel6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel6.Controls.Add(label10);
		System.Windows.Forms.Label discordLabel = new System.Windows.Forms.Label();
		discordLabel.AutoSize = true;
		discordLabel.Location = new System.Drawing.Point(0, 154);
		discordLabel.Name = "DiscordLabel";
		discordLabel.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
		discordLabel.TabIndex = 8;
		discordLabel.Text = "For suggestions, contact #therattus on Discord";
		panel6.Controls.Add(discordLabel);
		panel6.Controls.Add(label9);
		panel6.Controls.Add(label8);
		panel6.Controls.Add(this.VersionLabel);
		panel6.Controls.Add(label7);
		panel6.Controls.Add(this.DocumentationLink);
		panel6.Dock = System.Windows.Forms.DockStyle.Fill;
		panel6.Location = new System.Drawing.Point(0, 0);
		panel6.Name = "AboutPanel";
		panel6.Size = new System.Drawing.Size(262, 210);
		panel6.TabIndex = 2;
		this.VersionLabel.AutoSize = true;
		this.VersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.VersionLabel.Location = new System.Drawing.Point(133, 9);
		this.VersionLabel.Name = "VersionLabel";
		this.VersionLabel.Size = new System.Drawing.Size(49, 20);
		this.VersionLabel.TabIndex = 4;
		this.VersionLabel.Text = "1.0.0";
		this.VersionLabel.Visible = false;
		label7.AutoSize = true;
		label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		label7.Location = new System.Drawing.Point(4, 9);
		label7.Name = "NameLabel";
		label7.Size = new System.Drawing.Size(250, 15);
		label7.TabIndex = 3;
		label7.Text = "EVE-O Preview - Rattus Edition v1.1";
		this.DocumentationLink.Location = new System.Drawing.Point(0, 184);
		this.DocumentationLink.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
		this.DocumentationLink.Name = "DocumentationLink";
		this.DocumentationLink.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
		this.DocumentationLink.Size = new System.Drawing.Size(262, 24);
		this.DocumentationLink.TabIndex = 2;
		this.DocumentationLink.TabStop = true;
		this.DocumentationLink.Text = "to be set from prresenter to be set from prresenter to be set from prresenter to be set from prresenter";
		this.DocumentationLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DocumentationLinkClicked_Handler);
		this.NotifyIcon.ContextMenuStrip = this.TrayMenu;
		this.NotifyIcon.Icon = (System.Drawing.Icon)resources.GetObject("NotifyIcon.Icon");
		this.NotifyIcon.Text = "EVE-O Preview";
		this.NotifyIcon.Visible = true;
		this.NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RestoreMainForm_Handler);
		this.TrayMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
		this.TrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { toolStripMenuItem3, toolStripMenuItem, toolStripSeparator, toolStripMenuItem2 });
		this.TrayMenu.Name = "contextMenuStrip1";
		this.TrayMenu.Size = new System.Drawing.Size(152, 76);
		label8.BackColor = System.Drawing.Color.Transparent;
		label8.Location = new System.Drawing.Point(0, 26);
		label8.Name = "DescriptionLabel";
		label8.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
		label8.Size = new System.Drawing.Size(261, 112);
		label8.TabIndex = 5;
		label8.Text = resources.GetString("DescriptionLabel.Text");
		label9.AutoSize = true;
		label9.Location = new System.Drawing.Point(0, 170);
		label9.Name = "DocumentationLinkLabel";
		label9.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
		label9.Size = new System.Drawing.Size(222, 19);
		label9.TabIndex = 6;
		label9.Text = "Source code and updates:";
		label10.AutoSize = true;
		label10.Location = new System.Drawing.Point(0, 138);
		label10.Name = "CreditMaintLabel";
		label10.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
		label10.Size = new System.Drawing.Size(258, 19);
		label10.TabIndex = 7;
		label10.Text = "Credit to previous maintainer: Phrynohyas Tig-Rah";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Control;
		base.ClientSize = new System.Drawing.Size(390, 218);
		base.Controls.Add(tabControl);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(0);
		base.MaximizeBox = false;
		base.Name = "MainForm";
		this.Text = "EVE-O Preview";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormClosing_Handler);
		base.Load += new System.EventHandler(this.MainFormResize_Handler);
		base.Resize += new System.EventHandler(this.MainFormResize_Handler);
		tabControl.ResumeLayout(false);
		tabPage.ResumeLayout(false);
		panel.ResumeLayout(false);
		panel.PerformLayout();
		tabPage2.ResumeLayout(false);
		panel2.ResumeLayout(false);
		panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailsWidthNumericEdit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailsHeightNumericEdit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailOpacityTrackBar).EndInit();
		this.ZoomTabPage.ResumeLayout(false);
		panel3.ResumeLayout(false);
		panel3.PerformLayout();
		this.ZoomAnchorPanel.ResumeLayout(false);
		this.ZoomAnchorPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.ThumbnailZoomFactorNumericEdit).EndInit();
		tabPage3.ResumeLayout(false);
		panel4.ResumeLayout(false);
		panel4.PerformLayout();
		tabPage4.ResumeLayout(false);
		panel5.ResumeLayout(false);
		panel5.PerformLayout();
		tabPage5.ResumeLayout(false);
		panel6.ResumeLayout(false);
		panel6.PerformLayout();
		this.TrayMenu.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	void IView.Hide()
	{
		Hide();
	}

	void IView.Close()
	{
		Close();
	}
}
