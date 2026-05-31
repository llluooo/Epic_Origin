using UnityEngine;

[CreateAssetMenu(menuName = "Epic Origin/Map Visual Config")]
public class MapVisualConfig : ScriptableObject
{
    [Header("Strongholds")]
    public Sprite humanStronghold;
    public Sprite heavenStronghold;
    public Sprite ghostStronghold;

    [Header("Race Grounds")]
    public Sprite humanGround;
    public Sprite heavenGround;
    public Sprite ghostGround;

    [Header("Neutral Grounds")]
    public Sprite[] neutralGrounds;

    [Header("Points of Interest")]
    public Sprite resourcePoi;
    public Sprite armyCampPoi;
    public Sprite eventPoi;
    public Sprite[] obstaclePois;

    [Header("Backdrop")]
    public Sprite backdropSprite;

    [Header("Overlays")]
    public Sprite selectedOverlay;
    public Sprite reachableOverlay;
    public Sprite targetOverlay;

    public Sprite GetStrongholdSprite(RaceType race)
    {
        return race switch
        {
            RaceType.Human => humanStronghold,
            RaceType.Heaven => heavenStronghold,
            RaceType.Ghost => ghostStronghold,
            _ => null
        };
    }

    public Sprite GetRaceGroundSprite(RaceType race)
    {
        return race switch
        {
            RaceType.Human => humanGround,
            RaceType.Heaven => heavenGround,
            RaceType.Ghost => ghostGround,
            _ => null
        };
    }

    public Sprite GetNeutralGroundSprite(RaceType fallbackRace)
    {
        if (neutralGrounds != null && neutralGrounds.Length > 0)
        {
            return neutralGrounds[Random.Range(0, neutralGrounds.Length)];
        }

        return GetRaceGroundSprite(fallbackRace);
    }

    public Sprite GetNeutralGroundSprite(int index, RaceType fallbackRace)
    {
        if (neutralGrounds != null && neutralGrounds.Length > 0)
        {
            int safeIndex = Mathf.Abs(index) % neutralGrounds.Length;
            return neutralGrounds[safeIndex];
        }

        return GetRaceGroundSprite(fallbackRace);
    }

    public Sprite GetObstaclePoiSprite()
    {
        if (obstaclePois != null && obstaclePois.Length > 0)
        {
            return obstaclePois[Random.Range(0, obstaclePois.Length)];
        }

        return null;
    }
}
