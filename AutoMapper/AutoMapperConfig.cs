using AutoMapper;

namespace DemoMinimalAPI.AutoMapper
{
    public class AutoMapperConfig
    {
        public static MapperConfiguration RegisterMappings()
        {
            return new MapperConfiguration(config =>
            {
                config.AddProfile(new InputDtoToDomainMappingProfile());
            });
        }
    }
}
