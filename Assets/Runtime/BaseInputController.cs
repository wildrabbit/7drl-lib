using System;
using System.Collections.Generic;
using UnityEngine;

public class InputEntry
{
    public bool Value;
    float _last;

    KeyCode _key;
    float _delay;

    public InputEntry(KeyCode key, float delay)
    {
        _key = key;
        _delay = delay;
        _last = -1;
        Value = false;
    }

    public bool Read()
    {
        if ((_last < 0 || Time.time - _last >= _delay) && Input.GetKey(_key))
        {
            _last = Time.time;
            Value = true;
        }
        else
        {
            Value = false;
        }
        return Value;
    }

    public void UpdateKey(KeyCode newKey)
    {
        _key = newKey;
        _last = -1;
        Value = false;
    }
}

public enum MoveDirection
{
    None = 0,
    N,
    NE,
    SE,
    S,
    SW,
    NW,
    E,
    W
}

public enum LayoutType
{
    Qwerty = 0,
    Azerty
}

public class BaseInputController
{
    protected float _moveInputDelay;
    LayoutType _currentLayout = LayoutType.Qwerty;

    BaseInputData _inputData;

    public event Action<LayoutType> OnLayoutChanged;

    public bool IdleTurn => idle.Value;

    public bool Any => Input.anyKeyDown;

    public bool RangeStart => rangeTarget.Value;
    public bool ActionConfirm => actionConfirm.Value;
    public bool ActionCancel => actionCancel.Value;

    public MoveDirection MoveDir;

    public bool ShiftPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    InputEntry rangeTarget;

    InputEntry actionCancel;
    InputEntry actionConfirm;

    public bool[] NumbersPressed;
    public KeyCode StartKeyCode;

    InputEntry dirNW;
    InputEntry dirN;
    InputEntry dirNE;
    InputEntry dirSW;
    InputEntry dirS;
    InputEntry dirSE;

    Dictionary<MoveDirection, InputEntry> _directionEntries;

    InputEntry idle;

    public void ChangeLayout(LayoutType type)
    {
        _currentLayout = type;
        foreach(var mapping in _inputData.Layouts[(int)_currentLayout].Mappings)
        {
            _directionEntries[mapping.MoveDir].UpdateKey(mapping.KeyCode);
        }
        OnLayoutChanged?.Invoke(_currentLayout);
    }

    public void Init(BaseInputData inputData, float inputDelay)
    {
        _inputData = inputData;
        _currentLayout = _inputData.DefaultLayout;
        
        _moveInputDelay = inputDelay;

        _directionEntries = new Dictionary<MoveDirection, InputEntry>();
        foreach (var mapping in _inputData.Layouts[(int)_currentLayout].Mappings)
        {
            _directionEntries.Add(mapping.MoveDir, new InputEntry(mapping.KeyCode, _moveInputDelay));
        }

        idle = new InputEntry(KeyCode.Space, _moveInputDelay);
        actionCancel = new InputEntry(KeyCode.Escape, _moveInputDelay);
        actionConfirm = new InputEntry(KeyCode.Return, _moveInputDelay);

        rangeTarget = new InputEntry(KeyCode.J, _moveInputDelay);


        NumbersPressed = new bool[_inputData.NumberKeys];
        NumbersPressed.Fill<bool>(false);
        StartKeyCode = KeyCode.Alpha1;

        DoInit();   
    }

    public virtual void DoInit() { }

    public void Read()
    {
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            int nextLayoutIdx = ((int)_currentLayout + 1) % 2;
            ChangeLayout((LayoutType)nextLayoutIdx);
        }

        idle.Read();
        rangeTarget.Read();

        actionConfirm.Read();
        actionCancel.Read();

        MoveDir = MoveDirection.None;

        

        foreach (var entry in _directionEntries)
        {
            if(entry.Value.Read())
            {
                MoveDir = entry.Key;
                break;
            }
        }

        for(int i = 0; i < _inputData.NumberKeys; ++i)
        {
            NumbersPressed[i] = Input.GetKeyUp(i + StartKeyCode);
        }

        InternalRead();
    }

    protected virtual void InternalRead()
    {

    }
}
