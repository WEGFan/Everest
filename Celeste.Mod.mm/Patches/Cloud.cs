﻿using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_Cloud : Cloud {

        public bool? Small;

        public patch_Cloud(Vector2 position, bool fragile)
            : base(position, fragile) {
            // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
        }

        [MonoModIgnore] // We don't want to change anything about the method...
        [PatchCloudAdded] // ... except for manually manipulating the method via MonoModRules
        public extern new void Added(Scene scene);

        public bool IsSmall(bool value) => Small ?? value;

    }
}
