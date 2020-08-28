namespace VendingMachineCSharp
{
    internal class InsertCoinState : VendingMachineState
    {
        private static VendingMachineState _instance = null;

        private InsertCoinState()
        {
        }

        public static VendingMachineState Instance()
        {
            // TODO Make it thread-safe?
            if (null == _instance)
            {
                _instance = new InsertCoinState();
            }

            return _instance;
        }

        public string ViewDisplayMessage()
        {
            return "INSERT COIN";
        }
    }
}