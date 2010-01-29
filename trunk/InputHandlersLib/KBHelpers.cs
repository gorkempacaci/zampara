//Copyright 2008 David Fidge
//Licensed under the Apache License, Version 2.0 (the "License"); 
//you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software 
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License.

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Text;
using InputHandlers.State;
using InputHandlers.StateMachine;


namespace InputHandlers.Keyboard
{



    /// <summary>
    /// This is a bit field for "modifier" keys, i.e. control, shift and alt keys.  Bit 1 = either control key down, bit 2 = either shift key down, bit 3 = either alt key down
    /// </summary>
    [FlagsAttribute]
    public enum KBModifiers : int
    {
        None = 0,
        Ctrl = 1,
        Shift = 2,
        Alt = 4
    };

    /// <summary>
    /// Provides helper functions for operating with Keys objects
    /// </summary>
    public static class KBHelper
    {
        //change this to false if you do not want the numeric keypad treated as if it is normal number keys
        public static bool treatnumpadasnumeric = true;

        public static bool IsKeyAlpha(Keys k)
        {

            if (k >= Keys.A && k <= Keys.Z)
            {
                return true;
            }
            return false;
        }
        public static bool IsKeyNumber(Keys k)
        {
            if (k >= Keys.D0 && k <= Keys.D9)
            {
                return true;
            }
            return false;
        }
        public static bool IsKeyNumberpad(Keys k)
        {
            if (k >= Keys.NumPad0 && k <= Keys.NumPad9)
            {
                return true;
            }
            return false;
        }

        public static bool IsKeyNumeric(Keys k)
        {
            if (IsKeyNumber(k))
            {
                return true;
            }
            if (treatnumpadasnumeric && IsKeyNumberpad(k))
            {
                return true;
            }
            return false;
        }

        public static bool IsKeyAlphanumeric(Keys k)
        {
            if (IsKeyAlpha(k) || IsKeyNumeric(k))
                return true;

            return false;
        }

        public static bool IsFkey(Keys k)
        {
            if (k >= Keys.F1 && k <= Keys.F12)
            {
                return true;
            }
            return false;
        }

        public static bool IsKeySpace(Keys k)
        {
            return k == Keys.Space;
        }

        public static bool IsShift(Keys k)
        {
            if (k == Keys.LeftShift || k == Keys.RightShift)
            {
                return true;
            }
            return false;
        }
        public static bool IsCtrl(Keys k)
        {
            if (k == Keys.LeftControl || k == Keys.RightControl)
            {
                return true;
            }
            return false;
        }
        public static bool IsAlt(Keys k)
        {
            if (k == Keys.LeftAlt || k == Keys.RightAlt)
            {
                return true;
            }
            return false;
        }

        public static bool IsShiftDown(KBModifiers m)
        {
            if ((KBModifiers.Shift & m) == KBModifiers.Shift)
                return true;
            return false;
        }
        public static bool IsCtrlDown(KBModifiers m)
        {
            if ((KBModifiers.Ctrl & m) == KBModifiers.Ctrl)
                return true;
            return false;
        }
        public static bool IsAltDown(KBModifiers m)
        {
            if ((KBModifiers.Alt & m) == KBModifiers.Alt)
                return true;
            return false;
        }

        /// <summary>
        /// returns whether this key is a "modifier" key i.e. control, shift or alt
        /// </summary>
        public static bool IsMod(Keys k)
        {
            return (KBHelper.IsShift(k) || KBHelper.IsAlt(k) || KBHelper.IsCtrl(k));
        }

        /// <summary>
        /// returns kbmodifer object which has shift bit flagged if a shift key was pressed
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static KBModifiers IsShiftM(Keys k)
        {
            if (k == Keys.LeftShift || k == Keys.RightShift)
            {
                return KBModifiers.Shift;
            }
            return KBModifiers.None;
        }

        /// <summary>
        /// returns kbmodifier object with control key flagged if a control key was pressed
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static KBModifiers IsCtrlM(Keys k)
        {
            if (k == Keys.LeftControl || k == Keys.RightControl)
            {
                return KBModifiers.Ctrl;
            }
            return KBModifiers.None;
        }

        /// <summary>
        /// returns kbmodifier object with alt key flagged if an alt key is pressed
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static KBModifiers IsAltM(Keys k)
        {
            if (k == Keys.LeftAlt || k == Keys.RightAlt)
            {
                return KBModifiers.Alt;
            }
            return KBModifiers.None;
        }
        /// <summary>
        /// convert the current key into a printable string.  Defaults to any kind of printable character and accounts for shift key. 
        /// </summary>
        /// <param name="k"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string ToPrintableString(Keys k, KBModifiers m)
        {
            return ToPrintableString(k, m, true, true, true, false);
        }
        /// <summary>
        /// convert the current key into a printable string.  Defaults to alphanumerics on and and accounts for shift key
        /// </summary>
        /// <param name="k"></param>
        /// <param name="m"></param>
        /// <param name="selectspecials"></param>
        /// <returns></returns>
        public static string ToPrintableString(Keys k, KBModifiers m, bool selectspecials)
        {
            return ToPrintableString(k, m, selectspecials, true, true, false);
        }

        /// <summary>
        /// convert the current key into a printable string given filtering options. Defaults to spaces allowed
        /// </summary>
        /// <param name="k"></param>
        /// <param name="m"></param>
        /// <param name="selectspecials">filters so that specials are included in the string if true</param>
        /// <param name="selectalphas">filters so that alphas are selected if true</param>
        /// <param name="selectnumerics">filters so that numerics are selected if true </param>
        /// <returns></returns>
        public static string ToPrintableString(Keys k, KBModifiers m, bool selectspecials, bool selectalphas, bool selectnumerics)
        {
            return ToPrintableString(k, m, selectspecials, selectalphas, selectnumerics, false);
        }


        /// <summary>
        /// convert the current key into a printable string.  If users want to force upper case or lower case they can change modifiers m (for example, to enable use of the caps lock key, test it outside the function, if its on pass in a kbmodifiers with shift flag, if off pass KBModifiers.None)
        /// </summary>
        /// <param name="k"></param>
        /// <param name="m"></param>
        /// <param name="selectspecials">filters so that specials are included in the string if true</param>
        /// <param name="selectalphas">filters so that alphas are selected if true</param>
        /// <param name="selectnumerics">filters so that numerics are selected if true </param>
        /// <param name="suppressspace">no spaces are output</param>
        /// <returns></returns>
        public static string ToPrintableString(Keys k, KBModifiers m, bool selectspecials, bool selectalphas, bool selectnumerics, bool suppressspace)
        {

            if (IsKeySpace(k) && !suppressspace)
            {
                return " ";
            }
            else if ((IsKeyAlpha(k) && selectalphas) ||
                     (IsKeyNumber(k) && selectnumerics) ||
                     (treatnumpadasnumeric && IsKeyNumberpad(k) && selectnumerics) ||
                     (selectspecials && ((!IsKeyAlpha(k) && !IsKeyNumeric(k)) || (IsKeyNumber(k) && IsShiftDown(m)))))
            {
                if (IsShiftDown(m))
                {
                    if (!(!selectspecials && IsKeyNumber(k)))
                        return shiftedkeysstring[k.GetHashCode()];
                }
                else
                {
                    return unshiftedkeysstring[k.GetHashCode()];
                }
            }

            return "";

            #region testcode
            //char []ch = new char[1];

            //byte []asciicode = new byte[1];
            //int i = k.GetHashCode();
            //TypeCode tc = k.GetTypeCode();

            //int bused = 0;
            //int chused = 0;
            //bool wascompleted = false;
            //if (i != 0)
            //{
            //    asciicode[0] = Convert.ToByte(k.GetHashCode());

            //    Decoder d = Encoding.ASCII.GetDecoder();

            //    d.Convert(asciicode,0,1,ch,0,1,true,out bused,out chused,out wascompleted);
            //    //d.GetChars(asciicode, 0, 1, ch, 0);

            //    string blah = new string(ch);

            //    return new string(ch);
            //}
            //ch[1] = '\0';

            //if (ch[0] == '\0')
            //    return "";

            // blah.


            //return "";
            #endregion

        }

        /// <summary>
        /// gets a bit field (kbmodifiers) indicating which "modifier" keys are down (control, shift, alt)
        /// </summary>
        /// <param name="ks">keyboard state to test</param>
        /// <returns>bit field with modifier keys flagged</returns>
        public static KBModifiers GetModifiers(KeyboardState ks)
        {
            KBModifiers mod = KBModifiers.None;
            Keys[] klist = ks.GetPressedKeys();
            foreach (Keys dwn in klist)
            {
                mod = mod | IsShiftM(dwn);
                mod = mod | IsAltM(dwn);
                mod = mod | IsCtrlM(dwn);
            }

            return mod;
        }



        #region US keyboard mapping

        //adapted from http://forums.xna.com/forums/p/5462/28811.aspx "The ZMan"

        private static string[] unshiftedkeysstring = new string[] 
            {
                "", "", "", "", "", "", "", "", "", "", 
                "", "", "", "", "", "", "", "", "", "", 
                "", "", "", "", "", "", "", "", "", "", 
                "", "", " ",  "", "", "", "", "", "", "", //30-39
                "", "", "", "", "", "", "", "", "0",  "1",  //40-49
                "2",  "3",  "4",  "5",  "6",  "7",  "8",  "9",  "", "", //50-59
                "", "", "", "", "", "a",  "b",  "c",  "d",  "e", //60-69
                "f",  "g",  "h",  "i",  "j",  "k",  "l",  "m",  "n",  "o", //70-79
                "p",  "q",  "r",  "s",  "t",  "u",  "v",  "w",  "x",  "y", //80-89
                "z",  "", "", "", "", "", "0",  "1",  "2",  "3", //90-99
                "4",  "5",  "6",  "7",  "8",  "9",  "*",  "+",  "", "-", //100-109
                ".",  "/",  "", "", "", "", "", "", "", "", //110-119
                "", "", "", "", "", "", "", "", "", "", //120-129
                "", "", "", "", "", "", "", "", "", "", //130-139
                "", "", "", "", "", "", "", "", "", "", //140-149
                "", "", "", "", "", "", "", "", "", "", //150-159
                "", "", "", "", "", "", "", "", "", "", //160-169
                "", "", "", "", "", "", "", "", "", "", //170-179
                "", "", "", "", "", "", ";",  "=",  ",",  "-", //180-189
                ".",  "/",  "`",  "", "", "", "", "", "", "", //190-199
                "", "", "", "", "", "", "", "", "", "", //200-209
                "", "", "", "", "", "", "", "", "", "[", //210-219
                "\\",  "]", "'", "", "", "", "", "", "", "", //220-229
                "", "", "", "", "", "", "", "", "", "", //230-239
                "", "", "", "", "", "", "", "", "", "", //240-249
                "", "", "", "", "", ""};                         //250-255

        private static string[] shiftedkeysstring = new string[] 
            {
                "", "", "", "", "", "", "", "", "", "", 
                "", "", "", "", "", "", "", "", "", "", 
                "", "", "", "", "", "", "", "", "", "", 
                "", "", " ",  "", "", "", "", "", "", "", //30-39
                "", "", "", "", "", "", "", "", ")",  "!",  //40-49
                "@",  "#",  "$",  "%",  "^",  "&",  "*",  "(",  "", "", //50-59
                "", "", "", "", "", "A",  "B",  "C",  "D",  "E", //60-69
                "F",  "G",  "H",  "I",  "J",  "K",  "L",  "M",  "N",  "O", //70-79
                "P",  "Q",  "R",  "S",  "T",  "U",  "V",  "W",  "X",  "Y", //80-89
                "Z",  "", "", "", "", "", "",  "",  "",  "", //90-99
                "",  "",  "",  "",  "",  "",  "*",  "+",  "", "-", //100-109
                ".",  "/",  "", "", "", "", "", "", "", "", //110-119
                "", "", "", "", "", "", "", "", "", "", //120-129
                "", "", "", "", "", "", "", "", "", "", //130-139
                "", "", "", "", "", "", "", "", "", "", //140-149
                "", "", "", "", "", "", "", "", "", "", //150-159
                "", "", "", "", "", "", "", "", "", "", //160-169
                "", "", "", "", "", "", "", "", "", "", //170-179
                "", "", "", "", "", "", ":",  "+",  "<",  "_", //180-189
                ">",  "?",  "~",  "", "", "", "", "", "", "", //190-199
                "", "", "", "", "", "", "", "", "", "", //200-209
                "", "", "", "", "", "", "", "", "", "{", //210-219
                "|",  "}", "\"", "", "", "", "", "", "", "", //220-229
                "", "", "", "", "", "", "", "", "", "", //230-239
                "", "", "", "", "", "", "", "", "", "", //240-249
                "", "", "", "", "", ""};                         //250-255
        #endregion

    }
}
