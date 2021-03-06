using System;
using System.Collections.Generic;

namespace AI
{
    public interface IStateController
    {
        void TransitionToNextState(State t);
    }
}
