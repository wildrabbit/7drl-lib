using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New BSPGeneratorData", menuName = "7DRL_Lib/MapGen/BSP Generator Data")]
public class BSPGeneratorData: BaseMapGeneratorData
{
    public override GeneratorType GeneratorType => GeneratorType.BSP;
    [SerializeField] Vector2Int _mapSize;

    public override Vector2Int MapSize
    {
        get
        {
            return _mapSize;
        }
    }

    [Header("Patterns")]
    public float PatternRoomsChance;
    public List<int[,]> PatternsList
    {
        get
        {
            //if(_patternsList == null)
            {
                BuildPatternsList();
            }
            return _patternsList;
        }
    }
    List<int[,]> _patternsList;

    public TextAsset PatternsFile;

    [Header("BSP Control data")]
    public float HorizontalSplitChance; // Vertical == 1 - horz :P
    public float HorizontalSplitRatio;
    public float VerticalSplitRatio;
    public float EmptyRoomChance;
    public Vector2Int MinAreaSize;
    public Vector2Int MinRoomSize;
    public Vector2Int MaxRoomSize;

    [Header("Misc")]
    public bool IsSeeded;
    public string Seed;

    public bool BuildPatternsList()
    {
        _patternsList = new List<int[,]>();

        string[] lines = PatternsFile.text.Split('\n');
        if (lines.Length == 0)
        {
            Debug.LogError("Invalid length");
            return false;
        }

        int numPatterns = Int32.Parse(lines[0]);
        int nextLine = 1;
        for(int i = 0; i < numPatterns; ++i)
        {
            if(nextLine >= lines.Length)
            {
                Debug.LogError("Reached the end!");
                return false;
            }

            string[] dims = lines[nextLine].Split(',');
            if (dims.Length != 2)
            {
                Debug.LogError("Invalid pattern size");
                return false;
            }

            Vector2Int patternSize = new Vector2Int(Int32.Parse(dims[0]), Int32.Parse(dims[1]));
            int[,] pattern = new int[patternSize.x, patternSize.y];
            _patternsList.Add(pattern);
            if (nextLine + patternSize.x >= lines.Length)
            {
                Debug.LogError("Invalid row count");
                return false;
            }

            nextLine++;
            for (int offset = 0; offset < patternSize.x; ++offset)
            {
                string[] tilesRow = lines[nextLine + offset].Trim().TrimEnd(',').Split(',');
                if (tilesRow.Length != patternSize.y)
                {
                    Debug.LogError($"Invalid col count @ row {i - 1}");
                    return false;
                }
                for (int j = 0; j < tilesRow.Length; ++j)
                {
                    pattern[offset, j] = Int32.Parse(tilesRow[j]);
                }
            }
            nextLine += patternSize.x;
        }
        return true;
    }
}
