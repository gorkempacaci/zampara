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

using InputHandlers.State;

namespace InputHandlers.StateMachine
{

    //reusable state machine and state classes
    //based on code from http://www.wordware.com/files/ai/
    //"Programming AI By Example", Mat Buckland, 2005

    public class StateMachine<EntityType>
    {
        private EntityType owner;
        private State<EntityType> _curstate;
        private State<EntityType> _prevstate;
        private State<EntityType> _globstate;
        public State<EntityType> curstate { get { return _curstate; } }
        public State<EntityType> prevstate { get { return _prevstate; } }
        public State<EntityType> globstate { get { return _globstate; } }

        public StateMachine(EntityType own)
        {
            owner = own;
            _curstate = null;
            _prevstate = null;
            _globstate = null;

        }
        public void SetCurrentState(State<EntityType> cur)
        {
            _curstate = cur;
        }

        public void SetPreviousState(State<EntityType> prev)
        {
            _prevstate = prev;
        }
        public void SetGlobalState(State<EntityType> glob)
        {
            _globstate = glob;
        }

        public void Update()
        {
            if (_globstate != null)
                _globstate.Execute(owner);

            if (_curstate != null)
                _curstate.Execute(owner);
        }

        public void ChangeState(State<EntityType> newstate)
        {
            _prevstate = _curstate;
            _curstate.Exit(owner);
            _curstate = newstate;
            _curstate.Enter(owner);
        }

        public void RevertToPreviousState()
        {
            ChangeState(_prevstate);
        }

        public bool IsInState(State<EntityType> st)
        {
            if (st.GetType() == _curstate.GetType())
                return true;
            return false;

        }

        public string PrintCurrentState()
        {
            return _curstate.GetType().Name.ToString();
        }

    }
}
