using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.MachineLearningManualApproval.Queriers;
using AvePoint.RA.Service.Services.Myhub.Actions;
using AvePoint.RA.Service.Services.Myhub.Views;
using AvePoint.Wrapper.Restore;
using FluentFTP.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    internal class MyhubFolderNodeIdQuerier : IFilter
    {
        private readonly IRALogger _logger = new RALogger(typeof(MyhubFolderNodeIdQuerier));

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.MyhubFolderNodeId;

        private IRMMyhubServices _IRMMyhubServices;
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService(ref _IRMMyhubServices);

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            _logger.Info($"GetCosmosDBFilterExpressionAsync: Filtering by MyhubFolderNodeId with value '{value}'.");

            var filterResult = JsonConvert.DeserializeObject<ManualApprovalMyhubFolderNodeIdQueryDefinition>(value);
            var path = await RMMyhubServices.GetPendingDisposalFolderFilterPathAsync(filterResult.PartitionKeyId, filterResult.NodeId, true);
            return root => filterResult.PartitionKeyId == root.L2PartitionKey && root.ManualFullPath.ToLower().StartsWith(path.ToLower());

        }
    }
}
