using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aaLogReader;
using Newtonsoft.Json;
using System.Net;
using aaLogReader.Helpers;
using System.Net.Http;
using System.Timers;
using System.Collections.Concurrent;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]
namespace aaLogSplunkHTTP
{
    class Program
    {
        #region Globals

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static HttpClient _client = new HttpClient();
                
        //private static List<LogRecord> logRecords;
        private static aaLogReader.aaLogReader _logReader;

        //ClientID Guid for Splunk
        private static Guid _splunkClientID;

        //Setup Timer for reading logs
        private static Timer _readTimer;

        // Cache off the last read record
        private static LogRecord _lastRecordTransmitted;

        //Runtime Options Object
        private static OptionsStruct _runtimeOptions;
        
        #endregion

        static void Main(string[] args)
        {

            // Setup logging
            log4net.Config.BasicConfigurator.Configure();
            
            try
            {
                _runtimeOptions = JsonConvert.DeserializeObject<OptionsStruct>(System.IO.File.ReadAllText("options.json"));
                _runtimeOptions.IgnoreCacheFileOnFirstRead = true;

                _logReader = new aaLogReader.aaLogReader(_runtimeOptions);

                _splunkClientID = _runtimeOptions.ClientID;

                _client.BaseAddress = new Uri(_runtimeOptions.SplunkBaseAddress);
                _client.DefaultRequestHeaders.Add("Authorization", "Splunk " + _runtimeOptions.AuthorizationToken);

                // Configure Timers
                _readTimer = new Timer(_runtimeOptions.ReadInterval);
                _readTimer.Elapsed += ReadTimer_Elapsed;

                //Start Timers
                _readTimer.Start();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            
            Console.Read();
            return;
        }
        
        /// <summary>
        /// Read Timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var logRecords = _logReader.GetUnreadRecords(_runtimeOptions.MaxUnreadRecords);

                log.InfoFormat("{0} records read", logRecords.Count);

                if (logRecords.Count > 0)
                {
                    var result = TransmitValues(_client, _splunkClientID, logRecords.ToKVP()).Result;
                    
                    // If the last transmission was successful then capture the last record transmitted for the next read
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        // Reset the timer interval to default option on success
                        if(_readTimer.Interval != _runtimeOptions.ReadInterval)
                        {
                            _readTimer.Interval = _runtimeOptions.ReadInterval;
                        }

                        _lastRecordTransmitted = logRecords.Last<LogRecord>();
                        log.InfoFormat("Last MessageNumber : {0}", _lastRecordTransmitted.MessageNumber);
                    }
                    else
                    {
                        log.Warn(result);
                        //Force a reset on the cache back to the previous last read record
                        ResetUnreadRecordPointer();
                    }                    
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }           
        }
        
        /// <summary>
        /// Transmit the log records via HTTP to the Splunk HTTP Raw Collector
        /// </summary>
        /// <param name="client"></param>
        /// <param name="clientID"></param>
        /// <param name="records"></param>
        static async Task<HttpResponseMessage> TransmitValues(HttpClient client, Guid clientID, string kvpValues)
        {

            HttpResponseMessage responseMessage = new HttpResponseMessage();

            try
            {
                responseMessage = await client.PostAsync("/services/collector/raw?channel=" + clientID, new StringContent(kvpValues));
            }
            catch (Exception ex)
            {
                if(ex is HttpRequestException || ex is AggregateException)
                {
                    IncrementTimerBackoff();
                    responseMessage.StatusCode = HttpStatusCode.ServiceUnavailable;
                    responseMessage.ReasonPhrase = string.Format("Transmit failed : {0}", ex.Message);
                }
                else
                {
                    log.Error(ex);
                }

                ResetUnreadRecordPointer();

            }

            return responseMessage;
        }

        private static void ResetUnreadRecordPointer()
        {
            try
            {
                // If we haven't recorded a last record transmitted then pull just the last record
                if (_lastRecordTransmitted != null)
                {
                    log.InfoFormat("Transmission not successful.  Writing status cache file for message number {0}", _lastRecordTransmitted.MessageNumber);
                    _logReader.WriteStatusCacheFile(_lastRecordTransmitted);
                }
                else
                {
                    log.Warn("Last record transmitted is Null");
                    //TODO: Develop a better method to handle.  For now just depend on GetUnread Internal caching
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void IncrementTimerBackoff()
        {
            try
            {
                var currentInterval = _readTimer.Interval;

                if (currentInterval < _runtimeOptions.MaximumReadInterval)
                {
                    _readTimer.Interval = System.Math.Min(currentInterval * 2, _runtimeOptions.MaximumReadInterval);
                    log.WarnFormat("Read Timer interval set to {0} milliseconds", _readTimer.Interval);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
                // Set to a default read interval of 5000
                _readTimer.Interval = 5000;
            }
        }
    }
}
