using System;
using physical.math;

namespace physical {
    public class Plane3f {
        public Vector3f Normal { get; set; }

        public float DistanceToOrigin { get; set; }

        public Plane3f () : this( new Vector3f( 0, 1, 0 ), 0 ) {
        }

        public Plane3f ( Vector3f normal, float distanceToOrigin ) {
            Normal = normal;
            DistanceToOrigin = distanceToOrigin;
        }

        public float getDistance ( Vector3f point ) {
            return Normal.dot( point ) - DistanceToOrigin;
        }

        public float getDistance ( Matrix4f transform ) {
            return Normal.dot( transform.TranslationX, transform.TranslationY, transform.TranslationZ ) - DistanceToOrigin;
        }
    }
}

