using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;

namespace ManejoPresupuesto.Services
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<cuenta, CreateCuentaView>();
        }
    }
}
