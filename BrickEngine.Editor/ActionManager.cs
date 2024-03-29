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
        public event Action? OnExecute;
        public event Action? OnUndo;
        public event Action? OnRedo;
        private readonly Stack<ICommand> actionStack = new Stack<ICommand>();
        private readonly Stack<ICommand> undoStack = new Stack<ICommand>();

        public void Execute<T>(T command) where T : ICommand
        {
            Console.WriteLine($"Executed command {typeof(T).Name}");
            undoStack.Clear();
            command.Execute();
            actionStack.Push(command);
            OnExecute?.Invoke();
        }

        public bool Undo()
        {
            if (actionStack.Count <= 0) return false;
            var cmd = actionStack.Pop();
            Console.WriteLine($"Undid command {cmd.GetType().Name}");
            cmd.Undo();
            undoStack.Push(cmd);
            OnUndo?.Invoke();
            return true;
        }

        public bool Redo()
        {
            if (undoStack.Count <= 0) return false;
            var cmd = undoStack.Pop();
            Console.WriteLine($"Redid command {cmd.GetType().Name}");
            cmd.Execute();
            actionStack.Push(cmd);
            OnRedo?.Invoke();
            return true;
        }
    }
}
