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
using AvePoint.Common;
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LS.SPWorkflowProcessor
{
    class SetItemPermissionsActionMappingManager : ActionMappingManagerBase
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SetItemPermissionsActionMappingManager));
        public SetItemPermissionsActionMappingManager(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
            : base(listLookupCacheManager, dataMappingManager, listLookupMappingManager)
        { }

        public override void ConvertActionData(WorkflowAction workflowAction)
        {
            base.ConvertActionData(workflowAction);

            MappingListTitle(workflowAction.Configuration.Properties[1].Parameters[0]);
            MappingUserLogingName(workflowAction.Configuration.Properties[6].Parameters[0]);
            MappingUserTypeColumnValue(workflowAction.Configuration.Properties[2].Parameters[0], workflowAction.Configuration.Properties[1].Parameters[0].Value.PrimitiveValue.Value.StringValue);
        }

        private void MappingUserTypeColumnValue(Parameters parameter, string QueryListTitle)
        {
            if (parameter.Value.PrimitiveValue != null && string.IsNullOrEmpty(parameter.Value.PrimitiveValue.Value.StringValue))
            {
                return;
            }
            try
            {
                var xml = parameter.Value.PrimitiveValue.Value.StringValue.Substring(1);
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                var result = document.SelectNodes("//Eq");
                foreach (XmlNode node in result)
                {
                    var fieldNode = node.SelectSingleNode("FieldRef");
                    var valueNode = node.SelectSingleNode("Value");
                    var fieldName = fieldNode.Attributes["Name"].Value;
                    var value = valueNode.InnerText;
                    if (string.Equals(fieldName, "{0}", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(value, "{0}", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var queryList = base.dataMappingManager.GetParentWeb().GetListByName(QueryListTitle, false);
                    if (queryList != null)
                    {
                        var field = queryList.Fields.GetFieldByInternalName(fieldName);
                        if (field.Type == AvePoint.Wrapper.Common.AveFieldType.User)
                        {
                            valueNode.InnerText = dataMappingManager.GetMappingLoginName(value);
                        }
                    }
                }
                parameter.Value.PrimitiveValue.Value.StringValue = string.Format("0{0}", document.InnerXml);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while mapping user type column, error: {0}", e);
            }
        }
        private void MappingUserLogingName(Parameters parameter)
        {
            if (parameter.Value.PrimitiveValue == null || string.IsNullOrEmpty(parameter.Value.PrimitiveValue.Value.StringValue))
            {
                return;
            }

            string[] userLoginNames = parameter.Value.PrimitiveValue.Value.StringValue.Split(';');
            StringBuilder tempUserLoginNames = new StringBuilder();
            foreach (var userLoginName in userLoginNames)
            {
                var tmpDestUser = dataMappingManager.GetMappingLoginName(userLoginName);
                tempUserLoginNames.AppendFormat("{0};", RemoveUserPrefix(tmpDestUser));
            }
            tempUserLoginNames.Length--;
            parameter.Value.PrimitiveValue.Value.StringValue = tempUserLoginNames.ToString();
        }

        /// <summary>
        /// on-premise 该值存的是list id
        /// </summary>
        /// <param name="parameter"></param>
        private void MappingListTitle(Parameters parameter)
        {
            if (Validator.IsGuid(parameter.Value.PrimitiveValue.Value.StringValue))
            {
                parameter.Value.PrimitiveValue.Value.StringValue = dataMappingManager.GetListTitleFromMapping(new Guid(parameter.Value.PrimitiveValue.Value.StringValue));
            }
        }


    }
}
