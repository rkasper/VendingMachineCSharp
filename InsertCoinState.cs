namespace VendingMachineCSharp
{
    public class InsertCoinState : VendingMachineState
    {
        private static VendingMachineState _instance = null;

        private InsertCoinState()
        {
        }

        protected internal static VendingMachineState Instance()
        {
            // TODO Make it thread-safe?
            if (null == _instance)
            {
                _instance = new InsertCoinState();
            }

            return _instance;
        }

        protected internal override string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            return "INSERT COIN";
        }
    }
}