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
using InputHandlers.Keyboard;
using InputHandlers.State;
using InputHandlers.StateMachine;


namespace InputHandlers.Keyboard
{


    /// <summary>
    /// This class handles keyboard input
    /// </summary>
    public sealed class KBHandler
    {


        private static readonly KBHandler instance = new KBHandler();

        public static KBHandler Instance
        {
            get
            {
                return instance;
            }
        }
        private StateMachine<KBHandler> kbsm;

        private KeyboardState oldkb;

        /// <summary>
        /// XNA keyboard state from the previous Poll call 
        /// </summary>
        public KeyboardState OldKB { get { return oldkb; } }
        private KeyboardState curkb;
        /// <summary>
        /// XNA keyboard state from the current Poll call
        /// </summary>
        public KeyboardState CurKB { get { return curkb; } }

        //any keys in this unmanaged keys list are completely ignored (and thus dont change the state of the state machine).  Keys can be added to this list at any time.
        private List<Keys> unmanaged;
        public List<Keys> Unmanaged { get { return unmanaged; } }

        //holds time of last poll
        private GameTime lastpolltime;
        public GameTime LastPollTime { get { return lastpolltime; } }

        //this is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time.  This is useful for example if your game wants to deal with the whole kbkeydown list itself, so it can react on the first kbkeydown event and ignore the rest by checking the updatenumber.
        private uint updatenumber;
        //this is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time (remember that a KBKeyDown event is sent for every different key detected.  This is useful for example if your game wants to deal with the whole kbkeydown list itself i.e. only one event, so it can react on the first kbkeydown event and ignore the rest by checking the updatenumber).
        public uint UpdateNumber { get { return updatenumber; } }


        //these are common variables used by all the states
        private Keys[] lastlist;
        private KBModifiers lastmodbits;
        private Keys focuskey;
        private List<Keys> newfoundkeys;
        private Keys[] newlist;
        private KBModifiers newmodbits;

        //how long the keyboard handler waits until it moves to a keyboardrepeat state
        private double repeatdelay = 1000.0;
        public double RepeatDelay { get { return repeatdelay; } set { if (value > 0)repeatdelay = value; } }

        //frequency of repeat events when a key is repeating
        private double repeatfrequency = 50.0;
        public double RepeatFrequency { get { return repeatfrequency; } set { if (value > 0) repeatfrequency = value; } }

        //remove modifiers from key list.  When set to true (default), shift, ctrl, alt wont appear in the keylist (get them by probing KBModifiers m parameter).  When set to false, the program treats them as any other key and you'll see them in the keylist and they will also send repeat events when held down.  You will also be able to differentiate between left and right keys by looking at the keylist.
        private bool removemodifiersfromkeylist = true;
        public bool RemoveModifiersFromKeyList { get { return removemodifiersfromkeylist; } set { removemodifiersfromkeylist = value; } }

        //exposed delegates - your keyboard handler function must make a new delegate object, passing in its corresponding IKBHandlers implementation function as the parameter.  
        public delegate void DelHandleKBKeyDown(Keys[] klist, Keys focus, KBModifiers m);
        public delegate void DelHandleKBKeyLost(Keys[] klist, KBModifiers m);
        public delegate void DelHandleKBKeyRepeat(Keys repeatkey, KBModifiers m);
        public delegate void DelHandleKBKeysReleased();

        //exposed events - the delegate object you created above is to be added to the corresponding event below.  You can make multiple delegates and add them all to the corresponding event.  
        public event DelHandleKBKeyDown HandleKBKeyDown;
        public event DelHandleKBKeyLost HandleKBKeyLost;
        public event DelHandleKBKeyRepeat HandleKBKeyRepeat;
        public event DelHandleKBKeysReleased HandleKBKeysReleased;

        //private functions - the states call these when a keyboard event happens.  This in turn calls the event, the event then calls all the delegates that have been added to it


        private void CallHandleKBKeyDown(Keys[] klist, Keys focus, KBModifiers m) { if (HandleKBKeyDown != null) HandleKBKeyDown(klist, focus, m); }
        private void CallHandleKBKeyLost(Keys[] klist, KBModifiers m)
        {
            if (HandleKBKeyLost != null) HandleKBKeyLost(klist, m);
        }
        private void CallHandleKBKeyRepeat(Keys repeatkey, KBModifiers m) { if (HandleKBKeyRepeat != null) HandleKBKeyRepeat(repeatkey, m); }
        private void CallHandleKBKeysReleased() { if (HandleKBKeysReleased != null) HandleKBKeysReleased(); }

        /// <summary>
        /// create keyboard handler object and initialise its state to unpressed
        /// </summary>
        private KBHandler()
        {
            updatenumber = 0;
            newfoundkeys = new List<Keys>(0);
            kbsm = new StateMachine<KBHandler>(this);
            kbsm.SetCurrentState(KBUnpressed.Instance);
            kbsm.SetPreviousState(KBUnpressed.Instance);
            unmanaged = new List<Keys>(0);
        }

        /// <summary>
        /// poll the keyboard for updates.  This does all the work and will generate keyboard events if it detects changes compared to the last time poll was called.
        /// </summary>
        /// <param name="st">a keyboard state.  You should use the XNA input function, Keyboard.GetState(), as this parameter.</param>
        /// <param name="gt">game time.  The XNA Game.Update function provides a GameTime object; this is what you should pass in.</param>
        public void Poll(KeyboardState st, GameTime gt)
        {
            updatenumber++; if (updatenumber == uint.MaxValue) updatenumber = 0;
            lastpolltime = gt;
            oldkb = curkb;
            curkb = st;
            kbsm.Update();
        }

        /// <summary>
        /// reset mouse to stationary state.  You may wish to call this when, for example, switching interface screens.
        /// </summary>
        public void Reset()
        {
            newfoundkeys.Clear();
            updatenumber = 0;
            kbsm.curstate.Reset(this);
            kbsm.SetCurrentState(KBUnpressed.Instance);
            kbsm.SetPreviousState(KBUnpressed.Instance);
        }


        /// <summary>
        /// get current state as a string
        /// </summary>
        /// <returns>typeof(currentstate).name</returns>
        public string CurrentStateAsString()
        {
            return kbsm.PrintCurrentState();
        }


        /// <summary>
        /// strip a keylist of unmanaged keys and ctrl,alt,shifts, returning a new array
        /// </summary>
        /// <param name="klist">key list to strip unmanaged keys from</param>
        /// <param name="k">keyboard handler object</param>
        /// <returns>array with keys stripped (new memory array if unmanaged key list has items or removemodifiersfromkeylist is true)</returns>
        private static void Strip(ref Keys[] klist, KBHandler k)
        {
            //return original array reference if nothing in the unmanaged list
            if (k.unmanaged.Count == 0 && k.removemodifiersfromkeylist == false)
                return;
            int a;
            int b = 0;
            for (a = 0; a < klist.Length; a++)
            {
                if (!((k.removemodifiersfromkeylist == true && KBHelper.IsMod(klist[a])) || k.unmanaged.Contains(klist[a])))
                {//if not modifier and not in unmanaged list then copy the value to next valid part in array, otherwise skip over(a will advance, b will stay the same and then next time the modiffier/unmanaged key will be overwritten by a good key)
                    klist[b] = klist[a];
                    b++;
                }

            }
            if (b < a)
            {
                Keys[] newlist = new Keys[b];
                for (int j = 0; j < b; j++)
                {
                    newlist[j] = klist[j];
                }
                klist = newlist;
            }


        }


        /// <summary>
        /// finds new keys in newl compared to oldl and stores these keys in store.          
        /// </summary>
        /// <param name="oldl"></param>
        /// <param name="newl"></param>
        /// <param name="store"></param>
        /// <returns>true if newl contains a different key compared to newl or if they both have the same keys, false otherwise.</returns>
        private static bool FindNewKeys(Keys[] oldl, Keys[] newl, List<Keys> store)
        {
            //store should be cleared before entering this function
            if (store.Count > 0)
                throw new System.ArgumentException("store must be empty");

            bool found = false;

            if (oldl.Length > newl.Length)
            {//old list is longer, see if keys are different
                foreach (Keys newkey in newl)
                {
                    foreach (Keys oldkey in oldl)
                    {
                        if (oldkey == newkey)
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        //this key is new, store it
                        store.Add(newkey);
                    }
                    found = false;
                }

                if (store.Count == 0)
                    //return false if old list is longer and no new key found, this means we move to keylost state
                    return false;
                return true;

            }
            else
            {//same length or new list is longer
                foreach (Keys newkey in newl)
                {
                    foreach (Keys oldkey in oldl)
                    {
                        if (oldkey == newkey)
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        //this key is new, store it
                        store.Add(newkey);
                    }
                    found = false;
                }

                //return true signifying no keys lost
                return true;
            }


        }


        /// <summary>
        /// unpressed state - keyboard has no keys pressed
        /// </summary>
        private sealed class KBUnpressed : State<KBHandler>
        {
            private static readonly KBUnpressed _instance = new KBUnpressed();

            private KBUnpressed() { }

            public static KBUnpressed Instance
            {
                get
                {
                    return _instance;
                }
            }

            public override void Enter(KBHandler k)
            {
                //send a key released event (assumptions which are true as of this writing: game always starts in unpressed state and this is not called when initialising KBHandler)
                k.CallHandleKBKeysReleased();

            }
            public override void Execute(KBHandler k)
            {
                //get pressed keys list
                Keys[] klist;

                klist = k.CurKB.GetPressedKeys();

                foreach (Keys dwn in klist)
                {//only change state if it is not an unmanaged key being pressed
                    if (k.unmanaged.Count == 0)
                    {
                        k.kbsm.ChangeState(KBKeyDown.Instance);
                        break;
                    }
                    else if (!k.unmanaged.Contains(dwn))
                    {
                        k.kbsm.ChangeState(KBKeyDown.Instance);
                        break;
                    }
                }
            }

            public override void Exit(KBHandler e)
            {

            }

            public override void Reset(KBHandler e)
            {

            }

        }

        /// <summary>
        /// key down state.  A keydown event is sent for EVERY new key found.  If one or more modifier keys only then only one kbkeydown is sent.
        /// </summary>
        private sealed class KBKeyDown : State<KBHandler>
        {
            private static readonly KBKeyDown _instance = new KBKeyDown();

            private KBKeyDown() { }

            public static KBKeyDown Instance
            {
                get
                {
                    return _instance;
                }
            }

            //keeps track of how long keys held down for, will change to a keyrepeat state if held down for longer than the delay specified in the KBHandler object
            private TimeSpan timedelay;

            public override void Enter(KBHandler k)
            {
                timedelay = new TimeSpan();

                if (State<KBUnpressed>.ReferenceEquals(k.kbsm.prevstate, KBUnpressed.Instance))
                {//coming from unpressed, need to do processing
                    //if entering this state always reset time and focus key
                    k.focuskey = Keys.None;

                    //get keylist, strip out ctrl,alt,shift,unmanaged keys then send events
                    k.lastlist = k.CurKB.GetPressedKeys();
                    k.lastmodbits = KBHelper.GetModifiers(k.CurKB);
                    KBHandler.Strip(ref k.lastlist, k);

                    if (k.lastlist.Length == 0)
                    {
                        k.CallHandleKBKeyDown(k.lastlist, Keys.None, k.lastmodbits);
                    }
                    foreach (Keys kevent in k.lastlist)
                    {//send event for each key (focus being different for each key)
                        k.CallHandleKBKeyDown(k.lastlist, kevent, k.lastmodbits);
                        //focus key will always be the last one processed
                        k.focuskey = kevent;
                    }
                }
                else
                {//the other state has done the processing so just need to do the down event
                    foreach (Keys newkey in k.newfoundkeys)
                    {
                        k.CallHandleKBKeyDown(k.newlist, newkey, k.newmodbits);
                        k.focuskey = newkey;
                    }

                }

            }
            public override void Execute(KBHandler k)
            {
                k.newfoundkeys.Clear();
                k.newlist = k.CurKB.GetPressedKeys();

                if (k.newlist.Length == 0)
                {//nothing pressed, change state back to unpressed
                    k.kbsm.ChangeState(KBUnpressed.Instance);
                    return;
                }

                k.newmodbits = KBHelper.GetModifiers(k.CurKB);
                //strip modifiers and unmanaged keys  
                KBHandler.Strip(ref k.newlist, k);

                //see if list is same
                if (!KBHandler.FindNewKeys(k.lastlist, k.newlist, k.newfoundkeys))
                {
                    //a key was released but keys are still down
                    k.kbsm.ChangeState(KBKeyLost.Instance);
                }
                else
                {
                    if (k.newfoundkeys.Count == 0)
                    {
                        //no keys changed, need to compare modifiers now to see if any events
                        if (k.newmodbits != k.lastmodbits)
                        {//something's changed
                            KBModifiers testmodbits;
                            testmodbits = k.newmodbits & k.lastmodbits;

                            //if we remain in this state in this portion of code, focus key will be none
                            k.focuskey = Keys.None;
                            //since a change has happened make a new timespan
                            timedelay = new TimeSpan();

                            if (testmodbits == KBModifiers.None)
                                //changed completely, send a keyboard down
                                k.CallHandleKBKeyDown(k.newlist, Keys.None, k.newmodbits);
                            else if ((testmodbits & k.newmodbits) == (testmodbits & k.lastmodbits))
                                //had one key the same but other two were different, send keyboard down
                                k.CallHandleKBKeyDown(k.newlist, Keys.None, k.newmodbits);
                            else if ((testmodbits & k.newmodbits) == testmodbits)
                                //new mod bits only had 1, which means it lost one, change to lost state
                                k.kbsm.ChangeState(KBKeyLost.Instance);
                            else if ((testmodbits & k.lastmodbits) == testmodbits)
                                //old mod bits is less, send down event
                                k.CallHandleKBKeyDown(k.newlist, Keys.None, k.newmodbits);
                            else
                                throw new System.Exception("code error, unhandled mod key state");
                        }
                        else
                        {
                            if (k.focuskey != Keys.None)
                            {//nothing at all changed and the focus key is a real key, increase time for repeat purposes
                                timedelay += k.lastpolltime.ElapsedRealTime;

                                if (timedelay.TotalMilliseconds > k.repeatdelay)
                                {
                                    k.kbsm.ChangeState(KBKeyRepeat.Instance);
                                }
                            }


                        }

                    }
                    else
                    {//new keys were found, send a down event for each key
                        foreach (Keys newkey in k.newfoundkeys)
                        {
                            k.CallHandleKBKeyDown(k.newlist, newkey, k.newmodbits);
                            k.focuskey = newkey;
                            timedelay = new TimeSpan();
                        }
                    }
                }

                //update last list and mod bits
                k.lastlist = k.newlist;
                k.lastmodbits = k.newmodbits;
            }



            public override void Exit(KBHandler k)
            {
                //update last list and mod bits so the new state knows where we are at
                k.lastlist = k.newlist;
                k.lastmodbits = k.newmodbits;
            }

            public override void Reset(KBHandler e)
            {

            }

        }


        /// <summary>
        /// key lost state, this happens when one or more keys are released but keys are still being held down.  Only one kbkeylost event is sent regardless of how many keys were lost.
        /// note - sometimes more than 2 keys wont register.  See this for explanation of keyboard hardware limitations: http://blogs.msdn.com/shawnhar/archive/2007/03/28/keyboards-suck.aspx
        /// 2nd note - GetPressedKeys has a few other issues too - for example, holding down shift and pressing numpad9 or numpad3 will register a pageup/pagedown key in XNA, then on releasing the shift key and then releasing the numpad key will cause th
        /// </summary>
        private sealed class KBKeyLost : State<KBHandler>
        {
            private static readonly KBKeyLost _instance = new KBKeyLost();

            private KBKeyLost() { }

            public static KBKeyLost Instance
            {
                get
                {
                    return _instance;
                }
            }

            public override void Enter(KBHandler k)
            {
                //send key lost 
                k.CallHandleKBKeyLost(k.newlist, k.newmodbits);

            }
            public override void Execute(KBHandler k)
            {
                k.newfoundkeys.Clear();
                k.newlist = k.CurKB.GetPressedKeys();

                if (k.newlist.Length == 0)
                {//nothing pressed, change state back to unpressed
                    k.kbsm.ChangeState(KBUnpressed.Instance);
                    return;
                }

                k.newmodbits = KBHelper.GetModifiers(k.CurKB);
                KBHandler.Strip(ref k.newlist, k);

                //see if list is same
                if (!KBHandler.FindNewKeys(k.lastlist, k.newlist, k.newfoundkeys))
                {
                    //a key was released but keys are still down
                    //send key lost 
                    k.CallHandleKBKeyLost(k.newlist, k.newmodbits);
                }
                else
                {
                    if (k.newfoundkeys.Count == 0)
                    {
                        //no keys changed, need to compare modifiers now to see if any events
                        if (k.newmodbits != k.lastmodbits)
                        {//something's changed
                            KBModifiers testmodbits;
                            testmodbits = k.newmodbits & k.lastmodbits;

                            //if we remain in this state in this portion of code, focus key will be none
                            k.focuskey = Keys.None;

                            if (testmodbits == KBModifiers.None)
                                //changed completely, send a keyboard down
                                k.kbsm.ChangeState(KBKeyDown.Instance);
                            else if ((testmodbits & k.newmodbits) == (testmodbits & k.lastmodbits))
                                //had one key the same but other two were different, send keyboard down
                                k.kbsm.ChangeState(KBKeyDown.Instance);
                            else if ((testmodbits & k.newmodbits) == testmodbits)
                                //new mod bits only had 1, which means it lost one,
                                //send key lost 
                                k.CallHandleKBKeyLost(k.newlist, k.newmodbits);
                            else if ((testmodbits & k.lastmodbits) == testmodbits)
                                //old mod bits is less, send down event
                                k.kbsm.ChangeState(KBKeyDown.Instance);
                            else
                                throw new System.Exception("code error, unhandled mod key state");
                        }
                        else
                        {
                            //nothing at all changed, do nothing and stay in this state

                        }

                    }
                    else
                    {//new keys were found, send a down event for each key
                        k.kbsm.ChangeState(KBKeyDown.Instance);

                    }
                }

                //update last list and mod bits
                k.lastlist = k.newlist;
                k.lastmodbits = k.newmodbits;
            }

            public override void Exit(KBHandler k)
            {
                //update last list and mod bits so the new state knows where we are at
                k.lastlist = k.newlist;
                k.lastmodbits = k.newmodbits;
            }

            public override void Reset(KBHandler e)
            {

            }

        }


        /// <summary>
        /// key repeat state, this is entered when a key is held down for long enough and nothing else occurs.  A key repeat event happens every single time a poll occurs if the repeat delay time has been exceeded.
        /// </summary>
        private sealed class KBKeyRepeat : State<KBHandler>
        {
            private static readonly KBKeyRepeat _instance = new KBKeyRepeat();

            private KBKeyRepeat() { }

            public static KBKeyRepeat Instance
            {
                get
                {
                    return _instance;
                }
            }
            private double repeatrunning = -1.0;

            public override void Enter(KBHandler k)
            {
                repeatrunning = -1.0;
                //repeat time has been exceeded in kbkeydown, but since it spams kbkeyrepeat each update theres no need to hurry, can wait until next update before doing anything
            }
            public override void Execute(KBHandler k)
            {
                k.newfoundkeys.Clear();
                k.newlist = k.CurKB.GetPressedKeys();


                if (k.newlist.Length == 0)
                {//nothing pressed, change state back to unpressed
                    k.kbsm.ChangeState(KBUnpressed.Instance);
                    return;
                }

                k.newmodbits = KBHelper.GetModifiers(k.CurKB);
                KBHandler.Strip(ref k.newlist, k);

                //see if list is same
                if (!KBHandler.FindNewKeys(k.lastlist, k.newlist, k.newfoundkeys))
                {
                    //a key was released but keys are still down
                    k.kbsm.ChangeState(KBKeyLost.Instance);
                }
                else
                {
                    if (k.newfoundkeys.Count == 0)
                    {
                        //no keys changed, need to compare modifiers now to see if any events
                        if (k.newmodbits != k.lastmodbits)
                        {//something's changed
                            KBModifiers testmodbits;
                            testmodbits = k.newmodbits & k.lastmodbits;

                            //if we remain in this state in this portion of code, focus key will be none
                            k.focuskey = Keys.None;

                            if (testmodbits == KBModifiers.None)
                                //changed completely, send a keyboard down
                                k.kbsm.ChangeState(KBKeyDown.Instance);
                            else if ((testmodbits & k.newmodbits) == (testmodbits & k.lastmodbits))
                                //had one key the same but other two were different, send keyboard down
                                k.kbsm.ChangeState(KBKeyDown.Instance);
                            else if ((testmodbits & k.newmodbits) == testmodbits)
                                //new mod bits only had 1, which means it lost one, change to lost state
                                k.kbsm.ChangeState(KBKeyLost.Instance);
                            else if ((testmodbits & k.lastmodbits) == testmodbits)
                                //old mod bits is less, send down event
                                k.kbsm.ChangeState(KBKeyDown.Instance);
                            else
                                throw new System.Exception("code error, unhandled mod key state");
                        }
                        else
                        {
                            //nothing at all changed, send a key repeat event (note - repeat time is detected in kbkeydown, so no need to keep track of time)
                            repeatrunning -= k.lastpolltime.ElapsedRealTime.TotalMilliseconds;
                            if (repeatrunning < 0)
                            {
                                k.CallHandleKBKeyRepeat(k.focuskey, k.lastmodbits);
                                repeatrunning = k.repeatfrequency;
                            }

                        }

                    }
                    else
                    {//new keys were found, change to keyboard down
                        k.kbsm.ChangeState(KBKeyDown.Instance);

                    }
                }

                //update last list and mod bits
                k.lastlist = k.newlist;
                k.lastmodbits = k.newmodbits;
            }


            public override void Exit(KBHandler k)
            {
                //update last list and mod bits so the new state knows where we are at
                k.lastlist = k.newlist;
                k.lastmodbits = k.newmodbits;
            }

            public override void Reset(KBHandler e)
            {

            }

        }

    }


    /// <summary>IKBHandlers provides the functions to implement handlers for mouse events.  
    /// <para>Give this interface to any class that you want to directly or indirectly deal with events.  Write the implementation for each event (for those events you dont want to implement, write a function with does nothing - because this is an interface, you must provide an implementation for all functions).  In the addevents function, add the handlers you want KBHandler to call to the corresponding KBHandler event object using the appropriate delegate provided.
    /// </para>
    /// </summary>
    /// <example>
    /// If you wanted a class to handle a kbdown event do the following:
    /// <code>
    /// Class MyClass : IKBHandlers
    /// {
    ///     KBHandler kh;
    ///     HandleKBKeyDown(Keys[] klist, Keys focus, KBModifiers m) 
    ///     {
    ///         ...your handling code here...
    ///     }
    ///     
    ///     void AddKBEvents()
    ///     {
    ///         //add your handler by adding a new delegate (with your interface function as a parameter) to the event
    ///         kh.HandleKBKeyDown += new KBHandler.DelHandleKBKeyDown(this.HandleKBKeyDown);
    ///     }
    /// }
    /// </code>
    /// Then whenever the user presses a keyboard key your HandleKBKeyDown function will be called.
    /// </example>
    public interface IKBHandlers
    {
        /// <summary>
        /// handle a key down event.  A unique call is made to this event every single time an individual key is pressed.  The 'focus' key of each call is retrievable through the focus parameter.
        /// </summary>
        /// <param name="klist">The entire set of keys currently held down on the keyboard (please note that number of keys detectable is subject to hardware limitations on your keyboard)</param>
        /// <param name="focus">The 'focus' key of this event.</param>
        /// <param name="m">a bit field holding control, alt and shift key status</param>
        void HandleKBKeyDown(Keys[] klist, Keys focus, KBModifiers m);
        /// <summary>
        /// handle a key lost event.  This occurs when multiple keys are held down and then a key is released but soome keys are still being held.
        /// </summary>
        /// <param name="klist">The entire set of keys currently held down on the keyboard (please note that number of keys detectable is subject to hardware limitations on your keyboard)</param>
        /// <param name="m">a bit field holding control, alt and shift key status</param>
        void HandleKBKeyLost(Keys[] klist, KBModifiers m);
        /// <summary>
        /// handle a key repeat event.  Once the repeat delay threshold is exceeded when the same key(s) are held down for long enough then the program will start sending key repeat events on every update.
        /// </summary>
        /// <param name="repeatkey">the key that is to be repeated.  This is always the last key held down.</param>
        /// <param name="m">a bit field holding control, alt and shift key status</param>
        void HandleKBKeyRepeat(Keys repeatkey, KBModifiers m);
        /// <summary>
        /// handle the situation where all keys have been released.
        /// </summary>
        void HandleKBKeysReleased();
        /// <summary>
        /// use this to add your newly created delegates to events in the KBHandler object.
        /// </summary>
        void AddKBEvents();
    }

}


/* keep below comments for easy cut and paste into classes
 
        public void HandleKBKeyDown(Keys[] klist, Keys focus, KBModifiers m) { }
        public void HandleKBKeyLost(Keys[] klist, KBModifiers m) { }
        public void HandleKBKeyRepeat(Keys repeatkey, KBModifiers m) { }
        public void HandleKBKeysReleased() { }
        public void AddKBEvents() { }
 
        public void AddKBEvents() 
        {
            kb.HandleKBKeyDown += new KBHandler.DelHandleKBKeyDown(HandleKBKeyDown);
            kb.HandleKBKeyLost += new KBHandler.DelHandleKBKeyLost(HandleKBKeyLost);
            kb.HandleKBKeyRepeat += new KBHandler.DelHandleKBKeyRepeat(HandleKBKeyRepeat);
            kb.HandleKBKeysReleased += new KBHandler.DelHandleKBKeysReleased(HandleKBKeysReleased);
        }

*/