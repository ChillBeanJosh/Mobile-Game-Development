using Unity.Services.LevelPlay;
using UnityEngine;

public class AdsManagerScript : MonoBehaviour
{
    [SerializeField] private string appKey;
    [SerializeField] private string interstitialAdKey;
    
    private LevelPlayInterstitialAd interstitialAd;
    
    private void Awake()
    {
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;
        
        LevelPlay.Init(appKey);
    }
    
    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }
    
    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.LogWarning($"LevelPlay init success");
    
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdKey);
        
        // Register to event listeners
        interstitialAd.OnAdLoaded += (info) => Debug.Log("Interstitial Loaded");
        interstitialAd.OnAdLoadFailed += (error) => Debug.LogError($"Interstitial Load Failed: {error.ErrorMessage}");
        interstitialAd.OnAdDisplayed += (info) => Debug.Log("Interstitial Displayed");
        interstitialAd.OnAdClosed += (info) => 
        {
            Debug.Log("Interstitial Closed. Prefetching next ad...");
            LoadInterstitial(); // Automatically reload the next ad
        };
        
        // Load the initial ad
        LoadInterstitial();      
        ShowInterstitial();
    }
    
    public void LoadInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.LoadAd();
        }
    }
    
    public void ShowInterstitial()
    {
        // Check if the ad is ready before showing
        if (interstitialAd != null && interstitialAd.IsAdReady())
        {
            interstitialAd.ShowAd();
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet.");
        }
    }
    
    void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogWarning($"LevelPlay init error");
    }
}