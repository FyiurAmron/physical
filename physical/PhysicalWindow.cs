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
using physical.model;

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
    normal = (transform * vec4(in_normal, 0)).xyz;
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
    //vec3 shadedLightColor = lightColor;
    vec3 shadedLightColor = lightColor * clamp( dot( lightDirUnit, normal ), 0.0, 1.0 );
    out_fragColor = vec4( diffuseColor * clamp( shadedLightColor + ambientColor, 0.0, 1.0 ), 1.0 );
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

        Texture angrySquirrelTexture;
        Texture[] worldTextures;
        Model sphereModel;
        RectangleModel[] worldModels;
        UniformManager uniformManager = new UniformManager();

        public static readonly String[] attribs = { "in_position", "in_normal", "in_uv" };
        public static readonly Dictionary<string,int> uniformMap = new Dictionary<string,int>();
        public const string APP_NAME = "Physical";

        const float BALL_RADIUS = 1.0f, JUMP_HEIGHT = 10f;

        UniformMatrix4f transformMatrixUniform;

        Action<Matrix4f> cameraOnFrame;

        public PhysicalWindow ()
            : base( 800, 600,
                    new GraphicsMode(), APP_NAME + " v0.0", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        override protected  void OnLoad ( System.EventArgs e ) {
            float aspectRatio = ClientSize.Width / (float) ( ClientSize.Height );

            projectionMatrix.set( Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI / 4, aspectRatio, 1, 1000 ) );

            cameraOnFrame = delegate( Matrix4f mvMatrix ) {
                float radius = 40, y = 20;
                Vector3 pos = new Vector3(
                                  (float) Math.Sin( time.Value ) * radius,
                                  y,
                                  (float) Math.Cos( time.Value ) * radius );
                mvMatrix.set( Matrix4.LookAt( pos, new Vector3( 0, y * 0.5f * ( 2 + (float) Math.Sin( time.Value ) ), 0 ), new Vector3( 0, 1, 0 ) ) );
            };
            cameraOnFrame( modelviewMatrix );
            //cameraOnFrame = null; // enable/disable

            ambientColor.set( 0.4f, 0.4f, 0.4f );
            lightColor.set( 1.0f, 1.0f, 1.0f );
            lightDirUnit.set( 0.5f, 1.0f, 0.5f );
            //lightDirUnit.set( 0.1f, 5.0f, 0.1f );
            lightDirUnit.normalize();
            //texture = new Texture( "E:\\drop\\logo-dark.jpg" );
            angrySquirrelTexture = new Texture( "angry-squirrel.png" );

            worldTextures = new Texture[] { 
                new Texture( "drzewka-1.png" ),
                new Texture( "drzewka-3.png" ),
                new Texture( "sky.png" ),
                new Texture( "grass.png" ),
                new Texture( "drzewka-2.png" ),
                new Texture( "drzewka-4.png" )
            };

            CreateShaders();

            sphereModel = new SphereModel( BALL_RADIUS, 10, 10, true );
            //sphereModel.Transform.setIdentity(); // now made auto
            //sm = new SphereModel( 1.5f, 4, 2, true );

            sphereModel.UpdateAction = delegate( Model model ) {
                Matrix4f transform = model.Transform;
                transform.set( Matrix4.CreateRotationZ( time.Value ) );
                //transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * 0.5f * (1 + (float) Math.Sin( time.Value )) ); // hover
                transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * (float) Math.Abs( Math.Sin( time.Value ) ) ); // hover
            };
            sphereModel.Transform.setTranslation( 10, 0, 10 );
            sphereModel.Texture = angrySquirrelTexture;
            //sm.writeOBJ();
            models.Add( sphereModel );

            float boxX = 100, boxY = 50, boxZ = 100, shiftX = 0.5f * boxX, shiftY = 0.5f * boxY, shiftZ = 0.5f * boxZ;
            worldModels = new RectangleModel[] {
                new RectangleModel( Model.OZ, boxX, boxY ),
                new RectangleModel( Model.OZ, boxX, -boxY ),
                new RectangleModel( Model.OY, boxX, boxZ ),
                new RectangleModel( Model.OY, -boxX, boxZ ),
                new RectangleModel( Model.OZ, -boxX, boxY ),
                new RectangleModel( Model.OZ, boxX, boxY )
            };
            for ( int i = worldModels.Length - 1; i >= 0; i-- ) {
                Model m = worldModels[i];
                m.Texture = worldTextures[i];
                models.Add( m );
            }
            Matrix4f trans;
            trans = worldModels[0].Transform;
            trans.setZero();
            trans.Data[2] = 1;
            trans.Data[5] = 1;
            trans.Data[8] = 1;
            trans.Data[15] = 1;
            trans.setTranslation( shiftX, shiftY, 0 );
            trans = worldModels[1].Transform;
            trans.setZero();
            trans.Data[2] = -1;
            trans.Data[5] = -1;
            trans.Data[8] = -1;
            trans.Data[15] = 1;
            worldModels[1].Transform.setTranslation( -shiftX, shiftY, 0 );
            worldModels[2].Transform.setTranslation( 0, boxY, 0 );
            worldModels[3].Transform.setTranslation( 0, 0, 0 );
            worldModels[4].Transform.setTranslation( 0, shiftY, shiftZ );
            worldModels[5].Transform.setTranslation( 0, shiftY, -shiftZ );

            foreach ( Model m in models ) {
                m.init();
            }

            VSync = VSyncMode.On;
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

            if ( cameraOnFrame != null )
                cameraOnFrame( modelviewMatrix );
            
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
            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.Viewport( 0, 0, Width, Height );

            GL.ClearColor( System.Drawing.Color.DarkOliveGreen );
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            uniformManager.updateGl();

            foreach ( Model m in models ) {
                transformMatrix.set( m.Transform );
                uniformManager.updateGl( transformMatrixUniform );
                m.render();
            }

            SwapBuffers();

            ErrorCode errorCode = GL.GetError();
            if ( errorCode != ErrorCode.NoError )
                Console.WriteLine( errorCode );
        }
    }
}