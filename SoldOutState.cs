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
        protected internal override void TransitionTo(VendingMachine vendingMachine, State nextState, VendingMachineState nextVMState)
        {
            vendingMachine.State = nextState;
            vendingMachine.VMState = nextVMState;
        }


        protected internal override string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            if (0 == vendingMachine.Balance)
            {
                TransitionTo(vendingMachine, State.SoldOut, InsertCoinState.Instance());
            }
            else
            {
                TransitionTo(vendingMachine, State.HasCustomerCoins, HasCustomerCoinsState.Instance());
            }

            return "SOLD OUT";
        }

    }
}