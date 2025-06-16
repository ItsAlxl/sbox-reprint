namespace Reprint;

public abstract class FactoryStep
{
	abstract public string ApplyTo( Painting p );
}

public abstract class FactoryPanel : PanelComponent
{
	public readonly FactoryStep factory;
	abstract protected FactoryStep CreateFactory();

	public FactoryPanel() : base()
	{
		factory = CreateFactory();
	}
}
