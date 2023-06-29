using BrickEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine
{
    public interface IFeatureLayer
    {
        bool IsEnabled { get; set; }

        void OnLoad(GameWindow gameWindow);

        void Update();

        virtual void Display() { }

        void OnUnload();
    }
}
