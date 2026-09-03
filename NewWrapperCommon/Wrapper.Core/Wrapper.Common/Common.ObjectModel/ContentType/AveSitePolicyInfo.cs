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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;

    public class AveSitePolicyInfo
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "projectpolicy is the part of a  url. ")]
        public void LoadFromXml(string schema)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(schema);
            XmlNode contentTypeNode = doc.SelectSingleNode("ContentType");
            Id = contentTypeNode.Attributes["ID"].Value;
            Name = contentTypeNode.Attributes["Name"].Value;
            Description = contentTypeNode.Attributes["Description"].Value;
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("pp", "http://schemas.microsoft.com/office/server/projectpolicy");
            XmlNode policyNode = doc.SelectSingleNode("/ContentType/XmlDocuments/XmlDocument/pp:ProjectPolicy", nsManager);
            if (policyNode == null)
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_PolicySchemaIsNull);
            }
            CloseDeleteOption = policyNode.SelectSingleNode("CloseDeleteOption").InnerXml;
            WorkflowId = policyNode.SelectSingleNode("WorkflowId").InnerXml;
            NumberOfTimePeriodOnWorkflow = policyNode.SelectSingleNode("NumberOfTimePeriodOnWorkflow").InnerXml;
            TimePeriodOnWorkflow = policyNode.SelectSingleNode("TimePeriodOnWorkflow").InnerXml;
            AllowWorkflowRecur = policyNode.SelectSingleNode("AllowWorkflowRecur").InnerXml;
            NumberOfTimePeriodOnWorkflowRecur = policyNode.SelectSingleNode("NumberOfTimePeriodOnWorkflowRecur").InnerXml;
            TimePeriodOnWorkflowRecur = policyNode.SelectSingleNode("TimePeriodOnWorkflowRecur").InnerXml;
            NumberOfTimePeriodOnClose = policyNode.SelectSingleNode("NumberOfTimePeriodOnClose").InnerXml;
            TimePeriodOnClose = policyNode.SelectSingleNode("TimePeriodOnClose").InnerXml;
            FieldNameOnDelete = policyNode.SelectSingleNode("FieldNameOnDelete").InnerXml;
            NumberOfTimePeriodOnDelete = policyNode.SelectSingleNode("NumberOfTimePeriodOnDelete").InnerXml;
            TimePeriodOnDelete = policyNode.SelectSingleNode("TimePeriodOnDelete").InnerXml;
            AllowEmailNotification = policyNode.SelectSingleNode("AllowEmailNotification").InnerXml;
            NumberOfTimePeriodOnEmailNotification = policyNode.SelectSingleNode("NumberOfTimePeriodOnEmailNotification").InnerXml;
            TimePeriodOnEmailNotification = policyNode.SelectSingleNode("TimePeriodOnEmailNotification").InnerXml;
            AllowEmailFollowUp = policyNode.SelectSingleNode("AllowEmailFollowUp").InnerXml;
            NumberOfTimePeriodOnEmailFollowUp = policyNode.SelectSingleNode("NumberOfTimePeriodOnEmailFollowUp").InnerXml;
            TimePeriodOnEmailFollowUp = policyNode.SelectSingleNode("TimePeriodOnEmailFollowUp").InnerXml;
            AllowPostpone = policyNode.SelectSingleNode("AllowPostpone").InnerXml;
            NumberOfTimePeriodOnPostpone = policyNode.SelectSingleNode("NumberOfTimePeriodOnPostpone").InnerXml;
            TimePeriodOnPostpone = policyNode.SelectSingleNode("TimePeriodOnPostpone").InnerXml;
            CloseToReadOnly = policyNode.SelectSingleNode("CloseToReadOnly").InnerXml;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string CloseDeleteOption { get; private set; }
        public string WorkflowId { get; private set; }
        public string NumberOfTimePeriodOnWorkflow { get; private set; }
        public string TimePeriodOnWorkflow { get; private set; }
        public string AllowWorkflowRecur { get; private set; }
        public string NumberOfTimePeriodOnWorkflowRecur { get; private set; }
        public string TimePeriodOnWorkflowRecur { get; private set; }
        public string NumberOfTimePeriodOnClose { get; private set; }
        public string TimePeriodOnClose { get; private set; }
        public string FieldNameOnDelete { get; private set; }
        public string NumberOfTimePeriodOnDelete { get; private set; }
        public string TimePeriodOnDelete { get; private set; }
        public string AllowEmailNotification { get; private set; }
        public string NumberOfTimePeriodOnEmailNotification { get; private set; }
        public string TimePeriodOnEmailNotification { get; private set; }
        public string AllowEmailFollowUp { get; private set; }
        public string NumberOfTimePeriodOnEmailFollowUp { get; private set; }
        public string TimePeriodOnEmailFollowUp { get; private set; }
        public string AllowPostpone { get; private set; }
        public string NumberOfTimePeriodOnPostpone { get; private set; }
        public string TimePeriodOnPostpone { get; private set; }
        public string CloseToReadOnly { get; private set; }
    }
}
