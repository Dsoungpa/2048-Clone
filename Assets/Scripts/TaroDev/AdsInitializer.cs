using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string androidId;
    [SerializeField] string iosgameId;
    string gameId;
    [SerializeField] bool testmode = true;

    private void Awake() {
        if(Advertisement.isInitialized)
        {
            Debug.Log("Advertisement is initialized");
            LoadInerstitialAd();
        }
        else{
            InitializeAds();
        }
        
    }


    public void InitializeAds()
    {
        gameId = (Application.platform == RuntimePlatform.IPhonePlayer)?iosgameId:androidId;
        Advertisement.Initialize(gameId, testmode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadInerstitialAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()}) - {message}");
    }

    public void LoadInerstitialAd()
    {
        Advertisement.Load("Interstitial_Android",this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("OnUnityAdsAdLoaded");
        Advertisement.Show(placementId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error showing Ad Unity {placementId}: {error.ToString()} - {message}");
    }


     public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("OnUnityAdsShowFailure");
    }
 
    public void OnUnityAdsShowStart(string placementId) 
    {
        Debug.Log("OnUnityAdsShowStart");
        Time.timeScale = 0;
    }
    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("OnUnityAdsShowClick");
    }
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("OnUnityAdsShowComplete");
        Time.timeScale = 1;
    }
}
