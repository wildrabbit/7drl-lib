using UnityEngine;
using System;

public class BaseGameEvents
{
    public class PlayerEvents
    {
        public event Action PlayerDied; // TODO: Action<PlayerDeathEvent>
        public event Action<Vector2Int, Vector3> Moved;

        public void SendPlayerDied()
        {
            PlayerDied?.Invoke(); // PlayerDied?.Invoke(new PlayerDeathEvent(....));
        }

        public void SendPlayerMoved(Vector2Int coords, Vector3 pos)
        {
            Moved?.Invoke(coords, pos);
        }
        //.....
    }


    public PlayerEvents Player;

    public BaseGameEvents()
    {
        Player = new PlayerEvents();
    }
}
