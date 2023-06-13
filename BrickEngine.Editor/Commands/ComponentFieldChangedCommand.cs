//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor.Commands
//{
//    public struct ComponentFieldChangedCommand : ICommand
//    {
//        readonly Dictionary<EcsWorld, EcsLocalEntity[]> worlds;
//        readonly Type componentType;
//        readonly FieldInfo fieldInfo;
//        readonly object[] prevfieldVals;
//        readonly object newFieldVal;

//        public ComponentFieldChangedCommand(Dictionary<EcsWorld, EcsLocalEntity[]> worlds, Type componentType, FieldInfo fieldInfo, object[] prevfieldVals, object newFieldVal)
//        {
//            this.worlds = worlds;
//            this.componentType = componentType;
//            this.fieldInfo = fieldInfo;
//            this.prevfieldVals = prevfieldVals;
//            this.newFieldVal = newFieldVal;
//        }

//        public void Execute()
//        {
//            foreach (var item in worlds)
//            {
//                var world = item.Key;
//                var entities = item.Value;
//                var pool = world.GetPoolByType(componentType)!;
//                for (int i = 0; i < entities.Length; i++)
//                {
//                    if (entities[i].TryUnpack(world, out int entity))
//                    {
//                        var obj = pool.GetRaw(entity);
//                        fieldInfo.SetValue(obj, newFieldVal);
//                        pool.SetRaw(entity, obj);
//                    }
//                }
//            }
//        }

//        public void Undo()
//        {
//            foreach (var item in worlds)
//            {
//                var world = item.Key;
//                var entities = item.Value;
//                var pool = world.GetPoolByType(componentType)!;
//                for (int i = 0; i < entities.Length; i++)
//                {
//                    if (entities[i].TryUnpack(world, out int entity))
//                    {
//                        var obj = pool.GetRaw(entity);
//                        fieldInfo.SetValue(obj, prevfieldVals[i]);
//                        pool.SetRaw(entity, obj);
//                    }
//                }
//            }
//        }
//    }
//}
