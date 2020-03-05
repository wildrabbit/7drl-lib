using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IMapController
{
    void Init(BaseMapData mapData);
    void BuildMap();
    void Cleanup();

    int Distance(Vector2Int from, Vector2Int to);
    Vector2Int PlayerStart { get; } 

    Rect WorldBounds { get; }
    BoundsInt CellBounds { get; }
    Vector2Int CalculateMoveOffset(MoveDirection inputDir, Vector2Int playerCoords);
    Vector2Int CoordsFromWorld(Vector3 worldPos);
    TileBase GetTileAt(Vector2Int coords);
    Vector3 WorldFromCoords(Vector2Int coords);
    void ConstrainCoords(ref Vector2Int coords);


    void GetNeighbourDeltas(Vector2Int currentCoords, out Vector2Int[] offsets);
    bool ValidCoords(Vector3Int coords);
    void SetRandomCoords(Vector2Int refCoords, int scatterLimitRadius, ref Vector2Int[] coordList, bool firstIsRef, Predicate<Vector2Int> exclusionCheck);

    Vector2Int RandomNeighbour(Vector2Int refCoords, Predicate<Vector2Int> restrictions = null);
}

