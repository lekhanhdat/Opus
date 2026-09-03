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
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMMLTermDao : IBaseDao<RMMLTerm>
    {
        void AddOrUpdateTerms(List<MLTermDto> dtos, Guid trainingModeId, int maxTermCount);
        void DeleteTerms(List<Guid> ids);
        void MarkTermRemoveStatus(List<Guid> ids);
        Task SetAutoApplyAsync(Guid termId, bool autoApply);
        List<MLTermDto> Query(MLTermQueryParam param, out int totalCount, bool isZeroShot);
        List<MLTermDto> GetAllMLTerm();
        List<Guid> GetAllMLTermIds();
        MLTermDto GetTrainingTerm(Guid id);
        Task UpdateDescription(MLTermDto dto);
        MLTermDto GetValidTrainingTerm(Guid id);
        Task UpdateZeroApprovalCount(Guid id, long count);
        Task UpdateZeroReclassifyCount(Guid id, long count);
        Task UpdateZeroApprovalReclassifyCount(Guid id, long approvalCount, long reclassifyCount);
        Task AddTermTrainingScopeCountValueAsync(Guid termId, int addValue);
        Task SubTermTrainingScopeCountValueAsync(Guid termId, int subValue);
        Task<IEnumerable<RMMLTerm>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertMLTermTableAsync(IEnumerable<RMMLTerm> mLTerms);
        Task<long> MultiGeoDeleteAllMLTermAsync();
    }
}
