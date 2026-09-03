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
using System.Text;
using AvePoint.Wrapper.Common;
using System.Xml;

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class PeopleControl : BaseControl
    {
        public PeopleControl(IAveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager, string prefix)
            : base(web, list, contentTypeId, controlNode, nsManager, prefix)
        {
            mWeb = web;
            mList = list;
            mControlNode = controlNode;
            this.nsManager = nsManager;
            this.contentTypeId = contentTypeId;
            AddControlNameSpace();
        }

        public override void ProcessControl(bool isPost)
        {
            base.ProcessControl(isPost);
            UpdateSelectionSetNode();
            ProcessDefaultValueUserMapping();
        }

        private void ProcessDefaultValueUserMapping()
        {
            var sourceDeafultValue = mControlNode.SelectSingleNode(GetXPath("DefaultValue"), nsManager);
            if (sourceDeafultValue == null || string.IsNullOrEmpty(sourceDeafultValue.InnerText))
            {
                return;
            }
            string userLoginName = sourceDeafultValue.InnerText;
            sourceDeafultValue.InnerText = mWeb.ParentSite.SPMembers.GetMappingUserLogin(userLoginName);

        }

        /// <summary>
        /// selection set node can't be import successfully with local node type
        /// update it to online node namespace and property name
        /// </summary>
        private void UpdateSelectionSetNode()
        {
            var rootPrefix = GetNodePrefixAndAddNameSpace(mControlNode, "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls");
            var selectionSetNode = mControlNode.SelectSingleNode(GetXPath(rootPrefix,"SelectionSet"),nsManager);
            var selectionSetElement = selectionSetNode as XmlElement;
            if (selectionSetElement != null)
            {
                var subPrefix = GetNodePrefixAndAddNameSpace(selectionSetElement, "http://schemas.datacontract.org/2004/07/Microsoft.SharePoint.WebControls");
                var attribute = selectionSetElement.GetAttributeNode(string.Format("xmlns:{0}", subPrefix));// "xmlns:d4p1");
                if (attribute != null)
                {
                    attribute.Value = "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint";
                }
                selectionSetElement.InnerXml = selectionSetElement.InnerXml.Replace(
                "http://schemas.datacontract.org/2004/07/Microsoft.SharePoint.WebControls",
                "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint")
                .Replace(
                "PeopleEditor.AccountType",
                "SP_AccountGroup");
            }
        }

        public string GetNodePrefixAndAddNameSpace(XmlNode node,string nameSpace)
        {
            string prefix = node.GetPrefixOfNamespace(nameSpace);
            if (!string.IsNullOrEmpty(prefix))
            {
                nsManager.AddNamespace(prefix, nameSpace);
            }
            return prefix;
        }

        public override void AddControlNameSpace()
        {
            nsManager.AddNamespace("d3p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls");
        }
    }
}

//llocal
//<d3p1:SelectionSet xmlns:d4p1="http://schemas.datacontract.org/2004/07/Microsoft.SharePoint.WebControls">
//					<d4p1:PeopleEditor.AccountType>User</d4p1:PeopleEditor.AccountType>
//					<d4p1:PeopleEditor.AccountType>SecGroup</d4p1:PeopleEditor.AccountType>
//					<d4p1:PeopleEditor.AccountType>SPGroup</d4p1:PeopleEditor.AccountType>
//				</d3p1:SelectionSet>
//online
//<a:SelectionSet xmlns:b="http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint">
//			<b:SP_AccountGroup>User</b:SP_AccountGroup>
//			<b:SP_AccountGroup>SecGroup</b:SP_AccountGroup>
//			<b:SP_AccountGroup>SPGroup</b:SP_AccountGroup>
//		</a:SelectionSet>
