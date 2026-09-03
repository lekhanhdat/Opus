using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class HoldMembershipDao : BaseDao<RMHoldMemberships>, IHoldMembershipDao
    {
        public void DeleteHoldMembershipsByHoldIds(List<string> holdIds)
        {
            if (holdIds == null || !holdIds.Any())
            {
                return;
            }

            using (var context = GetNewContext())
            {
                var memberships = context.RMHoldMemberships
                    .Where(m => holdIds.Contains(m.HoldId))
                    .ToList();

                if (memberships.Any())
                {
                    context.RMHoldMemberships.RemoveRange(memberships);
                    context.SaveChanges();
                }
            }
        }

        public List<string> GetCurrentUserHoldIds(List<string> holdIds)
        {
            if (holdIds == null || !holdIds.Any())
            {
                return new List<string>();
            }

            using (var context = GetNewContext())
            {
                var userId = TenantLocalValue.LogonUserId;

                var userGroupIds = context.LnkUserGroup
                    .Where(ug => ug.UserId == userId)
                    .Select(ug => ug.GroupId)
                    .ToList();

                return context.RMHoldMemberships
                    .Where(m => holdIds.Contains(m.HoldId)
                        && (m.UserId == userId
                            || userGroupIds.Contains(m.UserId)))
                    .Select(m => m.HoldId)
                    .Distinct()
                    .ToList();
            }
        }
    }
}
