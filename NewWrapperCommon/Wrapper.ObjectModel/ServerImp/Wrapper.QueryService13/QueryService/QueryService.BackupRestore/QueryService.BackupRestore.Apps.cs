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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        public int GetAppAuthorAndAppEditor(Guid siteId, string appPrincipalId)
        {
            int id = -1;
            try
            {
                string cmdText = @"SELECT TOP 1 Id FROM AppPrincipals WITH(NOLOCK) WHERE SiteId =@SiteId AND Name=@AppPrincipalId";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@AppPrincipalId", appPrincipalId);

                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        id = dr.GetInt32(0);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            return id;
        }

        public AveAppInstanceStatus CheckAppInstallationStatus(Guid siteId, Guid webId, Guid sourceInfoId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.CheckAppInstallationStatus"))
            {
                string cmdText = @"SELECT TOP 1 Installations.Status FROM
    TVF_AppSourceInfo_SourceInfoId(@SiteId, @SourceInfoId) as InstallingSourceInfo --the item being installed
    CROSS APPLY TVF_AppPackages_PackageFingerprint(@SiteId, InstallingSourceInfo.PackageFingerprint) as InstallingPackage --the package being installed
    CROSS APPLY TVF_AppPackages_ProductId(@SiteId, InstallingPackage.ProductId) as ProductPackages --all packages with same ProductId
    CROSS APPLY TVF_AppSourceInfo_PackageFingerprint(@SiteId, ProductPackages.PackageFingerprint) as ProductSourceInfo --all source info with same product id
    CROSS APPLY TVF_AppInstallations_WebIdSourceInfoId(@SiteId, @WebId, ProductSourceInfo.SourceInfoId) AS Installations --all installations with same product id";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@WebId", webId);
                mQueryWorker.AddParameter("@SourceInfoId", sourceInfoId);

                object status = mQueryWorker.ExecuteScalar(cmdText);
                if (status == null)
                {
                    return AveAppInstanceStatus.InvalidStatus;
                }
                else
                {
                    return (AveAppInstanceStatus)Convert.ToInt32(status);
                }
            }
        }

        public string GetAppManifest(byte[] appFingerprint, Guid siteId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.GetAppManifest"))
            {
                string cmdText = @"SELECT TOP 1 Manifest FROM AppPackages WITH(NOLOCK) WHERE SiteId=@SiteId AND PackageFingerprint=@PackageFingerprint";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@PackageFingerprint", appFingerprint);

                object buffer = mQueryWorker.ExecuteScalar(cmdText);
                if (buffer == null)
                {
                    return null;
                }
                else
                {
                    return Encoding.UTF8.GetString((byte[])buffer);
                }
            }
        }
    }
}
