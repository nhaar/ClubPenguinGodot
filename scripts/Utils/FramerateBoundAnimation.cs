using Godot;
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
        public StringName Animation { get; set; }

        /// <summary>
        /// Reference to the sprite frames, an object containing all the frames of the animation
        /// 
        /// For use by this animator, each frame should have an integer frame duration
        /// </summary>
        public SpriteFrames SpriteFrames { get; set; }

        /// <summary>
        /// Reference to the parent object that displayes the animation
        /// </summary>
        public Sprite2D Parent { get; set; }

        /// <summary>
        /// Total number of frames in the animation
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Current frame of the animation
        /// </summary>
        public int CurrentFrame { get; set; }

        /// <summary>
        /// Array that will have length equal to the frame count, and every
        /// element is the index of the frame in the sprite frames for that respective frame
        /// 
        /// For example, { 0, 0, 1 } means that the first two frames are the first frame in the sprite frames,
        /// and the third frame is the second frame in the sprite frames.
        /// </summary>
        public int[] Frames { get; private set; }

        public FramerateBoundAnimation(SpriteFrames spriteFrames, Sprite2D parent, StringName animation = null)
        {
            SpriteFrames = spriteFrames;
            Parent = parent;
            Animation = animation ?? new StringName("default");
            int totalFrames = SpriteFrames.GetFrameCount(Animation);
            List<int> frames = new List<int>();
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
        /// Starts animation
        /// </summary>
        public void Start()
        {
            CurrentFrame = 0;
            Advance();
        }

        /// <summary>
        /// Advances the animation by one frame
        /// </summary>
        public void Advance()
        {
            Parent.Texture = SpriteFrames.GetFrameTexture(Animation, Frames[CurrentFrame]);
            CurrentFrame = (CurrentFrame + 1) % FrameCount;
        }
    }
}
