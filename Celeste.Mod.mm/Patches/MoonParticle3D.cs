using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System.Collections.Generic;

namespace Celeste {
    class patch_MoonParticle3D : MoonParticle3D {
        private MountainModel model;
        private List<Particle> particles;

        public patch_MoonParticle3D(MountainModel model, Vector3 center, Color[] starColors1, Color[] starColors2) : base(model, center) {
            // dummy constructor
        }

        [MonoModLinkTo("Monocle.Entity", "System.Void .ctor()")]
        [MonoModIgnore]
        public extern void EntityCtor();

        [MonoModConstructor]
        public void ctor(MountainModel model, Vector3 center, Color[] starColors1, Color[] starColors2) {
            particles = new List<Particle>();
            EntityCtor();

            this.model = model;
            Visible = false;
            Matrix matrix = Matrix.CreateRotationZ(0.4f);
            if (starColors1.Length != 0) {
                for (int i = 0; i < 20; i++) {
                    Add(new Particle(OVR.Atlas["star"], Calc.Random.Choose(starColors1), center, 1f, matrix));
                }
                for (int i = 0; i < 30; i++) {
                    Add(new Particle(OVR.Atlas["snow"], Calc.Random.Choose(starColors1), center, 0.3f, matrix));
                }
            }
            matrix = Matrix.CreateRotationZ(0.8f) * Matrix.CreateRotationX(0.4f);
            if (starColors2.Length != 0) {
                for (int i = 0; i < 20; i++) {
                    Add(new Particle(OVR.Atlas["star"], Calc.Random.Choose(starColors2), center, 1f, matrix));
                }
                for (int i = 0; i < 30; i++) {
                    Add(new Particle(OVR.Atlas["snow"], Calc.Random.Choose(starColors2), center, 0.3f, matrix));
                }
            }
        }

        public class patch_Particle : Particle {
            public patch_Particle(MTexture texture, Color color, Vector3 center, float size, Matrix matrix)
                : base(texture, color, center, size, matrix) {
            }

            public extern void orig_ctor(MTexture texture, Color color, Vector3 center, float size, Matrix matrix);

            [MonoModConstructor]
            public void ctor(MTexture texture, Color color, Vector3 center, float size, Matrix matrix) {
                orig_ctor(texture, color, center, size, matrix);
                Spd = 1f;
                SurroundTime = Calc.Random.Choose(SurroundTimeChoice);
            }

            private static readonly float[] SurroundTimeChoice = {
                10f,
                11.25f,
                15f
            };

            public Vector3 Center;
            public Matrix Matrix;
            public float Rotation;
            public float Distance;
            public float YOff;
            public float Spd;
            public float SurroundTime;

            [MonoModReplace]
            public override void Update() {
                float num = Engine.FrameCounter * ((float)Math.PI * 2f) / SurroundTime * Engine.DeltaTime;
                Vector3 position = new Vector3((float)Math.Cos((double)Rotation + (double)num) * Distance, (float)Math.Sin((Rotation + num) * 3f) * 0.25f + YOff, (float)Math.Sin((double)Rotation + (double)num) * Distance);
                Position = Center + Vector3.Transform(position, Matrix);
            }

        }
    }
}