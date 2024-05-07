using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugConsole
{
    public class DebugCommandBase
    {

        private string _commandId;
        private string _commandDescription;
        private string _commandFormate;

        public string commandId { get { return _commandId; } }
        public string commandDescription { get { return _commandDescription; } }
        public string commandFormate{ get { return _commandFormate; } }

        public DebugCommandBase(string id, string des, string format)
        {
            _commandId = id;
            _commandDescription = des;
            _commandFormate = format;
        }
    }

    public class DebugCommand : DebugCommandBase
    {
        private Action command;
        public DebugCommand(string id, string des, string format, Action command) : base(id, des, format)
        {
            this.command = command;
        }

        public void Invoke()
        {
            command.Invoke();
        }
    }
    public class DebugCommand<T1> : DebugCommandBase
    {
        private Action<T1> command;
        public DebugCommand(string id, string des, string format, Action<T1> command) : base(id, des, format)
        {
            this.command = command;
        }

        public void Invoke(T1 value)
        {
            command.Invoke(value);
        }
    }
    
}