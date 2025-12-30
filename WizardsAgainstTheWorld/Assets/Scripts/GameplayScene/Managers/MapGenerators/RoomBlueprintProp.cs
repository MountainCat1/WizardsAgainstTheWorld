using UnityEngine;

[System.Serializable]
public class RoomBlueprintProp
{
    [SerializeField] public GameObject prefab = null!;
    [SerializeField] public PropPosition position = PropPosition.Anywhere;
    [SerializeField] public Vector2 offset = Vector2.zero;
    [SerializeField] public int count  = 1;
    [SerializeField] public bool required = false;
}