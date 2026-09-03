using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class SourceNoFsQuerier : IFilter 
    {
        private readonly IRALogger _logger = new RALogger(typeof(SourceNoFsQuerier));

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.SourceNoFs;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            _logger.Info($"GetCosmosDBFilterExpressionAsync: Filtering by SourceNoFs with value '{value}'.");

            return root => root.SourceFlag != (int)SourceFlag.FileSystem;
        }
    }
}
