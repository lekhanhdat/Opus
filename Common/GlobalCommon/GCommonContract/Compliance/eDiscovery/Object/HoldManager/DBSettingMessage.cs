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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Object.HoldManager
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DBSettingMessage : HoldBaseMessage
    {
        [DataMember]
        public CplDBSettingsDto DBSettingDto { get; set; }
        [DataMember]
        public SettingType SettingType { get; set; }
        [DataMember]
        public bool TestUserSeccuss { get; set; }
        [DataMember]
        public bool SetDBSeccuss { get; set; }
        /// <summary>
        /// HeldMapping DeleteFlag 0:没有删除，1：HeldData被删除 2：Hold被删除 4：关联关系被删除 
        /// </summary>
        [DataMember]
        public static string queryCreateTable = @"
USE [{0}];

IF OBJECT_ID('[HeldData]', 'U') IS NOT NULL
    DROP TABLE [HeldData];

IF OBJECT_ID('[HoldItem]', 'U') IS NOT NULL
    DROP TABLE [HoldItem];

IF OBJECT_ID('[HeldMapping]', 'U') IS NOT NULL
    DROP TABLE [HeldMapping];

CREATE TABLE [HeldData] (
    [Id] nvarchar(255)  NOT NULL,
    [Name] nvarchar(255)  NULL,
    [DataGuid] nvarchar(128)  NULL,
    [DataSource] int  NOT NULL,
	[DataType] int DEFAULT 0,
    [Size] int DEFAULT 0,
    [CreateBy] nvarchar(128)  NULL,
	[LastModifiedTime] bigint DEFAULT 0,
	[Location] nvarchar(max) Null,
    [FarmId] nvarchar(128)  NULL,
    [WebAppId] nvarchar(128)  NULL,
    [SiteCollectionId] nvarchar(128)  NULL,
    [WebId] nvarchar(128)  NULL,
    [ListId] nvarchar(128)  NULL
);


CREATE TABLE [HoldItem] (
    [Id] nvarchar(255)  NOT NULL,
    [Name] nvarchar(255)  NULL,
    [Description] nvarchar(max) NULL,
    [ManagedBy] nvarchar(255) NULL,
    [Location] nvarchar(max) NULL,
    [LastModifiedTime] bigint DEFAULT 0,
    [HoldItemGuid] nvarchar(128)  NULL,
	[Type] int NOT NULL,
	[HeldDataCount] int DEFAULT 0,
	[ParentId] nvarchar(128)  NULL,
    [FarmId] nvarchar(128)  NULL,
    [WebAppId] nvarchar(128)  NULL,
    [SiteCollectionId] nvarchar(128)  NULL,
    [WebId] nvarchar(128)  NULL,
	[ListId] nvarchar(128)  NULL
);


CREATE TABLE [HeldMapping] (
    [Id] nvarchar(255)  NOT NULL,
    [HoldItemId] nvarchar(128)  NOT NULL,
    [HoldItemGuid] nvarchar(128) NOT NULL,
    [HeldDataId] nvarchar(128) NULL,
    [HeldDataGuid] nvarchar(max) NULL,
    [DataSource] int Not NULL,
    [FarmId] nvarchar(128)  NULL,
    [WebAppId] nvarchar(128)  NULL,
    [SiteCollectionId] nvarchar(128)  NULL,
    [WebId] nvarchar(128)  NULL,
	[ListId] nvarchar(128)  NULL,
    [DeleteFlag] int NOT NULL
);

ALTER TABLE [HeldMapping] ADD  CONSTRAINT [DF_HeldMapping_DeleteFlag]  DEFAULT ((0)) FOR [DeleteFlag]


ALTER TABLE [HeldData]
ADD CONSTRAINT [PK_HeldData]
    PRIMARY KEY CLUSTERED ([Id] ASC);


ALTER TABLE [HoldItem]
ADD CONSTRAINT [PK_HoldItem]
    PRIMARY KEY CLUSTERED ([Id] ASC);


ALTER TABLE [HeldMapping]
ADD CONSTRAINT [PK_HeldMapping]
    PRIMARY KEY CLUSTERED ([Id] ASC);
";
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SettingType
    {
        [EnumMember]
        GetDBServer = 0,
        [EnumMember]
        TestUser = 1,
        [EnumMember]
        SetDatabase = 2
    }
}
