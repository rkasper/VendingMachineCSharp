namespace VendingMachineCSharp
{
    public partial class VendingMachine
    {
        public class HasCustomerCoinsState : VendingMachineState
        {
            private static VendingMachineState _instance = null;

            private HasCustomerCoinsState()
            {
            }

            protected internal static VendingMachineState Instance()
            {
                if (null == _instance)
                {
                    _instance = new HasCustomerCoinsState();
                }

                return _instance;
            }

            protected internal override string ViewDisplayMessage()
            {
                return VendingMachine.Instance().DisplayAmount(VendingMachine.Instance()._balance);
            }
        }
    }
}