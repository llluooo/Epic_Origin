using NUnit.Framework;
using UnityEngine;

public class HeroWalkAnimatorTests
{
    [Test]
    public void GetDirectionRow_maps_cardinal_and_diagonal_movement()
    {
        Assert.AreEqual(HeroWalkAnimator.SouthRow, HeroWalkAnimator.GetDirectionRow(new Vector2(0f, -1f)));
        Assert.AreEqual(HeroWalkAnimator.WestRow, HeroWalkAnimator.GetDirectionRow(new Vector2(-1f, 0f)));
        Assert.AreEqual(HeroWalkAnimator.NorthEastRow, HeroWalkAnimator.GetDirectionRow(new Vector2(1f, 1f)));
    }

    [Test]
    public void GetDirectionRow_uses_default_or_last_facing_for_zero_delta()
    {
        Assert.AreEqual(HeroWalkAnimator.SouthRow, HeroWalkAnimator.GetDirectionRow(Vector2.zero));
        Assert.AreEqual(HeroWalkAnimator.NorthEastRow, HeroWalkAnimator.GetDirectionRow(Vector2.zero, HeroWalkAnimator.NorthEastRow));
    }

    [Test]
    public void GetFrameArrayIndex_maps_direction_and_frame_to_linear_sprite_array()
    {
        Assert.AreEqual(HeroWalkAnimator.DirectionCount, 8);
        Assert.AreEqual(HeroWalkAnimator.FramesPerDirection, 4);
        Assert.AreEqual(32, HeroWalkAnimator.RequiredFrameCount);
        Assert.AreEqual(0, HeroWalkAnimator.GetFrameArrayIndex(HeroWalkAnimator.SouthRow, 0));
        Assert.AreEqual(3, HeroWalkAnimator.GetFrameArrayIndex(HeroWalkAnimator.SouthRow, 3));
        Assert.AreEqual(5, HeroWalkAnimator.GetFrameArrayIndex(HeroWalkAnimator.SouthWestRow, 1));
        Assert.AreEqual(23, HeroWalkAnimator.GetFrameArrayIndex(HeroWalkAnimator.EastRow, 3));
        Assert.AreEqual(31, HeroWalkAnimator.GetFrameArrayIndex(HeroWalkAnimator.SouthEastRow, 3));
    }
}
