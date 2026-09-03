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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class Ave2010ListFlags
    {
        private const ulong allowContentTypes = (ulong)0x2000000000L;
        private const ulong allowDeletion = (ulong)4L;
        private const ulong contentTypesEnabled = (ulong)0x400000L;
        private const ulong enableAssignToEmail = (ulong)0x40L;
        private const ulong enableAttachments = (ulong)8L;
        private const ulong enableDeployWithDependentList = (ulong)0x8000000L;
        private const ulong enableFolderCreation = (ulong)0x20000000L;
        private const ulong enableVersioning = (ulong)0x80L;
        private const ulong enableMinorVersions = (ulong)0x80000L;
        private const ulong enableModeration = (ulong)0x400L;
        private const ulong enablePeopleSelector = (ulong)0x80000000000L;
        private const ulong enableResourceSelector = (ulong)0x200000000000L;
        private const ulong enableSchemaCaching = (ulong)0x200000000L;
        private const ulong enforceDataValidation = (ulong)0x4000000000000L;
        private const ulong hidden = (ulong)0x100L;
        private const ulong ordered = (ulong)1L;
        private const ulong hasExternalDataSource = (ulong)0x400000000000L;
        private const ulong noCrawl = (ulong)0x800000000L;
        private const ulong irmEnabled = (ulong)0x8000000000L;
        private const ulong irmExpire = (ulong)0x10000000000L;
        private const ulong irmReject = (ulong)0x20000000000L;
        private const ulong enableSyndication = (ulong)0x4000000000L;
        private const ulong excludeFromOfflineClient = (ulong)0x2000000000000L;
        private const ulong excludeFromTemplate = (ulong)0x2000L;
        private const ulong draftVersionVisibility_Author = (ulong)0x100000L;
        private const ulong draftVersionVisibility_Approver = (ulong)0x200000L;
        private const ulong forceCheckout = (ulong)0x40000L;
        private const ulong multipleDataList = (ulong)0x20L;
        private const ulong allowEveryoneViewItems = (ulong)0x2000000L;
        private const ulong allowMultiResponses = (ulong)0x800L;
        private const ulong browserFileHandling = (ulong)0x40000000000000L;
        private const ulong calculationOptions_PreserveEmptyValue = (ulong)0x800000000000L;
        private const ulong calculationOptions_StrictTypeCoercion = (ulong)0x100000000000000L;
        private const ulong defaultItemOpen_A = (ulong)0x8000000000000L;
        private const ulong defaultItemOpen_B = (ulong)0x10000000L;
        private const ulong defaultItemOpenUseListSetting = (ulong)0x8000000000000L;
        private const ulong disableGridEditing = (ulong)0x20000000000000L;
        private const ulong isApplicationList = (ulong)0x10000000000000L;
        private const ulong requestAccessEnabled = (ulong)0x200L;
        private const ulong restrictedTemplateList = (ulong)0x40000000L;
        private const ulong rootWebOnly = (ulong)0x4000L;
        private const ulong showUser = (ulong)0x1000L;
        private const ulong navigateForFormsPages = (ulong)0x80000000000000L;
        private const ulong workflowsAssociated = (ulong)0x4000000L;

        private const ulong crawlNonDefaultViews = (ulong)2L;

        public static bool AllowContentTypes(ulong mFlags, AveListTemplateType baseTemplate)
        {
            if (((((baseTemplate == AveListTemplateType.Survey) ||
                (baseTemplate == AveListTemplateType.ListTemplateCatalog)) ||
                ((baseTemplate == AveListTemplateType.SolutionCatalog) ||
                (baseTemplate == AveListTemplateType.WebPartCatalog))) ||
                (((baseTemplate == AveListTemplateType.WebTemplateCatalog) ||
                (baseTemplate == AveListTemplateType.Meetings)) ||
                ((baseTemplate == AveListTemplateType.MeetingObjective) ||
                (baseTemplate == AveListTemplateType.MeetingUser)))) ||
                ((((baseTemplate == AveListTemplateType.Agenda) ||
                (baseTemplate == AveListTemplateType.TextBox)) ||
                ((baseTemplate == AveListTemplateType.Decision) ||
                (baseTemplate == AveListTemplateType.ThingsToBring))) ||
                ((baseTemplate == AveListTemplateType.HomePageLibrary) ||
                 HasExternalDataSource(mFlags))))
            {
                return false;
            }
            return ((mFlags & allowContentTypes) == 0L);
        }

        public static bool AllowDeletion(ulong mFlags)
        {
            return ((mFlags & allowDeletion) == 0L);
        }
        public static bool ContentTypesEnabled(ulong mFlags)
        {
            return ((mFlags & contentTypesEnabled) != 0L);
        }
        public static bool EnableAssignToEmail(ulong mFlags)
        {
            return ((mFlags & enableAssignToEmail) != 0L);
        }
        public static bool WorkflowsAssociated(ulong mFlags)
        {
            return ((mFlags & workflowsAssociated) != 0L);
        }

        public static bool EnableAttachments(ulong mFlags, AveBaseType baseType, AveListTemplateType baseTemplate)
        {
            if (HasExternalDataSource(mFlags))
            {
                return false;
            }
            if ((baseType == AveBaseType.DocumentLibrary) || (baseType == AveBaseType.Survey))
            {
                return false;
            }
            return ((mFlags & enableAttachments) == 0L);
        }

        public static bool EnableCrawlNonDefaultViews(ulong mFlags)
        {
            return ((mFlags & crawlNonDefaultViews) != 0L);
        }

        public static bool EnableDeployWithDependentList(ulong mFlags)
        {
            return ((mFlags & enableDeployWithDependentList) == 0L);
        }
        public static bool EnableFolderCreation(ulong mFlags)
        {
            return ((mFlags & enableFolderCreation) == 0L);
        }
        public static bool EnableVersioning(ulong mFlags)
        {
            return ((mFlags & enableVersioning) != 0L);
        }
        public static bool EnableMinorVersions(ulong mFlags, AveBaseType baseType)
        {
            if (baseType != AveBaseType.DocumentLibrary)
            {
                return false;
            }
            return ((mFlags & enableMinorVersions) != 0L);
        }
        public static bool EnableModeration(ulong mFlags)
        {
            return ((mFlags & enableModeration) != 0L);
        }
        public static bool EnablePeopleSelector(ulong mFlags)
        {
            return ((mFlags & enablePeopleSelector) != 0L);
        }
        public static bool EnableResourceSelector(ulong mFlags)
        {
            return ((mFlags & enableResourceSelector) != 0L);
        }
        public static bool EnableSchemaCaching(ulong mFlags)
        {
            return ((mFlags & enableSchemaCaching) != 0L);
        }
        public static bool EnforceDataValidation(ulong mFlags)
        {
            return ((mFlags & enforceDataValidation) != 0L);
        }
        public static bool Hidden(ulong mFlags)
        {
            return ((mFlags & hidden) != 0L);
        }
        public static bool Ordered(ulong mFlags)
        {
            return ((mFlags & ordered) != 0L);
        }
        public static bool HasExternalDataSource(ulong mFlags)
        {
            return ((mFlags & hasExternalDataSource) > 0L);
        }
        public static bool NoCrawl(ulong mFlags)
        {
            return ((mFlags & noCrawl) != 0L);
        }
        public static bool IrmEnabled(ulong mFlags)
        {
            return ((mFlags & irmEnabled) != 0L);
        }
        public static bool IrmExpire(ulong mFlags)
        {
            return (IrmEnabled(mFlags) && ((mFlags & irmExpire) != 0L));
        }
        public static bool IrmReject(ulong mFlags)
        {
            return (IrmEnabled(mFlags) && ((mFlags & irmReject) != 0L));
        }
        public static bool EnableSyndication(ulong mFlags)
        {
            return ((mFlags & enableSyndication) == 0L);
        }
        public static bool ExcludeFromOfflineClient(ulong mFlags)
        {
            return ((mFlags & excludeFromOfflineClient) != 0L);
        }
        public static bool ExcludeFromTemplate(ulong mFlags)
        {
            return (HasExternalDataSource(mFlags) || ((mFlags & excludeFromTemplate) != 0L));
        }
        public static AveDraftVisibilityType DraftVersionVisibility(ulong mFlags, AveBaseType baseType)
        {
            if (EnableMinorVersions(mFlags, baseType) || EnableModeration(mFlags))
            {
                if ((mFlags & draftVersionVisibility_Author) != 0L)
                {
                    return AveDraftVisibilityType.Author;
                }
                if ((mFlags & draftVersionVisibility_Approver) != 0L)
                {
                    return AveDraftVisibilityType.Approver;
                }
            }
            return AveDraftVisibilityType.Reader;
        }
        public static bool ForceCheckout(ulong mFlags, AveBaseType baseType)
        {
            if (baseType != AveBaseType.DocumentLibrary)
            {
                return false;
            }
            return ((mFlags & forceCheckout) != 0L);
        }
        public static bool MultipleDataList(ulong mFlags)
        {
            return ((mFlags & multipleDataList) != 0L);
        }
        public static bool AllowEveryoneViewItems(ulong mFlags)
        {
            return ((mFlags & allowEveryoneViewItems) != 0L);
        }
        public static bool AllowMultiResponses(ulong mFlags)
        {
            return ((mFlags & allowMultiResponses) != 0L);
        }
        public static AveBrowserFileHandling BrowserFileHandling(ulong mFlags)
        {
            bool flag = 0L != (mFlags & browserFileHandling);
            if (!flag)
            {
                return AveBrowserFileHandling.Permissive;
            }
            return AveBrowserFileHandling.Strict;
        }
        public static AveCalculationOptions CalculationOptions(ulong mFlags)
        {
            AveCalculationOptions none = AveCalculationOptions.None;
            if ((mFlags & (calculationOptions_PreserveEmptyValue)) != 0L)
            {
                none |= AveCalculationOptions.PreserveEmptyValues;
            }
            if ((mFlags & (calculationOptions_StrictTypeCoercion)) != 0L)
            {
                none |= AveCalculationOptions.StrictTypeCoercion;
            }
            return none;
        }

        public static AveDefaultItemOpen DefaultItemOpen(ulong mFlags, bool mParentWeb_Site_BrowserDocumentsEnabled)
        {
            if (IrmEnabled(mFlags))
            {
                return AveDefaultItemOpen.PreferClient;
            }
            if ((mFlags & defaultItemOpen_A) != 0L)
            {
                if ((mFlags & defaultItemOpen_B) != 0L)
                {
                    return AveDefaultItemOpen.Browser;
                }
                return AveDefaultItemOpen.PreferClient;
            }
            if (!mParentWeb_Site_BrowserDocumentsEnabled)
            {
                return AveDefaultItemOpen.PreferClient;
            }
            return AveDefaultItemOpen.Browser;
        }
        public static bool DefaultItemOpenUseListSetting(ulong mFlags)
        {
            return ((mFlags & defaultItemOpenUseListSetting) != 0L);
        }
        public static bool DisableGridEditing(ulong mFlags)
        {
            return ((mFlags & disableGridEditing) != 0L);
        }
        public static bool IsApplicationList(ulong mFlags)
        {
            return ((mFlags & isApplicationList) != 0L);
        }
        public static bool IsCatalog_Client()
        {
            return false;
        }
        public static bool RequestAccessEnabled(ulong mFlags)
        {
            return ((mFlags & requestAccessEnabled) == 0L);
        }
        public static bool RestrictedTemplateList(ulong mFlags)
        {
            return ((mFlags & restrictedTemplateList) != 0L);
        }
        public static bool RootWebOnly(ulong mFlags)
        {
            return ((mFlags & rootWebOnly) != 0L);
        }
        public static bool ShowUser(ulong mFlags)
        {
            return ((mFlags & showUser) != 0L);
        }
        public static bool EnableDeployingList()
        {
            return true;
        }
        public static bool NavigateForFormsPages(ulong mFlags)
        {
            return (0L != (mFlags & navigateForFormsPages));
        }
    }

    public enum AveDraftVisibilityType
    {
        // Summary:
        //      Init Value = -1.
        None = -1,
        // Summary:
        //     Reader. Value = 0.
        Reader = 0,
        //
        // Summary:
        //     Author. Value = 1.
        Author = 1,
        //
        // Summary:
        //     Approver. Value = 2.
        Approver = 2,
    }

    public enum AveBrowserFileHandling
    {
        Permissive,
        Strict
    }

    public enum AveCalculationOptions
    {
        None,
        PreserveEmptyValues,
        StrictTypeCoercion
    } 

}
