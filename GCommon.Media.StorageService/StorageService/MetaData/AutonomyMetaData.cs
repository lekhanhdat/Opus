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




namespace AvePoint.GCommon.Media.StorageService
{
    #region using direcitives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion using direcitives

    public class AutonomyMetaData
        : MetaData
    {
        [AutonomyMetaData("#DREREFERENCE", "")]
        public String DreReference { get; set; }

        [AutonomyMetaData("#DRETITLE", "")]
        public String DreTitle { get; set; }

        [AutonomyMetaData("#DREFIELD", "AbsoluteApiUrl")]
        public String AbsoluteApiUrl { get; set; }

        [AutonomyMetaData("#DREFIELD", "AUTN_COLLECTGROUP")]
        public String AutonomyCollectGroup { get; set; }

        [AutonomyMetaData("#DREFIELD", "AUTN_GROUP")]
        public String AutonomyGroup { get; set; }

        [AutonomyMetaData("#DREFIELD", "AUTN_IDENTIFIER")]
        public String AutonomyIdentifier { get; set; }

        [AutonomyMetaData("#DREFIELD", "AUTONOMYMETADATA")]
        public String AutonomyMetaDataString { get; set; }

        [AutonomyMetaData("#DREFIELD", "BaseSharePointType")]
        public String BaseSharePointType { get; set; }

        [AutonomyMetaData("#DREFIELD", "ContentSize")]
        public String ContentSize { get; set; }

        [AutonomyMetaData("#DREFIELD", "DisplayName")]
        public String DisplayName { get; set; }

        [AutonomyMetaData("#DREFIELD", "DocTrackingId")]
        public String DocTrackId { get; set; }

        [AutonomyMetaData("#DREDATE", "")]
        public String DreDate { get; set; }

        [AutonomyMetaData("#DREDBNAME", "")]
        public String DreDatabaseName { get; set; }

        [AutonomyMetaData("#DREFIELD", "FileType")]
        public String FileType { get; set; }

        [AutonomyMetaData("#DREFIELD", "ItemIdInList")]
        public String ItemIdInList { get; set; }

        [AutonomyMetaData("#DREFIELD", "ListBaseTemplate")]
        public String ListBaseTemplate { get; set; }

        [AutonomyMetaData("#DREFIELD", "ListBaseType")]
        public String ListBaseType { get; set; }

        [AutonomyMetaData("#DREFIELD", "ListDescription")]
        public String ListDescription { get; set; }

        [AutonomyMetaData("#DREFIELD", "ListGuid")]
        public String ListGuid { get; set; }

        [AutonomyMetaData("#DREFIELD", "ListTitle")]
        public String ListTitle { get; set; }

        [AutonomyMetaData("#DREFIELD", "ListUrl")]
        public String ListUrl { get; set; }

        [AutonomyMetaData("#DREFIELD", "Name")]
        public String Name { get; set; }

        [AutonomyMetaData("#DREFIELD", "SiteCollectionGuid")]
        public String SiteCollectionGuid { get; set; }

        [AutonomyMetaData("#DREFIELD", "SiteDescription")]
        public String SiteDescription { get; set; }

        [AutonomyMetaData("#DREFIELD", "SiteGuid")]
        public String SiteGuid { get; set; }

        [AutonomyMetaData("#DREFIELD", "SiteGuidHierarchy")]
        public String SiteGuidHierarchy { get; set; }

        [AutonomyMetaData("#DREFIELD", "SiteTitle")]
        public String SiteTitle { get; set; }

        [AutonomyMetaData("#DREFIELD", "SiteUrl")]
        public String SiteUrl { get; set; }

        [AutonomyMetaData("#DREFIELD", "Title")]
        public String Title { get; set; }

        [AutonomyMetaData("#DREFIELD", "UniqueId")]
        public String UniqueId { get; set; }

        [AutonomyMetaData("#DREFIELD", "UnixDateModified")]
        public String UnixDateModified { get; set; }

        [AutonomyMetaData("#DREFIELD", "Url")]
        public String Url { get; set; }

        [AutonomyMetaData("#DREFIELD", "VersionId")]
        public String VersionId { get; set; }

        [AutonomyMetaData("#DREFIELD", "WebApplicationGuid")]
        public String WebApplicationGuid { get; set; }

        [AutonomyMetaData("#DREFIELD", "WindowsDateModified")]
        public String WindowsDateModified { get; set; }

        [AutonomyMetaData("#DREFIELD", "X-DATASOURCE")]
        public String XDataSource { get { return "DocAve6"; } }

        [AutonomyMetaData("#DREFIELD", "X-FILENAME")]
        public String XFileName { get; set; }

        [AutonomyMetaData("#DREFIELD", "X-ZANTAZ-PRIMARY-UID")]
        public String XZantazPrimaryUid { get; set; }

        [AutonomyMetaData("#DRECONTENT", "")]
        public String DreContent { get; set; }

        String identifierTemplate = "<id s=\"{0}\" r=\"{1}\"><p n=\"{2}\" v=\"{3}\" /></id>";

        public String GetAutonomyIdentifier(
            String referenceUrl,
            String apiUrl,
            String sectionName = "SHAREPOINT2010",
            String navigate = "ApiUrl")
        {
            var identifier = identifierTemplate.FormatWith(sectionName, referenceUrl, navigate, apiUrl);
            var identifierArray = Encoding.UTF8.GetBytes(identifier);
            return Convert.ToBase64String(identifierArray);
        }
    }
}