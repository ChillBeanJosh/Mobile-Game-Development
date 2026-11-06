using TMPro;
using UnityEngine;

public class UpgradeMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCurrency;
    private const string MoneySaveKey = "TotalMoney";

    private void Start()
    {
        RefreshCurrencyDisplay();
    }

    private void OnEnable()
    {
        RefreshCurrencyDisplay();
    }

    public void RefreshCurrencyDisplay()
    {
        //Store total money from PlayerPrefs:
        float totalMoney = PlayerPrefs.GetFloat(MoneySaveKey, 0f);

        if (totalCurrency != null)
        {
            totalCurrency.text = $"Total Money: {totalMoney:F0}";
        }
    }
    public void OnClickBack()
    {
        this.gameObject.SetActive(false);
    }
}
