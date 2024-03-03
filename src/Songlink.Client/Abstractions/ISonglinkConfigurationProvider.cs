using Songlink.Client.Configuration;

namespace Songlink.Client.Abstractions;

public interface ISonglinkConfigurationProvider
{
	public SonglinkClientConfiguration GetConfiguration();
}
