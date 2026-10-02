using System;
using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    [Header("Coin Collection")]
    public static int collectedCoins = 0;


    private void Start()
    {
        //Reset The Collected Coins Count When The Game Starts
        collectedCoins = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            collectedCoins++;
            Destroy(other.gameObject);
        }
    }

    public int GetCollectedCoins() => collectedCoins;
}
