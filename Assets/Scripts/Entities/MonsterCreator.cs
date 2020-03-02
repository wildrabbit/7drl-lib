using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterSpawnData
{
    public const int InfiniteRespawns = -1;

    public Vector2Int Coords;

    public MonsterData Monster;
    public string[] SpawnTags;

    public int MinAmount;
    public int MaxAmount;

    public bool Scatter;

    public bool Respawnable;
    public int MaxRespawns;
    public float RespawnDelayTimeUnits;

    public bool ActivatesByDistance;
    public int PlayerDistance;
}


public class MonsterSpawn
{
    public MonsterSpawnData SpawnData;
    public float RespawnTimeElapsed;
    public int RespawnCount;

    public bool Exhausted => SpawnData.Respawnable && SpawnData.MaxRespawns != MonsterSpawnData.InfiniteRespawns && RespawnCount >= SpawnData.MaxRespawns;
    public bool OnCooldown => SpawnData.Respawnable && RespawnTimeElapsed >= 0 && RespawnTimeElapsed < SpawnData.RespawnDelayTimeUnits;

    public bool FarFromPlayer(Player p, IMapController map)
    {
        return SpawnData.ActivatesByDistance && map.Distance(p.Coords, SpawnData.Coords) > SpawnData.PlayerDistance;
    }

    public bool Ready(Player p, IMapController map)
    {
        return !Exhausted && !OnCooldown && !FarFromPlayer(p, map);
    }
}

public class MonsterCreator: IScheduledEntity
{
    const int ScatterLimitRadius = 2;
    private IEntityController _entityController;
    private AIController _aiController;
    private IMapController _map;
    private BaseGameEvents.MonsterEvents _monsterEvents;
    private TimeController _timeController;

    private List<MonsterData> _monsterData;

    private List<MonsterSpawn> _spawnPoints;

    public void Init(IEntityController entityController, AIController aiController, IMapController map, TimeController timeController, BaseGameEvents.MonsterEvents events, List<MonsterData> monsterData)
    {
        _timeController = timeController;
        _timeController.AddScheduledEntity(this);

        _aiController = aiController;
        _entityController = entityController;
        _map = map;
        _monsterEvents = events;
        _monsterData = monsterData;
    }

    public List<MonsterData> FindMonstersMatchingTagCollection(IEnumerable<string> tags)
    {
        return _monsterData.FindAll(data => data.MatchesTagSet(tags));
    }

    public void RegisterSpawnPoints(List<MonsterSpawnData> spawnData)
    {
        _spawnPoints = new List<MonsterSpawn>();
        foreach(var data in spawnData)
        {
            _spawnPoints.Add(new MonsterSpawn
            {
                SpawnData = data,
                RespawnCount = 0,
                RespawnTimeElapsed = -1.0f
            });
        }
    }

    public void ProcessInitialSpawns()
    {
        List<MonsterSpawn> spawnsToProcess = new List<MonsterSpawn>();
        foreach(var spawn in _spawnPoints)
        {
            if(spawn.Ready(_entityController.Player, _map))
            {
                spawnsToProcess.Add(spawn);                
            }
        }

        SpawnMobs(spawnsToProcess);
    }

    public void SpawnMobs(List<MonsterSpawn> spawns)
    {
        List<MonsterSpawn> removeCandidates = new List<MonsterSpawn>();
        foreach (var spawn in spawns)
        {
            ActivateMobSpawn(spawn);
            if(spawn.Exhausted)
            {
                removeCandidates.Add(spawn);
            }
        }

        foreach(var cand in removeCandidates)
        {
            _spawnPoints.Remove(cand);
        }
    }

    public void ActivateMobSpawn(MonsterSpawn spawn)
    {
        var refCoords = spawn.SpawnData.Coords;

        int min = Mathf.Max(spawn.SpawnData.MinAmount, 1);
        int numMonsters = UnityEngine.Random.Range(min, spawn.SpawnData.MaxAmount);
        var monsterToChoose = spawn.SpawnData.Monster;
        if(monsterToChoose == null)
        {
            var candidates = FindMonstersMatchingTagCollection(spawn.SpawnData.SpawnTags);
            if(candidates.Count == 0)
            {
                Debug.Log($"Couldn't find candidate monsters to spawn @ {refCoords}");
                return;
            }

            monsterToChoose = candidates[UnityEngine.Random.Range(0, candidates.Count - 1)];
        }

        Vector2Int[] coordList = new Vector2Int[numMonsters];
        coordList.Fill<Vector2Int>(refCoords);
        if(spawn.SpawnData.Scatter)
        {
            _map.GetRandomOffsets(refCoords, ScatterLimitRadius, ref coordList, firstIsRef: true, (coords) =>
            {
                if (!monsterToChoose.MovingTraitData.MatchesTile(_map.GetTileAt(coords))) return true;
                if (_entityController.ExistsEntitiesAt(coords)) return true;

                return (bool)false;
            });
        }
        List<(MonsterData, Vector2Int)> monsterList = new List<(MonsterData, Vector2Int)>();
        for(int i = 0; i < numMonsters; ++i)
        {
            monsterList.Add((monsterToChoose, coordList[i]));
        }
        
        
        _entityController.CreateMonsters(monsterList, _aiController);
        spawn.RespawnCount++;
        spawn.RespawnTimeElapsed = 0.0f;
    }


    public void Cleanup()
    {
        _timeController.RemoveScheduledEntity(this);
        _spawnPoints.Clear();
    }

    public void AddTime(float timeUnits, ref int playState)
    {
        List<MonsterSpawn> readySpawns = new List<MonsterSpawn>();
        foreach(var spawn in _spawnPoints)
        {
            spawn.RespawnTimeElapsed += timeUnits;
            if (spawn.Ready(_entityController.Player, _map))
            {
                readySpawns.Add(spawn);
            }
        }

        SpawnMobs(readySpawns);
    }
}
