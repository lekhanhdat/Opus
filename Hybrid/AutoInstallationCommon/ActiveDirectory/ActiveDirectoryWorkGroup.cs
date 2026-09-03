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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace AutoInstallationCommon.ActiveDirectory
{
    public class ActiveDirectoryWorkGroup
    {
        private List<ActiveDirectoryWorkGroup> groups;

        private List<ActiveDirectoryWorkGroup> members;

        public ActiveDirectoryWorkGroup(string mComputerFullName, string mObjectName)
        {
            this.mComputerFullName = mComputerFullName;
            this.mObjectName = mObjectName;
            RebuildPath();
        }

        public ActiveDirectoryWorkGroup(string directPath)
        {
            mPath = directPath;
        }

        private string mComputerFullName { get; set; }
        private string mObjectName { get; set; }
        private string mPath { get; set; }
        private string mUserName { get; set; }
        private SecureString mPassword { get; set; }
        public ActiveDirectoryEntry ObjectEntry { get; set; }

        public string ObjectType => ObjectEntry.SchemaClassName;

        public string ObjectSID => ObjectEntry.ObjectSID;

        public string Path => ObjectEntry.Path;

        public List<ActiveDirectoryWorkGroup> Groups
        {
            get
            {
                if (groups == null)
                {
                    groups = new List<ActiveDirectoryWorkGroup>();
                    var groupResults = (IEnumerable) ObjectEntry.Invoke("Groups");
                    foreach (var group in groupResults)
                    {
                        var groupEntry = new ActiveDirectoryEntry(group);
                        groups.Add(new ActiveDirectoryWorkGroup(groupEntry.Path));
                    }
                }

                return groups;
            }
        }

        public List<ActiveDirectoryWorkGroup> Members
        {
            get
            {
                if (members == null)
                {
                    members = new List<ActiveDirectoryWorkGroup>();

                    var memberResults = (IEnumerable) ObjectEntry.Invoke("Members");
                    foreach (var member in memberResults)
                    {
                        var memberEntry = new ActiveDirectoryEntry(member);
                        members.Add(new ActiveDirectoryWorkGroup(memberEntry.Path));
                    }
                }

                return members;
            }
        }

        private void RebuildPath()
        {
            mPath = string.Format("WinNT://{0}{1}",
                mComputerFullName,
                string.IsNullOrEmpty(mObjectName.Trim()) ? "" : string.Format("/{0}", mObjectName));
        }

        public ActiveDirectoryWorkGroup ChangeObject(string mObjectName)
        {
            this.mObjectName = mObjectName;
            Refresh();
            var p = Marshal.SecureStringToBSTR(mPassword);
            Logon(mUserName, Marshal.PtrToStringBSTR(p));
            Marshal.FreeBSTR(p);
            RebuildPath();
            return this;
        }

        /// <summary>
        ///     Need to logon again
        /// </summary>
        /// <param name="mComputerFullName"></param>
        /// <param name="mObjectName"></param>
        /// <returns></returns>
        public ActiveDirectoryWorkGroup Change(string mComputerFullName, string mObjectName)
        {
            this.mComputerFullName = mComputerFullName;
            this.mObjectName = mObjectName;
            RebuildPath();
            mUserName = null;
            mPassword = null;
            Refresh();
            return this;
        }

        public ActiveDirectoryWorkGroup Logon(string userName, string password)
        {
            mUserName = userName;
            mPassword = new SecureString();
            foreach (var c in password) mPassword.AppendChar(c);
            ObjectEntry =
                new ActiveDirectoryEntry(mPath,
                    userName,
                    password);

            return this;
        }

        public ActiveDirectoryWorkGroup Logon(string userName, SecureString password)
        {
            var p = Marshal.SecureStringToBSTR(password);
            Logon(mUserName, Marshal.PtrToStringBSTR(p));
            Marshal.FreeBSTR(p);
            return this;
        }

        public bool IsMemberOf(ActiveDirectoryWorkGroup obj)
        {
            var isIn = (bool) obj.ObjectEntry.Invoke("IsMember", Path);
            if (!isIn && obj.Members != null)
                foreach (var objMember in obj.Members)
                    return (bool) objMember
                        .Logon(obj.mUserName, obj.mPassword)
                        .ObjectEntry
                        .Invoke("IsMember", Path);
            return false;
        }

        public void Refresh()
        {
            groups = null;
            members = null;
        }
    }
}