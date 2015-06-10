
using physical.util;

namespace physical.model {
    public class MeshData {
        int[] indices;
        float[] vertices, normals, uvs;
        float[][] data;
        int vertexCount;

        public float[] Vertices{ get { return vertices; } }

        public float[] Normals{ get { return normals; } }

        public float[] Uvs { get { return uvs; } }

        public float[][] Data { get { return data; } }

        public int[] Indices { get { return indices; } }

        public int VertexCount { get { return vertexCount; } }

        protected static int[] createDefaultIndices ( int vertexCount ) {
            int[] indices = new int[vertexCount];
            for ( int i = 0; i < vertexCount; i++ )
                indices[i] = i;
            return indices;
        }

        public MeshData ( Buffer<float> vertices, Buffer<float> normals, Buffer<float> uvs )
            : this( vertices.Array, normals.Array, uvs.Array, createDefaultIndices( vertices.Size / Mesh.V_DIMS ) ) {
        }

        public MeshData ( Buffer<float> vertices, Buffer<float> normals, Buffer<float> uvs, Buffer<int> indices )
            : this( vertices.Array, normals.Array, uvs.Array, indices.Array ) {
        }

        public MeshData ( float[] vertices, float[] normals, float[] uvs )
            : this( vertices, normals, uvs, createDefaultIndices( vertices.Length / Mesh.V_DIMS ) ) {
        }


        public MeshData ( float[] vertices, float[] normals, float[] uvs, int[] indices ) {
            vertexCount = indices.Length;
            this.vertices = vertices;
            this.normals = normals;
            this.uvs = uvs;
            data = new float[][]{ vertices, normals, uvs };

            this.indices = indices;
        }
    }
}

