using UnityEngine;
using System.Collections;

public class GameOverStateContext : PlayStateContext
{
    public float Delay;
    public float Elapsed;


    public GameController Controller;
    // Replace with IGameFlowController logic so we only care about the restart bit.
    
    public override void Refresh(GameController gameController)
    {
        Controller = gameController;
    }

    public override void Init(GameController gameController)
    {
        Elapsed = -1.0f;
    }
}


public class GameOverState : IPlayState
{
    public int Update(PlayStateContext context, out bool willSpendTime)
    {
        willSpendTime = false;

        GameOverStateContext ctxt = context as GameOverStateContext;
        ctxt.Elapsed += Time.deltaTime;

        if (ctxt.Elapsed < ctxt.Delay)
        {
            return GameController.PlayStates.GameOver;
        }

        if (ctxt.Input.Any)
        {
            ctxt.Controller.Restart();   
        }

        return GameController.PlayStates.GameOver;
    }

    public virtual void Enter(PlayStateContext context)
    {

    }


    public virtual void Exit(PlayStateContext context)
    {

    }
}

