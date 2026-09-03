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
using System.Globalization;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Media.Storage.Resources.CAStorI18N;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.CAStor.CAStorFeature.#GenerateDocAveGUIFeatureUnit(System.Globalization.CultureInfo)", MessageId = "cr")]
namespace AvePoint.Media.Storage.CAStor
{
    class CAStorFeature : StorageFeature
    {

        #region CAStorFeature 自己负责初始化的部分, 在Application生存周期内只会初始化一次, 外部不需要知道的逻辑

        private CAStorFeature(int type, string culture)
        {
            this.Init(type, culture);
        }

        /// <summary>
        /// 实际初始化DocAve GUI Feature Unit的部分
        /// </summary>
        /// 

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Stor")]
        protected override void GenerateDocAveGUIFeatureUnit(CultureInfo culture)
        {
            StorageFeature sf = new StorageFeature();

            //定义StorageType
            StorageType dell = new StorageType();
            dell.Display = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_DELL_DX_Storage", culture);
            dell.Index = 5;
            dell.Value = "DELLDXStorage";
            dell.Vim = new List<string>() { "castor_vim" };
            dell.IsSupportMovableRetention = false;
            sf.Type = dell;
            sf.Type.DefaultXris = new List<string>() { "docave-xam://castor_vim?" };
            sf.IsNeedSpaceThreshold = true;
            sf.IsObjectType = true;
            sf.ProgressForeground = new FeatureColor(255, 190, 190, 190);

            sf.Features = new List<FeatureUnit>() { 
                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_CSN_Private_Network_IP",culture), Tag = "castor_primary_nodes", Key = "primaryNode".ToLower(CultureInfo.InvariantCulture), KeyName = "PrimaryNode", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox",DemoValue=@"10.2.6.170"},
                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_SCSP_Proxy_Port",culture), Tag = "castor_primary_port", Key = "primaryPort".ToLower(CultureInfo.InvariantCulture), KeyName = "PrimaryPort", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox",DemoValue=@"80"},
                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Cluster_Name",culture), Tag = "castor_cluster_name", Key = "CLUSTERNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "ClusterName", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox",DemoValue = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Cluster_Name_Demo", culture)},
                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Primary_DX_CR_Publisher",culture), Tag = "castor_cr_publisher", Key = "CRPUBLISHER".ToLower(CultureInfo.InvariantCulture), KeyName = "CrPublisher", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true"},
                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Primary_DX_CR_Publisher_Port",culture), Tag = "castor_cr_publisher_port", Key = "CRPUBLISHERPORT".ToLower(CultureInfo.InvariantCulture), KeyName = "CrPublisherPort", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true"},
                
                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_With_Remote_D_R_Cluster",culture), Tag = "castor_with_remote_cluster", Key = "withRemoteCluster".ToLower(CultureInfo.InvariantCulture), Value = "castor_with_remote_cluster_value",  KeyName = "WithRemoteCluster", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>() {
                    new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Access_Mode",culture), Tag = "castor_with_remote_cluster_value", Value = "AccessMode".ToLower(CultureInfo.InvariantCulture), Key = "ACCESSMODE".ToLower(CultureInfo.InvariantCulture), KeyName = "AccessMode", Visibility="Collapsed", ValType = "string", GuiType = "ComboBox", ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_CSN",culture), Tag = "castor_with_remote_cluster_value_son_remote", Value = "remoteCSNValue".ToLower(CultureInfo.InvariantCulture), Key = "REMOTECSN".ToLower(CultureInfo.InvariantCulture), KeyName = "RemoteCSN", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() { 
                            new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_CSN_Host",culture), Tag = "castor_with_remote_cluster_value_son_remote", Key = "remoteCSNHost".ToLower(CultureInfo.InvariantCulture), KeyName = "RemoteCSNHost", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_CSN_Port",culture), Tag = "castor_with_remote_cluster_value_son_remote", Key = "remoteCSNPort".ToLower(CultureInfo.InvariantCulture), KeyName = "RemoteCSNPort", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"}}
                        },
                        new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Local_Proxy",culture), Tag = "castor_with_remote_cluster_value_son_Local", Value = "localProxyValue".ToLower(CultureInfo.InvariantCulture), Key = "LOCALPROXY".ToLower(CultureInfo.InvariantCulture), KeyName = "LocalProxy", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() { 
                            new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_SCSP_Proxy_Host",culture), Tag = "castor_with_remote_cluster_value_son_Local", Key = "SCSPPROXYHOST".ToLower(CultureInfo.InvariantCulture), KeyName = "SCSPProxyHost", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_SCSP_Proxy_Port",culture), Tag = "castor_with_remote_cluster_value_son_Local", Key = "SCSPPROXYPORT".ToLower(CultureInfo.InvariantCulture), KeyName = "SCSPProxyPort", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_Cluster_Name",culture), Tag = "castor_with_remote_cluster_value_son_Local", Key = "REMOTECLUSTERNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "RemoteClusterName", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"}}
                        }
                    }}}},

                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Number_of_Object_Replicas",culture), Tag = "castor_replicas_number", Key = "REPLICASNUMBER".ToLower(CultureInfo.InvariantCulture), KeyName = "ReplicasNumber", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox",DemoValue=@"2"},

                new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_DX_Optimizer_Compression",culture), Tag = "castor_compress_type", Value = "COMPRESSTYPE".ToLower(CultureInfo.InvariantCulture), Key = "COMPRESSTYPE".ToLower(CultureInfo.InvariantCulture), KeyName = "CompressType", Visibility = "Visible", ValType="string", GuiType="ComboBox", ChildFeatures = new List<FeatureUnit>() {
                    new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_None",culture), Tag = "castor_compress_none", Value = "no", Key = "castor_COMPRESSNONE".ToLower(CultureInfo.InvariantCulture), KeyName = "CompressNone", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem"},
                    new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Best",culture), Tag = "castor_compress_best", Value = "best", Key = "castor_CASTORCOMPRESSBEST".ToLower(CultureInfo.InvariantCulture), KeyName = "CastorCompressBest", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Compress_After_Days",culture), Tag = "castor_compress_best", Key = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture), KeyName = "BestDeferCompression", Visibility = "Collapsed", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true", DemoValue = "1-29"}}
                    },
                    new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Fast",culture), Tag = "castor_compress_fast", Value = "fast", Key = "castor_CASTORCOMPRESSFAST".ToLower(CultureInfo.InvariantCulture), KeyName = "CastorCompressFast", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Compress_After_Days",culture), Tag = "castor_compress_fast", Key = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture), KeyName = "FastDeferCompression", Visibility = "Collapsed", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true", DemoValue = "1-29"}}}
                }},
                //new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Compress_After_Days",culture), Tag = "castor_defer_compression", Key = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture), KeyName = "DeferCompression", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true"},
            
                 new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Advanced",culture), Tag = "castor_vim_advanced", Key = "advanced", Value = "castor_vim_ExtendedParameters",  KeyName = "CAStorAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "castor_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Extended_Parameters",culture),Tag = "castor_vim_ExtendedParameters", Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "CAStorExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}   
            };
            Add(sf);


            StorageFeature crgSf = new StorageFeature();
            StorageType crgDell = new StorageType();
            crgDell.Display = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Caringo_Storage", culture);
            crgDell.Index = 8;
            crgDell.Value = "CARINGOStorage";
            crgDell.Vim = new List<string>() { "caringo_vim" };
            crgDell.IsSupportMovableRetention = false;
            crgSf.Type = crgDell;
            crgSf.Type.DefaultXris = new List<string>() { "docave-xam://caringo_vim?" };
            crgSf.IsNeedSpaceThreshold = true;
            crgSf.IsObjectType = true;
            crgSf.ProgressForeground = new FeatureColor(255, 100, 149, 237);

            crgSf.Features = new List<FeatureUnit>() { 

                  new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Communication_Type",culture), Tag = "caringo_with_communication_type_value", Value = "CommunicationType".ToLower(CultureInfo.InvariantCulture), Key = "COMMUNICATIONTYPE".ToLower(CultureInfo.InvariantCulture), KeyName = "CommunicationType", Visibility="Visible", ValType = "string", GuiType = "ComboBox", ChildFeatures = new List<FeatureUnit>() {
                       new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Proxy_Locator",culture), Tag = "caringo_with_communication_type_value_son_proxy", Value = "Proxy".ToLower(CultureInfo.InvariantCulture), Key = "CRGProxyLocator".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGProxyLocator", Visibility="Visible", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() { 
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_CSN_Private_Network_IP",culture), Tag = "caringo_with_communication_type_value_son_proxy", Key = "primaryNode".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCSNPNI", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", CanNullOrEmpty = "true"},
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_SCSP_Proxy_Port",culture), Tag = "caringo_with_communication_type_value_son_proxy", Key = "primaryPort".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGProxyPort", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", CanNullOrEmpty = "true"},
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Cluster_Name",culture), Tag = "caringo_with_communication_type_value_son_proxy", Key = "CLUSTERNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGClusterName", Visibility="Visible", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", CanNullOrEmpty = "true"}
                       }},
                       new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Static_Locator",culture), Tag = "caringo_with_communication_type_value_son_static", Value = "Static".ToLower(CultureInfo.InvariantCulture), Key = "CRGStaticLocator".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGStaticLocator", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() { 
                         new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Primary_DX_Storage_Node",culture), Tag = "caringo_with_communication_type_value_son_static", Key = "primaryNode".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGStorageNode", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", CanNullOrEmpty = "true"},
                         new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Primary_Storage_Node_Port",culture), Tag = "caringo_with_communication_type_value_son_static", Key = "primaryPort".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGStoragePort", Visibility="Collapsed", ValType = "string", IsRequiredOption = true, GuiType = "TextBox", CanNullOrEmpty = "true"}
                    }}
                  }},
                    
                  new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Primary_DX_CR_Publisher",culture), Tag = "caringo_cr_publisher", Key = "CRPUBLISHER".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCrPublisher", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true"},
                  new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Primary_DX_CR_Publisher_Port",culture), Tag = "caringo_cr_publisher_port", Key = "CRPUBLISHERPORT".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCrPublisherPort", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true"},

                    new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Require_Authentication",culture), Tag = "caringo_with_require_authentication", Key = "CRGWithRequireAuthentication".ToLower(CultureInfo.InvariantCulture), Value = "caringo_with_require_authentication_value",  KeyName = "CRGWithRequireAuthentication", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>() {
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Username",culture), Tag = "caringo_with_require_authentication_value", Key = "CRGUSERNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGUsername", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Password",culture), Tag = "caringo_with_require_authentication_value", Key = "CRGPASSNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGPassword", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Authentication_Realm",culture), Tag = "caringo_with_require_authentication_value", Key = "CRGAUTHENTICATIONREALM".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGAuthenticationRealm", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"}
                    }},            

                    new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_With_Remote_D_R_Cluster",culture), Tag = "caringo_with_remote_cluster", Key = "CRGWithRemoteCluster".ToLower(CultureInfo.InvariantCulture), Value = "caringo_with_remote_cluster_value",  KeyName = "CRGWithRemoteCluster", Visibility="Visible", ValType = "string", GuiType = "CheckBox", ChildFeatures = new List<FeatureUnit>() {
                    new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Access_Mode",culture), Tag = "caringo_with_remote_cluster_value", Value = "AccessMode".ToLower(CultureInfo.InvariantCulture), Key = "CRGACCESSMODE".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGAccessMode", Visibility="Collapsed", ValType = "string", GuiType = "ComboBox", ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_CSN",culture), Tag = "caringo_with_remote_cluster_value_son_remote", Value = "remoteCSNValue".ToLower(CultureInfo.InvariantCulture), Key = "REMOTECSN".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGRemoteCSN", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() { 
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_CSN_Host",culture), Tag = "caringo_with_remote_cluster_value_son_remote", Key = "remoteCSNHost".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGRemoteCSNHost", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_CSN_Port",culture), Tag = "caringo_with_remote_cluster_value_son_remote", Key = "remoteCSNPort".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGRemoteCSNPort", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"}}
                        },
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Local_Proxy",culture), Tag = "caringo_with_remote_cluster_value_son_Local", Value = "localProxyValue".ToLower(CultureInfo.InvariantCulture), Key = "CRGLOCALPROXY".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGLocalProxy", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() { 
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_SCSP_Proxy_Host",culture), Tag = "caringo_with_remote_cluster_value_son_Local", Key = "SCSPPROXYHOST".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGSCSPProxyHost", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_SCSP_Proxy_Port",culture), Tag = "caringo_with_remote_cluster_value_son_Local", Key = "SCSPPROXYPORT".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGSCSPProxyPort", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"},
                            new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Remote_Cluster_Name",culture), Tag = "caringo_with_remote_cluster_value_son_Local", Key = "REMOTECLUSTERNAME".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGRemoteClusterName", Visibility="Collapsed", ValType = "string", GuiType = "TextBox", CanNullOrEmpty = "true"}}
                        }
                    }}}},

                new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Number_of_Object_Replicas",culture), Tag = "caringo_replicas_number", Key = "REPLICASNUMBER".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGReplicasNumber", Visibility = "Visible", ValType="string", IsRequiredOption = true, GuiType="TextBox",DemoValue=@"2"},

                new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Caringo_Optimizer_Compression",culture), Tag = "caringo_compress_type", Value = "CRGCompressType".ToLower(CultureInfo.InvariantCulture), Key = "CRGCOMPRESSTYPE".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCompressType", Visibility = "Visible", ValType="string", GuiType="ComboBox", ChildFeatures = new List<FeatureUnit>() {
                    new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_None",culture), Tag = "caringo_compress_none", Value = "no", Key = "caringo_COMPRESSNONE".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCompressNone", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem"},
                    new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Best",culture), Tag = "caringo_compress_best", Value = "best", Key = "caringo_CASTORCOMPRESSBEST".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCastorCompressBest", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>(){
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Compress_After_Days",culture), Tag = "caringo_compress_best", Key = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGBestDeferCompression", Visibility = "Collapsed", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true", DemoValue = "1-29"}}
                    },
                    new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Fast",culture), Tag = "caringo_compress_fast", Value = "fast", Key = "caringo_CASTORCOMPRESSFAST".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGCastorCompressFast", Visibility="Collapsed", ValType = "string", GuiType = "ComboBoxItem", ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Compress_After_Days",culture), Tag = "caringo_compress_fast", Key = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGFastDeferCompression", Visibility = "Collapsed", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true", DemoValue = "1-29"}}
                    }}
                },
                    //new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Compress_After_Days",culture), Tag = "caringo_defer_compression", Key = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGDeferCompression", Visibility = "Visible", ValType="string", GuiType="TextBox", CanNullOrEmpty = "true"},
                new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Advanced",culture), Tag = "caringo_vim", Key = "advanced", KeyName = "CRGStorAdvanced",Visibility="Visible", ValType = "string", GuiType = "CheckBox", Value = "caringo_vim_ExtendedParameters",  ChildFeatures = new List<FeatureUnit>() {
                        new FeatureUnit() {Vim = "caringo_vim", DisplayName = CAStorI18N.ResourceManager.GetString("MediaStorage_CAStor_Extended_Parameters",culture),Tag = "caringo_vim_ExtendedParameters",  Key = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture), KeyName = "CRGStorExtendedParameters",  Visibility="Collapsed", ValType = "string", GuiType = "TextArea", CanNullOrEmpty = "true"}}}   
            };
            Add(crgSf);


        }

        protected override void GenerateSingleTypeFeatureUnit(CultureInfo culture)
        {
            GenerateDocAveGUIFeatureUnit(culture);
        }

        #endregion

        /// <summary>
        /// 供VIM调用, 获取相应的Feature
        /// </summary>
        private static readonly Dictionary<string, CAStorFeature> instances = new Dictionary<string, CAStorFeature>();
        private static Object locker = new Object();
        public static CAStorFeature Getstances(int type, string culture = "en")
        {
            lock (locker)
            {
                if (!instances.ContainsKey(type + culture))
                {
                    CAStorFeature dx = new CAStorFeature(type, culture);
                    foreach (var feature in dx.FeatureObjs)
                    {
                        feature.AdvancedOptions.AddRange(new List<string>() 
                    {
                        "LocatorType=Proxy",
                        "LocatorType=Static",
                        "CustomizedMetadata={[testKey1,testValue1],[testKey2,testValue2],[testKey3,testValue3]}",
                        "CustomizedMode=Close",
                        "CustomizedMode=SupportAll",
                        "CustomizedMode=DocAveOnly",
                        "CustomizedMode=CustomizedOnly"
                    });
                    }
                    instances[type + culture] = dx;
                    return dx;
                }
                else
                {
                    return instances[type + culture];
                }
            }
        }
    }
}
