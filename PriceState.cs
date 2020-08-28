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

        protected internal override void TransitionTo(VendingMachine vendingMachine, State nextState, VendingMachineState nextVMState)
        {
            vendingMachine.State = nextState;
            vendingMachine.VMState = nextVMState;
        }

        protected internal override string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            TransitionTo(vendingMachine, State.InsertCoin, InsertCoinState.Instance());
            return "PRICE " + vendingMachine.DisplayAmount(vendingMachine.DisplayPrice);
        }
    }
}
