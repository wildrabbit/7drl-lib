using System;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public enum PatternMatchType
{
    None,
    Smaller,
    Exact
}

public class PatternSelection: IComparable<PatternSelection>
{
    public int[,] pattern;
    public PatternMatchType matchType;

    public PatternSelection(int[,] array, PatternMatchType match)
    {
        pattern = array;
        matchType = match;
    }

    public int CompareTo(PatternSelection other)
    {
        return matchType.CompareTo(other.matchType);
    }
}

public class BSPContext: BaseMapContext
{
    public BSPGeneratorData BSPData => ((BSPGeneratorData)GeneratorData);
    public Vector2Int PlayerStart;
    public BSPNode Tree;
}


public class BSPMapGenerator : IMapGenerator
{
    BSPContext _context;

    BSPGeneratorData _bspGenData;

    List<BSPRect> _rooms = new List<BSPRect>();

    public BSPMapGenerator()
    {}

    public void Init(/* dependencies*/)
    {
        
    }

    public void GenerateMap(ref int[] map, BaseMapContext mapGenContext)
    {
        _context = (BSPContext)mapGenContext;
        _bspGenData = (BSPGeneratorData)_context.GeneratorData;

        
        if(_bspGenData.IsSeeded)
        {
            URandom.state = JsonUtility.FromJson<URandom.State>(_bspGenData.Seed);
        }
        else
        {
            Debug.Log("Current state: " + JsonUtility.ToJson(URandom.state));
        }


        int[,] mapAux = new int[_bspGenData.MapSize.x, _bspGenData.MapSize.y];
        mapAux.Fill<int>(_bspGenData.NoTile);

        var tree = new BSPNode();
        _context.Tree = tree;
        tree.context = _context;
        tree.left = tree.right = null;
        tree.area = new BSPRect(1, 1, _bspGenData.MapSize.x - 2, _bspGenData.MapSize.y - 2);

        GenerateRooms(ref mapAux);
        GeneratorUtils.ConvertGrid(mapAux, out map);
    }

    public int CompareSelections(PatternSelection one, PatternSelection other)
    {
        return one.CompareTo(other);
    }

    public List<int[,]> GetPatternCandidates(BSPRect rect)
    {
        List<PatternSelection> selection = new List<PatternSelection>();
        foreach(var pattern in _bspGenData.PatternsList)
        {
            var match = PatternFitsInRoom(pattern, rect);
            if (match == PatternMatchType.None)
                continue;
            selection.Add(new PatternSelection(pattern, match));
        }


        return selection.ConvertAll(x => x.pattern);
    }

    PatternMatchType PatternFitsInRoom(int[,] woodPattern, BSPRect roomRect)
    {
        int patternHeight = woodPattern.GetLength(0);
        int patternWidth = woodPattern.GetLength(1);

        if (roomRect.Height == patternHeight && roomRect.Width == patternWidth)
        {
            return PatternMatchType.Exact;
        }
        else if (patternHeight <= roomRect.Height && patternWidth <= roomRect.Width )
        {
            return PatternMatchType.Smaller;
        }
        return PatternMatchType.None;
    }

    public void ApplyPattern(BSPRect rect, int noTile, int[,] pattern, ref int[,] map)
    {
        int height = pattern.GetLength(0);
        int width = pattern.GetLength(1);

        int row = URandom.Range(rect.Row, rect.Row + rect.Height - height);
        int col = URandom.Range(rect.Col, rect.Col+ rect.Width - width);

        for(var r = 0; r < height; ++r)
        {
            for(var c = 0; c < width; ++c)
            {
                if(pattern[r,c] != noTile && (_context.PlayerStart.x != row + r && _context.PlayerStart.y != col + c))
                {
                    map[row + r, col + c] = pattern[r, c];
                }
            }
        }
    }

    void GenerateRooms(ref int[,] mapAux)
    {
        _rooms = new List<BSPRect>();

        List<BSPNode> leaves = new List<BSPNode>();

        _context.Tree.Split();
        _context.Tree.GetLeaves(ref leaves);
        foreach (var leaf in leaves)
        {
            bool skipRoom = URandom.value < _bspGenData.EmptyRoomChance;
            if (skipRoom) continue;

            int height = URandom.Range(_bspGenData.MinRoomSize.x, Mathf.Min(leaf.area.Height, _bspGenData.MaxRoomSize.x) + 1);
            int width = URandom.Range(_bspGenData.MinRoomSize.y, Mathf.Min(leaf.area.Width, _bspGenData.MaxRoomSize.y) + 1);

            int row = leaf.area.Row + URandom.Range(1, leaf.area.Height - height);
            int col = leaf.area.Col + URandom.Range(1, leaf.area.Width - width);

            leaf.roomRect = new BSPRect(row, col, height, width);
            _rooms.Add(leaf.roomRect);
            GeneratorUtils.DrawRoom(new Vector2Int(row, col), new Vector2Int(height, width), _bspGenData.GroundTile, ref mapAux);
        }
        Connect(_context.Tree, ref mapAux);
        GeneratorUtils.PlaceWalls(_bspGenData.WallTile, _bspGenData.GroundTile, _bspGenData.NoTile, ref mapAux);

        // Player start
        int randomPlayerStart = URandom.Range(0, _rooms.Count);
        BSPRect playerStart = _rooms[randomPlayerStart];

        int maxIters = 10;
        int curIter = 0;
        do
        {
            _context.PlayerStart = GetRandomCoordsInRoom(playerStart);
            curIter++;
        } while (mapAux[_context.PlayerStart.x, _context.PlayerStart.y] != _bspGenData.GroundTile && curIter < maxIters);

        if(curIter == maxIters)
        {
            Debug.Log("WTF, MAX ITERS REACHED");
        }

        
        // Place patterns:
        var patternsList = _bspGenData.PatternsList;
        foreach (var r in _rooms)
        {
            bool tryApplyPattern = URandom.value < _bspGenData.PatternRoomsChance;
            if (tryApplyPattern && patternsList != null && patternsList.Count > 0)
            {
                var candidates = GetPatternCandidates(r);
                if (candidates != null && candidates.Count > 0)
                {
                    ApplyPattern(r, _bspGenData.NoTile, candidates[URandom.Range(0, candidates.Count)], ref mapAux);
                }
            }
        }
        Debug.Log($"Player start { _context.PlayerStart}");
        int tileValue = mapAux[_context.PlayerStart.x, _context.PlayerStart.y];
        if (tileValue != _bspGenData.GroundTile)
        {
            Debug.Log($"On top of a tile {tileValue}");
        }

    }

    public Vector2Int GetRandomCoordsInRoom(BSPRect rect)
    {
        return new Vector2Int(URandom.Range(rect.Row, rect.Row + rect.Height),
            URandom.Range(rect.Col, rect.Col + rect.Width));
    }

    public Vector2Int GetRandomCoordsInRoom(BSPRect rect, Predicate<Vector2Int> coordsCheck, int maxAttempts)
    {
        int numAttempts = 0;
        do
        {
            Vector2Int coords = GetRandomCoordsInRoom(rect);
            if (coordsCheck(coords))
            {
                return coords;
            }
            else numAttempts++;
        } while (numAttempts < maxAttempts);
        return new Vector2Int(-1, -1);
    }
    
    public void Connect(BSPNode tree, ref int[,] mapAux)
    {
        if(tree.left == null && tree.right == null)
        {
            return;
        }

        if(tree.left != null)
        {
            Connect(tree.left, ref mapAux);
        }

        if(tree.right != null)
        {
            Connect(tree.right, ref mapAux);
        }

        if(tree.left != null && tree.right != null)
        {
            BSPNode leftRoom = tree.left.GetLeafNode();
            BSPNode rightRoom = tree.right.GetLeafNode();
            if(leftRoom != null && rightRoom != null)
            {
                ConnectRooms(leftRoom.roomRect, rightRoom.roomRect, ref mapAux);
            }
        }
    }

    public void ConnectRooms(BSPRect leftRoom, BSPRect rightRoom, ref int[,] mapAux)
    {
        int column1 = URandom.Range(leftRoom.Col, leftRoom.Col + leftRoom.Width);
        int row1 = URandom.Range(leftRoom.Row, leftRoom.Row + leftRoom.Height);

        int column2 = URandom.Range(rightRoom.Col, rightRoom.Col + rightRoom.Width);
        int row2 = URandom.Range(rightRoom.Row, rightRoom.Row + rightRoom.Height);

        int deltaRows = row2 - row1;
        int deltaCols = column2 - column1;


        if (deltaRows < 0) // down
        {
            if (deltaCols != 0)
            {
                bool isCornerUp = URandom.value < 0.5f;
                if(isCornerUp)
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row1, column2), _bspGenData.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column2), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
                }
                else
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column1), _bspGenData.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row2, column1), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
                }
            }
            else
            {
                GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
            }
        }
        else if (deltaRows > 0) // up
        {
            if (deltaCols != 0)
            {
                bool isCornerUp = URandom.value < 0.5f;
                if (isCornerUp)
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column1), _bspGenData.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row2, column1), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
                }
                else
                {
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row1, column2), _bspGenData.GroundTile, ref mapAux);
                    GeneratorUtils.DrawCorridor(new Vector2Int(row1, column2), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
                }
            }
            else
            {
                GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
            }
        }
        else
        {
            GeneratorUtils.DrawCorridor(new Vector2Int(row1, column1), new Vector2Int(row2, column2), _bspGenData.GroundTile, ref mapAux);
        }
    }

    public BSPNode GetRoomNode()
    {
        return _context.Tree;
    }
}
