namespace Songlink.Client.Configuration;

public class SonglinkClientConfiguration(string songlinkApiKey)
{
	public static string SonglinkHttpClientName = "SonglinkHttpClient";
	
	public string SonglinkApiKey { get; } = songlinkApiKey;
}
