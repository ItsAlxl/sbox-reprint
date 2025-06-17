namespace Reprint;

[Group( "Reprint" )]
[Title( "Toolbox" )]
[Icon( "home_repair_service" )]

public sealed class Toolbox : Component
{
	const float CHILD_SPACING = 30.0f;

	public class Tool
	{
		public string title;
		public GameObject prefab;
	}
	public readonly List<Tool> tools = [
		new(){ title = "Line", prefab = GameObject.GetPrefab("prefabs/StepLine.prefab")},
	];

	public static float LeftBound { get => 0.0f; }
	public float RightBound { get => (sequence.Count - 1) * CHILD_SPACING; }

	public Painting scratchPaint = new( 4, 4 );
	private GameObject scratchPanel;
	private readonly List<GameObject> sequence = [];

	protected override void OnStart()
	{
		scratchPanel = GameObject.Children.Find( ( go ) => go.Name == "Scratch" );
		sequence.Add( scratchPanel );
	}

	public void AddStep( GameObject prefab )
	{
		sequence.Add( prefab.Clone() );
		AdjustSequenceLayout();
	}

	private void AdjustSequenceLayout()
	{
		for ( var i = 0; i < sequence.Count; i++ )
		{
			var go = sequence[i];
			go.LocalPosition = go.LocalPosition.WithY( i * CHILD_SPACING );
		}
	}

	private FactoryStep GetFactoryStep( int idx )
	{
		return sequence[idx].Components.Get<FactoryPanel>().factory;
	}

	private void MoveInSequence( int fromIdx, int toIdx )
	{
		var item = sequence[fromIdx];
		sequence.RemoveAt( fromIdx );
		sequence.Insert( toIdx, item );
	}

	private int GetScratchIdx()
	{
		return sequence.FindIndex( ( go ) => go == scratchPanel );
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "AdvScratch" ) )
		{
			var scratchIdx = GetScratchIdx();
			if ( scratchIdx < sequence.Count - 1 )
			{
				GetFactoryStep( scratchIdx + 1 ).ApplyTo( scratchPaint );
				MoveInSequence( scratchIdx, scratchIdx + 1 );
				AdjustSequenceLayout();
			}
		}
		if ( Input.Pressed( "RstScratch" ) )
		{
			MoveInSequence( GetScratchIdx(), 0 );
			AdjustSequenceLayout();
		}
	}
}
