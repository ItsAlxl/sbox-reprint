using System;

namespace Reprint;

public abstract class FactoryStepStroke : FactoryStep
{
	public FactoryStroke strokeMain = new();
	public FactoryStroke StrokeFig { get => Workspace.figStroke; }
	public bool useFig = false;

	public FactoryStroke.Mode MainMode
	{
		set
		{
			useFig = false;
			strokeMain.StrokeMode = value;
			ConfigUpdated();
		}
		get => strokeMain.StrokeMode;
	}
	public FactoryStroke CurrentStroke { get => useFig ? StrokeFig : strokeMain; }

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

