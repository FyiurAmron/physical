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
using physical.physics;

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
uniform float random;

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
    //gl_Position.y += random; // earthquake!
    //gl_Position.y += 4 * random; // BIG earthquake!
    //gl_Position.y *= 0.5 + 0.5 * abs(sin(time+gl_Position.x+gl_Position.z)); // wavy!
    //gl_Position.y += 2 * abs(sin(time+gl_Position.x+gl_Position.z)); // wavy 2!
}";

        string fragmentShaderSource = @"
#version 140
precision highp float;

uniform vec3 ambientColor;
uniform vec3 lightColor;
uniform vec3 lightDirUnit;
uniform float time;
uniform float random;

uniform sampler2D textureSampler;

in vec3 normal;
in vec2 uv;
out vec4 out_fragColor;

void main() {
    //vec3 diffuseColor = vec3(uv,0); // UV debugging
    //vec3 diffuseColor = texture(textureSampler, uv).rgb;
    vec3 diffuseColor = texture(textureSampler, uv).rgb * vec3(uv,0) * abs(sin(time*2));
    //vec3 shadedLightColor = lightColor;
    vec3 shadedLightColor = lightColor * clamp( dot( lightDirUnit, normal ), 0.0, 1.0 );
    out_fragColor = vec4( diffuseColor * clamp( shadedLightColor + ambientColor, 0.0, 1.0 ), 1.0 );
}";

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle;

        Random RNG = new Random();

        Value1f //
            time = new Value1f(),
            random = new Value1f(),
            textureSampler = new Value1f( 0 );
        Vector3f //
            ambientColor = new Vector3f(),
            lightColor = new Vector3f(),
            lightDirUnit = new Vector3f();
        Matrix4f //
            projectionMatrix = new Matrix4f(),
            modelviewMatrix = new Matrix4f(),
            transformMatrix = new Matrix4f( true );

        Vector3 basePosition = new Vector3();
        MouseState lastMouseState;

        UniformManager uniformManager = new UniformManager();
        BodyManager bodyManager = new BodyManager();

        public static readonly string[] attribs = { "in_position", "in_normal", "in_uv" };
        public static readonly Dictionary<string,int> uniformMap = new Dictionary<string,int>();
        public const string APP_NAME = "Physical";
        const int SIZE_X = 800, SIZE_Y = 600;

        const float BALL_RADIUS = 1.0f, JUMP_HEIGHT = 10f, MIN_RADIUS = 10f, BASE_VIEW_HEIGHT = 10f, WORLD_MOVE_FRAME_DELTA = 0.5f;

        UniformMatrix4f transformMatrixUniform;

        Action<Matrix4f> cameraOnFrame;

        public PhysicalWindow ()
            : base( SIZE_X, SIZE_Y,
                    new GraphicsMode(), APP_NAME + " v0.1", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        override protected  void OnLoad ( System.EventArgs e ) {
            lastMouseState = OpenTK.Input.Mouse.GetState();

            Texture angrySquirrelTexture, angryTurtleTexture;
            Texture[] worldTextures;
            Model sphereModel;
            RectangleModel[] worldModels;

            float aspectRatio = ClientSize.Width / (float) ( ClientSize.Height );

            projectionMatrix.set( Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI / 4, aspectRatio, 1, 1000 ) );

            cameraOnFrame = delegate( Matrix4f mvMatrix ) {
                float radius = 40, y = 20, timeRatio = 0.5f;
                Vector3 pos = new Vector3(
                                  (float) Math.Sin( time.Value * timeRatio ) * radius,
                                  y,
                                  (float) Math.Cos( time.Value * timeRatio ) * radius );
                mvMatrix.set( Matrix4.LookAt( pos, new Vector3( 0, y * 0.5f * ( 1.5f + (float) Math.Sin( time.Value * timeRatio ) ), 0 ), new Vector3( 0, 1, 0 ) ) );
            };
            cameraOnFrame( modelviewMatrix );
            cameraOnFrame = null; // enable/disable

            ambientColor.set( 0.4f, 0.4f, 0.4f );
            lightColor.set( 1.0f, 1.0f, 1.0f );
            lightDirUnit.set( 0.5f, 1.0f, 0.5f );
            //lightDirUnit.set( 0.1f, 5.0f, 0.1f );
            lightDirUnit.normalize();
            //texture = new Texture( "E:\\drop\\logo-dark.jpg" );
            angrySquirrelTexture = new Texture( "gfx/angry-squirrel.png" );
            angryTurtleTexture = new Texture( "gfx/angry-turtle.png" );

            worldTextures = new Texture[] { 
                new Texture( "gfx/drzewka-1.png" ),
                new Texture( "gfx/drzewka-3.png" ),
                new Texture( "gfx/sky.png" ),
                new Texture( "gfx/grass.png" ),
                new Texture( "gfx/drzewka-2.png" ),
                new Texture( "gfx/drzewka-4.png" )
            };

            CreateShaders();

            sphereModel = new SphereModel( BALL_RADIUS * 5, 10, 10, true );
            sphereModel.Texture = angryTurtleTexture;
            //sphereModel.Transform.setIdentity(); // now made auto
            //sm = new SphereModel( 1.5f, 4, 2, true );

            sphereModel.UpdateAction = delegate( Model model ) {
                Matrix4f transform = model.Transform;
                transform.set( Matrix4.CreateRotationX( -time.Value ) );
                //transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * 0.5f * (1 + (float) Math.Sin( time.Value )) ); // hover
                transform.setTranslation( -10, BALL_RADIUS * 5, 5 ); // roll
            };
            sphereModel.UpdateAction( sphereModel );

            //sm.writeOBJ();
            models.Add( sphereModel );

            sphereModel = new SphereModel( BALL_RADIUS, 20, 20, true );
            sphereModel.Texture = angrySquirrelTexture;

            sphereModel.UpdateAction = delegate( Model model ) {
                Matrix4f transform = model.Transform;
                transform.set( Matrix4.CreateRotationZ( time.Value ) );
                //transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * 0.5f * (1 + (float) Math.Sin( time.Value )) ); // hover
                transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * (float) Math.Abs( Math.Sin( time.Value ) ) ); // hover
            };
            sphereModel.UpdateAction( sphereModel );
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
                new Uniform1f( "random", random ),
                new Uniform1f( "textureSamples", textureSampler )
            );
            uniformManager.init( shaderProgramHandle );
        }

        override protected  void OnUpdateFrame ( FrameEventArgs e ) {
            MouseState currentMouseState = OpenTK.Input.Mouse.GetState();
            int deltaX, deltaY, deltaZ;
            if ( currentMouseState != lastMouseState ) {
                deltaX = currentMouseState.X - lastMouseState.X;
                deltaY = currentMouseState.Y - lastMouseState.Y;
                deltaZ = currentMouseState.Wheel - lastMouseState.Wheel;
                lastMouseState = currentMouseState;
            } else {
                deltaX = 0;
                deltaY = 0;
                deltaZ = 0;
            }

            KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();
            if ( keyboardState.IsKeyDown( Key.W ) ) {
                basePosition.X += WORLD_MOVE_FRAME_DELTA;
            }
            if ( keyboardState.IsKeyDown( Key.S ) ) {
                basePosition.X -= WORLD_MOVE_FRAME_DELTA;
            }
            if ( keyboardState.IsKeyDown( Key.A ) ) {
                basePosition.Z += WORLD_MOVE_FRAME_DELTA;
            }
            if ( keyboardState.IsKeyDown( Key.D ) ) {
                basePosition.Z -= WORLD_MOVE_FRAME_DELTA;
            }
            if ( keyboardState.IsKeyDown( Key.LShift ) ) {
                basePosition.Y += WORLD_MOVE_FRAME_DELTA;
            }
            if ( keyboardState.IsKeyDown( Key.LControl ) ) {
                basePosition.Y -= WORLD_MOVE_FRAME_DELTA;
            }

            time.Value = ( DateTime.Now.Ticks % ( 100L * 1000 * 1000 * 1000 ) ) / 1E7f;
            random.Value = (float) RNG.NextDouble();
            //Console.WriteLine( time[0] );

            if ( cameraOnFrame != null )
                cameraOnFrame( modelviewMatrix );
            else {
                float radius = currentMouseState.Wheel + MIN_RADIUS;

                Vector3 pos = new Vector3(
                                  basePosition.X + (float) Math.Sin( -2 * Math.PI / SIZE_X * currentMouseState.X ) * radius,
                                  basePosition.Y + BASE_VIEW_HEIGHT,
                                  basePosition.Z + (float) Math.Cos( -2 * Math.PI / SIZE_X * currentMouseState.X ) * radius
                              );
                //Console.WriteLine( pos );
                modelviewMatrix.set( Matrix4.LookAt( pos,
                    new Vector3( basePosition.X, BASE_VIEW_HEIGHT + basePosition.Y - radius * currentMouseState.Y / SIZE_Y, basePosition.Z ),
                    new Vector3( 0, 1, 0 ) ) );

                //modelviewMatrix.setTranslation( deltaX, deltaY, deltaZ );
            }
            
            foreach ( Model m in models ) {
                m.update();
            }

            bodyManager.update();

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