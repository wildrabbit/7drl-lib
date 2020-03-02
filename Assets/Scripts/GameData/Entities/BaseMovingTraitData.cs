
using UnityEngine;
using UnityEngine.Tilemaps;


public abstract class BaseMovingTraitData : ScriptableObject
{
    public abstract BaseMovingTrait CreateRuntimeTrait();
    public abstract bool MatchesTile(TileBase t);
}
