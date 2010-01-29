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

namespace InputHandlers.Mouse
{
    /// <summary>
    /// This class handles mouse events
    /// </summary>
    public sealed class MouseHandler
    {


        private static readonly MouseHandler instance = new MouseHandler();

        public static MouseHandler Instance
        {
            get
            {
                return instance;
            }
        }

        private StateMachine<MouseHandler> mousesm;

        private MouseState oldmouse;
        /// <summary>
        /// XNA mouse state from the previous Poll call 
        /// </summary>
        public MouseState OldMouse { get { return oldmouse; } }
        private MouseState curmouse;
        /// <summary>
        /// XNA mouse state from the current Poll call
        /// </summary>
        public MouseState CurMouse { get { return curmouse; } }
        //this stores the original position of the mouse when a drag event begins.  This is passed as a parameter into the necessary events, but it is publically available for use anyway. 
        private MouseState dragoriginposition;
        public MouseState DragOriginPosition { get { return dragoriginposition; } }

        //holds time of last poll
        private GameTime lastpolltime;
        public GameTime LastPollTime { get { return lastpolltime; } }
        //dragvariance is a fudging factor for detecting the difference between mouse clicks and mouse drags.  This is because a fast user may do a mouse click while slightly moving the mouse between mouse down and mouse up.  If it wasnt for this fudging factor then it would go into drag mode which isnt what the user probably wanted.  Unfortunately this introduces a few complications in the interface.  Most of it should be okay, but check mousemove events if any anomolies.
        private uint dragvariance;
        public uint DragVariance { get { return dragvariance; } set { dragvariance = value; } }
        //if time between two clicks is less than this time (in milliseconds) then a double click event will be sent
        private uint doubleclicktime;
        public uint DoubleClickTime { get { return doubleclicktime; } set { doubleclicktime = value; } }


        //this is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
        private uint updatenumber;
        //this is incremented on each update.  This can be used to determine whether a sequence of events have occurred within the same update time. 
        public uint UpdateNumber { get { return updatenumber; } }

        //exposed delegates - your mouse handler function must make a new delegate object, passing in its corresponding IKBHandlers implementation function as the parameter
        public delegate void DelHandleMouseScrollWheelMove(MouseState m, int diff);
        public delegate void DelHandleMouseMoving(MouseState m); 
        public delegate void DelHandleLeftMouseClick(MouseState m);
        public delegate void DelHandleLeftMouseDoubleClick(MouseState m); 
        public delegate void DelHandleLeftMouseDown(MouseState m);
        public delegate void DelHandleLeftMouseUp(MouseState m); 
        public delegate void DelHandleLeftMouseDragging(MouseState m, MouseState origin);
        public delegate void DelHandleLeftMouseDragDone(MouseState m, MouseState origin);
        public delegate void DelHandleRightMouseClick(MouseState m);
        public delegate void DelHandleRightMouseDoubleClick(MouseState m);
        public delegate void DelHandleRightMouseDown(MouseState m);
        public delegate void DelHandleRightMouseUp(MouseState m);
        public delegate void DelHandleRightMouseDragging(MouseState m, MouseState origin);
        public delegate void DelHandleRightMouseDragDone(MouseState m, MouseState origin);

        //exposed events - the delegate object you created above is to be added to the corresponding event below.  You can make multiple delegates and add them all to the corresponding event.
        public event DelHandleMouseScrollWheelMove HandleMouseScrollWheelMove;
        public event DelHandleMouseMoving HandleMouseMoving;
        public event DelHandleLeftMouseClick HandleLeftMouseClick;
        public event DelHandleLeftMouseDoubleClick HandleLeftMouseDoubleClick;
        public event DelHandleLeftMouseDown HandleLeftMouseDown;
        public event DelHandleLeftMouseUp HandleLeftMouseUp;
        public event DelHandleLeftMouseDragging HandleLeftMouseDragging;
        public event DelHandleLeftMouseDragDone HandleLeftMouseDragDone;
        public event DelHandleRightMouseClick HandleRightMouseClick;
        public event DelHandleRightMouseDoubleClick HandleRightMouseDoubleClick;
        public event DelHandleRightMouseDown HandleRightMouseDown;
        public event DelHandleRightMouseUp HandleRightMouseUp;
        public event DelHandleRightMouseDragging HandleRightMouseDragging;
        public event DelHandleRightMouseDragDone HandleRightMouseDragDone;

        //private functions - the states call these when a mouse event happens.  This in turn calls the event, the event then calls all the delegates that have been added to it
        private void CallHandleMouseScrollWheelMove(MouseState m, int diff) { if (HandleMouseScrollWheelMove != null) HandleMouseScrollWheelMove(m, diff); }
        private void CallHandleMouseMoving(MouseState m) { if (HandleMouseMoving != null) HandleMouseMoving(m); }
        private void CallHandleLeftMouseClick(MouseState m) { if (HandleLeftMouseClick != null) HandleLeftMouseClick(m); }
        private void CallHandleLeftMouseDoubleClick(MouseState m) { if (HandleLeftMouseDoubleClick != null) HandleLeftMouseDoubleClick(m); }
        private void CallHandleLeftMouseDown(MouseState m) { if (HandleLeftMouseDown != null) HandleLeftMouseDown(m); }
        private void CallHandleLeftMouseUp(MouseState m) { if (HandleLeftMouseUp != null) HandleLeftMouseUp(m); }
        private void CallHandleLeftMouseDragging(MouseState m, MouseState origin) { if (HandleLeftMouseDragging != null) HandleLeftMouseDragging(m, origin); }
        private void CallHandleLeftMouseDragDone(MouseState m, MouseState origin) { if (HandleLeftMouseDragDone != null) HandleLeftMouseDragDone(m, origin); }
        private void CallHandleRightMouseClick(MouseState m) { if (HandleRightMouseClick != null) HandleRightMouseClick(m); }
        private void CallHandleRightMouseDoubleClick(MouseState m) { if (HandleRightMouseDoubleClick != null) HandleRightMouseDoubleClick(m); }
        private void CallHandleRightMouseDown(MouseState m) { if (HandleRightMouseDown != null)HandleRightMouseDown(m); }
        private void CallHandleRightMouseUp(MouseState m) { if (HandleRightMouseUp != null)HandleRightMouseUp(m); }
        private void CallHandleRightMouseDragging(MouseState m, MouseState origin) { if (HandleRightMouseDragging != null) HandleRightMouseDragging(m, origin); }
        private void CallHandleRightMouseDragDone(MouseState m, MouseState origin) { if (HandleRightMouseDragDone != null) HandleRightMouseDragDone(m, origin); }

        /// <summary>
        /// create mouse handler object and initialise state to stationary
        /// </summary>
        private MouseHandler()
        {
            updatenumber = 0;
            mousesm = new StateMachine<MouseHandler>(this);
            mousesm.SetCurrentState(MouseStationary.Instance);
            mousesm.SetPreviousState(MouseStationary.Instance);
            dragvariance = 10;
            doubleclicktime = 400;
        }
        /// <summary>
        /// poll the mouse for updates.  This does all the work and will generate mouse events if it detects changes compared to the last time poll was called.
        /// </summary>
        /// <param name="st">a mouse state.  You should use the XNA input function, Mouse.GetState(), as this parameter.</param>
        /// <param name="gt">game time.  The XNA Game.Update function provides a GameTime object; this is what you should pass in.</param>
        public void Poll(MouseState st, GameTime gt)
        {
            updatenumber++; if (updatenumber == uint.MaxValue) updatenumber = 0;
            lastpolltime = gt;
            oldmouse = curmouse;
            curmouse = st;
			
			//process scroll wheel events first
			CheckScrollWheel();		
            //update state machine for the rest of the mouse
			mousesm.Update();
        }

        /// <summary>
        /// reset mouse to stationary state.  You may wish to call this when, for example, switching interface screens 
        /// </summary>
        public void Reset()
        {
            updatenumber = 0;
            mousesm.curstate.Reset(this);
            mousesm.SetCurrentState(MouseStationary.Instance);
            mousesm.SetPreviousState(MouseStationary.Instance);
        }

        /// <summary>
        /// get current state as a string
        /// </summary>
        /// <returns>typeof(currentstate).name</returns>
        public string CurrentStateAsString()
        {
            return mousesm.PrintCurrentState();
        }


        /// <summary>
        /// checks scroll wheel for any changes and sends a scroll wheel event if changes found.  +ve is scroll wheel down.  -ve is scroll wheel up.
        /// </summary>
        private void CheckScrollWheel()
		{
			int diff = curmouse.ScrollWheelValue - oldmouse.ScrollWheelValue;
			
			if (diff != 0)
			{
				HandleMouseScrollWheelMove(curmouse, diff);
			}
		}

        /// <summary>
        /// mouse stationary state
        /// </summary>
        private sealed class MouseStationary : State<MouseHandler>
        {
            private static readonly MouseStationary _instance = new MouseStationary();

            private MouseStationary() { }

            public static MouseStationary Instance
            {
                get
                {
                    return _instance;
                }
            }

            public override void Enter(MouseHandler e)
            {

            }
            public override void Execute(MouseHandler e)
            {
                //change state if mouse performs an action (left/right down or mouse moves), give left preference
                if (e.CurMouse.LeftButton == ButtonState.Pressed)
                {
                    e.mousesm.ChangeState(MouseLeftDown.Instance);
                }
                else if (e.CurMouse.RightButton == ButtonState.Pressed)
                {
                    e.mousesm.ChangeState(MouseRightDown.Instance);
                }
                else if (e.CurMouse.X != e.OldMouse.X || e.CurMouse.Y != e.OldMouse.Y)
                {
                    e.mousesm.ChangeState(MouseMoving.Instance);
                }
            }

            public override void Exit(MouseHandler e)
            {

            }

            public override void Reset(MouseHandler e)
            {

            }

        }


        /// <summary>
        /// mouse moving state
        /// </summary>
        private sealed class MouseMoving : State<MouseHandler>
        {
            private static readonly MouseMoving _instance = new MouseMoving();

            private MouseMoving() { }

            public static MouseMoving Instance
            {
                get
                {
                    return _instance;
                }
            }

            public override void Enter(MouseHandler e)
            {
                e.CallHandleMouseMoving(e.CurMouse);
            }

            public override void Execute(MouseHandler e)
            {
                //give left preference
                if (e.CurMouse.LeftButton == ButtonState.Pressed)
                {
                    e.mousesm.ChangeState(MouseLeftDown.Instance);
                }
                else if (e.CurMouse.RightButton == ButtonState.Pressed)
                {
                    e.mousesm.ChangeState(MouseRightDown.Instance);
                }
                else if (e.CurMouse.X == e.OldMouse.X && e.CurMouse.Y == e.OldMouse.Y)
                {//mouse stopped moving, change to stationary
                    e.mousesm.ChangeState(MouseStationary.Instance);
                }
                else
                {
                    //send a moving event while the mouse still moves
                    e.CallHandleMouseMoving(e.CurMouse);
                }
            }
            public override void Exit(MouseHandler e)
            {

            }

            public override void Reset(MouseHandler e)
            {

            }

        }

        /// <summary>
        /// left mouse down state
        /// </summary>
        private sealed class MouseLeftDown : State<MouseHandler>
        {
            private static readonly MouseLeftDown _instance = new MouseLeftDown();

            //these two variables enable double click detection
            private bool doubleclickwasdoneflag;
            private double detectdoubleclicktime;
            private MouseLeftDown() { detectdoubleclicktime = int.MinValue; doubleclickwasdoneflag = false; }

            public static MouseLeftDown Instance
            {
                get
                {
                    return _instance;
                }
            }
            public override void Enter(MouseHandler e)
            {

                //store the point where the initial click has been made
                e.dragoriginposition = e.CurMouse;
                
                if (detectdoubleclicktime == double.NegativeInfinity)
                {//first mouse down for detection, set double click time
                    detectdoubleclicktime = e.lastpolltime.TotalRealTime.TotalMilliseconds;
                    //send event to interface
                    e.CallHandleLeftMouseDown(e.CurMouse);
                }
                else
                {//double click time has been set, subtract elapsed time since first click

                    detectdoubleclicktime -= e.lastpolltime.TotalRealTime.TotalMilliseconds;

                    if (detectdoubleclicktime >= -e.doubleclicktime)
                    {//double click has happened in the allowed timeframe, do a double click event
                        e.CallHandleLeftMouseDoubleClick(e.dragoriginposition);

                        detectdoubleclicktime = double.NegativeInfinity;
                        
                        //now change back to stationary state
                        //e.mousesm.changestate(mousestationary.Instance);

                        //rather than just change back to a stationary state, keep it consistent and wait until we get a mouse up (see execute)
                        doubleclickwasdoneflag = true;
                    }
                    else
                    {//delay between first mouse click and this 2nd click was too long.  Stay in this state.  However this could be the first click of another double click, so set double click time 
                        detectdoubleclicktime = e.lastpolltime.TotalRealTime.TotalMilliseconds; 
                        //send event to interface
                        e.CallHandleLeftMouseDown(e.CurMouse);
                    }

                }
            }
            public override void Execute(MouseHandler e)
            {
                if (doubleclickwasdoneflag)
                {//a double click was just done on the entry code.  Dont want to send a mouse up or anything when this happens.  Just want to wait for the user to release the mouse button.
                    if (e.CurMouse.LeftButton == ButtonState.Released)
                    {
                        doubleclickwasdoneflag = false;
                        e.mousesm.ChangeState(MouseStationary.Instance);
                    }
                }
                //check the 2 exit conditions - released mouse or moved mouse sufficiently to warrant dragging
                else if (e.CurMouse.LeftButton == ButtonState.Released)
                {
                    //send a mouse up then a mouse click event to the interface, but use old position
                    e.CallHandleLeftMouseUp(e.dragoriginposition);
                    e.CallHandleLeftMouseClick(e.dragoriginposition);
                    //now change back to stationary state
                    e.mousesm.ChangeState(MouseStationary.Instance);
                }
                else if (System.Math.Abs(e.dragoriginposition.X - e.CurMouse.X) > e.dragvariance ||
                         System.Math.Abs(e.dragoriginposition.Y - e.CurMouse.Y) > e.dragvariance)
                {//dont go into drag unless it breaks the fudging factor threshold
                    e.mousesm.ChangeState(MouseLeftDragging.Instance);
                }
            }
            public override void Exit(MouseHandler e)
            {


            }

            public override void Reset(MouseHandler e)
            {
                detectdoubleclicktime = int.MinValue; doubleclickwasdoneflag = false; 
            }
        }

        /// <summary>
        /// left mouse dragging state
        /// </summary>
        private sealed class MouseLeftDragging : State<MouseHandler>
        {
            private static readonly MouseLeftDragging _instance = new MouseLeftDragging();

            private MouseLeftDragging() { }

            public static MouseLeftDragging Instance
            {
                get
                {
                    return _instance;
                }
            }

            public override void Enter(MouseHandler e)
            {
                //send a dragging event
                e.CallHandleLeftMouseDragging(e.CurMouse, e.dragoriginposition);
            }

            public override void Execute(MouseHandler e)
            {
                if (e.CurMouse.LeftButton == ButtonState.Released)
                {
                    //send a mouse up then a mouse drag done event to the interface
                    e.CallHandleLeftMouseUp(e.CurMouse);
                    e.CallHandleLeftMouseDragDone(e.CurMouse,e.dragoriginposition);
                    //now change back to stationary state
                    e.mousesm.ChangeState(MouseStationary.Instance);
                }
                else if (e.CurMouse.X == e.OldMouse.X && e.CurMouse.Y == e.OldMouse.Y)
                {
                    //if stationary while dragging, do nothing

                }
                else
                {
                    //send a dragging event while the mouse still drags
                    e.CallHandleLeftMouseDragging(e.CurMouse,e.dragoriginposition);
                }
            }
            public override void Exit(MouseHandler e)
            {


            }
            public override void Reset(MouseHandler e)
            {

            }
        }


        /// <summary>
        /// right mouse down state
        /// </summary>
        private sealed class MouseRightDown : State<MouseHandler>
        {
            private static readonly MouseRightDown _instance = new MouseRightDown();

            //these two variables enable double click detection
            private bool doubleclickwasdoneflag;
            private double detectdoubleclicktime;
            private MouseRightDown() { detectdoubleclicktime = int.MinValue; doubleclickwasdoneflag = false; }

            public static MouseRightDown Instance
            {
                get
                {
                    return _instance;
                }
            }
            public override void Enter(MouseHandler e)
            {

                //store the point where the initial click has been made
                e.dragoriginposition = e.CurMouse;

                if (detectdoubleclicktime == double.NegativeInfinity)
                {//first mouse down for detection, set double click time
                    detectdoubleclicktime = e.lastpolltime.TotalRealTime.TotalMilliseconds;
                    //send event to interface
                    e.CallHandleRightMouseDown(e.CurMouse);
                }
                else
                {//double click time has been set, subtract elapsed time since first click

                    detectdoubleclicktime -= e.lastpolltime.TotalRealTime.TotalMilliseconds;

                    if (detectdoubleclicktime >= -e.doubleclicktime)
                    {//double click has happened in the allowed timeframe, do a double click event
                        e.CallHandleRightMouseDoubleClick(e.dragoriginposition);

                        detectdoubleclicktime = double.NegativeInfinity;

                        //now change back to stationary state
                        //e.mousesm.changestate(mousestationary.Instance);

                        //rather than just change back to a stationary state, keep it consistent and wait until we get a mouse up (see execute)
                        doubleclickwasdoneflag = true;
                    }
                    else
                    {//delay between first mouse click and this 2nd click was too long.  Stay in this state.  However this could be the first click of another double click, so set double click time 
                        detectdoubleclicktime = e.lastpolltime.TotalRealTime.TotalMilliseconds;
                        //send event to interface
                        e.CallHandleRightMouseDown(e.CurMouse);
                    }

                }
            }
            public override void Execute(MouseHandler e)
            {
                if (doubleclickwasdoneflag)
                {//a double click was just done on the entry code.  Dont want to send a mouse up or anything when this happens.  Just want to wait for the user to release the mouse button.
                    if (e.CurMouse.RightButton == ButtonState.Released)
                    {
                        doubleclickwasdoneflag = false;
                        e.mousesm.ChangeState(MouseStationary.Instance);
                    }
                }
                //check the 2 exit conditions - released mouse or moved mouse sufficiently to warrant dragging
                else if (e.CurMouse.RightButton == ButtonState.Released)
                {
                    //send a mouse up then a mouse click event to the interface, but use old position
                    e.CallHandleRightMouseUp(e.dragoriginposition);
                    e.CallHandleRightMouseClick(e.dragoriginposition);
                    //now change back to stationary state
                    e.mousesm.ChangeState(MouseStationary.Instance);
                }
                else if (System.Math.Abs(e.dragoriginposition.X - e.CurMouse.X) > e.dragvariance ||
                         System.Math.Abs(e.dragoriginposition.Y - e.CurMouse.Y) > e.dragvariance)
                {//dont go into drag unless it breaks the fudging factor threshold
                    e.mousesm.ChangeState(MouseRightDragging.Instance);
                }
            }
            public override void Exit(MouseHandler e)
            {


            }

            public override void Reset(MouseHandler e)
            {
                detectdoubleclicktime = int.MinValue; doubleclickwasdoneflag = false; 
            }
        }

        /// <summary>
        /// right mouse dragging state
        /// </summary>
        private sealed class MouseRightDragging : State<MouseHandler>
        {
            private static readonly MouseRightDragging _instance = new MouseRightDragging();

            private MouseRightDragging() { }

            public static MouseRightDragging Instance
            {
                get
                {
                    return _instance;
                }
            }

            public override void Enter(MouseHandler e)
            {
                //send a dragging event
                e.CallHandleRightMouseDragging(e.CurMouse, e.dragoriginposition);
            }

            public override void Execute(MouseHandler e)
            {
                if (e.CurMouse.RightButton == ButtonState.Released)
                {
                    //send a mouse up then a mouse drag done event to the interface
                    e.CallHandleRightMouseUp(e.CurMouse);
                    e.CallHandleRightMouseDragDone(e.CurMouse, e.dragoriginposition);
                    //now change back to stationary state
                    e.mousesm.ChangeState(MouseStationary.Instance);
                }
                else if (e.CurMouse.X == e.OldMouse.X && e.CurMouse.Y == e.OldMouse.Y)
                {
                    //if stationary while dragging, do nothing

                }
                else
                {
                    //send a dragging event while the mouse still drags
                    e.CallHandleRightMouseDragging(e.CurMouse, e.dragoriginposition);
                }
            }
            public override void Exit(MouseHandler e)
            {


            }
            public override void Reset(MouseHandler e)
            {

            }
        }

    }



    /// <summary>IMouseHandlers provides the functions to implement handlers for mouse events.  
    /// <para>Give this interface to any class that you want to directly or indirectly deal with events.  Write the implementation for each event (for those events you dont want to implement, write a function with does nothing - because this is an interface, you must provide an implementation for all functions).  In the addevents function, add the handlers you want MouseHandler to call to the corresponding MouseHandler event object using the appropriate delegate provided.
    /// </para>
    /// </summary>
    /// <example>
    /// If you wanted a class to handle a left mouse click do the following:
    /// <code>
    /// Class MyClass : IMouseHandlers
    /// {
    ///     MouseHandler mh;
    ///     void HandleLeftMouseClick(MouseState m)
    ///     {
    ///         ...your handling code here...
    ///     }
    ///     
    ///     void addmouseevents()
    ///     {
    ///         //add your handler by adding a new delegate (with your interface function as a parameter) to the event
    ///         mh.HandleLeftMouseClick += new MouseHandler.DelHandleLeftMouseClick(this.HandleLeftMouseClick);
    ///     }
    /// }
    /// </code>
    /// Then whenever the user clicks the left mouse your HandleLeftMouseClick function will be called.
    /// </example>
    public interface IMouseHandlers
    {
        /// <summary>
        /// handle mouse wheel movement
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="diff">direction and magnitude the user moved the wheel since last update.  Positive is down, negative is up.</param>
        void HandleMouseScrollWheelMove(MouseState m, int diff);
        /// <summary>
        /// handle mouse movement when neither left or right mouse buttons are down.  This event is continuously sent every update while the mouse moves.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleMouseMoving(MouseState m);
        /// <summary>
        /// handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseClick(MouseState m);
        /// <summary>
        /// handle left mouse double click.  Unlike a left mouse click, no mouse up event is sent for this action - this is normal as unlike a single click which is processed on mouse up, a double click is processed immediately on the mouse down.  Windows desktop works like this too.  The mouse up done after releasing from double click is suppressed but the mouse state will remain in a mouse down state but will do absolutely nothing until the mouse button is released.  Note, all actions as described in HandleLeftMouseClick WILL be performed for the first mouse click in the double click sequence, so your code may have to consider this if handling both single click and double click events.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseDoubleClick(MouseState m);
        /// <summary>
        /// handle left mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in the same place within the threshold then a mouse up and mouse click event will be sent.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseDown(MouseState m);
        /// <summary>
        /// handle left mouse up.  This event is only called at the end of a single click or dragging is done.  It is not called at the end of a double click.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleLeftMouseUp(MouseState m);
        /// <summary>
        /// handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent every update while the mouse moves.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">state of the mouse when drag was initiated.  This can be used to retrieve the position where the drag was initiated</param>
        void HandleLeftMouseDragging(MouseState m, MouseState origin);
        /// <summary>
        /// handle left mouse drag completion.  A mouse up event is sent just prior to this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">state of the mouse when drag was initiated.  This can be used to retrieve the position where the drag was initiated</param>
        void HandleLeftMouseDragDone(MouseState m, MouseState origin);
        /// <summary>
        /// handle left mouse click.  A mouse up event is sent just prior to this and is followed up by this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseClick(MouseState m);
        /// <summary>
        /// handle right mouse double click.  See left mouse double click description for in depth info.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseDoubleClick(MouseState m);
        /// <summary>
        /// handle right mouse down.  If the user holds down the mouse button and moves the mouse past the threshold for dragging then HandleLeftMouseDragging events will be sent afterwards.  If the user eventually releases the mouse in the same place within the threshold then a mouse up and mouse click event will be sent.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseDown(MouseState m);
        /// <summary>
        /// handle right mouse up.  This event is only called at the end of a single click or dragging is done.  It is not called at the end of a double click.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        void HandleRightMouseUp(MouseState m);
        /// <summary>
        /// handle the situation where the mouse is being held down while the mouse is moving.  This event is continuously sent every update while the mouse moves.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">state of the mouse when drag was initiated.  This can be used to retrieve the position where the drag was initiated</param>
        void HandleRightMouseDragging(MouseState m, MouseState origin);
        /// <summary>
        /// handle right mouse drag completion.  A mouse up event is sent just prior to this event.
        /// </summary>
        /// <param name="m">state of mouse when handler was called</param>
        /// <param name="origin">state of the mouse when drag was initiated.  This can be used to retrieve the position where the drag was initiated</param>
        void HandleRightMouseDragDone(MouseState m, MouseState origin);
        /// <summary>
        /// use this to add your newly created delegates to events in the MouseHandler object.
        /// </summary>
        void AddMouseEvents();
    }
}

/* keep below comments for easy cut and paste into classes
 
        public void HandleMouseScrollWheelMove(MouseState m, int diff) { }
        public void HandleMouseMoving(MouseState m) { }
        public void HandleLeftMouseClick(MouseState m) { }
        public void HandleLeftMouseDoubleClick(MouseState m) { }
        public void HandleLeftMouseDown(MouseState m) { }
        public void HandleLeftMouseUp(MouseState m) { }
        public void HandleLeftMouseDragging(MouseState m, MouseState origin) { }
        public void HandleLeftMouseDragDone(MouseState m, MouseState origin) { }
        public void HandleRightMouseClick(MouseState m) { }
        public void HandleRightMouseDoubleClick(MouseState m) { }
        public void HandleRightMouseDown(MouseState m) { }
        public void HandleRightMouseUp(MouseState m) { }
        public void HandleRightMouseDragging(MouseState m, MouseState origin) { }
        public void HandleRightMouseDragDone(MouseState m, MouseState origin) { }
        public void AddMouseEvents() {}
 
        public void AddMouseEvents()
        {
            mouse.HandleMouseScrollWheelMove += new MouseHandler.DelHandleMouseScrollWheelMove(this.HandleMouseScrollWheelMove);
            mouse.HandleMouseMoving += new MouseHandler.DelHandleMouseMoving(this.HandleMouseMoving);
            mouse.HandleLeftMouseClick += new MouseHandler.DelHandleLeftMouseClick(this.HandleLeftMouseClick);
            mouse.HandleLeftMouseDoubleClick += new MouseHandler.DelHandleLeftMouseDoubleClick(this.HandleLeftMouseDoubleClick);
            mouse.HandleLeftMouseDown += new MouseHandler.DelHandleLeftMouseDown(this.HandleLeftMouseDown);
            mouse.HandleLeftMouseUp += new MouseHandler.DelHandleLeftMouseUp(this.HandleLeftMouseUp);
            mouse.HandleLeftMouseDragging += new MouseHandler.DelHandleLeftMouseDragging(this.HandleLeftMouseDragging);
            mouse.HandleLeftMouseDragDone += new MouseHandler.DelHandleLeftMouseDragDone(this.HandleLeftMouseDragDone);    
        }

*/