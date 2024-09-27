using Godot;
using System;
using System.Collections.Generic;

namespace ClubPenguinPlus.Utils
{
    /// <summary>
    /// Helper class that can create an animation bounded by frame for a Sprite2D node.
    /// 
    /// Bound by frame means that the framerate determines its speed.
    /// </summary>
    public class FramerateBoundAnimation
    {
        /// <summary>
        /// Name of the animation in the sprite frames, needed to access it
        /// </summary>
        private StringName Animation { get; set; }

        /// <summary>
        /// Reference to the sprite frames, an object containing all the frames of the animation
        /// 
        /// For use by this animator, each frame should have an integer frame duration
        /// </summary>
        private SpriteFrames SpriteFrames { get; set; }

        /// <summary>
        /// Reference to the parent object that displayes the animation
        /// </summary>
        private Sprite2D Parent { get; set; }

        /// <summary>
        /// Total number of frames in the animation
        /// </summary>
        private int FrameCount { get; set; }

        /// <summary>
        /// Current frame of the animation
        /// </summary>
        private int CurrentFrame { get; set; }

        /// <summary>
        /// Array that will have length equal to the frame count, and every
        /// element is the index of the frame in the sprite frames for that respective frame
        /// 
        /// For example, { 0, 0, 1 } means that the first two frames are the first frame in the sprite frames,
        /// and the third frame is the second frame in the sprite frames.
        /// </summary>
        private int[] Frames { get; set; }

        /// <summary>
        /// Next frame will be the first frame starting animation
        /// </summary>
        private bool IsDelayed { get; set; }

        /// <summary>
        /// Position to displace when the animation begins
        /// </summary>
        private Vector2 DelayedPosition { get; set; }

        public FramerateBoundAnimation(SpriteFrames spriteFrames, Sprite2D parent, StringName animation = null)
        {
            SpriteFrames = spriteFrames;
            Parent = parent;
            Animation = animation ?? new StringName("default");
            int totalFrames = SpriteFrames.GetFrameCount(Animation);
            var frames = new List<int>();
            FrameCount = 0;
            for (int i = 0; i < totalFrames; i++)
            {
                int frameDuration = (int)SpriteFrames.GetFrameDuration(Animation, i);
                for (int j = 0; j < frameDuration; j++)
                {
                    frames.Add(i);
                }
                FrameCount += frameDuration;
            }
            Frames = frames.ToArray();
        }

        /// <summary>
        /// Starts animation, advancing first frame, should be used inside _Ready methods.
        /// </summary>
        public void StartOnReady()
        {
            CurrentFrame = 0;
            Advance();
        }

        /// <summary>
        /// Start animation, not updating in this frame, should be ALWAYS be used if starting in a _Process method
        /// </summary>
        /// <param name="positionDelta">
        /// Delta to displace position once animation starts
        /// </param>
        public void StartOnProcess(Vector2? positionDelta = null)
        {
            CurrentFrame = 0;
            IsDelayed = true;
            if (positionDelta.HasValue)
            {
                DelayedPosition = positionDelta.Value;
            }
            else
            {
                DelayedPosition = Vector2.Zero;
            }
        }

        /// <summary>
        /// Advances the animation by one frame
        /// </summary>
        /// <returns>
        /// Whether or not the animation ended in the frame
        /// </returns>
        public bool Advance()
        {
            if (IsDelayed)
            {
                IsDelayed = false;
                Parent.Position += DelayedPosition;
            }
            Parent.Texture = SpriteFrames.GetFrameTexture(Animation, Frames[CurrentFrame]);
            CurrentFrame = (CurrentFrame + 1) % FrameCount;
            return CurrentFrame == 0;
        }

        public Texture2D GetFrameTexture(int frame)
        {
            return SpriteFrames.GetFrameTexture(Animation, frame);
        }
    }
}
