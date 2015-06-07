package vax.sqvaardcraft.ui.sc3d.model;

import vax.sqvaardcraft.ui.sc3d.SC3D_Model;
import java.awt.Color;
import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import javax.media.opengl.GL2;

import vax.sqvaardcraft.ui.sc3d.SC3D_Mode;
import vax.sqvaardcraft.ui.sc3d.SC3D_Util;
import vax.sqvaardcraft.util.ArrayCompiler;
import static vax.sqvaardcraft.util.math.SC_Vector.*;
import static vax.sqvaardcraft.util.math.FloatMath.*;
import static vax.sqvaardcraft.ui.sc3d.SC3D_const.*;
import static vax.sqvaardcraft.ui.sc3d.model.VertexArrayModel.constant.*;

/**

 @author poponuro
 */
public class PrimitiveModel {
    final static public float[] RECT_VT_PROTO = {
    0, 1, /* */ 1, 1,/* */ 0, 0,
    0, 0, /* */ 1, 1,/* */ 1, 0, //
  };
  final static public float[] TRI_VT_PROTO = {
    1, 1, /* */ 1, 0, /* */ 0, 1 };

  static public class point extends SC3D_Model {
    public Float size;
    public float[] v, color;

    public point( float[] v ) {
      this.v = v;
    }

    public point( Color c, float[] v ) {
      color = c.getRGBComponents( null );
      this.v = v;
    }

    public point( float[] color, float size, float[] v ) {
      this.size = size;
      this.v = v;
      this.color = color;
    }

    @Override
    public void render( GL2 gl ) {
      float old_size = 0;
      float[] old_color = null;
      if ( size != null ) {
        old_size = SC3D_Util.get_GL_float( gl, GL2.GL_LINE_WIDTH );
        gl.glPointSize( size );
      }
      if ( color != null ) {
        old_color = new float[4];
        gl.glGetFloatv( GL2.GL_CURRENT_COLOR, old_color, 0 );
        gl.glColor4fv( color, 0 );
      }
      gl.glBegin( GL2.GL_POINTS );
      gl.glVertex3fv( v, 0 );
      gl.glEnd();
      if ( color != null )
        gl.glColor4fv( old_color, 0 );
      if ( size != null )
        gl.glLineWidth( old_size );
    }
  }

  static public class line extends SC3D_Model {
    public Float width;
    public float[] v1, v2, color;

    public line() {
      v1 = v3f.ZERO;
      v2 = v3f.ZERO;
    }

    public line( Color c ) {
      this();
      color = c.getRGBComponents( null );
    }

    public line( Color c, line_seg3f line_segment ) {
      this( c, line_segment.get_v0().v, line_segment.get_seg_end().v );
    }

    public line( float[] color, float[] v1, float[] v2 ) {
      this.color = color;
      this.v1 = v1;
      this.v2 = v2;
    }

    public line( Color c, float[] v1, float[] v2 ) {
      color = c.getRGBComponents( color );
      this.v1 = v1;
      this.v2 = v2;
    }

    public line( Color c, float[] v1, float[] v2, float width ) {
      this( c, v1, v2 );
      this.width = width;
    }

    public void match_line_segment( line_seg3f ls ) {
      v1 = ls.get_v0().v;
      v2 = ls.get_seg_end().v;
    }

    @Override
    public void set_state( Object state_key ) {
      if ( state_key instanceof Color )
        color = ( (Color) state_key ).getColorComponents( null );
    }

    @Override
    public void render( GL2 gl ) {
      float old_width = 0;
      float[] old_color = null;
      if ( width != null ) {
        old_width = SC3D_Util.get_GL_float( gl, GL2.GL_LINE_WIDTH );
        gl.glLineWidth( width );
      }
      if ( color != null ) {
        old_color = new float[4];
        gl.glGetFloatv( GL2.GL_CURRENT_COLOR, old_color, 0 );
        gl.glColor4fv( color, 0 );
      }
      gl.glBegin( GL2.GL_LINES );
      gl.glVertex3fv( v1, 0 );
      gl.glVertex3fv( v2, 0 );
      gl.glEnd();
      if ( color != null )
        gl.glColor4fv( old_color, 0 );
      if ( width != null )
        gl.glLineWidth( old_width );
    }
  }

  static public class rectangle extends VertexArrayModel { // p4 is assumed opposite of p1
    //
    static protected float[][] prepare_rectangle_arrs( float[] p1, float[] p2, float[] p3, int seg_u, int seg_v ) {
      int v_total = 2 * VERTEX_COUNT * seg_u * seg_v; // 2 tris for each seg, 6 verts per seg
      float[] vx = new float[V_DIMS * v_total],
              vn = new float[VN_DIMS * v_total],
              vt = new float[VT_DIMS * v_total],
              u = get_diff( p2, p1 ),
              v = get_diff( p3, p1 );

      float u0, u1, v0, v1; // calculated in-place to avoid loss of precision for large surfaces
      for( int i = 0, i_step = 2 * VERTEX_COUNT * V_DIMS, i_u = 0, k; i_u < seg_u; i_u++ )
        for( int i_v = 0; i_v < seg_v; i_v++, i += i_step )
          for( int j = 0; j < V_DIMS; j++ ) {
            k = i + j;
            u0 = u[j] * i_u / seg_u;
            u1 = u[j] * ( i_u + 1 ) / seg_u;
            v0 = v[j] * i_v / seg_v;
            v1 = v[j] * ( i_v + 1 ) / seg_v;

            vx[k] = p1[j] + u0 + v0;
            vx[k + V_DIMS] = p1[j] + u1 + v0;
            vx[k + 2 * V_DIMS] = p1[j] + u0 + v1;
            vx[k + 3 * V_DIMS] = vx[k + 2 * V_DIMS];
            vx[k + 4 * V_DIMS] = vx[k + V_DIMS];
            vx[k + 5 * V_DIMS] = p1[j] + u1 + v1;
          }

      for( int i = 0, max = VT_DIMS * v_total, vt_step = RECT_VT_PROTO.length; i < max; i += vt_step )
        System.arraycopy( RECT_VT_PROTO, 0, vt, i, vt_step );

      float[] n = v3f.get_normal( p1, p2, p3 ); // the normal should be equal for the whole rectangle
      for( int i = 0, max = VN_DIMS * v_total; i < max; i += VN_DIMS )
        System.arraycopy( n, 0, vn, i, VN_DIMS );

      return new float[][]{ vx, vn, vt };
    }

    static protected float[][] prepare_axed_rectangle( int axis, float size_1, float size_2 ) {
      size_1 *= 0.5f;
      size_2 *= 0.5f;
      switch ( axis ) {
        case OX:
          return new float[][]{ { 0, -size_1, -size_2 }, { 0, size_1, -size_2 }, { 0, -size_1, size_2 } };
        case OY:
          return new float[][]{ { -size_1, 0, -size_2 }, { size_1, 0, -size_2 }, { -size_1, 0, size_2 } };
        case OZ:
          return new float[][]{ { -size_1, -size_2, 0 }, { size_1, -size_2, 0 }, { -size_1, size_2, 0 } };
      }
      throw new IllegalArgumentException( "unknown axis nr '" + axis + "'" );
    }

    /**
     Creates a new OXY rectangle of given sizes, centered at OY axis.

     @param size_x
     @param size_y
     */
    public rectangle( float size_x, float size_y ) { // OXY rectangle,
      this( new float[]{ -size_x / 2, 0, 0 }, new float[]{ size_x / 2, 0, 0 }, new float[]{ -size_x / 2, size_y, 0 }, 1, 1 );
    }

    /**
     Creates a new rectangle orthogonal to given axis, centered at it.

     @param axis axis int as defined by SC3D_constant ( 0 == OX, 1 == OY, 2 == OZ )
     @param size_1 first size (X or Y)
     @param size_2 second size (Y or Z)
     */
    public rectangle( int axis, float size_1, float size_2 ) {
      this( prepare_axed_rectangle( axis, size_1, size_2 ), 1, 1 );
    }

    /* public rectangle( float[] p1, float[] p2, float[] p3 ) {
     super( prepare_rectangle_arrs( p1, p2, p3, 1, 1 ) );
     } */
    public rectangle( float[][] ps, int seg_u, int seg_v ) {
      this( ps[0], ps[1], ps[2], seg_u, seg_v );
    }

    public rectangle( float[] p1, float[] p2, float[] p3, int seg_u, int seg_v ) {
      super( prepare_rectangle_arrs( p1, p2, p3, seg_u, seg_v ) );
    }
  }

  static public class hexagon extends VertexArrayModel.IntIndexed {
    static final public float A = 2, HALF_A = A * 0.5f, H = sqrt( 3 ) * HALF_A;
    static final protected float[] //
            v = { // hexagon vertices
              0, 0, 0, // center
              HALF_A, H, 0, // v1
              A, 0, 0,
              HALF_A, -H, 0,
              -HALF_A, -H, 0,
              -A, 0, 0,
              -HALF_A, H, 0, // v6
            },
            vn = { // vertex normals
              0, 0, 1, // center
              0, 0, 1, // v1
              0, 0, 1,
              0, 0, 1,
              0, 0, 1,
              0, 0, 1,
              0, 0, 1, // v6
            },
            vt1 = { // vertex UVs cropped
              0.5f, 0.5f, // center
              0.75f, 0, // v1
              1, 0.5f,
              0.75f, 1,
              0.25f, 1,
              0, 0.5f,
              0.25f, 0, // v6
            },
            vt2 = { // vertex UVs stretched
              0.5f, 0.5f, // center
              1, 0, // v1
              1, 0.5f,
              1, 1,
              0, 1,
              0, 0.5f,
              0, 0, // v6
            };
    static final protected int[] ix = { 0, 1, 6, 5, 4, 3, 2, 1 }; // hexagon trifan indices
    static final protected FloatBuffer //
            fbv = SC3D_Util.new_direct_FloatBuffer( v ),
            fbvn = SC3D_Util.new_direct_FloatBuffer( vn ),
            fbvt1 = SC3D_Util.new_direct_FloatBuffer( vt1 ),
            fbvt2 = SC3D_Util.new_direct_FloatBuffer( vt2 );
    static final protected IntBuffer ib = SC3D_Util.new_direct_IntBuffer( ix );

    /**
     Constructs a 6-triangle Hexagon with A=2 and center in (0,0).

     @param vt UV mapping (vt1 = crop mode, vt2 = stretch mode)
     */
    protected hexagon( FloatBuffer vt ) {
      super( fbv, fbvn, vt, ib );
      set_SC3D_Mode( SC3D_Mode.TRIANGLE_FAN );
    }

    static final protected VertexArrayModel.IntIndexed //
            singleton_cropped = new hexagon( fbvt1 ),
            singleton_stretched = new hexagon( fbvt2 );

    static public VertexArrayModel.IntIndexed get_singleton( boolean stretched ) {
      return stretched ? singleton_stretched : singleton_cropped;
    }

    static public VertexArrayModel.IntIndexed get_singleton() {
      return singleton_cropped;
    }
  }

  static public class wire_box extends VertexArrayModel.IntIndexed {
    final static protected IntBuffer box_indices_buf = SC3D_Util.new_direct_IntBuffer( new int[]{
      0, 1, 0, 2, 0, 4, 1, 3, 1, 5, 2, 3, 2, 6, 3, 7, 4, 5, 4, 6, 5, 7, 6, 7
    } );
    final static protected int OPT_X = 2, OPT_Y = 2, OPT_Z = 2;

    static protected VertexArrayModel.IntIndexed.buffer_collection prepare_wire_box_arrs( float x, float y, float z ) {
      FloatBuffer fb = SC3D_Util.new_direct_FloatBuffer( OPT_X * OPT_Y * OPT_Z * VERTEX_COUNT );
      for( int i = 0; i < OPT_X; i++ )
        for( int j = 0; j < OPT_Y; j++ )
          for( int k = 0; k < OPT_Z; k++ ) {
            fb.put( i * x );
            fb.put( j * y );
            fb.put( k * z );
          }
      fb.rewind();
      return new VertexArrayModel.IntIndexed.buffer_collection( fb, null, null, box_indices_buf );
    }

    public wire_box( float size_x, float size_y, float size_z ) {
      super( prepare_wire_box_arrs( size_x, size_y, size_z ) );
      set_SC3D_Mode( SC3D_Mode.LINES );
    }

    public wire_box( float[] sizes ) {
      this( sizes[0], sizes[1], sizes[2] );
    }
  }

  static public class prism extends VertexArrayModel {
    final static protected int PRISM_VERTEX_COUNT = ( 2 + 3 * 2 ) * VERTEX_COUNT; // top/bottom + 3 sides (2 tri each)

    public prism( float[][] ps, float height ) {
      super( prepare_prism_arrs( ps, height ) );
    }

    public prism( float[] p1, float[] p2, float[] p3, float height ) {
      this( new float[][]{ p1, p2, p3 }, height );
    }

    static protected float[][] prepare_prism_arrs( float[][] p, float height ) { // p = float[3][3]; // three vert., three coords
      float[][] r = new float[VERTEX_COUNT][V_DIMS];

      float[] base_norm = v3f.get_normal( p[0], p[1], p[2] ),
              rev_norm = { -base_norm[0], -base_norm[1], -base_norm[2] };

      for( int i = 0; i < VERTEX_COUNT; i++ )
        for( int j = 0; j < V_DIMS; j++ )
          r[i][j] = p[i][j] + rev_norm[j] * height; // since rev normal is set to 'inside', it goes up from the base

      ArrayCompiler.floats vx = new ArrayCompiler.floats( V_DIMS * PRISM_VERTEX_COUNT ),
              vn = new ArrayCompiler.floats( VN_DIMS * PRISM_VERTEX_COUNT ),
              vt = new ArrayCompiler.floats( VT_DIMS * PRISM_VERTEX_COUNT );

      for( int i = 0; i < VERTEX_COUNT; i++ ) {
        vx.put( p[i] );
        vn.put( base_norm );
      }
      vt.put( TRI_VT_PROTO );

      float[] n;
      for( int i = 0, j = 1; i < 3; i++, j++ ) {
        if ( j == 3 )
          j = 0;

        vx.put( p[i] );
        vx.put( r[i] );
        vx.put( p[j] );

        vx.put( p[j] );
        vx.put( r[i] );
        vx.put( r[j] );

        n = v3f.get_normal( p[i], r[i], p[j] );
        for( int k = 0; k < 2 * VERTEX_COUNT; k++ )
          vn.put( n );

        vt.put( RECT_VT_PROTO );
      }

      for( int i = VERTEX_COUNT - 1; i >= 0; i-- ) { // reverses winding
        vx.put( r[i] );
        vn.put( rev_norm );
      }
      vt.put( TRI_VT_PROTO );

      return new float[][]{ vx.compile(), vn.compile(), vt.compile() };
    }
  }

  static public class sphere extends VertexArrayModel {
    public sphere( float radius, int slices, int stacks, boolean outside ) {
      super( prepare_sphere( radius, slices, stacks, outside ) );
    }

    public sphere( float radius, int slices, int stacks ) {
      this( radius, slices, stacks, true );
    }

    public sphere( float radius, int slices ) {
      this( radius, slices, slices, true );
    }
    //

    static protected class sphere_prep {
      /** * &lt; 0.5 creates distortion on last valid stack, &gt; 0.5 generates unnecessAry 'sewing' on it */
      final static protected float CAP_RATIO = 0.5f;
      final protected float radius, ds;
      final protected int slices, stacks, nsign;
      final protected float[] m_s_theta, c_theta;
      protected float rho, t;
      final protected float[][][][] v_cache;
      final protected ArrayCompiler.floats[] fac;

      protected void swap_cache() {
        float[][][] tmp = v_cache[0];
        v_cache[0] = v_cache[1];
        v_cache[1] = tmp;
      }

      protected void fac_put( float[][]
        ... vs ) {
        if ( vs != null )
          for( float[][] fv : vs )
            for( int i = 0; i < VS_COUNT; i++ )
              fac[i].put( fv[i] );
      }

      protected void set_cache() {
        float s_rho = sin( rho ), c_rho = cos( rho ),
                s = 0,
                x, y, z;
        for( int j = 0; j <= slices; j++, s += ds ) {
          z = m_s_theta[j] * s_rho;
          y = c_rho * nsign;
          x = c_theta[j] * s_rho;
          v_cache[1][j] = new float[][]{ { x * radius, y * radius, z * radius }, { x * nsign, y * nsign, z * nsign }, { s, t } };
        }
      }

      protected sphere_prep( float radius, int slices, int stacks, boolean outside ) {
        if ( stacks < 2 || slices < 2 )
          throw new IllegalArgumentException( "stacks < 2 || slices < 2" );
        this.radius = radius;
        this.slices = slices;
        this.stacks = stacks;

        int ops = ( stacks + 1 ) * slices * 6; // stacks +1 for 2 'caps' with half tris at each sphere's 'end'
        fac = new ArrayCompiler.floats[]{
          new ArrayCompiler.floats( ops * V_DIMS ),
          new ArrayCompiler.floats( ops * VN_DIMS ),
          new ArrayCompiler.floats( ops * VT_DIMS ) };
        nsign = outside ? 1 : -1;

        m_s_theta = new float[slices + 1];
        c_theta = new float[slices + 1];
        float theta = 0, dtheta = TWO_PI / slices;
        for( int j = 0; j <= slices; j++, theta += dtheta ) {
          m_s_theta[j] = -sin( theta );
          c_theta[j] = cos( theta );
        }
        v_cache = new float[2][slices + 1][][];
        ds = 1.0f / slices;
      }

      public float[][] prepare() { // note: the caps are constructed differently vs GLU (due to CAP_RATIO)
        float drho = PI / stacks, dt = 1.0f / stacks, s, half_ds = ds / 2;
        rho = drho * CAP_RATIO;
        t = dt * CAP_RATIO; // this is reversed vs GLU, since we draw top-to-bottom (correct alignment)

        set_cache(); // cap 1
        s = half_ds; // pushed to the middle to make the trifan smoother; use s = 0 for legacy (GLU) rendering
        for( int j = 0; j < slices; j++, s += ds )
          fac_put( v_cache[1][j + 1], new float[][]{ { 0, radius * nsign, 0 }, { 0, nsign, 0 }, { s, 0 } }, v_cache[1][j] );
        swap_cache();
        rho = drho;
        t = dt;

        for( int i = 1; i < stacks; i++, rho += drho, t += dt ) {
          set_cache();
          for( int j = 0; j < slices; j++ )
            fac_put( v_cache[0][j], v_cache[1][j], v_cache[0][j + 1], v_cache[1][j + 1], v_cache[0][j + 1], v_cache[1][j] );
          swap_cache();
        }

        rho = PI - drho * CAP_RATIO;
        t = 1 - dt * CAP_RATIO;
        set_cache();
        for( int j = 0; j < slices; j++ )
          fac_put( v_cache[0][j], v_cache[1][j], v_cache[0][j + 1], v_cache[1][j + 1], v_cache[0][j + 1], v_cache[1][j] );
        swap_cache();

        s = half_ds; // cap 2
        for( int j = 0; j < slices; j++, s += ds ) // last slice with tip
          fac_put( v_cache[0][j], new float[][]{ { 0, -radius * nsign, 0 }, { 0, -nsign, 0 }, { s, 1 } }, v_cache[0][j + 1] );

        return new float[][]{ fac[0].compile(), fac[1].compile(), fac[2].compile() };
      }
    }

    static public float[][] prepare_sphere( float radius, int slices, int stacks, boolean outside ) {
      return new sphere_prep( radius, slices, stacks, outside ).prepare();
    }
  }

  private PrimitiveModel() {
    throw new UnsupportedOperationException();
  }
}
