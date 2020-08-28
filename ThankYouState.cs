namespace VendingMachineCSharp
{
    public class ThankYouState : VendingMachineState
    {
        private static VendingMachineState _instance = null;

        private ThankYouState()
        {
        }

        protected internal static VendingMachineState Instance()
        {
            if (null == _instance)
            {
                _instance = new ThankYouState();
            }

            return _instance;
        }

        protected internal override string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            vendingMachine.State = State.InsertCoin;
            vendingMachine.VMState = InsertCoinState.Instance();
            return "THANK YOU";
        }
    }
}