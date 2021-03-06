using UnityEngine.Tilemaps;

public abstract class BaseMovingTrait
{
    public abstract void Init(BaseMovingTraitData data);
    public abstract bool EvaluateTile(TileBase t);
}