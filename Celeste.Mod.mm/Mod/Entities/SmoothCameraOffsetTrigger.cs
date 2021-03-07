﻿using Microsoft.Xna.Framework;

namespace Celeste.Mod.Entities {
    /// <summary>
    /// A Camera Offset Trigger lerping the offset depending on the player position.
    /// 
    /// Attributes:
    /// - `offsetXFrom` / `offsetYFrom`: offset to fade from
    /// - `offsetXTo` / `offsetYTo`: offset to fade to
    /// - `positionMode`: fade direction
    /// - `onlyOnce`: enable to have the trigger remove itself when the player leaves it.
    /// </summary>
    [CustomEntity("everest/smoothCameraOffsetTrigger", "SpringCollab2020/SmoothCameraOffsetTrigger")]
    public class SmoothCameraOffsetTrigger : Trigger {

        private Vector2 offsetFrom;
        private Vector2 offsetTo;
        private PositionModes positionMode;
        private bool onlyOnce;
        private bool xOnly;
        private bool yOnly;

        public SmoothCameraOffsetTrigger(EntityData data, Vector2 offset) 
            : base(data, offset) {
            // parse the trigger attributes. Multiplying X dimensions by 48 and Y ones by 32 replicates the vanilla offset trigger behavior.
            offsetFrom = new Vector2(data.Float("offsetXFrom") * 48f, data.Float("offsetYFrom") * 32f);
            offsetTo = new Vector2(data.Float("offsetXTo") * 48f, data.Float("offsetYTo") * 32f);
            positionMode = data.Enum<PositionModes>("positionMode");
            onlyOnce = data.Bool("onlyOnce");
            xOnly = data.Bool("xOnly");
            yOnly = data.Bool("yOnly");
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            if (!yOnly) {
                SceneAs<Level>().CameraOffset.X = MathHelper.Lerp(offsetFrom.X, offsetTo.X, GetPositionLerp(player, positionMode));
            }
            if (!xOnly) {
                SceneAs<Level>().CameraOffset.Y = MathHelper.Lerp(offsetFrom.Y, offsetTo.Y, GetPositionLerp(player, positionMode));
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (onlyOnce) {
                RemoveSelf();
            }
        }
    }
}
