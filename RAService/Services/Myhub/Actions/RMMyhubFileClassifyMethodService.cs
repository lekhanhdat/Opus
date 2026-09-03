using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Myhub;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AvePoint.RA.Service.Services.MyHub.Actions.RMMyhubRunActionMethod;

namespace AvePoint.RA.Service.Services.MyHub.Actions
{
    [Audit]
    public class RMMyhubFileClassifyMethodService:RMServiceBase, IRMMyhubFileClassifyMethodService
    {
        private RMMyhubRunActionMethod _actionMethod;
        private RMMyhubRunActionMethod ActionMethod => _actionMethod ??= new RMMyhubRunActionMethod();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.FSMyhub, Action = AuditAction.MyhubClassify, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.MyhubClassify, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<RAReturnMessage> PatchClassifyAsync(ClassCodePolicyInfo classCodePolicyInfo, RMMyhubClassifyQueryInfo queryInfo, RMMyhubActionTarget target,  OlderThanTimeDto timerDto)
        {
            try
            {
                var container = await ActionMethod.GetContainerAsync();

                var partitionKey = BuildPartitionKey(target);

                var currentRecordResponse = await container.ReadItemAsync<Record>(
                    id: target.SelectId.ToString(),
                    partitionKey: partitionKey);
                var currentRecord = currentRecordResponse.Resource;

                if (currentRecord == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_FS_Myhub_Classify_ErrorForFile") + target.SelectId,
                        Extsion1 = target.SelectId.ToString()
                    };
                }

                var operations = await BuildPatchOperations(queryInfo, target, timerDto);

                var response = await container.PatchItemAsync<Record>(
                    id: currentRecord.NodeId.ToString(),
                    partitionKey: partitionKey,
                    patchOperations: operations);
                return response.StatusCode == System.Net.HttpStatusCode.OK
            ? new RAReturnMessage
            {
                MessageType = RAMessageType.Successful,
                Extension = currentRecord.LeafName
            }
            : new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_FS_Myhub_Classify_ErrorForFile") + currentRecord.LeafName,
                Extsion1 = currentRecord.LeafName
            };
            }
            catch (Exception ex)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Exception,
                    ErrorMessage = I18NEntity.GetString("RM_FS_Myhub_Classify_ErrorForFile")
                };
            }
        }


        private async Task<List<PatchOperation>> BuildPatchOperations(RMMyhubClassifyQueryInfo queryInfo, RMMyhubActionTarget target, OlderThanTimeDto timerDto)
        {
            //var startDateTicks = DateTime.TryParse(queryInfo.StartDate, out var parsedDate) ? parsedDate.Ticks : 0;
            
            var retentionType = queryInfo.RetentionType switch
            {
                "Event" => "1",
                "Flat" => "2",
                _ => null
            };
            var startDateTicks = retentionType == "1"?(await GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.Parse(queryInfo.StartDate), queryInfo.TimeZoneId)).Ticks:0;
            var endTime = ResolveEndTime(queryInfo, target.TimeModified, timerDto, startDateTicks);

            return new List<PatchOperation>
            {
                PatchOperation.Set("/termId", queryInfo.TermUniqueId),
                PatchOperation.Set("/termName", queryInfo.ClassCode),
                PatchOperation.Set("/classCode", queryInfo.ClassCode),
                PatchOperation.Set("/countryCode", queryInfo.CountryCode),
                PatchOperation.Set("/retentionType", retentionType),
                PatchOperation.Set("/startDate", startDateTicks),
                PatchOperation.Set("/endTime", endTime),
                PatchOperation.Set("/policyValueNumber", timerDto.Number.ToString()),
                PatchOperation.Set("/policyValueUnit", ((int)timerDto.PolicyValueUnit).ToString())
            };
        }

        private static long ResolveEndTime(RMMyhubClassifyQueryInfo queryInfo, long timeModified, OlderThanTimeDto timerDto, long startDateTicks)
        {
            if (timerDto == null)
            {
                return 0;
            }

            var retentionType = queryInfo.RetentionType switch
            {
                "Event" => 1,
                "Flat" => 2,
                _ => 0
            };

            var baseTicks = retentionType == 1 ? startDateTicks : timeModified;
            if (baseTicks == 0)
            {
                return 0;
            }

            var baseTime = new System.DateTime(baseTicks, System.DateTimeKind.Utc);
            var endTime = timerDto.PolicyValueUnit switch
            {
                PolicyValueUnit.Days => baseTime.AddDays(timerDto.Number),
                PolicyValueUnit.Weeks => baseTime.AddDays(timerDto.Number * 7),
                PolicyValueUnit.Months => baseTime.AddMonths(timerDto.Number),
                PolicyValueUnit.Years => baseTime.AddYears(timerDto.Number),
                _ => baseTime
            };

            return endTime.Ticks;
        }
    }
}