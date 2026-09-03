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
using RADownloadCentre.SettingExport.Base;
using AvePoint.RA.I18N.Core;

namespace RADownloadCentre.SettingExport.ContentSourceExportColumns.Teams;

public class TeamsHeaderExportColumns : BaseExportCsv
{
    private static readonly string ContainerColumn = I18NEntity.GetString("RM_JS_BCM_Export_ContainerColumn");
    private static readonly string TeamsOrGroupColumn = I18NEntity.GetString("RM_JS_BCM_Export_TeamsOrGroupColumn");
    private static readonly string SiteCollectionColumn = I18NEntity.GetString("RM_JS_BCM_Export_SiteCollectionColumn");
    private static readonly string SiteColumn = I18NEntity.GetString("RM_JS_BCM_Export_SiteColumn");
    private static readonly string ListColumn = I18NEntity.GetString("RM_JS_BCM_Export_LibraryColumn");
    private static readonly string FolderColumn = I18NEntity.GetString("RM_JS_BCM_Export_FolderColumn");

    private readonly List<string> _basicColumns =
    [
        ContainerColumn,
        TeamsOrGroupColumn,
        SiteCollectionColumn,
        SiteColumn,
        ListColumn,
        FolderColumn
    ];
    
    public TeamsHeaderExportColumns()
    {
        ExportColumns.AddRange(_basicColumns);
    }
}