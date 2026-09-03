using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/FSConnManagement/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class RMFSConnManagementController : RAWebApiBase 
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMFSConnManagementController));

        private IFSConnectionDao _FSConnectionDao;

        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);

        [HttpPost]
        public bool CheckConnectionStatus([FromBody] string connId)
        {
            logger.Info(" RMFSConnManagementController =>CheckConnectionStatus, Param[connId]: " + connId);
            if (string.IsNullOrEmpty(connId)) {
                return false;
            }
            Guid gConnId = Guid.Parse(connId);
            FSConnection conn = FSConnectionDao.GetConnectionById(gConnId);
            if (conn != null && conn.IsPause == 1) {
                return true;
            }
            return false;
        }


    }
}
