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
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement
{
    [DataContract]
    public class SuiteDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public Guid UniqueId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public SuiteStartFromType StartFromType { set; get; }
        [DataMember]
        public SuiteRootTemplateCreateType RootTemplateCreateType { get; set; }
        [DataMember]
        public Guid RootTemplateUniqueId { get; set; }
        [DataMember]
        public string RootTemplateName { get; set; }
    }

    public class SimplifySuiteDto
    {
        public Guid UniqueId { get; set; }

        public string Name { get; set; }

        public SuiteStartFromType StartFrom { get; set; }
    }

    [DataContract]
    public class SimplifyTemplateDto
    {
        [DataMember]
        public Guid UniqueId { get; set; }
        [DataMember]
        public int? Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public TemplateType Type { set; get; }

        public override bool Equals(object obj)
        {
            SimplifyTemplateDto other = obj as SimplifyTemplateDto;
            if (other == null)
            {
                return false;
            }
            return UniqueId == other.UniqueId && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return this.UniqueId.GetHashCode();
        }
    }

    public class ExistingTemplatesInfo {
        public List<SimplifyTemplateDto> Templates { get; set; }

        public List<SimplifyTemplateDto> FolderTemplates { get; set; }
        public List<SimplifyTemplateDto> RecordTemplates { get; set; }
    }

    public class ViewSuiteTemplateDto
    {
        public ViewDataLevel ViewDataLevel { get; set; }

        public Guid SuiteUniqueId { get; set; }

        public string SuiteName { get; set; }

        public string TemplateName { get; set; }

        public Guid TemplateUniqueId { get; set; }

        public string Description { get; set; }

        public SuiteStartFromType StartFromType { set; get; } //for suite

        public string ParentPath { get; set; }

        public string Creater { set; get; }

        public string CreatedOn { get; set; }

        public string Modifier { set; get; }

        public string LastModifiedOn { get; set; }

        public int FolderTemplateCount { get; set; }

        public int RecordTemplateCount { get; set; }

        public Guid RootTemplateGuid { get; set; }
        public string TemplateDescription { get; set; }

    }
    [DataContract]
    public class SuiteTemplateQueryDto
    {
        [DataMember]
        public SuiteTemplatePagingInfo PagingInfo { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public Guid TemplateIdUniqueId { get; set; }
        /// <summary>
        /// id list start from suite to current. first one is suite unique id, others are template id(not unique id)
        /// </summary>
        [DataMember]
        public List<string> TemplateIdList { get; set; }

        [Obsolete]
        public Guid ParentUniqueId { get; set; }
        [Obsolete]
        public Guid SuiteUniqueId { get; set; }
        [Obsolete]
        public Guid BoxTemplateUniqueId { get; set; }
        [Obsolete]
        public Guid FolderTemplateUniqueId { get; set; }

    }
    [DataContract]
    public class SuiteTemplatePagingInfo
    {
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
    }

    public class SuiteTemplateResultDto
    {
        public int TotalCount { get; set; }

        public List<ViewSuiteTemplateDto> ResultList { get; set; }
        public TemplateInfoOfBreadCrumbs TemplateInfoOfBreadCrumbs { get; set; }
    }

    public enum ViewDataLevel
    {
        Suite = 1,
        Box = 2,
        Folder = 3,
        Record = 4
    }

    public class TemplateInfoOfBreadCrumbs {
        public string BoxTemplateName { get; set; }
        public Guid BoxTemplateId { get; set; }
        public string FolderTemplateName { get; set; }
        public Guid FolderTemplateId { get; set; }
    }
    [DataContract]
    public class QueryExistingTemplatesDto : BaseTemplateParam
    {
        [DataMember]
        public Guid UniqueId { get; set; }
        [DataMember]
        public List<TemplateType> TemplateTypes { get; set; }
        //public TemplateType Type { get; set; }
        //public Guid SuiteId { get; set; }
        //public Guid BoxTemplateId { get; set; }
        //public Guid FolderTemplateId { get; set; }
    }
    [DataContract]
    public class AddExistingTemplatesDto : BaseTemplateParam
    {
        [DataMember]
        public Guid UniqueId { get; set; }
        [DataMember]
        public List<Guid> Ids { get; set; }
        [DataMember]
        public Guid SuiteId { get; set; }
        [DataMember]
        public Guid BoxTemplateId { get; set; }
        [DataMember]
        public Guid FolderTemplateId { get; set; }
    }
    [DataContract]
    public class GlobalUniqueIdSettingsDto
    {
        [DataMember]
        public string BoxTemplatePrefix { get; set; }
        [DataMember]
        public int BoxTemplateNumberOfDigits { get; set; }
        [DataMember]
        public string FolderTemplatePrefix { get; set; }
        [DataMember]
        public int FolderTemplateNumberOfDigits { get; set; }
        [DataMember]
        public string RecordTemplatePrefix { get; set; }
        [DataMember]
        public int RecordTemplateNumberOfDigits { get; set; }
        [DataMember]
        public string CustomTemplatePrefix { get; set; }
        [DataMember]
        public int CustomTemplateNumberOfDigits { get; set; }
    }
    [DataContract]
    public class DelTemplateParam : BaseTemplateParam
    {
        [DataMember]
        public Guid TemplateId { get; set; }
        //public Guid ParentFolderId { get; set; }
        //public Guid ParentBoxId { get; set; }
    }
    [DataContract]
    public class BaseTemplateParam
    {
        /// <summary>
        /// id list start from suite. first one is suite unique id, others are template id(not unique id)
        /// </summary>
        [DataMember]
        public List<string> TemplateIdList { get; set; }
    }
    public enum SaveTemplateResult
    {
        None = 0,
        MissUniqueIdSettingMode = 1,
        PrefixDuplicate = 2,
        NameDuplicate = 3,
        CustomTeplateExceedMaxDepth = 4,
        Success = 10,
        Failed = 11
    }

    public class SaveTemplateResultWithTemplate
    {
        public SaveTemplateResult SaveTemplateResult { get; set; }

        public object TemplateInfo { get; set; }
    }

    public class SuiteTemplateTreeNode
    {
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        public TemplateType Type { get; set; }
        /// <summary>
        /// only valid while value of Type property is TemplateType.Suite
        /// </summary>
        public SuiteStartFromType? StartFromType { set; get; } 
        /// <summary>
        /// id list start from suite to current. first one is suite unique id, others are template id(not unique id)
        /// </summary>
        public List<string> TemplateIdList { get; set; }

        public List<SuiteTemplateTreeNode> Children { get; set; }
        public int ChildrenCount { get; set; }

        public bool IsUnderDefaultSuite { get; set; }
    }

    public class SuiteTemplateBrowserResultDto
    {
        public int ChildrenCount { get; set; }

        public List<SuiteTemplateTreeNode> Children { get; set; }
    }
    [DataContract]
    public class SuiteTemplateBrowserDto
    {
        [DataMember]
        public SuiteTemplatePagingInfo PagingInfo { get; set; }
        [DataMember]
        public SuiteTemplateTreeNode Node { get; set; }

    }
}
