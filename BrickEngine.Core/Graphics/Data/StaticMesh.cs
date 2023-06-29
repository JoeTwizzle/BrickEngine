namespace BrickEngine.Core.Graphics.Data
{
    public sealed class StaticMesh
    {
        public readonly StaticVertex[] Vertices;
        public readonly uint[] Indices;

        public StaticMesh(StaticVertex[] vertices, uint[] uints)
        {
            this.Vertices = vertices;
            this.Indices = uints;
        }
    }
}