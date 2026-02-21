using UnityEngine;
using UnityEngine.Events;

namespace ChefJourney.Economy
{
    /// <summary>
    /// Manages the in-game currency system — coins, gems, shop purchases,
    /// and persistent wallet state via PlayerPrefs.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        private const string CoinsKey = "icooks_coins";
        private const string GemsKey  = "icooks_gems";

        [Header("Starting Balance")]
        [SerializeField] private int startingCoins = 100;
        [SerializeField] private int startingGems = 10;

        // ─── Runtime ───────────────────────────────────────────
        private int _coins;
        private int _gems;

        public int Coins => _coins;
        public int Gems  => _gems;

        // ─── Events ────────────────────────────────────────────
        [HideInInspector] public UnityEvent<int> OnCoinsChanged;
        [HideInInspector] public UnityEvent<int> OnGemsChanged;
        [HideInInspector] public UnityEvent<string> OnPurchaseCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            OnCoinsChanged ??= new UnityEvent<int>();
            OnGemsChanged  ??= new UnityEvent<int>();
            OnPurchaseCompleted ??= new UnityEvent<string>();

            LoadWallet();
        }

        // ─── Coins ─────────────────────────────────────────────
        public void AddCoins(int amount)
        {
            _coins += amount;
            OnCoinsChanged?.Invoke(_coins);
            SaveWallet();
        }

        public bool SpendCoins(int amount)
        {
            if (_coins < amount) return false;
            _coins -= amount;
            OnCoinsChanged?.Invoke(_coins);
            SaveWallet();
            return true;
        }

        // ─── Gems ──────────────────────────────────────────────
        public void AddGems(int amount)
        {
            _gems += amount;
            OnGemsChanged?.Invoke(_gems);
            SaveWallet();
        }

        public bool SpendGems(int amount)
        {
            if (_gems < amount) return false;
            _gems -= amount;
            OnGemsChanged?.Invoke(_gems);
            SaveWallet();
            return true;
        }

        // ─── Shop ──────────────────────────────────────────────
        public bool TryPurchase(string itemId, int coinCost, int gemCost = 0)
        {
            if (_coins < coinCost || _gems < gemCost) return false;

            _coins -= coinCost;
            _gems  -= gemCost;
            OnCoinsChanged?.Invoke(_coins);
            OnGemsChanged?.Invoke(_gems);
            OnPurchaseCompleted?.Invoke(itemId);
            SaveWallet();

            Debug.Log($"[CurrencyManager] Purchased {itemId} for {coinCost}c + {gemCost}g");
            return true;
        }

        // ─── Persistence ───────────────────────────────────────
        private void SaveWallet()
        {
            PlayerPrefs.SetInt(CoinsKey, _coins);
            PlayerPrefs.SetInt(GemsKey, _gems);
            PlayerPrefs.Save();
        }

        private void LoadWallet()
        {
            _coins = PlayerPrefs.GetInt(CoinsKey, startingCoins);
            _gems  = PlayerPrefs.GetInt(GemsKey, startingGems);
        }

        public void ResetWallet()
        {
            _coins = startingCoins;
            _gems  = startingGems;
            SaveWallet();
            OnCoinsChanged?.Invoke(_coins);
            OnGemsChanged?.Invoke(_gems);
        }
    }
}
