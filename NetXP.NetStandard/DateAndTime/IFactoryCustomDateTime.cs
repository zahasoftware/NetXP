﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetXP.DateAndTime
{
    public interface IFactoryCustomDateTime
    {
        DateTime CreateNew(DateTime dt);
        DateTime CreateSingleton();
    }
}
