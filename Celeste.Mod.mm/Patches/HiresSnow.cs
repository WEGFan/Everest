#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_HiresSnow : HiresSnow {

        public patch_HiresSnow(float overlayAlpha = 0.45f)
            : base(overlayAlpha) {
            // no-op.
        }

        public float Alpha;
        public float ParticleAlpha;
        public Vector2 Direction;
        public ScreenWipe AttachAlphaTo;
        private patch_Particle[] particles;
        private MTexture overlay;
        private MTexture snow;
        private float timer;
        private float overlayAlpha;

        public extern void orig_ctor(float overlayAlpha = 0.45f);
        [MonoModConstructor]
        public void ctor(float overlayAlpha = 0.45f) {
            // THe vanilla overlay texture has got a 4x4 transparent blob formed by transparent pixels at each corner.
            MTexture overlay = OVR.Atlas["overlay"];
            if (overlay.Texture.GetMetadata() == null) {
                Texture2D texture = overlay.Texture.Texture;
                if (overlay.ClipRect.X == 0 && overlay.ClipRect.Y == 0 &&
                    overlay.ClipRect.Width == texture.Width && overlay.ClipRect.Height == texture.Height) {
                    Color[] data = new Color[texture.Width * texture.Height];
                    texture.GetData(data);

                    bool changed = false;

                    Color c = data[0];
                    if (c.A == 0 || (c.R == 0 && c.G == 0 && c.B == 0)) {
                        data[0] = new Color(65, 116, 225, 255);
                        changed = true;
                    }

                    c = data[texture.Width - 1];
                    if (c.A == 0 || (c.R == 0 && c.G == 0 && c.B == 0)) {
                        data[texture.Width - 1] = new Color(67, 117, 223, 255);
                        changed = true;
                    }

                    c = data[texture.Width * (texture.Height - 1)];
                    if (c.A == 0 || (c.R == 0 && c.G == 0 && c.B == 0)) {
                        data[texture.Width * (texture.Height - 1)] = new Color(66, 118, 225, 255);
                        changed = true;
                    }

                    c = data[texture.Width * texture.Height - 1];
                    if (c.A == 0 || (c.R == 0 && c.G == 0 && c.B == 0)) {
                        data[texture.Width * texture.Height - 1] = new Color(64, 116, 223, 255);
                        changed = true;
                    }

                    if (changed) {
                        texture.SetData(data);
                    }
                }
            }

            orig_ctor(overlayAlpha);
        }

        [MonoModReplace]
        public override void Render(Scene scene) {
            float num = Calc.Clamp(Direction.Length(), 0f, 20f);
            float num2 = 0f;
            Vector2 value = Vector2.One;
            bool flag = num > 1f;
            if (flag) {
                num2 = Direction.Angle();
                value = new Vector2(num, 0.2f + (1f - num / 20f) * 0.8f);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, null, null, null, Engine.ScreenMatrix);
            float num3 = Alpha * ParticleAlpha;
            for (int i = 0; i < particles.Length; i++) {
                Color color = particles[i].Color;
                float rotation = particles[i].Rotation;
                if (num3 < 1f) {
                    color *= num3;
                }
                snow.DrawCentered(particles[i].Position, color, value * particles[i].Scale, flag ? num2 : rotation);
            }
            float num4 = 1920f * Math.Max(0L, patch_Snow3D.FrameCount) / (patch_MountainRenderer.RotateDuration / Engine.DeltaTime);
            float num5 = 1080f * Math.Max(0L, patch_Snow3D.FrameCount) / (patch_MountainRenderer.RotateDuration / Engine.DeltaTime);
            if (num4 >= 1920f && num5 >= 1080f) {
                num5 = (num4 = 0f);
            }
            Draw.SpriteBatch.Draw(overlay.Texture.Texture, Vector2.Zero, new Rectangle(-(int)num4, -(int)num5, 1920, 1080), Color.White * (Alpha * overlayAlpha));
            Draw.SpriteBatch.End();
        }

        [MonoModIgnore]
        private struct patch_Particle {
            public float Scale;
            public Vector2 Position;
            public float Speed;
            public float Sin;
            public float Rotation;
            public Color Color;

            public extern void Reset(Vector2 direction);
        }
    }
}
