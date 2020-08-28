using System.ComponentModel;

namespace VendingMachineCSharp
{
    public partial class VendingMachine
    {

        public abstract class VendingMachineState
        {
            protected internal virtual string ViewDisplayMessage()
            {
                return "";
            }
        }
    }
}