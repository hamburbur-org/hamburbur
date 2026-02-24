using System.Runtime.InteropServices;

namespace hamburbur.Misc;

public class WindowsMediaController
{
    //  https://github.com/The-Graze/MusicControls

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    private static void SendKey(VirtualKeyCodes virtualKeyCode) => keybd_event((uint)virtualKeyCode, 0, 0, 0);
    public static  void NextTrack()                             => SendKey(VirtualKeyCodes.NextTrack);
    public static  void PreviousTrack()                         => SendKey(VirtualKeyCodes.PreviousTrack);
    public static  void PlayPause()                             => SendKey(VirtualKeyCodes.PlayPause);

    private enum VirtualKeyCodes
            : uint
    {
        NextTrack     = 0xB0,
        PreviousTrack = 0xB1,
        PlayPause     = 0xB3,
    }
}