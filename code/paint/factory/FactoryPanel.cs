namespace Reprint;

public abstract class FactoryPanel : PanelComponent
{
	public Workspace workspace;
	public readonly FactoryStep factory;
	abstract protected FactoryStep CreateFactory();

	public FactoryPanel() : base()
	{
		factory = CreateFactory();
		factory.panel = this;
	}

	protected override void OnStart()
	{
		workspace = Scene.Get<Workspace>();
	}
}
