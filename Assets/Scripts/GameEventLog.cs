using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum EventCategory
{
    GameSetup,
    PlayerAction,
    Information,
    GameEnd
}

public abstract class BaseEvent
{
    public abstract EventCategory Category { get; }
    public abstract string Message();

    public int Turns;
    public float Time;

    public BaseEvent(int turns, float timeUnits)
    {
        Turns = turns;
        Time = timeUnits;
    }

    public string PrintTime()
    {
        return string.Empty;// $"<color=blue>[T:{Turns}, TU: {Time}]</color> ";
    }

}

public class GameSetupEvent : BaseEvent
{
    public override EventCategory Category => EventCategory.GameSetup;
    public int Seed; // Random stuff (TODO)

    public Vector2Int PlayerCoords;
    public int HP;
    public int MaxHP;
    public Vector2Int MapSize;
    public int[] MapTiles;

    string _welcomeMessage;

    public GameSetupEvent(string welcomeMessage)
        : base(0, 0)
    {
        _welcomeMessage = welcomeMessage;
    }
    public override string Message()
    {
        StringBuilder b = new StringBuilder(_welcomeMessage);
        return b.ToString();
    }
}

public class GameFinishedEvent : BaseEvent
{
    public override EventCategory Category => EventCategory.GameEnd;
    public GameResult Result;
    string _endMessage;

    public GameFinishedEvent(int turns, float time, GameResult result, string endMessage)
        : base(turns, time)
    {
        Result = result;
        _endMessage = endMessage;
    }

    public override string Message()
    {
        StringBuilder builder = new StringBuilder(PrintTime());
        builder.Append(_endMessage);
        return builder.ToString();
    }
}


public delegate void EventAddedDelegate(BaseEvent lastAdded);
public delegate void SessionStartedDelegate();
public delegate void SessionFinishedDelegate();

public class GameEventLog
{
    public event EventAddedDelegate OnEventAdded;
    public event SessionStartedDelegate OnSessionStarted;
    public event SessionFinishedDelegate OnSessionFinished;


    List<BaseEvent> _events;

    public void Init()
    {
        _events = new List<BaseEvent>();
    }

    public void StartSession(GameSetupEvent evt)
    {
        Clear();
        OnSessionStarted?.Invoke();
        AddEvent(evt);
    }

    public void AddEvent(BaseEvent evt)
    {
        _events.Add(evt);
        OnEventAdded?.Invoke(evt);
    }

    public void EndSession(GameFinishedEvent evt)
    {
        AddEvent(evt);
        OnSessionFinished?.Invoke();
    }

    public void Clear()
    {
        _events.Clear();
    }

    public string Flush()
    {
        StringBuilder builder = new StringBuilder();
        foreach(var evt in _events)
        {
            builder.Append(evt.Message());
        }
        return builder.ToString();
    }

    public List<string> GetLastItemMessages(int lastMessagesToDisplay)
    {
        List<string> messages = new List<string>();
        int positionIdx = Mathf.Max(0, _events.Count - lastMessagesToDisplay);
        for(int i = positionIdx; i < _events.Count; ++i)
        {
            messages.Add(_events[i].Message());
        }
        return messages;
    }
}
