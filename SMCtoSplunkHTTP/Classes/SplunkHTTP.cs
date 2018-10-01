// Copyright (c) Andrew Robinson. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace SplunkHTTPUtility
{
    class SplunkHTTP
    {
        private HttpClient client;
        public HttpClient Client
        {
            get
            {
                return client;
            }

            set
            {
                client = value;
            }
        }

        Guid clientID;
        public Guid ClientID
        {
            get
            {
                return clientID;
            }

            set
            {
                clientID = value;
            }
        }

        string splunkBaseAddress;
        public string SplunkBaseAddress
        {
            get
            {
                return splunkBaseAddress;
            }

            set
            {
                splunkBaseAddress = value;
            }
        }

        private ILog log;
        public ILog Log
        {
            get
            {
                return log;
            }

            set
            {
                log = value;
            }
        }

        private string splunkAuthorizationToken;
        public string SplunkAuthorizationToken
        {
            get
            {
                return splunkAuthorizationToken;
            }

            set
            {
                splunkAuthorizationToken = value;
            }
        }

        public SplunkHTTP()
        {
            client = new HttpClient();
            ClientID = Guid.NewGuid();
            Setup();
        }

        public SplunkHTTP(log4net.ILog log, string splunkAuthorizationToken)
        {
            client = new HttpClient();
            ClientID = Guid.NewGuid();
            Log = log;
            SplunkAuthorizationToken = splunkAuthorizationToken;
            Setup();
        }

        public SplunkHTTP(log4net.ILog log, string splunkAuthorizationToken, string splunkBaseAddress)
        {
            client = new HttpClient();
            ClientID = Guid.NewGuid();
            Log = log;
            SplunkAuthorizationToken = splunkAuthorizationToken;
            SplunkBaseAddress = splunkBaseAddress;
            Setup();
        }

        public SplunkHTTP(log4net.ILog log, string splunkAuthorizationToken, string splunkBaseAddress, Guid clientID)
        {
            client = new HttpClient();
            ClientID = clientID;
            Log = log;
            SplunkAuthorizationToken = splunkAuthorizationToken;
            SplunkBaseAddress = splunkBaseAddress;
            Setup();
        }

        public void Setup()
        {
            Log.Debug("Setting up Splunk HTTP Collector");
            Client.BaseAddress = new Uri(SplunkBaseAddress);
            Client.DefaultRequestHeaders.Add("Authorization", "Splunk " + SplunkAuthorizationToken);
        }

        /// <summary>
        /// Transmit the KVP values via HTTP to the Splunk HTTP Raw Collector
        /// </summary>
        /// <param name="kvpValues"></param>
        //public async Task<HttpResponseMessage> TransmitValues (string kvpValues)
        public HttpResponseMessage TransmitValues(string kvpValues)
        {
            var responseMessage = new HttpResponseMessage();
            string uri = "/services/collector/raw?channel=" + ClientID;

            try
            {
                Log.DebugFormat("Transmitting {0} bytes to {1}", System.Text.ASCIIEncoding.Unicode.GetByteCount(kvpValues), SplunkBaseAddress + uri);
                responseMessage = Client.PostAsync(uri, new StringContent(kvpValues)).Result;
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException || ex is AggregateException)
                {
                    responseMessage.StatusCode = HttpStatusCode.ServiceUnavailable;
                    responseMessage.ReasonPhrase = string.Format("Transmit failed : {0}", ex.Message);
                }
                else
                {
                    throw;
                }
            }
            return responseMessage;
        }
    }
}
