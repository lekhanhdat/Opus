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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    internal class ActionMappingManagerBase
    {
        protected INintexDataMappingManager dataMappingManager;
        protected NWListLookupCacheManager listLookupCacheManager;
        protected ListLookupMappingManger listLookupMappingManager;
        public ActionMappingManagerBase(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager dataMappingManager, ListLookupMappingManger listLookupMappingManager)
        {
            this.listLookupCacheManager = listLookupCacheManager;
            this.dataMappingManager = dataMappingManager;
            this.listLookupMappingManager = listLookupMappingManager;
        }

        private void MappingValueData(Value value)
        {
            if (value.ListLookup != null)
            {
                listLookupMappingManager.MappingListLookupData(value.ListLookup);
            }

            if (value.PrimitiveValue != null)
            {
                listLookupMappingManager.MappingPrimitiveValueListLookupData(value.PrimitiveValue);
            }

        }

        private void MappingParametersData(Parameters parameter)
        {
            if (parameter.Value == null)
            {
                return;
            }
            if (parameter.Value.ListLookup != null)
            {
                listLookupMappingManager.MappingListLookupData(parameter.Value.ListLookup);
            }

            if (parameter.Value.Dictionary != null)
            {
                foreach (var dictionaryValue in parameter.Value.Dictionary)
                {
                    if (dictionaryValue.Value != null)
                    {
                        MappingValueData(dictionaryValue.Value);
                    }
                }
            }

            if (parameter.Value.PrimitiveValue != null)
            {
                listLookupMappingManager.MappingPrimitiveValueListLookupData(parameter.Value.PrimitiveValue);
            }


            if (parameter.Value.Collection != null && parameter.Value.Collection.SelectedValue != null)
            {
                foreach (var selectedValue in parameter.Value.Collection.SelectedValue)
                {
                    if (selectedValue.ListLookup != null)
                    {
                        listLookupMappingManager.MappingListLookupData(selectedValue.ListLookup);
                    }
                    if (selectedValue.user != null)
                    {
                        selectedValue.user.Login = dataMappingManager.GetMappingLoginName(selectedValue.user.Login);
                    }
                }
            }

            if (parameter.Value.User != null)
            {
                parameter.Value.User.Login = dataMappingManager.GetMappingLoginName(parameter.Value.User.Login);
            }
        }

        private void MappingCommonData(WorkflowAction workflowAction)
        {
            if (workflowAction.Configuration.Properties == null)
            {
                return;
            }

            foreach (var property in workflowAction.Configuration.Properties)
            {
                foreach (var parameter in property.Parameters)
                {
                    MappingParametersData(parameter);
                }
            }
        }

        protected string RemoveUserPrefix(string userLoginName)
        {
            string tempUser = userLoginName;
            if (!string.IsNullOrEmpty(tempUser))
            {
                if (tempUser.IndexOf('|') > 0 && tempUser.Split('|').Length == 3)
                {
                    tempUser = tempUser.Split('|')[2];
                }
                else if (tempUser.IndexOf(':') > 0 && tempUser.Split(':').Length == 2)
                {
                    tempUser = tempUser.Split(':')[1];
                }
            }
            return tempUser;
        }

        protected Parameters FindParameterByName(Parameters[] parameters, string parameterName)
        {
            return parameters.First(parameter => string.Equals(parameterName, parameter.Name, StringComparison.OrdinalIgnoreCase));
        }
        public virtual void ConvertActionData(WorkflowAction workflowAction)
        {
            MappingCommonData(workflowAction);
        }
    }
}
