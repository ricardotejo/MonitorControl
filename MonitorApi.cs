using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonitorControl
{




    public class MonitorApi : IDisposable
    {

        public void TEST()
        {
            List<(IntPtr ptr, Rect rect, int d)> hMonitors = new List<(IntPtr, Rect, int)>();

            MonitorEnumProc callback = (IntPtr hDesktop, IntPtr hdc, ref Rect prect, int d) =>
            {
                hMonitors.Add((hDesktop, prect, d));
                return true;
            };

            List<PHYSICAL_MONITOR> monitors = new List<PHYSICAL_MONITOR>();
            if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0))
            {
                foreach (var m in hMonitors)
                {
                    uint mcount = 0;
                    if (!GetNumberOfPhysicalMonitorsFromHMONITOR(m.ptr, ref mcount))
                    {
                        throw new Exception("Cannot get monitor count!");
                    }
                    PHYSICAL_MONITOR[] physicalMonitors = new PHYSICAL_MONITOR[mcount];

                    if (!GetPhysicalMonitorsFromHMONITOR(m.ptr, mcount, physicalMonitors))
                    {
                        throw new Exception("Cannot get phisical monitor handle!");
                    }

                    Debug.WriteLine($"PM:{physicalMonitors.Length}) RECT: T:{m.rect.top}/L:{m.rect.left}/R:{m.rect.right}/B:{m.rect.bottom}  SCALE:{m.d}");
                    monitors.AddRange(physicalMonitors);
                }

                for (int i = 0; i < monitors.Count; i++)
                {

                    uint cv = 0, mv = 0;
                    if (GetVCPFeatureAndVCPFeatureReply(monitors[i].hPhysicalMonitor, SVC_FEATURE__POWER_MODE, IntPtr.Zero, ref cv, ref mv))
                    {
                        Debug.WriteLine($"{i}) {monitors[i].hPhysicalMonitor} + {monitors[i].szPhysicalMonitorDescription} + `{cv}/{mv}`");
                    }
                    else
                    {
                        string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                        Debug.WriteLine($"{i}) ERROR: {errorMessage}");

                        // TODO:  use HighLevel API to set brightness on these monitors
                        // https://docs.microsoft.com/en-us/windows/win32/api/highlevelmonitorconfigurationapi/nf-highlevelmonitorconfigurationapi-setmonitorbrightness
                    }
                }

                // call power

                //:: Current value>  1=On / 2=Standby / 3=Suspended / 4=Off / 5=OffByButton
                MonitorSwitch(monitors[2], PowerModeEnum.PowerOn); // <<<< OK but doesn't work on laptop screen
                //MonitorBrightness(monitors[0], 60);  // <<<< OK but doesn't work on laptop screen

                PHYSICAL_MONITOR[] toDestroy = monitors.ToArray();
                DestroyPhysicalMonitors((uint)toDestroy.Length, ref toDestroy);
            }
            else
            {
                //error
            }

        }



        //Switch monitor power
        void MonitorSwitch(PHYSICAL_MONITOR monitor, PowerModeEnum mode)
        {
            if (SetVCPFeature(monitor.hPhysicalMonitor, SVC_FEATURE__POWER_MODE, (uint)mode))
            {
                Debug.WriteLine("works!");
            }
            else
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Debug.WriteLine(errorMessage);
            }
        }


        //Switch monitor power
        void MonitorBrightness(PHYSICAL_MONITOR monitor, uint pctg)
        {
            if (SetVCPFeature(monitor.hPhysicalMonitor, SVC_FEATURE__Brightness, (uint)pctg))
            {
                Debug.WriteLine("works!");
            }
            else
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Debug.WriteLine(errorMessage);
            }
        }

    }
}
