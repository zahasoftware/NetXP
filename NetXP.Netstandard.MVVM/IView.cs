using System;
using System.Collections.Generic;
using System.Text;

namespace NetXP.NetStandard.MVVM
{
    public interface IView
    {
        object DataContext { get; set; }

        void Close();
    }
}
