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


using LS.SPWorkflowProcessor;

namespace AvePoint.Wrapper.Restore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LS.SPWorkflowProcessor.Services;
    using AvePoint.Wrapper.Common;
    using System.Text.RegularExpressions;

    internal sealed class AveUserMappingService : UserMappingService
    {
        private const string regsting = @"\\|([/*?""<>|:])";
        private Dictionary<string, string> loginNameExclude = null;

        [ThreadStatic]
        private static AveSPSite mAveSite;

        internal static AveSPSite AveSite
        {
            get { return mAveSite; }
            set { mAveSite = value; }
        }

        internal static string DefaultUser
        {
            get;
            set;
        }

        private AveUserMappingService()
            : base()
        { }

        private AveUserMappingService(Dictionary<string, string> param)
        {
            if (param != null)
            {
                loginNameExclude = param;
                if (param.ContainsKey("DefaultUser"))
                {
                    DefaultUser = param["DefaultUser"];
                }
            }
        }
        public override IAveUser GetOrCreateUser(string loginName)
        {
            IAvePrincipal principal = GetOrCreateMember(loginName);
            if (principal is IAveUser)
            {
                return principal as IAveUser;
            }
            return null;
        }

        public override IAvePrincipal GetOrCreateMember(string name)
        {
            if (AveSite == null || AveSite.SPMembers == null)
                return null;
            if (CheckLoginName(name))
            {
                return null;
            }
            IAvePrincipal mappedMember = null;
            var member = AveSite.SPMembers.GetMemberObjectByLogin(name);
            if (member != null)
            {
                mappedMember = AveSite.SPMembers.GetOrAddPrincipal(member, false);
            }
            else if (SPWorkflowProcessorRuntime.ForceEnsureUsersInWorkflow)
            {
                mappedMember = AveSite.SPMembers.GetOrAddUser(name);
            }
            //add default user.
            if (mappedMember == null && !string.IsNullOrEmpty(DefaultUser))
            {
                mappedMember = AveSite.SPSite.RootWeb.EnsureAvailableUser(DefaultUser);
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
