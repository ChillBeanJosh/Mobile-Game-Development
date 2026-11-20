using UnityEngine;
using System;

public class StatusUpgrade : MonoBehaviour
{
    public static StatusUpgrade Instance { get; private set; }

    // Shared currency key used by PlayerHealth and StatusUpgrade.
    public const string MoneySaveKey = "TotalMoney";

    //Upgrade IDs used by the UI:
    public const string FadeDurationID = "FadeDuration";
    public const string MoveID = "Move";
    public const string DragID = "Drag";
    public const string ParryDurationID = "ParryDuration";
    public const string ParrySpeedIncreaseID = "ParrySpeedIncrease";
    public const string ParrySpeedReductionID = "ParrySpeedReduction";
    public const string ScoreMultiplierID = "ScoreMultiplier";

    //
    private const string FadeDurationLevelKey = "FadeDurationLevel";
    private const string MoveUpgradeLevelKey = "MoveUpgradeLevel";
    private const string DragUpgradeLevelKey = "DragUpgradeLevel";
    private const string ParryDurationLevelKey = "ParryDurationLevel";
    private const string ParrySpeedIncreaseLevelKey = "ParrySpeedIncreaseLevel";
    private const string ParrySpeedReductionLevelKey = "ParrySpeedReductionLevel";
    private const string ScoreMultiplierLevelKey = "ScoreMultiplierLevel";



    [Header("Upgrade Pricing:")]
    [SerializeField] private int baseUpgradeCost = 100;
    [SerializeField, Min(1.01f)] private float upgradeCostMultiplier = 1.5f;
    [SerializeField, Min(1)] private int maximumUpgrades = 10;

    [Header("Joystick Upgrades:")]
    [SerializeField] private float fadeDuration_IncrementValue;

    [Header("Movement Upgrades:")]
    [SerializeField] private float moveAcceleration_IncrementValue;
    [SerializeField] private float dragCoefficient_DecrementValue;

    [Header("Parry Upgrades:")]
    [SerializeField] private float parryDuration_IncrementValue;
    [SerializeField] private float parryMovespeedIncreaseFactor_IncrementValue;
    [SerializeField] private float parryMoveSpeedReductionFactor_DecrementValue;

    [Header("Score Upgrades:")]
    [SerializeField] private float scoreMultiplier_IncrementValue;


    //Current Upgrade Levels:
    private int fadeDurationLevel;
    private int moveUpgradeLevel;
    private int dragUpgradeLevel;
    private int parryDurationLevel;
    private int parrySpeedIncreaseLevel;
    private int parrySpeedReductionLevel;
    private int scoreMultiplierLevel;


    //Notify all upgrade panels when something changes:
    public event Action OnUpgradeDataChanged;


    //Total upgrades purchased across all categories:
    private int TotalUpgradesPurchased =>
        fadeDurationLevel +
        moveUpgradeLevel +
        dragUpgradeLevel +
        parryDurationLevel +
        parrySpeedIncreaseLevel +
        parrySpeedReductionLevel +
        scoreMultiplierLevel;


    private void Awake()
    {
        // Prevent duplicate instances.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadUpgradeLevels();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;     
    }

    private void LoadUpgradeLevels()
    {
        fadeDurationLevel = PlayerPrefs.GetInt(FadeDurationLevelKey, 0);
        moveUpgradeLevel = PlayerPrefs.GetInt(MoveUpgradeLevelKey, 0);
        dragUpgradeLevel = PlayerPrefs.GetInt(DragUpgradeLevelKey, 0);
        parryDurationLevel = PlayerPrefs.GetInt(ParryDurationLevelKey, 0);
        parrySpeedIncreaseLevel = PlayerPrefs.GetInt(ParrySpeedIncreaseLevelKey, 0);
        parrySpeedReductionLevel = PlayerPrefs.GetInt(ParrySpeedReductionLevelKey, 0);
        scoreMultiplierLevel = PlayerPrefs.GetInt(ScoreMultiplierLevelKey, 0);
    }


    public float GetCurrentMoney() => PlayerPrefs.GetFloat(MoneySaveKey, 0f);
    public int GetNextUpgradeCost()
    {
        float cost = baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, TotalUpgradesPurchased);
        return Mathf.CeilToInt(cost);
    }
    public int GetMaximumUpgrades() => maximumUpgrades;
    public int GetTotalUpgradesPurchased() => TotalUpgradesPurchased;

    
    //Given Generic ID String -> Return Level Increment:
    public int GetUpgradeLevel(string upgradeID)
    {
        switch (upgradeID)
        {
            case FadeDurationID:
                return fadeDurationLevel;

            case MoveID:
                return moveUpgradeLevel;

            case DragID:
                return dragUpgradeLevel;

            case ParryDurationID:
                return parryDurationLevel;

            case ParrySpeedIncreaseID:
                return parrySpeedIncreaseLevel;

            case ParrySpeedReductionID:
                return parrySpeedReductionLevel;

            case ScoreMultiplierID:
                return scoreMultiplierLevel;

            default:
                return -1;
        }
    }
    
    //Given Generic ID String -> Return Level Amount:
    public float GetUpgradeAmount(string upgradeID)
    {
        switch (upgradeID)
        {
            case FadeDurationID:
                return GetUpgradedFadeDuration();

            case MoveID:
                return GetUpgradedMove();

            case DragID:
                return GetUpgradedDrag();

            case ParryDurationID:
                return GetUpgradedParryDuration();

            case ParrySpeedIncreaseID:
                return GetUpgradedParrySpeedIncreaseFactor();

            case ParrySpeedReductionID:
                return GetUpgradedParrySpeedDecreaseFactor();

            case ScoreMultiplierID:
                return GetUpgradedScoreMultiplier();

            default:
                return 0f;
        }
    }


    //Given Generic ID String -> Determine If Possible To Upgrade:
    public bool CanPurchaseUpgrade(string upgradeID)
    {
        int level = GetUpgradeLevel(upgradeID);
        return (level >= 0 && level < maximumUpgrades) && (GetCurrentMoney() >= GetNextUpgradeCost());
    }

    //Given Generic ID String -> Perform Upgrade Action:
    public bool TryPurchaseUpgrade(string upgradeID)
    {
        switch (upgradeID)
        {
            case FadeDurationID:
                return PurchaseUpgrade(FadeDurationLevelKey, ref fadeDurationLevel);

            case MoveID:
                return PurchaseUpgrade(MoveUpgradeLevelKey, ref moveUpgradeLevel);

            case DragID:
                return PurchaseUpgrade(DragUpgradeLevelKey, ref dragUpgradeLevel);

            case ParryDurationID:
                return PurchaseUpgrade(ParryDurationLevelKey, ref parryDurationLevel);

            case ParrySpeedIncreaseID:
                return PurchaseUpgrade(ParrySpeedIncreaseLevelKey, ref parrySpeedIncreaseLevel);

            case ParrySpeedReductionID:
                return PurchaseUpgrade(ParrySpeedReductionLevelKey, ref parrySpeedReductionLevel);

            case ScoreMultiplierID:
                return PurchaseUpgrade(ScoreMultiplierLevelKey, ref scoreMultiplierLevel);

            default:
                Debug.LogError($"Unknown upgrade ID: {upgradeID}");
                return false;
        }
    }

    private bool PurchaseUpgrade(string saveKey, ref int upgradeLevel)
    {
        if (upgradeLevel >= maximumUpgrades)
        {
            Debug.Log("This upgrade has reached its maximum level!");
            return false;
        }

        float currentMoney = GetCurrentMoney();
        int cost = GetNextUpgradeCost();
        if (currentMoney < cost)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        //Deduct Money + Increment Level Upgrade:
        PlayerPrefs.SetFloat(MoneySaveKey, currentMoney - cost);
        upgradeLevel++;

        //Store + Save:
        PlayerPrefs.SetInt(saveKey, upgradeLevel);
        PlayerPrefs.Save();
        Debug.Log($"Upgrade purchased! Cost: {cost}");

        //Update UI:
        OnUpgradeDataChanged?.Invoke();
        return true;
    }

    public void ResetAllProgress()
    {
        //Reset Levels:
        PlayerPrefs.SetInt("UnlockedLevels", 1);

        //Reset Upgrades:
        fadeDurationLevel = 0;
        moveUpgradeLevel = 0;
        dragUpgradeLevel = 0;
        parryDurationLevel = 0;
        parrySpeedIncreaseLevel = 0;
        parrySpeedReductionLevel = 0;
        scoreMultiplierLevel = 0;

        PlayerPrefs.SetInt(FadeDurationLevelKey, 0);
        PlayerPrefs.SetInt(MoveUpgradeLevelKey, 0);
        PlayerPrefs.SetInt(DragUpgradeLevelKey, 0);
        PlayerPrefs.SetInt(ParryDurationLevelKey, 0);
        PlayerPrefs.SetInt(ParrySpeedIncreaseLevelKey, 0);
        PlayerPrefs.SetInt(ParrySpeedReductionLevelKey, 0);
        PlayerPrefs.SetInt(ScoreMultiplierLevelKey, 0);

        //Reset Currency:
        PlayerPrefs.SetFloat(MoneySaveKey, 0f);


        PlayerPrefs.Save();
        OnUpgradeDataChanged?.Invoke();

        Debug.Log("All currency and upgrades have been reset!");
    }


    //Getters For Upgraded Stats:
    public float GetUpgradedFadeDuration() => fadeDuration_IncrementValue * fadeDurationLevel;
    public float GetUpgradedMove() => moveAcceleration_IncrementValue * moveUpgradeLevel;
    public float GetUpgradedDrag() => dragCoefficient_DecrementValue * dragUpgradeLevel;
    public float GetUpgradedParryDuration() => parryDuration_IncrementValue * parryDurationLevel;
    public float GetUpgradedParrySpeedIncreaseFactor() => parryMovespeedIncreaseFactor_IncrementValue * parrySpeedIncreaseLevel;
    public float GetUpgradedParrySpeedDecreaseFactor() => parryMoveSpeedReductionFactor_DecrementValue * parrySpeedReductionLevel;
    public float GetUpgradedScoreMultiplier() => scoreMultiplier_IncrementValue * scoreMultiplierLevel; 
}
