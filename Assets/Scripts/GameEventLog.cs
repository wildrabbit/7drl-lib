using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum EventLogCategory
{
    GameSetup,
    PlayerAction,
    Information,
    GameEnd
}

public abstract class EventLogMessage
{
    public abstract EventLogCategory Category { get; }
    public abstract string Message();

    public int Turns;
    public float Time;

    public EventLogMessage() { }
    public EventLogMessage(int turns, float timeUnits)
    {
        Turns = turns;
        Time = timeUnits;
    }

    public string PrintTime()
    {
        return string.Empty;// $"<color=blue>[T:{Turns}, TU: {Time}]</color> ";
    }

}

public class GameSetupEvent : EventLogMessage
{
    public override EventLogCategory Category => EventLogCategory.GameSetup;
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

public class GameFinishedEvent : EventLogMessage
{
    public override EventLogCategory Category => EventLogCategory.GameEnd;
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


public delegate void EventAddedDelegate(EventLogMessage lastAdded);
public delegate void SessionStartedDelegate();
public delegate void SessionFinishedDelegate();

public class GameEventLog
{
    public event EventAddedDelegate OnEventAdded;
    public event SessionStartedDelegate OnSessionStarted;
    public event SessionFinishedDelegate OnSessionFinished;

    protected TimeController _timeController;
    protected BaseGameEvents _gameEvents;
    List<EventLogMessage> _eventRecord;

    public virtual void Init(TimeController timeController, BaseGameEvents events)
    {
        _gameEvents = events;
        _eventRecord = new List<EventLogMessage>();
        _timeController = timeController;
    }

    public void StartSession(GameSetupEvent evt)
    {
        Clear();
        OnSessionStarted?.Invoke();
        AddEvent(evt);
    }

    public void AddEvent(EventLogMessage evt)
    {
        _eventRecord.Add(evt);
        OnEventAdded?.Invoke(evt);
    }

    public void EndSession(GameFinishedEvent evt)
    {
        AddEvent(evt);
        OnSessionFinished?.Invoke();
    }

    public virtual void Cleanup()
    {
        Clear();
    }

    public void Clear()
    {
        _eventRecord.Clear();
    }

    public string Flush()
    {
        StringBuilder builder = new StringBuilder();
        foreach(var evt in _eventRecord)
        {
            builder.Append(evt.Message());
        }
        return builder.ToString();
    }

    public List<string> GetLastItemMessages(int lastMessagesToDisplay)
    {
        List<string> messages = new List<string>();
        int positionIdx = Mathf.Max(0, _eventRecord.Count - lastMessagesToDisplay);
        for(int i = positionIdx; i < _eventRecord.Count; ++i)
        {
            messages.Add(_eventRecord[i].Message());
        }
        return messages;
    }
}
