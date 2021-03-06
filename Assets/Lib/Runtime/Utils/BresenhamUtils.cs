using System;
using System.Collections.Generic;
using UnityEngine;

public static class BresenhamUtils
{
    public static List<Vector2Int> CalculateLine(Vector2Int p1, Vector2Int p2)
    {
        List<Vector2Int> coords = new List<Vector2Int>();

        Vector2Int coord1 = p1;
        Vector2Int coord2 = p2;
        bool reverse = false;
        if (coord1.x > coord2.x)
        {
            Vector2Int swap = coord1;
            coord1 = coord2;
            coord2 = swap;
            reverse = true;
        }

        int deltaCol = coord2.y - coord1.y;
        int deltaRow = coord2.x - coord1.x;

        if (deltaCol > 0)
        {
            if (deltaCol > deltaRow)
            {
                Octant0(coord1, deltaCol, deltaRow, 1, ref coords);
            }
            else
            {
                Octant1(coord1, deltaCol, deltaRow, 1, ref coords);
            }
        }
        else
        {
            deltaCol = -deltaCol;
            if (deltaCol > deltaRow)
            {
                Octant0(coord1, deltaCol, deltaRow, -1, ref coords);
            }
            else
            {
                Octant1(coord1, deltaCol, deltaRow, -1, ref coords);
            }
        }

        if (reverse)
        {
            coords.Reverse();
        }
        return coords;
    }

    private static void Octant0(Vector2Int start, int deltaCol, int deltaRow, int direction, ref List<Vector2Int> coords)
    {
        int delta2Line = deltaRow << 1;
        int delta2LineMinusDelta2Col = (deltaRow - deltaCol) << 1;
        int errorTerm = delta2Line - deltaCol;

        coords.Add(start);

        int colCount = deltaCol;
        int row = start.x;
        int col = start.y;
        while (colCount-- > 0)
        {
            if (errorTerm >= 0)
            {
                row++;
                errorTerm += delta2LineMinusDelta2Col;
            }
            else
            {
                errorTerm += delta2Line;
            }
            col += direction;

            coords.Add(new Vector2Int(row, col));
        }
    }
    private static void Octant1(Vector2Int start, int deltaCol, int deltaRow, int direction, ref List<Vector2Int> tiles)
    {
        int delta2Col = deltaCol << 1;
        int delta2ColMinusDelta2Line = (deltaCol - deltaRow) << 1;
        int errorTerm = delta2Col - deltaRow;

        tiles.Add(start);

        int rowCount = deltaRow;
        int row = start.x;
        int col = start.y;
        while (rowCount-- > 0)
        {
            if (errorTerm >= 0)
            {
                col += direction;
                errorTerm += delta2ColMinusDelta2Line;
            }
            else
            {
                errorTerm += delta2Col;
            }
            row++;

            tiles.Add(new Vector2Int(row, col));
        }
    }
}