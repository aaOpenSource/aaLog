using System.Net;
using System.Net.NetworkInformation;

namespace aaLogReader.Helpers
{
  /// <summary>
  /// Provides a method for looking up the fully qualified domain name 
  /// of the local machine so it can be shared between classes.
  /// </summary>
  public static class Fqdn
  {
    /// <summary>
    /// Gets the fully qualified domain name of the local machine.
    /// </summary>
    /// <returns>string</returns>
    public static string GetFqdn()
    {
      // Credits: http://stackoverflow.com/questions/804700/how-to-find-fqdn-of-local-machine-in-c-net
      var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
      var hostName = Dns.GetHostName();
      if (!hostName.EndsWith(domainName) && !string.IsNullOrWhiteSpace(domainName))
      {
        hostName = string.Format("{0}.{1}", hostName, domainName);
      }
      return hostName;
    }
  }
}