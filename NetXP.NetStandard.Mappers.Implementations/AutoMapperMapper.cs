using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;

namespace NetXP.NetStandard.Mappers.Implementation
{
    public class AutoMapperMapper : IMapper
    {
        private readonly AutoMapper.IMapper mapper;

        public IConfigurationProvider ConfigurationProvider
        { get { return this.mapper.ConfigurationProvider; } }

        public AutoMapperMapper(AutoMapper.IMapper mapper)
        {
            this.mapper = mapper;
        }

        public T2 Map<T1, T2>(T1 list)
        {
            return this.mapper.Map<T1, T2>(list);
        }

        public void Map<T1, T2>(T1 source, T2 dest)
        {
            this.mapper.Map(source, dest);
        }

        public TDestination Map<TDestination>(object source)
        {
            return this.mapper.Map<TDestination>(source);
        }
    }
}
