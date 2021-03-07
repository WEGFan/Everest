﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_CrystalStaticSpinner : CrystalStaticSpinner {

        private CrystalColor color;

        private int ID;

        public patch_CrystalStaticSpinner(Vector2 position, bool attachToSolid, CrystalColor color)
            : base(position, attachToSolid, color) {
            // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
        }

        public extern void orig_ctor(EntityData data, Vector2 offset, CrystalColor color);

        [MonoModConstructor]
        public void ctor(EntityData data, Vector2 offset, CrystalColor color) {
            orig_ctor(data, offset, color);
            ID = data.ID;
        }

        public extern void orig_Awake(Scene scene);
        public override void Awake(Scene scene) {
            if ((int) color == -1) {
                Add(new CoreModeListener(this));
                if ((scene as Level).CoreMode == Session.CoreModes.Cold) {
                    color = CrystalColor.Blue;
                } else {
                    color = CrystalColor.Red;
                }
            }

            orig_Awake(scene);
        }

        [MonoModIgnore] // do not change anything in the method...
        [PatchSpinnerCreateSprites] // ... except manually manipulating it via MonoModRules
        private extern void CreateSprites();

        [MonoModIgnore]
        private class CoreModeListener : Component {
            public CoreModeListener(CrystalStaticSpinner parent)
                : base(true, false) {
                // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
            }
        }

    }
}
