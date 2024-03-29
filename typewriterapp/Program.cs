﻿using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Wave;
using System.IO;

namespace typewriterapp
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static int counter;
        private static string mainSound;

        private static float volumeLevel = 0.1f;

        public static void Main()
        {
            mainSound = File.ReadAllLines("settings.txt")[0];
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            ++counter;
            if (counter == 200)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                counter = 0;
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine(vkCode);
                switch (vkCode)
                {
                    //return
                    case 13:
                        PlayWorker("return-new.wav");
                        break;
                    //space
                    case 32:
                        PlayWorker("space-new.wav");
                        break;
                    //backspace
                    case 8:
                        PlayWorker("backspace.wav");
                        break;
                    //ctrl
                    case 162:
                        break;
                    //ctrl right
                    case 163:
                        break;
                    //shift
                    case 160:
                        break;
                    //shift right
                    case 161:
                        break;
                    ////capslock
                    //case 20:
                    //    break;
                    //windows
                    case 91:
                        break;
                    //win right
                    case 93:
                        break;
                    //alt
                    case 164:
                        break;
                    //right alt
                    case 165:
                        break;
                    //array down
                    case 40:
                        PlayWorker("scrollDown.wav");
                        break;
                    //array up
                    case 38:
                        PlayWorker("scrollUp.wav");
                        break;
                    //array left
                    case 37:
                        break;
                    //array right
                    case 39:
                        break;
                    default:
                        PlayWorker(mainSound);
                        break;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void PlayWorker(string filename)
        {
            WaveOut wavePlayer = new WaveOut();
            AudioFileReader audioFileReader = new AudioFileReader(filename);

            audioFileReader.Volume = volumeLevel;
            wavePlayer.Init(audioFileReader);
            wavePlayer.Play();
            wavePlayer.PlaybackStopped += new EventHandler<StoppedEventArgs>((_,__)=> { audioFileReader.Dispose(); wavePlayer.Dispose(); });
        }

        private static void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
