using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI
{
    [CreateAssetMenu(fileName ="newState", menuName ="7DRL_Lib/FSM")]
    public class State: ScriptableObject
    {
        public List<Action> Actions;
        public List<Transition> Transitions;

        public void Process(Monster stateController, float timeUnits) // TODO: Generalise monster if needed
        {
            ExecuteActions(stateController, timeUnits);
            CheckTransitions(stateController, timeUnits);
        }

        public void ExecuteActions(Monster stateController, float timeUnits)
        {
            for(int i = 0; i < Actions.Count; ++i)
            {
                Actions[i].Execute(stateController, timeUnits);
            }
        }

        public void CheckTransitions(Monster stateController, float timeUnits)
        {
            Transitions.Sort((t1, t2) => t1.Priority.CompareTo(t2.Priority));
            foreach(var transition in Transitions)
            {
                if(transition == null)
                {
                    Debug.LogWarning($"State {name} has a null transition. REMOVE");
                    continue;
                }
                if(transition.Condition.Evaluate(stateController, timeUnits))
                {
                    stateController.TransitionToNextState(transition.SuccessState);
                }
                else
                {
                    stateController.TransitionToNextState(transition.FailState);
                }
            }
        }
    }

}
