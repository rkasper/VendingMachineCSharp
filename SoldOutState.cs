namespace VendingMachineCSharp
{
    public class SoldOutState : VendingMachineState
    {
        private static VendingMachineState _instance = null;

        private SoldOutState()
        {
        }

        protected internal static VendingMachineState Instance()
        {
            if (null == _instance)
            {
                _instance = new SoldOutState();
            }

            return _instance;
        }

        protected internal override string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            if (0 == vendingMachine.Balance)
            {
                vendingMachine.State = State.SoldOut;
                vendingMachine.VMState = InsertCoinState.Instance();
            }
            else
            {
                vendingMachine.State = State.HasCustomerCoins;
                vendingMachine.VMState = HasCustomerCoinsState.Instance();
            }

            return "SOLD OUT";
        }

    }
}