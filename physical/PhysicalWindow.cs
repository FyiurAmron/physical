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
        HashSet<Mesh> meshes = new HashSet<Mesh>();

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
    vec3 diffuseColor = texture(textureSampler, uv).rgb;
    //vec3 diffuseColor = texture(textureSampler, uv).rgb * vec3(uv,0) * abs(sin(time*2)); // Groovy Disco TM
    //vec3 shadedLightColor = lightColor;
    vec3 shadedLightColor = lightColor * clamp( dot( lightDirUnit, normal ), 0.0, 1.0 );
    out_fragColor = vec4( diffuseColor * clamp( shadedLightColor + ambientColor, 0.0, 1.0 ), 1.0 );
}";

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle;

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
        KeyboardState lastKeyboardState;

        UniformManager uniformManager = new UniformManager();
        BodyManager bodyManager = new BodyManager();

        public static readonly string[] attribs = { "in_position", "in_normal", "in_uv" };
        public static readonly Dictionary<string,int> uniformMap = new Dictionary<string,int>();
        public const string APP_NAME = "Physical";
        const int SIZE_X = 800, SIZE_Y = 600;

        const float BALL_RADIUS = 1.0f, JUMP_HEIGHT = 10f, MIN_RADIUS = 10f, BASE_VIEW_HEIGHT = 10f, WORLD_MOVE_FRAME_DELTA = 0.5f;

        UniformMatrix4f transformMatrixUniform;

        Action<Matrix4f> cameraOnFrame;

        SphereBody squirrelBody, turtleBody, dilloBody;
        PlaneBody[] planeBodies;

        public PhysicalWindow ()
            : base( SIZE_X, SIZE_Y,
                    new GraphicsMode(), APP_NAME + " v0.1", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        override protected  void OnLoad ( System.EventArgs e ) {
            lastMouseState = OpenTK.Input.Mouse.GetState();

            Texture angrySquirrelTexture, angryTurtleTexture, angryDilloTexture;
            Texture[] worldTextures;
            Mesh squirrelMesh, turtleMesh, dilloMesh;
            RectangleMesh[] worldMeshes;

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
            angryDilloTexture = new Texture( "gfx/angry-armadillo.png" );

            worldTextures = new Texture[] { 
                new Texture( "gfx/drzewka-1.png" ),
                new Texture( "gfx/drzewka-3.png" ),
                new Texture( "gfx/sky.png" ),
                new Texture( "gfx/grass.png" ),
                new Texture( "gfx/drzewka-2.png" ),
                new Texture( "gfx/drzewka-4.png" )
            };

            CreateShaders();

            squirrelMesh = new SphereMesh( BALL_RADIUS, 20, 20, true );
            squirrelMesh.Texture = angrySquirrelTexture;
            /*
            squirrelMesh.UpdateAction = delegate( Mesh model ) {
                Matrix4f transform = model.Transform;
                transform.setScaleAndRotation( Matrix4.CreateRotationZ( time.Value ) );
                //transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * 0.5f * (1 + (float) Math.Sin( time.Value )) ); // hover
                //transform.setTranslationY( BALL_RADIUS + JUMP_HEIGHT * (float) Math.Abs( Math.Sin( time.Value ) ) ); // hover
            };
            squirrelMesh.UpdateAction( squirrelMesh );
            */
            //sm.writeOBJ();
            meshes.Add( squirrelMesh );

            turtleMesh = new SphereMesh( BALL_RADIUS * 5, 10, 10, true );
            turtleMesh.Texture = angryTurtleTexture;
            /*
            turtleMesh.UpdateAction = delegate( Mesh model ) {
                Matrix4f transform = model.Transform;
                transform.setScaleAndRotation( Matrix4.CreateRotationX( -time.Value ) );
            };
            turtleMesh.UpdateAction( turtleMesh );
            */
            meshes.Add( turtleMesh );

            dilloMesh = new SphereMesh( BALL_RADIUS * 2, 20, 20, true );
            dilloMesh.Texture = angryDilloTexture;
            /*
            dilloMesh.UpdateAction = delegate( Mesh model ) {
                Matrix4f transform = model.Transform;
                transform.setScaleAndRotation( Matrix4.CreateRotationX( -time.Value ) );
            };
            dilloMesh.UpdateAction( dilloMesh );
            */
            meshes.Add( dilloMesh );

            float boxX = 100, boxY = 50, boxZ = 100, shiftX = 0.5f * boxX, shiftY = 0.5f * boxY, shiftZ = 0.5f * boxZ;
            worldMeshes = new RectangleMesh[] {
                new RectangleMesh( Vector3f.OZ, boxX, boxY ),
                new RectangleMesh( Vector3f.OZ, boxX, -boxY ),
                new RectangleMesh( Vector3f.OY, boxX, boxZ ),
                new RectangleMesh( Vector3f.OY, -boxX, boxZ ),
                new RectangleMesh( Vector3f.OZ, -boxX, boxY ),
                new RectangleMesh( Vector3f.OZ, boxX, boxY )
            };
            for ( int i = worldMeshes.Length - 1; i >= 0; i-- ) {
                Mesh m = worldMeshes[i];
                m.Texture = worldTextures[i];
                meshes.Add( m );
            }
            Matrix4f trans;
            trans = worldMeshes[0].Transform;
            trans.setZero();
            trans.Data[2] = 1;
            trans.Data[5] = 1;
            trans.Data[8] = 1;
            trans.Data[15] = 1;
            trans.setTranslation( shiftX, shiftY, 0 );
            trans = worldMeshes[1].Transform;
            trans.setZero();
            trans.Data[2] = -1;
            trans.Data[5] = -1;
            trans.Data[8] = -1;
            trans.Data[15] = 1;
            worldMeshes[1].Transform.setTranslation( -shiftX, shiftY, 0 );
            worldMeshes[2].Transform.setTranslation( 0, boxY, 0 );
            worldMeshes[3].Transform.setTranslation( 0, 0, 0 );
            worldMeshes[4].Transform.setTranslation( 0, shiftY, shiftZ );
            worldMeshes[5].Transform.setTranslation( 0, shiftY, -shiftZ );

            foreach ( Mesh m in meshes ) {
                m.init();
            }

            //bodyManager.Gravity.setZero();
            /*SphereBody*/
            squirrelBody = new SphereBody( 1, BALL_RADIUS );
            squirrelBody.Transform.setTranslation( 40, 40, 40 );
            squirrelBody.RotationSpeed = 1f;
            /*SphereBody*/
            turtleBody = new SphereBody( 25, BALL_RADIUS * 5 );
            turtleBody.Transform.setTranslation( 0, 20, 0 );
            //turtleBody.Velocity.Y = 5;
            turtleBody.RotationSpeed = 1f;
            /*SphereBody*/
            dilloBody = new SphereBody( 25, BALL_RADIUS * 2 );
            //dilloBody.Transform.setTranslation( -20, 100, -30 );
            dilloBody.Transform.setTranslation( 0, 30, 0 );
            //dilloBody.Velocity.Y = -5;
            dilloBody.RotationSpeed = 1f;
            Vector3f fixPoint = new Vector3f( 0, 42, 9 );
            /*
             // spring - vertical harmonic oscillator
            dilloBody.Forces.Add( delegate( Body obj ) {
                Vector3f disp = obj.Transform.getDisplacement( fixPoint );
                float k = 20.0f;
                disp.scale( k );
                obj.applyForce( disp );
            } );
            */

            // springy pendulum - 3D harmonic oscillator
            dilloBody.Forces.Add( delegate( Body obj ) {
                Vector3f disp = obj.Transform.getDisplacement( fixPoint );
                float k = 10.0f, l = 15.0f;
                disp.scale( k * ( disp.length() - l ) );
                obj.applyForce( disp );
            } );

            planeBodies = new PlaneBody[] {
                new PlaneBody( new Plane3f( new Vector3f( 0, 1, 0 ), 0 ) ),
                new PlaneBody( new Plane3f( new Vector3f( 0, -1, 0 ), boxY ) ),
                new PlaneBody( new Plane3f( new Vector3f( 1, 0, 0 ), shiftX ) ),
                new PlaneBody( new Plane3f( new Vector3f( -1, 0, 0 ), shiftX ) ),
                new PlaneBody( new Plane3f( new Vector3f( 0, 0, 1 ), shiftZ ) ),
                new PlaneBody( new Plane3f( new Vector3f( 0, 0, -1 ), shiftZ ) )
            };

            foreach ( PlaneBody pb in planeBodies ) {
                pb.Friction = 0.1f;
                bodyManager.addBody( pb );
            }
            bodyManager.addBody( squirrelBody, squirrelMesh );
            bodyManager.addBody( turtleBody, turtleMesh );
            bodyManager.addBody( dilloBody, dilloMesh );

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

        override protected void OnUpdateFrame ( FrameEventArgs e ) {
            KeyboardState currentKeyboardState = OpenTK.Input.Keyboard.GetState();
            if ( currentKeyboardState[Key.Escape] )
                Exit();

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

            float
            xFactor = (float) Math.Sin( -2 * Math.PI / SIZE_X * currentMouseState.X ),
            zFactor = (float) Math.Cos( -2 * Math.PI / SIZE_X * currentMouseState.X );

            if ( currentKeyboardState[Key.W] ) {
                basePosition.X -= WORLD_MOVE_FRAME_DELTA * xFactor;
                basePosition.Z -= WORLD_MOVE_FRAME_DELTA * zFactor;
            }
            if ( currentKeyboardState[Key.S] ) {
                basePosition.X += WORLD_MOVE_FRAME_DELTA * xFactor;
                basePosition.Z += WORLD_MOVE_FRAME_DELTA * zFactor;
            }
            if ( currentKeyboardState[Key.A] ) {
                basePosition.X -= WORLD_MOVE_FRAME_DELTA * zFactor;
                basePosition.Z += WORLD_MOVE_FRAME_DELTA * xFactor;
            }
            if ( currentKeyboardState[Key.D] ) {
                basePosition.X += WORLD_MOVE_FRAME_DELTA * zFactor;
                basePosition.Z -= WORLD_MOVE_FRAME_DELTA * xFactor;
            }
            if ( currentKeyboardState[Key.LShift] ) {
                basePosition.Y += WORLD_MOVE_FRAME_DELTA;
            }
            if ( currentKeyboardState[Key.LControl] ) {
                basePosition.Y -= WORLD_MOVE_FRAME_DELTA;
            }

            if ( currentKeyboardState[Key.G] && !lastKeyboardState[Key.G] ) {
                bodyManager.Gravity.invert();
            }
            if ( currentKeyboardState[Key.R] && !lastKeyboardState[Key.R] ) {
                bodyManager.Gravity.rotate( 1 );
            }
            if ( currentKeyboardState[Key.BackSlash] && !lastKeyboardState[Key.BackSlash] ) {
                squirrelBody.Restitution = 1 - squirrelBody.Restitution;
                dilloBody.Restitution = 1 - dilloBody.Restitution;
                turtleBody.Restitution = 1 - turtleBody.Restitution;
            }

            if ( currentKeyboardState[Key.Number1] ) {
                //squirrelBody.applyImpulse( Vector3f.getRandom( -10, 10 ) );
                squirrelBody.Velocity.add( Vector3f.getRandom( -10, 10 ) );
            }
            if ( currentKeyboardState[Key.Number2] ) {
                //dilloBody.applyImpulse( Vector3f.getRandom( -10, 10 ) );
                dilloBody.Velocity.add( Vector3f.getRandom( -10, 10 ) );
            }
            if ( currentKeyboardState[Key.Number3] ) {
                //turtleBody.applyImpulse( Vector3f.getRandom( -10, 10 ) );
                turtleBody.Velocity.add( Vector3f.getRandom( -10, 10 ) );
            }

            if ( currentKeyboardState[Key.Enter] ) {
                squirrelBody.Transform.setTranslation( 45, BALL_RADIUS, 45 );
                squirrelBody.Velocity.set( -15, 0, -15 );
            }

            lastKeyboardState = currentKeyboardState;

            time.Value = ( DateTime.Now.Ticks % ( 100L * 1000 * 1000 * 1000 ) ) / 1E7f;
            random.Value = MathUtils.nextFloat();
            //Console.WriteLine( time[0] );

            if ( cameraOnFrame != null )
                cameraOnFrame( modelviewMatrix );
            else {
                float radius = currentMouseState.Wheel + MIN_RADIUS;

                Vector3 pos = new Vector3(
                                  basePosition.X + xFactor * radius,
                                  basePosition.Y + BASE_VIEW_HEIGHT,
                                  basePosition.Z + zFactor * radius
                              );
                //Console.WriteLine( pos );

                modelviewMatrix.set( Matrix4.LookAt( pos,
                    new Vector3( basePosition.X, BASE_VIEW_HEIGHT + basePosition.Y - radius * currentMouseState.Y / SIZE_Y, basePosition.Z ),
                    new Vector3( 0, 1, 0 ) ) );

                //modelviewMatrix.setTranslation( deltaX, deltaY, deltaZ );
            }
            
            foreach ( Mesh m in meshes ) {
                m.update();
            }

            if ( !currentKeyboardState.IsKeyDown( Key.LAlt ) ) {
                float TIME_THRESHOLD = 1f / 60f, stepTime = (float) e.Time;
                while ( stepTime > TIME_THRESHOLD ) {
                    bodyManager.update( TIME_THRESHOLD );
                    stepTime -= TIME_THRESHOLD;
                }
                bodyManager.update( stepTime );
            }

            /*
            ambientColor.Y = (float) Math.Sin( time.Value );
            Console.WriteLine( ambientColor.Y );
            */
        }

        override protected void OnRenderFrame ( FrameEventArgs e ) {
            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.Viewport( 0, 0, Width, Height );

            GL.ClearColor( System.Drawing.Color.DarkOliveGreen );
            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            uniformManager.updateGl();

            foreach ( Mesh m in meshes ) {
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