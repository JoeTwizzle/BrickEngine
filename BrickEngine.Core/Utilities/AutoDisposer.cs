using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Utilities
{
    //I have no idea how this works anymore...
    public class AutoDisposer
    {
        readonly List<Fence> freeFences = new List<Fence>();
        readonly List<Fence> usedFences = new List<Fence>();
        readonly List<int> fenceGroups = new List<int>();
        readonly List<int> disposeGroups = new List<int>();
        readonly List<IDisposable> disposables = new List<IDisposable>();
        int fencesThisFrame = 0;
        int disposesThisFrame = 0;
        public Fence? GetPooledFence()
        {
            for (int i = 0; i < freeFences.Count; i++)
            {
                var freeFence = freeFences[i];
                if (!freeFence.Signaled) //sanity check
                {
                    freeFences.RemoveAt(i); //No longer free remove
                    return freeFence;
                }
            }
            return null;
        }

        public void EndFrame()
        {
            DisposeItems();
            fenceGroups.Add(fencesThisFrame);
            fencesThisFrame = 0;
            disposeGroups.Add(disposesThisFrame);
            disposesThisFrame = 0;
        }

        public void AddFence(Fence fence)
        {
            //fencesThisFrame++;
            //usedFences.Add(fence);
        }

        public void DisposeWhenUnused(IDisposable disposable)
        {
            //disposesThisFrame++;
            //disposables.Add(disposable);
        }

        void DisposeItems()
        {
            int startingFence = usedFences.Count - 1;
            int startingDisposeable = disposables.Count - 1;
            for (int i = fenceGroups.Count - 1; i >= 0; i--)
            {
                int groupFenceCount = fenceGroups[i];
                int groupDisposableCount = disposeGroups[i];

                bool allSignaled = true;

                for (int j = 0; j < groupFenceCount; j++)
                {
                    allSignaled &= usedFences[startingFence - j].Signaled; //Set false if not signaled
                }
                if (groupFenceCount == 0)
                {
                    allSignaled = false;
                }
                if (allSignaled)
                {
                    for (int j = 0; j < groupFenceCount; j++)
                    {
                        //Reset for reuse
                        var fence = usedFences[startingFence - j];
                        fence.Reset();
                        freeFences.Add(fence);
                        usedFences.RemoveAt(startingFence - j);//take from back
                        fenceGroups.RemoveAt(i);
                    }
                    disposeGroups.RemoveAt(i);
                    for (int j = 0; j < groupDisposableCount; j++)
                    {
                        Debug.WriteLine("Disposed resource of type: " + disposables[startingDisposeable - j].GetType().Name);
                        disposables[startingDisposeable - j].Dispose();
                        disposables.RemoveAt(startingDisposeable - j);
                    }
                }
                startingFence -= groupFenceCount;
                startingDisposeable -= groupDisposableCount;
            }
        }
    }
}
