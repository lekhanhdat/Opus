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

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    internal class JpmcConnectionIdQuerier : IFilter
    {
        private readonly IRALogger _logger = new RALogger(typeof(JpmcConnectionIdQuerier));

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.JpmcConnectionId;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            _logger.Info($"GetCosmosDBFilterExpressionAsync: Filtering by JpmcConnectionId with value '{value}'.");
            
            return root => root.L2PartitionKey == value;
        }
    }
}
