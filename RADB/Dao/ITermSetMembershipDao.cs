/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ITermSetMembershipDao:IBaseDao<RMTermSetMembership>
    {
        bool IsTermUsed(int termId);

        List<RMTermSetMembership> GetRMTermSetMemberships(int[] termIds, bool isWithRemoved = false);
        int GetSubTermCountByTermSetId(int termSetId);
        List<RMTermSetMembership> GetSubTermMembershipsByTermSetId(int termSetId);
        List<RMTermSetMembership> GetSubTermMembershipByTermId(int termId);

        RMTermSetMembership GetByTermNameAndParentId(int parentId, string termName, bool isRootTerm = false);

        void DeleteAllMemberShips();
        RMTermSetMembership GetMembershipByTermId(int termId);

        string GetMaxDeepTermPath();
        Task<IEnumerable<RMTermSetMembership>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertTermSetMembershipTableAsync(IEnumerable<RMTermSetMembership> termSetMemberships);
        Task<long> MultiGeoDeleteAllTermSetMembershipAsync();
    }
}
