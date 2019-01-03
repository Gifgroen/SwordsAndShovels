using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SessionKeeper 
{
    public List<SessionStats> Sessions;

    public SessionKeeper()
    {
        Sessions = new List<SessionStats>();
    }
}
