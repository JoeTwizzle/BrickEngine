namespace BrickEngine.Core.Graphics.Data
{
    public struct StaticVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Tangent;
        public Vector4 UVScale;

        public StaticVertex(Vector3 pos, Vector3 normal, Vector4 tangent, Vector4 uvScale)
        {
            this.Position = pos;
            this.Normal = normal;
            this.Tangent = tangent;
            this.UVScale = uvScale;
        }
    }
}