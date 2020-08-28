using System.ComponentModel;

namespace VendingMachineCSharp
{
    public abstract class VendingMachineState
    {
        protected internal virtual void TransitionTo(VendingMachine vendingMachine,
            State nextState,
            VendingMachineState nextVMState)
        {
        }

        protected internal virtual string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            TransitionTo(null, 0, null);
            return "";
        }
    }
}