#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Monocle;
using MonoMod;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace Celeste {
    class patch_Snow3D : Snow3D {
        public patch_Snow3D(MountainModel model) : base(model) {
        }

        private static Color[] alphas;
        private List<Particle> particles;
        private BoundingFrustum Frustum;
        private BoundingFrustum LastFrustum;
        private MountainModel Model;
        private float Range;

        public List<Particle> Particles => particles;

        private static readonly int randomSeed = "wegfan".GetHashCode();
        private static Random random;

        public static long FrameCount { get; private set; }

        private const int particleCount = 1600;

        [MonoModLinkTo("Monocle.Entity", "System.Void .ctor()")]
        [MonoModIgnore]
        public extern void base_ctor();

        [MonoModConstructor]
        [MonoModReplace]
        public void ctor(MountainModel model) {
            base_ctor();
            particles = new List<Particle>();
            Frustum = new BoundingFrustum(Matrix.Identity);
            LastFrustum = new BoundingFrustum(Matrix.Identity);
            Range = 30f;
            Model = model;
            random = new Random(randomSeed);
            for (int i = 0; i < alphas.Length; i++) {
                alphas[i] = Color.White * ((float)i / alphas.Length);
            }
            for (int i = 0; i < particleCount; i++) {
                patch_Particle particle = new patch_Particle(this, 1f);
                particles.Add(particle);
                Add(particle);
            }
            FrameCount = -900;
        }

        public extern void orig_Update();

        public override void Update() {
            FrameCount++;
            if (FrameCount > patch_MountainRenderer.RotateDuration / Engine.DeltaTime) {
                FrameCount = 0L;
            }
            if (FrameCount == 0L) {
                random = new Random(randomSeed);
            }
            orig_Update();
        }

        public class patch_Particle : Particle {
            public patch_Particle(Snow3D manager, float size) : base(manager, size) {
            }

            [MonoModLinkTo("Celeste.Billboard", "System.Void .ctor(Monocle.MTexture,Microsoft.Xna.Framework.Vector3,System.Nullable`1<Microsoft.Xna.Framework.Vector2>,System.Nullable`1<Microsoft.Xna.Framework.Color>,System.Nullable`1<Microsoft.Xna.Framework.Vector2>)")]
            [MonoModIgnore]
            public extern void base_ctor(MTexture texture, Vector3 position, Vector2? size = null, Color? color = null, Vector2? scale = null);

            [MonoModConstructor]
            public void ctor(patch_Snow3D manager, float size) {
                base_ctor(OVR.Atlas["snow"], Vector3.Zero);
                Manager = manager;
                this.size = size;
                Size = Vector2.One * size;
                generatedKeyframes = new List<Keyframe>();
                frame = 0;
                keyframeIndex = 0;
                Random = new Random(random.Next());
                Generate();
            }

            public patch_Snow3D Manager;
            public Vector2 Float;
            public float Duration;
            public float Speed;
            private float size;
            public Random Random;

            public float Percent => (float)frame / generatedKeyframes[keyframeIndex].DurationFrames;

            [MonoModReplace]
            private bool InView() {
                return true;
            }

            [MonoModReplace]
            private bool InLastView() {
                return true;
            }

            [MonoModReplace]
            public new void ResetPosition() {
                return;
                float angle = random.Range(0f, (float)Math.PI * 2f);
                // float distance = (float)Math.Sqrt(random.Range(0f, ((patch_MountainModel)Manager.Model).TargetState == 3 ? 70f : 140f));
                float distance = (float)Math.Sqrt(random.Range(0f, 600f));
                Position = new Vector3(distance * (float)Math.Cos(angle), random.Range(0f, 30f), distance * (float)Math.Sin(angle));
            }

            [MonoModReplace]
            public new void Reset(float percent = 0f) {
                return;
                float num = Manager.Range / 30f;
                Speed = random.Range(1f, 6f) * num;
                frame = (int)Math.Round(generatedKeyframes[keyframeIndex].DurationFrames * percent);
                Duration = random.Range(3f, 10f);
                Float = new Vector2(random.Range(-1, 1), random.Range(-1, 1)).SafeNormalize() * 0.25f;
                Scale = Vector2.One * 0.05f * num;
            }

            [MonoModReplace]
            public override void Update() {
                frame++;
                if (frame < 0) {
                    return;
                }
                Scale = Vector2.One * 0.05f;
                if (frame >= generatedKeyframes[keyframeIndex].DurationFrames) {
                    keyframeIndex = (keyframeIndex + 1) % generatedKeyframes.Count;
                    frame = 0;
                    Keyframe keyframe = generatedKeyframes[keyframeIndex];
                    Position = keyframe.Position;
                    Duration = keyframe.Duration;
                    Speed = keyframe.Speed;
                    Float = keyframe.Float;
                    Color = Color.White * 0f;
                    return;
                }
                // Percent += Engine.DeltaTime / Duration;
                float num2 = Calc.YoYo(Percent);
                if (Manager.Model.SnowForceFloat > 0f) {
                    num2 *= Manager.Model.SnowForceFloat;
                } else if (Manager.Model.StarEase > 0f) {
                    num2 *= Calc.Map(Manager.Model.StarEase, 0f, 1f, 1f, 0f);
                }
                Color = Color.White * num2;
                Size.Y = size + Manager.Model.SnowStretch * (1f - Manager.Model.SnowForceFloat);
                Position.Y -= (Speed + Manager.Model.SnowSpeedAddition) * (1f - Manager.Model.SnowForceFloat) * Engine.DeltaTime;
                Position.X += Float.X * Engine.DeltaTime;
                Position.Z += Float.Y * Engine.DeltaTime;
            }

            public string StatusString =>
                $"Pos: {Position.ToFixedString()}, Float: {Float.ToFixedString()}, " +
                $"Speed: {Speed:F3}, Progress: {frame}/{generatedKeyframes[keyframeIndex].DurationFrames}({Percent:F3})\n" +
                $"- Keyframe: [{keyframeIndex}/{generatedKeyframes.Count}] {generatedKeyframes[keyframeIndex]}";

            private class Keyframe {
                public Vector3 Position;
                public Vector2 Float;

                public float Duration => DurationFrames * Engine.DeltaTime;

                public int DurationFrames;
                public float Speed;

                public override string ToString() {
                    return $"Position: {Position.ToFixedString()}, Float: {Float.ToFixedString()}, " +
                        $"Speed: {Speed:F3}, Duration: {DurationFrames}({Duration:F3})";
                }
            }

            private List<Keyframe> generatedKeyframes;
            private int frame;
            private int keyframeIndex;

            public void Generate() {
                generatedKeyframes.Clear();
                int totalDurationFrames = (int)Math.Round(patch_MountainRenderer.RotateDuration / Engine.DeltaTime);
                int framesLeft = totalDurationFrames;
                int minDurationFrames = (int)Math.Round(3 / Engine.DeltaTime);
                int maxDurationFrames = (int)Math.Round(10 / Engine.DeltaTime);

                while (framesLeft >= minDurationFrames) {
                    Keyframe keyframe = new Keyframe {
                        Position = ((Func<Vector3>)(() => {
                            float angle = Random.Range(0f, (float)Math.PI * 2f);
                            float distance = (float)Math.Sqrt(Random.Range(0f, 400f));
                            return new Vector3(distance * (float)Math.Cos(angle), Random.Range(0f, 30f), distance * (float)Math.Sin(angle));
                        })).Invoke(),
                        DurationFrames = Random.Next(minDurationFrames, maxDurationFrames),
                        Float = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).SafeNormalize() * 0.25f,
                        Speed = Random.Range(1f, 6f)
                    };
                    generatedKeyframes.Add(keyframe);
                    framesLeft -= keyframe.DurationFrames;
                }
                // Console.WriteLine(framesLeft);
                while (framesLeft != 0) {
                    List<Keyframe> list = generatedKeyframes;
                    int frames = Random.Next(1, Math.Max(1, Math.Abs(framesLeft) / 3));
                    if (framesLeft > 0) {
                        list = list.Where(keyframe => keyframe.DurationFrames <= maxDurationFrames - frames)
                            .ToList();
                    } else {
                        list = list.Where(keyframe => keyframe.DurationFrames >= minDurationFrames + frames)
                            .ToList();
                    }
                    // Console.WriteLine($"{framesLeft}, {frames}");
                    // Console.WriteLine(string.Join(", ",
                    //     list
                    //         .OrderBy(i => i.DurationFrames)
                    //         .Select(i => $"{i.DurationFrames}/{i.Duration:F3}")));
                    Keyframe keyframe = Random.Choose(list);
                    keyframe.DurationFrames += Math.Sign(framesLeft) * frames;
                    framesLeft -= Math.Sign(framesLeft) * frames;
                }
                Debug.Assert(generatedKeyframes.Sum(i => i.DurationFrames) == 90 * 60);
                frame = -Random.Next(60, 600);
            }
        }
    }
}