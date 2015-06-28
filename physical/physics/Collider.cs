using System;
using physical.math;

namespace physical.physics {

    public class ColliderDescriptor : Tuple<Type,Type> {
        public ColliderDescriptor ( Type bodyType1, Type bodyType2 ) : base( bodyType1, bodyType2 ) {
        }

        public bool checkTypes ( Body body1, Body body2 ) {
            return Item1.IsInstanceOfType( body1 ) && Item2.IsInstanceOfType( body2 );
        }
    }

    abstract public class Collider {
        /*<T1,T2> where T1 : Body where T2 : Body*/
        readonly ColliderDescriptor colliderDescriptor;

        public Collider ( Type t1, Type t2 ) {
            colliderDescriptor = new ColliderDescriptor( t1, t2 );
        }

        abstract public bool collide ( /*T1*/ Body body1, /*T2*/Body body2 );

        /*
        public void collide ( T2 body1, T1 body2 ) {
            collide( body2, body1 );
        }
        */

        public ColliderDescriptor getDescriptor () {
            return colliderDescriptor;
        }
    }

    public class SphereSphereCollider : Collider {
        public SphereSphereCollider () : base( typeof(SphereBody), typeof(SphereBody) ) {
        }

        override public bool collide ( Body body1, Body body2 ) {
            SphereBody sb1 = body1 as SphereBody, sb2 = body2 as SphereBody;
            if ( sb1 == null || sb2 == null )
                throw new InvalidOperationException();
            
            float res = sb1.Restitution * sb2.Restitution;
            Vector3f disp = sb1.Transform.getDisplacement( sb2.Transform );
            float totalRadius = sb1.Radius + sb2.Radius;
            float dist = disp.length();
            //Console.WriteLine( totalRadius + " " + dist );
            float depth = totalRadius - dist;
            if ( depth < 0 /* || depth > totalRadius */ /* <- has to be false*/ )
                return false;
            disp.normalize();
            Vector3f normal = disp;
            depth *= 0.5f;

            sb1.Transform.addTranslation( normal.getScaled( -depth ) );
            //sb2.Transform.addTranslation( normal.getScaled( depth ) );

            float v1n = sb1.Velocity.dot( normal ), v2n = sb2.Velocity.dot( normal ), v1n2, v2n2,
            vDiffRes = res * ( v1n - v2n );
            v1n2 = ( sb1.Mass * v1n + sb2.Mass * ( v2n - vDiffRes ) ) / ( sb1.Mass + sb2.Mass );
            v2n2 = vDiffRes + v1n2;

            sb1.Velocity.add( normal.getScaled( v1n2 - v1n ) );
            sb2.Velocity.add( normal.getScaled( v2n2 - v2n ) );
            return true;
        }
    }

    public class SpherePlaneCollider : Collider {
        public SpherePlaneCollider () : base( typeof(SphereBody), typeof(PlaneBody) ) {
        }

        override public bool collide ( Body body1, Body body2 ) {
            SphereBody sb = body1 as SphereBody;
            PlaneBody pb = body2 as PlaneBody;
            if ( sb == null || pb == null )
                throw new InvalidOperationException();
            float dist = pb.Plane3f.getDistance( sb.Transform );
            float depth = sb.Radius - dist;
            if ( depth < 0 || depth > sb.Radius ) // a) sphere-to-plane collision occured, b) not too far yet
                return false;
            Vector3f normal = pb.Plane3f.Normal;
            sb.Transform.addTranslation( normal.getScaled( depth ) );

            float res = sb.Restitution * pb.Restitution;
            float vn = sb.Velocity.dot( normal );
            Vector3f vTangent = new Vector3f( sb.Velocity );
            vTangent.subtract( normal.getScaled( vn ) );
            sb.Velocity.add( normal.getScaled( ( -1f - res ) * vn ) );

            Vector3f F = sb.getForce();
            float fn = F.dot( normal );
            F.subtract( normal.getScaled( fn ) ); // surface reaction
            if ( vTangent.lengthSq() > Body.KINEMATIC_EPSILON_SQ ) {
                vTangent.normalize().scale( fn * pb.Friction );
                sb.applyForce( vTangent );
            }

            return true;
        }
    }

}

