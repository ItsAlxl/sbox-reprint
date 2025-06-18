namespace Reprint;

public sealed class Toolbox
{
	public class Tool
	{
		public string title;
		public GameObject prefab;
	}

	public static readonly List<Tool> tools = [
		new(){ title = "Line", prefab = GameObject.GetPrefab("prefabs/StepLine.prefab")},
		new(){ title = "Rotate", prefab = GameObject.GetPrefab("prefabs/StepRot.prefab")},
		new(){ title = "Flip", prefab = GameObject.GetPrefab("prefabs/StepFlip.prefab")},
		new(){ title = "Cursor", prefab = GameObject.GetPrefab("prefabs/StepCurs.prefab")},
	];
}
