using System.Net;
using System.Net.NetworkInformation;
using System;

namespace aaLogReader.Helpers
{
  /// <summary>
  /// Provides a method for looking up the fully qualified domain name 
  /// of the local machine so it can be shared between classes.
  /// </summary>
  public static class Fqdn
  {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets the fully qualified domain name of the local machine.
        /// </summary>
        /// <returns>string</returns>
        public static string GetFqdn()
    {
            string hostName;

            try
            {
                // Credits: http://stackoverflow.com/questions/804700/how-to-find-fqdn-of-local-machine-in-c-net
                var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                hostName = Dns.GetHostName();
                if (!hostName.EndsWith(domainName) && !string.IsNullOrWhiteSpace(domainName))
                {
                    hostName = string.Format("{0}.{1}", hostName, domainName);
                }
            }
            catch(Exception ex)
            {
                log.Warn(ex);
                hostName = "";
                
            }

      return hostName;

    }
  }
}