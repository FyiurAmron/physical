using System;
using physical.math;
using OpenTK;

namespace physical.physics {
    public class SphereBody : Body {
        readonly float radius;

        // a nasty hack for ball rotation
        public float RotationAngle { get; set; }

        public float RotationSpeed { get; set; }

        public int RotationAxis { get; set; }

        public float RotationVelocityThreshold { get; set; }

        //Matrix4 tmpRotMatrix = new Matrix4();

        public SphereBody ( float mass, float radius ) : base( mass ) {
            this.radius = radius;
            Restitution = 0.5f;
            //Restitution = 0.75f;
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
            RotationAngle += v * deltaT * RotationSpeed;
            Transform.setScaleAndRotation( createRotationMatrix4() );
        }

        override public void checkCollision ( Body body ) {
            if ( body is SphereBody ) {
                SphereBody sb = (SphereBody) body;
                Vector3f disp = Transform.getDisplacement( body.Transform );
                float totalRadius = sb.radius + radius;
                float dist = disp.length();
                //Console.WriteLine( totalRadius + " " + dist );
                float depth = totalRadius - dist;
                if ( depth < 0 /* || depth > totalRadius */ /* <- has to be false*/ )
                    return;
                disp.normalize();
                Vector3f normal = disp;
                depth *= 0.5f;
                Transform.addTranslation( normal.getScaled( -depth ) );
                sb.Transform.addTranslation( normal.getScaled( depth ) );

                float combinedRestitution = Restitution * body.Restitution;
                Velocity.add( normal.getScaled( ( -1f - combinedRestitution ) * Velocity.dot( normal ) ) );
                sb.Velocity.add( normal.getScaled( ( -1f - combinedRestitution ) * sb.Velocity.dot( normal ) ) );
               
            } else if ( body is PlaneBody ) {
                PlaneBody pb = (PlaneBody) body;
                float dist = pb.Plane3f.getDistance( Transform );
                float depth = radius - dist;
                if ( depth < 0 || depth > radius ) // a) sphere-to-plane collision occured, b) not too far yet
                    return;
                Vector3f normal = pb.Plane3f.Normal;
                Transform.addTranslation( normal.getScaled( depth ) );
                float combinedRestitution = Restitution * body.Restitution;
                Velocity.add( normal.getScaled( ( -1f - combinedRestitution ) * Velocity.dot( normal ) ) );
            } else
                throw new ArgumentException();
        }
    }
}

