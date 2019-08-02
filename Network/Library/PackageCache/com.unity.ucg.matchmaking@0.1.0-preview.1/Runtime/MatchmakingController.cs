using System;
using UnityEngine.Networking;

namespace UnityEngine.Ucg.Matchmaking
{
    public class MatchmakingController
    {
        MatchmakingClient m_Client;
        UnityWebRequestAsyncOperation m_GetAssignmentOperation;
        UnityWebRequestAsyncOperation m_RequestMatchOperation;
        Action<string> m_OnGetAssignmentError;
        Action<Assignment> m_OnGetAssignmentSuccess;
        Action<string> m_OnRequestMatchError;
        Action m_OnRequestMatchSuccess;
        
        public MatchmakingController(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException($"{nameof(endpoint)} must be a non-null, non-0-length string", nameof(endpoint));

            m_Client = new MatchmakingClient(endpoint);
        }

        /// <summary>
        ///     Start a matchmaking request call on the controller
        /// </summary>
        public void StartRequestMatch(MatchmakingRequest request, Action successHandler, Action<string> errorHandler)
        {
            m_OnRequestMatchSuccess = successHandler ?? throw new ArgumentNullException(nameof(successHandler));
            m_OnRequestMatchError = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

            // Throws errors if request is invalid            
            m_RequestMatchOperation = m_Client.RequestMatchAsync(request);
        }

        /// <summary>
        ///     Update the state of the request; if it is complete, this will invoke the correct registered callback
        /// </summary>
        public void UpdateRequestMatch()
        {
            if (m_RequestMatchOperation == null)
            {
                Debug.LogWarning($"{nameof(UpdateRequestMatch)} called before {nameof(StartRequestMatch)}");
                return;
            }

            if (!m_RequestMatchOperation.isDone)
                return;

            if (m_RequestMatchOperation.webRequest.isNetworkError || m_RequestMatchOperation.webRequest.isHttpError)
            {
                Debug.LogError($"Error calling matchmaking {nameof(StartRequestMatch)}. Error: {m_RequestMatchOperation.webRequest.error}");
                m_OnRequestMatchError.Invoke(m_RequestMatchOperation.webRequest.error);
                return;
            }

            var result = JsonUtility.FromJson<MatchmakingResult>(m_RequestMatchOperation.webRequest.downloadHandler.text);

            if (!result.Success)
            {
                m_OnRequestMatchError.Invoke(result.Error);
                return;
            }

            m_OnRequestMatchSuccess.Invoke();
        }

        /// <summary>
        ///     Start a matchmaking request to get the connection information for the provided unique request id
        /// </summary>
        public void StartGetAssignment(string id, Action<Assignment> successHandler, Action<string> errorHandler)
        {
            m_OnGetAssignmentSuccess = successHandler ?? throw new ArgumentNullException(nameof(successHandler));
            m_OnGetAssignmentError = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

            // Throws errors if id is invalid
            m_GetAssignmentOperation = m_Client.GetAssignmentAsync(id);
        }

        /// <summary>
        ///     Update the state of the request; if it is complete, this will invoke the correct registered callback
        /// </summary>
        public void UpdateGetAssignment()
        {
            if (m_GetAssignmentOperation == null)
            {
                Debug.LogWarning($"{nameof(UpdateGetAssignment)} called before {nameof(StartGetAssignment)}");
                return;
            }

            if (!m_GetAssignmentOperation.isDone)
                return;

            if (m_GetAssignmentOperation.webRequest.isNetworkError || m_GetAssignmentOperation.webRequest.isHttpError)
            {
                Debug.LogError($"Error calling matchmaking {nameof(UpdateGetAssignment)}. Error: {m_GetAssignmentOperation.webRequest.error}");
                m_OnGetAssignmentError.Invoke(m_GetAssignmentOperation.webRequest.error);
                return;
            }

            var result = JsonUtility.FromJson<Assignment>(m_GetAssignmentOperation.webRequest.downloadHandler.text);

            if (!string.IsNullOrEmpty(result.AssignmentError))
            {
                m_OnGetAssignmentError.Invoke(result.AssignmentError);
                return;
            }

            m_OnGetAssignmentSuccess.Invoke(result);
        }
    }
}
