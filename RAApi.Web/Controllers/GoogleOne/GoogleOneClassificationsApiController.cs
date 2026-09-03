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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne;

[Route("api/googleone/classification")]
public class GoogleOneClassificationsApiController : GoogleOneApiBaseController
{
    private readonly IRALogger _logger = new RALogger(typeof(GoogleOneClassificationsApiController));
    private readonly IRuleContainerService _ruleContainerService = PlatformWindsorManager.GetService<IRuleContainerService>();
    private readonly IRuleManagerService _ruleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();
    private readonly ITaxonomyService _taxonomyService = PlatformWindsorManager.GetService<ITaxonomyService>();

    private readonly ITermRuleAssociationDao _termRuleAssociationDao = PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

    private readonly IRMSharePointTaxonomyService _sharePointTaxonomyService = PlatformWindsorManager.GetService<IRMSharePointTaxonomyService>();

    [HttpPost("get")]
    public Task<string> GetClassfications(TreePage tree)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetClassfications");
        int pIndex = 0;
        if (tree.PageIndex != null)
        {
            int.TryParse(tree.PageIndex.ToString(), out pIndex);
        }
        int pSize = 0;
        if (tree.PageSize != null)
        {
            int.TryParse(tree.PageSize.ToString(), out pSize);
        }
        pIndex = pIndex == 0 ? pIndex : pIndex - 1;

        string nodeId = string.Empty;
        if (tree.NodeId != null)
        {
            nodeId = tree.NodeId;
        }

        string nodeType = string.Empty;
        if (tree.NodeType != null)
        {
            nodeType = tree.NodeType;
        }
        var filterOption = new FilterTermObjOption
        {
            NeedCheckPermission = false,
            SourceFlag = RA.Contract.Explorer.SourceFlag.Google,
            ContainerId = tree.ContainerId,
        };
        return _taxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, 0, filterOption);
    }

    [HttpPost("group/create")]
    public async Task<string> CreateTermGroup(TermInfo termModel)
    {
        var result = await _taxonomyService.CreateTermGroupAsync(this.ReplaceStr(termModel.TermGroupName));
        if (result.IsNullOrEmpty())
        {
            return I18NEntity.GetString("RM_JS_TM_TermGroupSameNameErrorMsg");
        }
        var termGroup = JsonConvert.DeserializeObject<RMTermGroup>(result);

        var firstTermSetString = await _taxonomyService.AddFirstTermSetAsync(termGroup.UniqueId);
        var termSet = JsonConvert.DeserializeObject<RMTermSet>(firstTermSetString);

        termGroup.subTermCount = 1;
        termGroup.subTerms = [termSet];
        return JsonConvert.SerializeObject(termGroup);
    }

    [HttpPost("group/delete")]
    public async Task<int> DeleteTermGroup([FromBody] Guid termGroupId)
    {
        await _taxonomyService.DeleteTermGroupAsync(termGroupId);
        return (int)HttpStatusCode.NoContent;
    }

    [HttpPost("group/rename")]
    public async Task<string> RenameTermGroup(TermInfo termModel)
    {
        var result = await _taxonomyService.RenameTermGroupAsync(termModel.TermId, this.ReplaceStr(termModel.TermName));
        var termGroup = JsonConvert.DeserializeObject<RMTermGroup>(result);
        if(termGroup is null || termGroup.UniqueId == Guid.Empty)
        {
            return I18NEntity.GetString("RM_JS_TM_TermGroupSameNameErrorMsg");
        }
        return result;
    }

    [HttpGet("groups/get")]
    public async Task<Dictionary<string, string>> GetAllTermGroups()
    {
        return await _taxonomyService.GetAllTermGroups();
    }

    [HttpPost("nodes/groups")]
    public async Task<Dictionary<string, string>> GetTermGroupByMultipleNodes([FromBody] RMClassificationGroupMultipleNodes nodes)
    {
        return await _taxonomyService.GetAllTermGroupsByMultipleNodes(nodes);
    }
    [HttpGet("groups/getbyid")]
    public async Task<string> GetTermsByGroupId(Guid groupId)
    {
        return await _taxonomyService.GetTermsByGroupId(groupId);
    }

    [HttpPost("create")]
    public async Task<string> CreateTerm(TermInfo termModel)
    {
        termModel.TermName = this.ReplaceStr(termModel.TermName);
        if (termModel.TermSetId == 0)
        {
            var firstTermSetId = await _taxonomyService.FindOrAddFirstTermSetAsync(termModel.TermGroupUniqueId);
            if (firstTermSetId == 0)
            {
                return I18NEntity.GetString("RM_JS_TM_TermSetSameNameErrorMsg");
            }
            termModel.TermSetId = firstTermSetId;
        }
        var result = await _taxonomyService.CreateTermAsync(termModel);
        if(result == "1")
        {
            return I18NEntity.GetString("RM_JS_TM_TermSameNameErrorMsg");
        }
        return result;
    }

    [HttpPost("rename")]
    public async Task<string> RenameTerm(TermInfo termModel)
    {
        var result = await _taxonomyService.RenameTermAsync(termModel.TermId, this.ReplaceStr(termModel.TermName), termModel.TermSetId);
        var term = JsonConvert.DeserializeObject<RMTerm>(result);
        if (term is null || term.UniqueId == Guid.Empty)
        {
            return I18NEntity.GetString("RM_JS_TM_TermSameNameErrorMsg");
        }
        return result;
    }

    [HttpPost("delete")]
    public async Task<int> DeleteTerm([FromBody] int termId)
    {
        await _taxonomyService.DeleteTermAsync(termId);
        return (int)HttpStatusCode.NoContent;
    }

    [HttpPost("search")]
    public Task<string> Search(RMTermSearch termSearch)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.SearchClassfications");
        return _taxonomyService.SearchAsync(1, this.ReplaceStr(termSearch.TermLabel), termSearch.TermGroupIds, termSearch.WithRuleName);
    }

    [HttpPost("savesetting")]
    public async Task<string> SaveTermSettings(List<TermSettingsInfo> settings)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.SaveSettingClassfication");
        StringBuilder sb = new();
        foreach (var setting in settings)
        {
            try
            {
                //Handle conflict rules
                var result = await _taxonomyService.GetRuleAssicationWithTermIdAsync(setting.tId);
                var dbRuleInfos = JsonConvert.DeserializeObject<List<RMTermRuleAssociation>>(result);
                foreach (var dbRuleInfo in dbRuleInfos.OrderBy(rule => rule.RuleOrder))
                {
                    var ruleInfo = await _ruleManagerService.LoadRuleAsync(dbRuleInfo.RuleId.ToString());
                    if (ruleInfo.GoogleDriveRule == null)
                    {
                        var ruleByOrder = setting.infos.FirstOrDefault(r => r.RuleOrder == dbRuleInfo.RuleOrder);
                        if (ruleByOrder != null && dbRuleInfo.RuleId != new Guid(ruleByOrder.RuleId))
                        {
                            var index = setting.infos.IndexOf(ruleByOrder);

                            setting.infos.ForEach(info =>
                            {
                                if (info.RuleOrder >= dbRuleInfo.RuleOrder)
                                {
                                    info.RuleOrder += 1;
                                }
                            });

                            RuleDisplayInfo pureM365Rule = new()
                            {
                                Id = dbRuleInfo.Id,
                                RuleId = dbRuleInfo.RuleId.ToString(),
                                RuleLevel = dbRuleInfo.RuleLevel,
                                RuleName = dbRuleInfo.RuleName,
                                RuleOrder = dbRuleInfo.RuleOrder
                            };
                            setting.infos.Insert(index, pureM365Rule);
                        }
                    }
                }


                await _taxonomyService.SaveTermSettingAsync(setting);
            }
            catch (Exception ex)
            {
                _logger.Error($"Save term setting error: {setting.tId}, {ex.Message}");
                sb.Append($"{setting.tId},");
            }
        }
        return sb.ToString();
    }

    [HttpGet("setting")]
    public async Task<string> GetTermSetting(int classificationId)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetSettingClassfication");
        try
        {
            return await _taxonomyService.GetTermSettingWithGoogleRuleAsync(classificationId);
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while TaxonomyService.GetParentInhertSetting, termId:[{classificationId}], ERROR:{ex.ToString()}");
            return I18NEntity.GetString("RM_JS_SPS_ErrorMessage_NoTermError");
        }
    }

    [HttpPost("sync")]
    public async Task<bool> Finish()
    {
        RAReturnMessage res = await _sharePointTaxonomyService.RunSyncRMTermTreeToSharePointAsync(JobRunBy.Control, false);
        return res.MessageType == RAMessageType.Successful;
    }
    [HttpPost("getclassificationforview")]
    public async Task<string> GetClassificationForview(TreePage tree)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetClassificationForview");
        try
        {
            int pIndex = 0;
            if (tree.PageIndex != null)
            {
                int.TryParse(tree.PageIndex.ToString(), out pIndex);
            }
            int pSize = 0;
            if (tree.PageSize != null)
            {
                int.TryParse(tree.PageSize.ToString(), out pSize);
            }
            pIndex = pIndex == 0 ? pIndex : pIndex - 1;

            string nodeId = string.Empty;
            if (tree.NodeId != null)
            {
                nodeId = tree.NodeId;
            }

            string nodeType = string.Empty;
            if (tree.NodeType != null)
            {
                nodeType = tree.NodeType;
            }
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = false,
                SourceFlag = RA.Contract.Explorer.SourceFlag.Google,
                ContainerId = tree.ContainerId,
            };

            return await _taxonomyService.GetTaxonomyGoogleTermTreeDataAsync(filterOption, pIndex, pSize);
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while TaxonomyService.GetClassificationForview, ERROR:{ex.ToString()}");
            return String.Empty;
        }
    }
    [HttpGet("getclassificationinfo")]
    public async Task<string> GetClassificationInfo(Guid termId)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetClassificationInfo");
        try
        {
            return _taxonomyService.GetTermWithPathByTermId(termId);
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while TaxonomyService.GetClassificationInfo, ERROR:{ex.ToString()}");
            return String.Empty;
        }
    }
    [HttpGet("getclassificationrulelist")]
    public async Task<string> GetClassificationRuleList(int termId, SourceFlag sourceflag)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetClassificationRuleList");
        try
        {
            return _taxonomyService.GetTermRuleInfoByTermIdAndSourceFlagForGoogleOne(termId, sourceflag);
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while TaxonomyService.GetClassificationRuleList, ERROR:{ex.ToString()}");
            return String.Empty;
        }
    }
    [HttpPost("loadmore")]
    public async Task<string> GetChildrenTreeNodes(TreePage tree)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetChildrenTreeNodes");
        int pIndex = tree.PageIndex ?? 0;
        int pSize = tree.PageSize ?? 0;

        if (pIndex > 0)
        {
            pIndex -= 1;
        }

        string nodeId = tree.NodeId ?? string.Empty;
        string nodeType = tree.NodeType ?? string.Empty;
        int SettingType = tree.SettingType != null ? Convert.ToInt32(tree.SettingType) : 0;
        var filterOption = new FilterTermObjOption
        {
            SourceFlag = tree.SourceFlag,
            ContainerId = tree.ContainerId,
        };

        return await _taxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, SettingType, filterOption);
    }
    [HttpGet("groups/getwithpermission")]
    public async Task<string> GetClassficationGroupsWithPermission(int termId, SourceFlag sourceflag)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetClassficationGroupsWithPermission");
        try
        {
            return await _taxonomyService.LoadTermGroupsAsync(new FilterTermObjOption());
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while ClassficationsApiController.GetClassficationGroupsWithPermission, ERROR:{ex.ToString()}");
            return String.Empty;
        }
    }
    [HttpPost("groups/getchildren")]
    public async Task<string> GetChildrenClassificationGroups(TreePage tree)
    {
        using var performance = new PerformanceScope("ClassficationsApiController.GetChildrenClassificationGroups");
        string nodeId = string.Empty;
        if (tree.NodeId != null)
        {
            nodeId = tree.NodeId;
        }

        string nodeType = string.Empty;
        if (tree.NodeType != null)
        {
            nodeType = tree.NodeType;
        }
        var filterOption = new FilterTermObjOption
        {
            SourceFlag = SourceFlag.Google,
            ContainerId = tree.ContainerId,
        };
        return await _taxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, filterOption, true);
    }

    private string ReplaceStr(string sourceStr)
    {
        string resultStr = "";
        if (!string.IsNullOrEmpty(sourceStr))
        {
            Regex reg = new Regex(@"[;<>|]+");
            sourceStr = reg.Replace(sourceStr.Trim(), "");
            if (!string.IsNullOrEmpty(sourceStr) && (sourceStr.Contains("&") || sourceStr.Contains("\"")))
            {
                resultStr = sourceStr.Replace('&', '＆').Replace('"', '＂');
            }
            else
            {
                resultStr = sourceStr;
            }
        }
        return resultStr;
    }

   
}