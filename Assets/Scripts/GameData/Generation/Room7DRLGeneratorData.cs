using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New 7DRL GeneratorData", menuName = "7DRL_Lib/MapGen/7DRL Generator Data")]
public class Room7DRLGeneratorData : BaseMapGeneratorData
{
    [SerializeField] Vector2Int _mapSize;

    public int NumRooms = 30;

    public int MaxRoomSize = 8;
    public int MinRoomSize = 4;

    public int RectMin = 4;
    public int RectMax = 8;

    public int CrossMin = 4;
    public int CrossMax = 8;

    public float StartCavernChance = 0.4f;
    public int CaveMaxSize = 12;

    public float WallProbability = 0.4f;
    public int Neighbours = 4;

    public float SquareChance = 0.2f;
    public float XChance = 0.15f;
    public float CustomChance = 0.0f;

    public List<TextAsset> CustomRooms;

    public int BuildAttempts = 500;
    public int PlaceAttempts = 20;
    public int MaxTunnelLength = 8;

    public bool IncludeShortcuts = true;
    public int ShortcutAttempts = 300;
    public int ShortcutLength = 5;
    public int MinPathfindingDistance = 50;

    public bool IsSeeded = false;
    public string Seed;

    public override GeneratorType GeneratorType => GeneratorType.Room7DRL;
    public override Vector2Int MapSize => _mapSize;
}