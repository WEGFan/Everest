﻿using Microsoft.Xna.Framework;
using MonoMod;

namespace Celeste {
    class patch_SlashFx : SlashFx {

        [MonoModLinkFrom("System.Void Celeste.SlashFx::Burst(Microsoft.Xna.Framework.Vector2,System.Single)")]
        public static void _Burst(Vector2 position, float direction) {
            Burst(position, direction);
        }

    }
}
