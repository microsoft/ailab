// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Net.NetworkInformation;

namespace SnipInsight.Telemetry
{
    /// <summary>
    /// Singleton Class for ILogger class for logging events using ARIA
    /// </summary>
    class ApplicationLogger
    {
        private static ApplicationLogger applicationLogger = new ApplicationLogger();
        private string versionNumber, macAddress;

        /// <summary>
        /// Get function for the instance of Singleton ApplicationLogger Class
        /// </summary>
        public static ApplicationLogger Instance
        {
            get
            {
                return applicationLogger;
            }
        }

        /// <summary>
        ///  Constructor for Application Logger private to follow singleton design pattern
        /// </summary>
        private ApplicationLogger()
        {
        }

        /// <summary>
        /// Generates initialization event to submit to ARIA
        /// </summary>
        public void SubmitEvent(string eventName)
        {
            // TODO: Log generic user events
        }

        public void SubmitButtonClickEvent(string eventName, string viewName)
        {
            // TODO : Log clicks on UI elements by users
        }

        public void SubmitApiCallEvent(string eventName, string apiCalled, long timeToCompleteInMilliSeconds, string apiResponseStatusCode)
        {
            // TODO : Log web request to AI services
        }

        public void SubmitStateTransitionEvent(string eventName,
            StateMachine.SnipInsightState source,
            StateMachine.SnipInsightTrigger trigger,
            StateMachine.SnipInsightState destination)
        {
            // TODO : Log transition of states in application.
        }

        /// <summary>
        /// Initializes Application Logger
        /// </summary>
        public void Initialize()
        {
            //TODO: Initialize the or set the logging module to be used to gather user telemetry
            versionNumber = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            macAddress = GetMacAddress();
        }

        /// <summary>
        /// Finds the MAC address of the NIC with maximum speed.
        /// </summary>
        /// <returns>The MAC address.</returns>
        private string GetMacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }
    }
}
