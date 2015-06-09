
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using physical.math;

namespace physical {
    public interface IUniform {
        void updateGl ( int uniformLocation );

        string getName ();

        string getPrefix ();
    }

    public abstract class Uniform<T> : IUniform {
        protected T value;
        protected string name;

        public Uniform ( string name, T value ) {
            this.value = value;
            this.name = name;
        }

        public string getName () {
            return name;
        }

        override public string ToString () {
            return getPrefix() + " " + getName() + "[" + value + "]";
        }

        public string ToShaderString () {
            return "uniform " + getPrefix() + " " + getName() + ";\n";
        }

        abstract public void updateGl ( int uniformLocation );

        abstract public string getPrefix ();
    }

    public class UniformManager {
        public static readonly List<IUniform> uniforms = new List<IUniform>();
        public static readonly Dictionary<IUniform,int> uniformMap = new Dictionary<IUniform,int>();
        public static readonly Dictionary<string,IUniform> uniformNameMap = new Dictionary<string,IUniform>();

        public void addUniform ( IUniform iUniform ) {
            uniforms.Add( iUniform );
        }

        public void addUniforms ( params IUniform[] iUniforms ) {
            foreach ( IUniform iUniform in iUniforms )
                uniforms.Add( iUniform );
        }

        public IUniform getIUniform ( string name ) {
            return uniformNameMap[name];
        }

        public int getLocation ( IUniform iUniform ) {
            return uniformMap[iUniform];
        }

        public void init ( int shaderProgramHandle ) {
            foreach ( IUniform uniform in uniforms ) {
                int uniformLocation = GL.GetUniformLocation( shaderProgramHandle, uniform.getName() );
                uniformMap.Add( uniform, uniformLocation );
                uniformNameMap.Add( uniform.getName(), uniform );
                uniform.updateGl( uniformLocation );
            }
        }

        public void updateGl ( string uniformName ) {
            updateGl( getIUniform( uniformName ) );
        }

        public void updateGl ( IUniform iUniform ) {
            iUniform.updateGl( getLocation( iUniform ) );
        }

        public void updateGl () {
            foreach ( IUniform uniform in uniforms )
                updateGl( uniform );
        }
    }

    /*
    public class Uniform1f : Uniform<float[]> {
        public Uniform1f ( String name, ref float[] value ) : base( name, ref value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform1( uniformLocation, value[0] );
        }
    }


    public class Uniform2f : Uniform<Vector2> {
        public Uniform2f ( String name, ref Vector2 value ) : base( name, ref value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform2( uniformLocation, ref value );
        }
    }


    public class Uniform3f : Uniform<Vector3> {
        public Uniform3f ( String name, ref Vector3 value ) : base( name, ref value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform3( uniformLocation, ref value );
        }
    }

    public class Uniform4f : Uniform<Vector4> {
        public Uniform4f ( String name, ref Vector4 value ) : base( name, ref value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform4( uniformLocation, ref value );
        }
    }

    public class UniformMatrix4 : Uniform<Matrix4> {
        public UniformMatrix4 ( String name, Matrix4 value ) : base( name, ref value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.UniformMatrix4( uniformLocation, false, value );
        }
    }
    */

    public class Uniform1f : Uniform<Value1f> {
        public Uniform1f ( string name, Value1f value ) : base( name, value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform1( uniformLocation, value.Value );
        }

        override public string getPrefix () {
            return "float";
        }
    }

    public class Uniform2f : Uniform<Vector2f> {
        public Uniform2f ( string name, Vector2f value ) : base( name, value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform2( uniformLocation, 1, value.Data );
        }

        override public string getPrefix () {
            return "vec2";
        }
    }


    public class Uniform3f : Uniform<Vector3f> {
        public Uniform3f ( string name, Vector3f value ) : base( name, value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform3( uniformLocation, 1, value.Data );
        }

        override  public string getPrefix () {
            return "vec3";
        }
    }


    public class Uniform4f : Uniform<Vector4f> {
        public Uniform4f ( string name, Vector4f value ) : base( name, value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.Uniform4( uniformLocation, 1, value.Data );
        }

        override  public string getPrefix () {
            return "vec4";
        }
    }

    public class UniformMatrix4f : Uniform<Matrix4f> {
        public UniformMatrix4f ( string name, Matrix4f value ) : base( name, value ) {
        }

        override public void updateGl ( int uniformLocation ) {
            GL.UniformMatrix4( uniformLocation, 1, false, value.Data );
        }

        override public string getPrefix () {
            return "mat4";
        }
    }
}

