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

        //Setup Timer for reading logs
        private static Timer readTimer;
        public static Timer ReadTimer
        {
            get
            {
                if (readTimer == null)
                {
                    readTimer = new Timer();
                }

                return readTimer;
            }

            set
            {
                readTimer = value;
            }
        }

        private static CommandOption optionsFilePathOption;
        internal static CommandOption OptionsFilePathOption
        {
            get
            {
                return optionsFilePathOption;
            }

            set
            {
                optionsFilePathOption = value;
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

                OptionsFilePathOption = app.Option("-o| --optionsfile <PATH>", "Path to options file (Optional)", CommandOptionType.SingleValue);

                app.OnExecute(() =>
                {
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
                    ReadTimer.Interval = RuntimeOptions.ReadInterval;

                    // Create delegate to handle elapsed time event
                    ReadTimer.Elapsed += ReadTimer_Elapsed;

                    //Start Timer
                    ReadTimer.Start();

                    //Prevent console from exiting
                    Console.Read();
                    return 0;
                });

                app.Command("createdefaultoptionsfile", c =>
                {
                    c.Description = "Create a default options.json file";
                    c.HelpOption("-?| -h| --help");

                    var overWriteOption = c.Option("-o| --overwrite", "Overwrite existing options.json file", CommandOptionType.NoValue);
                    var fileNameOption = c.Option("-f| --filename <PATH>", "Name of options file (Optional)", CommandOptionType.SingleValue);

                    c.OnExecute(() =>
                    {
                        return CreateDefaultOptionsFile(fileNameOption.Value() ?? "options.json", overWriteOption.HasValue());
                    });
                });

                //Debug the startup arguments                
                log.DebugFormat("Startup Arguments {0}", JsonConvert.SerializeObject(args));

                //Always make sure we load runtime options first
                RuntimeOptions = ReadOptionsFile(OptionsFilePathOption);

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
        
        /// <summary>
        /// Write a default options file to disk
        /// </summary>
        /// <param name="fileName">Filename for the options file</param>
        /// <param name="overWrite">Overwrite an existing file if it exists</param>
        /// <returns></returns>
        private static int CreateDefaultOptionsFile(string fileName = "options.json", bool overWrite = false)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    log.InfoFormat("{0} exists", fileName);

                    if (!overWrite)
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
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return -1;
            }
        }

        /// <summary>
        /// Read an options file and return an Options object
        /// </summary>
        /// <param name="optionsFilePathOption">Path to options file</param>
        /// <returns></returns>
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

        /// <summary>
        /// Read data and transmit via HTTP to Splunk
        /// </summary>
        /// <param name="logReader"></param>
        private static void ReadAndTransmitData(aaLogReader.aaLogReader logReader)
        {
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
                            ClearTimerBackoff(ReadTimer, RuntimeOptions);
                        }

                       // Cache off the last record transmitted successfully
                       LastRecordTransmitted = logRecords.Last<LogRecord>();
                    }
                    else
                        {
                        
                        //Write the cached last record transmitted to the cache file in the event HTTP transmission to Splunk fails                        
                        logReader.WriteStatusCacheFile(LastRecordTransmitted);

                        // Implement a timer backoff so we don't flood the endpoint
                        IncrementTimerBackoff(ReadTimer, RuntimeOptions);
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
        private static void IncrementTimerBackoff(Timer readTimer, Options runtimeOptions)
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
        private static void ClearTimerBackoff(Timer readTimer, Options runtimeOptions)
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