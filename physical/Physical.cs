﻿/**
 * 
 *
 */

using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace physical {
    public class PhysicalWindow : GameWindow {
        public const string APP_NAME = "Physical";

        string vertexShaderSource = @"
#version 140
precision highp float;
uniform mat4 projectionMatrix;
uniform mat4 modelviewMatrix;
in vec3 in_position;
in vec3 in_normal;
out vec3 normal;
void main() {
  //works only for orthogonal modelview
  normal = (modelviewMatrix * vec4(in_normal, 0)).xyz;
  
  gl_Position = projectionMatrix * modelviewMatrix * vec4(in_position, 1);
}";

        string fragmentShaderSource = @"
#version 140
precision highp float;
const vec3 ambient = vec3(0.1, 0.1, 0.1);
const vec3 lightVecNormalized = normalize(vec3(0.5, 0.5, 2.0));
const vec3 lightColor = vec3(0.9, 0.9, 0.7);
in vec3 normal;
out vec4 out_frag_color;
void main() {
  float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);
//float diffuse = 0;
  out_frag_color = vec4(ambient + diffuse * lightColor, 1.0);
}";

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle,
            modelviewMatrixLocation,
            projectionMatrixLocation,
            vaoHandle,
            positionVboHandle,
            normalVboHandle,
            eboHandle;

        Vector3[] positionVboData = new Vector3[] {
            new Vector3( -1.0f, -1.0f, 1.0f ),
            new Vector3( 1.0f, -1.0f, 1.0f ),
            new Vector3( 1.0f, 1.0f, 1.0f ),
            new Vector3( -1.0f, 1.0f, 1.0f ),
            new Vector3( -1.0f, -1.0f, -1.0f ),
            new Vector3( 1.0f, -1.0f, -1.0f ), 
            new Vector3( 1.0f, 1.0f, -1.0f ),
            new Vector3( -1.0f, 1.0f, -1.0f )
        };
        int[] indicesVboData;
        /*
        int[] indicesVboData = new int[] {
            // front face
            0, 1, 2, 2, 3, 0,
            // top face
            3, 2, 6, 6, 7, 3,
            // back face
            7, 6, 5, 5, 4, 7,
            // left face
            4, 0, 3, 3, 7, 4,
            // bottom face
            0, 1, 5, 5, 4, 0,
            // right face
            1, 5, 6, 6, 2, 1,
        };*/

        Matrix4 projectionMatrix, modelviewMatrix;

        public PhysicalWindow ()
            : base( 640, 480,
                    new GraphicsMode(), "OpenGL 3 Example", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        SphereModel sm;

        protected override void OnLoad ( System.EventArgs e ) {
            sm = new SphereModel(0.5f,10,10,true);

            int vCount = sm.getVertices().Length / 3;
            indicesVboData = new int[vCount];
            for ( int i = 0; i < vCount; i++ )
                indicesVboData[i] = i;

            VSync = VSyncMode.On;

            CreateShaders();
            CreateVBOs();
            CreateVAOs();

            // Other state
            GL.Enable( EnableCap.DepthTest );
            GL.ClearColor( System.Drawing.Color.MidnightBlue );
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

            GL.BindAttribLocation( shaderProgramHandle, 0, "in_position" );
            GL.BindAttribLocation( shaderProgramHandle, 1, "in_normal" );

            GL.LinkProgram( shaderProgramHandle );
            Debug.WriteLine( GL.GetProgramInfoLog( shaderProgramHandle ) );
            GL.UseProgram( shaderProgramHandle );

            // Set uniforms
            projectionMatrixLocation = GL.GetUniformLocation( shaderProgramHandle, "projectionMatrix" );
            modelviewMatrixLocation = GL.GetUniformLocation( shaderProgramHandle, "modelviewMatrix" );

            float aspectRatio = ClientSize.Width / (float) ( ClientSize.Height );
            Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI / 4, aspectRatio, 1, 100, out projectionMatrix );
            modelviewMatrix = Matrix4.LookAt( new Vector3( 0, 3, 5 ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) );

            GL.UniformMatrix4( projectionMatrixLocation, false, ref projectionMatrix );
            GL.UniformMatrix4( modelviewMatrixLocation, false, ref modelviewMatrix );
        }

        void CreateVBOs () {
            positionVboHandle = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, positionVboHandle );
            GL.BufferData<float>( BufferTarget.ArrayBuffer,
                new IntPtr( sm.getVertices().Length * Vector3.SizeInBytes ),
                sm.getVertices(), BufferUsageHint.StaticDraw );

            normalVboHandle = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, normalVboHandle );

            GL.BufferData<float>( BufferTarget.ArrayBuffer,
                new IntPtr( sm.getNormals().Length * Vector3.SizeInBytes ),
                sm.getNormals(), BufferUsageHint.StaticDraw );


            eboHandle = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, eboHandle );
            GL.BufferData( BufferTarget.ElementArrayBuffer,
                new IntPtr( sizeof(uint) * indicesVboData.Length ),
                indicesVboData, BufferUsageHint.StaticDraw );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
        }

        void CreateVAOs () {
            // GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
            // This means we do not have to re-issue VertexAttribPointer calls
            // every time we try to use a different vertex layout - these calls are
            // stored in the VAO so we simply need to bind the correct VAO.
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray( vaoHandle );

            GL.EnableVertexAttribArray( 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, positionVboHandle );
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0 );

            GL.EnableVertexAttribArray( 1 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, normalVboHandle );
            GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0 );

            GL.BindBuffer( BufferTarget.ElementArrayBuffer, eboHandle );

            GL.BindVertexArray( 0 );
        }


        protected override void OnUpdateFrame ( FrameEventArgs e ) {
            Matrix4 rotation = Matrix4.CreateRotationY( (float) e.Time );
            Matrix4.Mult( ref rotation, ref modelviewMatrix, out modelviewMatrix );
            GL.UniformMatrix4( modelviewMatrixLocation, false, ref modelviewMatrix );

            var keyboard = OpenTK.Input.Keyboard.GetState();
            if ( keyboard[OpenTK.Input.Key.Escape] )
                Exit();
        }

        protected override void OnRenderFrame ( FrameEventArgs e ) {
            GL.Viewport( 0, 0, Width, Height );

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

            GL.BindVertexArray( vaoHandle );

            GL.DrawElements( PrimitiveType.Triangles, indicesVboData.Length,
                DrawElementsType.UnsignedInt, IntPtr.Zero );

            //GL.DrawArrays( PrimitiveType.Triangles, 0, sphereData[0].Length );
            SwapBuffers();
        }

        [STAThread]
        public static void Main () {
            using ( PhysicalWindow example = new PhysicalWindow() ) {

                example.Run( 30 );
            }
        }
    }
}