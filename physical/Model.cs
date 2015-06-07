using System;
using System.IO;

namespace physical {
    public class Model : Renderable {
        public const int 
            VERTEX_COUNT = 3, // because we use triangles
            V_DIMS = 3, VN_DIMS = 3, VT_DIMS = 2, // for reference for other implementing classes
            VS_COUNT = 3;
        // v, vn, vt; 3 total.
        public const float PI = (float) Math.PI, TWO_PI = 2 * PI;
        protected readonly String[][] OBJ_section_name = {
            new String[]{ "vertices", "v" },
            new String[]{ "normals", "vn" },
            new String[]{ "UVs", "vt" }
        };
            
        protected readonly float[] vs, vns, vts;
        protected readonly float[][] data;
        protected readonly int vertexCount;

        public Model ( float[][] data ) {
            this.data = data;
            vs = data[0];
            vns = data[1];
            vts = data[2];
            vertexCount = vs.Length / V_DIMS;
        }

        public Model ( float[] vertices, float[] normals, float[] uvs ) : this( new float[][]{ vertices, normals, uvs } ) {
        }

        public void render () {
        }

        public float[] getVertices () {
            return vs;
        }

        public float[] getNormals () {
            return vns;
        }

        public float[] getUVs () {
            return vts;
        }

        protected void writeOBJ_buf ( StreamWriter sw, int bufNr ) {
            var fb = new FloatBuffer( data[bufNr] );
            sw.Write( "\n# " + OBJ_section_name[bufNr][0] );
            String prefix = "\n" + OBJ_section_name[bufNr][1] + " ";
            if ( bufNr != 2 )
                for ( int j = 0; j < vertexCount; j++ )
                    sw.Write( prefix + fb.get() + " " + fb.get() + " " + fb.get() );
            else
                for ( int j = 0; j < vertexCount; j++ )
                    sw.Write( prefix + fb.get() + " " + fb.get() );
            sw.Write( "\n" );
        }

        public void writeOBJ ( String filename ) {
            using ( StreamWriter sw = new StreamWriter( filename ) ) {
                sw.Write( "# created by " + PhysicalWindow.APP_NAME + "\n" );
                int tri_cnt = vertexCount / VERTEX_COUNT;
                sw.Write( "# " + vertexCount + " vertex total == normals == UVs\n"
                + "# " + tri_cnt + " tris == faces\n"
                + "\n# VERTEX" );
                writeOBJ_buf( sw, 0 );
                writeOBJ_buf( sw, 2 );
                writeOBJ_buf( sw, 1 );
                for ( int i = 0, j = 1, k = 2, l = 3; i < tri_cnt; i++, k += 3, j += 3, l += 3 ) {
                    sw.Write( "f " + j + "/" + j + "/" + j + " " + k + "/" + k + "/" + k + " " + l + "/" + l + "/" + l + "\n" );
                }
                sw.Write( "\n# EOF\n" );
            }
        }
    }
}

