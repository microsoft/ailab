// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;

namespace SnipInsight.Util
{
    internal class LogUploader : IDisposable
    {
        internal delegate string RetrieveContentDelegate(ref DateTime contentTime);
        internal delegate void SuccessCallbackDelegate();
        internal delegate void FailureCallbackDelegate();

        const int cPollingPeriod = 521;

        Uri _url;
        RetrieveContentDelegate _retrieveContent;
        SuccessCallbackDelegate _successCallback;
        FailureCallbackDelegate _failureCallback;
        object _lock;
        DateTime _lastRequest;
        DateTime _lastSent;
        bool _sending;
        System.Threading.Timer _timer;
        int _idleThreshold;
        int _uploadPeriod;

        internal LogUploader(string url, Guid id, string logType, int idleThreshold, int uploadDelay,
            RetrieveContentDelegate retrieveContent, SuccessCallbackDelegate successCallback, FailureCallbackDelegate failureCallback)
        {
            if (retrieveContent == null)
            {
                throw new ArgumentNullException("retrieveContent");
            }
            _url = CreateReportUrl(url, id, logType);
            _idleThreshold = idleThreshold;
            _uploadPeriod = uploadDelay;
            _retrieveContent = retrieveContent;
            _successCallback = successCallback;
            _failureCallback = failureCallback;
            _lock = new object();
            _lastRequest = _lastSent = DateTime.MinValue;
            _timer = new System.Threading.Timer(Callback, null, cPollingPeriod, cPollingPeriod);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
        }

        Uri CreateReportUrl(string reportString, Guid id, string logType)
        {
            UriBuilder url = new UriBuilder(reportString);

            url.Query = string.Format("source=SnipInsight&type={0}&requestId={1}", logType, id.ToString());

            return url.Uri;
        }

        internal void Queue(DateTime requestTime)
        {
            lock (_lock)
            {
                if (requestTime > _lastRequest)
                {
                    _lastRequest = requestTime;
                }
            }
        }

        void Callback(Object state)
        {
            bool send = false;
            DateTime now = DateTime.UtcNow;

            // evaluate whether we need to initiate a send
            lock (_lock)
            {
                if (!_sending)
                {
                    if (_lastRequest > _lastSent)
                    {
                        // process lazy request after the delay has expired
                        if ((now - _lastRequest).TotalMilliseconds > _idleThreshold && (now - _lastSent).TotalMilliseconds > _uploadPeriod)
                        {
                            send = _sending = true;
                        }
                    }
                }
            }

            if (send)
            {
                Send(now);
            }
        }

        async void Send(DateTime sendTime)
        {
            try
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    // Queue for retry
                    Queue(DateTime.UtcNow + TimeSpan.FromMilliseconds(_uploadPeriod));
                    if (_failureCallback != null)
                    {
                        _failureCallback();
                    }
                    return;
                }

                DateTime contentTime = DateTime.MinValue;
                string uploadContent = _retrieveContent(ref contentTime);
                if (!string.IsNullOrEmpty(uploadContent))
                {
                    uploadContent = Compress(uploadContent);
                    using (StringContent payload = new StringContent(uploadContent))
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            // Build the POST request
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _url);
                            request.Headers.Add("X-MS-SnipInsight", "WqCK64ev30QAPKSoLUIEig4s9QJoJqYwzGDMXOrmS7CU3iHrHqZhosksf34QAu==");
                            request.Content = payload;
                            request.Content.Headers.ContentEncoding.Add("gzip");
                            HttpResponseMessage response = await client.SendAsync(request);
                            if (response.IsSuccessStatusCode)
                            {
                                lock (_lock)
                                {
                                    _lastSent = sendTime > contentTime ? sendTime : contentTime;
                                    if (_successCallback != null)
                                    {
                                        _successCallback();
                                    }
                                }
                            }
                            else
                            {
                                // Queue for retry
                                Queue(DateTime.UtcNow + TimeSpan.FromMilliseconds(_uploadPeriod));
                                if (_failureCallback != null)
                                {
                                    _failureCallback();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    // SendAsync may have thrown, queue for retry
                    Queue(DateTime.UtcNow + TimeSpan.FromMilliseconds(_uploadPeriod));
                }
                Diagnostics.LogException(ex);
                if (_failureCallback != null)
                {
                    _failureCallback();
                }
            }
            finally
            {
                lock (_lock)
                {
                    _sending = false;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        static string Compress(string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress, true))
                    {
                        msi.CopyTo(gs);
                    }
                    return Convert.ToBase64String(mso.ToArray());
                }
            }
        }
    }
}
