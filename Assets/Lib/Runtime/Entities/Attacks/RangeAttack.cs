﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RangeAttack : BaseAttack
{
    RangedAttackData _rangeData;
    public RangeAttack(RangedAttackData data)
        :base(data)
    {
        _rangeData = data;
    }

    public List<Vector2Int> BoundedLine(Vector2Int src, Vector2Int tgt)
    {
        var rangeLine = BresenhamUtils.CalculateLine(src, tgt);
        int firstIdx = 0;
        int distance = 0;

        if(rangeLine.Count < 2)
        {
            return rangeLine;
        }

        firstIdx = 0;
        distance = _mapController.Distance(src, rangeLine[firstIdx]);
        while(firstIdx < rangeLine.Count && distance < _rangeData.MinRange)
        {
            firstIdx++;
            if(firstIdx < rangeLine.Count)
            {
                distance = _mapController.Distance(src, rangeLine[firstIdx]);
            }

        }

        int lastIdx = rangeLine.Count - 1;
        while (lastIdx > firstIdx && distance > _rangeData.MaxRange)
        {
            lastIdx--;
            distance = _mapController.Distance(src, rangeLine[lastIdx]);
        }
        return rangeLine.GetRange(firstIdx, lastIdx - firstIdx + 1);
    }

    public override bool CanTargetBeReached(IBattleEntity attacker, IBattleEntity target)
    {
        Vector2Int srcCoords = attacker.Coords;
        Vector2Int tgtCoords = target.Coords;

        var line = BoundedLine(srcCoords, tgtCoords);

        bool firstProcessed = false;
        foreach(var lineCoord in line)
        {
            int distanceToSrc = _mapController.Distance(srcCoords, lineCoord);

            if (firstProcessed && !_rangeData.AllowedTiles.Contains(_mapController.GetTileAt(lineCoord)))
            {
                break; // Occluder found
            }

            if(!firstProcessed)
            {
                firstProcessed = true;
            }

            if(tgtCoords == lineCoord)
            {
                return true;
            }
            
            var others = _entityController.GetEntitiesAt(lineCoord);
            others.RemoveAll(x => !x.BlocksRanged(_rangeData.Piercing));
            if (others.Count > 0)
            {
                break;
            }
        }
        return false;
    }

    public override List<IBattleEntity> FindAllReachableTargets(IBattleEntity source)
    {
        var allInMaxRange = _entityController.GetNearbyEntities(source.Coords, _rangeData.MaxRange);
        allInMaxRange.RemoveAll(x => !typeof(IBattleEntity).IsAssignableFrom(x.GetType()));

        var allBattlers = allInMaxRange.ConvertAll(x => (IBattleEntity)x);
        allBattlers.RemoveAll(x => !source.IsHostileTo(x));
        allBattlers.RemoveAll(x => _mapController.Distance(x.Coords, source.Coords) < _rangeData.MinRange);        
        allBattlers.RemoveAll(x => !CanTargetBeReached(source, x));
        return allBattlers;        
    }

    public override List<IBattleEntity> FindTargetsAtCoords(IBattleEntity source, Vector2Int refCoords)
    {
        List<IBattleEntity> candidates = new List<IBattleEntity>();

        Vector2Int srcCoords = source.Coords;
        Vector2Int tgtCoords = refCoords;

        var line = BoundedLine(srcCoords, tgtCoords);

        bool blockersFound = false;
        foreach (var lineCoord in line)
        {
            if (blockersFound)
            {
                break; // Occluder found
            }

            int distanceToSrc = _mapController.Distance(srcCoords, lineCoord);
            bool groundBlockFound = !_rangeData.AllowedTiles.Contains(_mapController.GetTileAt(lineCoord));
            if(groundBlockFound)
            {
                if(!blockersFound)
                {
                    blockersFound = true;
                }
            }
            
            var others = _entityController.GetEntitiesAt(lineCoord);
            var hostileOthers = CollectionUtils.GetImplementors<BaseEntity, IBattleEntity>(others);
            hostileOthers.RemoveAll(x => !source.IsHostileTo(x));
            candidates.AddRange(hostileOthers);

            if (tgtCoords == lineCoord || others.Exists(x => x.BlocksRanged(_rangeData.Piercing)))
            {
                break;
            }
        }
        return candidates;
    }

    public override List<bool> GetReachableStateForCoords(IBattleEntity source, List<Vector2Int> coords)
    {
        var srcCoords = source.Coords;
        List<bool> visible = new List<bool>();
        bool anyBlockingFound = false;

        for (int i = 0; i < coords.Count; ++i)
        {
            int dist = _mapController.Distance(srcCoords, coords[i]);
            if (dist <_rangeData.MinRange || dist > _rangeData.MaxRange )
            {
                visible.Add(false);
                continue;
            }

            if (anyBlockingFound)
            {
                visible.Add(false);
                continue;
            }

            TileBase tile = _mapController.GetTileAt(coords[i]);
            if (!_rangeData.AllowedTiles.Contains(tile))
            {
                if(!anyBlockingFound)
                {
                    anyBlockingFound = true;
                }
                else
                {
                    visible.Add(false);
                    continue; // Occluder found
                }
            }


            var others = _entityController.GetEntitiesAt(coords[i]);
            bool blockersFound = others.Exists(x => x.BlocksRanged(_rangeData.Piercing));
            var hostiles = CollectionUtils.GetImplementors<BaseEntity, IBattleEntity>(others);
            hostiles.RemoveAll(x => !source.IsHostileTo(x));
            visible.Add(hostiles.Count > 0 || !blockersFound);
            
            if (!anyBlockingFound && blockersFound)
            {
                anyBlockingFound = true;
            }
        }
        return visible;
    }
}
