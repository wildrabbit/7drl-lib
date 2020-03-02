using UnityEngine;
using System.Collections;

namespace AI
{
    [System.Serializable]
    public class Transition
    {
        public Condition Condition;

        public abstract bool Check(Monster monster, float timeUnits)
        {
            return;
        }
        
    }

}
