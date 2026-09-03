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

namespace AvePoint.RA.I18N.Core.DaoMigration
{
    public class CPI18NResource
    {
        public static string Execution(string key, params object[] args)
        {
            return GetString(key);
        }

        public const string DeleteTheData = "Delete the Data";
        public const string MoveDatatoLogicalDevice = "Move the Data to Logical Device";
        public const string CustomAction = "Custom Action";
        public const string Automatically = "Automatically";
        public const string Manually = "Manually";
        public const string JobInformation = "Job Information";
        public const string RetentionJobs = "Retention Jobs";
        public const string OneOrMore = "One or more physical devices are not available in the specified logical device of the storage policy {0}.";
        public const string MergeIndexSuccess = "Successfully merged the index.";

        public static string GetString(string comment)
        {
            switch (comment)
            {
                case DeleteTheData:
                    return Get("ControlPanel.Service_3c977214-312b-4eb9-95f6-8965e8b3547f", "Delete the Data");
                case MoveDatatoLogicalDevice:
                    return Get("ControlPanel.Service_1256bd84-cb05-4097-85eb-6602590349c8", "Move the Data to Logical Device");
                case CustomAction:
                    return Get("ControlPanel.Service_3af00928-3118-4588-abb5-77e4a9c77796", "Custom Action");
                case Automatically:
                    return Get("ControlPanel.Service_d3b13588-cad9-4594-a8a1-5e4dc88df627", "Automatically");
                case Manually:
                    return Get("ControlPanel.Service_c5549258-0d10-41b1-9a78-a8a135789333", "Manually");
                case JobInformation:
                    return Get("ControlPanel.Service_73157232-c747-4a7e-9a1c-98cb77e3ef06", "Job Information");
                case RetentionJobs:
                    return Get("ControlPanel.Service_673619a7-6a12-4605-8c8a-9a619dd265cd", "Retention Jobs");
                case MergeIndexSuccess:
                    return Get("ControlPanel.Service_992CF4CD-29E0-498F-AA3C-B169A7DA6C21", "Successfully merged the index.");
                default:
                    return string.Empty;
            }
        }
        private static string Get(string key, string defaultValue, params object[] args)
        {
            var value = I18NEntity.GetString(key, args);
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }
            return value;
        }
    }
}
