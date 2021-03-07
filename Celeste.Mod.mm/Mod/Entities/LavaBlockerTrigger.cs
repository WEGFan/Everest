﻿using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Entities {
    [CustomEntity("everest/lavaBlockerTrigger", "cavern/lavablockertrigger")]
    public class LavaBlockerTrigger : Trigger {
        List<DynData<RisingLava>> risingLavas;
        List<SandwichLava> sandwichLavas;
        private readonly bool canReenter;
        private bool enabled = true;

        public LavaBlockerTrigger(EntityData data, Vector2 offset) 
            : base(data, offset) {
            canReenter = data.Bool("canReenter", false);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            risingLavas = scene.Entities.OfType<RisingLava>().Select(lava => new DynData<RisingLava>(lava)).ToList();
            sandwichLavas = scene.Entities.OfType<SandwichLava>().ToList();
        }

        public override void OnStay(Player player) {
            if (!enabled)
                return;

            foreach (DynData<RisingLava> data in risingLavas)
                if (data.IsAlive)
                    data.Set("waiting", true);
            foreach (SandwichLava lava in sandwichLavas)
                if (lava != null)
                    lava.Waiting = true;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (!canReenter)
                enabled = false;
        }
    }
}
