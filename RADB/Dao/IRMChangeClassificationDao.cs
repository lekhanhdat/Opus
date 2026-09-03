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
    public interface IRMChangeClassificationDao
    {
        void AddChange(List<Guid> changeIds, int changeType);
        Task RemoveChangeAsync(int changeType);
        List<Guid> GetAllChange(long ticks, int changeType);

        List<RMChangeClassification> GetAllChangedInfo(long ticks, int changeType);

        List<Guid> GetAllChangeByType(int changeType);

        /// <summary>
        ///  Update ChangeClassifications for term and label (key: TermId/LabelId, value: ChangeType)
        /// </summary>
        /// <param name="changes"></param>
        void AddChangeLabelsAndTerms(Dictionary<Guid, int> changes);
        Task<IEnumerable<RMChangeClassification>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertChangeClassificationTableAsync(IEnumerable<RMChangeClassification> changeClassifications);
        Task<long> MultiGeoDeleteAllChangeClassificationAsync();
    }
}
