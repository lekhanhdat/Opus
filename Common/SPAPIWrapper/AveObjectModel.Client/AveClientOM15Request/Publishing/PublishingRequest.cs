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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client.Publishing;

    public partial class AveClientOM2013Request
    {
        public void ApplyDesignPackage(AveDesignPackageInfo info)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var package = new DesignPackageInfo()
                {
                    PackageName=info.PackageName,
                    PackageGuid=info.PackageGuid,
                    MajorVersion=info.MajorVersion,
                    MinorVersion=info.MinorVersion
                };
                DesignPackage.Apply(context, context.Site, package);
                context.ExecuteQuery();
            }
        }

        public AveDesignPackageInfo ExportEnterpriseDesignPackage(bool includeSearchConfiguration)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var package = DesignPackage.ExportEnterprise(context, context.Site, includeSearchConfiguration);
                context.ExecuteQuery();
                var avePackage = new AveDesignPackageInfo()
                {
                    PackageName = package.Value.PackageName,
                    PackageGuid = package.Value.PackageGuid,
                    MajorVersion = package.Value.MajorVersion,
                    MinorVersion = package.Value.MinorVersion,
                };
                return avePackage;
            }
        }

        public AveDesignPackageInfo ExportSmallBusinessDesignPackage(string packageName, bool includeSearchConfiguration)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var package = DesignPackage.ExportSmallBusiness(context, context.Site, packageName, includeSearchConfiguration);
                context.ExecuteQuery();
                var avePackage = new AveDesignPackageInfo()
                {
                    PackageName = package.Value.PackageName,
                    PackageGuid = package.Value.PackageGuid,
                    MajorVersion = package.Value.MajorVersion,
                    MinorVersion = package.Value.MinorVersion,
                };
                return avePackage;
            }
        }

        public void InstallDesignPackage(string solutionFileName, string solutionFileServerRelativeUrl)
        {
            AveDesignPackageInfo aveDesignPackageInfo = new AveDesignPackageInfo
            {
                MajorVersion = 1,
                MinorVersion = 1,
                PackageGuid = Guid.Empty,
                PackageName = solutionFileName
            };
            InstallDesignPackage(aveDesignPackageInfo, solutionFileServerRelativeUrl);
        }

        public void InstallDesignPackage(AveDesignPackageInfo info, string path)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var package = new DesignPackageInfo()
                {
                    PackageName = info.PackageName,
                    PackageGuid = info.PackageGuid,
                    MajorVersion = info.MajorVersion,
                    MinorVersion = info.MinorVersion
                };
                DesignPackage.Install(context, context.Site, package,path);
                context.ExecuteQuery();
            }
        }

        public void UnInstallDesignPackage(AveDesignPackageInfo info)
        {
            using (var context = CreateContext(mWebUrl))
            {
                var package = new DesignPackageInfo()
                {
                    PackageName = info.PackageName,
                    PackageGuid = info.PackageGuid,
                    MajorVersion = info.MajorVersion,
                    MinorVersion = info.MinorVersion
                };
                DesignPackage.UnInstall(context, context.Site, package);
                context.ExecuteQuery();
            }
        }
    }
}
