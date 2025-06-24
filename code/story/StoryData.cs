namespace Reprint;

[GameResource( "StoryData", "pzstory", "A series of puzzles.", Category = "Reprint", Icon = "auto_stories" )]
public partial class StoryData : GameResource
{
	[ResourceType( "ppzzl" )]
	public List<string> Scenarios { get; set; }
}

[GameResource( "ScenarioData", "ppzzl", "A painting puzzle.", Category = "Reprint", Icon = "extension" )]
public partial class ScenarioData : GameResource
{
	public string Title { get; set; }
	[TextArea]
	public string Desc { get; set; }
	public string Paint { get; set; }
	[ResourceType( "ptbox" )]
	public string Toolbox { get; set; }
	public bool UseSpongeBurn { get; set; } = true;
}
