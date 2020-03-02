using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI
{
    public class State : ScriptableObject
    {
        public List<Action> Actions;
        public List<Transition> Transitions;

        public void Update(Monster monster, float timeUnits) // TODO: Generalise monster if needed
        {

        }

        public void ExecuteActions(Monster monster, float timeUnits)
        {

        }

        public void EvaluateTransitions(Monster monster, float timeUnits)
        {

        }
    }

}
