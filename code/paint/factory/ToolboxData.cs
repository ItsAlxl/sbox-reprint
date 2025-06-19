namespace Reprint;

[GameResource( "Tool", "ptool", "A painting tool.", Category = "Reprint", Icon = "brush" )]
public partial class ToolData : GameResource
{
	public string Title { get; set; }
	[ResourceType( "prefab" )]
	public string Prefab { get; set; }
}

[GameResource( "ToolboxData", "ptbox", "A set of painting tools.", Category = "Reprint", Icon = "home_repair_service" )]
public partial class ToolboxData : GameResource
{
	[ResourceType( "ptool" )]
	public List<string> Tools { get; set; }
}
