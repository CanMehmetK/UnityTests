using System;
using UnityEngine;
using UnityEngine.Ucg.Matchmaking;

public class MatchmakingManager_Callbacks : MonoBehaviour
{
    public string MatchmakingServiceUrl { get; set; }
    public Action<Assignment> OnSuccess { get; set; }
    public Action<string> OnError { get; set; }

    protected Matchmaker m_Matchmaker;

    public void Update()
    {
        m_Matchmaker?.Update();
    }

    public void RequestMatch(string playerId, int hats, int mode)
    {
        var playerProps = new MatchmakingUtilities.PlayerProperties() { hats = hats };
        var groupProps = new MatchmakingUtilities.GroupProperties() { mode = mode };
        var request = MatchmakingUtilities.CreateMatchmakingRequest(playerId, playerProps, groupProps);

        m_Matchmaker = new Matchmaker(MatchmakingServiceUrl, OnSuccess, OnError);
        m_Matchmaker.RequestMatch(request);
    }

}
