using UnityEngine;
using System.Collections;

public class HexGridMap: IMapHelper
{
    Vector3Int[] cubeOffsets = new Vector3Int[]
    {
        new Vector3Int(0, 0, 0), new Vector3Int(0, -1, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, -1), new Vector3Int(-1, 0, -1), new Vector3Int(-1, -1, 0)
    };

    // We'll start with neutral, then N and then clockwise
    Vector2Int[][] _neighbourOffsets = new Vector2Int[][]
    {
        new Vector2Int[]{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1,1), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1)},
        new Vector2Int[]{ new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1),new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),new Vector2Int(1, -1)}
    };

    // Redblobgames <3
    public Vector3Int CubeFromCoords(Vector2Int coords)
    {
        Vector3Int cube = new Vector3Int();
        int row = coords.x;
        int col = coords.y;

        int x = col;
        int z = row + (col + (col & 1)) / 2;
        int y = x - z;
        cube.Set(x, y, z);
        return cube;
    }

    public Vector2Int CoordsFromCube(Vector3Int cube)
    {
        int row = cube.z - (cube.x + (cube.x & 1)) / 2; // &1: even check
        int col = cube.x;
        return new Vector2Int(row, col);
    }

    public int CubeDistance(Vector3Int cube1, Vector3Int cube2)
    {
        return (Mathf.Abs(cube1.x - cube2.x) + Mathf.Abs(cube1.y - cube2.y) + Mathf.Abs(cube1.z - cube2.z)) / 2;
    }
    public int Distance(Vector2Int coords1, Vector2Int coords2)
    {
        Vector3Int cube1 = CubeFromCoords(coords1);
        Vector3Int cube2 = CubeFromCoords(coords2);
        return CubeDistance(cube1, cube2);
    }

    public bool IsOnRay(Vector2Int coords1, Vector2Int coords2)
    {
        Vector3Int cube1 = CubeFromCoords(coords1);
        Vector3Int cube2 = CubeFromCoords(coords2);
        Vector3Int diff = cube2 - cube1;

        int distance = CubeDistance(cube2, cube1);

        Vector3Int[] scaled = System.Array.ConvertAll(cubeOffsets, offset =>
        {
            offset.Scale(new Vector3Int(distance, distance, distance));
            return offset;
        });

        foreach (var offset in scaled)
        {
            if (diff == offset)
            {
                return true;
            }
        }

        return false;
    }

    public Vector2Int GetDirectionOffset(MoveDirection moveDirection, Vector2Int srcCoords)
    {
        bool isEven = srcCoords.y % 2 == 0;
        Vector2Int[] offsets = isEven ? _neighbourOffsets[0] : _neighbourOffsets[1];
        return offsets[(int)moveDirection];
    }

    public Vector2Int[] GetOffsets(Vector2Int srcCoords)
    {
        return _neighbourOffsets[srcCoords.y & 1];
    }
}
