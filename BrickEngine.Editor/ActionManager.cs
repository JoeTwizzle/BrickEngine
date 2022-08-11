﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
    public class ActionManager
    {
        private readonly Stack<ICommand> actionStack = new Stack<ICommand>();
        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();

        public void Execute<T>(T command) where T : ICommand
        {
            undoStack.Clear();
            command.Execute();
            actionStack.Push(command);
        }

        public bool Undo()
        {
            if (actionStack.Count <= 0) return false;
            var cmd = actionStack.Pop();
            cmd.Undo();
            undoStack.Push(cmd);
            return true;
        }

        public bool Redo()
        {
            if (undoStack.Count <= 0) return false;
            var cmd = undoStack.Pop();
            cmd.Execute();
            actionStack.Push(cmd);
            return true;
        }
    }
}
