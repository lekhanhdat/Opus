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




namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum CrawlType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        FullCrawl = 1,
        [EnumMember]
        IncrementalCrawl = 2
    }

    [DataContract]
    public enum CrawlSettingActions
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        InstallCrawlComponent = 1,
        [EnumMember]
        UnInstallCrawlComponent,
        [EnumMember]
        RetrieveCrawlComponent,
        [EnumMember]
        RetrieveContentSourceStatus,
        [EnumMember]
        CreateContantSource,
        [EnumMember]
        StartFullCrawl,
        [EnumMember]
        StartIncrementalCrawl
    }

    [DataContract]
    public class ContentSourceStatus
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        //[DataMember]
        //public CrawlStatus Status { get; set; }

        public ContentSourceStatus(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
