/**
 * 
 *
 */

using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace physical {
    public class PhysicalWindow : GameWindow {
        HashSet<Renderable> renderables = new HashSet<Renderable>();

        string vertexShaderSource = @"
#version 140
precision highp float;
uniform mat4 projectionMatrix;
uniform mat4 modelviewMatrix;
in vec3 in_position;
in vec3 in_normal;
out vec3 normal;

void main() {
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
    out_frag_color = vec4(ambient + diffuse * lightColor, 1.0);
}";

        int vertexShaderHandle,
            fragmentShaderHandle,
            shaderProgramHandle,
            modelviewMatrixLocation,
            projectionMatrixLocation;

        Matrix4 projectionMatrix, modelviewMatrix;

        public PhysicalWindow ()
            : base( 640, 480,
                    new GraphicsMode(), "OpenGL 3 Example", 0,
                    DisplayDevice.Default, 3, 2,
                    GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug ) {
        }

        SphereModel sm;

        protected override void OnLoad ( System.EventArgs e ) {
            sm = new SphereModel( 0.5f, 10, 10, true );
            sm.init();
            //sm.writeOBJ("sphere.obj");
            renderables.Add( sm );

            VSync = VSyncMode.On;

            CreateShaders();

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

            foreach ( Renderable r in renderables ) {
                r.render();
            }

            SwapBuffers();
        }



    }
}