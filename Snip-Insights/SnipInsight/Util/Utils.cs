// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SnipInsight.Util
{
    internal class Utils
    {
        internal static TimeSpan GetUserIdleTime()
        {
            NativeMethods.LASTINPUTINFO lii = new NativeMethods.LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(lii);

            NativeMethods.GetLastInputInfo(ref lii);
            ulong tickCount = NativeMethods.GetTickCount64();

            return TimeSpan.FromMilliseconds(tickCount - lii.dwTime);
        }

        // if UAC is enabled (default) and the process being queried is running as Built-in Administrator, this function will return 'false'.
        // The BI Admin account is not technically elevated since it is always Admin, so technically the return value of 'false' is correct.
        // Should not be a problem if current Windows user account is BI Admin since (likely) all processes then will be running as BI Admin.
        // Could be a problem if the current Windows user account is not the BI Admin and we're checking a process that was 'Run-As' the BI Admin.
        // In that case the current process elevation might be 'false' and the one running as BI Admin will also be 'false',
        // but indeed there will be an elevation mismatch between the two. This fcn does not account for that specifically as it does not
        // check the current Windows Identity
        // But in practice, the unelevated process querying an elevated one should return the correct result of 'true' with the 'Access Denied'
        // error handling in this fcn
        internal static bool IsProcessRunningElevated(Process process)
        {
            IntPtr processToken = IntPtr.Zero;
            IntPtr tokenInformationElevation = IntPtr.Zero;

            try
            {
                try
                {
                    if (!NativeMethods.OpenProcessToken(process.Handle, NativeMethods.TOKEN_READ, out processToken))
                    {
                        throw new ApplicationException("OpenProcessToken() failed", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                    }
                }
                catch (Exception ex)
                {
                    if (ex is System.ComponentModel.Win32Exception && (uint)ex.HResult == 0x80004005)
                    {
                        // may get Access Denied if current process is not running elevated and tries to open an elevated process
                        Process currentProcess = Process.GetCurrentProcess();
                        if (process != currentProcess)
                        {
                            if (!IsProcessRunningElevated(currentProcess)) // recursion should not re-enter this block when calling with the current process
                            {
                                return true;
                            }
                        }
                    }

                    throw;
                }

                uint tokenInfoLength = (uint)Marshal.SizeOf(typeof(NativeMethods.TokenElevation));
                tokenInformationElevation = Marshal.AllocHGlobal((int)tokenInfoLength);

                if (!NativeMethods.GetTokenInformation(processToken, NativeMethods.TokenInformationClass.TokenElevation, tokenInformationElevation, tokenInfoLength, out tokenInfoLength))
                {
                    throw new InvalidOperationException("GetTokenInformation() failed", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                }

                NativeMethods.TokenElevation elevationStatus = (NativeMethods.TokenElevation)Marshal.PtrToStructure(tokenInformationElevation, typeof(NativeMethods.TokenElevation));

                return elevationStatus.TokenIsElevated > 0;
            }
            finally
            {
                if (processToken != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(processToken);
                }

                if (tokenInformationElevation != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tokenInformationElevation);
                }
            }
        }
    }
}
