using System;
using UnityEngine;
using System.Collections.Generic;

public class MonsterDebug: MonoBehaviour
{
    public LineRenderer _lines;
    Monster _monster;

    void Awake()
    {
        _monster = GetComponent<Monster>();
        _lines.enabled = false;
    }

    // cannot call this on update, line wont be visible then.. and if used OnPostRender() thats works when attached to camera only
    void Update()
    {
        if (!_monster.DebugPaths)
        {
            if(_lines.enabled)
            {
                _lines.enabled = false;
                _monster.PathChanged -= PathChanged;
            }
        }
        else
        {
            if (!_lines.enabled)
            {
                _lines.enabled = true;
                PathChanged();
                _monster.PathChanged += PathChanged;
            }
        }
    }

    void PathChanged()
    {
        if(_monster.Path != null)
        {
            var pos = _monster.PathWorld;
            _lines.positionCount = pos.Length;
            _lines.SetPositions(pos);
        }
    }
}
