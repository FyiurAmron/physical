using System;
using physical.math;
using OpenTK;

namespace physical.physics {
    public class SphereBody : CenteredBody {
        // a nasty hack for ball rotation
        public float RotationAngle { get; set; }

        public float RotationSpeed { get; set; }

        public int RotationAxis { get; set; }

        public float RotationVelocityThreshold { get; set; }

        //Matrix4 tmpRotMatrix = new Matrix4();

        public SphereBody ( float mass, float radius ) : base( mass, radius ) {
            Restitution = 0.75f;
            RotationVelocityThreshold = 0.15f;
        }

        Matrix4 createRotationMatrix4 () {
            switch ( RotationAxis ) {
            case Vector3f.OX:
                return Matrix4.CreateRotationX( RotationAngle );
            case Vector3f.OY:
                return Matrix4.CreateRotationY( RotationAngle );
            case Vector3f.OZ:
                return Matrix4.CreateRotationZ( RotationAngle );
            }
            throw new ArgumentException();
        }

        override public void timeStep ( float deltaT ) {
            base.timeStep( deltaT );
            float v = Velocity.length();
            if ( v < RotationVelocityThreshold )
                return;
            //Console.WriteLine( Velocity.length() );
            /*
            RotationAngle += v * deltaT * RotationSpeed;
            Transform.setScaleAndRotation( createRotationMatrix4() );
            */
        }
    }
}

