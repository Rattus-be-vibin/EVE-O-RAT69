using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EveOPreview.View;

public class ThumbnailOverlay : Form
{
	private readonly Action<object, MouseEventArgs> _areaClickAction;

	private IContainer components = null;

	private Label OverlayLabel;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 128;
			return createParams;
		}
	}

	public ThumbnailOverlay(Form owner, Action<object, MouseEventArgs> areaClickAction)
	{
		Owner = owner;
		_areaClickAction = areaClickAction;
		InitializeComponent();
	}

	private void OverlayArea_Click(object sender, MouseEventArgs e)
	{
		_areaClickAction(this, e);
	}

	public void SetOverlayLabel(string label)
	{
		OverlayLabel.Text = label;
	}

	public void EnableOverlayLabel(bool enable)
	{
		OverlayLabel.Visible = enable;
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
		this.OverlayLabel = new System.Windows.Forms.Label();
		System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
		base.SuspendLayout();
		pictureBox.BackColor = System.Drawing.Color.Transparent;
		pictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
		pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
		pictureBox.Location = new System.Drawing.Point(0, 0);
		pictureBox.Name = "OverlayAreaPictureBox";
		pictureBox.Size = new System.Drawing.Size(284, 262);
		pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		pictureBox.TabIndex = 0;
		pictureBox.TabStop = false;
		pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OverlayArea_Click);
		this.OverlayLabel.AutoSize = true;
		this.OverlayLabel.Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.OverlayLabel.ForeColor = System.Drawing.Color.DarkGray;
		this.OverlayLabel.Location = new System.Drawing.Point(8, 8);
		this.OverlayLabel.Name = "OverlayLabel";
		this.OverlayLabel.Size = new System.Drawing.Size(25, 13);
		this.OverlayLabel.TabIndex = 1;
		this.OverlayLabel.Text = "...";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(284, 262);
		base.ControlBox = false;
		base.Controls.Add(this.OverlayLabel);
		base.Controls.Add(pictureBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ThumbnailOverlay";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		this.Text = "PreviewOverlay";
		base.TransparencyKey = System.Drawing.Color.Black;
		((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
