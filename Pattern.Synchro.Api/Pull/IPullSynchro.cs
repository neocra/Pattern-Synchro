using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Pattern.Synchro.Client;

namespace Pattern.Synchro.Api.Pull
{
    public interface IPullSynchro
    {
        List<IEntity> GetPull(HttpContext context, DateTime lastSynchro, int previousVersion, int version);
    }
}