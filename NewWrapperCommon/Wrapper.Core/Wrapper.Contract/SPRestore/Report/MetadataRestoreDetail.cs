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

using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using System.Text;
namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// Restore Details
    /// </summary>
    public sealed class MetadataRestoreDetails
    {
        /// <summary>
        /// Metadata Status
        /// </summary>
        public WrapperRestoreStatus Status { get; internal set; }

        /// <summary>
        /// Error Message
        /// </summary>
        public WrapperInternationalMessage Message { get; internal set; }

        /// <summary>
        /// Details including object restore dto
        /// </summary>
        public List<SPObjectRestoreDto> WrapperDetails { get; internal set; }

        public MetadataRestoreDetails()
            : this(WrapperRestoreStatus.None)
        { }

        /// <summary>
        /// constructure method
        /// </summary>
        public MetadataRestoreDetails(WrapperRestoreStatus status)
            : this(status, string.Empty)
        {

        }

        public MetadataRestoreDetails(WrapperRestoreStatus status, string message)
        {
            this.Status = status;
            this.Message = message;
        }

        public MetadataRestoreDetails(WrapperRestoreStatus status, WrapperInternationalMessage message)
        {
            this.Status = status;
            this.Message = message;
        }

        /// <summary>
        /// Ensure Wrapper Details
        /// </summary>
        private void EnsureWrapperDetails()
        {
            if(WrapperDetails == null)
            {
                WrapperDetails = new List<SPObjectRestoreDto>();
            }
        }

        /// <summary>
        /// Analyze report and add report.
        /// </summary>
        /// <param name="report"></param>
        internal void AnalyzeReport(IReport report)
        {
            if(report != null)
            {
                EnsureWrapperDetails();

                using (report)
                {
                    foreach (var reportDto in report.GetDetails())
                    {
                        var dto = new SPObjectRestoreDto()
                        {
                            Name = reportDto.Name,
                            Type = ConvertToSPObjectType(reportDto.Type),
                            Status = Convert(reportDto.Status),
                            Message = new WrapperInternationalMessage(reportDto.ErrorMessage) { Key = reportDto.Key, Arguments = reportDto.Parameters }
                        };

                        WrapperDetails.Add(dto);
                    }
                }
            }
        }

        /// <summary>
        /// combine another details
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        internal MetadataRestoreDetails Combine(MetadataRestoreDetails details)
        {
            if (details != null && details.Status != WrapperRestoreStatus.None)
            {
                Status = (Status > details.Status) ? Status : details.Status;

                if (Message != null)
                {
                    Message = string.Format("{0} \r\n {1}", Message, details.Message);
                }
                else
                {
                    Message = details.Message;
                }

                // 把details里面的wrapperDetails Combine进来
                if (details.WrapperDetails != null && details.WrapperDetails.Count > 0)
                {
                    EnsureWrapperDetails();

                    foreach (var detail in details.WrapperDetails)
                    {
                        WrapperDetails.Add(detail);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Convert status to Metadata Restore Status
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        internal static WrapperRestoreStatus Convert(AveStatus status)
        {
            switch(status)
            {
                case AveStatus.Failed:
                    return WrapperRestoreStatus.Failed;
                case AveStatus.Skipped:
                    return WrapperRestoreStatus.Skipped;
                case AveStatus.Successful:
                    return WrapperRestoreStatus.Successful;
                default:
                    return WrapperRestoreStatus.None;
            }
        }

        /// <summary>
        /// Add Dto
        /// </summary>
        /// <param name="restoreDto"></param>
        internal void AddDto(SPObjectRestoreDto restoreDto)
        {
            EnsureWrapperDetails();
            WrapperDetails.Add(restoreDto);
        }

        /// <summary>
        /// old type -> new type
        /// </summary>
        private static readonly Dictionary<string, SPObjectType> specialTypeDict = new Dictionary<string, SPObjectType>(System.StringComparer.OrdinalIgnoreCase) 
        {
            {"List Workflow Definition", SPObjectType.WorkflowAssociation},
            {"Workflow Definition for List Content Type", SPObjectType.ContentTypeWorkflowAssociation},
            {"Site Workflow Definition", SPObjectType.WorkflowAssociation},
            {"Workflow Definition for Site Content Type", SPObjectType.ContentTypeWorkflowAssociation},
            {"Workflow Instance", SPObjectType.WorkflowInstance},
            {"List Content Type", SPObjectType.ContentType},
            {"Site Content Type", SPObjectType.ContentType},
            {"List Column", SPObjectType.Field},
            {"Site Column", SPObjectType.Field},
            {"List Property", SPObjectType.Setting},
            {"SiteSetting", SPObjectType.Setting}
        };

        private static SPObjectType ConvertToSPObjectType(string type)
        {
            SPObjectType objectType;

            if (!specialTypeDict.TryGetValue(type, out objectType))
            {
                objectType = (SPObjectType)System.Enum.Parse(typeof(SPObjectType), type, true);
            }

            return objectType;
        }
    }

    /// <summary>
    /// 还原一个Metadata会出现很多细节，
    /// 比如还原users，会出现一个user还原不了，
    /// 那么就需要report这个user给外围。
    /// </summary>
    public sealed class SPObjectRestoreDto
    {
        /// <summary>
        /// 状态
        /// </summary>
        public WrapperRestoreStatus Status { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public SPObjectType Type { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public WrapperInternationalMessage Message { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SPObjectType : byte
    {
        /// <summary>
        /// Web Part
        /// </summary>
        WebPart = 0,
        /// <summary>
        /// User
        /// </summary>
        User,
        /// <summary>
        /// Group
        /// </summary>
        Group,
        /// <summary>
        /// Content Type
        /// </summary>
        ContentType,
        /// <summary>
        /// Field
        /// </summary>
        Field,
        /// <summary>
        /// Permission Level
        /// </summary>
        PermissionLevel,
        /// <summary>
        /// Role Assignment
        /// </summary>
        RoleAssignment,
        /// <summary>
        /// Feature
        /// </summary>
        Feature,
        /// <summary>
        /// setting
        /// </summary>
        Setting,
        /// <summary>
        /// Self
        /// </summary>
        Self,
        /// <summary>
        /// User Setting
        /// </summary>
        UserSetting,
        /// <summary>
        /// Group Distribution setting
        /// </summary>
        GroupDistributionSetting,
        /// <summary>
        /// Group members
        /// </summary>
        GroupMembers,
        /// <summary>
        /// Group settings
        /// </summary>
        GroupSettings,
        /// <summary>
        /// Search keyword
        /// </summary>
        SearchKeyword,
        /// <summary>
        /// Search Scope
        /// </summary>
        SearchScope,
        /// <summary>
        /// Search Scope Display Group
        /// </summary>
        SearchScopeDisplayGroup,

        /// <summary>
        /// Social Comment
        /// </summary>
        SocialComment,

        /// <summary>
        /// Socia lTag
        /// </summary>
        SocialTag,
        /// <summary>
        /// workflow association
        /// </summary>
        WorkflowAssociation,
        /// <summary>
        /// Content type workflow association
        /// </summary>
        ContentTypeWorkflowAssociation,
        /// <summary>
        /// workflow instance
        /// </summary>
        WorkflowInstance,

        /// <summary>
        /// Metadata
        /// </summary>
        ManagedMetadata,

        /// <summary>
        /// Metadata
        /// </summary>
        TermGroup,

        /// <summary>
        /// Metadata
        /// </summary>
        TermSet,

        /// <summary>
        /// Metadata
        /// </summary>
        Term,

        /// <summary>
        /// Event receiver
        /// </summary>
        EventReceiver,

        /// <summary>
        /// language file
        /// </summary>
        LanguageFile,
        /// <summary>
        /// workflow schedual
        /// </summary>
        WorkflowSchedual,
        /// <summary>
        /// workflow template
        /// </summary>
        WorkflowTemplate
    }

    /// <summary>
    /// Object level
    /// </summary>
    public enum SPObjectLevel : byte
    {
        /// <summary>
        /// Site Collection
        /// </summary>
        SiteCollection=1,
        /// <summary>
        /// web
        /// </summary>
        Web=2,
        /// <summary>
        /// list
        /// </summary>
        List=4,
        /// <summary>
        /// folder
        /// </summary>
        Folder=8,
        /// <summary>
        /// document
        /// </summary>
        Document=16,
        /// <summary>
        /// listitem
        /// </summary>
        ListItem=32,
        /// <summary>
        /// attachment
        /// </summary>
        Attachment=64
    }

    /// <summary>
    /// 支持国际化的message
    /// </summary>
    public sealed class WrapperInternationalMessage
    {
        /// <summary>
        /// 只有message的构造函数
        /// </summary>
        /// <param name="message"></param>
        public WrapperInternationalMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// 只有key和arguments的构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arguments"></param>
        public WrapperInternationalMessage(string key, List<object> arguments)
        {
            this.Key = key;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Formated message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Message key
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// Related arguments
        /// </summary>
        public List<object> Arguments { get; internal set; }

        /// <summary>
        /// User-defined conversion from WrapperInternationalMessage to string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static implicit operator string(WrapperInternationalMessage message)
        {
            return message.Message;
        }

        /// <summary>
        /// User-defined conversion from string to WrapperInternationalMessage
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static implicit operator WrapperInternationalMessage(string message)
        {
            return new WrapperInternationalMessage(message);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("Message:");
            builder.Append(Message);
            builder.Append(", Key:");
            builder.Append(Key);
            builder.Append(", Arguments:");
            var arguments = Arguments;
            if (arguments != null && arguments.Count > 0)
            {
                foreach(var item in arguments)
                {
                    builder.Append(item);
                    builder.Append("\t");
                }
            }

            return builder.ToString();
        }
    }
}