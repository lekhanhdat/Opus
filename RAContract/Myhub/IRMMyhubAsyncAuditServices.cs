using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub
{
    public interface IRMMyhubAsyncAuditServices
    {

        Task<ManualApprovalActionResult> PauseAsync(PauseOrResumeReq req);

        Task<ManualApprovalActionResult> ResumeAsync(PauseOrResumeReq req);
    }
}
