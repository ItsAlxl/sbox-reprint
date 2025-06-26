using System.Net;

namespace Reprint;

public sealed class Scenario
{
	const string GZIP_BASE64_PREFIX = "H4sI";
	const char SERIAL_DELIMITER = '&';

	public static IEnumerable<ScenarioData> Order { get => _order ??= ResourceLibrary.Get<StoryData>( "story/main.pzstory" ).Scenarios.Select( ResourceLibrary.Get<ScenarioData> ); }
	private static IEnumerable<ScenarioData> _order = null;

	public static ScenarioData GetNext( ScenarioData sdata )
	{
		var isNext = false;
		foreach ( var s in Order )
		{
			if ( isNext )
				return s;
			isNext = s == sdata;
		}
		return null;
	}

	public string title;
	public string desc;
	public string paint;
	public string toolbox = "all";
	public bool useBurnSponge = true;
	public bool useConfigurator = true;
	public bool useBreakpoints = true;

	public string LeaderboardKey { get => Score.GetLeaderboardKey( paint, toolbox ); }

	private string ToolboxPath { get => toolbox is not null && toolbox.EndsWith( ".ptbox" ) ? toolbox : ("tools/boxes/" + (toolbox == "" || toolbox is null ? "all" : toolbox) + ".ptbox"); }
	public IEnumerable<ToolData> Tools { get => _tools ??= ToolPaths.Select( ResourceLibrary.Get<ToolData> ); }
	private IEnumerable<string> ToolPaths
	{
		get => useConfigurator ?
			ResourceLibrary.Get<ToolboxData>( ToolboxPath ).Tools.Append( "tools/fig.ptool" ) :
			ResourceLibrary.Get<ToolboxData>( ToolboxPath ).Tools;
	}
	private IEnumerable<ToolData> _tools = null;

	public bool IsEmpty { get => paint is null || paint == ""; }

	public Scenario( string t = "MISSING_TITLE", string d = "MISSING_DESCRIPTION", string p = "" )
	{
		title = t;
		desc = d;
		paint = p;
	}

	public Scenario( ScenarioData data )
	{
		title = data.Title;
		desc = data.Desc;
		paint = data.Paint;
		useBurnSponge = data.UseSpongeBurn;
		useConfigurator = data.UseConfigurator;
		useBreakpoints = data.UseBreakpoints;
		if ( toolbox != data.Toolbox )
		{
			_tools = null;
			toolbox = data.Toolbox;
		}
	}

	public Scenario( string import )
	{
		Import( import );
	}

	public void CopyTo( ScenarioData data )
	{
		data.Title = title;
		data.Desc = desc;
		data.Paint = paint;
		data.UseSpongeBurn = useBurnSponge;
		data.UseConfigurator = useConfigurator;
		data.UseBreakpoints = useBreakpoints;
		data.Toolbox = toolbox;
	}

	public void BakePainting( Painting p )
	{
		paint = p.Serialize();
	}

	public string ExportRaw()
	{
		return WebUtility.UrlEncode( title ) + SERIAL_DELIMITER +
			WebUtility.UrlEncode( desc ) + SERIAL_DELIMITER +
			WebUtility.UrlEncode( paint );
	}

	public string Export()
	{
		return StringCompressor.Compress( ExportRaw() );
	}

	public void Import( string imp )
	{
		var isRaw = imp.Contains( SERIAL_DELIMITER );
		if ( isRaw || imp.StartsWith( GZIP_BASE64_PREFIX ) )
		{
			var comps = (isRaw ? imp : StringCompressor.Decompress( imp )).Split( SERIAL_DELIMITER );
			title = WebUtility.UrlDecode( comps[0] );
			desc = WebUtility.UrlDecode( comps[1] );
			paint = WebUtility.UrlDecode( comps[2] );
		}
		else
		{
			title = "NO NAME";
			desc = "NO DESC";
			paint = WebUtility.UrlDecode( imp );
		}
	}
}
