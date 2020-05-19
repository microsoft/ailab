using System;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Speaker.Recorder.Services.IdentificationService
{
    public class DesktopNameIdentificationService : BaseIdentificationService
    {
        public static string RawIdentifier;

        public override string GetRawIdentifier()
        {
            if (RawIdentifier == null)
            {
                RawIdentifier = $"{Environment.MachineName}-{GetMachineIdentifier()}";
            }

            return RawIdentifier;
        }

        public static string GetMachineIdentifier()
        {
            try
            {
                string uuid = string.Empty;

                using ManagementClass mc = new ManagementClass("Win32_ComputerSystemProduct");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    uuid = mo.Properties["UUID"].Value.ToString();
                    break;
                }

                return uuid;
            }
            catch { }
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Only consider Ethernet network interfaces
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                        nic.OperationalStatus == OperationalStatus.Up)
                    {
                        return nic.GetPhysicalAddress().ToString();
                    }
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
