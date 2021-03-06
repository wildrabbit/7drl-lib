using System;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public class Room7DRL
{
    public int[,] tiles;
    public Vector2Int coords;
    public Vector2Int size;
}

// Credit: https://github.com/AtTheMatinee/dungeon-generation/blob/master/dungeonGenerationAlgorithms.py
public class Room7DRLGeneratorContext: BaseMapContext
{
    public Vector2Int PlayerStart;


    public List<MonsterSpawnData> Monsters;
    public List<TrapSpawn> Traps;
    public List<BlockSpawn> Blocks;
}

public class PlaceResult
{
    public Vector2Int wallTile;
    public Vector2Int direction;
    public int length;
}

public class Room7DRLGenerator : IMapGenerator
{
    public void GenerateMap(ref int[] map, BaseMapContext mapGenContext)
    {
        var context = (Room7DRLGeneratorContext)mapGenContext;
        context.Monsters = new List<MonsterSpawnData>();
        context.Traps = new List<TrapSpawn>();
        context.Blocks = new List<BlockSpawn>();

        var genData = (Room7DRLGeneratorData)mapGenContext.GeneratorData;
        if (genData.IsSeeded)
        {
            URandom.state = JsonUtility.FromJson<URandom.State>(genData.Seed);
        }
        else
        {
            Debug.Log("Current state: " + JsonUtility.ToJson(URandom.state));
        }

        var mapAux = new int[genData.MapSize.x, genData.MapSize.y];
        mapAux.Fill(genData.NoTile);

        var rooms = new List<Room7DRL>();

        var first = GenerateRoom(genData, rooms);
        first.coords = new Vector2Int()
        {
            x = (genData.MapSize.x -  first.size.x)/2 -1,
            y = (genData.MapSize.y -  first.size.y)/2 -1
        };

        AddRoom(rooms, genData, ref mapAux, first);
        for(int i = 0; rooms.Count < genData.NumRooms && i < genData.BuildAttempts; ++i)
        {
            var next = GenerateRoom(genData, rooms);
            PlaceResult p = PlaceRoom(genData.MapSize, genData, ref mapAux, rooms, next);
            if(p != null)
            {
                AddRoom(rooms, genData, ref mapAux, next);
                AddTunnel(p.wallTile, p.direction, p.length, genData.GroundTile, ref mapAux);
                if(rooms.Count >= genData.NumRooms)
                {
                    break;
                }
            }
        }

        List<Vector2Int> offsets = new List<Vector2Int>();
        for(int i = 0; i < first.size.x; ++i)
        {
            for(int j = 0; j < first.size.y; ++j)
            {
                if(first.tiles[i,j] == genData.GroundTile)
                {
                    offsets.Add(new Vector2Int(i, j));
                }
            }
        }
        if(offsets.Count == 0)
        {
            context.PlayerStart = first.coords;

        }
        else
        {
            context.PlayerStart = first.coords + offsets[URandom.Range(0, offsets.Count)];
        }


        //if(genData.IncludeShortcuts)
        //{
        //    AddShortcuts(ref mapAux, genData.MapSize);
        //}
        GeneratorUtils.PlaceWalls(genData.WallTile, genData.GroundTile, genData.NoTile, ref mapAux);
        GeneratorUtils.ConvertGrid(mapAux, out map);
    }

    private void AddShortcuts(ref int[,] mapAux, Vector2Int mapSize)
    {
        throw new NotImplementedException();
    }

    private void AddTunnel(Vector2Int wall, Vector2Int offset, int length, int groundTile, ref int[,] mapAux)
    {
        Vector2Int start = wall + length * offset;
        Vector2Int end = wall;
        GeneratorUtils.DrawCorridor(start, end, groundTile, ref mapAux);
    }

    private PlaceResult PlaceRoom(Vector2Int mapSize, Room7DRLGeneratorData genData, ref int[,] mapAux, List<Room7DRL> rooms, Room7DRL next)
    {
        Debug.Log("Entered PlaceRoom");
        PlaceResult result = new PlaceResult();
        Vector2Int wall = Vector2Int.zero;

        
        List<(Vector2Int, Vector2Int)> candidateCoords = new List<(Vector2Int, Vector2Int)>();
        Vector2Int[] offsets = new Vector2Int[4]
        {
            new Vector2Int(1,0), // N
            new Vector2Int(-1,0), // S
            new Vector2Int(0,1), // E
            new Vector2Int(0,-1) // W
        };

        for (int i = 1; i < mapSize.x - 1; ++i)
        {
            for(int j = 1; j < mapSize.y - 1; ++j)
            {
                for(int dir = 0; dir < 4; ++dir)
                {
                    Vector2Int offset = offsets[dir];
                    try
                    {
                        if (mapAux[i, j] == genData.NoTile && mapAux[i + offsets[dir].x, j + offsets[dir].y] == genData.NoTile && mapAux[i - offsets[dir].x, j - offsets[dir].y] == genData.GroundTile)
                        {
                            candidateCoords.Add((new Vector2Int(i, j), offsets[dir]));
                        }
                    }
                    catch(IndexOutOfRangeException e)
                    {
                        Debug.LogError($"offset: {offset}, i,j:{(i, j)}");
                        Debug.DebugBreak();
                        continue;
                    }
                }
            }
        }
        if (candidateCoords.Count == 0)
        {
            // No way to place this room
            Debug.Log($"PLACE ROOMS:: No single coord was found matching the noTile/[noTile]/ground requirement :/");
            return null;
        }

        List<Vector2Int> groundTiles = new List<Vector2Int>();
        for(int i = 0; i < next.size.x; ++i)
        {
            for(int j = 0; j < next.size.y; ++j)
            {
                if(next.tiles[i,j] == genData.GroundTile)
                {
                    groundTiles.Add(new Vector2Int(i, j));
                }
            }
        }

        if(groundTiles.Count == 0)
        {
            Debug.Log("Invalid room. GTFO");
            return null;
        }

        for(int i = 0; i < genData.PlaceAttempts; ++i)
        {
            int idx = URandom.Range(0, candidateCoords.Count);

            wall = candidateCoords[idx].Item1;
            Vector2Int direction = candidateCoords[idx].Item2;

            Vector2Int start = groundTiles[URandom.Range(0, groundTiles.Count)];
            int startX = wall.x - start.x;
            int startY = wall.y - start.y;
            
            
            for(int tunnelLen = 0; tunnelLen < genData.MaxTunnelLength; ++tunnelLen)
            {
                Vector2Int cand = new Vector2Int(startX + direction.x * tunnelLen, startY + direction.y * tunnelLen);
                bool overlaps = Overlaps(next, mapAux, cand, mapSize, genData.GroundTile);
                if(!overlaps)
                {
                    next.coords = cand;
                    result.wallTile = wall;
                    result.direction = direction;
                    result.length = tunnelLen;

                    return result;
                }
            }
        }
        return null;
    }

    public bool Overlaps(Room7DRL room, int[,] mapAux, Vector2Int coords, Vector2Int mapSize, int groundTile)
    {
        for(int i = 0; i < room.size.x; ++i)
        {
            for(int j = 0; j < room.size.y; ++j)
            {
                if (room.tiles[i, j] != groundTile) continue;

                int testX = coords.x + i;
                int testY = coords.y + j;
                if(1 <= testX && testX < mapSize.x - 1 && 1 <= testY && testY < mapSize.y - 1)
                {
                    if(mapAux[testX - 1, testY - 1] == groundTile
                    || mapAux[testX, testY - 1] == groundTile
                    || mapAux[testX + 1, testY - 1] == groundTile
                    || mapAux[testX - 1, testY] == groundTile
                    || mapAux[testX, testY] == groundTile
                    || mapAux[testX + 1, testY + 1] == groundTile
                    || mapAux[testX, testY + 1] == groundTile
                    || mapAux[testX - 1, testY + 1] == groundTile)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector2Int GetRandomDirection()
    {
        Vector2Int[] dirs = new Vector2Int[4]
        {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };

        int idx = URandom.Range(0, 4);
        return dirs[idx];
    }

    private void AddRoom(List<Room7DRL> rooms, Room7DRLGeneratorData genData, ref int[,] mapAux, Room7DRL roomToAdd)
    {
        
        for(int i = 0; i < roomToAdd.size.x; ++i)
        {
            for (int j = 0; j < roomToAdd.size.y; ++j)
            {
                if(roomToAdd.tiles[i,j] == genData.GroundTile)
                {
                    mapAux[roomToAdd.coords.x + i, roomToAdd.coords.y + j] = roomToAdd.tiles[i, j];
                }
            }
        }
        rooms.Add(roomToAdd);
    }

    Room7DRL GenerateRoom(Room7DRLGeneratorData genData, List<Room7DRL> rooms)
    {
        Debug.Log("Entered GenerateRoom");
        if(rooms.Count > 0)
        {
            float choice = URandom.value;
            if(choice < genData.SquareChance)
            {
                return GenerateRect(genData);
            }
            else if (choice < (genData.SquareChance + genData.XChance))
            {
                return GenerateCross(genData);
            }
            //else if (choice < (genData.SquareChance + genData.XChance + genData.CustomChance))
            //{
            //      return GenerateCustom(genData);
            //}
            else
            {
                return GenerateCellAutomata(genData, genData.MaxRoomSize);
            }
        }
        else
        {
            if(URandom.value < genData.StartCavernChance)
            {
                return GenerateCavern(genData);
            }
            else
            {
                return GenerateRect(genData);
            }
            return new Room7DRL();
        }
    }

    private Room7DRL GenerateCavern(Room7DRLGeneratorData genData)
    {
        Debug.Log("Entered GenerateCavern");
        return GenerateCellAutomata(genData, genData.CaveMaxSize);
    }

    private Room7DRL GenerateCellAutomata(Room7DRLGeneratorData genData, int size)
    {
        Debug.Log("Entered GenerateCellAutomata");
        Room7DRL automata = new Room7DRL(); 
        int attempts = 0;
        while(true)
        {
            attempts++;

            automata.size = new Vector2Int(size, size);
            automata.tiles = new int[automata.size.x, automata.size.y];
            automata.tiles.Fill(genData.NoTile);

            for(int i = 2; i < size - 2; ++i)
            {
                for(int j = 2; j < size - 2; ++j)
                {
                    if(URandom.value >= genData.WallProbability)
                    {
                        automata.tiles[i, j] = genData.GroundTile;
                    }
                }
            }

            for(int i = 0; i < 4; ++i)
            {
                for(int j = 1; j < size - 1; ++j)
                {
                    for(int k = 1; k < size - 1; ++k)
                    {
                        int adjacent = CountAdjacentWalls(automata.tiles, j, k, genData.NoTile); // check if this changes state!
                        if (adjacent > genData.Neighbours)
                        {
                            automata.tiles[j, k] = genData.NoTile;
                        }
                        else if (adjacent < genData.Neighbours)
                        {
                            automata.tiles[j, k] = genData.GroundTile;
                        }
                    }
                }
            }

           //FloodFill(automata, genData.GroundTile, genData.NoTile, genData.MinRoomSize);

            for(int i = 0; i < automata.size.x; ++i)
            {
                for (int j = 0; j < automata.size.y; ++j)
                {
                    if (automata.tiles[i, j] == genData.GroundTile)
                    {
                        Debug.Log($"Exited after {attempts}");
                        return automata;
                    }
                }
            }
        }
        return automata;
    }

    private void FloodFill(Room7DRL automata, int groundTile, int noTile, int minSize)
    {
        Debug.Log("Entered Floodfill");
        HashSet<Vector2Int> largest = new HashSet<Vector2Int>();

        for(int i = 0; i < automata.size.x; ++i)
        {
            for(int j = 0; j < automata.size.y; ++j)
            {
                if (automata.tiles[i, j] != groundTile) continue;

                HashSet<Vector2Int> newR = new HashSet<Vector2Int>();
                Vector2Int coords = new Vector2Int(i, j);
 
                List<Vector2Int> toBeFilled= new List<Vector2Int>();
                toBeFilled.Add(coords);
                while (toBeFilled.Count > 0)
                {
                    int idx = URandom.Range(0, toBeFilled.Count - 1);
                    Vector2Int testCoords = toBeFilled[idx];
                    toBeFilled.RemoveAt(idx);

                    if(!newR.Contains(testCoords))
                    {
                        newR.Add(testCoords);
                        automata.tiles[testCoords.x, testCoords.y] = noTile;

                        Vector2Int n = new Vector2Int(testCoords.x + 1, testCoords.y);
                        Vector2Int s = new Vector2Int(testCoords.x - 1, testCoords.y);
                        Vector2Int e = new Vector2Int(testCoords.x, testCoords.y + 1);
                        Vector2Int w = new Vector2Int(testCoords.x, testCoords.y - 1);

                        Vector2Int[] dirs = new Vector2Int[4]
                        {
                            n,s,e,w
                        };
                        foreach (var dir in dirs)
                        {
                            if (dir.x < 0 || dir.x >= automata.size.x || dir.y < 0 || dir.y >= automata.size.y) continue;

                            if (automata.tiles[dir.x, dir.y] == groundTile && !toBeFilled.Contains(dir) && !newR.Contains(dir))
                            {
                                toBeFilled.Add(dir);
                            }
                        }
                    }

                   
                }
                if(newR.Count > minSize && newR.Count > largest.Count)
                {
                    largest = newR;
                }
            }
        }

        foreach(var coords in largest)
        {
            automata.tiles[coords.x, coords.y] = groundTile;
        }
    }

    private int CountAdjacentWalls(int[,] tiles, int tileY, int tileX, int noTile)
    {
        int count = 0;
        for(int i = tileY - 1; i < tileY + 2; ++i)
        {
            for (int j = tileX - 1; j < tileX + 2; ++j)
            {
                if(tiles[i,j] == noTile && (i != tileY || j != tileX))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private Room7DRL GenerateCross(Room7DRLGeneratorData genData)
    {
        Debug.Log("Entered GenCross");
        int horWidth = URandom.Range(genData.CrossMin + 2, genData.CrossMax + 1) / 2 * 2;
        int verHeight = URandom.Range(genData.CrossMin + 2, genData.CrossMax + 1) / 2 * 2;
        int horHeight = URandom.Range(genData.CrossMin, verHeight - 1) / 2 * 2;
        int verWidth = URandom.Range(genData.CrossMin, horWidth - 1) / 2 * 2;

        Room7DRL r = new Room7DRL();
        r.size = new Vector2Int(verHeight, horWidth);
        r.tiles = new int[verHeight, horWidth];
        r.tiles.Fill(genData.NoTile);

        int verOffset = verHeight / 2 - horHeight / 2;
        for(int i = verOffset; i < verOffset + horHeight; ++i)
        {
            for(int j = 0; j < horWidth; ++j)
            {
                r.tiles[i, j] = genData.GroundTile;
            }
        }

        int horOffset = horWidth / 2 - verWidth / 2;
        for (int i = 0; i < verHeight; ++i)
        {
            for (int j = horOffset; j < horOffset + verWidth; ++j)
            {
                r.tiles[i, j] = genData.GroundTile;
            }
        }

        return r;
    }

    private Room7DRL GenerateRect(Room7DRLGeneratorData genData)
    {
        Debug.Log("Entered GenerateRect");
        int width = URandom.Range(genData.RectMin, genData.RectMax + 1);
        int height = URandom.Range((int)Mathf.Max((width * 0.5f), genData.RectMin), (int)Mathf.Min((int)width * 1.5f, genData.RectMax) + 1);

        Room7DRL r = new Room7DRL();
        r.size = new Vector2Int(height, width);
        r.tiles = new int[height, width];
        r.tiles.Fill(genData.GroundTile);
        return r;
    }
    private Room7DRL GenerateCustom(Room7DRLGeneratorData genData)
    {
        throw new NotImplementedException();
    }
}
