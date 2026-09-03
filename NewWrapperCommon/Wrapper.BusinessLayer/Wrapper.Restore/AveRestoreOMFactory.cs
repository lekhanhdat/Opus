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
using AvePoint.Wrapper.Common.Office;

namespace AvePoint.Wrapper.Restore
{
    public class AveWrapperRestoreOMFactory : AvePoint.Wrapper.Contract.AveRestoreOMFactory
    {
        public override IAveSPContentTypeCollection CreateAveSPContentTypeCollection(object obj)
        {
            return AveSPContentTypeCollection.CreateInstance(obj);
        }

        public override IAveSPFieldCollection CreateAveSPFieldCollection(object obj)
        {
            return AveSPFieldCollection.CreateInstance(obj);
        }

        public override IAveAudienceManager CreateAveAudienceManager(IAveSPSite aveSPSite)
        {
            return new AveAudienceManager(aveSPSite as AveSPSite);
        }

        public override IAveDocumentTagging CreateAveDocumentTagging(string url, IAveSPSite aveSPSite)
        {
            return new AveDocumentTagging(url, aveSPSite as AveSPSite);
        }

        [Obsolete("Use followed method with MetaDataServiceOption")]
        public override IAveMetadataService CreateAveMetadataService(IAveSPSite aveSPSite)
        {
            return new AveMetadataService(aveSPSite as AveSPSite);
        }

        public override IAveMetadataService CreateAveMetadataService(IAveSPSite aveSPSite, MetaDataServiceOption mmsOption)
        {
            return new AveMetadataService(aveSPSite as AveSPSite, mmsOption);
        }

        public override IAveObjectFeature CreateAveObjectFeature(object obj)
        {
            return AveObjectFeature.CreateInstance(obj);
        }

        public override IAveObjectSecurity CreateAveObjectSecurity(object obj)
        {
            return AveObjectSecurity.CreateInstance(obj);
        }

        public override IAveSPAlert CreateAveSPAlert(object obj)
        {
            return AveSPAlert.CreateInstance(obj);
        }

        public override IAveSPAttachment CreateAttachment(IAveSPFolder parent, IAveSPFolder folder, string name)
        {
            return new AveSPAttachment(parent as AveSPFolder, folder as AveSPFolder, name);
        }

        public override IAveSPAttachment CreateAttachment(IAveSPFolder parent, IAveSPListItem listItem, string name)
        {
            return new AveSPAttachment(parent as AveSPFolder, listItem as AveSPListItem, name);
        }

        public override IAveSPAttachment CreateAttachment(IAveSPFolder parent, string name)
        {
            return new AveSPAttachment(parent as AveSPFolder, name);
        }

        public override IAveSPAttachment CreateAttachment(IAveSPWeb aveWeb, IAveSPListItem aveSPItem, IAveRestoreStream aveRestoreStream)
        {
            return new AveSPAttachment(aveWeb as AveSPWeb, aveSPItem as AveSPListItem, aveRestoreStream);
        }

        public override IAveSPDoc CreateAveSPDoc(IAveSPFolder aveFolder, string name)
        {
            return new AveSPDoc(aveFolder as AveSPFolder, name);
        }

        public override IAveSPDoc CreateAveSPDoc(IAveSPSite aveSite)
        {
            return new AveSPDoc(aveSite as AveSPSite);
        }

        public override IAveSPEventReceiver CreateAveSPEventReceiver(object obj)
        {
            return AveSPEventReceiver.CreateInstance(obj);
        }

        public override IAveSPFolder CreateAveSPFolder(IAveSPFolder aveFolder, string name)
        {
            return new AveSPFolder(aveFolder as AveSPFolder, name);
        }

        public override IAveSPFolder CreateAveSPFolder(IAveSPList aveList, IAveRestoreStream restoreStream, string folderRelativeUrl, bool isRestoreFolder)
        {
            return new AveSPFolder(aveList as AveSPList, restoreStream, folderRelativeUrl, isRestoreFolder);
        }

        public override IAveSPFolder CreateAveSPFolder(IAveSPList aveList, string name)
        {
            return new AveSPFolder(aveList as AveSPList, name);
        }

        public override IAveSPItem CreateAveSPItem(AveItemType type, IAveSPFolder parentFolder, string name)
        {
            return new AveSPItem(type, parentFolder as AveSPFolder, name);
        }

        public override IAveSPItem CreateAveSPItem(IAveSPFolder parentFolder)
        {
            return new AveSPItem(parentFolder as AveSPFolder);
        }

        public override IAveSPItem CreateAveSPItem(IAveSPList aveSPList, IAveRestoreStream aveRestoreStream)
        {
            return new AveSPItem(aveSPList as AveSPList, aveRestoreStream);
        }

        public override IAveSPItem CreateAveSPItem(IAveSPSite aveSite)
        {
            return new AveSPItem(aveSite as AveSPSite);
        }

        public override IAveSPList CreateAveSPList(IAveSPWeb _AveWeb, string _name)
        {
            return new AveSPList(_AveWeb as AveSPWeb, _name);
        }

        public override IAveSPList CreateAveSPList(IAveSPWeb web, IAveRestoreStream restoreStream, AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig, string title)
        {
            return new AveSPList(web as AveSPWeb, restoreStream, securityMapping, restoreConfig, title);
        }

        public override IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name)
        {
            return new AveSPListItem(aveFolder as AveSPFolder, name);
        }

        public override IAveSPListItem CreateAveSPListItem(IAveSPFolder aveFolder, string name, int rowId)
        {
            return new AveSPListItem(aveFolder as AveSPFolder, name, rowId);
        }

        public override IAveSPMembers CreateAveSPMembers(IAveSPSite aveSPSite)
        {
            if (aveSPSite.SPSite.IsOnlineSite)
            {
                return new AveSPMembersMultiThread(aveSPSite as AveSPSite);
            }
            else
            {
                return new AveSPMembers(aveSPSite as AveSPSite);
            }
        }
        
        public override IAveSPMySite CreateAveSPMySite(IAveSPSite aveSPSite)
        {
            return new AveSPMySite(aveSPSite as AveSPSite);
        }

        public override IAveSPNavigation CreateAveSPNavigation(IAveSPSite site)
        {
            return new AveSPNavigation(site as AveSPSite);
        }

        public override IAveSPNavigation CreateAveSPNavigation(IAveSPSite site, NavigationRestoreSetting setting)
        {
            return new AveSPNavigation(site as AveSPSite, setting);
        }

        public override IAveSPSearch CreateAveSPSearch(IAveSPSite aveSPSite)
        {
            return new AveSPSearch(aveSPSite as AveSPSite);
        }

        public override IAveSPSearch CreateAveSPSearch(IAveSPWeb aveSPWeb)
        {
            return new AveSPSearch(aveSPWeb as AveSPWeb);
        }

        public override IAveSPSearchKeywords CreateAveSPSearchKeywords(IAveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            return new AveSPSearchKeywords(aveSite as AveSPSite, searchServiceAppProxy);
        }

        public override IAveSPSearchScope CreateAveSPSearchScope(IAveSPSite aveSite, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            return new AveSPSearchScope(aveSite as AveSPSite, searchServiceAppProxy);
        }

        public override IAveSPSearchScope CreateAveSPSearchScope(IAveSPWeb aveWeb, IAveOSearchServiceApplicationProxy searchServiceAppProxy)
        {
            return new AveSPSearchScope(aveWeb as AveSPWeb, searchServiceAppProxy);
        }

        public override IAveSPSite CreateAveSPSite(string _url, string parentFullPath, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            return new AveSPSite(_url, parentFullPath, contextKind, aveUserAccountInfo);
        }

        public override IAveSPSocialComment CreateAveSPSocialComment(IAveSPSite aveSPSite)
        {
            return new AveSPSocialComment(aveSPSite as AveSPSite);
        }

        public override IAveSPSocialComment CreateAveSPSocialComment(string url, IAveSPSite aveSPSite)
        {
            return new AveSPSocialComment(url, aveSPSite as AveSPSite);
        }

        public override IAveSPSocialTag CreateAveSocialTag(IAveSPSite aveSPSite)
        {
            return new AveSPSocialTag(aveSPSite as AveSPSite);
        }

        public override IAveSPSocialTag CreateAveSocialTag(string url, IAveSPSite aveSPSite)
        {
            return new AveSPSocialTag(url, aveSPSite as AveSPSite);
        }

        public override IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite)
        {
            return new AveSPUserProfile(_aveSite as AveSPSite);
        }

        public override IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, bool needInit)
        {
            return new AveSPUserProfile(_aveSite as AveSPSite, needInit);
        }

        public override IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, string loginName)
        {
            return new AveSPUserProfile(_aveSite as AveSPSite, loginName);
        }

        public override IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, string loginName, bool needInit)
        {
            return new AveSPUserProfile(_aveSite as AveSPSite, loginName, needInit);
        }

        public override IAveSPUserProfile CreateAveSPUserProfile(IAveSPSite _aveSite, uint destLCID)
        {
            return new AveSPUserProfile(_aveSite as AveSPSite, destLCID);
        }

        public override IAveSPUserProfile CreateAveSPUserProfile(IAveWebApplication webApp, uint destLCID, AveContextKind contextKind)
        {
            return new AveSPUserProfile(webApp, destLCID, contextKind);
        }

        public override IAveSPView CreateAveSPView(IAveSPList aveSPList)
        {
            return new AveSPView(aveSPList as AveSPList);
        }

        public override IAveSPWeb CreateAveSPWeb(IAveSPSite _AveSite, string _name)
        {
            return new AveSPWeb(_AveSite as AveSPSite, _name);
        }

        public override IAveSPWeb CreateAveSPWeb(IAveSPSite aveSite, IAveRestoreStream restoreStream, string webUrl)
        {
            return new AveSPWeb(aveSite as AveSPSite, restoreStream, webUrl);
        }

        public override IAveSPWeb CreateAveSPWeb(IAveSPSite aveSite, IAveRestoreStream restoreStream, string webUrl, bool isRestoreWeb)
        {
            return new AveSPWeb(aveSite as AveSPSite, restoreStream, webUrl, isRestoreWeb);
        }

        public override IAveSPWeb CreateAveSPWeb(IAveSPSite aveSite, string webUrl, bool option)
        {
            return new AveSPWeb(aveSite as AveSPSite, webUrl, option);
        }

        public override IAveSPWeb CreateAveSPWeb(IAveSPWeb aveWeb, IAveRestoreStream restoreStream)
        {
            return new AveSPWeb(aveWeb as AveSPWeb, restoreStream);
        }

        public override IAveSPWebAppPathManager CreateAveSPWebAppPathManager(IAveSPWebApp aveSPWebApp)
        {
            return new AveSPWebAppPathManager(aveSPWebApp as AveSPWebApp);
        }

        public override IAveSPWebAppPolicyManager CreateAveSPWebAppPolicyManager(IAveSPWebApp aveSPWebApp)
        {
            return new AveSPWebAppPolicyManager(aveSPWebApp as AveSPWebApp);
        }

        public override IAveSPWebAppPolicyRoleManager CreateAveSPWebAppPolicyRoleManager(IAveSPWebApp aveSPWebApp)
        {
            return new AveSPWebAppPolicyRoleManager(aveSPWebApp as AveSPWebApp);
        }

        public override IAveSPWebAppPropertyManager CreateAveSPWebAppPropertyManager(IAveSPWebApp aveWebApp, AveObjectModelFactory modelFactory)
        {
            return new AveSPWebAppPropertyManager(aveWebApp as AveSPWebApp, modelFactory);
        }

        public override IAveSPNavigation CreateAveSPNavigation(IAveSPWeb web)
        {
            return new AveSPNavigation(web as AveSPWeb);
        }

        public override IAveSPSite CreateAveSPSite(string _url, string parentFullPath, AvePoint.Common.AveSqlConnection _sqlConn, AveContextKind contextKind, AveBPOSAccountInfo aveUserAccountInfo)
        {
            return new AveSPSite(_url, parentFullPath, _sqlConn, contextKind, aveUserAccountInfo);
        }
        

        public override IAvePostAction CreateAvePostAction(IRestoreableObject param)
        {
            var typeName = param.GetType().FullName;
            return AveAssemblyUtility.CreateInstance(System.Reflection.Assembly.GetExecutingAssembly(),typeName + "PostAction", new Type[] { typeof(IRestoreableObject) }, new object[] { param }) as IAvePostAction;
        }

        public override IWFConflictResolution CreateWFConflictResolution()
        {
            return WFConflictResolution.Instance;
        }

        public override IAveSPFeature CreateAveSPFeature(object obj)
        {
            return new AveSPFeature(obj);
        }

        public override IAveSPAppManager CreateAppManager(IAveSPWeb web)
        {
            return new AveSPAppManager(web as AveSPWeb);
        }

        public override IItemMetadata CreateItemMetadata(IAveSPItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> mItemUserData, List<Dictionary<string, object>> mItemJunctionData)
        {
            return new ItemMetadata(mItem as AveSPItem, originalVersion, originalRowId, mItemUserData, mItemJunctionData);
        }
    }
}