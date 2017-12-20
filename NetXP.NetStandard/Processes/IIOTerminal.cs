using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetXP.NetStandard.Processes
{
    public interface IIOTerminal
    {
        ProcessOutput Execute(ProcessInput processInput);
    }
}
