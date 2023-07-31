using BinSerialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets.Data
{
    [MemoryPackable(GenerateType.CircularReference)]
    public sealed partial class ModelData
    {
        [MemoryPackOrder(0)]
        public MeshData[] Meshes;
        [MemoryPackOrder(1)]
        public MaterialData[] Materials;
        [MemoryPackConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ModelData()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
                
        }
        public ModelData(MeshData[] meshes, MaterialData[] materials)
        {
            Meshes = meshes;
            Materials = materials;
        }
    }
    [MemoryPackable(GenerateType.VersionTolerant)]
    public sealed partial class ModelDataNew
    {
        [MemoryPackOrder(0)]
        public int[] Meshes;
        [MemoryPackOrder(1)]
        public int[] Materials;
        [MemoryPackConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ModelDataNew()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }
        public ModelDataNew(int[] meshes, int[] materials)
        {
            Meshes = meshes;
            Materials = materials;
        }
    }
}
