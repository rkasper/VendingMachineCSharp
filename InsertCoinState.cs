namespace VendingMachineCSharp
{
    public class InsertCoinState : VendingMachineState
    {
        // TODO To make it thread-safe, create the singleton instance here instead of in the factory method.
        private static VendingMachineState _instance; /* = new InsertCoinState();*/

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