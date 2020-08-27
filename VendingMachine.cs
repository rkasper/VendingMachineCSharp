using System.Collections.Generic;

namespace VendingMachineCSharp
{
    internal enum State
    {
        InsertCoin,
        HasCustomerCoins,
        ThankYou,
        Price,
        SoldOut,
        ExactChangeOnly
    }

    public enum Product
    {
        None,
        Cola,
        Chips,
        Candy
    }

    public enum Coin
    {
        Penny,
        Nickel,
        Dime,
        Quarter
    }


    public class VendingMachine
    {
        private readonly Dictionary<Product, int> _inventory; // A list of Products and the number of each one that we have in inventory
        private State _state; // I am a state machine! This is what state I am in.
        private int _displayPrice; // When we're in state State.PRICE, this is the price to display.
        private HashSet<Coin> _coinReturnSlot; // The coins that the machine has ejected into the coin return slot
        private int _balance; // How much money the customers have inserted, in cents
        private Dictionary<Product, int> _priceList; // The products that this machine sells. Maps Product to its price in cents.
        private Dictionary<Coin, int> _customersCoins; // The number of each kind of coin that the customer has inserted for a new purchase
        private Dictionary<Coin, int> _coinVault; // The coins I've collected from customer purchases

        public VendingMachine()
        {
            // A default inventory
            _inventory = new Dictionary<Product, int> {{Product.Candy, 42}, {Product.Cola, 42}, {Product.Chips, 42}};

            InitializeVendingMachine();
        }

        public VendingMachine(Dictionary<Product, int> inventory)
        {
            _inventory = inventory;

            InitializeVendingMachine();
        }

        private void InitializeVendingMachine()
        {
            _state = State.InsertCoin;
            _displayPrice = 0;
            _balance = 0;
            _coinReturnSlot = new HashSet<Coin>();

            _priceList = new Dictionary<Product, int> { { Product.Cola, 100 }, { Product.Chips, 50 }, { Product.Candy, 65 } };

            _customersCoins = InitializeWithNoCoins();
            _coinVault = InitializeWithNoCoins();
        }


        private Dictionary<Coin, int> InitializeWithNoCoins()
        {
            Dictionary<Coin, int> coins = new Dictionary<Coin, int>
            {
                {Coin.Quarter, 0}, {Coin.Dime, 0}, {Coin.Nickel, 0}
            };

            return coins;
        }


        public string ViewDisplayMessage()
            {
                if (_state == State.InsertCoin)
                {
                    return "INSERT COIN";
                }
                else if (_state == State.HasCustomerCoins)
                {
                    return DisplayAmount(_balance);
                }
                else if (_state == State.Price)
                {
                    _state = State.InsertCoin;
                    return "PRICE " + DisplayAmount(_displayPrice);
                }
                else if (_state == State.ThankYou)
                {
                    _state = State.InsertCoin;
                    return "THANK YOU";
                }
                else if (_state == State.SoldOut)
                {
                    if (0 == _balance)
                    {
                        _state = State.InsertCoin;
                    }
                    else
                    {
                        _state = State.HasCustomerCoins;
                    }

                    return "SOLD OUT";
                }
                else // state is EXACT_CHANGE_ONLY
                {
                    return "EXACT CHANGE ONLY";
                }
            }

        private string DisplayAmount(int amount)
        {
            //Console.WriteLine("DisplayAmount(" + amount + ")");
            return $"{amount / 100.0:C}";
        }

        public bool DepositCoin(Coin coin)
        {
            if (coin == Coin.Penny)
            {
                _coinReturnSlot.Add(coin);
                return false;
            }

            _customersCoins[coin] += 1;

            if (coin == Coin.Nickel)
            {
                _balance += 5;
            }
            else if (coin == Coin.Dime)
            {
                _balance += 10;
            }
            else
            {
                _balance += 25;
            }

            _coinReturnSlot =  new HashSet<Coin>();
            _state = State.HasCustomerCoins;
            return true;
        }


        public HashSet<Coin> CheckCoinReturnSlot()
        {
            return _coinReturnSlot;
        }

        public Product SelectProduct(Product product)
        {
            int price = _priceList[product];
            if (IsInInventory(product))
            {
                if (_balance >= price)
                {
                    int changeToMake = _balance - price;
                    HashSet<Coin> change = MakeChange(changeToMake);
                    // change = self.__make_change_from_customers_coins(change_to_make) # Try to make change from the customer's coins
                    // if not change:
                    //      Try to make change from the machine's coin vault
                    //      change = self.__make_change_from_coin_vault(change_to_make)
                    if (changeToMake == 0 || change.Count > 0) // customer can make the purchase
                    {
                        RemoveFromInventory(product);
                        if (changeToMake == 0) // Take all the customer's coins
                        {
                            MoveAllOfCustomersCoinsToVault();
                        }

                        // else when we made change, it got taken care of
                        _state = State.ThankYou;
                        _balance = 0; // because I'm delivering both the product and the change
                        _coinReturnSlot = change;
                        return product;
                    }
                    else // can't make change
                    {
                        _state = State.ExactChangeOnly;
                        return Product.None;
                    }
                }
                else // customer didn't insert enough money
                {
                    _state = State.Price;
                    _displayPrice = price;
                    return Product.None;
                }
            }
            else // selected product is not in inventory
            {
                _state = State.SoldOut;
                return Product.None;
            }
        }

        private bool IsInInventory(Product product)
        {
            int quantity = _inventory[product];
            return quantity > 0;
        }

        // TODO There's a lot of untested code here! Add all the tests.
        // TODO Method too long - refactor it
        private HashSet<Coin> MakeChange(int changeToMake)
        {
            HashSet<Coin> coinsToReturn = new HashSet<Coin>();
            while (changeToMake > 0)
            {
                if (changeToMake >= 25)
                {
                    if (RemoveCustomerCoinFromCacheIfPossible(Coin.Quarter))
                    {
                        coinsToReturn.Add(Coin.Quarter);
                        changeToMake -= 25;
                    }
                    else if (RemoveCustomerCoinFromCacheIfPossible(Coin.Dime))
                    {
                        coinsToReturn.Add(Coin.Dime);
                        changeToMake -= 10;
                    }
                    else if (RemoveCustomerCoinFromCacheIfPossible(Coin.Nickel))
                    {
                        coinsToReturn.Add(Coin.Nickel);
                        changeToMake -= 5;
                    }
                    else if (RemoveCoinFromCoinVaultIfPossible(Coin.Quarter))
                    {
                        coinsToReturn.Add(Coin.Quarter);
                        changeToMake -= 25;
                    }
                    else if (RemoveCoinFromCoinVaultIfPossible(Coin.Dime))
                    {
                        coinsToReturn.Add(Coin.Dime);
                        changeToMake -= 10;
                    }
                    else if (RemoveCoinFromCoinVaultIfPossible(Coin.Nickel))
                    {
                        coinsToReturn.Add(Coin.Nickel);
                        changeToMake -= 5;
                    }
                    else
                    {
                        return new HashSet<Coin>(); // Can't make change
                    }
                }
                else if (changeToMake >= 10)
                {
                    if (RemoveCustomerCoinFromCacheIfPossible(Coin.Dime))
                    {
                        coinsToReturn.Add(Coin.Dime);
                        changeToMake -= 10;
                    }
                    else if (RemoveCustomerCoinFromCacheIfPossible(Coin.Nickel))
                    {
                        coinsToReturn.Add(Coin.Nickel);
                        changeToMake -= 5;
                    }
                    else if (RemoveCoinFromCoinVaultIfPossible(Coin.Dime))
                    {
                        coinsToReturn.Add(Coin.Dime);
                        changeToMake -= 10;
                    }
                    else if (RemoveCoinFromCoinVaultIfPossible(Coin.Nickel))
                    {
                        coinsToReturn.Add(Coin.Nickel);
                        changeToMake -= 5;
                    }
                    else
                    {
                        return new HashSet<Coin>(); // Can't make change
                    }
                }
                else if (changeToMake >= 5)
                {
                    if (RemoveCustomerCoinFromCacheIfPossible(Coin.Nickel))
                    {
                        coinsToReturn.Add(Coin.Nickel);
                        changeToMake -= 5;
                    }
                    else if (RemoveCoinFromCoinVaultIfPossible(Coin.Nickel))
                    {
                        coinsToReturn.Add(Coin.Nickel);
                        changeToMake -= 5;
                    }
                    else
                    {
                        return new HashSet<Coin>(); // Can't make change
                    }
                }
            }

            return coinsToReturn;
        }

        private bool RemoveCustomerCoinFromCacheIfPossible(Coin coin)
        {
            if (IsCustomerCoinStillAvailable(coin))
            {
                RemoveCustomerCoinFromCache(coin);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool RemoveCoinFromCoinVaultIfPossible(Coin coin)
        {
            if (IsCoinInVault(coin))
            {
                RemoveCoinFromVault(coin);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RemoveCustomerCoinFromCache(Coin coin)
        {
            _customersCoins[coin] -= 1;
        }

        private void RemoveCoinFromVault(Coin coin)
        {
            _coinVault[coin] -= 1;
        }

        private bool IsCustomerCoinStillAvailable(Coin coin)
        {
            return _customersCoins[coin] > 0;
        }

        private bool IsCoinInVault(Coin coin)
        {
            return _coinVault[coin] > 0;
        }

        private void RemoveFromInventory(Product product)
        {
            _inventory[product]--;
        }

        private void MoveAllOfCustomersCoinsToVault()
        {
            _coinVault[Coin.Quarter] += _customersCoins[Coin.Quarter];
            _coinVault[Coin.Dime] += _customersCoins[Coin.Dime];
            _coinVault[Coin.Nickel] += _customersCoins[Coin.Nickel];
            _customersCoins = InitializeWithNoCoins();
        }

        public void PressCoinReturnButton()
        {
            _balance = 0;
            _state = State.InsertCoin;

            _coinReturnSlot = new HashSet<Coin>();
            for (int i = 0; i < _customersCoins[Coin.Quarter]; i++)
            {
                _coinReturnSlot.Add(Coin.Quarter);
            }
            for (int i = 0; i < _customersCoins[Coin.Dime]; i++)
            {
                _coinReturnSlot.Add(Coin.Dime);
            }
            for (int i = 0; i < _customersCoins[Coin.Nickel]; i++)
            {
                _coinReturnSlot.Add(Coin.Nickel);
            }
        }
    }
}
