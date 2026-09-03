using Aspose.Pdf;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using AvePoint.RA.Service.Services.MyHub;
using AvePoint.RA.Service.Services.MyHub.Actions;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using HSMCommon.DeploymentXML;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Newtonsoft.Json;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Myhub.Actions
{
    public class RMMyhubPauseResumeMethod
    {
        RALogger logger = RALogger.GetInstance(typeof(RMMyhubPauseResumeMethod));

        private RMMyhubRunActionMethod _actionMethod;
        private RMMyhubRunActionMethod ActionMethod => _actionMethod ??= new RMMyhubRunActionMethod();

        private static ManualApprovalRecordRepository _repository => new ManualApprovalRecordRepository();


        public async Task<ManualApprovalActionResult> UpdateApprovalStatusAsync(List<ManualPauseActionParam> pauseParameters,
            ManualApprovalActionType actionType) {
            try {
                var groups = pauseParameters.GroupBy(x => x.IsFolder);
                List<ManualApprovalRecord> records = new List<ManualApprovalRecord>();
                foreach (var group in groups) {
                    bool isFolder = group.Key;
                    List<ManualPauseActionParam> list = group.ToList();
                    if (isFolder)
                    {
                        List<string> paths = list.Select(i => i.Path).ToList();
                        foreach (var path in paths) {
                            List<ManualApprovalRecord> folderFileRecords = await _repository.QueryItemsAsync(
                            record => string.Concat(record.DirPath,"\\",record.LeafName).ToLower().StartsWith(path.ToLower())  && 
                            ((actionType == ManualApprovalActionType.Pause && record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove) || 
                            (actionType == ManualApprovalActionType.Resume && record.ManualApprovedStatus == (int)SOApproveDBStatus.Pause)));
                            records.AddRange(folderFileRecords);
                        }
                    }
                    else 
                    {
                        List<string> nodeIds = list.Select(i => i.NodeId).ToList();
                        List<Guid> guidList = nodeIds.Select(s => Guid.Parse(s)).ToList();
                        List<ManualApprovalRecord> fileRecords = await _repository.QueryItemsAsync(record => guidList.Contains(record.Id) &&
                        ((actionType == ManualApprovalActionType.Pause && record.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove) ||
                            (actionType == ManualApprovalActionType.Resume && record.ManualApprovedStatus == (int)SOApproveDBStatus.Pause)));
                        records.AddRange(fileRecords);
                    }
                }

                var container = await ActionMethod.GetContainerAsync();
                
                foreach (var item in records)
                {
                    Microsoft.Azure.Cosmos.PartitionKey partitionKey = item.BuildPartitionKey();
                    List<PatchOperation> ops;
                    if (actionType == ManualApprovalActionType.Pause)
                    {
                        ops = new List<PatchOperation>
                        {
                            PatchOperation.Set("/manual_approvedStatus", (int)SOApproveDBStatus.Pause),
                            PatchOperation.Set("/manual_actionTime", DateTime.Now.Ticks)
                        };
                    }
                    else 
                    {
                        ops = new List<PatchOperation>
                        {
                            PatchOperation.Set("/manual_approvedStatus", (int)SOApproveDBStatus.WaitingApprove),
                            PatchOperation.Set("/manual_actionTime", DateTime.Now.Ticks)
                        };
                    }
                    await container.PatchItemAsync<Record>(
                        id: item.Id.ToString(),
                        partitionKey: partitionKey,
                        patchOperations: ops);
                }
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Succeed,
                    Message = ""
                };
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while UpdateApprovalStatus. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = ""
                };
            }
        }




    }
}
