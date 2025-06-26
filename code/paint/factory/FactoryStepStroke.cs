using System;

namespace Reprint;

public abstract class FactoryStepStroke : FactoryStep
{
	public FactoryStroke strokeMain = new();
	public FactoryStroke StrokeFig { get => Workspace.figStroke; }
	private bool _useFig = false;
	public bool UseFig
	{
		get => _useFig;
		set
		{
			_useFig = value;
			ConfigUpdated();
		}
	}

	public FactoryStroke.Mode MainMode
	{
		set
		{
			UseFig = false;
			strokeMain.StrokeMode = value;
			ConfigUpdated();
		}
		get => strokeMain.StrokeMode;
	}
	public FactoryStroke CurrentStroke { get => UseFig ? StrokeFig : strokeMain; }

	public override void OnStart()
	{
		base.OnStart();
		strokeMain.OnConfigChanged += ConfigUpdated;
	}

	protected int InkPixel( Painting p, int x, int y )
	{
		var pxl = p.PixelAt( x, y );
		if ( pxl is null )
			return 0;

		CurrentStroke.InkPixel( pxl );
		return 1;
	}
}

