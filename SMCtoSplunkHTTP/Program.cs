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
        private static aaLogReader.aaLogReader aaLogReader;
        internal static aaLogReader.aaLogReader AALogReader
        {
            get
            {
                if (aaLogReader == null)
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

                    aaLogReader = new aaLogReader.aaLogReader(logReaderOptions);
                }

                return aaLogReader;
            }

            set
            {
                aaLogReader = value;
            }
        }

        internal static string CacheFilename
        {
            get
            {
                if (string.IsNullOrEmpty(RuntimeOptions.CacheFilename))
                {
                    return RuntimeOptions.SplunkSourceData + ".txt";
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
            // Setup logging
            log4net.Config.BasicConfigurator.Configure();

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

            try
            {
                //Get the last message number written
                var lastMessageNumberWritten = LastMessageNumberWritten;

                if (lastMessageNumberWritten == 0)
                {
                    // Get records starting with last log record and move backwards since we are working with no existing cache file     
                    //AALogReader.Options.IgnoreCacheFileOnFirstRead = true;
                    logRecords = AALogReader.GetRecordsByEndTimestampAndCount(System.DateTime.Now.AddMinutes(1), (int)runtimeOptions.MaxRecords);
                }
                else
                {
                    // Get records based on last cached message number
                    logRecords = aaLogReader.GetRecordsByStartMessageNumberAndCount(lastMessageNumberWritten + 1, (int)runtimeOptions.MaxRecords);
                }
                
                //var logRecords = aaLogReader.GetUnreadRecords(runtimeOptions.MaxRecords);

                log.DebugFormat("{0} records retrieved", logRecords.Count);

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
        /// Calculate SQL query from options and cache values
        /// </summary>
        /// <param name="runtimeOptions">Runtime Options object</param>
        /// <returns>String representing SQL Query based on provided runtime options</returns>
        //private static string GetLogRecords(OptionsStruct runtimeOptions)
        //{
        //    string query = "";
        //    string cachedSqlSequenceFieldValue;
        //    DateTime cachedSqlSequenceFieldValueDateTime;
        //    DateTimeStyles cacheDateTimeStyle;
            
        //    // Add the where clause if we can get the cached Sequence Field Value
        //    try
        //    {
        //        query = runtimeOptions.SQLQuery;

        //        if (string.IsNullOrEmpty(query))
        //        {
        //            throw new Exception("SQL Query in options file is empty or null");
        //        }

        //        //Get the base query and limit by TOP XX.  If there is no {{MaxRecords}} component then this statement makes no change to the query
        //        query = query.Replace("{{MaxRecords}}", runtimeOptions.MaxRecords.ToString());

        //        if (File.Exists(CacheFilename))
        //        {
        //            cachedSqlSequenceFieldValue = File.ReadAllText(CacheFilename) ?? string.Empty;
        //        }
        //        else
        //        {
        //            cachedSqlSequenceFieldValue = runtimeOptions.SQLSequenceFieldDefaultValue;
        //        }

        //        if (runtimeOptions.CacheWriteValueIsUTCTimestamp)
        //        {
        //            cacheDateTimeStyle = DateTimeStyles.AssumeUniversal;
        //        }
        //        else
        //        {
        //            cacheDateTimeStyle = DateTimeStyles.AssumeLocal;
        //        }

        //        if (DateTime.TryParseExact(cachedSqlSequenceFieldValue, runtimeOptions.CacheWriteValueStringFormat, CultureInfo.InvariantCulture, cacheDateTimeStyle, out cachedSqlSequenceFieldValueDateTime))
        //        {
        //            cachedSqlSequenceFieldValue = cachedSqlSequenceFieldValueDateTime.AddMilliseconds(runtimeOptions.CacheWriteValueTimestampMillisecondsAdd).ToString("yyyy-MM-dd HH:mm:ss.ffffff");
        //        }

        //        if (cachedSqlSequenceFieldValue != string.Empty)
        //        {
        //            query += runtimeOptions.SQLWhereClause.Replace("{{SQLSequenceField}}", runtimeOptions.SQLSequenceField).Replace("{{LastSQLSequenceFieldValue}}", cachedSqlSequenceFieldValue);
        //        }

        //        //Finally add the Order By Clause
        //        query += runtimeOptions.SQLOrderByClause.Replace("{{SQLSequenceField}}", runtimeOptions.SQLSequenceField);

        //        log.DebugFormat("SQL Query : {0}", query);
        //    }
        //    catch
        //    {
        //        // Do Nothing
        //    }
            
        //    return query;
        //}

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