using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pattern.Synchro.Api
{
    public interface IServerCallback
    {
        Task Begin(IHeaderDictionary contextRequest);
    }
}