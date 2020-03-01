﻿

using System;

public class PlayStateContext
{
    public BaseInputController Input;
    public int Turns;
    public float TimeUnits;

    public virtual void Refresh(GameController gameController)
    {
        Turns = gameController.TimeController.Turns;
        TimeUnits = gameController.TimeController.TimeUnits;

    }

    public static implicit operator PlayStateContext(GameOverState v)
    {
        throw new NotImplementedException();
    }
}

public interface IPlayState
{
    int Update(PlayStateContext context, out bool willSpendTime);
}