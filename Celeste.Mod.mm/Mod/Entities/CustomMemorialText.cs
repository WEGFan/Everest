﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Entities {
    /// <summary>
    /// Based on MemorialText, spawned by CustomMemorial.
    /// </summary>
    public class CustomMemorialText : Entity {

        public CustomMemorial Memorial;

        public bool Show;

        public bool Dreamy;
        public string Message;
        public float Spacing;

        private float index;
        private float alpha = 0f;
        private float timer = 0f;
        private float[] lineWidths;
        private int firstLineLength;
        private SoundSource textSfx;
        private bool textSfxPlaying;

        private Dictionary<int, PixelFontCharacter> charChars;

        public CustomMemorialText(CustomMemorial memorial, bool dreamy, string text, float spacing)
            : base() {
            Tag = Tags.HUD | Tags.PauseUpdate;
            Add(textSfx = new SoundSource());

            Dreamy = dreamy;
            Memorial = memorial;
            Message = text;
            Spacing = spacing;

            firstLineLength = CountToNewline(0);

            string[] lines = text.Split('\n');
            lineWidths = new float[lines.Length];

            for (int i = 0; i < lines.Length; i++)
                lineWidths[i] = ActiveFont.Measure(lines[i]).X + spacing * lines[i].Length;

            charChars = ActiveFont.Font.Get(ActiveFont.BaseSize).Characters;
        }

        public override void Update() {
            base.Update();

            if (((Level) Scene).Paused) {
                textSfx.Pause();
                return;
            }

            timer += Engine.DeltaTime;

            if (!Show) {
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime);
                if (alpha <= 0f) {
                    index = firstLineLength;
                }

            } else {
                alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);
                if (alpha >= 1f) {
                    index = Calc.Approach(index, Message.Length, 32f * Engine.DeltaTime);
                }
            }

            if (Show && alpha >= 1f && index < Message.Length) {
                if (!textSfxPlaying) {
                    textSfxPlaying = true;
                    textSfx.Play(Dreamy ? SFX.ui_game_memorialdream_text_loop : SFX.ui_game_memorial_text_loop);
                    textSfx.Param("end", 0f);
                }

            } else if (textSfxPlaying) {
                textSfxPlaying = false;
                textSfx.Stop(true);
                textSfx.Param("end", 1f);
            }

            textSfx.Resume();
        }

        private int CountToNewline(int start) {
            int i;
            for (i = start; i < Message.Length; i++) {
                if (Message[i] == '\n') {
                    break;
                }
            }
            return i - start;
        }

        public override void Render() {
            Level level = (Level) Scene;
            if (level.FrozenOrPaused || level.Completed) {
                return;
            }

            if (index <= 0f || alpha <= 0f) {
                return;
            }

            Camera camera = level.Camera;
            Vector2 pos = new Vector2((Memorial.X - camera.X) * 6f, (Memorial.Y - camera.Y) * 6f - 350f - ActiveFont.LineHeight * 3.3f);
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                pos.X = 1920f - pos.X;

            float alphaEased = Ease.CubeInOut(alpha);

            int length = (int) Math.Min(Message.Length, index);

            // Render the text twice. Once for the border, then once again properly.
            for (int mode = 0; mode < 2; mode++) {
                float sink = 64f * (1f - alphaEased);
                int lineLength = CountToNewline(0);
                int lineIndex = 0;
                int line = 0;
                float xNext = 0f;
                for (int i = 0; i < length; i++) {
                    char c = Message[i];
                    if (c == '\n') {
                        lineIndex = 0;
                        line++;
                        lineLength = CountToNewline(i + 1);
                        sink += ActiveFont.LineHeight * 1.1f;
                        xNext = 0f;
                        continue;
                    }

                    if (!charChars.TryGetValue(c, out PixelFontCharacter pfc))
                        continue;

                    float x = xNext - lineWidths[line] * 0.5f;
                    xNext += pfc.XAdvance;
                    if (i < Message.Length - 1 && pfc.Kerning.TryGetValue(Message[i + 1], out int kerning))
                        xNext += kerning;
                    xNext += Spacing;

                    float xScale = 1f;
                    float yOffs = 0f;
                    if (Dreamy && c != ' ' && c != '-' && c != '\n') {
                        c = Message[(i + (int) (Math.Sin((timer * 2f + i / 8f)) * 4.0) + Message.Length) % Message.Length];
                        yOffs = (float) Math.Sin((timer * 2f + i / 8f)) * 8f;
                        xScale = (Math.Sin((timer * 4f + i / 16f)) < 0.0) ? -1f : 1f;
                    }

                    if (mode == 0)
                        ActiveFont.DrawOutline(c.ToString(), pos + new Vector2(x, sink + yOffs), new Vector2(0f, 1f), new Vector2(xScale, 1f), Color.Transparent, 2f, Color.Black * alphaEased * alphaEased * alphaEased);
                    else
                        ActiveFont.DrawOutline(c.ToString(), pos + new Vector2(x, sink + yOffs), new Vector2(0f, 1f), new Vector2(xScale, 1f), Color.White * alphaEased, 2f, Color.Transparent);

                    lineIndex++;
                }
            }
        }

    }
}
