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


namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    /// <summary>
    /// 此枚举中JobType定义和JobTypes.cs文件中的定义保持一致, 修改任意其中一个文件则需要同时修改另一个文件
    /// </summary>
    public enum CAJobTypes : int
    {
        /// <summary>
        /// 原始JobType, 6.2之前只有以下两种JobType
        /// </summary>
        CASearchJob = 2,
        /// <summary>
        /// 原始JobType, 6.2之前只有以下两种JobType
        /// </summary>
        CAJob = 3,

        #region 300-399 CA Used

        //#region Farm Level

        //CAFarmNewWebAppJob = 300,
        //CAFarmAdminSearchJob = 301,
        //CAFarmSearchDuplicateFileJob = 302,
        //CAFarmSecuritySearchJob = 303,
        //CAFarmCloneUserPermissionJob = 304,
        //CAFarmImportConfigurationFileJob = 305,
        //CAFarmDeadAccountCleanerJob = 306,

        //#endregion

        //#region Web App Level

        //CAWebAppAdminSearchJob = 310,
        //CAWebAppDeleteOrphanSiteJob = 311,
        //CAWebAppSearchWebPartJob = 312,
        //CAWebAppSearchDuplicateFileJob = 313,
        //CAWebAppSecuritySearchJob = 314,
        //CAWebAppCloneUserPermissionJob = 315,
        //CAWebAppDeadAccountCleanerJob = 316,

        //#endregion

        //#region Site Collection Level

        //CASiteCollectionMoveSiteCollectionJob = 320,
        //CASiteCollectionAdminSearchJob = 321,
        //CASiteCollectionCheckBrokenLinkJob = 322,
        //CASiteCollectionSearchWebPartJob = 323,
        //CASiteCollectionSearchDuplicateFileJob = 324,
        //CASiteCollectionSecuritySearchJob = 325,
        //CASiteCollectionCloneUserPermissionJob = 326,
        //CASiteCollectionCloneSitePermissionJob = 327,
        //CASiteCollectionStopInheritingPermissionsJob = 328,
        //CASiteCollectionDeadAccountCleanerJob = 329,

        //#endregion

        //#region Site Level

        //CASiteAdminSearchJob = 331,
        //CASiteCheckBrokenLinkJob = 332,
        //CASiteSearchWebPartJob = 333,
        //CASiteSearchDuplicateFileJob = 334,
        //CASiteSecuritySearchJob = 370,
        //CASiteCloneUserPermissionsJob = 335,
        //CASiteCloneSitePermissionJob = 336,
        //CASiteStopInheritingPermissionsJob = 337,
        //CASiteDeadAccountCleanerJob = 338,

        //#endregion

        //#region List Level

        //CAListAdminSearchJob = 340,
        //CAListSecuritySearchJob = 341,
        //CAListCloneUserPermissionJob = 342,
        //CAListCloneListLibraryPermissionJob = 343,
        //CAListStopInheritingPermissionsJob = 344,
        //CAListInheritPermissionsJob = 345,

        //#endregion

        //#region Folder Level

        //CAFolderAdminSearchJob = 350,
        //CAFolderSecuritySearchJob = 351,
        //CAFolderCloneUserPermissionJob = 352,
        //CAFolderCloneFolderPermissionJob = 353,
        //CAFolderStopInheritingPermissionsJob = 354,
        //CAFolderInheritPermissionsJob = 355,

        //#endregion

        //#region Item Level

        //CAItemAdminSearchJob = 360,
        //CAItemSecuritySearchJob = 361,
        //CAItemChangeMetadataJob = 363,

        //#endregion

        //#region 以下类型Job不区分Level

        //CADeleteTempPermissionJob = 371,
        CAOfflineExportReportJob = 372,
        CAProfileJob = 373,
        /// <summary>
        /// PE Job中只有Auditor类型的Rule
        /// </summary>
        CAOnlyAuditorRulePEJob = 374,
        CAOnlyScanRulePEJob = 375,

        #endregion

    }
}
