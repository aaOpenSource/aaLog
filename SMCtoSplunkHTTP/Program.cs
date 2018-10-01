// Copyright (c) Andrew Robinson. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using SMCtoSplunkHTTP.Helpers;
using System.IO;
using System.Net;
using System.Globalization;
using SplunkHTTPUtility;
using Microsoft.Extensions.CommandLineUtils;
using System.Reflection;
using aaLogReader;
using System.Collections.Generic;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]
namespace SMCtoSplunkHTTP
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
        public static Timer ReadTimer
        {
            get
            {
                if(readTimer == null)
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

        /// <summary>
        /// Read the cache file and confirm the last message number successfully written to Splunk
        /// </summary>
        /// <returns></returns>
        private static ulong LastMessageNumberWritten
        {
            get
            {
                ulong returnValue = 0;
                string cacheFileText;

                try
                {
                    //Get the cache file and read the last message number
                    cacheFileText = File.ReadAllText(CacheFilename);
                    cacheFileText = cacheFileText ?? "0";
                    ulong.TryParse(cacheFileText, out returnValue);
                }
                catch
                {
                    returnValue = 0;
                }

                return (ulong)returnValue;
            }
        }

        // Global aaLogReader
        private static aaLogReader.aaLogReader _aaLogReader;
        internal static aaLogReader.aaLogReader AALogReader
        {
            get
            {
                if (_aaLogReader == null)
                {
                    log.DebugFormat("Creating aaLogReader");

                    var logReaderOptions = new aaLogReader.OptionsStruct
                    {
                        //CacheFileAppendProcessNameToBaseFileName = runtimeOptions.CacheFileAppendProcessNameToBaseFileName,
                        //CacheFileBaseName = runtimeOptions.CacheFileBaseName,
                        //CacheFileNameCustom = runtimeOptions.CacheFileNameCustom,
                        //IgnoreCacheFileOnFirstRead = runtimeOptions.IgnoreCacheFileOnFirstRead,
                        LogRecordPostFilters = runtimeOptions.LogRecordPostFilters,
                        LogDirectory = runtimeOptions.LogDirectory
                    };

                    _aaLogReader = new aaLogReader.aaLogReader(logReaderOptions);
                }

                return _aaLogReader;
            }

            set
            {
                _aaLogReader = value;
            }
        }

        internal static string CacheFilename
        {
            get
            {
                if (string.IsNullOrEmpty(RuntimeOptions.CacheFilename))
                {
                    return RuntimeOptions.SplunkSourceData + "-cache.txt";
                }
                else
                {
                    return RuntimeOptions.CacheFilename;
                }
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
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                log.InfoFormat("Starting {0} Version {1}", assembly.Location, assembly.GetName().Version.ToString());
                
                #region Argument Options
                
                var app = new CommandLineApplication(throwOnUnexpectedArg: false)
                {
                    Name = "SQLToSplunkHTTP",
                    Description = "Command line application meant to forward records from a SQL Server Database to a Splunk HTTP collector",
                    FullName = "SQL Server to Splunk HTTP Collector"      
                };
                
                // Define app Options; 
                app.HelpOption("-?| -h| --help");
                app.VersionOption("-v| --version", assembly.GetName().Version.MajorRevision.ToString(), assembly.GetName().Version.ToString());

                optionsFilePathOption = app.Option("-o| --optionsfile <PATH>", "Path to options file (Optional)", CommandOptionType.SingleValue);
                
        app.OnExecute(() =>
                {
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
               
                app.Command("clearcache", c =>
                 {

                     c.Description = "Deletes the current cache file";
                     c.HelpOption("-?| -h| --help");

                     c.OnExecute(() =>
                     {
                         return ClearCache(CacheFilename);                         
                     });
                 });

                app.Command("createdefaultoptionsfile", c =>
                {
                    c.Description = "Create a default options.json file";
                    c.HelpOption("-?| -h| --help");

                    var overWriteOption = c.Option("-o| --overwrite", "Overwrite file if it exists",CommandOptionType.NoValue);
                    var fileNameOption = c.Option("-f| --filename <PATH>", "Name of options file (Optional)", CommandOptionType.SingleValue);
                    
                    c.OnExecute(() =>
                    {
                        return CreateDefaultOptionsFile(fileNameOption.Value() ?? "options.json", overWriteOption.HasValue());
                    });
                });

                //Debug the startup arguments                
                log.DebugFormat("Startup Arguments {0}",JsonConvert.SerializeObject(args));

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
        /// Clear cache file
        /// </summary>
        /// <param name="CacheFileName"></param>
        /// <returns></returns>
        private static int ClearCache(string CacheFileName)
        {
            try
            {
                log.InfoFormat("Deleting cache file {0}", CacheFileName);
                System.IO.File.Delete(CacheFileName);

                return 0;
            }
            catch(Exception ex)
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
            catch(Exception ex)
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
            ReadAndTransmitData(AALogReader);
        }
        
        /// <summary>
        /// Read data and transmit via HTTP to Splunk
        /// </summary>
        /// <param name="query"></param>
        /// <param name="sqlConnectionObject"></param>
        private static void ReadAndTransmitData(aaLogReader.aaLogReader aaLogReader)
        {
            string kvpValue = "";
            List<LogRecord> logRecords = new List<LogRecord>();
            List<LogRecord> lgxRecords = new List<LogRecord>();

            try
            {
                //Get the last message number written
                var lastMessageNumberWritten = LastMessageNumberWritten;

                if (lastMessageNumberWritten == 0)
                {
                    log.DebugFormat("Executing GetUnreadRecords for {0} records", runtimeOptions.MaxRecords);
                    // Get records starting with last log record and move backwards since we are working with no existing cache file     
                    //AALogReader.Options.IgnoreCacheFileOnFirstRead = true;
                    //logRecords = AALogReader.GetRecordsByEndTimestampAndCount(System.DateTime.Now.AddMinutes(1), (int)runtimeOptions.MaxRecords);
                    logRecords = AALogReader.GetUnreadRecords((ulong)runtimeOptions.MaxRecords);
                }
                else
                {
                    log.DebugFormat("Executing GetRecordsByStartMessageNumberAndCount for message number {0} with a maximum of {1} records", lastMessageNumberWritten + 1, runtimeOptions.MaxRecords);
                    // Get records based on last cached message number
                    logRecords = AALogReader.GetRecordsByStartMessageNumberAndCount(lastMessageNumberWritten+1, (int)runtimeOptions.MaxRecords);
                }
                
                log.InfoFormat("{0} records retrieved", logRecords.Count);

                if (logRecords.Count > 0)
                {
                    //Build the additional KVP values to Append
                    var additionalKVPValues = new StringBuilder();

                    additionalKVPValues.AppendFormat("{0}=\"{1}\", ", "SourceHost", RuntimeOptions.SplunkSourceHost);
                    additionalKVPValues.AppendFormat("{0}=\"{1}\", ", "SourceData", RuntimeOptions.SplunkSourceData);
    
                    //Get the KVP string for the records
                    kvpValue = logRecords.ToKVP(additionalKVPValues.ToString());

                    //Transmit the records
                    var result = SplunkHTTPClient.TransmitValues(kvpValue);

                    log.DebugFormat("Transmit Values Result - {0}", result);

                    //If successful then write the last sequence value to disk
                    if (result.StatusCode == HttpStatusCode.OK)
                    {

                        log.DebugFormat("Writing Cache File");

                        // Write the last sequence value to the cache value named for the SQLSequence Field.  Order the result set by the sequence field then select the first record
                        WriteCacheFile(logRecords, CacheFilename, RuntimeOptions);

                        if(ReadTimer.Interval != RuntimeOptions.ReadInterval)
                        {
                            //Reset timer interval
                            ClearTimerBackoff(ReadTimer, RuntimeOptions);
                        }                            
                    }
                    else
                    {
                        // Implement a timer backoff so we don't flood the endpoint
                        IncrementTimerBackoff(ReadTimer, RuntimeOptions);
                        log.WarnFormat("HTTP Transmission not OK - {0}",result);
                    }
                }

                var aaLGXDirectory = runtimeOptions.AALGXDirectory ?? "";

                // Parse AALGX Files
                if (aaLGXDirectory != "")
                {
                    
                    log.InfoFormat("Reading aaLGX files from {0}", aaLGXDirectory);

                    if (Directory.Exists(aaLGXDirectory))
                    {
                        var successFolder = aaLGXDirectory + "\\success";

                        if (!Directory.Exists(successFolder))
                        {
                            Directory.CreateDirectory(successFolder);
                        }
                        
                        var errorFolder = aaLGXDirectory + "\\error";

                        if (!Directory.Exists(errorFolder))
                        {
                            Directory.CreateDirectory(errorFolder);
                        }

                        string[] filesList = Directory.GetFiles(aaLGXDirectory, "*.aalgx");
                        foreach (string fileName in filesList)
                        {
                            log.InfoFormat("Processing file {0}", fileName);
                            var aaLGXRecords = aaLgxReader.ReadLogRecords(fileName);
                            
                            log.InfoFormat("Found {0} records in {1}", aaLGXRecords.Count(), fileName);

                            if (aaLGXRecords.Count() > 0)
                                {
                                    //Build the additional KVP values to Append
                                    var additionalKVPValues = new StringBuilder();

                                    additionalKVPValues.AppendFormat("{0}=\"{1}\", ", "SourceHost", RuntimeOptions.SplunkSourceHost);
                                    additionalKVPValues.AppendFormat("{0}=\"{1}\", ", "SourceData", RuntimeOptions.SplunkSourceData);
    
                                    //Get the KVP string for the records
                                    kvpValue = aaLGXRecords.ToKVP(additionalKVPValues.ToString());

                                    //Transmit the records
                                    var result = SplunkHTTPClient.TransmitValues(kvpValue);

                                    log.DebugFormat("Transmit Values Result - {0}", result);

                                    //If successful then write the last sequence value to disk
                                    if (result.StatusCode == HttpStatusCode.OK)
                                    {
                                        log.InfoFormat("Successfully transmitted {0} records from file {1}", aaLGXRecords.Count, fileName);
                                        log.InfoFormat("Moving {0} to {1}", fileName, successFolder);
                                        var destinationFilename = Path.Combine(successFolder, Path.GetFileName(fileName));

                                        try
                                        {
                                            File.Move(fileName, destinationFilename);
                                        }
                                        catch
                                        {
                                            log.WarnFormat("Error moving {0} to {1}", fileName, destinationFilename);
                                        }

                                        if (ReadTimer.Interval != RuntimeOptions.ReadInterval)
                                        {
                                            //Reset timer interval
                                            ClearTimerBackoff(ReadTimer, RuntimeOptions);
                                        }
                                    }
                                    else
                                    {
                                        // Implement a timer backoff so we don't flood the endpoint
                                        IncrementTimerBackoff(ReadTimer, RuntimeOptions);
                                        log.WarnFormat("HTTP Transmission not OK - {0}", result);

                                        log.InfoFormat("Moving {0} to {1}", fileName, errorFolder);
                                        var destinationFilename = Path.Combine(errorFolder, Path.GetFileName(fileName));

                                        try
                                        {
                                            File.Move(fileName, destinationFilename);
                                        }
                                        catch
                                        {
                                            log.WarnFormat("Error moving {0} to {1}", fileName, destinationFilename);
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        log.WarnFormat("aaLGX Directory {0} does not exist.", aaLGXDirectory);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Write an entry for the maximum sequence field value into the cache file
        /// </summary>
        /// <param name="dataTable">Transmitted records</param>
        /// <param name="cacheFileName">Filename to write cache data to</param>
        /// <param name="runtimeOptions">Runtime Options Object</param>
        private static void WriteCacheFile(List<LogRecord> logRecords, string cacheFileName, Options runtimeOptions)
        {
            string cacheWriteValue;
            ulong? messageNumberToWrite;

            try
            {
                messageNumberToWrite = logRecords.AsEnumerable().OrderByDescending(r => r.MessageNumber).First().MessageNumber;
                messageNumberToWrite = messageNumberToWrite ?? (ulong?)0;

                cacheWriteValue = messageNumberToWrite.ToString();

                log.DebugFormat("cacheWriteValue : {0}", cacheWriteValue);
                File.WriteAllText(cacheFileName, cacheWriteValue);
            }
            catch (Exception ex)
            {
                log.Error(ex);
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