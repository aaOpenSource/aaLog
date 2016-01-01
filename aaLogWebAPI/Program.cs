
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
 
// Add reference to:
using Microsoft.Owin.Hosting;

namespace aaLogWebAPI
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {

                // Specify the URI to use for the local host:
                // TODO: Make port an option on startup
                // Example: http://localhost:8080/odata/UnreadRecords?unreadcount=10
                string baseUri = "http://localhost:8080";

                Console.WriteLine("Starting web Server... at " + baseUri);
                using (var srv = WebApp.Start<Startup>(baseUri))
                {
                    Console.WriteLine("Server running at {0} - press Enter to quit. ", baseUri);
                    Console.ReadLine();
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
                Debug.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}