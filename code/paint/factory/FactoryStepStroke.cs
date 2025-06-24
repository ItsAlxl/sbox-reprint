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
			strokeMain.mode = value;
		}
		get => strokeMain.mode;
	}
	public FactoryStroke CurrentStroke { get => useFig ? StrokeFig : strokeMain; }

	protected int InkPixel( Painting p, int x, int y )
	{
		var pxl = p.PixelAt( x, y );
		if ( pxl is null )
			return 0;

		CurrentStroke.InkPixel( pxl );
		return 1;
	}
}

