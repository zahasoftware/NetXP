using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;

namespace NetXP.NetStandard
{
    /// <summary>
    /// Mapper interface by convention
    /// </summary>
    public interface IMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);

        void Map<TSource, TDestination>(TSource source, TDestination dest);

        TDestination Map<TDestination>(object source);
    }
}
