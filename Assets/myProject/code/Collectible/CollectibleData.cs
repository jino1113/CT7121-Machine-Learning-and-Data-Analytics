using UnityEngine;

[CreateAssetMenu(menuName = "Collectible/Collectible Data")]
public class CollectibleData : ScriptableObject
{
    public string collectibleName;
    public GameObject modelPrefab;
}
