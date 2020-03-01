using UnityEngine;
using UnityEngine.Tilemaps;

public interface IMap<TTile, TType> 
        where TTile:Tile
        where TType:System.Enum
{
    void InitFromArray(Vector2Int dimensions, TType[] typeArray, Vector2Int playerStart, bool arrayOriginTopLeft);
    void Cleanup();


    bool HasTileAt(Vector3Int coords);
    int[] AllTileValues { get; }
    TTile AllTiles { get;}
    BoundsInt CellBounds { get; }
    void GetNeighbourDeltas(Vector2Int currentCoords, out Vector2Int[] offsets); // Do we need a copy??

    TTile TileAt(Vector2Int coords);
    int Distance(Vector2Int coords1, Vector2Int coords2);
    bool SetTile(Vector2Int coords, TType type);
    TType GetTypeFromTile(TileBase tile);

    TTile GetTileByType(TType type);

    Vector2 WorldFromCoords(Vector2Int coords);
    Vector2Int CoordsFromWorld(Vector2Int coords);
    Rect GetBounds();

    bool IsGoal(Vector2Int coords);
    bool InsideBounds(Vector2Int coords);
    

    // void InitFromData(MapData data, EventLog log);
    // List<Vector2Int> GetDestructibleNeighbours(Vector2Int refCoords);
    // List<Vector2Int> GetWalkableNeighbours(Vector2Int coords);


}
