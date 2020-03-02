using UnityEngine;
using System.Collections;


namespace AI
{
    public abstract class Action : ScriptableObject
    {
        public abstract void Execute(Monster controller, float timeUnits);
    }
}
