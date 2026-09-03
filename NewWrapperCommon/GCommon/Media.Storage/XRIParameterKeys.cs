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


using System.Diagnostics.CodeAnalysis;
using System.Globalization;


[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.XRIParameterKeys.#.cctor()", MessageId = "signatureversion")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.XRIParameterKeys.#.cctor()", MessageId = "customizedregion")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.XRIParameterKeys.#.cctor()", MessageId = "Lan")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.XRIParameterKeys.#.cctor()", MessageId = "Comm")]
namespace AvePoint.Media.Storage
{
    /// <summary>
    /// 这里定义的主要是xri字符串里的参数名称常量, 规定比如docave-xam://fs_vim!d:\docavecache?cache=true问号?之后的称为参数, 名称必须为常量.
    /// </summary>
    public class XRIParameterKeys //same as PhysicalDeviceParameterKeys class
    {
        /// <summary>
        /// 用于表示device的Id(physical device id)
        /// </summary>
        public static readonly string SYSTEM_ID_KEY = "id";

        /// <summary>
        /// 用于表示device的display name(physical device name)
        /// </summary>
        public static readonly string SYSTEM_NAME_KEY = "DEVICENAME".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用于表示device是online还是offline
        /// </summary>
        public static readonly string SYSTEM_STATUS_KEY = "status";

        /// <summary>
        /// 用于表示device用户，存放all/data/index
        /// </summary>
        public static readonly string SYSTEM_USAGE_KEY = "usage";

        /// <summary>
        /// 用于表示用于各种device用于验证的"用户名"
        /// </summary>
        public static readonly string USERNAME_KEY = "name";

        /// <summary>
        /// 用于表示用于各种device用于验证的"密码"， 这里的值value必须是秘文
        /// </summary>
        public static readonly string PASSWORD_KEY = "secret";

        /// <summary>
        /// 用于表示是否使用了扩展参数
        /// </summary>
        public static readonly string ADVANCED_KEY = "Advanced".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用于表示各种介质的扩展参数
        /// </summary>
        public static readonly string EXTENDED_PARAMETERS = "ExtendedParameters".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用于表示各种Cloud介质的重连次数
        /// </summary>
        public static readonly string RETRY_COUNT = "RetryCount".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用于表示各种Cloud介质的Folder是Shared还是Privated的
        /// </summary>
        public static readonly string USE_SHARED = "UseShared".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用于表示各种Cloud介质的重连间隔时间
        /// </summary>
        public static readonly string RETRY_INTERVAL = "RetryInterval".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 保留空间大小，如果剩余空闲空间小于这个值，那么不能再向这个device写数据，当做磁盘满处理
        /// </summary> 
        public static readonly string SPACE_THRESHOLD_KEY = "spaceThreshold".ToLower(CultureInfo.InvariantCulture);

        public static readonly string Proxy_Setting = "ProxySetting".ToLower(CultureInfo.InvariantCulture);

        public static readonly string PROXY_IP = "ProxyIp".ToLower(CultureInfo.InvariantCulture);

        public static readonly string PROXY_PORT = "ProxyPort".ToLower(CultureInfo.InvariantCulture);

        public static readonly string PROXY_USERNAME = "ProxyUsername".ToLower(CultureInfo.InvariantCulture);

        public static readonly string PROXYPASSWORD = "ProxyPasswordSecret".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// //保留空间单位，可能是MB,也可能是%
        /// </summary>
        public static readonly string SPACE_THRESHOLD_UNIT_KEY = "spaceThresholdUnit".ToLower(CultureInfo.InvariantCulture);

        public static readonly string CREATE_IF_NOT_EXISTS = "creation";

        public static readonly string SECURELY_DELETE = "SecurelyDelete".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        ///这个参数主要是用来区分上层功能的，media的话，不给这个值赋值。connector是1.其他功能也要赋相应的值
        /// </summary>
        public static readonly string MODULE_TYPE_KEY = "moduleType".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        ///主要是用来表示Netshare device配置的Path;
        /// </summary>
        public static readonly string LocationKey = "location";

        /// <summary>
        ///主要是用来表示extend param 里配置的Customizedmeta 的level;
        /// </summary>
        public static readonly string CustomizedModeKey = "CustomizedMode".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        ///主要是用来表示extend param 里配置的Customized meta 的字符串;
        ///解析后是一个keyvaluepair的list
        /// </summary>
        public static readonly string CustomizedMetaKey = "customizedMetaData".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用来判断底层是否去Retry， Retry=true, 底层去Retry, Retry=false 底层不去Retry.
        /// </summary>
        public static readonly string IS_RETRY = "IsRetry".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 用来判断底层用来判断是否是readOnly 类型device
        /// </summary>
        public static readonly string READONLY = "READONLY".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 根据界面的配置，来获取相应的语言环境
        /// </summary>
        public static readonly string CultureInfo_Key = "culture".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        /// 根据界面的配置storage将需要添加的connectionstring的参数传给前台
        /// </summary>
        public static readonly string AppendConnectionStringKey = "AppendConnectionStringKey".ToLower(CultureInfo.InvariantCulture);//AppendConnectionStringKey:spell
        /// <summary>
        ///  强制跳过对device的验证
        /// </summary>
        public static readonly string ForcePassValidationKey = "ForcePassValidation".ToLower(CultureInfo.InvariantCulture);

        /// <summary>
        ///  用于属性直接的分隔符
        /// </summary>
        public static readonly string ParamSeparator = "&";

        /**************************For FS*********************************/
        public static readonly string XRI_KEY_AUTH_METHOD = "AUTHMETHOD".ToLower(CultureInfo.InvariantCulture);
        public static readonly string XRI_KEY_ReadFailover = "READFAILOVER".ToLower(CultureInfo.InvariantCulture);
        public static readonly string XRI_KEY_ReadFailover_DFSNAME = "DFSNAME".ToLower(CultureInfo.InvariantCulture);
        public static readonly string XRI_KEY_ReadFailover_ENUMLEVEL = "ENUMLEVEL".ToLower(CultureInfo.InvariantCulture);
        public static readonly string XRI_KEY_ReadFailover_Prefix = "PREFIX".ToLower(CultureInfo.InvariantCulture);
        public static readonly string FS_KEY_LongPathEnabled = "LongPathEnabled".ToLower(CultureInfo.InvariantCulture);
        public static readonly string FS_KEY_BufferSize = "BufferSize".ToLower(CultureInfo.InvariantCulture);
        public static readonly string FS_Key_FileOptions = "FileOptions".ToLower(CultureInfo.InvariantCulture);
        /**************************For FS*********************************/

        /**************************For Ftp*********************************/
        public static readonly string FTP_HOST = "host";

        public static readonly string FTP_PORT = "port";

        public static readonly int FTP_DEFAULT_PORT = 21;

        public static readonly string FTP_DEFAULT_NAME = "anonymous";

        public static readonly string FTP_SCHEMA = "schema";

        public static readonly string FTPTypekey = "ftpType".ToLower(CultureInfo.InvariantCulture);

        public static readonly string FTP_RootFolder = "FTPRootFolder".ToLower(CultureInfo.InvariantCulture);

        public static readonly string FTP_USEPASSIVE = "usepassive";

        public static readonly string FTP_USEFLUENTFTP = "usefluentftp";

        /**************************For Ftp*********************************/

        /**************************For Egnyte*************************************/
        public static readonly string Egnyte_Domain = "Domain".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Egnyte_Token = "egnyteAccessTokenSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Egnyte_UserName = "username".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Egnyte_Password = "passwordSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Egnyte_RootFolderName = "root_folder_name".ToLower(CultureInfo.InvariantCulture);
        /**************************For Egnyte*************************************/

        /**************************For Object Atmos*********************************/
        public static readonly string OBJECT_ATMOS_VALIDATE_KEY = "isValidate".ToLower(CultureInfo.InvariantCulture);
        public static readonly string OBJECT_ATMOS_FAILOVER_KEY = "ValidateFailoverInterval".ToLower(CultureInfo.InvariantCulture);
        public static readonly string OBJECT_ATMOS_CHECKSUM_UPLOAD = "EnableChecksumForCreate".ToLowerInvariant();
        public static readonly string OBJECT_ATMOS_CHECKSUM_DOWNLOAD = "VerifyChecksumAtRead".ToLowerInvariant();
        public static readonly string OBJECT_ATMOS_PROXY_SETTING = "cloud_atmos_Proxy".ToLower(CultureInfo.InvariantCulture);
        /**************************For Object Atmos*********************************/


        /**************************For Cloud Common*********************************/

        /**************************For Cloud Common*********************************/
        public static readonly string Cloud_USERNAME_KEY = "name";
        public static readonly string Cloud_PASSWORD_KEY = "secret";
        public static readonly string AccessPoinyKey = "ACCESSPOINT".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ContainerKey = "containerName".ToLowerInvariant();
        public static readonly string PROXY_INFO = "PROXY".ToLowerInvariant();
        public static readonly string Enable_SSL = "EnableSSL".ToLowerInvariant();
        /**************************For Cloud Common*********************************/

        /**************************For Cloud Atmos and AT&T*********************************/
        public static readonly string CLOUD_TYPE_KEY = "cType".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CTYRE_ATMOS = "atmos";
        public static readonly string CTYRE_ATT = "att";
        public static readonly string ATMOS_PROXY_SETTING = "cloud_atmos_Proxy".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ATT_PROXY_SETTING = "cloud_att_Proxy".ToLower(CultureInfo.InvariantCulture);
        /**************************For Cloud Atmos and AT&T*********************************/

        /**************************For Cloud Rackspace*********************************/
        public static readonly string CDN_KEY = ("CDN").ToLower(CultureInfo.InvariantCulture);
        public static readonly string RACKSPACE_PROXY_SETTING = "Rackspace_Proxy".ToLower(CultureInfo.InvariantCulture);
        /**************************For Cloud Rackspace*********************************/

        /**************************For Cloud Azure*********************************/
        public static readonly string CDNED = "CDNED".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CDN_GUID = "CDNGUID".ToLower(CultureInfo.InvariantCulture);
        public static readonly string BLOCK_LENGTH = "BlockLength".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AZURE_PROXY_SETTING = "azure_Proxy".ToLower(CultureInfo.InvariantCulture);
        /**************************For Cloud Azure*********************************/

        /**************************For Cloud Amazon*********************************/
        public static readonly string REGION_KEY = "region";
        public static readonly string CUSTOMIZEDREGION_KEY = "customizedregion";
        public static readonly string SIGNATUREVERSION_KEY = "signatureversion";
        public static readonly string BUCKET_NAME = "bucketName".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AMAZON_PROXY_SETTING = "Amazon_Proxy".ToLower(CultureInfo.InvariantCulture);
        /**************************For Cloud Amazon*********************************/

        /**************************For Cloud Dropbox*********************************/
        //public static readonly string APP_KEY = "appKey".ToLower(CultureInfo.InvariantCulture);
        //public static readonly string APP_SECRET = "appSecret".ToLower(CultureInfo.InvariantCulture);
        //public static readonly string TOKEN_ACCESS = "tokenAccess".ToLower(CultureInfo.InvariantCulture);
        //public static readonly string TOKEN_SECRET = "tokenSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string DROPBOX_VALIDATE_KEY = "isValidate".ToLower(CultureInfo.InvariantCulture);
        public static readonly string DROPBOX_CUSTOMIZED_APP = "cloud_dropbox_customized".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ACCESS_TOKEN = "DropboxAccessTokenSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string DROPBOX_PROXY_SETTING = "cloud_dropbox_Proxy".ToLower(CultureInfo.InvariantCulture);
        /**************************For Cloud Dropbox*********************************/

        /**************************For TSM*********************************/
        public static readonly string DSM_NODE_NAME = "node"; //表示界面上填写的node name
        public static readonly string DSM_NODE_PWD = "secret"; //表示界面上填写的node的密码
        public static readonly string DSM_ENABLE_NODE_PROXY = "enablenodeproxy"; //Client Node Proxy
        public static readonly string DSM_Asnodename = "asnodename";
        public static readonly string DSM_MC = "managementClass".ToLower(CultureInfo.InvariantCulture); //表示界面上填写的node name
        public static readonly string DSM_COMMMETHOD = "COMMMETHOD".ToLower(CultureInfo.InvariantCulture);
        public static readonly string DSM_PORT = "port";
        public static readonly string DSM_ROOT = "../DSM".ToLower(CultureInfo.InvariantCulture);
        public static readonly long DSM_MAXFILESIZE = 50 * 1024 * 1024; //maxFileSize
        public static readonly long DSM_DEFAULT_FILE_SPACE_CAPACITY = 10 * 1024 * 1024 * 1024L;
        public static readonly long DSM_DEFAULT_FILE_SPACE_OCCUPANCY = 0L;
        public static readonly string DSM_SERVER_ADDRESS = "address";
        public static readonly string DSM_VALIDATE_KEY = "isValidate".ToLower(CultureInfo.InvariantCulture);
        public static readonly string DSM_MODIFY_TIME_KEY = "modifyTime".ToLower(CultureInfo.InvariantCulture);
        public static readonly string FILESPACE_KEY = "FileSpace".ToLower(CultureInfo.InvariantCulture);
        public static readonly string SINGLESESSION_KEY = "SingleSession".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ENABLELANFREE = "EnableLanFree".ToLower(CultureInfo.InvariantCulture);
        public static readonly string LANFREETCPPORT = "LanFreeTcpPort".ToLower(CultureInfo.InvariantCulture);
        public static readonly string LANFREETCPSERVERADDRESS = "LanFreeTcpServerAddress".ToLower(CultureInfo.InvariantCulture);
        public static readonly string LANFREECOMMENTTHOD = "LanFreeCommMethod".ToLower(CultureInfo.InvariantCulture);

        //用于判断是不是SINGLESESSION来创建TSMSystem   
        public static readonly string SINGLESESSIONTRUE = "SingleSession=true".ToLower(CultureInfo.InvariantCulture);
        //public static readonly string DSM_DIR_KEY = "dsmidir";
        //public static readonly string DSM_LOG_KRY = "dsmilog";
        //public static readonly string LOG_NMAE_KRY = "logname";
        //public static readonly string CONFIG_FILE_KEY = "configfile";
        //public static readonly string FILESPACE_KEY = "filespace";
        //public static readonly string DSM_CONFIG_KEY = "configfile";
        //public static readonly string CAPACITY_KEY = "capacity";
        //public static readonly string OCCUPANCY_KEY = "occupancy";
        //public static readonly string SIZE_ESTIMATE_KEY = "sizeestimate";
        /**************************For TSM*********************************/

        /**************************For Dell DX*********************************/
        public static readonly string PramaryNodeKey = "primaryNode".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ParamyNodePortKey = "primaryPort".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ClusterNameKey = "clusterName".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CRPublisherKey = "CRPUBLISHER".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CRPublisherPortKey = "CRPUBLISHERPORT".ToLower(CultureInfo.InvariantCulture);

        public static readonly string WithRemoteClusterKey = "WITHREMOTECLUSTER".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CRGWithRemoteClusterKey = "CRGWITHREMOTECLUSTER".ToLower(CultureInfo.InvariantCulture);

        public static readonly string AccessModeValueKey = "accessMode".ToLower(CultureInfo.InvariantCulture);
        public static readonly string RemoteCSNValueKey = "REMOTECSNVALUE".ToLower(CultureInfo.InvariantCulture);
        public static readonly string LocalProxyValueKey = "LOCALPROXYVALUE".ToLower(CultureInfo.InvariantCulture);

        public static readonly string RemoteCSNHostKey = "REMOTECSNHOST".ToLower(CultureInfo.InvariantCulture);
        public static readonly string RemoteCSNPorttKey = "REMOTECSNPORT".ToLower(CultureInfo.InvariantCulture);
        public static readonly string SCSPProxyHostKey = "SCSPPROXYHOST".ToLower(CultureInfo.InvariantCulture);
        public static readonly string SCSPProxyPortKey = "SCSPPROXYPORT".ToLower(CultureInfo.InvariantCulture);
        public static readonly string RemoteClusterNameKey = "REMOTECLUSTERNAME".ToLower(CultureInfo.InvariantCulture);
        public static readonly string NumberOfObjectReplicasKey = "REPLICASNUMBER".ToLower(CultureInfo.InvariantCulture);

        public static readonly string DxOptimizerCompressionValueKey = "COMPRESSTYPE".ToLower(CultureInfo.InvariantCulture);
        public static readonly string DxOptimizerNoneCompressionValueKey = "none";
        public static readonly string DxOptimizerBestCompressionValueKey = "best";
        public static readonly string DxOptimizerFastCompressionValueKey = "fast";

        public static readonly string DerferCompresstionKey = "DEFERCOMPRESSION".ToLower(CultureInfo.InvariantCulture);

        public static readonly string Locator = "Locator".ToLower(CultureInfo.InvariantCulture);
        public static readonly string LocatorType = "LocatorType".ToLower(CultureInfo.InvariantCulture);

        public static readonly string CACHE_REMOTE_HOST = "CacheRemoteHost".ToLower(CultureInfo.InvariantCulture);
        public static readonly string REMOTE_HOST_TIMEOUT = "RemoteHostTimeout".ToLower(CultureInfo.InvariantCulture);

        /**************************For Dell DX*********************************/

        /**************************For Caringo*********************************/
        public static readonly string Caringo_Communication_Key = "CommunicationType".ToLower(CultureInfo.InvariantCulture);
        /**************************For Caringo*********************************/

        /**************************For Alpha FS*********************************/

        public static readonly string AlphaFSLocation = "alphaLocation".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AlphsUsername = "alphaName".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AlphaFSPassword = "alphaSecret".ToLower(CultureInfo.InvariantCulture);
        /**************************For Alpha FS*********************************/

        /**************************For Mirror FS*********************************/
        public static readonly string SyncModeKey = "syncMode".ToLower(CultureInfo.InvariantCulture);

        /**************************For Mirror FS*********************************/

        /**************************For HCP*********************************/
        public static readonly string KEY_HOST = "host";
        public static readonly string KEY_SECONDHOST = "secondHost".ToLower(CultureInfo.InvariantCulture);
        public static readonly string KEY_USERNAME = "name";
        public static readonly string KEY_PASSWORD = "secret";
        public static readonly string KEY_NAMESPACE = "ns";
        public static readonly string KEY_LIBRARY = "lib";
        public static readonly string FLUSH_DNS = "FlushDNS".ToLower(CultureInfo.InvariantCulture);
        public static readonly string FAIL_OVER_MODE = "FailOverMode".ToLower(CultureInfo.InvariantCulture);
        public static readonly string SECONDARY_NAMESPACE_TIMEOUT = "SecondaryNamespaceTimeout".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CACHE_SECONDARY_NAMESPACE = "CacheSecondaryNamespace".ToLower(CultureInfo.InvariantCulture);
        /**************************For HCP*********************************/

        /**************************For EMC Centera*********************************/
        public static readonly string AUTHENTICATION_ADDRESS_KEY = "address";

        public static readonly string AUTHENTICATION_TYPE_KEY = "AUTHTYPE".ToLower(CultureInfo.InvariantCulture);

        public const string AUTHENTICATION_NAME_SECRET = @"n/sauth";

        //not key just value
        public const string AUTHENTICATION_PROFILES_SECRET = "pea";

        public static readonly string AUTHENTICATION_NAME_KEY = "name";

        public static readonly string AUTHENTICATION_SECRET_KEY = "secret";

        public static readonly string AUTHENTICATION_PROFILES_KEY = "PAEAUTH".ToLower(CultureInfo.InvariantCulture);

        public static readonly string AUTHENTICATION_PROFILES_NAME_KEY = "PAEU".ToLower(CultureInfo.InvariantCulture);

        public static readonly string AUTHENTICATION_PROFILES_PASSWORD_KEY = "PAEPSECRET".ToLower(CultureInfo.InvariantCulture);

        public static readonly string Centera_KEY_RetentionDays = "RetentionDays".ToLower(CultureInfo.InvariantCulture);
        /**************************For EMC Centera*********************************/


        /**************************************For NetApp************************************************/
        public static readonly string SNAPLOCKENABLED_KEY = "SnapLockEnabled".ToLower(CultureInfo.InvariantCulture);
        /************************************For NetApp****************************************************/

        /**************************************SkyDrive && google drive ************************************************/
        public static readonly string REFRESH_TOKEN = "RefreshTokenSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Client_ID = "Client_ID".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Client_Secret = "Client_Secret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Redirect_Domain = "Redirect_Domain".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Root_Folder_Id = "Root_Folder_Id".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Root_Folder_Name = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ONEDRIVE_CUSTOMIZED_APP = "SkyDrive_Customized".ToLower(CultureInfo.InvariantCulture);
        public static readonly string GOOGLEDRIVE_CUSTOMIZED_APP = "GoogleDrive_Customized".ToLower(CultureInfo.InvariantCulture);
        public static readonly string GOOGLEDRIVE_PROXY_SETTING = "GoogleDrive_Proxy".ToLower(CultureInfo.InvariantCulture);
        public static readonly string ONEDRIVE_PROXY_SETTING = "SkyDrive_Proxy".ToLower(CultureInfo.InvariantCulture);
        /************************************SkyDrive****************************************************/

        /***********************************Box************************************************************/
        public static readonly string Box_Client_ID = "boxClientId".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Email_Address = "boxEmailAddress".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Client_Secret = "boxRefreshSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Refresh_Token = "boxRefreshTokenSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Root_Folder_Name = "Root_Folder_Name".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Config_Location = "boxConfigLocation".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Config_Username = "boxConfigUsername".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Config_Password = "boxConfigPasswordSecret".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Manager_User_Name = "boxManagedUserName".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Manager_User_Id = "boxManagedUserId".ToLower(CultureInfo.InvariantCulture);
        //public static readonly string Box_Root_Folder_Id = "boxRootFolderId".ToLower(CultureInfo.InvariantCulture);
        public static readonly string Box_Validate_Key = "isValidate".ToLower(CultureInfo.InvariantCulture);
        public static readonly string BOX_CUSTOMIZED_APP = "Box_customized".ToLower(CultureInfo.InvariantCulture);
        public static readonly string BOX_PROXY_SETTING = "box_Proxy".ToLower(CultureInfo.InvariantCulture);
        /***********************************Box************************************************************/
        public static string[] GetFromDevice = new string[] { Root_Folder_Id };


        /**************************For Cloud Cleversafe*********************************/
        public static readonly string ACCESSER_IP = "accesser_ip";
        public static readonly string VAULT_NAME = "vaultName".ToLower(CultureInfo.InvariantCulture);
        /**************************For Cloud Cleversafe*********************************/

        /************************************For S3Compatible****************************************************/
        public static readonly string EndPoint = "endpoint";
        /************************************For S3Compatible****************************************************/


    }
}
