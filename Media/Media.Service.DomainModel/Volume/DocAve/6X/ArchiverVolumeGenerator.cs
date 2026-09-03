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




namespace AvePoint.Media.Service.DomainModel.DocAve6x
{
    #region using directives
    using System;
    using System.IO;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.DomainModel;
    using DocumentFormat.OpenXml.Wordprocessing;

    #endregion

    public class ArchiverVolumeGenerator : ArchiverVolumeGeneratorBase
    {
        public override String GenerateDataVolume(VolumeParameter param)
        {
            if (string.IsNullOrEmpty(param.SiteCollectionUrl))
            {
                return SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(ContainerPath, "DataVolume"), ProcessFarmName(param.FarmName));
            }
            String webAppName, siteName;
            ParseSitePath(param.SiteCollectionUrl, out webAppName, out siteName);
            return SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(ContainerPath, "DataVolume"), ConvertFarmNameToUpper(ProcessFarmName(param.FarmName))), webAppName), siteName);
        }

        public override String GenerateIndexVolume(VolumeParameter param)
        {
            if (string.IsNullOrEmpty(param.SiteCollectionUrl))
            {
                return SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(ServiceConstants.ArchiverPath, "IndexVolume"), ProcessFarmName(param.FarmName));
            }
            String webAppName, siteName;
            ParseSitePath(param.SiteCollectionUrl, out webAppName, out siteName);
            return SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(ContainerPath, "IndexVolume"), ConvertFarmNameToUpper(ProcessFarmName(param.FarmName))), webAppName), siteName);
        }

        public string GenerateSitePath(string siteUrl)
        {
            ParseSitePath(siteUrl, out var webAppName, out var siteName);
            return SecurityUtils.SafeCombinePath(webAppName, siteName);
        }

        public override string GenerateTempDataVolume(VolumeParameter param)
        {
            throw new NotImplementedException();
        }

        public virtual String ContainerPath
        {
            get
            {
                return ServiceConstants.ArchiverPath;
            }
        }
    }

    public class TeamsArchiverVolumeGenerator : ArchiverVolumeGeneratorBase
    {
        public override String GenerateDataVolume(VolumeParameter param)
        {
            return $"{ContainerPath}/DataVolume/{ConvertEmailToPath(param.SiteCollectionUrl)}";
        }

        public override String GenerateIndexVolume(VolumeParameter param)
        {
            return $"{ContainerPath}/IndexVolume/{ConvertEmailToPath(param.SiteCollectionUrl)}";
        }

        private string ConvertEmailToPath(string emailAddress)
        {
            var (domain, name) = ParseEmailPath(emailAddress);
            return $"{domain}/{name}";
        }

        private (string, string) ParseEmailPath(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentException("Email address cannot be null or empty.");
            }
            var parts = emailAddress.Split('@');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid email address format.");
            }
            var name = parts[0];
            var domain = parts[1];
            return (domain, name);
        }

        public override string GenerateTempDataVolume(VolumeParameter param)
        {
            throw new NotImplementedException();
        }

        public virtual String ContainerPath
        {
            get
            {
                return ServiceConstants.TeamsArchiverPath;
            }
        }
    }

    public class ExchangeVolumeGenerator : VolumeGeneratorBase
    {
        public override string GenerateDataVolume(VolumeParameter param)
        {
            var modulePath = ServiceConstants.EXOArchiverPath;
            return Path.Combine(modulePath, Path.Combine(ServiceConstants.DefaultDataVolume, ConvertEmailToPath(param.EmailAddress), param.PlanId ?? string.Empty, Path.Combine(param.CycleId ?? string.Empty)));
        }
        public override string GenerateTempDataVolume(VolumeParameter param)
        {
            var modulePath = ServiceConstants.EXOArchiverPath;
            return Path.Combine(modulePath, param.TempFolder, Path.Combine(ConvertEmailToPath(param.EmailAddress), param.PlanId ?? string.Empty, Path.Combine(param.CycleId ?? string.Empty)));
        }
        public override string GenerateIndexVolume(VolumeParameter param)
        {
            var modulePath = ServiceConstants.EXOArchiverPath;
            return Path.Combine(modulePath, Path.Combine(ServiceConstants.DefaultIndexVolume, ConvertEmailToPath(param.EmailAddress),param.PlanId ?? string.Empty, param.CycleId ?? string.Empty));
        }
        private string ConvertEmailToPath(string emailAddress)
        {
            var (domain, name) = ParseEmailPath(emailAddress);
            return SecurityUtils.SafeCombinePath(domain, name);
        }

        private (string, string) ParseEmailPath(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentException("Email address cannot be null or empty.");
            }
            var parts = emailAddress.Split('@');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid email address format.");
            }
            var name = parts[0];
            var domain = parts[1];
            return (domain, name);
        }
    }
    public class GDriveArchiverVolumeGenerator : ArchiverVolumeGeneratorBase
    {
        public override String GenerateDataVolume(VolumeParameter param)
        {
            return $"{ContainerPath}/DataVolume/{param.TenantId}/{param.DriveId}";
        }

        public override String GenerateIndexVolume(VolumeParameter param)
        {
            return $"{ContainerPath}/IndexVolume/{param.TenantId}/{param.DriveId}";
        }

        public override string GenerateTempDataVolume(VolumeParameter param)
        {
            throw new NotImplementedException();
        }

        public virtual String ContainerPath
        {
            get
            {
                return ServiceConstants.GoogleArchiverPath;
            }
        }
    }
}