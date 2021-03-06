using UnityEngine;
using System;
using System.Collections.Generic;


namespace AI
{
    public abstract class Condition: ScriptableObject
    {
        public abstract bool Evaluate(Monster monster, float timeUnits);
    }
}