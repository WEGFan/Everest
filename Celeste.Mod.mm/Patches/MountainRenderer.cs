#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using Celeste.Mod.Meta;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste {
    class patch_MountainRenderer : MountainRenderer {
        public bool ForceNearFog;
        public Action OnEaseEnd;
        public static readonly Vector3 RotateLookAt;
        private const float rotateDistance = 15f;
        private const float rotateYPosition = 3f;
        private bool rotateAroundCenter;
        private bool rotateAroundTarget;
        private float rotateAroundTargetDistance;
        private float rotateTimer;
        private const float DurationDivisor = 3f;
        public MountainCamera UntiltedCamera;
        public MountainModel Model;
        public bool AllowUserRotation;
        private Vector2 userOffset;
        private float percent;
        private float duration;
        private MountainCamera easeCameraFrom;
        private MountainCamera easeCameraTo;
        private float easeCameraRotationAngleTo;
        private float timer;
        private float door;

        public const float RotateDuration = 90f;

        [MonoModIgnore]
        public new int Area { get; private set; }

        [MonoModIgnore]
        public new bool Animating { get; private set; }

        private bool inFreeCameraDebugMode;

        public float EaseCamera(int area, MountainCamera transform, float? duration = null, bool nearTarget = true) {
            return EaseCamera(area, transform, duration, nearTarget, false);
        }

        [MonoModIgnore]
        private extern string GetCameraString();

        [MonoModIgnore]
        private extern Vector3 GetBetween(Vector3 from, Vector3 to, float ease);

        public void _orig_Update(Scene scene) {
            timer += Engine.DeltaTime;
            Model.Update();
            Vector2 value = AllowUserRotation ? (-Input.MountainAim.Value * 0.8f) : Vector2.Zero;
            userOffset += (value - userOffset) * (1f - (float)Math.Pow(0.0099999997764825821, (double)Engine.DeltaTime));
            if (!rotateAroundCenter) {
                if (Area == 8) {
                    userOffset.Y = Math.Max(0f, userOffset.Y);
                }
                if (Area == 7) {
                    userOffset.X = Calc.Clamp(userOffset.X, -0.4f, 0.4f);
                }
            }
            if (!inFreeCameraDebugMode) {
                if (rotateAroundCenter) {
                    // modified
                    rotateTimer = (0f - (float)Math.Max(0L, patch_Snow3D.FrameCount)) * ((float)Math.PI * 2f) / RotateDuration * Engine.DeltaTime;

                    Vector3 value2 = new Vector3((float)Math.Cos((double)rotateTimer) * 15f, 3f, (float)Math.Sin((double)rotateTimer) * 15f);
                    MountainModel model = Model;
                    // modified
                    model.Camera.Position = value2;

                    Model.Camera.Target = MountainRenderer.RotateLookAt + Vector3.Up * userOffset.Y;
                    Quaternion quaternion = default(Quaternion).LookAt(Model.Camera.Position, Model.Camera.Target, Vector3.Up);
                    // modified
                    Model.Camera.Rotation = quaternion;

                    UntiltedCamera = Camera;
                } else {
                    if (Animating) {
                        percent = Calc.Approach(percent, 1f, Engine.DeltaTime / duration);
                        float num = Ease.SineOut(percent);
                        Model.Camera.Position = this.GetBetween(easeCameraFrom.Position, easeCameraTo.Position, num);
                        Model.Camera.Target = this.GetBetween(easeCameraFrom.Target, easeCameraTo.Target, num);
                        Vector3 vector = easeCameraFrom.Rotation.Forward();
                        Vector3 vector2 = easeCameraTo.Rotation.Forward();
                        float length = Calc.LerpClamp(vector.XZ().Length(), vector2.XZ().Length(), num);
                        Vector2 vector3 = Calc.AngleToVector(MathHelper.Lerp(vector.XZ().Angle(), easeCameraRotationAngleTo, num), length);
                        float y = Calc.LerpClamp(vector.Y, vector2.Y, num);
                        Model.Camera.Rotation = default(Quaternion).LookAt(new Vector3(vector3.X, y, vector3.Y), Vector3.Up);
                        if (percent >= 1f) {
                            rotateTimer = new Vector2(Model.Camera.Position.X, Model.Camera.Position.Z).Angle();
                            Animating = false;
                            if (OnEaseEnd != null) {
                                OnEaseEnd();
                            }
                        }
                    } else if (rotateAroundTarget) {
                        // modified
                        rotateTimer = (0f - (float)Math.Max(0L, patch_Snow3D.FrameCount)) * ((float)Math.PI * 2f) / RotateDuration * Engine.DeltaTime;
                        float num2 = (new Vector2(easeCameraTo.Target.X, easeCameraTo.Target.Z) - new Vector2(easeCameraTo.Position.X, easeCameraTo.Position.Z)).Length();
                        Vector3 value3 = new Vector3(easeCameraTo.Target.X + (float)Math.Cos((double)rotateTimer) * num2, easeCameraTo.Position.Y, easeCameraTo.Target.Z + (float)Math.Sin((double)rotateTimer) * num2);
                        MountainModel model2 = Model;
                        // modified
                        model2.Camera.Position = value3;
                        Model.Camera.Target = easeCameraTo.Target + Vector3.Up * userOffset.Y * 2f + Vector3.Left * userOffset.X * 2f;
                        Quaternion quaternion2 = default(Quaternion).LookAt(Model.Camera.Position, Model.Camera.Target, Vector3.Up);
                        // modified
                        Model.Camera.Rotation = quaternion2;
                        UntiltedCamera = Camera;
                    } else {
                        Model.Camera.Rotation = easeCameraTo.Rotation;
                        Model.Camera.Target = easeCameraTo.Target;
                    }
                    UntiltedCamera = Camera;
                    if (userOffset != Vector2.Zero && !rotateAroundTarget) {
                        Vector3 value4 = Model.Camera.Rotation.Left() * userOffset.X * 0.25f;
                        Vector3 value5 = Model.Camera.Rotation.Up() * userOffset.Y * 0.25f;
                        Vector3 pos = Model.Camera.Position + Model.Camera.Rotation.Forward() + value4 + value5;
                        Model.Camera.LookAt(pos);
                    }
                }
            } else {
                Vector3 vector4 = Vector3.Transform(Vector3.Forward, Model.Camera.Rotation.Conjugated());
                Model.Camera.Rotation = Model.Camera.Rotation.LookAt(Vector3.Zero, vector4, Vector3.Up);
                Vector3 vector5 = Vector3.Transform(Vector3.Left, Model.Camera.Rotation.Conjugated());
                Vector3 vector6 = new Vector3(0f, 0f, 0f);
                if (MInput.Keyboard.Check(Keys.W)) {
                    vector6 += vector4;
                }
                if (MInput.Keyboard.Check(Keys.S)) {
                    vector6 -= vector4;
                }
                if (MInput.Keyboard.Check(Keys.D)) {
                    vector6 -= vector5;
                }
                if (MInput.Keyboard.Check(Keys.A)) {
                    vector6 += vector5;
                }
                if (MInput.Keyboard.Check(Keys.Q)) {
                    vector6 += Vector3.Up;
                }
                if (MInput.Keyboard.Check(Keys.Z)) {
                    vector6 += Vector3.Down;
                }
                MountainModel model3 = Model;
                model3.Camera.Position = model3.Camera.Position + vector6 * (MInput.Keyboard.Check(Keys.LeftShift) ? 0.5f : 5f) * Engine.DeltaTime;
                if (MInput.Mouse.CheckLeftButton) {
                    MouseState state = Mouse.GetState();
                    int num3 = Engine.Graphics.GraphicsDevice.Viewport.Width / 2;
                    int num4 = Engine.Graphics.GraphicsDevice.Viewport.Height / 2;
                    int num5 = state.X - num3;
                    int num6 = state.Y - num4;
                    MountainModel model4 = Model;
                    model4.Camera.Rotation = model4.Camera.Rotation * Quaternion.CreateFromAxisAngle(Vector3.Up, (float)num5 * 0.1f * Engine.DeltaTime);
                    MountainModel model5 = Model;
                    model5.Camera.Rotation = model5.Camera.Rotation * Quaternion.CreateFromAxisAngle(vector5, (float)(-(float)num6) * 0.1f * Engine.DeltaTime);
                    Mouse.SetPosition(num3, num4);
                }
                if (Area >= 0) {
                    Vector3 target = AreaData.Areas[Area].MountainIdle.Target;
                    Vector3 vector7 = vector5 * 0.05f;
                    Vector3 value6 = Vector3.Up * 0.05f;
                    Model.DebugPoints.Clear();
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector7 + value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector7 + value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector7 - value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector7 + value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector7 - value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector7 - value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector7 * 0.25f - value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector7 * 0.25f - value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector7 * 0.25f + Vector3.Down * 100f, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector7 * 0.25f - value6, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector7 * 0.25f + Vector3.Down * 100f, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector7 * 0.25f + Vector3.Down * 100f, Color.Red));
                }
            }
            door = Calc.Approach(door, (float)((Area == 9 && !rotateAroundCenter) ? 1 : 0), Engine.DeltaTime * 1f);
            Model.CoreWallPosition = Vector3.Lerp(Vector3.Zero, -new Vector3(-1.5f, 1.5f, 1f), Ease.CubeInOut(door));
            Model.NearFogAlpha = Calc.Approach(Model.NearFogAlpha, (float)((ForceNearFog || rotateAroundCenter) ? 1 : 0), (float)(rotateAroundCenter ? 1 : 4) * Engine.DeltaTime);
            if (Celeste.PlayMode == Celeste.PlayModes.Debug) {
                if (MInput.Keyboard.Pressed(Keys.P)) {
                    Console.WriteLine(this.GetCameraString());
                }
                if (MInput.Keyboard.Pressed(Keys.F2)) {
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.ReturnFromOptions, null);
                }
                if (MInput.Keyboard.Pressed(Keys.Space)) {
                    inFreeCameraDebugMode = !inFreeCameraDebugMode;
                }
                Model.DrawDebugPoints = inFreeCameraDebugMode;
                if (MInput.Keyboard.Pressed(Keys.F1)) {
                    AreaData.ReloadMountainViews();
                }
                if (MInput.Keyboard.Check(Keys.LeftControl)) {
                    if (MInput.Keyboard.Pressed(Keys.A)) {
                        showParticleDebugString = !showParticleDebugString;
                    } else if (MInput.Keyboard.Pressed(Keys.OemMinus)) {
                        particleDebugStringCount = Math.Max(0, particleDebugStringCount - 1);
                    } else if (MInput.Keyboard.Pressed(Keys.OemPlus)) {
                        particleDebugStringCount = Math.Max(0, particleDebugStringCount + 1);
                    }
                }
            }
        }

        [MonoModReplace]
        public override void Update(Scene scene) {
            AreaData area = -1 < Area && Area < (AreaData.Areas?.Count ?? 0) ? AreaData.Get(Area) : null;
            MapMeta meta = area?.GetMeta();

            bool wasFreeCam = inFreeCameraDebugMode;

            if (meta?.Mountain?.ShowCore ?? false) {
                Area = 9;
                _orig_Update(scene);
                Area = area.ID;

            } else {
                _orig_Update(scene);
            }

            Overworld overworld = scene as Overworld;
            if (!wasFreeCam && inFreeCameraDebugMode && (
                ((overworld.Current ?? overworld.Next) is patch_OuiFileNaming naming && naming.UseKeyboardInput) ||
                ((overworld.Current ?? overworld.Next) is OuiModOptionString stringInput && stringInput.UseKeyboardInput))) {

                // we turned on free cam mode (by pressing Space) while on an text entry screen using keyboard input... we should turn it back off.
                inFreeCameraDebugMode = false;
            }
        }

        public void SetFreeCam(bool value) {
            inFreeCameraDebugMode = value;
        }

        private bool showParticleDebugString;
        private int particleDebugStringCount;

        public extern void orig_Render(Scene scene);

        public override void Render(Scene scene) {
            orig_Render(scene);
            if (!showParticleDebugString) {
                return;
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            string text = string.Concat(new object[] {
                $"{timer:F3} {Engine.DeltaTime}\n",
                $"Camera rotation: {Model.Camera.Rotation.ToFixedString()}\n",
                $"Camera rotation angle: {Model.Camera.Rotation.EulerAngle().ToFixedString()}\n",
                $"{Engine.FrameCounter} {patch_Snow3D.FrameCount}"
            });
            ActiveFont.DrawOutline(text, new Vector2(8f, 8f), Vector2.Zero, Vector2.One * 0.5f, Color.White, 2f, Color.Black);
            List<string> particleStatusText = ((Engine.Scene as patch_Overworld).GetSnow3D as patch_Snow3D).Particles
                .Select((particle, index) => $"[{index}] {(particle as patch_Snow3D.patch_Particle).StatusString}")
                .Take(particleDebugStringCount)
                .ToList();
            for (int i = 0; i < particleStatusText.Count; i++) {
                Vector2 position = new Vector2(8f, 4 * ActiveFont.LineHeight * 0.5f + 8f + i * 2 * ActiveFont.LineHeight * 0.5f);
                ActiveFont.DrawOutline(particleStatusText[i], position, Vector2.Zero, Vector2.One * 0.5f, Color.White, 2f, Color.Black);
            }
            Draw.SpriteBatch.End();
        }
    }
}