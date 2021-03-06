﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreAudio;

namespace WinAudioMute
{
    class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        internal static void AttachConsoleForCommandLineMode()
        {
            //must call before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);
        }


        private static MMDevice device;

        static void Main(string[] args)
        {
            int exitcode = Main2(args);
            // HACK
            // for some reason the app hangs before exit after a mute/unmute call. I had to do this to get an immediate exit!
            // Uncomment if you really need an immediate exit
            //System.Diagnostics.Process.GetCurrentProcess().Kill(); 
            // /END HACK
            // If above is uncommented.. then the below isn't always reached...
            Environment.Exit(exitcode);
        }

        internal static int Main2(string[] args)
        {
            MMDeviceEnumerator denum = new MMDeviceEnumerator();
            // Print devs for testing
            if (args.Length > 0 && args[0] == "-p")
            {
                MMDeviceCollection coll = denum.EnumerateAudioEndPoints(EDataFlow.eRender, DEVICE_STATE.DEVICE_STATE_ACTIVE);
                AttachConsoleForCommandLineMode();
                System.Console.WriteLine("Number of devs: {0}", coll.Count);
                for (int i = 0; i < coll.Count; ++i)
                {
                    MMDevice d = coll[i];
                    System.Console.WriteLine("Dev: {0} / FriendlyName: {1} / ID: {2}", i, d.FriendlyName, d.ID);
                    AudioEndpointVolume vol = d.AudioEndpointVolume;
                    System.Console.WriteLine("    Muted?: {0}  MasterVol: {1}  MasterVolScalar: {2}", vol.Mute,
                        vol.MasterVolumeLevel, vol.MasterVolumeLevelScalar);
                }
                return 0;
            }
            //

            device = denum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

            if (device == null)
            {
                AttachConsoleForCommandLineMode();
                System.Console.Error.WriteLine("ERROR: Could not get default audio endpoint (no sound device?)");
                return 1;
            }

            if (args.Length > 0 && args[0].ToLower() == "mute")
            {
                Mute();
            }
            else
            {
                UnMute();
            }

            return 0;
        }

        private static void Mute()
        {
            if (!device.AudioEndpointVolume.Mute)
                device.AudioEndpointVolume.Mute = true;
        }

        private static void UnMute()
        {
            if (device.AudioEndpointVolume.Mute)
                device.AudioEndpointVolume.Mute = false;
        }
    }
}
