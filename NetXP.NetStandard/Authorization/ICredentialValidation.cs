using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.NetStandard.Authorization
{
    public interface ICredentialValidation
    {
        /// <summary>
        /// User validation for authentication
        /// </summary>
        /// <param name="credential"></param>
        void Validate(Credential credential);
    }
}
