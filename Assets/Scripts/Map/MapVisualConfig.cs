using UnityEngine;

[CreateAssetMenu(menuName = "Epic Origin/Map Visual Config")]
public class MapVisualConfig : ScriptableObject
{
    [Header("Strongholds")]
    public Sprite humanStronghold;
    public Sprite heavenStronghold;
    public Sprite ghostStronghold;

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

    public Sprite GetObstaclePoiSprite()
    {
        if (obstaclePois != null && obstaclePois.Length > 0)
        {
            return obstaclePois[Random.Range(0, obstaclePois.Length)];
        }

        return null;
    }
}
