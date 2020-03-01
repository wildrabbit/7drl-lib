using UnityEngine;
using System.Collections;
using System;

public class BaseGameEvents
{
    public class PlayerEvents
    {
        public event Action PlayerDied; // TODO: Action<PlayerDeathEvent>

        public void SendPlayerDied()
        {
            PlayerDied?.Invoke(); // PlayerDied?.Invoke(new PlayerDeathEvent(....));
        }
        //.....
    }


    PlayerEvents Player;
}
