
using UnityEngine;
using UnityEngine.Tilemaps;


public abstract class BaseMovingTraitData : ScriptableObject
{
    public abstract BaseMovingTrait CreateRuntimeTrait();
}
