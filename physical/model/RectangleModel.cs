using System;
using physical.math;

namespace physical.model {
    public class RectangleModel : Model {
        /**
     Creates a new OXY rectangle of given sizes, centered at OY axis.

     @param size_x
     @param size_y
     */
        public RectangleModel ( float size_x, float size_y ) :
            this( new float[]{ -size_x / 2, 0, 0 }, new float[]{ size_x / 2, 0, 0 }, new float[] {
                -size_x / 2,
                size_y,
                0
            }, 1, 1 ) {
        }

        /**
     Creates a new rectangle orthogonal to given axis, centered at it.

     @param axis axis int as defined by SC3D_constant ( 0 == OX, 1 == OY, 2 == OZ )
     @param size_1 first size (X or Y)
     @param size_2 second size (Y or Z)
     */
        public RectangleModel ( int axis, float size_1, float size_2 ) :
            this( buildAxedRectangle( axis, size_1, size_2 ), 1, 1 ) {
        }

        /* public rectangle( float[] p1, float[] p2, float[] p3 ) {
     super( prepare_rectangle_arrs( p1, p2, p3, 1, 1 ) );
     } */
        public RectangleModel ( float[][] ps, int seg_u, int seg_v ) :
            this( ps[0], ps[1], ps[2], seg_u, seg_v ) {
        }

        public RectangleModel ( float[] p1, float[] p2, float[] p3, int seg_u, int seg_v ) : base( buildRectangle( p1, p2, p3, seg_u, seg_v ) ) {
        }

        static protected ModelData buildRectangle ( float[] p1, float[] p2, float[] p3, int seg_u, int seg_v ) {
            int v_total = 2 * Model.VERTEX_COUNT * seg_u * seg_v; // 2 tris for each seg, 6 verts per seg
            float[] vx = new float[Model.V_DIMS * v_total],
            vn = new float[Model.VN_DIMS * v_total],
            vt = new float[Model.VT_DIMS * v_total],
            u = VectorFloat.getDiff( p2, p1 ),
            v = VectorFloat.getDiff( p3, p1 );

            float u0, u1, v0, v1; // calculated in-place to avoid loss of precision for large surfaces
            for ( int i = 0, i_step = 2 * Model.VERTEX_COUNT * Model.V_DIMS, i_u = 0, k; i_u < seg_u; i_u++ )
                for ( int i_v = 0; i_v < seg_v; i_v++, i += i_step )
                    for ( int j = 0; j < Model.V_DIMS; j++ ) {
                        k = i + j;
                        u0 = u[j] * i_u / seg_u;
                        u1 = u[j] * ( i_u + 1 ) / seg_u;
                        v0 = v[j] * i_v / seg_v;
                        v1 = v[j] * ( i_v + 1 ) / seg_v;

                        vx[k] = p1[j] + u0 + v0;
                        vx[k + Model.V_DIMS] = p1[j] + u1 + v0;
                        vx[k + 2 * Model.V_DIMS] = p1[j] + u0 + v1;
                        vx[k + 3 * Model.V_DIMS] = vx[k + 2 * Model.V_DIMS];
                        vx[k + 4 * Model.V_DIMS] = vx[k + Model.V_DIMS];
                        vx[k + 5 * Model.V_DIMS] = p1[j] + u1 + v1;
                    }

            for ( int i = 0, max = Model.VT_DIMS * v_total, vt_step = Model.RECT_VT_PROTO.Length; i < max; i += vt_step )
                System.Array.Copy( Model.RECT_VT_PROTO, 0, vt, i, vt_step );

            float[] n = Vector3f.getNormal( p1, p2, p3 ); // the normal should be equal for the whole rectangle
            for ( int i = 0, max = Model.VN_DIMS * v_total; i < max; i += Model.VN_DIMS )
                System.Array.Copy( n, 0, vn, i, Model.VN_DIMS );

            return new ModelData( vx, vn, vt );
        }

        static protected float[][] buildAxedRectangle ( int axis, float size_1, float size_2 ) {
            size_1 *= 0.5f;
            size_2 *= 0.5f;
            switch ( axis ) {
            case OX:
                return new float[][] {
                    new float[]{ 0, -size_1, -size_2 },
                    new float[]{ 0, size_1, -size_2 },
                    new float[]{ 0, -size_1, size_2 }
                };
            case OY:
                return new float[][] {
                    new float[]{ -size_1, 0, -size_2 },
                    new float[]{ size_1, 0, -size_2 },
                    new float[]{ -size_1, 0, size_2 }
                };
            case OZ:
                return new float[][] {
                    new float[]{ -size_1, -size_2, 0 },
                    new float[]{ size_1, -size_2, 0 },
                    new float[]{ -size_1, size_2, 0 }
                };
            }
            throw new InvalidOperationException( "unknown axis nr '" + axis + "'" );
        }
    }
}

