using System;

namespace UnityEngine.Ucg.Matchmaking
{
    public class Matchmaker
    {
        public enum MatchmakingState
        {
            None,
            Requesting,
            Searching,
            Found,
            Error
        }

        MatchmakingController m_MatchmakingController;
        MatchmakingRequest m_Request;

        public Matchmaker(string endpoint)
            : this(endpoint, null, null) { }

        /// <param name="matchmakingServiceUrl">The hostname[:port]/{projectid} of your matchmaking server</param>
        /// <param name="successHandler">If a match is found, this callback will provide the connection information</param>
        /// <param name="errorHandler">If matchmaking fails, this callback will provided some failure information</param>
        public Matchmaker(string matchmakingServiceUrl, Action<Assignment> successHandler, Action<string> errorHandler)
        {
            if (string.IsNullOrEmpty(matchmakingServiceUrl))
                throw new ArgumentException($"{nameof(matchmakingServiceUrl)} must be a non-null, non-0-length string", nameof(matchmakingServiceUrl));

            // OnSucess and OnError can be null; this class can be used in a pure Update() style instead of via callbacks if desired
            OnSuccess = successHandler;
            OnError = errorHandler;
            MatchmakingServiceUrl = matchmakingServiceUrl;
            State = MatchmakingState.None;
            EnableVerboseLogging = false;
        }

        public string MatchmakingServiceUrl { get; protected set; }
        public MatchmakingState State { get; protected set; }
        public bool EnableVerboseLogging { get; set; }
        public string Error { get; protected set; }
        public Assignment MatchAssignment { get; protected set; }
        public bool Done { get; protected set; }

        public event Action<Assignment> OnSuccess;
        public event Action<string> OnError;

        public void RequestMatch(MatchmakingRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(MatchmakingServiceUrl))
                throw new ArgumentException($"{nameof(MatchmakingServiceUrl)} must be a non-null, non-0-length string", nameof(MatchmakingServiceUrl));

            if (State != MatchmakingState.None)
                throw new InvalidOperationException($"Cannot call {nameof(RequestMatch)} more than once on a {nameof(Matchmaker)} instance.  You must create a new {nameof(Matchmaker)} instance to send a new match request.");

            if (EnableVerboseLogging)
                Debug.Log(State);

            m_Request = request;
            m_MatchmakingController = new MatchmakingController(MatchmakingServiceUrl);
            m_MatchmakingController.StartRequestMatch(m_Request, StartGetAssignment, MatchmakingErrorHandler);

            State = MatchmakingState.Requesting;
        }

        /// <summary>
        ///     Matchmaking state-machine driver
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Update()
        {
            switch (State)
            {
                case MatchmakingState.Requesting:
                    m_MatchmakingController.UpdateRequestMatch();
                    break;
                case MatchmakingState.Searching:
                    m_MatchmakingController.UpdateGetAssignment();
                    break;
                case MatchmakingState.Found:
                case MatchmakingState.Error:
                    // User hasn't stopped the state machine yet.
                    break;
                case MatchmakingState.None:
                    Debug.LogWarning($"Matchmaker: {nameof(Update)} called before a successful {nameof(RequestMatch)} call");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void StartGetAssignment()
        {
            if (State != MatchmakingState.Requesting)
                throw new InvalidOperationException($"{nameof(StartGetAssignment)} called before a successful {nameof(RequestMatch)} call");

            if (EnableVerboseLogging)
                Debug.Log(State);

            m_MatchmakingController.StartGetAssignment(m_Request.Players[0].PlayerId, MatchmakingSuccessHandler, MatchmakingErrorHandler);

            State = MatchmakingState.Searching;
        }

        void MatchmakingSuccessHandler(Assignment assignment)
        {
            State = MatchmakingState.Found;
            MatchAssignment = assignment;
            Done = true;

            if (EnableVerboseLogging)
                Debug.Log(State);

            OnSuccess?.Invoke(assignment);
        }

        void MatchmakingErrorHandler(string error)
        {
            State = MatchmakingState.Error;
            Error = error;
            Done = true;

            if (EnableVerboseLogging)
                Debug.Log(State);

            OnError?.Invoke(error ?? "No error message available.");
        }
    }
}
