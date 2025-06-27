using System;
using System.Security.Cryptography;
using System.Text;
using Sandbox.Services;

namespace Reprint;

public sealed class Score
{
	const string KEY_TIME_COST = "t";
	const string KEY_INK_COST = "i";
	const string KEY_SIZE_COST = "s";

	public class Data( string key, string title = "" )
	{
		public string id = key;
		public string name = title;
		public Leaderboards.Board2 friends;
		public bool fetched = false;

		public Stats.GlobalStat World { get => Stats.Global.Get( id ); }
		public Stats.PlayerStat Me { get => Stats.LocalPlayer.Get( id ); }

		public int MyBest { get => (int)Me.Min; }
		public int WorldBest { get => (int)World.Min; }
		public string WorldAvg { get => World.Avg.ToString( "0.00" ); }

		public async void Fetch( Action cb )
		{
			friends = Leaderboards.GetFromStat( id );
			friends.SetAggregationMin();
			friends.SetSortAscending();
			friends.SetFriendsOnly( true );
			friends.MaxEntries = 5;

			await friends.Refresh();
			fetched = true;
			cb();
		}
	}

	public class Level
	{
		public string id;
		public Data ink;
		public Data time;
		public Data size;

		public bool fetched = false;
		private bool fetching = false;

		public bool IsOld { get => age > 30.0f; }
		private TimeSince age;

		public Data[] Data { get => [time, ink, size]; }

		public Level( string key )
		{
			id = key;
			ink = new( id + KEY_INK_COST, "Ink" );
			time = new( id + KEY_TIME_COST, "Time" );
			size = new( id + KEY_SIZE_COST, "Size" );
		}

		public void Fetch()
		{
			if ( !fetching )
			{
				fetching = true;
				ink.Fetch( FinishFetch );
				time.Fetch( FinishFetch );
				size.Fetch( FinishFetch );
			}
		}

		private void FinishFetch()
		{
			if ( ink.fetched && time.fetched && size.fetched )
			{
				fetching = false;
				age = 0;
			}
		}
	}

	private static readonly Dictionary<string, Level> levelData = [];

	public static string GetLeaderboardKey( ScenarioData sceneData )
	{
		return GetLeaderboardKey( sceneData.Paint, sceneData.Toolbox );
	}

	private static string ReduceToolboxName( string toolbox )
	{
		if ( toolbox == null )
			return "all";
		var start = toolbox.IndexOf( " - " );
		start = start < 0 ? 0 : (start + 2);

		var len = toolbox.Length - start;
		if ( toolbox.EndsWith( ".ptbox" ) )
			len -= 6;
		return toolbox.Substring( start, len );
	}

	public static string ReducePaintSerial( string paint )
	{
		return paint.Split( ':' )[2];
	}

	public static string Hash( string ascii )
	{
		return Convert.ToBase64String( MD5.HashData( Encoding.ASCII.GetBytes( ascii ) ) )
			.Replace( "=", "" )
			.Replace( "+", "-" )
			.Replace( "/", "_" );
	}

	public static string GetLeaderboardKey( string paintSerial, string toolbox = null )
	{
		return Hash( ReduceToolboxName( toolbox ) + ReducePaintSerial( paintSerial ) ).ToLower();
	}

	public static void Send( string key, (int time, int ink, int size) scores )
	{
		Stats.SetValue( key + KEY_TIME_COST, scores.time );
		Stats.SetValue( key + KEY_INK_COST, scores.ink );
		Stats.SetValue( key + KEY_SIZE_COST, scores.size );
	}

	public static Level Fetch( string key )
	{
		if ( levelData.TryGetValue( key, out Level lvl ) )
		{
			if ( lvl.IsOld )
				lvl.Fetch();
			return lvl;
		}
		else
		{
			lvl = new Level( key );
			levelData.Add( key, lvl );
			lvl.Fetch();
		}
		return lvl;
	}

	public static Data[] DataFor( ScenarioData sceneData )
	{
		return Fetch( GetLeaderboardKey( sceneData ) ).Data;
	}

	public static Data[] DataFor( Scenario scene )
	{
		return Fetch( scene.LeaderboardKey ).Data;
	}

	public static int BuildHash() => HashCode.Combine( levelData.GetHashCode() );
}
