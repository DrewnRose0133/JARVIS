using System.Threading.Tasks;

namespace JARVIS.Modules.Devices.Interfaces
{
    public interface IThermostatService
    {
        Task SetThermostatAsync(string thermostatId, int temp);

        Task RaiseThermostatAsync(string thermostatId, int degree);

        Task LowerThermostatAsync(string thermostatId, int degree);

        Task GetThermostatTempAsync(string thermostatId);
    }
}
