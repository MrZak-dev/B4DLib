using Godot;
using Timer = System.Timers.Timer;

namespace B4DLib.Google
{
    public class Admob : Node
    {
        //Ads ids 
        private const string BannerId = "ca-app-pub-3940256099942544/6300978111";
        private const string InterstitialId = "ca-app-pub-3940256099942544/1033173712";
        private const string RewardedId = "ca-app-pub-3940256099942544/5224354917";

        //Ads Settings 
        private const bool IsBannerOnTop = false;
        private const bool IsReal = true;
        private const bool IsChildDirected = true;
        private const string MaxAdContentRate = "G";

        //Ads Timers
        //private readonly Timer TimerLoader = new Timer(20000); //20sec
        private readonly Timer AdsBetweenTimer = new Timer(65000);//65sec

        //GDPR Consent Signal
        [Signal]
        public delegate void ConsentResult(bool result);

        //GamePlay Ads
        public static bool IsRewardedRedeemed;
        private static bool AdsBetweenCompleted = true;


        private static readonly GDScript AdmobClass = ResourceLoader.Load<GDScript>("res://admob-lib/admob.gd");//Admob Lib path
        private static readonly Node AdmobAds = (Node) AdmobClass.New();

        public override void _Ready()
        {
            Connect(nameof(ConsentResult), this, nameof(OnConsent));
            AdmobAds.Connect("rewarded", this, nameof(OnReward));
            AdmobAds.Connect("interstitial_closed", this, nameof(OnInterstitialClosed));
            SetIds();
        }

        private void SetIds()
        {
            AdmobAds.Set("banner_id", BannerId);
            AdmobAds.Set("interstitial_id", InterstitialId);
            AdmobAds.Set("rewarded_id", RewardedId);
        }

        private void SetAddSettings(bool consentResult)
        {
            AdmobAds.Set("banner_on_top", IsBannerOnTop);
            AdmobAds.Set("is_real", IsReal);
            AdmobAds.Set("child_directed", IsChildDirected);
            AdmobAds.Set("is_personalized", consentResult);
            AdmobAds.Set("max_ad_content_rate", MaxAdContentRate);
        }

        private void OnConsent(bool result)
        {
            SetAddSettings(result);
            AddChild(AdmobAds);

            LoadAds();
            //SetLoaderTimer();
            SetAdsBetweenTimer();
        }

        public static void LoadBanner()
        {
            AdmobAds.Call("load_banner");
        }

        private static void LoadInterstitial()
        {
            AdmobAds.Call("load_interstitial");
        }

        private static void LoadRewarded()
        {
            AdmobAds.Call("load_rewarded_video");
        }

        public static void LoadAds()
        {
            if (!IsInterstitialLoaded())
                LoadInterstitial();

            if (!IsRewardedLoaded())
                LoadRewarded();
        }

        private static bool IsInterstitialLoaded()
        {
            return (bool)AdmobAds.Call("is_interstitial_loaded");
        }

        private static bool IsRewardedLoaded()
        {
            return (bool)AdmobAds.Call("is_rewarded_video_loaded");
        }

        public static void ShowBanner()
        {
            AdmobAds.Call("show_banner");
        }

        public static void HideBanner()
        {
            AdmobAds.Call("hide_banner");
        }

        public static void ShowInterstitial()
        {
            if (!AdsBetweenCompleted) return;
            AdmobAds.Call("show_interstitial");
        }

        public static void ShowRewarded()
        {
            AdmobAds.Call("show_rewarded_video");
        }

        //Rewarded Signal
        private void OnReward(string currency, int amount)
        {
            GD.Print("rewarded callback");
            IsRewardedRedeemed = true;
            GetTree().ReloadCurrentScene();
        }

        //Interstitial Signal
        private void OnInterstitialClosed()
        {
            GD.Print("interstitial callback");

            AdsBetweenCompleted = false;
            AdsBetweenTimer.Start();
        }

        //private void SetLoaderTimer()
        //{
        //    TimerLoader.Elapsed += (sender, args) =>
        //    {
        //        LoadAds();
        //    };
        //    TimerLoader.Start();
        //}

        private void SetAdsBetweenTimer()
        {
            AdsBetweenTimer.Elapsed += (sender, args) =>
            {
                AdsBetweenCompleted = true;
            };
            AdsBetweenTimer.AutoReset = false;
        }
    }
}
