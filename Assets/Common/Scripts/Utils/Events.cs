using UnityEngine.Events;
using UnityEngine;

public class Events
{
    [System.Serializable] public class EventFadeComplete : UnityEvent<bool> { }
    [System.Serializable] public class EventGameState : UnityEvent<GameManager.GameState, GameManager.GameState> { }
    [System.Serializable] public class EventMobDeath : UnityEvent<MobType, Vector3>{}
    [System.Serializable] public class EventIntegerEvent : UnityEvent<int> {}

}
