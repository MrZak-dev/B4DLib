using Godot;

namespace B4DLib.Facebook
{
    public class Analytics : Node {

        private static Object AnalyticsObj;
        private const string SingletonName = "FacebookAnalytics";
        
        /// <summary>
        /// Initialize Facebook SDK 
        /// </summary>
        /// <param name="appId">Facebook App ID</param>
        /// <returns>bool</returns>
        public static bool Init(string appId)
        {
            if (!Engine.HasSingleton(SingletonName)) return false;
         
            AnalyticsObj = Engine.GetSingleton(SingletonName);
            AnalyticsObj.Call("Init", appId);
            GD.Print("Facebook Analytics Has Been Initialized");
            
            return true;
        }

        /// <summary>
        /// Enable or disable auto FB sdk initialization
        /// </summary>
        /// <param name="autoInitEnabled"></param>
        public static void SetAutoInitEnabled(bool autoInitEnabled)
        {
            AnalyticsObj.Call("setAutoInitEnabled", autoInitEnabled);
        }

        /// <summary>
        /// Set Auto Log App Events Enabled (App install ,
        /// </summary>
        /// <param name="autoLogEnabled"></param>
        public static void SetAutoLogAppEventsEnabled(bool autoLogEnabled)
        {
            AnalyticsObj.Call("setAutoLogAppEventsEnabled", autoLogEnabled);
        }

        /// <summary>
        /// Enable or disable Advertiser Id Collection
        /// </summary>
        /// <param name="idCollectionEnabled"></param>
        public static void SetAdvertiserIdCollectionEnabled(bool idCollectionEnabled)
        {
            AnalyticsObj.Call("setAdvertiserIdCollectionEnabled", idCollectionEnabled);
        }

        /// <summary>
        /// Enable or disable debug mode
        /// </summary>
        /// <param name="debugEnabled"></param>
        public static void SetDebugEnabled(bool debugEnabled)
        {
            AnalyticsObj.Call("setDebugEnabled", debugEnabled);
        }

        /// <summary>
        /// Log Ad click Event with a given ad name
        /// </summary>
        /// <param name="adType"></param>
        public static void LogAdClickEvent(string adType)
        {
            AnalyticsObj.Call("logAdClickEvent", adType);
        }

        /// <summary>
        /// Log a custom Facebook app event e.g : (LevelFinishEvent)
        /// </summary>
        /// <param name="eventName"></param>
        public static void LogCustomEvent(string eventName)
        {
            AnalyticsObj.Call("logCustomEvent", eventName);
        }

        /// <summary>
        /// Log A custom App Event with  a value to sum and get average in Facebook Analytics Dashboard . 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="valueToSum"></param>
        public static void LogCustomSumEvent(string eventName , double valueToSum)
        {
            AnalyticsObj.Call("logCustomSumEvent", eventName , valueToSum);
        }

    }
}
