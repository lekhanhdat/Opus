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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

using LS.SPWorkflowProcessor.SerializableObjects;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor
{
    public enum SPPermissionScope
    {
        Site,
        Web,
        List,
        Item,
    }

    public class PermissionProcParam
    {
        public IAveSecurableObject mParentObject;
    }


    public class SPPermissionLevelUnit
    {
        #region Serializable Data
        //public int mId;
        //public long mBasePermissionLng;
        //public string mName;
        //public string mDescription;
        //public string mXML;
        //public bool mHidden;

        private SPPermissionLevelSerializableData mSerializableData = null;
        public SPPermissionLevelSerializableData SerializableData
        {
            get
            {
                if (mSerializableData == null)
                    mSerializableData = new SPPermissionLevelSerializableData();
                return mSerializableData;
            }
        }
        #endregion

        public SPPermissionLevelUnit()
        { }

        public SPPermissionLevelUnit(SPPermissionLevelSerializableData data)
        {
            mSerializableData = data;
        }

        public void Dispose()
        { }

        public void SetPropertiesBySPObject(IAveRoleDefinition spDefinition)
        {
            this.SerializableData.mId = spDefinition.ID;
            this.SerializableData.mName = spDefinition.Name;
            this.SerializableData.mDescription = spDefinition.Description;
            this.SerializableData.mBasePermissionLng = (long)spDefinition.BasePermissions;
            this.SerializableData.mHidden = spDefinition.Hidden;
        }
    }


    public class SPPrincipalUnit
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #region Serializable Data
        public string mName;
        public string mDisplayName;
        public string mEmail;
        public string mNote;
        public string mWebTitle;
        public SPPrincipalType mPrincipalType;
        public SPPrincipalUnit mOwner;
        public List<SPPrincipalUnit> mUsers;
        #endregion


        public SPPrincipalUnit()
        {
            mName = string.Empty;
            mDisplayName = string.Empty;
            mEmail = string.Empty;
            mNote = string.Empty;
            mPrincipalType = SPPrincipalType.Invalid;
        }

        public void SetPropertiesBySPObject(IAvePrincipal spPrincipal)
        {

            if (spPrincipal is IAveUser)
            {
                mPrincipalType = SPPrincipalType.User;
                mName = ((IAveUser)spPrincipal).LoginName;
            }
            else if (spPrincipal is IAveGroup)
            {
                using (IAveWeb web = spPrincipal.ParentWeb)
                {
                    mPrincipalType = SPPrincipalType.Group;
                    IAveGroup group = (IAveGroup)spPrincipal;
                    mName = group.Name;
                    mWebTitle = spPrincipal.ParentWeb.Title;

                    #region Process Owner
                    IAvePrincipal ownerPrincipal = (IAvePrincipal)group.Owner;
                    if (ownerPrincipal.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        IAveUser ownerUser = null;
                        try
                        {
                            ownerUser = web.SiteUsers["SHAREPOINT\\system"];
                        }
                        catch(Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.FindSiteOwnerFaild, e.ToString());
                            ownerUser = web.CurrentUser;
                        }
                        mOwner = new SPPrincipalUnit();
                        mOwner.SetPropertiesBySPObject(ownerUser);
                    }
                    else
                    {
                        mOwner = new SPPrincipalUnit();
                        mOwner.SetPropertiesBySPObject(ownerPrincipal);
                    }
                    #endregion

                    #region Process Users
                    if (group.Users.Count > 0)
                    {
                        mUsers = new List<SPPrincipalUnit>();
                        foreach (IAveUser user in group.Users)
                        {
                            SPPrincipalUnit member = new SPPrincipalUnit();
                            member.SetPropertiesBySPObject(user);
                            mUsers.Add(member);
                        }
                    }
                    #endregion
                }
            }
            else
                mPrincipalType = SPPrincipalType.Invalid;
        }

        public void FixupWebTitle(string newWebTitle)
        {
            if (this.mPrincipalType == SPPrincipalType.Group)
            {
                if (mName.StartsWith(mWebTitle + " ", StringComparison.OrdinalIgnoreCase))
                    mName = newWebTitle + mName.Substring(mWebTitle.Length);
            }
        }


        internal SPPrincipalSerializableData ConvertToData()
        {
            SPPrincipalSerializableData data = new SPPrincipalSerializableData();

            data.mDisplayName = this.mDisplayName;
            data.mEmail = this.mEmail;
            data.mName = this.mName;
            data.mNote = this.mNote;
            if (this.mOwner != null)
                data.mOwner = this.mOwner.ConvertToData();
            data.mPrincipalType = this.mPrincipalType;
            if (this.mUsers != null)
            {
                data.mUsers = new List<SPPrincipalSerializableData>();
                foreach (SPPrincipalUnit unit in this.mUsers)
                    data.mUsers.Add(unit.ConvertToData());
            }
            data.mWebTitle = this.mWebTitle;

            return data;
        }

        internal static SPPrincipalUnit ConvertToObject(SPPrincipalSerializableData data)
        {
            if (data == null)
                return null;
            SPPrincipalUnit unit = new SPPrincipalUnit();

            unit.mDisplayName = data.mDisplayName;
            unit.mEmail = data.mEmail;
            unit.mName = data.mName;
            unit.mNote = data.mNote;
            unit.mOwner = SPPrincipalUnit.ConvertToObject(data.mOwner);
            unit.mPrincipalType = data.mPrincipalType;
            if (data.mUsers != null)
            {
                unit.mUsers = new List<SPPrincipalUnit>();
                foreach (SPPrincipalSerializableData d in data.mUsers)
                    unit.mUsers.Add(SPPrincipalUnit.ConvertToObject(d));
            }
            unit.mWebTitle = data.mWebTitle;

            return unit;
        }
    }


    public class SPRoleAssignmentUnit
    {
        #region Serializable Data
        private List<SPPermissionLevelUnit> mRoleDefinitionBindings;
        private SPPrincipalUnit mPrincipalUnit;
        #endregion

        public List<SPPermissionLevelUnit> RoleDefinitionUnitBindings
        {
            get { return mRoleDefinitionBindings; }
        }
        public SPPrincipalUnit PrincipalUnit
        {
            get { return mPrincipalUnit; }
        }

        public SPRoleAssignmentUnit()
        {
            mRoleDefinitionBindings = new List<SPPermissionLevelUnit>();
        }

        public void Dispose()
        {
            foreach (SPPermissionLevelUnit plUnit in mRoleDefinitionBindings)
                plUnit.Dispose();
            mRoleDefinitionBindings.Clear();
            mRoleDefinitionBindings = null;
        }

        public void SetPropertiesBySPOjbect(IAveRoleAssignment spAssignment)
        {
            mPrincipalUnit = new SPPrincipalUnit();
            mPrincipalUnit.SetPropertiesBySPObject(spAssignment.Member);
            foreach (IAveRoleDefinition rd in spAssignment.RoleDefinitionBindings)
            {
                SPPermissionLevelUnit permLevelUnit = new SPPermissionLevelUnit();
                permLevelUnit.SetPropertiesBySPObject(rd);
                this.RoleDefinitionUnitBindings.Add(permLevelUnit);
            }

        }

        internal SPRoleAssignmentSerializableData ConvertToData()
        {
            SPRoleAssignmentSerializableData data = new SPRoleAssignmentSerializableData();

            if (this.mPrincipalUnit != null)
                data.mPrincipalUnit = this.mPrincipalUnit.ConvertToData();
            if (this.mRoleDefinitionBindings != null)
            {
                data.mRoleDefinitionBindings = new List<SPPermissionLevelSerializableData>();
                foreach (SPPermissionLevelUnit unit in this.mRoleDefinitionBindings)
                    data.mRoleDefinitionBindings.Add(unit.SerializableData);
            }

            return data;
        }

        internal static SPRoleAssignmentUnit ConvertToObject(SPRoleAssignmentSerializableData data)
        {
            if (data == null)
                return null;
            SPRoleAssignmentUnit unit = new SPRoleAssignmentUnit();

            unit.mPrincipalUnit = SPPrincipalUnit.ConvertToObject(data.mPrincipalUnit);
            if (data.mRoleDefinitionBindings != null)
            {
                unit.mRoleDefinitionBindings = new List<SPPermissionLevelUnit>();
                foreach (SPPermissionLevelSerializableData d in data.mRoleDefinitionBindings)
                    unit.mRoleDefinitionBindings.Add(new SPPermissionLevelUnit(d));
            }

            return unit;
        }
    }

    public class SPPermissionUnit
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #region Serializable Data
        private bool mHasUniqueRoleAssignments;
        private List<SPRoleAssignmentUnit> mRoleAssignmentCollection;
        #endregion

        public bool HasUniqueRoleAssignments
        {
            get { return mHasUniqueRoleAssignments; }
            set { mHasUniqueRoleAssignments = value; }
        }
        public List<SPRoleAssignmentUnit> RoleAssignmentUnitCollection
        {
            get { return mRoleAssignmentCollection; }
        }

        public SPPermissionUnit()
        {
            mHasUniqueRoleAssignments = false;
            mRoleAssignmentCollection = new List<SPRoleAssignmentUnit>();
        }

        public void Dispose()
        {
            foreach (SPRoleAssignmentUnit raUnit in mRoleAssignmentCollection)
                raUnit.Dispose();
            mRoleAssignmentCollection.Clear();
            mRoleAssignmentCollection = null;
        }

        public static byte[] Save(SPPermissionUnit permUnit)
        {
            try
            {
                if (permUnit == null)
                    return null;
                SPPermissionSerializableData SerializableData = permUnit.ConvertToData();

                MemoryStream stream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, SerializableData);
                byte[] data = LSUtilityOfBytes.LSStreamToBytes(stream);

                #region Compress MetaData
                using (MemoryStream stream2 = new MemoryStream(data.Length))
                {
                    using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress, true))
                    {
                        stream3.Write(data, 0, data.Length);
                    }
                    data = stream2.GetBuffer();
                    Array.Resize<byte>(ref data, Convert.ToInt32(stream2.Length));
                }
                #endregion
                stream.Dispose();

                return data;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperWorkflowResource.PermissionUnitSaveError, e);
            }
            return null;
        }

        public static SPPermissionUnit Load(byte[] serializedMetadata)
        {
            try
            {
                byte[] decompressedData = new byte[0];

                #region Decompress serialized Metadata
                MemoryStream tempStream = new MemoryStream(serializedMetadata);
                tempStream.Position = 0L;
                using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
                {

                    byte[] temp = new byte[4096];
                    int readLen;
                    while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                    {
                        LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                    }
                }
                #endregion

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = new WorkflowSerializationBinder();
                MemoryStream stream = new MemoryStream(decompressedData);
                SPPermissionSerializableData SerializableData = (SPPermissionSerializableData)formatter.Deserialize(stream);
                stream.Dispose();
                return SPPermissionUnit.ConvertToObject(SerializableData);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperWorkflowResource.PermissionUnitLoadError, e);
            }
            return null;
        }

        internal SPPermissionSerializableData ConvertToData()
        {
            SPPermissionSerializableData data = new SPPermissionSerializableData();

            data.mHasUniqueRoleAssignments = this.mHasUniqueRoleAssignments;
            if (this.mRoleAssignmentCollection != null)
            {
                data.mRoleAssignmentCollection = new List<SPRoleAssignmentSerializableData>();
                foreach (SPRoleAssignmentUnit unit in this.mRoleAssignmentCollection)
                    data.mRoleAssignmentCollection.Add(unit.ConvertToData());
            }

            return data;
        }

        internal static SPPermissionUnit ConvertToObject(SPPermissionSerializableData data)
        {
            if (data == null)
                return null;
            SPPermissionUnit unit = new SPPermissionUnit();

            unit.mHasUniqueRoleAssignments = data.mHasUniqueRoleAssignments;
            if (data.mRoleAssignmentCollection != null)
            {
                unit.mRoleAssignmentCollection = new List<SPRoleAssignmentUnit>();
                foreach (SPRoleAssignmentSerializableData d in data.mRoleAssignmentCollection)
                    unit.mRoleAssignmentCollection.Add(SPRoleAssignmentUnit.ConvertToObject(d));
            }

            return unit;
        }

    }


    public class SPPermissionProcessor : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveSecurableObject mParentObject;
        private IAveWeb mParentWeb;
        private IAveWeb mFirstUniqueRoleDefinitionWeb;

        /// <summary>
        /// TODO 暂时控制不了，可能外围赋值
        /// </summary>
        private static Dictionary<string, string> mPermissionLevelMapping;
        public static Dictionary<string, string> PermissionLevelMapping
        {
            get
            {
                if (mPermissionLevelMapping == null)
                    mPermissionLevelMapping = new Dictionary<string, string>();
                return mPermissionLevelMapping;
            }
            set
            {
                mPermissionLevelMapping = value;
            }
        }

        protected List<SPWFProcessorException> mInnerWarnings;
        public List<SPWFProcessorException> InnerWarnings
        {
            get
            {
                if (mInnerWarnings == null)
                    mInnerWarnings = new List<SPWFProcessorException>();
                return mInnerWarnings;
            }
        }

        public static SPPermissionProcessor CreateInstance(SPPermissionScope scope)
        {
            SPPermissionProcessor instance = null;
            switch (scope)
            {
                case SPPermissionScope.Site:
                case SPPermissionScope.Web:
                case SPPermissionScope.List:
                    break;
                case SPPermissionScope.Item:
                    instance = new SPItemPermProcessor();
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionScopeNotSupportedException);
            }
            return instance;
        }

        public static SPPermissionProcessor CreateInstance(SPPermissionScope scope, IAveSecurableObject parent)
        {
            SPPermissionProcessor instance = null;
            switch (scope)
            {
                case SPPermissionScope.Site:
                case SPPermissionScope.Web:
                case SPPermissionScope.List:
                    break;
                case SPPermissionScope.Item:
                    instance = new SPItemPermProcessor();
                    break;
                default:
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionScopeNotSupportedException);
            }
            if (instance != null)
            {
                PermissionProcParam param = new PermissionProcParam();
                param.mParentObject = parent;
                instance.SetProcParameters(param);
            }
            return instance;
        }

        public void Dispose()
        {
            if (mParentWeb != null)
                mParentWeb.Dispose();
            if (mFirstUniqueRoleDefinitionWeb != null)
                mFirstUniqueRoleDefinitionWeb.Dispose();
        }

        public virtual void SetProcParameters(PermissionProcParam param)
        {
            if (param.mParentObject == null)
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionParentIsNullException);

            mParentObject = param.mParentObject;
            if (mParentObject is IAveWeb)
            {
                mParentWeb = (IAveWeb)mParentObject;
            }
            else if (mParentObject is IAveList)
            {
                mParentWeb = ((IAveList)mParentObject).ParentWeb;
            }
            else if (mParentObject is IAveListItem)
            {
                mParentWeb = ((IAveListItem)mParentObject).Web;
            }
            else
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionParentInvalidException);
            }
            mFirstUniqueRoleDefinitionWeb = mParentWeb.FirstUniqueRoleDefinitionWeb;
        }

        public virtual byte[] Backup()
        {
            SPPermissionUnit permUnit = BackupWithoutSerialization();
            byte[] serializedMetadata = SPPermissionUnit.Save(permUnit);
            permUnit.Dispose();
            return serializedMetadata;
        }

        public virtual void Restore(byte[] serializedMetadata)
        {
            SPPermissionUnit permUnit = SPPermissionUnit.Load(serializedMetadata);
            RestorePermissionUnit(permUnit);

        }

        public SPPermissionUnit BackupWithoutSerialization()
        {
            SPPermissionUnit permUnit = null;
            try
            {
                if (mParentObject == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionParentIsNullException);

                permUnit = new SPPermissionUnit();
                if (mParentObject.HasUniqueRoleAssignments)
                {
                    permUnit.HasUniqueRoleAssignments = true;
                    foreach (IAveRoleAssignment ra in mParentObject.RoleAssignments)
                    {
                        try
                        {
                            SPRoleAssignmentUnit raUnit = new SPRoleAssignmentUnit();
                            raUnit.SetPropertiesBySPOjbect(ra);
                            permUnit.RoleAssignmentUnitCollection.Add(raUnit);
                        }
                        catch (SPWFProcessorException procException)
                        {
                            log.Log(AveLogLevel.DEBUG, "An processor error occurred while backup without serialization, error message: {0}.", procException);
                            InnerWarnings.Add(procException);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, "An error occurred while backup without serialization, error message: {0}.", e);
                            InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.PermissionBackupUnknownWarning, e));
                        }
                    }
                }
                return permUnit;
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.permissionBackupUnknownException, e);
            }
        }

        public void RestorePermissionUnit(SPPermissionUnit permUnit)
        {
            try
            {
                if (mParentObject == null)
                    throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionParentIsNullException);
                if (!permUnit.HasUniqueRoleAssignments)
                    return;
                if (!mParentObject.HasUniqueRoleAssignments)
                {
                    mParentObject.BreakRoleInheritance(false);
                }
                List<IAvePrincipal> remainRAs = new List<IAvePrincipal>();
                foreach (IAveRoleAssignment ra in mParentObject.RoleAssignments)
                    remainRAs.Add(ra.Member);
                for (int i = 0; i < remainRAs.Count; i++)
                    mParentObject.RoleAssignments.RemoveById(remainRAs[i].ID);

                foreach (SPRoleAssignmentUnit raUnit in permUnit.RoleAssignmentUnitCollection)
                {
                    try
                    {
                        IAvePrincipal principal = null;
                        try
                        {
                            principal = GetMappingPrincipal(raUnit.PrincipalUnit);
                        }
                        catch (Exception e)
                        {
                            throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionRestorePrincipalIsNullWarning, e);
                        }

                        IAveRoleAssignment ra = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateRoleAssignment(principal);
                        foreach (SPPermissionLevelUnit permLevelUnit in raUnit.RoleDefinitionUnitBindings)
                        {
                            IAveRoleDefinition rd = null;
                            try
                            {
                                rd = GetMappingPermissionLevel(permLevelUnit);
                            }
                            catch (Exception e)
                            {
                                throw new SPWFProcessorException(SPWFProcessorErrorCode.PermissionRestoreRoleDefinitionIsNullWarning, e);
                            }

                            if (rd.Type == AveRoleType.Guest)
                            {
                                InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.PermissionRestoreCannotGrantLimitedAccessWarning, null, AveSPResource.GetString("CannotAddUserToGuests", new object[0])));
                                continue;
                            }
                            ra.RoleDefinitionBindings.Add(rd);
                        }
                        if (ra.RoleDefinitionBindings.Count == 0)
                            continue;
                        mParentObject.RoleAssignments.Add(ra);
                    }
                    catch (SPWFProcessorException procException)
                    {
                        log.Log(AveLogLevel.DEBUG, "An processor error occurred while restoring permission unit, error message: {0}", procException);
                        InnerWarnings.Add(procException);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, "An error occurred while restoring permission unit, error message: {0}", e);
                        InnerWarnings.Add(new SPWFProcessorException(SPWFProcessorErrorCode.PermissionRestoreUnknownWarning, e));
                    }
                }
            }
            catch (SPWFProcessorException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SPWFProcessorException(SPWFProcessorErrorCode.permissionRestoreUnknownException, e);
            }
        }


        private IAvePrincipal GetMappingPrincipal(SPPrincipalUnit principalUnit)
        {
            string key = principalUnit.mName.ToLower(CultureInfo.CurrentCulture);

            principalUnit.FixupWebTitle(mParentWeb.Title);
            IAvePrincipal principal = null;
            if (principalUnit.mPrincipalType == SPPrincipalType.User)
            {
                return GetOrCreateUser(principalUnit.mName);
            }
            else if (principalUnit.mPrincipalType == SPPrincipalType.Group)
            {
                #region Principal is Group
                try
                {
                    principal = mParentWeb.SiteGroups[principalUnit.mName];
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetPricipalFromWebError, e.ToString());
                }//need not log
                if (principal == null)
                {
                    IAveMember owner = GetMappingPrincipal(principalUnit.mOwner);
                    if (owner == null)
                        return null;
                    try
                    {
                        mParentWeb.SiteGroups.Add(principalUnit.mName, owner, mParentWeb.CurrentUser, "");
                        principal = mParentWeb.SiteGroups[principalUnit.mName];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetPricipalFromWebError, e.ToString());
                    }//need not log
                }

                if (principal != null && principalUnit.mUsers != null && principalUnit.mUsers.Count > 0)
                {
                    IAveGroup group = (IAveGroup)principal;
                    foreach (SPPrincipalUnit unit in principalUnit.mUsers)
                    {
                        object memberObj = GetMappingPrincipal(unit);
                        if (memberObj != null)
                            group.AddUser((IAveUser)memberObj);
                    }
                }
                return principal;
                #endregion
            }
            else
                return null;
        }

        private IAveRoleDefinition GetMappingPermissionLevel(SPPermissionLevelUnit plUnit)
        {
            IAveRoleDefinition roleDefinition = null;
            string key = plUnit.SerializableData.mName.ToLower(CultureInfo.CurrentCulture);
            if (PermissionLevelMapping.ContainsKey(key))
            {
                plUnit.SerializableData.mName = PermissionLevelMapping[key];
            }

            int num = 0;
            IAveRoleDefinitionCollection rdCollection = mFirstUniqueRoleDefinitionWeb.RoleDefinitions;
            CultureInfo locale = mFirstUniqueRoleDefinitionWeb.Locale;
            while (num < rdCollection.Count)
            {
                if (string.Compare(rdCollection[num].Name, plUnit.SerializableData.mName, true, locale) != 0)
                {
                    num++;
                    continue;
                }
                roleDefinition = rdCollection[num];
                break;
            }

            if (roleDefinition == null)
            {
                roleDefinition = SPWorkflowProcessorRuntime.ObjectModelFactory.CreateRoleDefinition();
                roleDefinition.Name = plUnit.SerializableData.mName;
                roleDefinition.Description = plUnit.SerializableData.mDescription;
                roleDefinition.BasePermissions = (AveBasePermissions)plUnit.SerializableData.mBasePermissionLng;
                rdCollection.Add(roleDefinition);
                roleDefinition = rdCollection[roleDefinition.Name];
            }

            return roleDefinition;
        }

        public static IAveUser GetOrCreateUser(string loginName)
        {
            string displayName = string.Empty;
            return GetOrCreateUser(loginName, out displayName);
        }

        public static IAveUser GetOrCreateUser(string loginName, out string displayName)
        {
            displayName = string.Empty;
            IAveUser user = SPWorkflowProcessorRuntime.OnUserMapping(loginName);
            if (user != null)
                displayName = user.Name;
            return user;
        }

        public static IAvePrincipal GetOrCreateMember(string loginName)
        {
            IAvePrincipal member = SPWorkflowProcessorRuntime.OnMemberMapping(loginName);
            return member;
        }

        public static string GetUserOrGroupLoginNameFromId(IAveWeb parentWeb, int id)
        {
            string loginName = string.Empty;
            try
            {
                IAveWeb web = parentWeb;
                {
                    IAveUser user = null;
                    try
                    {
                        user = parentWeb.SiteUsers.GetByID(id);
                    }
                    catch (Exception e)
                    {
                        ;
                        log.Log(AveLogLevel.DEBUG, string.Format("GetUserLoginNameFromId:{0}", WrapperWorkflowResource.GetLoginNameByIdError), e);
                    }

                    if (user != null)
                    {
                        loginName = user.LoginName;
                    }
                    else
                    {
                        try
                        {
                            var group = parentWeb.SiteGroups.GetByID(id);
                            if (group == null)
                            {
                                log.Warn("Cannot find workflow associated user or group by id {0}", id);
                            }
                            else
                            {
                                loginName = group.Name;
                            }
                        }
                        catch (Exception e)//need not log
                        {
                            log.Log(AveLogLevel.DEBUG, string.Format("GetGroupLoginNameFromId:{0}", WrapperWorkflowResource.GetLoginNameByIdError), e);
                        }
                    }
                }
            }
            catch (Exception e)//need not log
            {
                log.Debug("An error occurred while getting login name by id:{0}.Message:{1}", id, e);
            }
            return loginName;
        }

        public static string OnModifyLogin(object sender, string login)
        {
            IAveUser user = SPWorkflowProcessorRuntime.OnUserMapping(login);
            if (user != null)
            {
                if (sender != null && sender is List<string>)
                {
                    List<string> temp = (List<string>)sender;
                    temp.Clear();
                    temp.Add(user.ID.ToString());
                    temp.Add(user.Name);
                    temp.Add(user.Notes);
                    temp.Add(user.Email);
                }
                return user.LoginName;
            }
            return login;


            //string newLogin = login;
            //string domain = string.Empty;
            //string sPreFBA = string.Empty;
            //string preFBA = string.Empty;
            //if (login.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase)
            //    || login.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase)
            //    || login.Equals("NT AUTHORITY\\local service", StringComparison.OrdinalIgnoreCase))
            //{
            //    return login;
            //}
            //if (login.IndexOf('|') > 0)
            //{
            //    sPreFBA = login.Substring(0, login.IndexOf('|') + 1);
            //    if (!PreFBAs.Contains(sPreFBA))
            //    {
            //        return login;
            //    }
            //    newLogin = login.Substring(login.IndexOf('|') + 1);
            //}


            //if (newLogin.IndexOf('\\') > 0)
            //{
            //    domain = newLogin.Substring(0, newLogin.IndexOf('\\'));
            //}
            //else if (newLogin.IndexOf('|') > 0)
            //{
            //    domain = newLogin.Substring(0, newLogin.IndexOf('|'));
            //}

            //if (!string.IsNullOrEmpty(domain))
            //{
            //    domain = domain.ToLower();
            //    newLogin = newLogin.ToLower();
            //    if (SPPrincipalNameMapping.ContainsKey(newLogin))
            //    {
            //        newLogin = SPPrincipalNameMapping[newLogin];
            //    }
            //    else if (SPPrincipalDomainMapping.ContainsKey(domain))
            //    {
            //        newLogin = newLogin.Replace(domain, SPPrincipalDomainMapping[domain]);
            //    }
            //}

            //return newLogin;
        }

        //private static string[] PreFBAs = new string[] { "i:0#.w|", "i:0#.f|", "c:0-.f|" };
        //private static string GetMappingUserLogin(SPWebApplication webApp, string login, bool needMapping)
        //{
        //    string newLogin = login;
        //    string domain = string.Empty;
        //    string sPreFBA = string.Empty;
        //    string preFBA = string.Empty;
        //    if (login.Equals("SHAREPOINT\\system", StringComparison.OrdinalIgnoreCase)
        //        || login.Equals("NT AUTHORITY\\authenticated users", StringComparison.OrdinalIgnoreCase)
        //        || login.Equals("NT AUTHORITY\\local service", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return login;
        //    }
        //    if (login.IndexOf('|') > 0)
        //    {
        //        sPreFBA = login.Substring(0, login.IndexOf('|') + 1);
        //        if (!PreFBAs.Contains(sPreFBA))
        //        {
        //            return login;
        //        }
        //        newLogin = login.Substring(login.IndexOf('|') + 1);
        //    }


        //    if (needMapping)
        //    {
        //        if (newLogin.IndexOf('\\') > 0)
        //        {
        //            domain = newLogin.Substring(0, newLogin.IndexOf('\\'));
        //        }
        //        else if (newLogin.IndexOf('|') > 0)
        //        {
        //            domain = newLogin.Substring(0, newLogin.IndexOf('|'));
        //        }

        //        domain = domain.ToLower();
        //        newLogin = newLogin.ToLower();
        //        if (SPPrincipalNameMapping.ContainsKey(newLogin))
        //        {
        //            newLogin = SPPrincipalNameMapping[newLogin];
        //        }
        //        else if (SPPrincipalDomainMapping.ContainsKey(domain))
        //        {
        //            newLogin = newLogin.Replace(domain, SPPrincipalDomainMapping[domain]);
        //        }
        //    }

        //    if (webApp.IisSettings[SPUrlZone.Default].AuthenticationMode == System.Web.Configuration.AuthenticationMode.Forms)
        //    {
        //        if (newLogin.IndexOf('\\') > 0)
        //        {
        //            preFBA = PreFBAs[0];
        //        }
        //        else if (newLogin.IndexOf('|') > 0)
        //        {
        //            preFBA = PreFBAs[1];
        //            if (sPreFBA != string.Empty && sPreFBA.Equals(PreFBAs[2]))
        //            {
        //                preFBA = sPreFBA;
        //            }
        //        }
        //        newLogin = preFBA + newLogin;
        //    }
        //    return newLogin;
        //}
    }

    public class SPItemPermProcessor : SPPermissionProcessor
    {
        private IAveListItem mParentItem = null;

        public IAveListItem ParentItem
        {
            get
            {
                if (mParentItem == null)
                    throw new NullReferenceException();
                return mParentItem;
            }
        }

        public override void SetProcParameters(PermissionProcParam param)
        {
            if (param.mParentObject == null)
                throw new NullReferenceException();
            mParentItem = (IAveListItem)param.mParentObject;
            base.SetProcParameters(param);
        }

        public override byte[] Backup()
        {
            return base.Backup();
        }

        public override void Restore(byte[] serializedMetadata)
        {
            base.Restore(serializedMetadata);
        }
    }

    public class SPListPermProcessor : SPPermissionProcessor
    { }

    public class SPWebPermProcessor : SPPermissionProcessor
    { }

    public class SPSitePermProcessor : SPPermissionProcessor
    { }
}
