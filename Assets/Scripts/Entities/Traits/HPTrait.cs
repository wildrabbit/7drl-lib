using System;
using UnityEngine;
using static BaseGameEvents;

public delegate void ExhaustedHP(IHealthTrackingEntity owner);
public delegate void HPChangedDelegate(int newHP, IHealthTrackingEntity Owner);

public class HPTrait
{
    public HPTraitData _data;

    public IHealthTrackingEntity  Owner => _owner;
    public int HP => _hp;
    public int MaxHP => _maxHP;
    public bool Regen => _regen;

    IHealthTrackingEntity _owner;
    HPEvents _events;

    int _hp;
    int _maxHP;
    float _timeUnitsForHPRefill = 1.0f;
    float _elapsedSinceLastRefill = 0.0f;
    int _regenAmount;
    bool _regen;


    public event Action<IHealthTrackingEntity,int> MaxHPChanged;
    
    public void Init(IHealthTrackingEntity owner, HPTraitData hpData, BaseGameEvents.HPEvents events)
    {
        _events = events;
        _data = hpData;
        _owner = owner;
        _maxHP = _data.MaxHP;
        _hp = _data.StartHP;
        _regen = _data.Regen;
        _regenAmount = _data.RegenAmount;
        _timeUnitsForHPRefill = _data.RegenRate;
    }

    public void IncreaseMaxHP(int newMax, bool refillCurrent = false)
    {
        _maxHP = newMax;
        _events.SendMaxHPChanged(_owner);

        if (refillCurrent)
        {
            _hp = _maxHP;
            MaxHPChanged?.Invoke(_owner, _maxHP);
        }
    }

    public void SetRegen(bool enabled)
    {
        _regen = enabled;
        _elapsedSinceLastRefill = 0.0f;
    }

    public void UpdateRegen(float units)
    {
        _elapsedSinceLastRefill += units;
        int hpIncrease = 0;
        while(_elapsedSinceLastRefill >= _timeUnitsForHPRefill)
        {
            _elapsedSinceLastRefill -= _timeUnitsForHPRefill;
            hpIncrease += _regenAmount;
        }

        if(hpIncrease > 0)
        {
            Add(hpIncrease, regen:true);
        }
    }

    public void Add(int delta, bool regen = false)
    {
        _hp = Mathf.Clamp(_hp + delta, 0, _maxHP);
        _events.SendHealthEvent(_owner, delta, true, false, regen);
    }

    public void Decrease(int delta)
    {
        _hp = Mathf.Clamp(_hp - delta, 0, _maxHP);
        _events.SendHealthEvent(_owner, delta, false, true, false);
        if (_hp == 0)
        {
            _events.SendHealthExhausted(_owner);
        }
    }


}