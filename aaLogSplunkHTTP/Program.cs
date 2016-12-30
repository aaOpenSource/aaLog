// Copyright (c) Andrew Robinson. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using aaLogReader;
using Newtonsoft.Json;
using System.Net;
using aaLogReader.Helpers;
using System.Timers;
using SplunkHTTPUtility;
using Microsoft.Extensions.CommandLineUtils;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]
namespace aaLogSplunkHTTP
{
    class Program
    {
        #region Globals

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // HTTP Client for data transmission to Splunk
        private static SplunkHTTP splunkHTTPClient;
        internal static SplunkHTTP SplunkHTTPClient
        {
            get
            {
                return splunkHTTPClient;
            }

            set
            {
                splunkHTTPClient = value;
            }
        }

        //Setup Timer for reading logs
        private static Timer readTimer;

        //Runtime Options Object
        private static Options runtimeOptions;
        internal static Options RuntimeOptions
        {
            get
            {
                if (runtimeOptions == null)
                {
                    runtimeOptions = JsonConvert.DeserializeObject<Options>(System.IO.File.ReadAllText("options.json"));
                }

                return runtimeOptions;
            }

            set
            {
                runtimeOptions = value;
            }
        }

        // Log reader object
        private static aaLogReader.aaLogReader logReader;
        public static aaLogReader.aaLogReader LogReader
        {
            get
            {
                return logReader;
            }

            set
            {
                logReader = value;
            }
        }

        // In memory cache of the last log record transmitted
        private static LogRecord lastRecordTransmitted;
        public static LogRecord LastRecordTransmitted
        {
            get
            {
                if(lastRecordTransmitted == null)
                {
                    lastRecordTransmitted = new LogRecord();
                }

                return lastRecordTransmitted;
            }

            set
            {
                lastRecordTransmitted = value;
            }
        }

        #endregion
        
        static int Main(string[] args)
        {
            // Setup logging
            log4net.Config.BasicConfigurator.Configure();

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                log.InfoFormat("Starting {0} Version {1}", assembly.Location, assembly.GetName().Version.ToString());

                #region Argument Options

                var app = new CommandLineApplication(throwOnUnexpectedArg: false)
                {
                    Name = "SMCLogsToSplunkHTTP",
                    Description = "Command line application meant to forward records from a Wonderware SMC Log to a Splunk HTTP collector",
                    FullName = "Wonderware SMC Logs to Splunk HTTP Collector"
                };

                // Define app Options; 
                app.HelpOption("-?| -h| --help");
                app.VersionOption("-v| --version", assembly.GetName().Version.MajorRevision.ToString(), assembly.GetName().Version.ToString());

                var optionsFilePathOption = app.Option("-o| --optionsfile <PATH>", "Path to options file (Optional)", CommandOptionType.SingleValue);

                app.OnExecute(() =>
                {
                    //Load runtime options
                    RuntimeOptions = ReadOptionsFile(optionsFilePathOption);

                    RuntimeOptions.MaxUnreadRecords = 3;

                    //Initialize the Log Reader
                    LogReader = new aaLogReader.aaLogReader(RuntimeOptions);

                    // Setup the SplunkHTTPClient
                    SplunkHTTPClient = new SplunkHTTP(log, RuntimeOptions.SplunkAuthorizationToken, RuntimeOptions.SplunkBaseAddress, RuntimeOptions.SplunkClientID);

                    //Eat any SSL errors if configured to do so via options
                    // TODO : Test this feature
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return RuntimeOptions.SplunkIgnoreSSLErrors;
                    };

                    // Configure Timer
                    readTimer = new Timer(RuntimeOptions.ReadInterval);

                    // Create delegate to handle elapsed time event
                    readTimer.Elapsed += ReadTimer_Elapsed;

                    //Start Timer
                    readTimer.Start();

                    //Prevent console from exiting
                    Console.Read();
                    return 0;
                });

                //app.Command("clearcache", c =>
                //{
                //    c.Description = "Deletes the current cache file";
                //    c.HelpOption("-?| -h| --help");

                //    c.OnExecute(() =>
                //    {
                //        //Load runtime options
                //        RuntimeOptions = ReadOptionsFile(optionsFilePathOption);

                //        log.InfoFormat("Deleting cache file {0}", LogReader);
                //        System.IO.File.Delete(CacheFileName);

                //        return 0;
                //    });
                //});

                app.Command("createdefaultoptionsfile", c =>
                {

                    c.Description = "Create a default options.json file";
                    c.HelpOption("-?| -h| --help");

                    var overWriteOption = c.Option("-o| --overwrite", "Overwrite existing options.json file", CommandOptionType.NoValue);
                    var fileNameOption = c.Option("-f| --filename <PATH>", "Name of options file (Optional)", CommandOptionType.SingleValue);

                    c.OnExecute(() =>
                    {
                        var fileName = fileNameOption.Value() ?? "options.json";

                        if (System.IO.File.Exists(fileName))
                        {
                            log.InfoFormat("{0} exists", fileName);

                            if (!overWriteOption.HasValue())
                            {
                                log.InfoFormat("Applications options not set to overwrite {0}.  Specify options to overwrite or use different filename.", fileName);
                                return 0;
                            }
                            else
                            {
                                log.InfoFormat("Overwriting {0}", fileName);
                            }
                        }

                        System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(new Options(), Formatting.Indented));

                        log.InfoFormat("Wrote default options to {0}", fileName);

                        return 0;
                    });
                });

                //Debug the startup arguments
                log.DebugFormat("Startup Arguments");
                log.Debug(JsonConvert.SerializeObject(args));

                // Run the application with arguments
                return app.Execute(args);

                #endregion
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return -1;
            }
        }

        private static Options ReadOptionsFile(CommandOption optionsFilePathOption)
        {
            try
            {
                var optionsPath = optionsFilePathOption.Value() ?? "options.json";

                log.DebugFormat("Using options file {0}", optionsPath);

                if (System.IO.File.Exists(optionsPath))
                {
                    return JsonConvert.DeserializeObject<Options>(System.IO.File.ReadAllText(optionsPath));
                }
                else
                {
                    log.WarnFormat("Specified options file {0} does not exist. Loading default values.", optionsPath);
                    return new Options();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        private static void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReadAndTransmitData(LogReader);
        }

        private static void ReadAndTransmitData(aaLogReader.aaLogReader logReader)
        {
            // string kvpValue = "";

            try
            {

                var logRecords = logReader.GetUnreadRecords(RuntimeOptions.MaxUnreadRecords);

                if(logRecords.Count > 0)
                    {
                    //Transmit the records
                    var result = SplunkHTTPClient.TransmitValues(logRecords.ToKVP()).Result;

                    //If successful then write the last sequence value to disk
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                       if (readTimer.Interval != RuntimeOptions.ReadInterval)
                        {
                            //Reset timer interval
                            ClearTimerBackoff(ref readTimer, RuntimeOptions);
                        }

                       // Cache off the last record transmitted successfully
                       LastRecordTransmitted = logRecords.Last<LogRecord>();
                    }
                    else
                        {
                        
                        //Write the cached last record transmitted to the cache file in the event HTTP transmission to Splunk fails                        
                        logReader.WriteStatusCacheFile(LastRecordTransmitted);

                        // Implement a timer backoff so we don't flood the endpoint
                        IncrementTimerBackoff(ref readTimer, RuntimeOptions);
                        log.WarnFormat("HTTP Transmission not OK {0}", result);
                        }
                    }               
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                // Do Nothing
            }
        }
        
        /// <summary>
        /// Slow down the timer by doubling the interval up to MaximumReadInterval
        /// </summary>
        private static void IncrementTimerBackoff(ref Timer readTimer, Options runtimeOptions)
        {
            try
            {
                lock (readTimer)
                {
                    var currentInterval = readTimer.Interval;

                    if (currentInterval < runtimeOptions.MaximumReadInterval)
                    {
                        readTimer.Interval = System.Math.Min(currentInterval * 2, runtimeOptions.MaximumReadInterval);
                        log.WarnFormat("Read Timer interval set to {0} milliseconds", readTimer.Interval);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                // Set to a default read interval of 60000
                readTimer.Interval = 60000;
            }
        }

        /// <summary>
        /// Slow down the timer by doubling the interval up to MaximumReadInterval
        /// </summary>
        private static void ClearTimerBackoff(ref Timer readTimer, Options runtimeOptions)
        {
            try
            {
                log.InfoFormat("Restoring transmission timer interval to {0}", runtimeOptions.ReadInterval);
                lock (readTimer)
                {
                    readTimer.Interval = RuntimeOptions.ReadInterval;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                // Set to a default read interval of 60000
                readTimer.Interval = 60000;
            }
        }        
    }
}