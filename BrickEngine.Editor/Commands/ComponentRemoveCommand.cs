using BrickEngine.Editor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Commands
{
    public struct ComponentRemoveCommand : ICommand
    {
        readonly Dictionary<EcsWorld, EcsLocalEntity[]> worlds;
        readonly Type componentType;
        object[] prevComponents;
        readonly EditorManager editorManager;
        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;
        public ComponentRemoveCommand(EditorManager editorManager, Dictionary<EcsWorld, EcsLocalEntity[]> worlds, Type componentType) : this()
        {
            this.editorManager = editorManager;
            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
            this.worlds = worlds;
            this.componentType = componentType;
            prevComponents = null!;
        }

        public void Execute()
        {
            prevComponents = new object[worlds.Values.Sum(x => x.Length)];
            foreach (var item in worlds)
            {
                var world = item.Key;
                var entities = item.Value;
                var pool = world.GetPoolByType(componentType)!;
                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i].TryUnpack(world, out int entity))
                    {
                        if (pool.Has(entity))
                        {
                            prevComponents[i] = pool.GetRaw(entity);
                            pool.Del(entity, false);
                        }
                    }
                }
            }
            _entityChangedPool.Add(editorManager.MessageId, new SelectedEnititiesChanged());
        }

        public void Undo()
        {
            foreach (var item in worlds)
            {
                var world = item.Key;
                var entities = item.Value;
                var pool = world.GetPoolByType(componentType)!;
                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i].TryUnpack(world, out int entity))
                    {
                        if (prevComponents[i] is not null)
                        {
                            if (pool.Has(entity))
                            {
                                pool.SetRaw(entity, prevComponents[i]);
                            }
                            else
                            {
                                pool.AddRaw(entity, prevComponents[i]);
                            }
                        }
                    }
                }
            }
            _entityChangedPool.Add(editorManager.MessageId, new SelectedEnititiesChanged());
        }
    }
}
