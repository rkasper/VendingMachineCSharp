using System.Collections.Generic;
using NUnit.Framework;

namespace VendingMachineCSharp
{
    public class VendingMachineTests
    {
        [Test]
        public void NUnitWorksProperly()
        {
            Assert.Pass();
        }

        /*
        Accept Coins

        As a vendor
        I want a vending machine that accepts coins
        So that I can collect money from the customer

        The vending machine will accept valid coins (nickels, dimes, and quarters) and reject invalid ones (pennies).
        When a valid coin is inserted the amount of the coin will be added to the current amount and the display will be
        updated. When there are no coins inserted, the machine displays INSERT COIN. Rejected coins are placed in the
        coin return.

        NOTE: The temptation here will be to create Coin objects that know their value. However, this is not how a real
        vending machine works. Instead, it identifies coins by their weight and size and then assigns a value to what
        was inserted. You will need to do something similar. This can be simulated using strings, constants, enums,
        symbols, or something of that nature.
        */
        [Test]
        public void AcceptCoins()
        {
            var vm = VendingMachine.Instance();
            vm.Reset();

            // When we turn on the vending machine, it displays "INSERT COIN".
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());

            // When we add a nickel, it displays the balance: $0.05.
            Assert.True(vm.DepositCoin(Coin.Nickel));
            Assert.AreEqual("$0.05", vm.ViewDisplayMessage());
            Assert.IsEmpty(vm.CheckCoinReturnSlot(), "The coin return slot should be empty.");
            
            // When we add another nickel, it displays the new balance: $0.10
            Assert.True(vm.DepositCoin(Coin.Nickel));
            Assert.AreEqual("$0.10", vm.ViewDisplayMessage());
            
            // When we add a dime, it displays the new balance: $0.20
            Assert.True(vm.DepositCoin(Coin.Dime));
            Assert.AreEqual("$0.20", vm.ViewDisplayMessage());
            
            // When we add a quarter, it displays the new balance: $0.45
            Assert.True(vm.DepositCoin(Coin.Quarter));
            Assert.AreEqual("$0.45", vm.ViewDisplayMessage());
            
            // When we try to add a penny, the penny is placed in the coin return and the balance doesn't change.
            Assert.False(vm.DepositCoin(Coin.Penny), "Should not accept a penny");
            Assert.AreEqual("$0.45", vm.ViewDisplayMessage());
            var expectedCoins = new HashSet<Coin> {Coin.Penny};
            Assert.AreEqual(expectedCoins, vm.CheckCoinReturnSlot(), "Rejected penny should be in coin return slot");
        }

        /*
        Select Product

        As a vendor
        I want customers to select products
        So that I can give them an incentive to put money in the machine

        There are three products: cola for $1.00, chips for $0.50, and candy for $0.65. When the respective button is
        pressed and enough money has been inserted, the product is dispensed and the machine displays THANK YOU. If the
        display is checked again, it will display INSERT COIN and the current amount will be set to $0.00. If there is
        not enough money inserted then the machine displays PRICE and the price of the item and subsequent checks of the
        display will display either INSERT COIN or the current amount as appropriate.
        */
        [Test]
        public void SelectProduct()
        {
            var vm = VendingMachine.Instance();
            vm.Reset();

            // When I insert enough money and select cola,
            // then I get my cola
            // and the display says THANK YOU
            // and then the display says INSERT COIN
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            var product = vm.SelectProduct(Product.Cola);
            Assert.AreEqual(Product.Cola, product, "The machine should give me a cola.");
            Assert.AreEqual("THANK YOU", vm.ViewDisplayMessage());
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());
            
            // ... and then there's no money left in the machine.
            // Given there's no money in the machine
            // when  I select cola
            // then  I receive nothing
            // and  the display tells me the price of the cola
            // and  then the display tells me to INSERT COIN
            product = vm.SelectProduct(Product.Cola);
            Assert.AreEqual(Product.None, product);
            Assert.AreEqual("PRICE $1.00", vm.ViewDisplayMessage());
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());

            // Given there's no money in the machine
            // when  I add a coin, but it's not enough to purchase cola
            // then  I receive nothing
            // and  the display tells me the price of the cola
            // and  then the display tells me to INSERT COIN
            vm.DepositCoin(Coin.Quarter);
            product = vm.SelectProduct(Product.Cola);
            Assert.AreEqual(Product.None, product);
            Assert.AreEqual("PRICE $1.00", vm.ViewDisplayMessage());
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());

            // Purchase chips for 50 cents
            product = vm.SelectProduct(Product.Chips);
            Assert.AreEqual(Product.None, product);
            Assert.AreEqual("PRICE $0.50", vm.ViewDisplayMessage(), "Wrong price for chips");
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());
            vm.DepositCoin(Coin.Quarter);
            product = vm.SelectProduct(Product.Chips);
            Assert.AreEqual(Product.Chips, product);
            Assert.AreEqual("THANK YOU", vm.ViewDisplayMessage());
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());

            // Purchase candy for 65 cents
            product = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(Product.None, product);
            Assert.AreEqual("PRICE $0.65", vm.ViewDisplayMessage(), "Wrong price for candy");
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            product = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(Product.Candy, product);
            Assert.AreEqual("THANK YOU", vm.ViewDisplayMessage());
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());
        }
 
        // Make Change
        //
        // As a vendor
        // I want customers to receive correct change
        // So that they will use the vending machine again
        //
        // When a product is selected that costs less than the amount of money in the machine, then the remaining amount is
        // placed in the coin return.
        [Test]
        public void MakeChange()
        {
            var vm = VendingMachine.Instance();
            vm.Reset();

            // Put a dollar in the machine. Buy a candy. Get 35 cents back.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            var product = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(product, Product.Candy);
            var change = vm.CheckCoinReturnSlot();
            var expectedChange = new HashSet<Coin> {Coin.Quarter, Coin.Dime};
            Assert.AreEqual(expectedChange, change);
            
            // Add 70 more cents, buy another candy. Get 5 cents back.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Dime);
            product = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(Product.Candy, product);
            change = vm.CheckCoinReturnSlot();
            expectedChange = new HashSet<Coin> {Coin.Nickel};
            Assert.AreEqual(expectedChange, change);

            // Add $1.10. Buy a cola. Get 10 cents back.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            product = vm.SelectProduct(Product.Cola);
            Assert.AreEqual(Product.Cola, product);
            change = vm.CheckCoinReturnSlot();
            expectedChange = new HashSet<Coin> {Coin.Dime};
            Assert.AreEqual(expectedChange, change);
            
            // Add $0.50, buy a chips, get 0 cents back.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            product = vm.SelectProduct(Product.Chips);
            Assert.AreEqual(Product.Chips, product);
            change = vm.CheckCoinReturnSlot();
            expectedChange = new HashSet<Coin>();
            Assert.AreEqual(expectedChange, change);
        }

        // Return Coins
        //
        // As a customer
        // I want to have my money returned
        // So that I can change my mind about buying stuff from the vending machine
        //
        // When the return coins button is pressed, the money the customer has placed in the machine is returned and the
        // display shows INSERT COIN.
        [Test]
        public void ReturnCoins()
        {
            var vm = VendingMachine.Instance();
            vm.Reset();

            // Put coins in the machine. Get your coins back.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            vm.PressCoinReturnButton();
            var returnedCoins = vm.CheckCoinReturnSlot();
            var expectedChange = new HashSet<Coin>
            {
                Coin.Quarter,
                Coin.Dime,
                Coin.Nickel
            };
            Assert.AreEqual(expectedChange, returnedCoins);
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());
        }

        // Sold Out
        //
        // As a customer
        // I want to be told when the item I have selected is not available
        // So that I can select another item
        //
        // When the item selected by the customer is out of stock, the machine displays SOLD OUT. If the display is checked
        // again, it will display the amount of money remaining in the machine or INSERT COIN if there is no money in the
        // machine.
        //
        // Note: I think the INSERT COIN state doesn't happen. When I try to buy the product, there's still money in the
        // machine.
        [Test]
        public void SoldOut()
        {
            var inventory = new Dictionary<Product, int> {{Product.Candy, 1}, {Product.Cola, 2}, {Product.Chips, 42}};
            var vm = VendingMachine.Instance();
            vm.Reset(inventory);
            
            // Buy a candy. Buy another candy. Notice that it's out of stock.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            vm.SelectProduct(Product.Candy);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            var product = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(Product.None, product);
            Assert.AreEqual("SOLD OUT", vm.ViewDisplayMessage());
            Assert.AreEqual("$0.65", vm.ViewDisplayMessage());

            // Get my money back. With no money inserted, try to buy a candy. It should tell me "SOLD OUT", then
            // "INSERT COIN"
            vm.PressCoinReturnButton();
            product = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(Product.None, product);
            Assert.AreEqual("SOLD OUT", vm.ViewDisplayMessage());
            Assert.AreEqual("INSERT COIN", vm.ViewDisplayMessage());
        }

        
        // Exact Change Only
        //
        // As a customer
        // I want to be told when exact change is required
        // So that I can determine if I can buy something with the money I have before inserting it
        //
        // When the machine is not able to make change with the money in the machine for any of the items that it sells, it
        // will display EXACT CHANGE ONLY instead of INSERT COIN.
        [Test]
        public void ExactChangeOnly()
        {
            var vm = VendingMachine.Instance();
            vm.Reset();

            // Easiest case: No money in the coin safe yet. Can't make change with the coins I inserted.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);  // That's 75 cents - enough for a candy
            Assert.AreEqual(Product.None, vm.SelectProduct(Product.Candy));
            Assert.AreEqual("EXACT CHANGE ONLY", vm.ViewDisplayMessage());
            Assert.AreEqual(new HashSet<Coin>(), vm.CheckCoinReturnSlot());

            // Buy a cola ($1.00) with 4 quarters. Put 3 more quarters into the machine. Try to buy candy ($0.65). The
            // machine can't make change from , so it tells me so.
            vm.DepositCoin(Coin.Quarter);
            Assert.AreEqual(Product.Cola, vm.SelectProduct(Product.Cola));
            Assert.AreEqual(new HashSet<Coin>(), vm.CheckCoinReturnSlot(),
                "Just made an exact-change purchase - there shouldn't be any coins in the coin return slot.");
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            Assert.AreEqual(Product.None, vm.SelectProduct(Product.Candy));
            Assert.AreEqual("EXACT CHANGE ONLY", vm.ViewDisplayMessage());
            Assert.AreEqual(new HashSet<Coin>(), vm.CheckCoinReturnSlot());
                
            // Get my coins back. Put exact change in (2 quarters + 1 dime + 1 nickel) and buy a candy.
            vm.PressCoinReturnButton();
            Assert.AreEqual(new HashSet<Coin>() {Coin.Quarter, Coin.Quarter, Coin.Quarter}, vm.CheckCoinReturnSlot());
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            Assert.AreEqual(Product.Candy, vm.SelectProduct(Product.Candy));
            Assert.AreEqual(new HashSet<Coin>(), vm.CheckCoinReturnSlot());
        }

        [Test]
        public void ReceiveChangeFromMachinesVault()
        {
            var vm = VendingMachine.Instance();
            vm.Reset();

            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Dime);
            vm.DepositCoin(Coin.Nickel);
            Assert.AreEqual(Product.Candy, vm.SelectProduct(Product.Candy));
            Assert.AreEqual(new HashSet<Coin>(), vm.CheckCoinReturnSlot());

            // The machine's vault now contains: 2 quarters, 1 dime, and 1 nickel. Add 3 quarters (75 cents) and buy a candy.
            // Should receive 10 cents change - the dime from the machine's vault.
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            vm.DepositCoin(Coin.Quarter);
            var candy = vm.SelectProduct(Product.Candy);
            Assert.AreEqual(Product.Candy, candy, "Should receive my candy");
            Assert.AreEqual(new HashSet<Coin>() {Coin.Dime}, vm.CheckCoinReturnSlot(),
                "Should receive change from machine's vault");
        }
    }
}