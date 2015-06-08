using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace physical {
    public class Model : Renderable {
        public const int 
            VERTEX_COUNT = 3, // because we use triangles
            V_DIMS = 3, VN_DIMS = 3, VT_DIMS = 2, // for reference for other implementing classes
            VS_COUNT = 3;
        // v, vn, vt; 3 total.
        public const float PI = (float) Math.PI, TWO_PI = 2 * PI;
        protected static readonly String[,] OBJ_section_name = {
            { "vertices", "v" },
            { "normals", "vn" },
            { "UVs", "vt" }
        };
        Matrix4 transform = new Matrix4();

        Matrix4 Transform { get { return transform; } }

        int
            vaoHandle,
            positionVboHandle,
            normalVboHandle,
            uvsVboHandle,
            eboHandle;

        protected readonly ModelData modelData;

        public ModelData ModelData{ get { return modelData; } }

        public Model ( ModelData modelData ) {
            this.modelData = modelData;
        }

        public Model ( float[] vertices, float[] normals, float[] uvs, int[] indices ) : this( new ModelData( vertices, normals, uvs, indices ) ) {
        }

        public void render () {
            GL.BindVertexArray( vaoHandle );

            GL.DrawElements( PrimitiveType.Triangles, modelData.Indices.Length,
                DrawElementsType.UnsignedInt, IntPtr.Zero );
        }

        int GenBuffer<T> ( BufferTarget bufferTarget, T[] data ) where T:struct {
            int handle = GL.GenBuffer();
            GL.BindBuffer( bufferTarget, handle );
            GL.BufferData<T>( bufferTarget, new IntPtr( data.Length * Marshal.SizeOf( data[0] ) ), data, BufferUsageHint.StaticDraw );
            return handle;
        }

        void EnableAttribute ( int attribNr, int handle, int size ) {
            GL.EnableVertexAttribArray( attribNr );
            GL.BindBuffer( BufferTarget.ArrayBuffer, handle );
            GL.VertexAttribPointer( attribNr, size, VertexAttribPointerType.Float, true, size * sizeof(float), 0 );
        }

        public void init () {
            positionVboHandle = GenBuffer( BufferTarget.ArrayBuffer, ModelData.Vertices );
            normalVboHandle = GenBuffer( BufferTarget.ArrayBuffer, ModelData.Normals );
            uvsVboHandle = GenBuffer( BufferTarget.ArrayBuffer, ModelData.Uvs );
            eboHandle = GenBuffer( BufferTarget.ElementArrayBuffer, ModelData.Indices );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray( vaoHandle );
            EnableAttribute( 0, positionVboHandle, Model.V_DIMS );
            EnableAttribute( 1, normalVboHandle, Model.VN_DIMS );
            EnableAttribute( 2, uvsVboHandle, Model.VT_DIMS );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, eboHandle );
            GL.BindVertexArray( 0 );
        }

        protected void writeOBJ_buf ( StreamWriter sw, int bufNr ) {
            var fb = new FloatBuffer( modelData.Data[bufNr] );
            sw.Write( "#\n# " + OBJ_section_name[bufNr, 0] + "\n#\n\n" );
            String prefix = OBJ_section_name[bufNr, 1] + " ";
            if ( bufNr != 2 )
                for ( int j = 0; j < modelData.VertexCount; j++ )
                    sw.Write( prefix + fb.get() + " " + fb.get() + " " + fb.get() + "\n" );
            else
                for ( int j = 0; j < modelData.VertexCount; j++ )
                    sw.Write( prefix + fb.get() + " " + fb.get() + "\n" );
            sw.Write( "\n" );
        }

        public void writeOBJ () {
            writeOBJ( "" + GetType() );
        }

        public void writeOBJ ( String filename ) {
            using ( StreamWriter sw = new StreamWriter( filename + ".obj" ) ) {
                sw.Write( "# created by " + main.APP_NAME + "\n" );
                int tri_cnt = modelData.VertexCount / VERTEX_COUNT;
                sw.Write( "# " + modelData.VertexCount + " vertex total == normals == UVs\n"
                + "# " + tri_cnt + " tris == faces\n\n"
                + "mtllib " + filename + ".mtl\nusemtl " + filename + "\n\n"
                + "#\n# " + GetType() + "\n#\n\n" );
                writeOBJ_buf( sw, 0 );
                writeOBJ_buf( sw, 2 );
                writeOBJ_buf( sw, 1 );
                sw.Write( "#\n# faces\n#\n" );
                for ( int i = 0, j = 1, k = 2, l = 3; i < tri_cnt; i++, k += 3, j += 3, l += 3 ) {
                    sw.Write( "f " + j + "/" + j + "/" + j + " " + k + "/" + k + "/" + k + " " + l + "/" + l + "/" + l + "\n" );
                }
                sw.Write( "\n# EOF\n" );
            }
        }
    }
}

