using System;
using physical.math;

namespace physical.physics {
    abstract public class CenteredBody : Body {

        public float Radius { get; set; }

        public CenteredBody ( float mass, float radius ) : base( mass ) {
            Radius = radius;
        }

        public CenteredBody ( float mass, float radius, Matrix4f initialTransform ) : base( mass, initialTransform ) {
            Radius = radius;
        }
    }
}

