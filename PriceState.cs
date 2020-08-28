namespace VendingMachineCSharp
{
    public class PriceState : VendingMachineState
    {
        private static VendingMachineState _instance = null;

        private PriceState()
        {
        }

        protected internal static VendingMachineState Instance()
        {
            if (null == _instance)
            {
                _instance = new PriceState();
            }

            return _instance;
        }

        protected internal override string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            vendingMachine.VMState = InsertCoinState.Instance();
            return "PRICE " + vendingMachine.DisplayAmount(vendingMachine.DisplayPrice);
        }
    }
}
