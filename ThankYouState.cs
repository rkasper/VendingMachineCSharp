namespace VendingMachineCSharp
{
    public partial class VendingMachine
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
                vendingMachine._vmState = InsertCoinState.Instance();
                return "THANK YOU";
            }
        }
    }
}