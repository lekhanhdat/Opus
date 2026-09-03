using AvePoint.Hybrid.ClientCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK.Services
{
    public interface IRMFSConnManagementService
    {
        [Api(Url = "api/FSConnManagement/CheckConnectionStatus", HttpMethod = "POST")]
        Task<bool> CheckConnectionStatus(string connId);
    }
}
