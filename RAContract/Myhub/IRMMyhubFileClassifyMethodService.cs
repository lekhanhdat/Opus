using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub
{
    public interface IRMMyhubFileClassifyMethodService
    {
        Task<RAReturnMessage> PatchClassifyAsync(ClassCodePolicyInfo classCodePolicyInfo, RMMyhubClassifyQueryInfo queryInfo, RMMyhubActionTarget target,  OlderThanTimeDto timerDto);
    }
}
