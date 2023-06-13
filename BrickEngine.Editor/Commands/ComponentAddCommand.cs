//using BrickEngine.Editor.Messages;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor.Commands
//{
//    public struct ComponentAddCommand : ICommand
//    {
//        readonly Dictionary<EcsWorld, EcsLocalEntity[]> worlds;
//        readonly Type componentType;
//        readonly EditorManager editorManager;
//        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;

//        public ComponentAddCommand(EditorManager editorManager, Dictionary<EcsWorld, EcsLocalEntity[]> worlds, Type componentType)
//        {
//            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
//            this.editorManager = editorManager;
//            this.worlds = worlds;
//            this.componentType = componentType;
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
//                        if (!pool.Has(entity))
//                        {
//                            pool.AddRaw(entity);
//                        }
//                    }
//                }
//            }
//            _entityChangedPool.Add(editorManager.MessageId, new SelectedEnititiesChanged());
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
//                        pool.Del(entity, false);
//                    }
//                }
//            }
//            _entityChangedPool.Add(editorManager.MessageId, new SelectedEnititiesChanged());
//        }
//    }
//}
