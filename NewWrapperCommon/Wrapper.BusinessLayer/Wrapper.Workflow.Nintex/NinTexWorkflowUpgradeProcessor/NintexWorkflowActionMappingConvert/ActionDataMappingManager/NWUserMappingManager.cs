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
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LS.SPWorkflowProcessor
{
    class NWUserMappingManager
    {
        private AveSPUserMappingManager userMappingManager;
        private IAveSPMembers spMembers;
        private bool forceEnsureUsersInWorkflow;
        private Dictionary<string, string> loginNameExclude = null;
        private const string regsting = @"\\|([/*?""<>|:])";

        public NWUserMappingManager(IAveSPMembers spMembers, bool forceEnsureUsersInWorkflow)
        {
            this.spMembers = spMembers;
            this.forceEnsureUsersInWorkflow = forceEnsureUsersInWorkflow;
            userMappingManager = new AveSPUserMappingManager(spMembers.UserAndDomainMapping.GetMappingLoginNameBeforeAdd, spMembers.UserAndDomainMapping.GetMappingDomainNameBeforeAdd);
        }

        public string GetMappingLoginName(string userLoginName)
        {
            return userMappingManager.GetMappingUserLogin(userLoginName, false, true);
        }

        public IAvePrincipal GetUserByLoginName(string userLoginName)
        {
            if (CheckLoginName(userLoginName))
            {
                return null;
            }
            IAvePrincipal mappedMember = null;
            var member = spMembers.GetMemberObjectByLogin(userLoginName);
            if (member != null)
            {
                mappedMember = spMembers.GetOrAddPrincipal(member, false);
            }
            else if (forceEnsureUsersInWorkflow)
            {
                mappedMember = spMembers.GetOrAddUser(userLoginName);
            }
            return mappedMember;
        }

        private bool CheckLoginName(string loginName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveUserMappingService.CheckLoginName"))
            {

                if (CheckExclude(loginName))
                {
                    return true;
                }
                if (string.Equals("{x:Null}", loginName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                int i;
                if ((i = loginName.IndexOf('\\')) > 0)
                {
                    string later = loginName.Substring(i + 1, loginName.Length - i - 1);
                    Regex regex = new Regex(regsting);
                    if (regex.IsMatch(later))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((i = loginName.LastIndexOf('|')) > 0)
                {
                    string later = loginName.Substring(i + 1, loginName.Length - i - 1);
                    Regex regex = new Regex(regsting);
                    if (regex.IsMatch(later))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((i = loginName.LastIndexOf(':')) > 0)
                {
                    string later = loginName.Substring(i + 1, loginName.Length - i - 1);
                    Regex regex = new Regex(regsting);
                    if (regex.IsMatch(later))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;


            }

        }
        private bool CheckExclude(string loginName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveUserMappingService.CheckExclude"))
            {

                if (loginNameExclude != null)
                {
                    if (loginNameExclude.ContainsKey("FilterString"))
                    {
                        string filterstring = loginNameExclude["FilterString"];
                        string[] filterstrings = filterstring.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in filterstrings)
                        {
                            if (loginName.Contains(s))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
                return false;


            }

        }
    }
}
