using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IHoldMembershipDao
    {
        void DeleteHoldMembershipsByHoldIds(List<string> holdIds);
        List<string> GetCurrentUserHoldIds(List<string> holdIds);
    }
}
