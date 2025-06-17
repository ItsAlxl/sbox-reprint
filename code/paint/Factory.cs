namespace Reprint;

public abstract class FactoryStep
{
	abstract public string ApplyTo( Painting p );
}

public abstract class FactoryPanel : PanelComponent
{
	public Toolbox toolbox;
	public readonly FactoryStep factory;
	abstract protected FactoryStep CreateFactory();

	public FactoryPanel() : base()
	{
		factory = CreateFactory();
	}

	protected override void OnStart()
	{
		toolbox = Scene.Get<Toolbox>();
	}
}
