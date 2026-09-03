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
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;

namespace LS.SPWorkflowProcessor
{
    public abstract class SPWorkflowAssociationDataProc
    {
        //protected Dictionary<string, object> assoData = new Dictionary<string, object>();
        protected string[] fieldsNeedChecked;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "dfs is a key")]
        protected Dictionary<string, string> GetAssociationDataDictionary(string strXml)
        {
            string str;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(strXml);
            }
            catch (XmlException)
            {
                return dict;
            }
            XmlNode firstChild = document.FirstChild;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);

            nsmgr.AddNamespace("dfs", "http://schemas.microsoft.com/office/InfoPath/2003/dataFormSolution");
            nsmgr.AddNamespace("d", "http://schemas.microsoft.com/office/InfoPath/2009/WSSList/dataFields");
            str = "/dfs:myFields/dfs:dataFields/d:SharePointListItem_RW";

            XmlNode node2 = document.SelectSingleNode(str, nsmgr);
            if (node2 != null)
            {
                foreach (XmlNode node3 in node2.ChildNodes)
                {
                    string localName = node3.LocalName;
                    dict.Add(node3.Name, node3.InnerXml);
                }
            }
            return dict;
        }

        protected virtual string[] FieldsNeedChecked
        {
            get
            {
                return fieldsNeedChecked;
            }
        }

        public virtual bool CheckAssociationData(string data1, string data2)
        {
            //return data1 == data2;
            Dictionary<string, string> dict1 = GetAssociationDataDictionary(data1);
            Dictionary<string, string> dict2 = GetAssociationDataDictionary(data2);
            if (FieldsNeedChecked != null)
            {
                foreach (string field in FieldsNeedChecked)
                {
                    if (dict1.ContainsKey(field) != dict2.ContainsKey(field))
                    {
                        return false;
                    }
                    if (dict1.ContainsKey(field))
                    {
                        if (CheckField(dict1[field], dict2[field]))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        protected virtual bool CheckField(string field1, string field2)
        {
            return field1 == field2;
        }
    }
    
    public class SPApprovalWFAssociationDataProc : SPWorkflowAssociationDataProc
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "the following chars are keys")]
        protected override string[] FieldsNeedChecked
        {
            get
            {
                if (fieldsNeedChecked == null)
                {
                    fieldsNeedChecked = new string[]
                    {
                        "d:Approvers",
                        "d:ExpandGroups",
                        "d:NotificationMessage",
                        "d:DueDateforAllTasks",
                        "d:DurationforSerialTasks",
                        "d:DurationUnits",
                        "d:CC",
                        "d:CancelonRejection",
                        "d:CancelonChange",
                        "d:EnableContentApproval"
                    };
                }
                return fieldsNeedChecked;
            }
        }
    }
    
    public class SPCollectFeedbackWFAssociationDataProc : SPWorkflowAssociationDataProc
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "the following chars are keys")]
        protected override string[] FieldsNeedChecked
        {
            get
            {
                if (fieldsNeedChecked == null)
                {
                    fieldsNeedChecked = new string[]
                    {
                        "d:Reviewers",
                        "d:ExpandGroups",
                        "d:NotificationMessage",
                        "d:DueDateforAllTasks",
                        "d:DurationforSerialTasks",
                        "d:DurationUnits",
                        "d:CC",
                        "d:CancelonChange"
                    };
                }
                return fieldsNeedChecked;
            }
        }

    }
    public class SPCollectSignatureWFAssociationDataProc : SPWorkflowAssociationDataProc
    {
        protected override string[] FieldsNeedChecked
        {
            get
            {
                if (fieldsNeedChecked == null)
                {
                    fieldsNeedChecked = new string[]
                    {
                        "d:Signers",
                        "d:CC"
                    };
                }
                return fieldsNeedChecked;
            }
        }
    }

    public class SPWorkflowAssociationDataFactory
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<Guid, Type> dict = new Dictionary<Guid, Type>();
        static SPWorkflowAssociationDataFactory()
        {
            dict.Add(new Guid("8AD4D8F0-93A7-4941-9657-CF3706F00409"), typeof(SPApprovalWFAssociationDataProc));
            dict.Add(new Guid("3BFB07CB-5C6A-4266-849B-8D6711700409"), typeof(SPCollectFeedbackWFAssociationDataProc));
            dict.Add(new Guid("77C71F43-F403-484B-BCB2-303710E00409"), typeof(SPCollectSignatureWFAssociationDataProc));
            //dict.Add();
        }

        public static SPWorkflowAssociationDataProc GetWorkflowAssociationDataProc(IAveWorkflowAssociation asso)
        {
            try
            {
                return (SPWorkflowAssociationDataProc)AveAssemblyUtility.CreateInstanceByType(GetTypeByBaseId(asso.BaseTemplate.BaseId));
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetWorkflowAssociationDataError, e.ToString());
                return null;
            }
        }

        private static Type GetTypeByBaseId(Guid guid)
        {
            if (dict.ContainsKey(guid))
            {
                return dict[guid];
            }
            return null;
        }
    }
}
