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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Wrapper.Backup;

namespace RAExportCommon
{
    public class RecordVEOParametersV3
    {
        public string VIOType { get; set; } // InformationObjectType
        public string VIODepth { get; set; } // InformationObjectDepth

        public string VId { get; set; } // IdentifierString

        public string VName { get; set; } // NameWords

        public string VUrl { get; set; } // about, Location

        public string VCreated { get; set; } // StartDate

        public bool HasIP { get; set; } // check has information piece

        public string? VContentPathName { get; set; } // PathName

        public string? VContentFullPath { get; set; } // cache full path to get HashValue

        public string VContentHashValue { get; set; } = string.Empty; // Message Disgest, HashValue if SHA-512

        public AveSPSite? AveSPSite { get; set; }
        
        public AveSPWeb? AveSPWeb { get; set; }

        public AveSPList? AveSPList { get; set; }

        public AveSPFolder? AveSPFolder { get; set; }

        public AveSPDoc? AveSPDoc { get; set; }

        public PolicyLevel Level { get; set; }

        /// <summary>
        /// add params for document
        /// </summary>
        public RecordVEOParametersV3(string vIOType, string vIODepth, string vId, string vName, string vUrl, string vCreated, string vFolderPath, string vContentFullPath, AveSPDoc aveSPDoc)
        {
            VIOType = vIOType;
            VIODepth = vIODepth;
            VId = vId;
            VName = vName;
            VUrl = vUrl;
            VCreated = vCreated;
            VContentPathName = vFolderPath;
            VContentFullPath = vContentFullPath;
            AveSPDoc = aveSPDoc;
            HasIP = true;
            Level = PolicyLevel.Document;
        }

        /// <summary>
        /// add params for for site, web, list, folder
        /// </summary>
        public RecordVEOParametersV3(string vIOType, string vIODepth, string vId, string vName, string vUrl, string vCreated)
        {
            VIOType = vIOType;
            VIODepth = vIODepth;
            VId = vId;
            VName = vName;
            VUrl = vUrl;
            VCreated = vCreated;
        }

        public RecordVEOParametersV3 Init(PolicyLevel level, AveSPSite? aveSite = null, AveSPWeb? aveWeb = null, AveSPList? aveList = null, AveSPFolder? aveFolder = null)
        {
            Level = level;
            AveSPSite = aveSite;
            AveSPWeb = aveWeb;
            AveSPList = aveList;
            AveSPFolder = aveFolder;
            return this;
        }
    }
}
