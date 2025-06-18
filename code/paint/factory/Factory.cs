namespace Reprint;

public abstract class FactoryStep
{
	abstract public string ApplyTo( Painting p );
}

public abstract class FactoryPanel : PanelComponent
{
	public Workspace workspace;
	public readonly FactoryStep factory;
	abstract protected FactoryStep CreateFactory();

	public FactoryPanel() : base()
	{
		factory = CreateFactory();
	}

	protected override void OnStart()
	{
		workspace = Scene.Get<Workspace>();
	}
}
