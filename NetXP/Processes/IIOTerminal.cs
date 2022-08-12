using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NetXP.Processes
{
    public interface IIOTerminal
    {
        ProcessOutput Execute(ProcessInput processInput);
    }
}
