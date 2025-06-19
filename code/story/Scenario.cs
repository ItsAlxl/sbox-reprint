using System.Net;

namespace Reprint;

public sealed class Scenario
{
	const char SERIAL_DELIMITER = '&';

	public static IEnumerable<ScenarioData> Order { get => _order ??= ResourceLibrary.Get<StoryData>( "story/main.pzstory" ).Scenarios.Select( ResourceLibrary.Get<ScenarioData> ); }
	private static IEnumerable<ScenarioData> _order = null;

	public string title;
	public string desc;
	public string paint;
	public string toolbox = "all";

	private string ToolboxPath { get => toolbox is not null && toolbox.EndsWith( ".ptbox" ) ? toolbox : ("tools/boxes/" + (toolbox == "" || toolbox is null ? "all" : toolbox) + ".ptbox"); }
	public IEnumerable<ToolData> Tools { get => _tools ??= ResourceLibrary.Get<ToolboxData>( ToolboxPath ).Tools.Select( ResourceLibrary.Get<ToolData> ); }
	private IEnumerable<ToolData> _tools = null;

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
		var comps = (imp.Contains( SERIAL_DELIMITER ) ? imp : StringCompressor.Decompress( imp )).Split( SERIAL_DELIMITER );
		title = WebUtility.UrlDecode( comps[0] );
		desc = WebUtility.UrlDecode( comps[1] );
		paint = WebUtility.UrlDecode( comps[2] );
	}
}
