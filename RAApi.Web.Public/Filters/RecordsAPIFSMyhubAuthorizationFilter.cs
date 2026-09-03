using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace AvePoint.RA.Api.Web.Public.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RecordsAPIFSMyhubAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
    {
        private static readonly IRALogger logger =
            RALogger.GetInstance(typeof(RecordsAPIFSMyhubAuthorizationFilter));

        public List<FSConnectionOwnerType> UserRoles { get; private set; }

        public IAccountDao AccountDao =>
            PlatformWindsorManager.GetService(typeof(IAccountDao)) as IAccountDao;

        public IRMFSConnectionAndOwnerRelationshipDao RMFSConnectionAndOwnerRelationshipDao =>
            PlatformWindsorManager.GetService(typeof(IRMFSConnectionAndOwnerRelationshipDao))
                as IRMFSConnectionAndOwnerRelationshipDao;

        public ILnkUserGroupDao LnkUserGroupDao =>
            PlatformWindsorManager.GetService(typeof(ILnkUserGroupDao)) as ILnkUserGroupDao;

        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();

        public RecordsAPIFSMyhubAuthorizationFilter(FSConnectionOwnerType[] userRoles)
        {
            UserRoles = new List<FSConnectionOwnerType>(userRoles);
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();

            var methodFilter = endpoint?.Metadata
                .GetOrderedMetadata<RecordsAPIFSMyhubAuthorizationFilter>()
                ?.LastOrDefault();

            if (methodFilter != null &&
                methodFilter != this)
            {
                logger.Info("Method-level authorization filter detected. Skip controller-level filter.");
                return;
            }

            if (!UserRoles.Any())
            {
                logger.Info("No roles configured. Skip authorization.");
                return;
            }

            var userIds = new List<int>();
            try
            {
                userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get removed/group ids for user [{TenantLocalValue.LogonUserId}]. Fallback to active user only. Error: {ex}");
            }
            var hasPermission = (await RMFSConnectionAndOwnerRelationshipDao
                .GetConnectionsByUserIdsAndRoles(userIds.Distinct().ToList(), UserRoles))
                .Any();

            if (!hasPermission)
            {
                context.Result = new JsonResult(new { hasPermission = false })
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
        }
    }
}
