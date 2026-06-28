using System.Collections.Generic;
using UnityEngine;

public class HeroWalkAnimator : MonoBehaviour
{
    public const int DirectionCount = 8;
    public const int FramesPerDirection = 4;
    public const int RequiredFrameCount = DirectionCount * FramesPerDirection;

    public const int SouthRow = 0;
    public const int SouthWestRow = 1;
    public const int WestRow = 2;
    public const int NorthWestRow = 3;
    public const int NorthRow = 4;
    public const int NorthEastRow = 5;
    public const int EastRow = 6;
    public const int SouthEastRow = 7;

    public Sprite[] humanWalkFrames;
    public Sprite[] heavenWalkFrames;
    public Sprite[] ghostWalkFrames;
    public float animationFps = 8f;

    private readonly Dictionary<RaceType, Sprite[]> frameCache = new();
    private SpriteRenderer spriteRenderer;
    private RaceType currentRace = RaceType.Human;
    private int currentDirectionRow = SouthRow;
    private int currentFrameIndex;
    private float frameTimer;
    private bool isWalking;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        BuildFrameCache();
        ApplyCurrentFrame();
    }

    private void Update()
    {
        if (!isWalking || animationFps <= 0f)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / animationFps;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrameIndex = (currentFrameIndex + 1) % FramesPerDirection;
            ApplyCurrentFrame();
        }
    }

    public void SetRace(RaceType race)
    {
        currentRace = race;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (frameCache.Count == 0)
        {
            BuildFrameCache();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        currentFrameIndex = 0;
        ApplyCurrentFrame();
    }

    public void StartWalking(Vector2 direction)
    {
        currentDirectionRow = GetDirectionRow(direction, currentDirectionRow);
        currentFrameIndex = 0;
        frameTimer = 0f;
        isWalking = true;
        ApplyCurrentFrame();
    }

    public void StopWalking()
    {
        isWalking = false;
        currentFrameIndex = 0;
        frameTimer = 0f;
        ApplyCurrentFrame();
    }

    public static int GetDirectionRow(Vector2 direction, int fallbackRow = SouthRow)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return Mathf.Clamp(fallbackRow, 0, DirectionCount - 1);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }

        if (angle >= 337.5f || angle < 22.5f) return EastRow;
        if (angle < 67.5f) return NorthEastRow;
        if (angle < 112.5f) return NorthRow;
        if (angle < 157.5f) return NorthWestRow;
        if (angle < 202.5f) return WestRow;
        if (angle < 247.5f) return SouthWestRow;
        if (angle < 292.5f) return SouthRow;
        return SouthEastRow;
    }

    public static int GetFrameArrayIndex(int directionRow, int frameIndex)
    {
        int clampedDirection = Mathf.Clamp(directionRow, 0, DirectionCount - 1);
        int clampedFrame = Mathf.Clamp(frameIndex, 0, FramesPerDirection - 1);
        return clampedDirection * FramesPerDirection + clampedFrame;
    }

    private void BuildFrameCache()
    {
        frameCache.Clear();
        AddFrames(RaceType.Human, humanWalkFrames);
        AddFrames(RaceType.Heaven, heavenWalkFrames);
        AddFrames(RaceType.Ghost, ghostWalkFrames);
    }

    private void AddFrames(RaceType race, Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        frameCache[race] = frames;
    }

    private void ApplyCurrentFrame()
    {
        if (spriteRenderer == null || !frameCache.TryGetValue(currentRace, out Sprite[] frames))
        {
            return;
        }

        int frameArrayIndex = GetFrameArrayIndex(currentDirectionRow, currentFrameIndex);
        if (frameArrayIndex >= frames.Length || frames[frameArrayIndex] == null)
        {
            return;
        }

        spriteRenderer.sprite = frames[frameArrayIndex];
    }
}
