using TMPro;
using UnityEngine;

public class UpgradeMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCurrency;

    private const string MoneySaveKey = "TotalMoney";

    private void OnEnable()
    {
        if (StatusUpgrade.Instance != null) StatusUpgrade.Instance.OnUpgradeDataChanged += RefreshCurrencyDisplay;
        RefreshCurrencyDisplay();
    }

    private void OnDisable()
    {
        if (StatusUpgrade.Instance != null) StatusUpgrade.Instance.OnUpgradeDataChanged -= RefreshCurrencyDisplay;   
    }

    public void RefreshCurrencyDisplay()
    {
        if (StatusUpgrade.Instance == null || totalCurrency == null) return;

        float totalMoney = StatusUpgrade.Instance.GetCurrentMoney();
        totalCurrency.text = $"Total Money: {totalMoney:N0}";
    }


    public void OnClickBack()
    {
        this.gameObject.SetActive(false);
    }
}
