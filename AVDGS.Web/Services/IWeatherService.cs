using System.Threading;
using System.Threading.Tasks;
using AVDGS.Web.Models;

namespace AVDGS.Web.Services
{
    public interface IWeatherService
    {
        Task<WeatherSnapshot?> GetCurrentAsync(CancellationToken ct = default);
    }
}