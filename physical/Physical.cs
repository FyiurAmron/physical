/**
 * 
 *
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace physical {
    public class PhysicalWindow : GameWindow {
        HashSet<Renderable> renderables = new HashSet<Renderable>();

        string vertexShaderSource = @"
#version 140
precision highp float;

uniform mat4 projectionMatrix;
uniform mat4 modelviewMatrix;
uniform float time;

in vec3 inPosition;
in vec3 inNormal;
out vec3 normal;

void main() {
    normal = (modelviewMatrix * vec4(inNormal, 0)).xyz;
  
    gl_Position = projectionMatrix * modelviewMatrix * vec4(inPosition, 1);
    gl_Position.y *= sin(time+gl_Position.x+gl_Position.z);
}";

        string fragmentShaderSource = @"
#version 140
precision highp float;

uniform vec3 ambientColor;
uniform vec3 lightColor;
uniform vec3 lightDirUnit;
uniform float time;

in vec3 normal;
out vec4 out_fragColor;

void main() {
    float diffuseColor = clamp( dot( lightDirUnit, normalize(normal) ), 0.0, 1.0 );
    out_fragColor = vec4( ambientColor + diffuseColor * lightColor, 1.0 );
}";

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle;

        //    modelviewMatrixLocation,
        //    projectionMatrixLocation;

        Matrix4 projectionMatrix, modelviewMatrix;
        Vector3 //
            ambientColor = new Vector3( 0.1f, 0.1f, 0.1f ),
            lightColor = new Vector3( 0.9f, 0.9f, 0.9f ),
            lightDirUnit = new Vector3( 0.5f, 0.5f, 2.0f ).Normalized();
        float time;

        SphereModel sm;
        public static readonly String[] attribs = { "inPosition", "inNormal" };
        public static readonly String[] uniforms = {
            "projectionMatrix",
            "modelviewMatrix",
            "ambientColor",
            "lightColor",
            "lightDirUnit",
            "time"
        };
        public static readonly Dictionary<string,int> uniformMap = new Dictionary<string,int>();

        public PhysicalWindow ()
            : base( 640, 480,
                    new GraphicsMode(), "OpenGL 3 Example", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        protected override void OnLoad ( System.EventArgs e ) {
            CreateShaders();

            sm = new SphereModel( 0.5f, 10, 10, true );
            sm.init();
            //sm.writeOBJ("sphere.obj");
            renderables.Add( sm );

            VSync = VSyncMode.On;

            GL.Enable( EnableCap.DepthTest );
            GL.ClearColor( System.Drawing.Color.DarkOliveGreen );
        }

        void CreateShaders () {
            vertexShaderHandle = GL.CreateShader( ShaderType.VertexShader );
            fragmentShaderHandle = GL.CreateShader( ShaderType.FragmentShader );

            GL.ShaderSource( vertexShaderHandle, vertexShaderSource );
            GL.ShaderSource( fragmentShaderHandle, fragmentShaderSource );

            GL.CompileShader( vertexShaderHandle );
            GL.CompileShader( fragmentShaderHandle );

            Debug.WriteLine( GL.GetShaderInfoLog( vertexShaderHandle ) );
            Debug.WriteLine( GL.GetShaderInfoLog( fragmentShaderHandle ) );

            // Create program
            shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader( shaderProgramHandle, vertexShaderHandle );
            GL.AttachShader( shaderProgramHandle, fragmentShaderHandle );

            for ( int i = 0; i < attribs.Length; i++ )
                GL.BindAttribLocation( shaderProgramHandle, i, attribs[i] );

            GL.LinkProgram( shaderProgramHandle );
            Debug.WriteLine( GL.GetProgramInfoLog( shaderProgramHandle ) );
            GL.UseProgram( shaderProgramHandle );

            foreach ( string s in uniforms )
                uniformMap.Add( s, GL.GetUniformLocation( shaderProgramHandle, s ) );

            float aspectRatio = ClientSize.Width / (float) ( ClientSize.Height );
            Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI / 4, aspectRatio, 1, 100, out projectionMatrix );
            modelviewMatrix = Matrix4.LookAt( new Vector3( 0, 3, 5 ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) );

            setUniforms();
        }

        protected override void OnUpdateFrame ( FrameEventArgs e ) {
            //Matrix4 rotation = Matrix4.CreateRotationY( (float) e.Time );
            //Matrix4.Mult( ref rotation, ref modelviewMatrix, out modelviewMatrix );

            var keyboard = OpenTK.Input.Keyboard.GetState();
            if ( keyboard[OpenTK.Input.Key.Escape] )
                Exit();
        }

        void setUniforms () {
            GL.UniformMatrix4( uniformMap[uniforms[0]], false, ref projectionMatrix );
            GL.UniformMatrix4( uniformMap[uniforms[1]], false, ref modelviewMatrix );
            GL.Uniform3( uniformMap[uniforms[2]], ref ambientColor );
            GL.Uniform3( uniformMap[uniforms[3]], ref lightColor );
            GL.Uniform3( uniformMap[uniforms[4]], ref lightDirUnit );
            GL.Uniform1( uniformMap[uniforms[5]], time );
        }

        protected override void OnRenderFrame ( FrameEventArgs e ) {
            time = ( DateTime.Now.Ticks % 100000000000 ) / 1E7f;
            //Console.WriteLine(time);

            GL.Viewport( 0, 0, Width, Height );

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            setUniforms();

            foreach ( Renderable r in renderables ) {
                r.render();
            }

            SwapBuffers();
        }



    }
}