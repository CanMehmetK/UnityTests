using System;
using System.Text;
using UnityEngine.Networking;

namespace UnityEngine.Ucg.Matchmaking
{
    public class MatchmakingClient
    {
        const string k_ApiVersion = "1";
        const string k_CreateRequestEndpoint = "/request";
        const string k_GetAssignmentEndpoint = "/assignment";

        /// <param name="matchmakingServiceBaseUrl">
        ///     The base URL of the matchmaking service, in the form of
        ///     'cloud.connected.unity3d.com/[UPID]'
        /// </param>
        public MatchmakingClient(string matchmakingServiceBaseUrl)
        {
            if (string.IsNullOrEmpty(matchmakingServiceBaseUrl))
                throw new ArgumentException($"{nameof(matchmakingServiceBaseUrl)} must be a non-null, non-0-length string", nameof(matchmakingServiceBaseUrl));

            MatchmakingServiceUrl = "https://" + matchmakingServiceBaseUrl + "/api/v" + k_ApiVersion + "/matchmaking";
        }

        public string MatchmakingServiceUrl { get; protected set; }

        /// <summary>
        ///     Start matchmaking for a provided request. This tells your matchmaking endpoint to add
        ///     the players and group data in the request to the matchmaking pool for consideration.
        /// </summary>
        /// <returns>
        ///     An asynchronous operation that can be used in various async flow patterns.
        ///     The webrequest inside will contain a json success object
        /// </returns>
        /// TODO: Strongly type expect contract return from successful call
        public UnityWebRequestAsyncOperation RequestMatchAsync(MatchmakingRequest request)
        {
            return RequestMatchAsync(MatchmakingServiceUrl, request);
        }

        /// <summary>
        ///     Retrieve the assignment for a given player. This call will perform a long GET while listening for
        ///     matchmaking results.
        /// </summary>
        /// <param name="requestId">The unique id of an existing matchmaking request</param>
        /// <returns>
        ///     An asynchronous operation that can be used in various async flow patterns.
        ///     The webrequest inside will contain a json connection string object
        /// </returns>
        /// TODO: Strongly type expect contract return from successful call
        public UnityWebRequestAsyncOperation GetAssignmentAsync(string requestId)
        {
            return GetAssignmentAsync(MatchmakingServiceUrl, requestId);
        }

        /// <summary>
        ///     Retrieve the assignment for a given player. This call will perform a long GET while listening for
        ///     matchmaking results.
        /// </summary>
        /// <param name="requestId">The unique id of an existing matchmaking request</param>
        /// <param name="matchmakingServiceUrl">The base URL of the matchmaking service you are using</param>
        /// <returns>
        ///     An asynchronous operation that can be used in various async flow patterns.
        ///     The webrequest inside will contain a json connection string object
        /// </returns>
        public static UnityWebRequestAsyncOperation GetAssignmentAsync(string matchmakingServiceUrl, string requestId)
        {
            if (string.IsNullOrEmpty(matchmakingServiceUrl))
                throw new ArgumentException($"{nameof(matchmakingServiceUrl)} must be a non-null, non-0-length string", nameof(matchmakingServiceUrl));

            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentException($"{nameof(requestId)} must be a non-null, non-0-length string", nameof(requestId));

            var url = matchmakingServiceUrl + k_GetAssignmentEndpoint + "?id=" + requestId;

            var webRequest = new UnityWebRequest(url, "GET");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log("Calling... " + url);

            return webRequest.SendWebRequest();
        }

        /// <summary>
        ///     Start matchmaking for a provided request. This tells your matchmaking endpoint to add
        ///     the players and group data in the request to the matchmaking pool for consideration.
        /// </summary>
        /// <param name="matchmakingServiceUrl">The base URL of the matchmaking service you are using</param>
        /// <returns>
        ///     An asynchronous operation that can be used in various async flow patterns.
        ///     The webrequest inside will contain a json success object
        /// </returns>
        public static UnityWebRequestAsyncOperation RequestMatchAsync(string matchmakingServiceUrl, MatchmakingRequest request)
        {
            if (string.IsNullOrEmpty(matchmakingServiceUrl))
                throw new ArgumentException($"{nameof(matchmakingServiceUrl)} must be a non-null, non-0-length string", nameof(matchmakingServiceUrl));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var url = matchmakingServiceUrl + k_CreateRequestEndpoint;

            var webRequest = new UnityWebRequest(url, "POST");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            var txtRec = JsonUtility.ToJson(request);
            var jsonToSend = new UTF8Encoding().GetBytes(txtRec);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            //TODO: Alter logging to be GDPR-compliant for final release
            Debug.Log("Calling... " + url + " " + txtRec);

            return webRequest.SendWebRequest();
        }
    }
}
