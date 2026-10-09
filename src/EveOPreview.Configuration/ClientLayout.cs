namespace EveOPreview.Configuration;

public class ClientLayout
{
	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public bool IsMaximized { get; set; }

	public ClientLayout()
	{
	}

	public ClientLayout(int x, int y, int width, int height, bool maximized)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		IsMaximized = maximized;
	}
}
