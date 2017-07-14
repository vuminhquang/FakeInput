using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RemoteDesktopClient.Tools
{
    public class Input
    {
        struct INPUT
        {
            public INPUTType type;
            public INPUTUnion Event;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUTUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        enum INPUTType : uint
        {
            INPUT_KEYBOARD = 1
        }

        [Flags]
        enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(int numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);
        [DllImport("user32.dll")]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public static uint ProcessKey(int[] key)
        {
            int countExceptionKeys = 0;
            for (int i = 0; i < key.Length; i++)
            {
                switch ((uint) key[i])
                {
                    //CTRL. ALT, SHIFT, CAPS not realeased after sendkey, need to process KEYEVENTF.KEYUP
                    case 0x11: //CTRL
                    case 0x12://ALT
                    case (uint)Keys.Shift:
                    case (uint)Keys.CapsLock:
                        countExceptionKeys++;
                        break;
                }
            }

            INPUT[] inputs = new INPUT[key.Length + 1 + countExceptionKeys];

            for (int i = 0; i < key.Length; i++)
            {
                switch ((uint) key[i])
                {
                    case (uint)Keys.ControlKey:
                    case (uint)Keys.Menu://Alt
                    case (uint) Keys.ShiftKey:
                    case (uint) Keys.CapsLock:
                        //CTRL. ALT, SHIFT, CAPS not realeased after sendkey, need to process KEYEVENTF.KEYUP
                        uint ctrlkey = MapVirtualKey((uint) key[i], 0);
                        inputs[key.Length + 1 + i].type = INPUTType.INPUT_KEYBOARD;
                        inputs[key.Length + 1 + i].Event.ki.dwFlags = KEYEVENTF.SCANCODE;
                        inputs[key.Length + 1 + i].Event.ki.dwFlags |= KEYEVENTF.KEYUP;
                        inputs[key.Length + 1 + i].Event.ki.wScan = (ushort) ctrlkey;
                        break;
                }
                uint skey = MapVirtualKey((uint)key[i], 0);
                inputs[i].type = INPUTType.INPUT_KEYBOARD;
                inputs[i].Event.ki.dwFlags = KEYEVENTF.SCANCODE;
                inputs[i].Event.ki.wScan = (ushort)skey;
            }

            inputs[key.Length].type = INPUTType.INPUT_KEYBOARD;
            inputs[key.Length].Event.ki.dwFlags = KEYEVENTF.UNICODE;
            inputs[key.Length].Event.ki.dwFlags |= KEYEVENTF.KEYUP;
            uint cSuccess = SendInput(inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            return cSuccess;
        }
    }
}