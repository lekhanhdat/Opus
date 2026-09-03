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
using AngleSharp.Css.Values;
using AvePoint.GCommon.Contract.Replicator.Object.ViewModels;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Item.Restore;
using AvePoint.Media.ClassicStorage.Util;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Core.IO;
using AvePoint.Media.Core.IO.Input;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.Media.StorageApi;
using AvePoint.Metadata;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.GraphApi.UsageReport;
using AvePoint.RA.Common.RAProcess.Extractor;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.RMTasks;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Google.Work;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Trigger;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;


//using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using AvePoint.RA.Service.Services.Settings;
using AvePoint.RA.SharePoint.Common;
//using AvePoint.Wrapper.Common;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Data.IE;
using Cloud.Sdk.IE;
using DocumentFormat.OpenXml.InkML;
using LiteDB;
using Media.Common.ClassicStorageApi;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using PnP.Framework.Extensions;
using Storage;
using Storage.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Util.AI.Text.Extractor;
using static Microsoft.Office.Project.Server.Schema.AnalysisDataSet;

namespace RADiscoveryUnitTest
{
    [TestClass]
    public class JobUnitTest
    {
        [TestInitialize]
        public void Init()
        {
            try
            {
                RALogger.ConfigFile = "TimerLog4net.config";
                var logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
                RMGlobalConfiguration.Init();
                string installPath = AppDomain.CurrentDomain.BaseDirectory;
                WindsorContainer windsorContainer = new WindsorContainer();
                windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                    Path.Combine(installPath, "Castle/ServiceCastle.config")));
                var selector = windsorContainer.Resolve<IModelInterceptorsSelector>("AvePoint.RA.Common.Audit.AuditInterceptorSelector");
                windsorContainer.Kernel.ProxyFactory.AddInterceptorSelector(selector);
                PlatformWindsorManager.SetUp(windsorContainer);
                AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
                TenantLocalValue.LogonGroupId = "35226de4-9d1c-44df-8dd9-2b419109e93c";
                //TenantLocalValue.LogonGroupId = "f1a44437-8070-4d86-80ed-b4d587cdd3d3";
                //f1a44437-8070-4d86-80ed-b4d587cdd3d3
                StorageApiConfiguration.Setup();
                ISettingProfileService SettingProfileService = PlatformWindsorManager.GetService<ISettingProfileService>();
                byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
                CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
            }
            catch (Exception e)
            {

            }


        }

        [TestMethod]
        public void TestMeta()
        {
            try
            {
                var meta = "      \u001c~\u0001          \u001a<Data version=\"1.0\"><Field name=\"DocProperty\" type=\"System.Collections.Generic.Dictionary`2[[System.String],[System.Object]]\"><Field name=\"Id\" type=\"guid\">27db0ebd-e441-4787-9663-963d8fe5dc38</Field><Field name=\"UIVersion\" type=\"int\">512</Field><Field name=\"DoclibRowId\" type=\"int\">2</Field><Field name=\"IsCurrentVersion\" type=\"bool\">True</Field><Field name=\"Level\" type=\"byte\">1</Field><Field name=\"TimeCreated\" type=\"datetime\">638866094040000000</Field><Field name=\"TimeLastModified\" type=\"datetime\">638866094040000000</Field><Field name=\"MetaInfo\" type=\"binary\">qKkwMQwAAAD/CQAAeJytVm1v2zYQ/m7A/0HwMKADLJt6tw0MmJO4qQsvbeME7YoCBSVSNhGKVEnKjjb3v+8oyYnTtcPQ1TZs+ngvD+/uOWpbl1RxJu5yRjmZrd8etsaUejYe7/f7UYoZqUaZLMZDp1n3e9lWsow+aJ83fx2v3zuXlTCqfqWu6YZJ0Wxe9Xs7wz6WWGmqdlTpZuP64MUjBG8/9lHY7xFsqGHFo1Mf+ZGLYtef3CA0az7vIXKlFBVZ3apdWLUROn21wQpJGKiQtLaR2AyxkTl0GoFrv12U556bUbs6eR1wWf6mt1jRUjJhWm8k1dmWFvhL8GEQjbqAmeRSGbyxyPu9tYHT2OXL5WrZnT+XnIB9lbYLZmiR2WzNlteHzolhhjdm/d5rqQ3m55J0/+02rsxWqv97oI+Le0MFoeSC6kyx0nR1OkX5JbgKKtdmfPn24Pn9Xk21VEK2srO3B6Mq2u+xAm8eC/jXIGecXuGCDmaDC6YzCfmrnfWbFbgflWIzGA6kYhsmMF9ay1ZVVJwPB22rXFOODdvRW8XBx1iDoR7fUFysYYW8sf2Za02NHq+Yhu9oQsJ4GlOXRIS4IQ0jd5oFvpsTGgc4mQTTkIwfsPzsoydoGIEoaerTzNpEHkncECWxmwZR4gZp5mdJNommNB8cAbbAjmzRfmTuRo/JtqQBVbOtilRgxq9t3hVV7SE/Q9MzXXJcf6yU+DBre0y7BcuU1DI3Lph/mMk8B3Idf396LAWkeIWLlCrj3IIQNfQrSs6wyNqsLBulfk9A/KMVUMZ7yhj0XTjmXTM+glhvqeiaCOpuYKbYXXPfkciSm2Nt9gKgQWfdXB/8xHlZCccy3UHTWeDPgthpurc1YQAqpbxUspC2TY/8gyYEjXNm6nb6YLHJthUEXxtFqbHCKznykTcZOu+YgPOIjdPu9XuvFNSgSQN6Ojq6MaXojtE9EzvMGbGgZzfvvoY0PCKFdipt6z7B4rxkoq6wcF5IQ7nzDP5u6ooSKM4vbaC9ZVkTBFpcCiPvaMPDBUv45Zpkf6plrq/erK92VRndzi9/Lze/2goLYK+5gey21UX3yIM3Ol88n8+nyTRO/CSKw8VkPo/D4CychknghwuvDaqrsoQ5pd+zcmkscXPMNTC3qLiBbFND781Ddx2FjpU6jdgBwJbpXGYN6hOyrzOpgL6uj4aDhYBRVluQwI4V6PKzSjNBtQY6rDrbtaxUZhXOIDcgfy5VgY2hZE6IsqqzwbeqOHQeEj10mhl74vdWsRNONjcYBMAl080lBovxLh7bE/C0QwVT5Y+rEMXJvRclSRh6YTBFkzCCcRElEfie74C+OGUcjgXOb8WdkHsBG7eCfaqgFD884lK/tp0oK81roDcEaEoFGz8+1kVL/25U/6cetjk5lglq31TlXwoG6paup95B1FyTIDtW8IunB6ttxV0fPSZ9qZcip/AgcMzKZ2srFYGrBG4Ii2gFrWAqAnZhMJr4aGL7Q2w6medHo9D7DBP4ScP/s8+b259mqi6NvmMwG7Bu50/c8QlmU6a//nTgBcHEPh5cUgmtaf2/tpeC86yN7nTAHOQgGAnfMYIXhJlvjWC4gl7UqWLklSoh4y+w3lJyxmWqzznFgja3dkf+vwEeoVPf</Field><Field name=\"HasStream\" type=\"int\">1</Field><Field name=\"LeafName\" type=\"string\">meta data test file.txt</Field><Field name=\"CustomizedPageStatus\" type=\"int\">0</Field><Field name=\"ComplianceTag\" type=\"string\" /><Field name=\"IsCheckOut\" type=\"bool\">False</Field><Field name=\"HasUniqueRoleAssignments\" type=\"bool\">False</Field></Field><Field name=\"DocData\" type=\"System.Collections.Generic.Dictionary`2[[System.String],[System.Object]]\"><Field name=\"#tp_ContentTypeId\" type=\"binary\">AQEAzvqpeWcnVk6KpkO0lHMk4Q==</Field><Field name=\"File_x0020_Type\" type=\"string\">txt</Field><Field name=\"textfield\" type=\"string\">text field value</Field><Field name=\"choicefield\" type=\"string\">Choice 1</Field><Field name=\"datetimefield\" type=\"datetime\">638866908000000000</Field><Field name=\"multipletextfield\" type=\"string\">multiple text field value</Field><Field name=\"userfield\" type=\"int\">12</Field><Field name=\"numberfield\" type=\"double\">1</Field><Field name=\"yesornofield\" type=\"bool\">True</Field><Field name=\"hyperlinkfield\" type=\"string\">https://www.baidu.com/</Field><Field name=\"hyperlinkfield#2\" type=\"string\">baidu</Field><Field name=\"currencyfield\" type=\"double\">20</Field><Field name=\"locationfield\" type=\"string\">{\"Score\":-20,\"EntityType\":\"LocalBusiness\",\"LocationSource\":\"Bing\",\"FormattedAddress\":\"No.2018, Xincheng Street, Changchun, JILIN\",\"LocationUri\":\"https://www.bingapis.com/api/v6/localbusinesses/YN4067x15774414390845394575\",\"Availability\":\"Unknown\",\"UniqueId\":\"https://www.bingapis.com/api/v6/localbusinesses/YN4067x15774414390845394575\",\"IsPreviouslyUsed\":false,\"Id\":\"https://www.bingapis.com/api/v6/localbusinesses/YN4067x15774414390845394575\",\"DisplayName\":\"Changchun Jinyuan Hotel (Jingyuedian)\",\"Address\":{\"Street\":\"No.2018, Xincheng Street\",\"City\":\"Changchun\",\"State\":\"JILIN\",\"CountryOrRegion\":\"CN\",\"Type\":\"Unknown\",\"IsInferred\":false},\"Coordinates\":{\"Latitude\":43.8208,\"Longitude\":125.41}}</Field><Field name=\"CountryOrRegion\" type=\"string\">CN</Field><Field name=\"State\" type=\"string\">JILIN</Field><Field name=\"City\" type=\"string\">Changchun</Field><Field name=\"Street\" type=\"string\">No.2018, Xincheng Street</Field><Field name=\"GeoLoc\" type=\"string\">Point (125.41 43.8208 0 0)</Field><Field name=\"DispName\" type=\"string\">Changchun Jinyuan Hotel (Jingyuedian)</Field><Field name=\"imagefield\" type=\"string\">{\"fileName\":\"Discovery SQLite.png\",\"originalImageName\":null,\"serverRelativeUrl\":\"/sites/TeamSite01/SiteAssets/Lists/58d4696e-d5dd-4e45-9c32-fde63a78394d/Discovery%20SQLite.png\",\"id\":\"bb2ecc32-51d7-4076-b357-3bc2c7c859ef\",\"serverUrl\":\"https://s25tk.sharepoint.com\",\"thumbnailRenderer\":null}</Field><Field name=\"#tp_ID\" type=\"int\">2</Field><Field name=\"Created\" type=\"datetime\">638866092950000000</Field><Field name=\"Author\" type=\"int\">7</Field><Field name=\"Modified\" type=\"datetime\">638866094040000000</Field><Field name=\"Editor\" type=\"int\">7</Field><Field name=\"#tp_ModerationStatus\" type=\"int\">0</Field><Field name=\"#tp_Level\" type=\"int\">1</Field><Field name=\"#tp_IsCurrentVersion\" type=\"bool\">True</Field><Field name=\"AppEditor\" type=\"string\">5;AvePoint Online Services Administration for Microsoft365</Field><Field name=\"#tp_UIVersion\" type=\"int\">512</Field><Field name=\"#tp_UIVersionString\" type=\"string\">1.0</Field><Field name=\"#tp_ItemOrder\" type=\"double\">100</Field><Field name=\"#tp_GUID\" type=\"guid\">20888e14-b617-40cb-b01d-62be29016234</Field></Field><Field name=\"DocDataJunction\" type=\"System.Collections.Generic.List`1[[System.Collections.Generic.Dictionary`2[[System.String],[System.Object]]]]\" /><Field name=\"DocStorageInfo\" type=\"AvePoint.Wrapper.Common.AveStorageInfo\"><Field name=\"Size\" type=\"long\">0</Field><Field name=\"StorageType\" type=\"AvePoint.Wrapper.Common.AveStorageType\">None</Field><Field name=\"IsBackupLinkForArchivedData\" type=\"bool\">False</Field><Field name=\"StubDataType\" type=\"AvePoint.Wrapper.Common.AveStubDataType\">UnKnown</Field></Field><Field name=\"DocVersions\" type=\"System.Collections.Generic.List`1[[System.Int32]]\"><Field type=\"int\">512</Field></Field><Field name=\"GroupCache\" type=\"AvePoint.Wrapper.Common.AveGroupList\"><Field name=\"Groups\" type=\"System.Collections.Generic.List`1[[AvePoint.Wrapper.Common.AveGroupInfo]]\" /></Field><Field name=\"FullTextIndex\" type=\"AvePoint.Wrapper.Backup.FullTextIndex\"><Field name=\"&lt;Created&gt;k__BackingField\" type=\"datetime\">0</Field><Field name=\"&lt;Modified&gt;k__BackingField\" type=\"datetime\">0</Field><Field name=\"&lt;TimeZoneInfoID&gt;k__BackingField\" type=\"string\">Pacific Standard Time</Field><Field name=\"&lt;ArchiveBy&gt;k__BackingField\" type=\"string\">lambert.shen@avepoint.com</Field><Field name=\"&lt;ArchiveTime&gt;k__BackingField\" type=\"datetime\">638866902398918596</Field><Field name=\"&lt;Size&gt;k__BackingField\" type=\"int\">0</Field><Field name=\"&lt;ContentTypeName&gt;k__BackingField\" type=\"string\" /><Field name=\"&lt;Attachments&gt;k__BackingField\" type=\"System.Collections.Generic.List`1[[System.String]]\" /><Field name=\"&lt;ColumnValues&gt;k__BackingField\" type=\"System.Collections.Generic.Dictionary`2[[System.String],[System.Object]]\" /></Field></Data>";
                var bytes = Encoding.UTF8.GetBytes(meta);
                var newBytes = new byte[bytes.Length - 20];
                Array.Copy(bytes, 20, newBytes, 0, bytes.Length - 20);
                using var streamReader = new StreamReader(new MemoryStream(newBytes));
                var metadataReader = new AveMemoryMetadataReader(streamReader);
                AveMetadata metadata;
                while((metadata = metadataReader.ReadMetadata()) != null) 
                {
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DocData:

                            var data = metadata.GetMetadata<Dictionary<string, object>>();
                            var metainfo = data["MetaInfo"] as byte[];
                            //var metainfobytes = Encoding.UTF8.GetBytes(metainfo);
                            var t = GetTDecompressedString(metainfo); 
                            break;
                        default:
                            break;
                    }
                }

            }
            catch(Exception e)
            {

            }
            
        }

        public static string GetTDecompressedString(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length < 12)
            {
                throw new ArgumentException("Invalid compressed data.");
            }

            int len = 0;
            for (int i = 3; i >= 0; --i)
            {
                len <<= 8;
                len += compressedData[i + 8];
            }

            byte[] temp = new byte[len];
            using (MemoryStream ms = new MemoryStream(compressedData, 12, compressedData.Length - 12))
            {
                using (ZLibStream ds = new ZLibStream(ms, CompressionMode.Decompress))
                {
                    ds.Read(temp, 0, len);
                }
            }

            return Encoding.UTF8.GetString(temp);
        }

        [TestMethod]
        public void TestPrivateChannelSetting()
        {
            IRMArchiverSettingDao _archiverSettingDao = PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
            var setting = _archiverSettingDao.LoadSiteArchiverSettingByUrl("https://s25tk.sharepoint.com/sites/ChannelTeamTest-PrivateChannel01");
        }

        [TestMethod]
        public void XmlTest()
        {
            var xml = "<RMDiscoveryOptimizationSetting xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.Discovery.Model.Configuration\">\r\n  <ArchiveDataType>1</ArchiveDataType>\r\n  <DataType>1</DataType>\r\n  <FileExtensionQueryParameter xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.Discovery.Model.Query.Parameter\">\r\n    <d2p1:FileExtensions xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />\r\n  </FileExtensionQueryParameter>\r\n  <InactiveRuleQueryParameter>\r\n    <Enable>false</Enable>\r\n    <RuleIds xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />\r\n  </InactiveRuleQueryParameter>\r\n  <MoveToAnotherTierType>0</MoveToAnotherTierType>\r\n  <NextTime>0</NextTime>\r\n  <NodeIds xmlns:d2p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" i:nil=\"true\" />\r\n  <NodeQueryParameter xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.Discovery.Model.Query.Parameter\">\r\n    <d2p1:ContainerIds xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">\r\n      <d3p1:int>1</d3p1:int>\r\n    </d2p1:ContainerIds>\r\n    <d2p1:IsDesc>false</d2p1:IsDesc>\r\n    <d2p1:JoinedContainerId>0</d2p1:JoinedContainerId>\r\n    <d2p1:PageIndex>0</d2p1:PageIndex>\r\n    <d2p1:PageSize>5</d2p1:PageSize>\r\n    <d2p1:SearchKey i:nil=\"true\" />\r\n    <d2p1:SiteIds xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />\r\n    <d2p1:SortBy i:nil=\"true\" />\r\n    <d2p1:ViewMode>Container</d2p1:ViewMode>\r\n  </NodeQueryParameter>\r\n  <O365TenantId>840daea4-6028-48c6-9233-e5d04ae79ff0</O365TenantId>\r\n  <ProcessActionParameter>\r\n    <DeleteRecords>false</DeleteRecords>\r\n    <FileAction>ArchiveAndRemove</FileAction>\r\n    <IsEnableLeaveStub>false</IsEnableLeaveStub>\r\n    <StubSettingDto xmlns:d3p1=\"http://schemas.datacontract.org/2004/07/AvePoint.GCommon.Contract.Server.StubSetting\">\r\n      <d3p1:Id i:nil=\"true\" />\r\n      <d3p1:IsDeclareStubAsRecords>false</d3p1:IsDeclareStubAsRecords>\r\n      <d3p1:LastModifiedTime i:nil=\"true\" />\r\n      <d3p1:Name i:nil=\"true\" />\r\n      <d3p1:StubContent i:nil=\"true\" />\r\n      <d3p1:StubCustomizeTags>0</d3p1:StubCustomizeTags>\r\n      <d3p1:StubType>0</d3p1:StubType>\r\n    </StubSettingDto>\r\n    <VersionAction>ArchiveAndRemoveVerison</VersionAction>\r\n  </ProcessActionParameter>\r\n  <ROTRuleQueryParameter>\r\n    <Enable>true</Enable>\r\n    <RuleCategories xmlns:d3p1=\"http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.Discovery.Model.Query.Parameter\">\r\n      <d3p1:RMDiscoveryROTRuleCategoryQueryParameter>\r\n        <d3p1:Checked>true</d3p1:Checked>\r\n        <d3p1:RuleCategory>2</d3p1:RuleCategory>\r\n        <d3p1:RuleIds xmlns:d5p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />\r\n      </d3p1:RMDiscoveryROTRuleCategoryQueryParameter>\r\n      <d3p1:RMDiscoveryROTRuleCategoryQueryParameter>\r\n        <d3p1:Checked>true</d3p1:Checked>\r\n        <d3p1:RuleCategory>3</d3p1:RuleCategory>\r\n        <d3p1:RuleIds xmlns:d5p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />\r\n      </d3p1:RMDiscoveryROTRuleCategoryQueryParameter>\r\n      <d3p1:RMDiscoveryROTRuleCategoryQueryParameter>\r\n        <d3p1:Checked>true</d3p1:Checked>\r\n        <d3p1:RuleCategory>4</d3p1:RuleCategory>\r\n        <d3p1:RuleIds xmlns:d5p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" />\r\n      </d3p1:RMDiscoveryROTRuleCategoryQueryParameter>\r\n    </RuleCategories>\r\n  </ROTRuleQueryParameter>\r\n  <ScheduleParameter>\r\n    <ScheduleType>Date</ScheduleType>\r\n    <SelectedTime>2026-01-30T10:33:00</SelectedTime>\r\n    <StartTime>2026-01-30T02:33:00Z</StartTime>\r\n    <TimeZoneId>China Standard Time</TimeZoneId>\r\n  </ScheduleParameter>\r\n  <SelectedStorage xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/AvePoint.GCommon.Contract.Storage.Entity\">\r\n    <d2p1:ArchiveRetentionRules xmlns:d3p1=\"http://www.avepoint.com\">\r\n      <d3p1:RetentionRule>\r\n        <d3p1:ActionGroupName i:nil=\"true\" />\r\n        <d3p1:CanSetMoveTo>false</d3p1:CanSetMoveTo>\r\n        <d3p1:DeleteTheData>false</d3p1:DeleteTheData>\r\n        <d3p1:EnableRetentionVisibility>false</d3p1:EnableRetentionVisibility>\r\n        <d3p1:ErrorMessage i:nil=\"true\" />\r\n        <d3p1:GroupTitleHeader i:nil=\"true\" />\r\n        <d3p1:HeaderVisibility>false</d3p1:HeaderVisibility>\r\n        <d3p1:IsMove>false</d3p1:IsMove>\r\n        <d3p1:KelpTitle i:nil=\"true\" />\r\n        <d3p1:LogicalDescription i:nil=\"true\" />\r\n        <d3p1:LogicalName i:nil=\"true\" />\r\n        <d3p1:MoToDeviceHeader i:nil=\"true\" />\r\n        <d3p1:MoveLogicalDeviceDto i:nil=\"true\" />\r\n        <d3p1:MoveToLogicalDtos i:nil=\"true\" />\r\n        <d3p1:MoveToValidationMsg i:nil=\"true\" />\r\n        <d3p1:RemoveTheJob>false</d3p1:RemoveTheJob>\r\n        <d3p1:RetentionIndex>0</d3p1:RetentionIndex>\r\n        <d3p1:SetupDataRetention>true</d3p1:SetupDataRetention>\r\n        <d3p1:ArchiveDateUnit>Day</d3p1:ArchiveDateUnit>\r\n        <d3p1:DeleteTheData>true</d3p1:DeleteTheData>\r\n        <d3p1:IsArchivedTier>false</d3p1:IsArchivedTier>\r\n        <d3p1:IsFitSoftDelete>false</d3p1:IsFitSoftDelete>\r\n        <d3p1:IsMarkDataTier>false</d3p1:IsMarkDataTier>\r\n        <d3p1:IsMove>false</d3p1:IsMove>\r\n        <d3p1:IsSoftDelete>false</d3p1:IsSoftDelete>\r\n        <d3p1:KeepOrphanedStub4CompatibilityExistingRule>false</d3p1:KeepOrphanedStub4CompatibilityExistingRule>\r\n        <d3p1:KeepValue>20</d3p1:KeepValue>\r\n        <d3p1:KeepValueErrorMessage i:nil=\"true\" />\r\n        <d3p1:MoveDeviceId />\r\n        <d3p1:RemoveOrphanedStub>true</d3p1:RemoveOrphanedStub>\r\n        <d3p1:RemoveTheJob>false</d3p1:RemoveTheJob>\r\n        <d3p1:RetentionDataTimeType>ArchiveTime</d3p1:RetentionDataTimeType>\r\n        <d3p1:SoftDeleteDateUnit>Day</d3p1:SoftDeleteDateUnit>\r\n        <d3p1:SoftDeleteKeepValue>0</d3p1:SoftDeleteKeepValue>\r\n        <d3p1:TakeEffectToExistingData>false</d3p1:TakeEffectToExistingData>\r\n        <d3p1:TierType>0</d3p1:TierType>\r\n      </d3p1:RetentionRule>\r\n    </d2p1:ArchiveRetentionRules>\r\n    <d2p1:CompressionSpeed>5</d2p1:CompressionSpeed>\r\n    <d2p1:ConnectionString i:nil=\"true\" />\r\n    <d2p1:DAOLogicalDeviceId i:nil=\"true\" />\r\n    <d2p1:DAOMigrated>false</d2p1:DAOMigrated>\r\n    <d2p1:DAOPhysicalDeviceId i:nil=\"true\" />\r\n    <d2p1:DAOStoragePolicyId i:nil=\"true\" />\r\n    <d2p1:Description i:nil=\"true\" />\r\n    <d2p1:EncryptionProfileId i:nil=\"true\" />\r\n    <d2p1:Extension>\r\n      <d2p1:TotalSpace>0</d2p1:TotalSpace>\r\n      <d2p1:UsedSpace>0</d2p1:UsedSpace>\r\n    </d2p1:Extension>\r\n    <d2p1:FreeSpace>0</d2p1:FreeSpace>\r\n    <d2p1:Id>f7bd8b5c-4e16-4683-aa1c-9bff81ad5d1f</d2p1:Id>\r\n    <d2p1:IsSystemStorage>false</d2p1:IsSystemStorage>\r\n    <d2p1:IsUsingDevice>false</d2p1:IsUsingDevice>\r\n    <d2p1:LastArchivedTime>19-03-2025 15:40:46  (UTC+08:00)</d2p1:LastArchivedTime>\r\n    <d2p1:LastModifiedTime>25-06-2024 10:43:45  (UTC+08:00)</d2p1:LastModifiedTime>\r\n    <d2p1:Name>Test</d2p1:Name>\r\n    <d2p1:NotificationId i:nil=\"true\" />\r\n    <d2p1:Schedule xmlns:d3p1=\"http://www.avepoint.com\" i:nil=\"true\" />\r\n    <d2p1:SetupDataRetention>true</d2p1:SetupDataRetention>\r\n    <d2p1:SpaceType>0</d2p1:SpaceType>\r\n    <d2p1:StorageDeviceSpace>-1</d2p1:StorageDeviceSpace>\r\n    <d2p1:Type>403</d2p1:Type>\r\n    <d2p1:UseCompression>true</d2p1:UseCompression>\r\n    <d2p1:UseEncryption>false</d2p1:UseEncryption>\r\n    <d2p1:UseSpace>-1</d2p1:UseSpace>\r\n    <d2p1:mCurrentXRI>\r\n      <d2p1:Params xmlns:d4p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">\r\n        <d4p1:KeyValueOfstringstring>\r\n          <d4p1:Key>accesspoint</d4p1:Key>\r\n          <d4p1:Value>https://blob.core.windows.net</d4p1:Value>\r\n        </d4p1:KeyValueOfstringstring>\r\n        <d4p1:KeyValueOfstringstring>\r\n          <d4p1:Key>containername</d4p1:Key>\r\n          <d4p1:Value>storagebucket</d4p1:Value>\r\n        </d4p1:KeyValueOfstringstring>\r\n        <d4p1:KeyValueOfstringstring>\r\n          <d4p1:Key>name</d4p1:Key>\r\n          <d4p1:Value>recoaksstorage</d4p1:Value>\r\n        </d4p1:KeyValueOfstringstring>\r\n        <d4p1:KeyValueOfstringstring>\r\n          <d4p1:Key>secret</d4p1:Key>\r\n          <d4p1:Value>00000000-0000-0000-0000-000000000000</d4p1:Value>\r\n        </d4p1:KeyValueOfstringstring>\r\n        <d4p1:KeyValueOfstringstring>\r\n          <d4p1:Key>advanced</d4p1:Key>\r\n          <d4p1:Value>false</d4p1:Value>\r\n        </d4p1:KeyValueOfstringstring>\r\n        <d4p1:KeyValueOfstringstring>\r\n          <d4p1:Key>creation</d4p1:Key>\r\n          <d4p1:Value>true</d4p1:Value>\r\n        </d4p1:KeyValueOfstringstring>\r\n      </d2p1:Params>\r\n      <d2p1:VIM>azure_vim</d2p1:VIM>\r\n    </d2p1:mCurrentXRI>\r\n  </SelectedStorage>\r\n  <SizeRangeQueryParameter xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.Discovery.Model.Query.Parameter\">\r\n    <d2p1:QueryMode>GenerateThanEqual</d2p1:QueryMode>\r\n    <d2p1:SizeRange>3</d2p1:SizeRange>\r\n  </SizeRangeQueryParameter>\r\n  <WithoutDateQueryParameter xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/AvePoint.RA.Contract.Discovery.Model.Query.Parameter\">\r\n    <d2p1:From>-1</d2p1:From>\r\n    <d2p1:To>999</d2p1:To>\r\n  </WithoutDateQueryParameter>\r\n</RMDiscoveryOptimizationSetting>";
            var newXml = RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(xml);
            var t= SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(newXml);
        }

        [TestMethod]
        public async Task GoogleTriggerTest()
        {
            try
            {
                await RMDiscoveryOffice365LicenseHelper.GetLicenseTypeAsync();
                //var trigger = new RMDiscoveryGoogleJobTrigger();
                //await trigger.TriggerAsync();
            }
            catch(Exception e)
            {

            }
        }

        [TestMethod]
        public async Task SqliteTest()
        {
            try
            {
                Process.GetProcessById(3584).Kill();
                //var context = RMDiscoveryGoogleSQLiteDBManager.GetContext();
            }
            catch(Exception e)
            {

            }
        }

        [TestMethod]
        public async Task DuplicateTest()
        {
            try
            {
                var a = new RMDiscoveryOffice365DuplicateCalculatorV4(new RMDiscoveryOffice365MainJob
                {
                    Id = new Guid("3EAE4538-0478-423B-A1EF-D5DE9517195D")
                });
                await a.CalculateAsync();
            }
            catch(Exception e)
            {

            }
        }

        [TestMethod]
        public async Task GoogleMonitorTest()
        {
            try
            {
                var trigger = new RMDiscoveryGoogleJobMonitor();
                await trigger.MonitorAsync();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task TestBB()
        {
            var retentionIndexSubInfoDao = PlatformWindsorManager.GetService<IRetentionIndexSubInfoDao>();
            var jobs = await retentionIndexSubInfoDao.GetRetentionInfoesAsync();
            foreach(var job in jobs)
            {
                await retentionIndexSubInfoDao.DeleteAsync(job);
            }
        }

        [TestMethod]
        public async Task ExtractFileTest() 
        {
            try
            {
                var k = 0L;
                var j = 3221225462L;
                k = (long)Math.Ceiling((j + 0.0) / 1024 / 1024 / 1024);
            }
            catch(Exception e)
            {

            }
            
        }

        [TestMethod]
        public async Task WaitUnitTest()
        {
            try
            {
                var task = Task.Run(() =>
                {
                    while(true)
                    {
                        Task.Delay(1000);
                    }
                });
                await task.WaitAsync(TimeSpan.FromSeconds(20));
            }
            catch(Exception e)
            {

            }
        }

        [TestMethod]
        public void SalesForceTest() 
        {

            RASalesforce.APIs.Tester.GetAccountAsync().GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task OdataTest()
        {
            try
            {
                var ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
                await ieApiClient.DatabaseManagementService.CreateIndexAsync(new IndexCreationModel
                {
                    DataType = DataType.SPDocument,
                    Office365TenantId = "840DAEA4-6028-48C6-9233-E5D04AE79FF0",
                    Indexes = new List<IndexModel>
                    {
                        new IndexModel
                        {
                            Name = "Compound_FileSize",
                            Definition = JsonConvert.SerializeObject(new Dictionary<string, int>
                            {
                                {"FileSize", 1 },
                                { "_id", 1 }
                            })
                        }
                    }
                });
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[SourceFlag.SharePoint]}?" +
                    $"$top={100}" +
                    $"&$filter=FileSize ge {1} " +
                    $"and not IsPHL " +
                    $"&$orderby=FileSize" +
                    $"&select=Name,FileSize";
                var dataJson = await ieApiClient.GetByODataUrlWithRetryAsync(sql, "840DAEA4-6028-48C6-9233-E5D04AE79FF0");
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task Test()
        {
            try
            {
                uint i = 1;
                var j = i - 5;

                var k = (i - 5) % 4;

                //var analyzer = new RMDiscoveryAnalyzer(SourceFlag.SharePoint, new()
                //{
                //    SharePointOnlineSiteSizeLimit = 0,
                //    OneDriveSiteSizeLimit = 0
                //}, new()
                //{
                //    Id = new Guid("D0B7868F-53F6-4F75-B87C-C2589610BE8F"),
                //    MainJobId = new Guid("EE988A49-7FA6-4B05-B580-35985C65466B"),
                //    DiscoveryJobId = new Guid("0877CF12-E7A0-4185-9466-4E2DBB1BCB7A"),
                //    O365TenantId = new Guid("CA6DF833-D301-40F4-BED5-9B47D599E223"),
                //    ContainerId = new Guid("CC73ADF0-A52B-41C0-BF78-7F2E3A6832A9"),
                //    SiteId = new Guid("B9267908-C1A2-4AC4-A0FA-8C9783E40820"),
                //    Url = "https://m365x38718414-my.sharepoint.com",
                //});
                //await analyzer.AnalysisAsync();
                //var sql = $"odata/sponedrivedocuments?$filter=SiteId eq 'bb338f49-a17c-400a-82c4-73d900a00e4e' and not IsPHL&$orderby=CreatedMonth asc&$top=1";
                //var ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
                //var res = await ieApiClient.GetByODataUrlAsync(sql, "CA6DF833-D301-40F4-BED5-9B47D599E223");
                //var maxAgeDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(res).FirstOrDefault();
                //var createTime = maxAgeDataObj.GetValue<long>("createdMonth");
                //var i = new RMDiscoveryProjectionAnalyzer();
                //await i.AnalyzeAsync();
                //var trigger = new RMDiscoveryJobTrigger();
                //await trigger.RegisterTagsAsync();
                //var usage = new RMGraphUsageReportManager("CA6DF833-D301-40F4-BED5-9B47D599E223");
                //await usage.Test();
                //await RMDiscoveryDBManager.UpgradeTablesAsync();
                //var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                //var queryResult = await client.RemoteNodeService.QueryRemoteNodesAsync(new RemoteNodesQueryParameter
                //{
                //    TenantId = "ca6df833-d301-40f4-bed5-9b47d599e223",
                //    NodeTypes = new() { RemoteNodeType.Office365Group },
                //    ContainerId = "1202f58a-739f-44bd-8f85-12d5eec92e88"
                //});
            }
            catch(Exception e)
            {

            }
            
        }

        [TestMethod]
        public async Task StorageUsageTestAsync()
        {
            //var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            //var o365Tenants = await client.Office365TenantService.GetSharePointSiteUsageStorageAsync();
            //var usageReport = o365Tenants.First().UsageStorage;
            //var reports = usageReport.Split("\r\n");
            //var latestReports = reports[1].Split(",");
            //var usageBytes = Convert.ToInt64(latestReports[2]);
            var executor = new UpdateAosStatisticsSizeExecutor();
            executor.UpdateSizeToAOS();
        }

        [TestMethod]
        public async Task InitDataAsync()
        {
            try
            {
                await RMDiscoveryDBManager.InitOffice365BuildInDataListAsync();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task TestPreparerAsync()
        {
            try
            {
                //var ticks = DateTime.UtcNow.AddDays(-5).Ticks;
                //var preparer = new RMDiscoveryJobPreparer(true);
                //await preparer.PrepareAsync();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task TestTriggerAsync()
        {
            try
            {
                //var trigger = new RMDiscoveryJobTrigger();
                //await trigger.TriggerAsync();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task TestMonitorAsync()
        {
            try
            {
                var cacheManager = new RMDiscoveryCacheManager(new Guid("CA6DF833-D301-40F4-BED5-9B47D599E223"), RMDiscoveryCacheDataSource.Office365);
                await cacheManager.ClearAsync();
                //var monitor = new RMDiscoveryJobMonitor();
                //await monitor.MonitorAsync();
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task TestDBSchema()
        {
            try
            {
                using (var context = await RMDiscoveryDBManager.GetEFContextAsync())
                {
                    await context.Office365AnalysisJobs.ToListAsync();
                }

                using (var context = await RMDiscoveryDBManager.GetOffice365EFContextAsync(new Guid("CA6DF833-D301-40F4-BED5-9B47D599E223")))
                {
                    await context.Office365SiteInfoes.ToListAsync();
                }
            }
            catch (Exception e)
            {

            }
        }

        [TestMethod]
        public async Task JobTest()
        {
            var runner = new RMArchivedFullTextIndexJobRunner("test123");
            await runner.RunAsync();
        }

        [TestMethod]
        public async Task DDD()
        {
            var localPath = System.Environment.CurrentDirectory;

            var localDeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives = new List<PhysicalDeviceDto>
                    {
                        PhysicalDeviceDto.GenterateFS(localPath, string.Empty, string.Empty)
                    }
            };
            var localSystem = XFactoryCommon.InstanceSystem(localDeviceDto.ToXRIS().First());
            localSystem.Open();
            var path = localSystem.SystemPath;
            var tempPath = localSystem.LocalTempPath;

            var buffer = new Byte[1024];
            using (var localStream = localSystem.OpenStream(new StorageInfo("index_db_cache", "abc.txt"), FileMode.CreateNew))
            {
                localStream.Write(System.Text.Encoding.UTF8.GetBytes("abc"), 0, System.Text.Encoding.UTF8.GetBytes("abc").Length);
                localStream.Write(System.Text.Encoding.UTF8.GetBytes(Environment.NewLine), 0, System.Text.Encoding.UTF8.GetBytes(Environment.NewLine).Length);
                localStream.Write(System.Text.Encoding.UTF8.GetBytes("abc"), 0, System.Text.Encoding.UTF8.GetBytes("abc").Length);
            }
        }

        [TestMethod]
        public async Task ReadIndexDbTestAsync()
        {
            try
            {
                var storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>(); 
                var deviceInfo = storageDeviceService.GetIndexDevice();
                
                //var indexLogicalDeviceDto = new LogicalDeviceDto
                //{
                //    PhysicalDrives = new List<PhysicalDeviceDto>
                //    {
                //        new PhysicalDeviceDto
                //        {
                //            Id = deviceInfo.Id,
                //            ConnectionString = deviceInfo.ConnectionString,
                //            ModifyTime = deviceInfo.ModifyTime,
                //            Type = deviceInfo.Type
                //        }
                //    }
                //};

                var indexLogicalDeviceDto = ConvertStorageDeviceDtoToLogicalDeviceDto(deviceInfo);

                var xri = indexLogicalDeviceDto.ToXRIS();
                var vim = XRI.ValueOf(xri.First()).VIM;

                var name = ConnectionBuilder.ValueOf(xri.First()).StorageName;

                //var deviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
                //deviceManager.Open(new() { "docave-xam://azure_vim?accesspoint=https://blob.core.windows.net&containername=storagebucket&name=recoaksstorage&secret=DVpdvGe1asAZs7Y+J9T8I6/532Ogt8BQacdzsv4kXppxlGjJCPOcSbdnJo5YeEcBVuVZoMu+E/95cO8Waqbx8Ee008gqtbCBP4S6hh9TIJ7nzmmKB0Ax89UbDKqLwYQxoAmdI6EqvjXSfyYdOa0VYQ%3D%3D&advanced=false&creation=true&id=f7bd8b5c-4e16-4683-aa1c-9bff81ad5d1f&modifytime=638451195511514081" });
                //var indexLogicalDevice = XFactoryCommon.InstanceLibrary(xri);
                //indexLogicalDevice.Open();
                var indexLogicalSystemDevice = XFactoryCommon.InstanceSystem("docave-xam://azure_vim?accesspoint=https://blob.core.windows.net&containername=storagebucket&name=recoaksstorage&secret=DVpdvGe1asAZs7Y+J9T8I6/532Ogt8BQacdzsv4kXppxlGjJCPOcSbdnJo5YeEcBVuVZoMu+E/95cO8Waqbx8Ee008gqtbCBP4S6hh9TIJ7nzmmKB0Ax89UbDKqLwYQxoAmdI6EqvjXSfyYdOa0VYQ%3D%3D&advanced=false&creation=true");
                indexLogicalSystemDevice.Open();

                var volumeGenerator = new ArchiverVolumeGenerator();
                var volumePara = new VolumeParameter
                {
                    FarmName = "",
                    SiteCollectionUrl = "https://m365x38718414.sharepoint.com/sites/TeamSite01"
                };

                var indexVolume = volumeGenerator.GenerateIndexVolume(volumePara);
                var indexDbName = "index.db";
                var logicalIndexStorageInfo = XConvert.FromNames(indexVolume, indexDbName, "");
                var exist = indexLogicalSystemDevice.FileExists(logicalIndexStorageInfo);

                var localPath = System.Environment.CurrentDirectory;

                var localDeviceDto = new LogicalDeviceDto
                {
                    PhysicalDrives = new List<PhysicalDeviceDto>
                    {
                        PhysicalDeviceDto.GenterateFS(localPath, string.Empty, string.Empty)
                    }
                };
                var localSystem = XFactoryCommon.InstanceSystem(localDeviceDto.ToXRIS().First());
                localSystem.Open();
                var path = localSystem.SystemPath;
                var tempPath = localSystem.LocalTempPath;

                var buffer = new Byte[1024];
                using (var downloader = indexLogicalSystemDevice.OpenStream(logicalIndexStorageInfo, FileMode.Open))
                {
                    using (var localStream = localSystem.OpenStream(new StorageInfo("index_db_cache", "index.db"), FileMode.CreateNew))
                    {
                        var readLen = 0;
                        while ((readLen = downloader.Read(buffer, 0, 1024)) > 0)
                        {
                            localStream.Write(buffer, 0, readLen);
                        }
                        localStream.Flush();
                    }
                }
                

                var dbHelper = new IndexDatabaseHelper();
                var key = new SettingProfileService().GetDBSEEMasterKey();
                dbHelper.Open(@"C:\Users\lambert.shen\Desktop\reco_product\reco-release\RADiscoveryUnitTest\bin\Debug\net8.0\index_db_cache\index.db", key);

                //dbHelper.Exe

                //using var localStream = await localSystem.OpenStreamAsync();
                //await indexLogicalSystemDevice.DownloadFileAsync(logicalIndexStorageInfo, localStream);
                //await localStream.FlushAsync();
            }
            catch(Exception e)
            {

            }
        }

        private LogicalDeviceDto ConvertStorageDeviceDtoToLogicalDeviceDto(StorageDeviceDto storageDevice)
        {
            if (storageDevice == null) { return null; }
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
            };

            var logical = new LogicalDeviceDto();
            logical.PhysicalDrives = new List<PhysicalDeviceDto>
            {
                physical
            };
            return logical;
        }

        [TestMethod]
        public async Task UpdateGroupMailboxForAllSiteMasterIndexTestAsync()
        {
            var RMNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
            var ArchiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
            try
            {
                var listerror = new List<string>();

                var teamsIdMailboxMapping = RMNodeDao.GetAllTeamId2TeamNameMapping();

                await foreach (var siteUrlBatch in ArchiverSiteMasterIndexDao.GetAllSiteDistinctUrlAsync())
                {
                    var addressSiteUrlsdic = RMNodeDao.GetGroupAddressAndRelatedSiteUrlsDic(siteUrlBatch, teamsIdMailboxMapping) ?? [];

                    foreach (var item in addressSiteUrlsdic)
                    {
                        if (string.IsNullOrEmpty(item.Key))
                        {
                            listerror.AddRange(item.Value);
                            continue;
                        }
                        ArchiverSiteMasterIndexDao.UpdateGroupMailboxAddressBySiteURL(item.Value, item.Key);
                    }
                }

                //Console.WriteLine(listerror);
            }
            catch (Exception ex)
            {

            }
        }

        //[TestMethod]
        //public async Task ReadDataDbTestAsync()
        //{
        //    try
        //    {

        //        var volumeGenerator = new ArchiverVolumeGenerator();
        //        var volumePara = new VolumeParameter
        //        {
        //            FarmName = "",
        //            SiteCollectionUrl = "https://m365x38718414.sharepoint.com/sites/TeamSite01"
        //        };
        //        var dataVolume = volumeGenerator.GenerateDataVolume(volumePara);

        //        var restoreMain = new AveItemRestoreMain();

        //        IArchiverSiteMasterIndexService ArchiverSiteMasterIndexService = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        //        IStorageDeviceService StorageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
        //        var  index = ArchiverSiteMasterIndexService.GetSiteCollectionStorageInfo(new()
        //        {
        //            SiteId = "1717d53f-2d8c-4780-9df8-2d058a256001",
        //            SiteURL = "https://m365x38718414.sharepoint.com/sites/TeamSite01"
        //        });
        //        var indexWithSubInfoes = ArchiverSiteMasterIndexService.GetSiteCollectionWithSubInfos(new()
        //        {
        //            SiteId = "1717d53f-2d8c-4780-9df8-2d058a256001",
        //            SiteURL = "https://m365x38718414.sharepoint.com/sites/TeamSite01"
        //        });
        //        var securityInfoes = restoreMain.GetRestoreSecurityInfoList(indexWithSubInfoes);

        //        var dataLogicalDeviceList = index.SubInfo.Select(item => ConvertStorageDeviceDtoToLogicalDeviceDto(
        //                StorageDeviceService.GetStorageDeviceById(string.IsNullOrEmpty(item.CurrentStorageId) ? item.StorageInfo : item.CurrentStorageId)
        //            )).Where(a => a != null).ToList();
        //        var dataLogicalDevice = new LogicalDeviceDto
        //        {
        //            PhysicalDrives = dataLogicalDeviceList.Select(item => item.PhysicalDrives).SelectMany(item => item).ToList()
        //        };

        //        MediaConfigInfo.CommonConfigInfo = PlatformWindsorManager.GetService<CommonConfigInfo>();

        //        var dataListener = new TestDataListener(dataLogicalDevice, dataVolume);
        //        IMediaGeneralInputStream input = new FormatedInputStream(new OpenInputStreamParameter
        //        {
        //            DataListener = dataListener,
        //            IsSupportAutoChangeDataBlock = false
        //        });
        //        var converter = (IXConverter)input;
        //        dataListener.SetConverter(converter);
        //        input = new EncryptedFormatedInputStream(input);
        //        input = new CompressedFormatedInputStream(input);
        //        input.Open();

        //        var encryptionInfoManager = new EncryptionInfoManager();
        //        var encryptionInfoDic = encryptionInfoManager.PutEncryptionInfos(securityInfoes);
        //        input.SetEncryptionInfos(encryptionInfoDic);

        //        var dbHelper = new IndexDatabaseHelper();
        //        var key = "aes256:??_~D_;jL1U%*-K5(`IS";
        //        dbHelper.Open(@"C:\Users\lambert.shen\Desktop\reco_product\reco-release\RADiscoveryUnitTest\bin\Debug\net8.0\index_db_cache\index.db", key);
        //        var documentObjs = dbHelper.ExecuteReader<ArchiverBasicIndex>("SELECT * FROM TB_BODY_INDEX WHERE COL_TYPE = 'D'", new());
        //        var document = documentObjs[13];
        //        var str = input.NextItem(document);

        //        var metaData1 = new Byte[1048576];
        //        var actualLength = 0;

        //        if (input.HasMetaDataPart1)
        //        {
        //            input.BeginRead(FileType.MetaData);
        //            var readLen = 0;
        //            while((readLen = input.ReadMetaDataPart1(metaData1, 0, metaData1.Length)) > 0)
        //            {
        //                actualLength += readLen;
        //            }
        //            input.EndRead(FileType.MetaData);
        //        }

        //        var streamReader = new StreamReader(new MemoryStream(metaData1, 20, actualLength - 20));
        //        var mDoc = new XmlDocument();
        //        mDoc.Load(streamReader);
        //        var rootElement = (XmlElement)mDoc.FirstChild;
        //        var xmlElement = (XmlElement)rootElement.ChildNodes[0];
        //        //rootElement.RemoveChild(xmlElement);
        //        var metadata = new AveMetadata(xmlElement);
        //        var obj = metadata.GetMetadata<Dictionary<string, object>>();

        //        using (var fileStream = File.OpenWrite(@"C:\Users\lambert.shen\Desktop\reco_product\reco-release\RADiscoveryUnitTest\bin\Debug\net8.0\index_db_cache\test.txt"))
        //        {
        //            fileStream.Write(metaData1, 20, actualLength);
        //        }

        //        var contentData = new Byte[1048576];
        //        actualLength = 0;

        //        if (input.HasContent)
        //        {
        //            input.BeginRead(FileType.Content);
        //            var readLen = 0;
        //            while ((readLen = input.ReadContent(contentData, 0, contentData.Length)) > 0)
        //            {
        //                actualLength += readLen;
        //            }
        //            input.EndRead(FileType.Content);
        //        }

        //        using (var memoryStream = new MemoryStream(contentData, 0, actualLength))
        //        {
        //            var extractor = new Extractor();
        //            var content = await extractor.ExtractAsync(memoryStream, "txt");
        //        }

        //        using (var fileStream = File.OpenWrite(@"C:\Users\lambert.shen\Desktop\reco_product\reco-release\RADiscoveryUnitTest\bin\Debug\net8.0\index_db_cache\test1.txt"))
        //        {
        //            fileStream.Write(contentData, 0, actualLength);
        //        }

        //        var metaData2 = new Byte[1048576];
        //        actualLength = 0;

        //        if (input.HasMetaDataPart2)
        //        {
        //            input.BeginRead(FileType.MetaData);
        //            var readLen = 0;
        //            while ((readLen = input.ReadMetaDataPart2(metaData2, 0, metaData1.Length)) > 0)
        //            {
        //                actualLength += readLen;
        //            }
        //            input.EndRead(FileType.MetaData);
        //        }
        //        using (var fileStream = File.OpenWrite(@"C:\Users\lambert.shen\Desktop\reco_product\reco-release\RADiscoveryUnitTest\bin\Debug\net8.0\index_db_cache\test2.txt"))
        //        {
        //            fileStream.Write(metaData2, 0, actualLength);
        //        }

        //        input.EndItem();
        //    }
        //    catch(Exception e)
        //    {

        //    }
            
        //}
    }

    public class TestDataListener : IInputDataListener
    {
        private readonly IXSystem _dataLogicDevice;

        private readonly string _dataVolume;

        private IXConverter _converter;

        public TestDataListener(LogicalDeviceDto dataLogicDeviceDto, string dataVolume)
        {
            _dataLogicDevice = XFactoryCommon.InstanceLibrary(dataLogicDeviceDto.ToXRIS());
            _dataLogicDevice.Open();
            _dataVolume = dataVolume;
            
        }

        public void SetConverter(IXConverter converter)
        {
            _converter = converter;
        }

        public void CloseDataBlock(FileType fileType, string fileName, Stream stream)
        {
            stream.Close();
        }

        public XStream OpenDataBlock(DataBlockOpenParam param, out DataBlockOpenOutParam outParam)
        {
            outParam = new DataBlockOpenOutParam();
            outParam.FileName = GenerateFileName(param);
            var info = _converter.FormNames(param.FileType, _dataVolume, outParam.FileName);
            var fileInfo = _dataLogicDevice.OpenFile(info);
            outParam.FileSize = fileInfo.FileSize;
            _converter.SetFileSize(param.FileType, outParam.FileSize, false);

            info = _converter.FormNames(param.FileType, _dataVolume, outParam.FileName);
            return _dataLogicDevice.OpenStream(info, FileMode.Open);
        }

        public XStream OpenDataBlockForGetVersion(DataBlockOpenParam param)
        {
            StorageInfo info = new StorageInfo();
            info.LowName = GenerateFileName(param);
            info.HighName = _dataVolume;
            info.Offset = 0;
            info.Length = 4;
            return _dataLogicDevice.OpenStream(info, FileMode.Open);
        }

        public string GenerateFileName(DataBlockOpenParam param)
        {
            if(param.FileType == FileType.Content)
            {
                return param.JobId + "_content_" + param.FileNumber + ".dat";
            }
            return param.JobId + "_meta_" + param.FileNumber + ".dat";
        }
    }
}
