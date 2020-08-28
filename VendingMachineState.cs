using System.ComponentModel;

namespace VendingMachineCSharp
{
    public abstract class VendingMachineState
    {
        protected internal virtual string ViewDisplayMessage(VendingMachine vendingMachine)
        {
            return "";
        }
    }
}