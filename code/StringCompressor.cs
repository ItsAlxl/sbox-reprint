using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Reprint;

public sealed class StringCompressor
{
	public static string Compress( string s )
	{
		var source = Encoding.UTF8.GetBytes( s );
		using var outStream = new MemoryStream();
		using var gzip = new GZipStream( outStream, CompressionLevel.SmallestSize );
		gzip.Write( source, 0, source.Length );
		gzip.Flush();
		return Convert.ToBase64String( outStream.ToArray() );
	}

	public static string Decompress( string s )
	{
		var source = Convert.FromBase64String( s );
		using var inStream = new MemoryStream( source );
		using var gzip = new GZipStream( inStream, CompressionMode.Decompress );
		using var reader = new StreamReader( gzip );
		return reader.ReadToEnd();
	}
}
