using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pattern.Synchro.Api
{
    public interface IDeviceInformation
    {
        Task<DateTime?> GetLastSynchro(HttpContext context, Guid deviceId);
        Task SaveLastSynchro(HttpContext context, Guid deviceId, DateTime dateTime,
            DateTime entitiesLastLocalSyncDateTime, int version);
        Task<DateTime?> GetLastLocalSynchro(HttpContext context, Guid deviceId);
        Task<int?> GetVersion(HttpContext context, Guid deviceId);
    }
}