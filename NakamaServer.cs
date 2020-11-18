using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Nakama;
using Encoding = System.Text.Encoding;

namespace B4DLib
{
    public static class NakamaServer
    {
        public enum ReadPermission
        {
            NoRead = 0, OwnerRead = 1, PublicRead = 2,
        }
        public enum WritePermission
        {
            NoWrite = 0, OwnerWrite = 1
        }

        #region Events

        /// <summary>
        /// Emitted when a socket connection is established.
        /// </summary>
        public static event Action Connected;
        /// <summary>
        /// Emitted when a socket connection is closed.
        /// </summary>
        public static event Action Closed;
        /// <summary>
        /// Emitted when a there is an error.
        /// </summary>
        public static event Action<Exception> Error;
        /// <summary>
        /// Emitted when a Match state is received.
        /// </summary>
        public static event Action<IMatchState> MatchStateReceived;
        /// <summary>
        /// Emitted when a Match presence is received.
        /// </summary>
        public static event Action<IMatchPresenceEvent> MatchPresenceReceived;
        /// <summary>
        /// Emitted when a channel message is received.
        /// </summary>
        public static event Action<IApiChannelMessage> ChannelMessageReceived;
        /// <summary>
        /// Emitted when a Matchmaker matched status is received.
        /// </summary>
        public static event Action<IMatchmakerMatched> MatchmakerMatched;
        

        #endregion
     
        //Config
        private const string Host = "127.0.0.1";
        private const string Key = "nakama_key";
        private const int Port = 7350;
        private const string Scheme = "http";

        //Server
        private static IClient NakamaClient;
        private static ISession NakamaSession;
        private static ISocket NakamaSocket;

        public static readonly Dictionary<string, IChannel> ChatChannels = new Dictionary<string, IChannel>();

        //User
        private static string AccessToken = string.Empty;


        ///<summary>
        /// Initialize a Nakama server connection.
        /// </summary>
        /// <param name="host">Nakama Server Host default localhost</param>
        /// <param name="key">Nakama Key set in docker-compose.yml</param>
        ///<returns></returns>
        public static void InitNakamaClient(string host= Host, string key = Key)
        {
            try
            {
                NakamaClient = new Client(Scheme,host,Port,key);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Authenticate the user using an email and password .
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <param name="createAccount">true = SignUp , false = login</param>
        /// <returns></returns>
        public static async Task AuthenticateAsync(string email , string password ,string username = null, bool createAccount = true)
        {
            try
            {
                NakamaSession =  await NakamaClient.AuthenticateEmailAsync(email, password, username, createAccount);
                AccessToken = NakamaSession.AuthToken;
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Authenticate the user with a custom ID 
        /// </summary>
        /// <param name="id">Custom ID</param>
        /// <param name="username">Username</param>
        /// <param name="createAccount">true = SignUp , false = login</param>
        public static async Task AuthenticateAsync(string id , string username = null , bool createAccount = true )
        {
            try
            {
                NakamaSession = await NakamaClient.AuthenticateCustomAsync(id, username, createAccount);
                AccessToken = NakamaSession.AuthToken;
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Open a Nakama server socket connection
        /// </summary>
        /// <returns></returns>
        public static async Task OpenConnectionAsync()
        {
            try
            {
                NakamaSocket = Socket.From(NakamaClient);

                NakamaSocket.Connected += OnConnected;
                NakamaSocket.Closed += OnClosed;
                NakamaSocket.ReceivedError += OnError;
                NakamaSocket.ReceivedMatchState += OnReceivedMatchState;
                NakamaSocket.ReceivedMatchPresence += OnReceivedMatchPresence;
                NakamaSocket.ReceivedChannelMessage += OnReceivedChannelMessage;
                NakamaSocket.ReceivedMatchmakerMatched += OnReceivedMatchmakerMatched;

                await NakamaSocket.ConnectAsync(NakamaSession);

            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

       

        /// <summary>
        /// Close a Nakama server socket connection.
        /// </summary>
        /// <returns></returns>
        public static async Task CloseConnectionAsync()
        {
            try
            {
                await NakamaSocket.CloseAsync();
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Restore Nakama Server connection .
        /// </summary>
        /// <param name="accessToken">default = null ,the last access token saved will be used !</param>
        public static void RestoreConnection(string accessToken = null)
        {
            try
            {
                NakamaSession = Session.Restore(accessToken ?? AccessToken);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Signal emitted when a socket match state is received.
        /// </summary>
        /// <param name="state">Match state object</param>
        private static void OnReceivedMatchState(IMatchState state)
        {
            MatchStateReceived?.Invoke(state);
        }

        /// <summary>
        /// Signal emitted when a Match presences is received.
        /// </summary>
        /// <param name="presences">Presences Joins And Leaves</param>
        private static void OnReceivedMatchPresence(IMatchPresenceEvent presences)
        {
            MatchPresenceReceived?.Invoke(presences);
        }

        /// <summary>
        /// Signal emitted when a socket channel message is received.
        /// </summary>
        /// <param name="content">Channel Message Object</param>
        private static void OnReceivedChannelMessage(IApiChannelMessage content)
        {
            ChannelMessageReceived?.Invoke(content);
        }

        /// <summary>
        /// Signal emitted when a socket connection is established.
        /// </summary>
        private static void OnConnected()
        {
            Connected?.Invoke();
        }

        /// <summary>
        /// Signal emitted when a socket connection is closed.
        /// </summary>
        private static void OnClosed()
        {
            Closed?.Invoke();
        }

        /// <summary>
        /// Signal emitted when an error has occured.
        /// </summary>
        private static void OnError(Exception exception)
        {
            Error?.Invoke(exception);
        }

        /// <summary>
        /// Signal emitted when a matchmaker match happened with another user.
        /// </summary>
        /// <param name="matched"></param>
        private static void OnReceivedMatchmakerMatched(IMatchmakerMatched matched)
        {
            MatchmakerMatched?.Invoke(matched);
        }

        

        /// <summary>
        /// Call an RPC function from defined in lua modules
        /// </summary>
        /// <param name="id">RPC function name.</param>
        /// <param name="payload">json payload sent to the server function</param>
        /// <returns>IApiRpc</returns>
        public static async Task<IApiRpc> CallRpcAsync(string id , string payload = "")
        {
            try
            {
                return await NakamaClient.RpcAsync(NakamaSession, id, payload);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
                return null;
            }
        }

        /// <summary>
        /// Join a Match by a given match id.
        /// </summary>
        /// <param name="matchId">Payload of an RPC world function.</param>
        /// <returns>IMatch</returns>
        public static async Task<IMatch> JoinMatchAsync(string matchId)
        {
            try
            {
                return await NakamaSocket.JoinMatchAsync(matchId);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
                return null;
            }
        }

        /// <summary>
        /// Join a Match by a given matchmaker matched.
        /// </summary>
        /// <param name="matched"></param>
        /// <returns>IMatch</returns>
        public static async Task<IMatch> JoinMatchAsync(IMatchmakerMatched matched)
        {
            try
            {
                return await NakamaSocket.JoinMatchAsync(matched);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
                return null;
            }
        }

        /// <summary>
        /// Leave a match by a given match id.
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <returns></returns>
        public static async Task LeaveMatchAsync(string matchId)
        {
            try
            {
                await NakamaSocket.LeaveMatchAsync(matchId);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Leave a match by a given Match object.
        /// </summary>
        /// <param name="match">Match object</param>
        /// <returns></returns>
        public static async Task LeaveMatchAsync(IMatch match)
        {
            try
            {
                await NakamaSocket.LeaveMatchAsync(match);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Send a match state WARNING : The lower the size of jsonState the better performance (lower latency)
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="operationCode"></param>
        /// <param name="jsonState"></param>
        /// <returns></returns>
        public static async Task SendMatchStateAsync(string matchId , long operationCode , string jsonState)
        {
            try
            {
                var encoding = Encoding.UTF8;
                var state = encoding.GetBytes(jsonState);
                await NakamaSocket.SendMatchStateAsync(matchId, operationCode, state);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Join a chat Channel.
        /// </summary>
        /// <param name="chatChannelName"></param>
        /// <param name="channelType">Default = Room</param>
        /// <returns></returns>
        public static async Task JoinChatAsync(string chatChannelName , ChannelType channelType = ChannelType.Room)
        {
            try
            {
                ChatChannels[chatChannelName] = await NakamaSocket.JoinChatAsync(chatChannelName, channelType);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Leave A Chat Channel  by Channel name used to join the group.
        /// </summary>
        /// <param name="chatChannelName"></param>
        /// <returns></returns>
        public static async Task LeaveChatAsync(string chatChannelName)
        {
            try
            {
                var channel = ChatChannels[chatChannelName];
                ChatChannels[chatChannelName] = null; // Remove the channel from channels list
                await NakamaSocket.LeaveChatAsync(channel);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Send a chat message to a chat channel.
        /// </summary>
        /// <param name="chatChannelName">Channel name used to join the group.</param>
        /// <param name="content">json formatted message</param>
        /// <returns></returns>
        public static async Task SendChatMsgAsync(string chatChannelName, string content)
        {
            try
            {
                var channel = ChatChannels[chatChannelName];
                await NakamaSocket.WriteChatMessageAsync(channel, content);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Write data to Nakama Storage.
        /// </summary>
        /// <param name="writeObject">list of data </param>
        /// <returns></returns>
        public static async Task StorageWriteAsync(IApiWriteStorageObject[] writeObject)
        {
            try
            {
                await NakamaClient.WriteStorageObjectsAsync(NakamaSession, writeObject);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Read and get data from Nakama storage. 
        /// </summary>
        /// <param name="objectIds"></param>
        /// <returns></returns>
        public static async Task<IApiStorageObjects> StorageReadAsync(IApiReadStorageObjectId[] objectIds)
        {
            try
            {
                return await NakamaClient.ReadStorageObjectsAsync(NakamaSession, objectIds);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
                return null;
            }
        }

        /// <summary>
        /// Create a matchmaker and returns a matchmaker ticket.
        /// </summary>
        /// <param name="matchmakerProperties">Matchmaker properties type of MatchmakerProperties </param>
        /// <returns>IMatchmakerTicket</returns>
        public static async Task<IMatchmakerTicket> CreateMatchmakerAsync(MatchmakerProperties matchmakerProperties)
        {
            try
            {
                var matchmakerTicket = await NakamaSocket.AddMatchmakerAsync(
                    matchmakerProperties.Query,
                    matchmakerProperties.MinCount,
                    matchmakerProperties.MaxCount,
                    matchmakerProperties.StringProperties,
                    matchmakerProperties.NumericProperties
                );
                return matchmakerTicket;
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
                return null;
            }
        }

        /// <summary>
        /// Remove a user from the matchmaker.
        /// </summary>
        /// <param name="matchmakerTicket"></param>
        /// <returns></returns>
        public static async Task RemoveMatchmakerAsync(IMatchmakerTicket matchmakerTicket)
        {
            try
            {
                await NakamaSocket.RemoveMatchmakerAsync(matchmakerTicket);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Get the current session user ID
        /// </summary>
        /// <returns>User ID</returns>
        public static string GetUserId()
        {
            return NakamaSession.UserId;
        }


    }

    /// <summary>
    /// A matchmaker properties , required to create and add a Nakama matchmaker .
    /// </summary>
    public class MatchmakerProperties
    {
        public string Query { get; set; } = "*";
        public int MinCount { get; set; } = 2;
        public int MaxCount { get; set; } = 2;
        public Dictionary<string, string> StringProperties { get; set; } = null;
        public Dictionary<string, double> NumericProperties { get; set; } = null;
    }

}
