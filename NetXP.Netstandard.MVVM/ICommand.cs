using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.MVVM
{
    public interface ICommand : System.Windows.Input.ICommand 
    {
        void RaiseCanExecuteChanged();
    }
}
