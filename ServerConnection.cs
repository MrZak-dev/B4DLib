using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Nakama;
using Encoding = System.Text.Encoding;

namespace B4DLib
{
    public static class ServerConnection
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
        /// Emitted when a Match state is received.
        /// </summary>
        public static event Action<IMatchState> MatchStateReceived;
        /// <summary>
        /// Emitted when a channel message is received.
        /// </summary>
        public static event Action<IApiChannelMessage> ChannelMessageReceived;
        

        #endregion
     
        //Config
        private const string Host = "127.0.01";
        private const string Key = "nakama_godot_demo";
        private const int Port = 7350;
        private const string Scheme = "http";

        //Server
        private static Client NakamaClient;
        public static Session NakamaSession;
        private static Socket NakamaSocket;
        public static readonly Dictionary<string,IMatch> NakamaMatches = new Dictionary<string, IMatch>();
        public static readonly Dictionary<string, IChannel> ChatChannels = new Dictionary<string, IChannel>();

        //User
        private static string AccessToken = string.Empty;


        ///<summary>
        /// Initialize a nakama server connection.
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
        /// Authenticate the user 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <param name="createAccount">true = SignUp , false = login </param>
        /// <returns></returns>
        public static async Task AuthenticateAsync(string email , string password ,string username = null, bool createAccount = true)
        {
            try
            {
                NakamaSession = (Session) await NakamaClient.AuthenticateEmailAsync(email, password, username, createAccount);
                AccessToken = NakamaSession.AuthToken;
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Open a nakama server socket connection
        /// </summary>
        /// <returns></returns>
        public static async Task OpenConnection()
        {
            try
            {
                NakamaSocket = new Socket();

                NakamaSocket.Connected += NakamaSocketOnConnected;
                NakamaSocket.Closed += NakamaSocketOnClosed;
                NakamaSocket.ReceivedMatchState += NakamaSocketOnReceivedMatchState;
                NakamaSocket.ReceivedChannelMessage += NakamaSocketOnReceivedChannelMessage;

                await NakamaSocket.ConnectAsync(NakamaSession);

            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
        }

        /// <summary>
        /// Signal emitted when a socket match state is received.
        /// </summary>
        /// <param name="obj">Match state object</param>
        private static void NakamaSocketOnReceivedMatchState(IMatchState obj)
        {
            MatchStateReceived?.Invoke(obj);
        }

        /// <summary>
        /// Signal emitted when a socket channel message is received.
        /// </summary>
        /// <param name="obj">Channel Message Object</param>
        private static void NakamaSocketOnReceivedChannelMessage(IApiChannelMessage obj)
        {
            ChannelMessageReceived?.Invoke(obj);
        }

        /// <summary>
        /// Signal emitted when a socket connection is established.
        /// </summary>
        private static void NakamaSocketOnConnected()
        {
            Connected?.Invoke();
        }

        /// <summary>
        /// Signal emitted when a socket connection is closed.
        /// </summary>
        private static void NakamaSocketOnClosed()
        {
            Closed?.Invoke();
        }


        /// <summary>
        /// Restore nakama Server connection .
        /// </summary>
        /// <param name="accessToken">default = null ,the last access token saved will be used !</param>
        public static void RestoreConnection(string accessToken = null)
        {
            try
            {
                NakamaSession = (Session) Session.Restore(accessToken ?? AccessToken);
            }
            catch (Exception e)
            {
                GD.PrintErr(e);
            }
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
        /// <returns></returns>
        public static async Task JoinMatchAsync(string matchId)
        {
            try
            {
                NakamaMatches[matchId] = await NakamaSocket.JoinMatchAsync(matchId);
            }
            catch (Exception e)
            {
                GD.Print(e);
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


    }
    
 


}
