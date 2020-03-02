using UnityEngine;
using System.Collections;

namespace AI
{
    [System.Serializable]
    public class Transition
    {
        public int Priority; // Lower value == Higher.
        public Condition Condition;
        
        public State SuccessState;
        public State FailState;
    }

}
