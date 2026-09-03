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
using Native13NinTexWorkflowEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    class UserVariableProcessor : VariablesProcessorBase
    {
        private IAveWeb parentWeb;

        private Dictionary<string, int> sharepointGroupsCache = new Dictionary<string, int>();
        public UserVariableProcessor(NWWorkflowVariable currentVar,IAveWeb parentWeb)
            : base(currentVar)
        {
            this.parentWeb = parentWeb;
        }

        protected override VariableConfiguration GetVariableConfiguration()
        {
            var configuration = new PersonGroupConfiguration
            {
                //User does not have default value
                DefaultValue = string.Empty,
                Description = currentVar.Description,
                ChoiceType = GetCoiceType(),
                AllowBlank = !currentVar.Required,
                AllowBlankSpecified = true,
                SharePointGroups = GetSharepointGroups(),
            };
            ConfigSelectTypeAndGroupId(configuration);
            return configuration;
        }

        private List<SharePointGroup> GetSharepointGroups()
        {
            var list = new List<SharePointGroup>();
            if (parentWeb != null && parentWeb.SiteGroups != null)
            {
                foreach (var group in parentWeb.SiteGroups)
                {
                    sharepointGroupsCache[group.Name] = group.ID;
                    list.Add(new SharePointGroup() {
                        Name = group.Name,
                        Id = group.ID,
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Online 只有PeopleOnlye和PeopleAndGroups这两个choice type，没法其他选择
        /// </summary>
        /// <param name="sourceChoiceType"></param>
        /// <returns></returns>
        private string GetCoiceType()
        {
            if (currentVar.Choice == null || currentVar.Choice.Length < 2)
            {
                return "PeopleAndGroups";
            }
            var sourceChoiceType = currentVar.Choice[1];
            if (string.Equals("User", sourceChoiceType, StringComparison.OrdinalIgnoreCase))
            {
                return "PeopleOnly";
            }
            return "PeopleAndGroups";
        }

        private void ConfigSelectTypeAndGroupId(PersonGroupConfiguration configuration)
        {
            if (currentVar.Choice == null || currentVar.Choice.Length < 1)
            {
                configuration.SelectionType = "AllUsers";
            }
            else
            {
                var groupName = currentVar.Choice[0];

                if (string.IsNullOrEmpty(groupName))
                {
                    configuration.SelectionType = "AllUsers";
                }
                else
                {
                    configuration.SelectionType = "SharePointGroup";
                    int id;
                    sharepointGroupsCache.TryGetValue(groupName, out id);
                    configuration.SharePointGroupId = id;
                }
            }
        }

        protected override bool GetInitiate()
        {
            return true;
        }

    }
}
