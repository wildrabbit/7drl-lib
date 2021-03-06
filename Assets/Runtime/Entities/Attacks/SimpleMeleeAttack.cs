using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMeleeAttack : BaseAttack
{
    public SimpleMeleeAttack(SimpleMeleeAttackData data)
        :base(data)
    {

    }

    public override bool CanTargetBeReached(IBattleEntity attacker, IBattleEntity target)
    {
        return AtMeleeRange(attacker.Coords, target.Coords);
    }

    public override List<IBattleEntity> FindAllReachableTargets(IBattleEntity source)
    {
        _mapController.GetNeighbourDeltas(source.Coords, out var offsets);

        List<IBattleEntity> targetsAtBestPos = new List<IBattleEntity>();
        foreach(var offset in offsets)
        {
            Vector2Int atCoords = offset + source.Coords;
            var targetsAtCoords = FindTargetsAtCoords(source, atCoords);
            var numTargets = targetsAtCoords.Count;
            if (numTargets == 0) continue;

            if (numTargets > targetsAtBestPos.Count)
            {
                targetsAtBestPos = targetsAtCoords;
            }
        }
        if(targetsAtBestPos.Count > 1 && ((SimpleMeleeAttackData)Data).SingleTarget)
        {
            targetsAtBestPos.RemoveRange(1, targetsAtBestPos.Count - 1);
        }
        return targetsAtBestPos;
    }


    bool AtMeleeRange(Vector2Int src, Vector2Int tgt)
    {
        return _mapController.Distance(src, tgt) == 1;
    }

    public override List<IBattleEntity> FindTargetsAtCoords(IBattleEntity source, Vector2Int refCoords)
    {
        if (!AtMeleeRange(source.Coords, refCoords)) return new List<IBattleEntity>();
        var filtered = _entityController.GetFilteredEntitiesAt<IBattleEntity>(refCoords).FindAll(x => source.IsHostileTo(x));
        if (filtered.Count > 1 && ((SimpleMeleeAttackData)Data).SingleTarget)
        {
            filtered.RemoveRange(1, filtered.Count - 1);
        }
        return filtered;
    }

    public override List<bool> GetReachableStateForCoords(IBattleEntity source, List<Vector2Int> coords)
    {
        return new List<bool>(); // Don't care
    }
}

