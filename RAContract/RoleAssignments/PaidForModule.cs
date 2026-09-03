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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RoleAssignments
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkedToFeatureAttribute : Attribute
    {
        public PaidForModule PaidForModule { get; private set; }

        public LinkedToFeatureAttribute(PaidForModule paidForModule)
        {
            PaidForModule = paidForModule;
        }
    }

    [Flags]
    public enum PaidForModule : long
    {
        None = 0,
        FileSystem = 1,
        SharePointOnPrem = 2,
        AzureFiles = 4,
        Connector = 8,
        Box = 16,
        Google = 32,
        Salesforce = 64,
        Office365 = 128,
        GoogleControl = 256,
    }

    [Flags]
    public enum PreviewFeature : long
    {
        None = 0,
        ExportIndex = 2,
        VEOV3 = 16,
        DiscoveryExportRowData = 64,
        FileSystemDiscovery = 128,
        DiscoveryPlan = 256,
    }

    public static class PaidForModuleExtension
    {
        public static bool HasAnyFlag(this PaidForModule source, PaidForModule destination)
        {
            var permissions = destination.SplitPermission();
            if (permissions.Any(p => source.HasFlag(p)))
            {
                return true;
            }
            return false;
        }

        public static List<PaidForModule> SplitPermission(this PaidForModule permissions)
        {
            List<PaidForModule> result = new List<PaidForModule>();

            var permissionList = permissions.ToString().Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permission in permissionList)
            {
                if (Enum.TryParse(permission, out PaidForModule p))
                {
                    result.Add(p);
                }

            }
            return result;
        }
    }


    [Flags]
    public enum PaidForProduct : long
    {
        None = 0,
        OpusIL = 1,
        OpusSO = 2,
        OpusDiscovery = 4,
        OpusGoogle = 8,
        OpusSalesforceDiscovery = 16,
        OpusGoogleWorkspaceDiscovery = 32,
        GoogleControl = 256,
        OpusFileSystemDiscovery = 64
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class LinkedToProductAttribute : Attribute
    {
        public PaidForProduct PaidForProduct { get; private set; }

        public LinkedToProductAttribute(PaidForProduct paidForProduct)
        {
            PaidForProduct = paidForProduct;
        }

        public LinkedToProductAttribute(PaidForProduct paidForProduct1, PaidForProduct paidForProduct2)
        {
            PaidForProduct = paidForProduct1 | paidForProduct2;

        }
        public LinkedToProductAttribute(PaidForProduct paidForProduct1, PaidForProduct paidForProduct2, PaidForProduct paidForProduct3)
        {
            PaidForProduct = paidForProduct1 | paidForProduct2 | paidForProduct3;

        }
    }

    public static class PaidForProductExtension
    {
        public static bool HasAnyFlag(this PaidForProduct source, PaidForProduct destination)
        {
            var permissions = destination.SplitPermission();
            if (permissions.Any(p => source.HasFlag(p)))
            {
                return true;
            }
            return false;
        }

        public static List<PaidForProduct> SplitPermission(this PaidForProduct permissions)
        {
            List<PaidForProduct> result = new List<PaidForProduct>();

            var permissionList = permissions.ToString().Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permission in permissionList)
            {
                if (Enum.TryParse(permission, out PaidForProduct p))
                {
                    result.Add(p);
                }

            }
            return result;
        }
    }
}
