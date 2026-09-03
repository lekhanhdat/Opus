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
using RADownloadCentre.SettingExport.Model;

namespace RADownloadCentre.SettingExport.ContentSourceExportColumns.Teams;

public class TeamsContentExportColumns : BaseExportCsv
{
    public TeamsContentExportColumns((string SiteCollection, string Site, string List, string Folder) splitPath, ExportTeamsSettingData setting)
    {
        ExportColumns.AddRange(AddBasicInformation(splitPath, setting));
    }

    private List<string> AddBasicInformation((string SiteCollection, string Site, string List, string Folder) splitPath, ExportTeamsSettingData setting)
    {
        var containerName = ProcessCol(setting.ContainerName);
        var teamsOrGroup = ProcessCol(setting.TeamsOrGroupName);
        var siteCollection = ProcessCol(splitPath.SiteCollection);
        var site = ProcessCol(splitPath.Site);
        var list = ProcessCol(splitPath.List);
        var folder = ProcessCol(splitPath.Folder);
        return [containerName,teamsOrGroup,siteCollection,site,list,folder];
    }
}