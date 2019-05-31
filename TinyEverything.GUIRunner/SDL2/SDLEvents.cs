/* SDL2# - C# Wrapper for SDL2
 *
 * Copyright (c) 2013-2016 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */

using System.Runtime.InteropServices;

namespace TinyEverything.GUIRunner.SDL2
{
    public static class SDLEvents
    {
        #region SDL2# Variables

        private const string NativeLibName = "SDL2";

        #endregion



        #region SDL_events.h

        /* The types of events that can be delivered. */
        public enum EventType : uint
        {
            None = 0,
            Quit = 0x100,
            KeyDown = 0x300,
            KeyUp
        }

        /* Keyboard button event structure (event.key.*) */
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowID;
            public byte State;
            public byte Repeat; /* non-zero if this is a repeat */
            private readonly byte padding2;
            private readonly byte padding3;
            public KeySymbol KeySymbol;
        }


        /* The "quit requested" event */
        [StructLayout(LayoutKind.Sequential)]
        public struct QuitEvent
        {
            public EventType type;
            public uint timestamp;
        }

        /* General event structure */
        // C# doesn't do unions, so we do this ugly thing. */
        [StructLayout(LayoutKind.Explicit)]
        public struct Event
        {
            [FieldOffset(0)]
            public EventType Type;
            [FieldOffset(0)]
            public KeyboardEvent Key;
            [FieldOffset(0)]
            public QuitEvent Quit;
        }

        /* Polls for currently pending events */
        [DllImport(NativeLibName, EntryPoint = "SDL_PollEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PollEvent(out Event _event);


        #endregion

        #region SDL_keycode.h

        public const int ScanCodeMask = 1073741824;

        public enum KeyCode
        {
            Escape = 27, // '\033'
            A = 'a',
            B = 'b',
            C = 'c',
            D = 'd',
            E = 'e',
            F = 'f',
            G = 'g',
            H = 'h',
            I = 'i',
            J = 'j',
            K = 'k',
            L = 'l',
            M = 'm',
            N = 'n',
            O = 'o',
            P = 'p',
            Q = 'q',
            R = 'r',
            S = 's',
            T = 't',
            U = 'u',
            V = 'v',
            W = 'w',
            X = 'x',
            Y = 'y',
            Z = 'z',
            Right = 79 | ScanCodeMask,
            Left = 80 | ScanCodeMask,
            Down = 81 | ScanCodeMask,
            Up = 82 | ScanCodeMask,
        }

        /* Key modifiers (bitfield) */
        #endregion

        #region SDL_keyboard.h

        [StructLayout(LayoutKind.Sequential)]
        public struct KeySymbol
        {
            private readonly int padding0;
            public KeyCode Symbol;
            private readonly ushort padding1;
            private readonly uint padding2;
        }


        #endregion
    }
}
