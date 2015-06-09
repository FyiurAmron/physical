
using physical.math;
using physical.util;

namespace physical.model {
    public class PrismModel : Model {
        const int PRISM_VERTEX_COUNT = ( 2 + 3 * 2 ) * Model.VERTEX_COUNT;
        // top/bottom + 3 sides (2 tri each)

        public PrismModel ( float[][] ps, float height ) : base( buildPrism( ps, height ) ) {
        }

        public PrismModel ( float[] p1, float[] p2, float[] p3, float height ) :
            base( buildPrism( new float[][] { p1, p2, p3 }, height ) ) {
        }

        public static ModelData buildPrism ( float[][] p, float height ) { // p = float[3][3]; // three vert., three coords
            float[][] r = new float[Model.VERTEX_COUNT][];
            for ( int i = 0; i < Model.VERTEX_COUNT; i++ )
                r[i] = new float[Model.V_DIMS];

            float[] base_norm = Vector3f.getNormal( p[0], p[1], p[2] ),
            rev_norm = { -base_norm[0], -base_norm[1], -base_norm[2] };

            for ( int i = 0; i < Model.VERTEX_COUNT; i++ )
                for ( int j = 0; j < Model.V_DIMS; j++ )
                    r[i][j] = p[i][j] + rev_norm[j] * height; // since rev normal is set to 'inside', it goes up from the base

            ArrayCompiler.Floats vx = new ArrayCompiler.Floats( Model.V_DIMS * PRISM_VERTEX_COUNT ),
            vn = new ArrayCompiler.Floats( Model.VN_DIMS * PRISM_VERTEX_COUNT ),
            vt = new ArrayCompiler.Floats( Model.VT_DIMS * PRISM_VERTEX_COUNT );

            for ( int i = 0; i < Model.VERTEX_COUNT; i++ ) {
                vx.put( p[i] );
                vn.put( base_norm );
            }
            vt.put( Model.TRI_VT_PROTO );

            float[] n;
            for ( int i = 0, j = 1; i < 3; i++, j++ ) {
                if ( j == 3 )
                    j = 0;

                vx.put( p[i] );
                vx.put( r[i] );
                vx.put( p[j] );

                vx.put( p[j] );
                vx.put( r[i] );
                vx.put( r[j] );

                n = Vector3f.getNormal( p[i], r[i], p[j] );
                for ( int k = 0; k < 2 * Model.VERTEX_COUNT; k++ )
                    vn.put( n );

                vt.put( RECT_VT_PROTO );
            }

            for ( int i = Model.VERTEX_COUNT - 1; i >= 0; i-- ) { // reverses winding
                vx.put( r[i] );
                vn.put( rev_norm );
            }
            vt.put( Model.TRI_VT_PROTO );
            return new ModelData( vx.compile(), vn.compile(), vt.compile() );
        }
    }
}

