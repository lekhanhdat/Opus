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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Physical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IPhysicalReqeustService
    {
        /// <summary>
        /// Get those load physical box/folder which return date is between fromTicks and toTicks.
        /// </summary>
        /// <param name="fromTicks">ticks of UTC time</param>
        /// <param name="toTicks">ticks of UTC time</param>
        /// <returns>the physical box/folder ids which loan date is large than fromTicks and less than or equals toTicks</returns>
        Task<List<Guid>> GetLoanObjectIdsAsync(long fromTicks, long toTicks);
        Task<PhysicalRequestResult> CreateAsync(PhysicalRequestDto dto);

        Task<PhysicalRequestDto> GetRequestAsync(int id);

        Task<PhysicalRequestDto> GetRequestByRecordIdAsync(string recordId);

        Task<PhysicalRequestResult> QueryAsync(PhysicalRequestParam query);

        Task<PhysicalRequestResult> UpdateAsync(PhysicalRequestDto dto);

        Task<PhysicalRequestResult> ApproveAsync(PhysicalRequestParam param);

        Task<PhysicalRequestResult> ApproveLoanForMobileAsync(MobileApprovalLoanDto requestDto);

        Task<PhysicalRequestResult> RejectAsync(PhysicalRequestParam param);

        Task<PhysicalRequestResult> CancelRequestAsync(PhysicalRequestParam param);

        Task<PhysicalRequestResult> LoanRequestAsync(LoanRequestDto dto);
        bool CheckItemOnHold(List<Guid> ids);
        Dictionary<int, object> GetFilterDataSource();
        List<PhysicalObjectDto> GetLoanFolderByBoxIds(List<Guid> guids);
        RAReturnMessage StartLoanOrReturnBoxJob(JobType jobType, BoxLoanJobMessage param);
        Task<string> RealRunStartLoanOrReturnBoxJobAsync(JobType jobType, string param);
        System.Threading.Tasks.Task SendEmailNotificationAsync(EmailTemplateInternalType templateInternalType, PhysicalRequestDto requestDto, ParameterMoveDto moveParam = null);
        Task<PhysicalRequestResult> MoveRequestAsync(MoveRequestDto dto);
        Task<PhysicalRequestDto> GetRequestDtoByGroupIdAndStatusAsync(Guid groupRequestId, PhysicalRequestStatus status);
        bool CheckItemsOnLoan(PhysicalRequestParam param);
        Task<string> RealRunStartMoveDataJobAsync(string param);
    }
}
