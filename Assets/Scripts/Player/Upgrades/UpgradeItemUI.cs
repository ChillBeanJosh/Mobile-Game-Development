using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("Upgrade Configuration:")]
    [SerializeField] private string upgradeID;
    [SerializeField] private string upgradeDisplayName;

    [Header("UI References:")]
    [SerializeField] private TMP_Text text1;
    [SerializeField] private TMP_Text text2;
    [SerializeField] private Button button1;
    [SerializeField] private TMP_Text buttonText;

    [Header("Progress Bar:")]
    [SerializeField] private Transform progressBarContainer;
    [SerializeField] private Image segmentPrefab;

    [SerializeField] private Color filledColor = Color.green;
    [SerializeField] private Color emptyColor = Color.gray;

    private readonly List<Image> progressSegments = new List<Image>();

    private void Awake()
    {
        if (StatusUpgrade.Instance == null)
        {
            Debug.LogError("Upgrade System reference is missing!", this);
            enabled = false;
            return;
        }
        BuildProgressBar();
    }

    private void OnEnable()
    {
        if (button1 != null) button1.onClick.AddListener(PurchaseUpgrade);
        if (StatusUpgrade.Instance != null)  StatusUpgrade.Instance.OnUpgradeDataChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (button1 != null) button1.onClick.RemoveListener(PurchaseUpgrade);
        if (StatusUpgrade.Instance != null) StatusUpgrade.Instance.OnUpgradeDataChanged -= RefreshUI;      
    }

    private void BuildProgressBar()
    {
        if (progressBarContainer == null || segmentPrefab == null)
        {
            Debug.LogError("Progress bar references are missing!", this);
            return;
        }

        int maxSegments = StatusUpgrade.Instance.GetMaximumUpgrades();

        for (int i = 0; i < maxSegments; i++)
        {
            Image segment = Instantiate(segmentPrefab, progressBarContainer);

            segment.name = $"Upgrade Segment {i + 1}";
            segment.color = emptyColor;

            progressSegments.Add(segment);
        }
    }

    private void UpdateProgressBar(int currentLevel)
    {
        for (int i = 0; i < progressSegments.Count; i++)
        {
            progressSegments[i].color = i < currentLevel ? filledColor : emptyColor;
        }
    }

    private void PurchaseUpgrade()
    {
        bool purchaseSuccessful = StatusUpgrade.Instance.TryPurchaseUpgrade(upgradeID);
        if (purchaseSuccessful) RefreshUI();      
    }


    public void RefreshUI()
    {
        if (StatusUpgrade.Instance == null) return;

        int currentLevel = StatusUpgrade.Instance.GetUpgradeLevel(upgradeID);
        int maximumLevel = StatusUpgrade.Instance.GetMaximumUpgrades();

        if (currentLevel < 0)
        {
            Debug.LogError($"Invalid Upgrade ID: {upgradeID}", this);
            button1.interactable = false;
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 0, maximumLevel);

        // Text 1: Upgrade Name.
        if (text1 != null)
        {
            text1.text = upgradeDisplayName;
        }

        // Text 2: Upgrade Level and Current Amount.
        if (text2 != null)
        {
            float amount =  StatusUpgrade.Instance.GetUpgradeAmount(upgradeID);
            text2.text = $"Level {currentLevel}/{maximumLevel} | {amount:0.##}";
        }

        // Segmented Progress Bar.
        UpdateProgressBar(currentLevel);

        // Purchase Button.
        bool isMaxed = currentLevel >= maximumLevel;

        if (button1 != null) button1.interactable = !isMaxed && StatusUpgrade.Instance.CanPurchaseUpgrade(upgradeID);
        

        // Display next purchase price.
        if (buttonText != null)
        {
            buttonText.text = isMaxed
                ? "MAX"
                : $"Upgrade - ${StatusUpgrade.Instance.GetNextUpgradeCost()}";
        }
    }
}
