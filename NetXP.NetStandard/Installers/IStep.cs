using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Installers
{
    public interface IStep
    {
        int Step { get; }
        bool IsValid();
        Task<bool> IsValidAsync();

        bool IsFinished();
        void StepLoaded();
    }
}
