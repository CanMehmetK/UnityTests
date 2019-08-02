using System;
using UnityEngine;
using UnityEngine.Ucg.Matchmaking;

public class MatchmakingManager_Update : MonoBehaviour
{
    public string MatchmakingServiceUrl { get; set; }
    public bool Done { get; private set; }
    public Matchmaker MatchRequest { get; private set; }

    public void Start()
    {
        Done = false;
    }

    public void Update()
    {
        if (Done)
            return;

        MatchRequest?.Update();
        switch (MatchRequest?.State)
        {
            case Matchmaker.MatchmakingState.Error:
                Debug.LogError("Error: " + MatchRequest.Error);
                Done = true;
                break;
            case Matchmaker.MatchmakingState.Found:
                Debug.Log("Match found.  Server:" + MatchRequest.MatchAssignment.ConnectionString + ";  Players: " + string.Join(", ", MatchRequest.MatchAssignment.Roster));
                Done = true;
                break;
            default:
                break;
        }
    }

    public void RequestMatch(string playerId, int hats, int mode)
    {
        Done = false;

        var playerProps = new MatchmakingUtilities.PlayerProperties(){ hats = hats };
        var groupProps = new MatchmakingUtilities.GroupProperties() { mode = mode };
        var request = MatchmakingUtilities.CreateMatchmakingRequest(playerId, playerProps, groupProps);

        // Use the matchmaker constructor without event handler args to use in Update() mode
        MatchRequest = new Matchmaker(MatchmakingServiceUrl);
        MatchRequest.RequestMatch(request);
    }

}
