using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.RACommonUtility.UniqueId;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/AppActionsForSPS/[action]")]
    [ApiController]
    public class AppActionsForSPSController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(AppActionsForSPSController));
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        [HttpPost]
        public async Task<string> SearchRecords([FromBody] AddPageSearchRecordsDto dto)
        {
            return JsonConvert.SerializeObject(
                await ExplorerService.SearchPhysicalRecordsAsync(
                    dto.PageIndex,
                    dto.PageSize,
                    dto.Value));
        }

        [HttpPost]
        public string UpdateRelatedRecordsWithSP([FromBody] UpdateRecordsDto dto)
        {
            var result = ExplorerService.UpdateRelatedRecords(
                dto.Id,
                dto.ReletedIds,
                dto.DeleteReletedIds ?? new List<Guid>(),
                dto.IdNameDict,
                out var addRelatedIdsForHistory);

            return JsonConvert.SerializeObject(new
            {
                result,
                addRelatedIdsForHistory
            });
        }

        [HttpPost]
        public string TryAddRecord([FromBody] TryAddRecordDto dto)
        {
            var result = new TryAddRecordResultDto();
            try
            {
                var record = ConvertToRecord(dto?.Input);
                result.Converted = true;
                result.Record = record;

                if (dto?.PersistAfterConvert != false)
                {
                    EnsureRecordsIdForItem(record);
                    if (ExplorerService.IsSPOnPremObjectExist(record.ScopeId, record.Id))
                    {
                        result.Persisted = true;
                        result.Success = true;
                        result.Message = "Record already exists.";
                    }
                    else
                    {
                        var message = ExplorerService.AddOrUpdateSPOnPremObject(record);
                        result.Persisted = message.MessageType == RAMessageType.Successful;
                        result.Success = result.Persisted;
                        result.Message = message.ErrorMessage;
                    }
                }
                else
                {
                    result.Persisted = false;
                    result.Success = true;
                    result.Message = "Converted only.";
                }
            }
            catch (Exception ex)
            {
                logger.Error($"TryAddRecord failed. Error:{ex}");
                result.Success = false;
                result.Message = ex.Message;
            }

            return JsonConvert.SerializeObject(result);
        }

        private static RecordDto ConvertToRecord(SharePointOnPremRecordInputDto input)
        {
            ThrowUtil.ThrowIfNull(input, nameof(input));
            if (input.ScopeId == Guid.Empty)
            {
                throw new ArgumentException("ScopeId cannot be empty.", nameof(input.ScopeId));
            }

            if (input.NodeId == Guid.Empty)
            {
                throw new ArgumentException("NodeId cannot be empty.", nameof(input.NodeId));
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            var timeCreated = input.TimeCreated > 0 ? input.TimeCreated : nowTicks;
            var createDate = input.CreateDate > 0 ? input.CreateDate : int.Parse(new DateTime(timeCreated, DateTimeKind.Utc).ToString("yyyyMMdd"));
            var aveSiteId = ResolveAveSiteId(input);

            return new RecordDto
            {
                Id = IDGenerator.GetRecordId(input.ScopeId, input.NodeId),
                ScopeId = input.ScopeId,
                NodeId = input.NodeId,
                NodeType = 500,  // 500 stands for File. This API will only be invoked for items of type File sourced from SharePoint On-Prem.
                AveSiteId = aveSiteId,
                WebId = input.WebId,
                ListId = input.ListId,
                ItemId = input.ItemId == Guid.Empty ? input.NodeId : input.ItemId,
                FolderId = input.FolderId,
                LeafName = input.LeafName,
                FullPath = input.FullPath,
                DirPath = input.DirPath,
                RecordsId = input.RecordsId,
                CollectionTime = nowTicks,
                TimeCreated = timeCreated,
                TimeLastModified = input.TimeLastModified,
                CreateDate = createDate,
                TermId = input.TermId,
                TermName = input.TermName,
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                DisposalDueDate = input.DisposalDueDate ?? string.Empty,
                PreviosDisposalDueDate = input.DisposalDueDate ?? string.Empty,
                RuleId = input.RuleId,
                RuleLevel = input.RuleLevel,
                DeclareAsRecord = input.DeclareAsRecord,
                CreatedBy = input.CreatedBy,
                ModifiedBy = input.ModifiedBy,
                ExtensionForFile = input.ExtensionForFile,
                MetaInfo = input.MetaInfo,
                RelatedRecords = input.RelatedRecords,
                RelatedRecordsCount = input.RelatedRecordsCount,
                ItemRowId = input.ItemRowId,
                ApproveUsers = input.ApproveUsers,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                SortTicks = Snowflake.Instance().GetTicks()
            };
        }

        private static string ResolveAveSiteId(SharePointOnPremRecordInputDto input)
        {
            var localSites = SharePointOnPremClient.GetAllLocalSiteCollectionsAsync().GetAwaiter().GetResult();

            // Normalize caller input first: it may be DocAve site id, or real SP site id by mistake.
            if (input.AveSiteId != Guid.Empty)
            {
                var inputSiteId = input.AveSiteId.ToString();
                var mappedByInput = localSites?.FirstOrDefault(site =>
                    (!string.IsNullOrWhiteSpace(site.Id) && site.Id.Equals(inputSiteId, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(site.SPObjectId) && site.SPObjectId.Equals(inputSiteId, StringComparison.OrdinalIgnoreCase)));

                if (mappedByInput != null && !string.IsNullOrWhiteSpace(mappedByInput.Id))
                {
                    return mappedByInput.Id;
                }
            }

            // Agent AssembleRecord uses SPSiteId as ScopeId and DocAve site id as AveSiteId.
            var scopeId = input.ScopeId.ToString();
            var localSite = localSites?.FirstOrDefault(site =>
                !string.IsNullOrWhiteSpace(site.SPObjectId)
                && site.SPObjectId.Equals(scopeId, StringComparison.OrdinalIgnoreCase));

            if (localSite != null && !string.IsNullOrWhiteSpace(localSite.Id))
            {
                return localSite.Id;
            }

            throw new ArgumentException($"AveSiteId cannot be resolved by ScopeId: {input.ScopeId}.", nameof(input.AveSiteId));
        }

        private static void EnsureRecordsIdForItem(RecordDto record)
        {
            if (record == null)
            {
                return;
            }

            if (record.NodeType == (int)NodeLevel.Item && string.IsNullOrWhiteSpace(record.RecordsId))
            {
                var idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, 1);
                record.RecordsId = idUtil.GenerateUniqueId();
            }
        }
    }
}
