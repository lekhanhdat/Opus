using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.TaxonomyModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/TermManagementApi/[action]")]
    [ApiController]
    public class TermManagementResourceApiController : RAWebApiBase
    {
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();

        #region Term Group
        [HttpPost]
        public Task<string> CreateTermGroup([FromBody] TermInfo termModel)
        {
            ValidateReplicaCreateTermGroupRequest(termModel);
            if (TaxonomyService.HasTermGroupName(termModel.TermGroupName))
            {
                throw new InvalidOperationException("Term set name already exists in the term group.");
            }
            termModel.TermGroupName = this.replaceStr(termModel.TermGroupName);
            return TaxonomyService.CreateTermGroupAsync(termModel);
        }

        [HttpPost]
        public Task<string> RenameTermGroup([FromBody] TermInfo termModel)
        {
            termModel.TermName = this.replaceStr(termModel.TermName);
            return TaxonomyService.RenameTermGroupAsync(termModel.TermId, termModel.TermName);
        }

        [HttpPost]
        public Task<string> DeleteTermGroup([FromBody] Guid termGroupId)
        {
            return TaxonomyService.DeleteTermGroupAsync(termGroupId);
        }

        [HttpPost]
        public Task<RAReturnMessage> SaveTermGroup([FromBody] TermInfo termModel)
        {
            termModel.TermGroupName = this.replaceStr(termModel.TermGroupName);
            return TaxonomyService.UpdateTermGroupAsync(termModel.TermGroupId, termModel.TermGroupName, termModel.Description, termModel.ReSiteInfos, termModel.UsingMMSSpecified, termModel.M365TermSyncOption, termModel.GoogleTermSyncOption);
        }
        #endregion

        #region Term Set
        [HttpPost]
        public Task<string> CreateTermSet([FromBody] TermInfo termModel)
        {
            ValidateReplicaCreateTermSetRequest(termModel);
            if (TaxonomyService.HasTermSetName(termModel.TermSetName, termModel.TermGroupUniqueId))
            {
                throw new InvalidOperationException("Term set name already exists in the term group.");
            }
            termModel.TermSetName = this.replaceStr(termModel.TermSetName);
            return TaxonomyService.CreateTermSetAsync(termModel);
        }

        [HttpPost]
        public Task<string> RenameTermSet([FromBody] TermInfo termModel)
        {
            termModel.TermName = this.replaceStr(termModel.TermName);
            return TaxonomyService.RenameTermSetAsync(termModel.TermId, termModel.TermName, termModel.TermGroupUniqueId);
        }

        [HttpPost]
        public string ApplyDeleteRootTerms([FromBody] int termSetId)
        {
            return TaxonomyService.DeleteRootTerms(termSetId);
        }

        [HttpPost]
        public Task<string> SaveTermSet([FromBody] TermInfo termModel)
        {
            termModel.TermSetName = this.replaceStr(termModel.TermSetName);
            return TaxonomyService.UpdateTermSetAsync(termModel.TermSetId, termModel.TermSetName, termModel.Description);
        }

        [HttpPost]
        public Task<string> InheritSettingToParent([FromBody] TermSettingsInfo termInfo)
        {
            string strTermDescription = string.Empty;

            if (!string.IsNullOrEmpty(termInfo.des))
            {
                strTermDescription = termInfo.des;
            }

            termInfo.des = strTermDescription;
            return TaxonomyService.SaveTermSettingInheritToParentAsync(termInfo.tId, termInfo);
        }
        #endregion

        #region Term
        [HttpPost]
        public Task<string> CreateTerm([FromBody] TermInfo termModel)
        {
            ValidateReplicaCreateTermRequest(termModel);
            termModel.TermName = this.replaceStr(termModel.TermName);
            return TaxonomyService.CreateTermAsync(termModel);
        }

        [HttpPost]
        public Task<string> RenameTerm([FromBody] TermInfo termModel)
        {
            termModel.TermName = this.replaceStr(termModel.TermName);
            return TaxonomyService.RenameTermAsync(termModel.TermId, termModel.TermName, termModel.TermSetId);
        }

        [HttpPost]
        public Task<string> ApplyDeleteTerm([FromBody] int termId)
        {
            return TaxonomyService.DeleteTermAsync(termId);
        }

        [HttpPost]
        public string ApplyEnableTerm([FromBody] int termId)
        {
            return TaxonomyService.EnableTerm(termId);
        }

        [HttpPost]
        public string ApplyDeprecateTerm([FromBody] int termId)
        {
            return TaxonomyService.DeprecateTerm(termId);
        }

        [HttpPost]
        public Task<String> SaveTermSettings([FromBody] TermSettingsInfo setting)
        {
            return TaxonomyService.SaveTermSettingAsync(setting);
        }
        #endregion

        private string replaceStr(string sourceStr)
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

        private static void ValidateReplicaCreateTermGroupRequest(TermInfo termModel)
        {
            if (termModel == null || termModel.TermGroupUniqueId == Guid.Empty)
            {
                throw new InvalidOperationException("Term group unique id is required for replica requests.");
            }
        }

        private static void ValidateReplicaCreateTermSetRequest(TermInfo termModel)
        {
            if (termModel == null || termModel.TermSetUniqueId == Guid.Empty || termModel.TermGroupUniqueId == Guid.Empty)
            {
                throw new InvalidOperationException("Term set unique ids are required for replica requests.");
            }
        }

        private static void ValidateReplicaCreateTermRequest(TermInfo termModel)
        {
            if (termModel == null || termModel.TermUniqueId == Guid.Empty)
            {
                throw new InvalidOperationException("Term unique id is required for replica requests.");
            }
        }
    }
}
