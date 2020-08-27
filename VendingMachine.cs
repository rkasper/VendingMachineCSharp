using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http.Headers;

namespace VendingMachineCSharp
{
    enum State
    {
        InsertCoin,
        HasCoins, // TODO Rename to HAS_CUSTOMER_COINS
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
        private Dictionary<Product, int> inventory; // A list of Products and the number of each one that we have in inventory
        private State state; // I am a state machine! This is what state I am in.
        private int displayPrice; // When we're in state State.PRICE, this is the price to display.
        private HashSet<Coin> coinReturnSlot; // The coins that the machine has ejected into the coin return slot
        private int balance; // How much money the customers have inserted, in cents
        private Dictionary<Product, int> priceList; // The products that this machine sells. Maps Product to its price in cents.
        private Dictionary<Coin, int> customersCoins; // The number of each kind of coin that the customer has inserted for a new purchase
        private Dictionary<Coin, int> coinVault; // The coins I've collected from customer purchases

        public VendingMachine()
        {
            // A default inventory
            inventory = new Dictionary<Product, int>();
            inventory.Add(Product.Candy, 42);
            inventory.Add(Product.Cola, 42);
            inventory.Add(Product.Chips, 42);

            state = State.InsertCoin;
            displayPrice = 0;
            balance = 0;
            coinReturnSlot = new HashSet<Coin>();

            priceList = new Dictionary<Product, int>();
            priceList.Add(Product.Cola, 100);
            priceList.Add(Product.Chips, 50);
            priceList.Add(Product.Candy, 65);

            customersCoins = InitializeWithNoCoins();
            coinVault = InitializeWithNoCoins();
        }

        public VendingMachine(Dictionary<Product, int> inventory)
        {
            this.inventory = inventory;

            state = State.InsertCoin;
            displayPrice = 0;
            balance = 0;
            coinReturnSlot = new HashSet<Coin>();

            priceList = new Dictionary<Product, int>();
            priceList.Add(Product.Cola, 100);
            priceList.Add(Product.Chips, 50);
            priceList.Add(Product.Candy, 65);

            customersCoins = InitializeWithNoCoins();
            coinVault = InitializeWithNoCoins();
        }


        private Dictionary<Coin, int> InitializeWithNoCoins()
        {
            Dictionary<Coin, int> coins = new Dictionary<Coin, int>();
            coins.Add(Coin.Quarter, 0);
            coins.Add(Coin.Dime, 0);
            coins.Add(Coin.Nickel, 0);

            return coins;
        }


        public string ViewDisplayMessage()
            {
                if (state == State.InsertCoin)
                {
                    return "INSERT COIN";
                }
                else if (state == State.HasCoins)
                {
                    return DisplayAmount(balance);
                }
                else if (state == State.Price)
                {
                    state = State.InsertCoin;
                    return "PRICE " + DisplayAmount(displayPrice);
                }
                else if (state == State.ThankYou)
                {
                    state = State.InsertCoin;
                    return "THANK YOU";
                }
                else if (state == State.SoldOut)
                {
                    if (balance == 0)
                    {
                        state = State.InsertCoin;
                    }
                    else
                    {
                        state = State.HasCoins;
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
            return String.Format("{0:C}", amount / 100.0);
        }

        public bool DepositCoin(Coin coin)
        {
            if (coin == Coin.Penny)
            {
                coinReturnSlot.Add(coin);
                return false;
            }

            customersCoins[coin] += 1;

            if (coin == Coin.Nickel)
            {
                balance += 5;
            }
            else if (coin == Coin.Dime)
            {
                balance += 10;
            }
            else
            {
                balance += 25;
            }

            coinReturnSlot =  new HashSet<Coin>();
            state = State.HasCoins;
            return true;
        }


        public HashSet<Coin> CheckCoinReturnSlot()
        {
            return coinReturnSlot;
        }

        public Product SelectProduct(Product product)
        {
            int price = priceList[product];
            if (IsInInventory(product))
            {
                if (balance >= price)
                {
                    int changeToMake = balance - price;
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
                        state = State.ThankYou;
                        balance = 0; // because I'm delivering both the product and the change
                        coinReturnSlot = change;
                        return product;
                    }
                    else // can't make change
                    {
                        state = State.ExactChangeOnly;
                        return Product.None;
                    }
                }
                else // customer didn't insert enough money
                {
                    state = State.Price;
                    displayPrice = price;
                    return Product.None;
                }
            }
            else // selected product is not in inventory
            {
                state = State.SoldOut;
                return Product.None;
            }
        }

        private bool IsInInventory(Product product)
        {
            int quantity = inventory[product];
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
            customersCoins[coin] -= 1;
        }

        private void RemoveCoinFromVault(Coin coin)
        {
            coinVault[coin] -= 1;
        }

        private bool IsCustomerCoinStillAvailable(Coin coin)
        {
            return customersCoins[coin] > 0;
        }

        private bool IsCoinInVault(Coin coin)
        {
            return coinVault[coin] > 0;
        }

        private void RemoveFromInventory(Product product)
        {
            inventory[product]--;
        }

        private void MoveAllOfCustomersCoinsToVault()
        {
            coinVault[Coin.Quarter] += customersCoins[Coin.Quarter];
            coinVault[Coin.Dime] += customersCoins[Coin.Dime];
            coinVault[Coin.Nickel] += customersCoins[Coin.Nickel];
            customersCoins = InitializeWithNoCoins();
        }

        public void PressCoinReturnButton()
        {
            balance = 0;
            state = State.InsertCoin;

            coinReturnSlot = new HashSet<Coin>();
            for (int i = 0; i < customersCoins[Coin.Quarter]; i++)
            {
                coinReturnSlot.Add(Coin.Quarter);
            }
            for (int i = 0; i < customersCoins[Coin.Dime]; i++)
            {
                coinReturnSlot.Add(Coin.Dime);
            }
            for (int i = 0; i < customersCoins[Coin.Nickel]; i++)
            {
                coinReturnSlot.Add(Coin.Nickel);
            }
        }
    }
}
