using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using ArchestrA.MxAccess;
using aaMXItem;
using InfluxData.Net;
using InfluxData.Net.Helpers;
using InfluxData.Net.Models;
using InfluxData.Net.Infrastructure.Influx;
using System.Collections.Concurrent;
using System.Timers;
using aaInflux;


[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]
namespace aaInflux
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // LMX Interface declarations
        private static ArchestrA.MxAccess.LMXProxyServerClass _LMX_Server;

        // handle of registered LMX server interface
        private static int _hLMX;

        //dictionary of tag handles and tag names
        private static Dictionary<int,subscription> _MXAccessTagDictionary;

        //object for MXAccess Settings
        private static localMXAccessSettings _MXAccessSettings;

        //Object for Influx Client Connection
        private static InfluxDb _InfluxClient;

        //Local stack of points to write in batches
        private static BlockingCollection<Point> _WriteItemCollection;

        private static BlockingCollection<Point> _StoreForwardPoints;

        //Setup Timer
        private static Timer _writeTimer;

        //Setup Timer
        private static Timer _StoreForwardTimer;

        static void Main(string[] args)
        {
            try
            {
                log.Info("Starting " + System.AppDomain.CurrentDomain.FriendlyName);

                _MXAccessTagDictionary = new Dictionary<int, subscription>();
                _WriteItemCollection = new BlockingCollection<Point>();
                _StoreForwardPoints = new BlockingCollection<Point>(new ConcurrentStack<Point>());

                _writeTimer = new Timer(3000);
                _writeTimer.Elapsed += _writeTimer_Elapsed;

                _StoreForwardTimer = new Timer(10000);
                _StoreForwardTimer.Elapsed += _StoreForwardTimer_Elapsed;
                
                // Simultaneous connect to MXAccess and InFlux
                Task.Run(async() =>
                {
                    await ConnectMXAccessAsync();
                    ConnectInfluxClient();
                }
               ).Wait();
                
                _writeTimer.Start();
                _StoreForwardTimer.Start();

                Console.ReadKey();

            }
            catch (Exception ex)
            {
                log.Error(ex);
                Console.ReadKey();
            }
            finally
            { 
                // Always disconnect on shutdown
                DisconnectMXAccess();                      
            }

        }
        
        private static bool ConnectInfluxClient()
        {
            localInfluxConnectionSettings influxSettings;
            bool isAwake = false;

            try
            {
                // Read in the MX Access Settings from the configuration file
                influxSettings = JsonConvert.DeserializeObject<localInfluxConnectionSettings>(System.IO.File.ReadAllText("influx.json"));

                log.InfoFormat("Connecting to Influx URL {0} with UserID {1}", influxSettings.URL, influxSettings.userid);

                _InfluxClient = new InfluxDb(influxSettings.URL,influxSettings.userid,influxSettings.password, InfluxData.Net.Enums.InfluxVersion.Auto);
                
                log.InfoFormat("Testing connection with Ping/Pong");
                isAwake = _InfluxClient.IsAwake();
                                
                if (!isAwake)
                {
                    throw new Exception("Ping to " + influxSettings.URL + " not successful.");
                }
                else
                {
                    return true;
                }                
            }
            catch
            {
                throw;
            }        
        }

       static Task ConnectMXAccessAsync()
        {
            int hitem;

            try
            {
                if (_LMX_Server == null)
                {
                    // instantiate an ArchestrA.MxAccess.LMXProxyServer
                    try
                    {
                        _LMX_Server = new ArchestrA.MxAccess.LMXProxyServerClass();
                    }
                    catch
                    {
                        throw;
                    }
                }

                if ((_LMX_Server != null) && (_hLMX == 0))
                {
                    // Register with LMX and get the Registration handle
                    _hLMX = _LMX_Server.Register("aaDataForwarderApp");

                    // connect the event handlers
                    _LMX_Server.OnDataChange += new _ILMXProxyServerEvents_OnDataChangeEventHandler(LMX_OnDataChange);
                }

                // Read in the MX Access Settings from the configuration file
                _MXAccessSettings = JsonConvert.DeserializeObject<localMXAccessSettings>(System.IO.File.ReadAllText("mxaccess.json"));


                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                Task.Run(() =>
                {
                    // Subscribe to all values
                    foreach (subscription sub in _MXAccessSettings.subscribetags)
                    {
                        log.Info("Adding Subscribe for " + sub.tag);
                        hitem = _LMX_Server.AddItem(_hLMX, sub.tag);

                        if (hitem > 0)
                        {
                            sub.hitem = hitem;
                            _MXAccessTagDictionary.Add(hitem, sub);
                            _LMX_Server.AdviseSupervisory(_hLMX, hitem);
                        }
                        else
                        {
                            log.WarnFormat("Failed to subscribe to tag {0}", sub);
                        }
                    }

                    tcs.SetResult(true);
                });

                return tcs.Task;
            }
            catch
            {
                throw;
            }
        }

        static void DisconnectMXAccess()
        {
            try
            {
                log.Info("Disconnecting MXAccess");

                // Remove all items 
                foreach (int hitem in _MXAccessTagDictionary.Keys)
                {
                    _LMX_Server.UnAdvise(_hLMX, hitem);
                    _LMX_Server.RemoveItem(_hLMX, hitem);
                }

                // Unregister
                _LMX_Server.Unregister(_hLMX);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void LMX_OnDataChange(int hLMXServerHandle, int phItemHandle, object pvItemValue, int pwItemQuality, object pftItemTimeStamp, ref MXSTATUS_PROXY[] pVars)
        {
            try
            {
                DateTime ts;
                ts = DateTime.Parse(pftItemTimeStamp.ToString());

                aaMXInfluxItem newItem = new aaMXInfluxItem();

                newItem.ItemHandle = phItemHandle;
                newItem.Quality = pwItemQuality;
                newItem.ServerHandle = hLMXServerHandle;
                newItem.TagName = _MXAccessTagDictionary[phItemHandle].tag;
                newItem.influxTag = _MXAccessTagDictionary[phItemHandle].influxtag;
                newItem.TimeStamp = DateTime.Parse(ts.ToString());
                newItem.Value = pvItemValue;
                
                _WriteItemCollection.Add(newItem.GetInfluxPoint());

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        
        private static async void WritePointsAsync(Point[] points)
        {
            try
            {
                InfluxDbApiResponse writeResponse = await _InfluxClient.WriteAsync("testdb", points);
                
                if (!writeResponse.Success)
                {
                    log.ErrorFormat("Failed to write points. Write Response: {0}", writeResponse.ToJson());
                    log.ErrorFormat("Adding {0} points to Store/Forward",points.Length);
                    bool result = _StoreForwardPoints.TryAddArray<Point>(points);
                    log.ErrorFormat("Store/Forward now has {0} points.", _StoreForwardPoints.Count);
                }  
                else
                {
                    log.DebugFormat("Wrote {0} points with Response : {1}", points.Length, writeResponse.ToJson());
                }              
            }
            catch
            {
                log.WarnFormat("Error attempting to write points.");
                log.WarnFormat("Adding {0} points to Store/Forward", points.Length);
                bool result = _StoreForwardPoints.TryAddArray<Point>(points);
                log.WarnFormat("Store/Forward now has {0} points.", _StoreForwardPoints.Count);
            }
        }

        private static void _StoreForwardTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int maxPointsPerBatch = 100;
            int sleepDelay = 1000;

            List<Point> points = new List<Point>();
            Point point;

            try
            {
                //First see if we have any points to send
                while (_StoreForwardPoints.Count > 0)
                {
                    //Verify the Server is Awake
                    if (_InfluxClient.IsAwake())
                    {

                        while ((_StoreForwardPoints.TryTake(out point) && (points.Count < maxPointsPerBatch)))
                        {                            
                            points.Add(point);                            
                        }

                        if (points.Count > 0)
                        {
                            log.DebugFormat("Sending {0} points from Store/Forward", points.Count);
                            WritePointsAsync(points.ToArray<Point>());
                        }

                        points.Clear();

                        log.DebugFormat("Sleep {0} milliseconds before next Store/Forward Batch", sleepDelay);
                        System.Threading.Thread.Sleep(sleepDelay);
                    }
                
                    else
                    {
                        log.Warn("InfluxDB Server is not available.  Waiting to retry.");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void _writeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<Point> points = new List<Point>();
            Point item;

            try
            {
                while(_WriteItemCollection.TryTake(out item))
                {                
                    points.Add(item);
                }

                if (points.Count > 0)
                {
                    log.DebugFormat("Sending {0} points from WriteCache", points.Count);
                    WritePointsAsync(points.ToArray<Point>());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
