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
	public float RightBound { get => GameObject.Children.Count * CHILD_SPACING; }

	public void AddStep( GameObject prefab )
	{
		prefab.Clone( GameObject, Vector3.Zero, Rotation.Identity, Vector3.One );
		AdjustChildrenLayout();
	}

	private void AdjustChildrenLayout()
	{
		for ( var i = 0; i < GameObject.Children.Count; i++ )
		{
			var child = GameObject.Children[i];
			child.LocalPosition = child.LocalPosition.WithY( i * CHILD_SPACING );
		}
	}
}
