using System;
using System.Collections.Generic;

namespace UnityEngine.Ucg.Matchmaking
{
    /// <summary>
    ///     Immutable object representing a single player and their associated properties
    /// </summary>
    [Serializable]
    public sealed class MatchmakingPlayer
    {
#pragma warning disable IDE0044
        [SerializeField]
        string id;
        [SerializeField]
        string properties;
#pragma warning restore IDE0044

        /// <param name="playerId">Unique ID of a player</param>
        /// <param name="serializedPlayerProperties">Pre-serialized player properties</param>
        public MatchmakingPlayer(string playerId, string serializedPlayerProperties)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException("argument must be a non-null, non-empty string", nameof(playerId));

            // Default player properties to an empty JSON block
            if (string.IsNullOrEmpty(serializedPlayerProperties))
            {
                Debug.LogWarning($"New {nameof(MatchmakingPlayer)} created with null {nameof(serializedPlayerProperties)}, defaulting to no properties");
                properties = "{}";
            }
            else
            {
                properties = serializedPlayerProperties;
            }

            id = playerId;
        }

        public string PlayerId => id;
        public string PlayerProperties => properties;
    }

    /// <summary>
    ///     Immutable object representing all players and properties required to request a match from the matchmaking service
    /// </summary>
    [Serializable]
    public sealed class MatchmakingRequest
    {
#pragma warning disable IDE0044
        [SerializeField]
        List<MatchmakingPlayer> players;
        [SerializeField]
        string properties;
#pragma warning restore IDE0044

        /// <param name="players">The player or group of players requesting a match</param>
        /// <param name="serializedRequestProperties">
        ///     Pre-serialized properties attached to the match request (separate from individual player properties)
        /// </param>
        public MatchmakingRequest(List<MatchmakingPlayer> players, string serializedRequestProperties)
        {
            if (players == null || players.Count == 0)
                throw new ArgumentException("argument must be a non-null list with at least 1 member", nameof(players));

            if (string.IsNullOrEmpty(serializedRequestProperties) || serializedRequestProperties.Equals(@"{}"))
                throw new ArgumentException("argument must be a non-null, non-empty string", nameof(serializedRequestProperties));

            this.players = players;
            properties = serializedRequestProperties;
        }

        public List<MatchmakingPlayer> Players => players;
        public string Properties => properties;
    }

    /// <summary>
    ///     Immutable object representing the response of the matchmaking service call to begin matchmaking
    /// </summary>
    [Serializable]
    public sealed class MatchmakingResult
    {
#pragma warning disable IDE0044
        [SerializeField]
        bool success;
        [SerializeField]
        string error;
#pragma warning restore IDE0044

        public MatchmakingResult(bool startMatchSuccess, string startMatchError)
        {
            success = startMatchSuccess;
            error = startMatchError;
        }

        public bool Success => success;
        public string Error => error;
    }

    /// <summary>
    ///     Immutable object representing the success or failure of the matchmaking service to find a match
    /// </summary>
    [Serializable]
    public sealed class Assignment
    {
#pragma warning disable IDE0044
        [SerializeField]
        string connection_string;
        [SerializeField]
        string assignment_error;
        [SerializeField]
        List<string> roster;
#pragma warning restore IDE0044

        public Assignment(string assignmentConnectionString, string assignmentError, List<string> assignmentRoster)
        {
            if (string.IsNullOrEmpty(assignmentConnectionString) && string.IsNullOrEmpty(assignmentError))
                throw new ArgumentException($"Cannot create an {nameof(Assignment)} where both {nameof(assignmentConnectionString)}" +
                    $" and {nameof(assignmentError)} are null or empty.  One of them must be valid.");

            if (assignmentConnectionString != null && assignmentError != null && assignmentConnectionString.Length > 0 && assignmentError.Length > 0)
                throw new ArgumentException($"Cannot create an {nameof(Assignment)} where both {nameof(assignmentConnectionString)}" +
                    $" and {nameof(assignmentError)} are populated.  Only one may be populated.");

            // No null-check for roster, as roster is not strictly required for a client to join a session

            connection_string = assignmentConnectionString;
            assignment_error = assignmentError;
            roster = assignmentRoster;
        }

        /// <summary>
        ///     Contains the URI (normally IP and port) of the dedicated game server attached to the match
        /// </summary>
        public string ConnectionString => connection_string;

        /// <summary>
        ///     Contains the error message provided if the service was unable to successfully find and initialize a match
        /// </summary>
        public string AssignmentError => assignment_error;

        /// <summary>
        ///     Contains the roster of other players / match ticket IDs attached to the match
        /// </summary>
        public List<string> Roster => roster;
    }
}
