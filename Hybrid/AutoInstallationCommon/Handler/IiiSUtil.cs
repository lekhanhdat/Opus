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


namespace AutoInstallationCommon.Utility.Handler
{
    public interface IiiSUtil
    {
        void CreateApplicationPool(string appPoolName, string username, string pwd, string framework = "v2.0");

        void CreateWebSite(string webSiteName, string webSiteHostHeader, string appPoolName, string schema, int port,
            string certHashString, string physicalPath, bool isExistingSite, string username, string password,
            bool anonymousAuthentication, bool windowsAuthentication);

        void ChangeCertificate(string webSiteName, string certHashString);
        void StartApplicationPool(string appPoolName);
        void StartWebSite(string webSiteName);
        void DeleteApplicationPool(string appPoolName);
        void DeleteWebSite(string webSiteName);
        void StopApplicationPool(string appPoolName);
        void StopWebSite(string webSiteName);
        void SetWebSiteAuthenticationInfo(bool anonymousAuthentication, bool windowsAuthentication, string webSiteName);

        void SetFolderAuthenticationInfo(bool anonymousAuthentication, bool windowsAuthentication, string webSiteName,
            string folderPath);
    }
}