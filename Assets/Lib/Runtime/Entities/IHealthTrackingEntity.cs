using System;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthTrackingEntity: IEntity
{
    HPTrait HPTrait { get; }
}
