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
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Common;

namespace AvePoint.Wrapper.Contract
{
    public abstract class AveRestoreOMFactory
    {
        internal const string RestoreAssemblyName = "AgentCommonWrapperRestore";
        internal const string RestoreTypeName = "AvePoint.Wrapper.Restore.AveWrapperRestoreOMFactory";

        public static AveRestoreOMFactory CreateRestoreOMFactory()
        {
            return AveAssemblyUtility.CreateInstance(RestoreAssemblyName, RestoreTypeName) as AveRestoreOMFactory;
        }

        public abstract IAveSPContentTypeCollection CreateAveSPContentTypeCollection(object obj);
        public abstract IAveSPFieldCollection CreateAveSPFieldCollection(object obj);
        public abstract IAveAudienceManager CreateAveAudienceManager(IAveSPSite aveSPSite);
        public abstract IAveDocumentTagging CreateAveDocumentTagging(string url, IAveSPSite aveSPSite);
        [Obsolete("Use followed method with MetaDataServiceOption")]
        public abstract IAveMetadataService CreateAveMetadataService(IAveSPSite aveSPSite);
        public abstract IAveMetadataService CreateAveMetadataService(IAveSPSite aveSPSite, MetaDataServiceOption mmsOption);
        public abstract IAveObjectFeature CreateAveObjectFeature(object obj);
        public abstract IAveObjectSecurity CreateAveObjectSecurity(object obj);
        public abstract IAveSPAlert CreateAveSPAlert(object obj);
        public abstract IAveSPAttachment CreateAttachment(IAveSPFolder parent, IAveSPFolder folder, string name);
        public abstract IAveSPAttachment CreateAttachment(IAveSPFolder parent, IAveSPListItem listItem, string name);
        public abstract IAveSPAttachment CreateAttachment(IAveSPFolder parent, string name);
        public abstract IAveSPAttachment CreateAttachment(IAveSPWeb aveWeb, IAveSPListItem aveSPItem, IAveRestoreStream aveRestoreStream);
        public abstract IAvePostAction CreateAvePostAction(IRestoreableObject param);
        public abstract IAveSPDoc CreateAveSPDoc(IAveSPFolder aveFolder, string name);
        public abstract IAveSPDoc CreateAveSPDoc(IAveSPSite aveSite);
        public abstract IAveSPEventReceiver CreateAveSPEventReceiver(object obj);
        public abstract IAveSPFolder CreateAveSPFolder(IAveSPFolder aveFolder, string name);
        public abstract IAveSPFolder CreateAveSPFolder(IAveSPList aveList, IAveRestoreStream restoreStream, string folderRelativeUrl, bool isRestoreFolder);
        public abstract IAveSPFolder CreateAveSPFolder(IAveSPList aveList, string name);
        public abstract IAveSPItem CreateAveSPItem(AveItemType type, IAveSPFolder parentFolder, string name);
        public abstract IAveSPItem CreateAveSPItem(IAveSPFolder parentFolder);
        public abstract IAveSPItem CreateAveSPItem(IAveSPList aveSPList, IAveRestoreStream aveRestoreStream);
        public abstract IAveSPItem CreateAveSPItem(IAveSPSite aveSite);
        public abstract IAveSPList CreateAveSPList(IAveSPWeb _AveWeb, string _name);
        public abstract IAveSPList CreateAveSPList(IAveSPWeb web, IAveRestoreStream restoreStream, AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig, string title);
        public abstract IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name);
        public abstract IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, int rowId);
        public abstract IAveSPMembers CreateAveSPMembers(IAveSPSite aveSPSite);
        public abstract IAveSPMySite CreateAveSPMySite(IAveSPSite aveSPSite);
        public abstract IAveSPNavigation CreateAveSPNavigation(IAveSPSite site);
        public abstract IAveSPNavigation CreateAveSPNavigation(IAveSPSite site, NavigationRestoreSetting setting);
        public abstract IAveSPNavigation CreateAveSPNavigation(IAveSPWeb web);
        public abstract IAveSPSearch CreateAveSPSearch(IAveSPSite aveSPSite);
        public abstract IAveSPSearch CreateAveSPSearch(IAveSPWeb aveSPWeb);
        public abstract IAveSPSearchKeywords CreateAveSPSearchKeywords(IAveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy);
        public abstract IAveSPSearchScope CreateAveSPSearchScope(IAveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy);
        public abstract IAveSPSearchScope CreateAveSPSearchScope(IAveSPWeb aveWeb, IAveOSearchServiceApplicationProxy searchServiceAppProxy);
        public abstract IAveSPSite CreateAveSPSite(string _url, string parentFullPath, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo);
        public abstract IAveSPSite CreateAveSPSite(string _url, string parentFullPath, AveSqlConnection _sqlConn, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo);
        public abstract IAveSPSocialComment CreateAveSPSocialComment(IAveSPSite aveSPSite);
        public abstract IAveSPSocialComment CreateAveSPSocialComment(string url, IAveSPSite aveSPSite);
        public abstract IAveSPSocialTag CreateAveSocialTag(IAveSPSite aveSPSite);
        public abstract IAveSPSocialTag CreateAveSocialTag(string url, IAveSPSite aveSPSite);
        public abstract IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite);
        public abstract IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, bool needInit);
        public abstract IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, string loginName);
        public abstract IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, string loginName, bool needInit);
        public abstract IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, uint destLCID);
        public abstract IAveSPUserProfile CreateAveSPUserProfile(IAveWebApplication webApp, uint destLCID, AveContextKind contextKind);
        public abstract IAveSPView CreateAveSPView(IAveSPList aveSPList);
        public abstract IAveSPWeb CreateAveSPWeb(IAveSPSite _AveSite, string _name);
        public abstract IAveSPWeb CreateAveSPWeb(IAveSPSite aveSite, IAveRestoreStream restoreStream, string webUrl);
        public abstract IAveSPWeb CreateAveSPWeb(IAveSPSite aveSite, IAveRestoreStream restoreStream, string webUrl, bool isRestoreWeb);
        public abstract IAveSPWeb CreateAveSPWeb(IAveSPSite aveSite, string webUrl, bool option);
        public abstract IAveSPWeb CreateAveSPWeb(IAveSPWeb aveWeb, IAveRestoreStream restoreStream);
        public abstract IAveSPWebAppPathManager CreateAveSPWebAppPathManager(IAveSPWebApp aveSPWebApp);
        public abstract IAveSPWebAppPolicyManager CreateAveSPWebAppPolicyManager(IAveSPWebApp aveSPWebApp);
        public abstract IAveSPWebAppPolicyRoleManager CreateAveSPWebAppPolicyRoleManager(IAveSPWebApp aveSPWebApp);
        public abstract IAveSPWebAppPropertyManager CreateAveSPWebAppPropertyManager(IAveSPWebApp aveWebApp, AveObjectModelFactory modelFactory);
        public abstract IWFConflictResolution CreateWFConflictResolution();
        public abstract IAveSPFeature CreateAveSPFeature(object obj);
        public abstract IAveSPAppManager CreateAppManager(IAveSPWeb web);
        public abstract IItemMetadata CreateItemMetadata(IAveSPItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> mItemUserData, List<Dictionary<string, object>> mItemJunctionData);
    }

}