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
using OpenTK.Input;

using physical.math;

namespace physical {
    public class PhysicalWindow : GameWindow {
        HashSet<Model> models = new HashSet<Model>();

        string vertexShaderSource = @"
#version 140
precision highp float;

uniform mat4 projectionMatrix;
uniform mat4 modelviewMatrix;
uniform mat4 transform;
uniform float time;

in vec3 in_position;
in vec3 in_normal;
in vec2 in_uv;

out vec3 normal;
out vec2 uv;

void main() {
    mat4 combined = modelviewMatrix * transform;
    normal = (combined * vec4(in_normal, 0)).xyz;
    uv = in_uv;

    gl_Position = projectionMatrix * combined * vec4(in_position, 1);
    //gl_Position.y *= 0.5 + 0.5 * abs(sin(time+gl_Position.x+gl_Position.z)); // wavy!
}";

        string fragmentShaderSource = @"
#version 140
precision highp float;

uniform vec3 ambientColor;
uniform vec3 lightColor;
uniform vec3 lightDirUnit;
uniform float time;

uniform sampler2D textureSampler;

in vec3 normal;
in vec2 uv;
out vec4 out_fragColor;

void main() {
    //vec3 diffuseColor = vec3(uv,0); // UV debugging
    vec3 diffuseColor = texture(textureSampler, uv).rgb;
    vec3 lightShadedColor = lightColor * clamp( dot( lightDirUnit, normal ), 0.0, 1.0 );
    out_fragColor = vec4( ambientColor + diffuseColor * lightShadedColor, 1.0 );
}";

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle;

        Value1f //
            time = new Value1f(),
            textureSampler = new Value1f( 0 );
        Vector3f //
            ambientColor = new Vector3f(),
            lightColor = new Vector3f(),
            lightDirUnit = new Vector3f();
        Matrix4f //
            projectionMatrix = new Matrix4f(),
            modelviewMatrix = new Matrix4f(),
            transformMatrix = new Matrix4f( true );

        SphereModel sphereModel;
        UniformManager uniformManager = new UniformManager();
        Texture angrySquirrelTexture;

        public static readonly String[] attribs = { "in_position", "in_normal", "in_uv" };
        public static readonly Dictionary<string,int> uniformMap = new Dictionary<string,int>();
        public const string APP_NAME = "Physical";

        UniformMatrix4f transformMatrixUniform;

        public PhysicalWindow ()
            : base( 800, 600,
                    new GraphicsMode(), APP_NAME + " v0.0", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        override protected  void OnLoad ( System.EventArgs e ) {
            ambientColor.set( 0.1f, 0.1f, 0.1f );
            lightColor.set( 1.0f, 1.0f, 1.0f );
            lightDirUnit.set( 2.0f, 0.5f, 0.5f );
            lightDirUnit.normalize();
            //texture = new Texture( "E:\\drop\\logo-dark.jpg" );
            angrySquirrelTexture = new Texture( "E:\\drop\\angry-squirrel.png" );

            CreateShaders();

            sphereModel = new SphereModel( 1f, 10, 10, true );
            sphereModel.Transform.setIdentity();
            //sm = new SphereModel( 1.5f, 4, 2, true );
            sphereModel.init();
            sphereModel.UpdateAction = delegate(Model model ) {
                Matrix4f transform = model.Transform;
                transform.set( Matrix4.CreateRotationZ( time.Value ) );
                transform.setTranslationY( (float) Math.Sin( time.Value ) );  
            };
            sphereModel.Texture = angrySquirrelTexture;
            //sm.writeOBJ();
            models.Add( sphereModel );

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

            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderHandle ) );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShaderHandle ) );

            shaderProgramHandle = GL.CreateProgram();

            GL.AttachShader( shaderProgramHandle, vertexShaderHandle );
            GL.AttachShader( shaderProgramHandle, fragmentShaderHandle );

            for ( int i = 0; i < attribs.Length; i++ )
                GL.BindAttribLocation( shaderProgramHandle, i, attribs[i] );

            GL.LinkProgram( shaderProgramHandle );
            Console.WriteLine( GL.GetProgramInfoLog( shaderProgramHandle ) );
            GL.UseProgram( shaderProgramHandle );

            float aspectRatio = ClientSize.Width / (float) ( ClientSize.Height );

            //Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI / 4, aspectRatio, 1, 100, out projectionMatrix );
            projectionMatrix.set( Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI / 4, aspectRatio, 1, 100 ) );
            modelviewMatrix.set( Matrix4.LookAt( new Vector3( -8, 0, -4 ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) ) );

            uniformManager.addUniforms( 
                new UniformMatrix4f( "projectionMatrix", projectionMatrix ),
                new UniformMatrix4f( "modelviewMatrix", modelviewMatrix ),
                transformMatrixUniform = new UniformMatrix4f( "transform", transformMatrix ),
                new Uniform3f( "ambientColor", ambientColor ),
                new Uniform3f( "lightColor", lightColor ),
                new Uniform3f( "lightDirUnit", lightDirUnit ),
                new Uniform1f( "time", time ),
                new Uniform1f( "textureSamples", textureSampler )
            );
            uniformManager.init( shaderProgramHandle );
        }

        override protected  void OnUpdateFrame ( FrameEventArgs e ) {
            time.Value = ( DateTime.Now.Ticks % ( 100L * 1000 * 1000 * 1000 ) ) / 1E7f;
            //Console.WriteLine( time[0] );

            foreach ( Model m in models ) {
                m.update();
            }

            /*
            Matrix4 rotation = Matrix4.CreateRotationZ( (float) e.Time * 4 ), source = modelviewMatrix.toMatrix4(), result = new Matrix4();
            Matrix4.Mult( ref rotation, ref source, out result );
            modelviewMatrix.set( result );
            ambientColor.Y = (float) Math.Sin( time.Value );
            Console.WriteLine( ambientColor.Y );
            */

            KeyboardState keyboard = OpenTK.Input.Keyboard.GetState();
            if ( keyboard[Key.Escape] )
                Exit();
        }

        override protected  void OnRenderFrame ( FrameEventArgs e ) {
            GL.Viewport( 0, 0, Width, Height );

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            uniformManager.updateGl();

            foreach ( Model m in models ) {
                transformMatrix.set( m.Transform );
                m.render();
            }

            SwapBuffers();
        }
    }
}