

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

    public virtual void Init(GameController gameController)
    {
    }
}

public interface IPlayState
{
    int Update(PlayStateContext context, out bool willSpendTime);
}