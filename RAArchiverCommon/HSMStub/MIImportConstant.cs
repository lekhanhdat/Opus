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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HSMCommon
{
    public class MIImportConstant
    {
        public const string MANIFEST_XML_NAME = "Manifest.xml";
        public const string EXPORTSETTINGS_XML_NAME = "ExportSettings.xml";
        public const string LOOKUPLISTSMAP_XML_NAME = "LookupListMap.xml";
        public const string REQUIREMENTS_XML_NAME = "Requirements.xml";
        public const string ROOTOBJECTMAP_XML_NAME = "RootObjectMap.xml";
        public const string SYSTEMDATA_XML_NAME = "SystemData.xml";
        public const string USETGROUP_XML_NAME = "UserGroup.xml";
        public const string VIEWFORMSLIST_XML_NAME = "ViewFormsList.xml";

        public const int THRESHOLD_CACHE_STUB_CACHE_IN_STORAGE = 10000 * 50;

        static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static Int32 FileValue = 0;
        public static Int32 PackageCountCapacity = 250;

        //Temp use Int Type
        public static Int32 PackageSizeCapacity = 100 * 1024;

        private static Dictionary<CultureLCID, string> nFileNameCultures = new Dictionary<CultureLCID, string>();

        private static string LcidCultureValue(Dictionary<CultureLCID, string> valueCollection, CultureLCID id)
        {
            string tempValue = string.Empty;
            try
            {
                if (valueCollection != null && valueCollection.Count > 0)
                {
                    if (valueCollection.TryGetValue(id, out tempValue))
                    {
                        return tempValue;
                    }
                }
            }
            catch (Exception el)
            {
                logger.Error("An error occurred while getting lcid value,details:{0}.", el.ToString());
                tempValue = string.Empty;
            }
            return tempValue;
        }

        private static string newFileName = null;
        private static CultureLCID lcid = CultureLCID.USA;
        public static string NEWFILENAME
        {
            get
            {
                if (string.IsNullOrEmpty(newFileName))
                {
                    newFileName = LcidCultureValue(nFileNameCultures, lcid);
                    //if (string.IsNullOrEmpty(newFileName))
                    //    newFileName = "New File Name";
                }
                return newFileName;
            }
        }
    }

    public enum CultureLCID
    {
        USA = 1033, GERMANY = 1031, JAPAN = 1041
    }
}
