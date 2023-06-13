//using BrickEngine.Editor.Messages;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BrickEngine.Editor.Commands
//{
//    public struct SelectEntityCommand : ICommand
//    {
//        readonly bool additive;
//        readonly EcsEntity[] entities;
//        readonly EditorManager editorManager;
//        readonly MessagePool<SelectedEnititiesChanged> _entityChangedPool;
//        (EcsWorld, EcsLocalEntity[])[] prevSelectedEntities;
//        public SelectEntityCommand(bool additive, EditorManager editorManager, params EcsEntity[] entities)
//        {
//            _entityChangedPool = editorManager.EditorMsgBus.GetPool<SelectedEnititiesChanged>();
//            this.additive = additive;
//            this.editorManager = editorManager;
//            this.entities = entities;
//            prevSelectedEntities = null!;
//        }

//        public void Execute()
//        {
//            var selected = editorManager.SelectedEntites;
//            prevSelectedEntities = new (EcsWorld, EcsLocalEntity[])[selected.Count];
//            int idx = 0;
//            foreach (var item in selected)
//            {
//                prevSelectedEntities[idx++] = (item.Key, item.Value.ToArray());
//            }
//            if (!additive)
//            {
//                editorManager.ClearSelectedEntities();
//            }
//            for (int i = 0; i < entities.Length; i++)
//            {
//                if (entities[i].TryUnpack(out var world, out int entityID))
//                {
//                    var entity = world.PackLocalEntity(entityID);
//                    if (additive)
//                    {
//                        editorManager.AddSelectedEntity(world, entity);
//                    }
//                    else
//                    {
//                        if (i == 0)
//                        {
//                            editorManager.SetSelectedEntity(world, entity);
//                        }
//                        else
//                        {
//                            editorManager.AddSelectedEntity(world, entity);
//                        }
//                    }
//                }
//            }
//            _entityChangedPool.Add(editorManager.MessageId, new SelectedEnititiesChanged());
//        }

//        public void Undo()
//        {
//            editorManager.ClearSelectedEntities();
//            foreach (var item in prevSelectedEntities)
//            {
//                var world = item.Item1;
//                var entities = item.Item2;
//                for (int i = 0; i < entities.Length; i++)
//                {
//                    if (entities[i].TryUnpack(world, out _))
//                    {
//                        editorManager.AddSelectedEntity(world, entities[i]);
//                    }
//                }
//            }
//            _entityChangedPool.Add(editorManager.MessageId, new SelectedEnititiesChanged());
//        }
//    }
//}
