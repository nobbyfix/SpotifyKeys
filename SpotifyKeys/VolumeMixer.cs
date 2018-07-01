using System;
using System.Runtime.InteropServices;

namespace SpotifyKeys
{
    public sealed class VolumeMixer
    {
        private VolumeMixer() { }
        
        public static float? GetVolume(uint pid)
        {
            ISimpleAudioVolume volume = GetAudioVolume(pid);
            if (volume != null)
            {
                volume.GetMasterVolume(out float level);
                Marshal.ReleaseComObject(volume);
                return level * 100;
            }
            return null;
        }

        public static bool? GetMute(uint pid)
        {
            ISimpleAudioVolume volume = GetAudioVolume(pid);
            if (volume != null)
            {
                volume.GetMute(out bool mute);
                Marshal.ReleaseComObject(volume);
                return mute;
            }
            return null;
        }

        public static void SetVolume(uint pid, float level)
        {
            ISimpleAudioVolume volume = GetAudioVolume(pid);
            if(volume != null)
            {
                Guid guid = Guid.Empty;
                volume.SetMasterVolume(level / 100, ref guid);
                Marshal.ReleaseComObject(volume);
            }
        }

        public static void SetMute(uint pid, bool mute)
        {
            ISimpleAudioVolume volume = GetAudioVolume(pid);
            if (volume != null)
            {
                Guid guid = Guid.Empty;
                volume.SetMute(mute, ref guid);
                Marshal.ReleaseComObject(volume);
            }
        }

        private static ISimpleAudioVolume GetAudioVolume(uint pid)
        {
            // get current active speaker device
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out IMMDevice audioDevice);

            // activate AudioSessionManager for current device
            Guid iidAudioSessionManager = typeof(IAudioSessionManager2).GUID;
            audioDevice.Activate(ref iidAudioSessionManager, 0, IntPtr.Zero, out object obj);
            IAudioSessionManager2 audioSessionManager = (IAudioSessionManager2)obj;

            // IAudioSessionEnumerator to enumerate through all audio sessions
            audioSessionManager.GetSessionEnumerator(out IAudioSessionEnumerator audioSessionEnumerator);
            audioSessionEnumerator.GetCount(out int count);

            // iterate through all audio sessions
            // find matching audio session by process id
            ISimpleAudioVolume simpleAudioVolume = null;
            for(int i = 0; i < count; i++)
            {
                audioSessionEnumerator.GetSession(i, out IAudioSessionControl2 audioSessionControl);
                audioSessionControl.GetProcessId(out uint audioPid);
                if(audioPid == pid)
                {
                    simpleAudioVolume = audioSessionControl as ISimpleAudioVolume;
                    break;
                }
                Marshal.ReleaseComObject(audioSessionControl);
            }
            Marshal.ReleaseComObject(audioSessionEnumerator);
            Marshal.ReleaseComObject(audioSessionManager);
            Marshal.ReleaseComObject(audioDevice);
            Marshal.ReleaseComObject(deviceEnumerator);
            return simpleAudioVolume;
        }
    }

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator { }

    internal enum EDataFlow
    {
        eRender,
        eCapture,
        eAll,
        EDataFlow_enum_count
    }

    internal enum ERole
    {
        eConsole,
        eMultimedia,
        eCommunications,
        ERole_enum_count
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        int NotImpl1();

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice device);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        int NotImplemented1();
        int NotImplemented2();

        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator audioSessionEnumerator);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig]
        int GetCount(out int count);

        [PreserveSig]
        int GetSession(int count, out IAudioSessionControl2 audioSessionControl);
    }

    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2
    {
        [PreserveSig]
        int NotImpl0();

        [PreserveSig]
        int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        [PreserveSig]
        int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)]string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

        [PreserveSig]
        int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        [PreserveSig]
        int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

        [PreserveSig]
        int GetGroupingParam(out Guid pRetVal);

        [PreserveSig]
        int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

        [PreserveSig]
        int NotImpl1();

        [PreserveSig]
        int NotImpl2();

        [PreserveSig]
        int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        [PreserveSig]
        int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        [PreserveSig]
        int GetProcessId(out uint pid);

        [PreserveSig]
        int IsSystemSoundsSession();

        [PreserveSig]
        int SetDuckingPreference(bool optOut);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fevel, ref Guid eventContext);

        [PreserveSig]
        int GetMasterVolume(out float level);

        [PreserveSig]
        int SetMute(bool mute, ref Guid eventContext);

        [PreserveSig]
        int GetMute(out bool mute);
    }
}
