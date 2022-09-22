using BrickEngine.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BrickEngine.Editor
{
    [JsonSerializable(typeof(EditorConfig))]
    internal class EditorConfig
    {
        public Dictionary<string, bool> WindowStates { get; set; }
        public EditorConfig()
        {
            WindowStates = new Dictionary<string, bool>();
        }
    }
}
