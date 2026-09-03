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

using System.ComponentModel;

namespace AvePoint.RA.Contract.Configurations
{
    /// <summary>
    /// 不需要加密的信息, 储存在本地配置文件中.
    /// </summary>
    public enum RMAppSettingKey
    {
        #region Agent 相关
        AGENT_INSTALLER_URL,
        AGENT_LATEST_VERSION,
        RETRY_COUNT_FOR_GET_AGENT,
        RETRY_INTERVAL_SECONDS_FOR_GET_AGENT,
        #endregion
        #region AOS 相关
        AOS_URL,
        AOS_API_URL,
        AOS_MODERN_API_URL,
        AOS_LOGIN_URL,
        AOS_AUTHORITY_URL,
        TOKEN_API_URL,
        AOS_APP_REGISTER_URL,
        AOS_SERVICE_BUS_TOPIC_NAME_PREFIX,
        AOS_SERVICE_BUS_SUBSCRIPTION_NAME_PREFIX,
        #endregion
        #region DAO 相关
        DAO_CONTROL_SERVICE_ADDRESS,
        #endregion
        #region Job Control
        MAX_JOBS_LIMIT_PER_TENANT,
        REALTIME_MAX_JOBS_LIMIT,
        NODE_COUNT_IN_SUB_JOB,
        SUB_JOB_COUNT_IN_MAIN_JOB,
        MAX_PARALLEL_SRN_JOBS_LIMIT,
        RUN_SRN_JOB_IF_NO_NOTIFIED_IN_MINUTES,
        JOB_CONFIG_FOR_CUSTOMERS,
        EXO_ENABLE_BULK_GENERATE_ITEMS,
        EXO_BULK_ITEMS_COUNT_LIMIT,
        EXO_BULK_ITEMS_SIZE_LIMIT,
        EXO_DISCOVER_THREADS_LIMIT,
        EXO_DISCOVER_ITEMS_PER_TASK,
        EXO_DUE_ITEMS_PER_TASK,
        SPO_APPLY_SETTINGS_ITEMS_PER_TASK,
        SPO_CALL_LIMIT_PER_SECOND,
        SPO_DISCOVER_ITEMS_PER_TASK,
        SPO_DUE_ITEMS_PER_TASK,
        SPO_SYNC_DATA_ITEMS_PER_TASK,
        SPO_SYNC_DATA_CALL_LIMIT_PER_SECOND,
        COSMOS_SYNC_DATA_CALL_LIMIT_PER_SECOND,
        UNIQUE_ID_JOB_RUN_TIME_OFFSET_MINUTES,
        #endregion

        #region Identity Service
        IDENTITY_SERVICE_URL,
        VALID_ISSUER_URLS,
        AUDIENCE_URL,
        PUBLIC_IDENTITY_SERVICE_URL,
        MULTI_GEO_PUBLIC_IDENTITY_SERVICE_URL,
        PUBLIC_AUDIENCE_URL,
        CLIENT_ID_IN_IDENTITY_SERVICE,
        SSO_PRODUCT_AUDIENCE_URL,
        #endregion

        #region Upgrade
        UPGRADE_MONITOR_URL,
        #endregion

        CLOUD_INSIGHTS_API_URL,
        COP_API_URL,
        MYHUB_API_URL,
        AOSP_API_URL,
        NEXUS_FOUNDATION_API_URL,
        NEXUS_GOVERNANCE_API_URL,
        GCONTROL_MYHUB_TASK_URL,
        COP_QUEUE_NAME,
        JOB_QUEUE_NAME,
        HIGH_PRIORITY_JOB_QUEUE_NAME,
        HIGHEST_PRIORITY_JOB_QUEUE_NAME,
        REALTIME_QUEUE_NAME,
        RECO_API_URL,
        PUBLIC_RECO_API_URL,
        MULTI_GEO_PUBLIC_RECO_API_URL,
        RECO_APP_LOGIN_URL,
        RECO_SSO_LOGIN_URL,
        SIGNALR_SERVER_URL,
        PUBLIC_SIGNALR_SERVER_URL,
        MULTI_GEO_PUBLIC_SIGNALR_SERVER_URL,
        FORWARD_TO_REPORT_CENTER,
        LOG_COSMOS_QUERY_METRICS,
        UNIQUE_ID_SP_SEARCH_SITE_COLUMN,
        UNIQUE_ID_SP_SEARCH_LIST_COLUMN,
        MOBILE_SESSION_TIMEOUT_MINUTES,
        ODATA_CLIENT_TIMEOUT_MINUTES,
        CLIENT_ID_IN_RECO_LOGIN_WEB,
        CLIENT_ID_IN_RELATED_RECORDS_APP,
        ENABLE_SECURITY_TRIMMING,
        CSP_FORM_ACTION,
        AOS_CUSTOM_APP_CONFIG,
        SSO_CLIENT_ID,
        SSO_SERVICE_URL,
        RECO_DOMAIN_URL,
        AOS_DATA_CENTER,
        ICS_API_URL,
        INSIGHTS_ENGINE_API_URL,
        EDISCOVERY_API_URL,
        DOWNLOADCENTER_DOWNLOAD_FILESIZE_LIMIT,
        DEFAULT_STORAGE_TIER,
        CORS_ALLOWED_ORIGIN,
        CHAT_BOT_API_URL,
        CHAT_BOT_URL,
        OPUS_CSD_API_URL,
        AOS_LOGIN_APP_ID,
        VERTEX_AI_PROJECT_ID,
        VERTEX_AI_TEXT_MODEL_NAME,
        VERTEX_AI_CHAT_MODEL_NAME,
        VERTEX_AI_SERVICE_ACCOUNT,
        VERTEX_AI_LOCATION,
        GENERATE_SUMMARY_CONTENT_URL,
        AZURE_OPENAI_ENDPOINT,
        AZURE_OPEN_AI_CHAT_DEPLOYMENT_NAME,
        AZURE_OPEN_AI_TEXT_DEPLOYMENT_NAME,
        JPMC_MULTI_GEO_DC_RESOURCE_API,
        DAL_GATEWAY_API_URL
    }

    /// <summary>
    /// DB信息
    /// </summary>
    public enum RMDatabaseSettingKey
    {
        RECO_COSMOS_DB_CONNECTION_STRING,
        RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION,
        RECO_CONTROL_SQL_CONNECTION_STRING,
        RECO_CONTROL_SQL_CONNECTION_STRING_FULL,
        RECO_CONTROL_SQL_PRIMARY_SERVER,
        SQL_SERVER_FOR_ELASTICPOOL,
        MAX_DATABASE_INPOOL
    }

    /// <summary>
    /// 基础环境信息
    /// </summary>
    public enum RMEnvSettingKey
    {
        DEV_MODE,
        ENVIRONMENT_NAME,
        AZURE_ENVIRONMENT,
        CIPHER_SERVICE_URL,
        INFRA_CIPHER_KEY,
        /// <summary>
        ///  只有local dev环境需要配置 MASTER_CERTIFICATE_THUMBPRINT，用于操作Azure资源
        /// </summary>
        MASTER_CERTIFICATE_THUMBPRINT,
        /// <summary>
        ///  只有local dev环境需要配置 KEY_VAULT_CLIENT_ID，用于操作KeyVault
        /// </summary>
        KEY_VAULT_CLIENT_ID, // not required in prod
        KEY_VAULT_URL,
        PRODUCT_CERTIFICATE_IDENTIFIER,
        PRODUCT_CERTIFICATE_THUMBPRINT, // not required in prod
        DAO_CERTIFICATE_IDENTIFIER,
        DAO_CERTIFICATE_THUMBPRINT, // not required in prod
        DAO_KEY_VAULT_CERTIFICATE_THUMBPRINT, // not required in prod
        DAO_KEY_VAULT_URL,
        DAO_KEY_VAULT_CLIENT_ID, // not required in prod
        OC_CERTIFICATE_IDENTIFIER,
        OC_CERTIFICATE_THUMBPRINT, // not required in prod
        ENCRYPTION_FSCERTIFICATE_KEY,
        DB_DEFAULT_ENCRYPTION_KEY,
        RES_CDN_URL,
        RASP_GUIDE_CDN_URL,
    }

    /// <summary>
    /// 多指需要加密的数据, 或者非常common的设置
    /// </summary>
    public enum RMCommonSettingKey
    {
        SERVICE_BUS_CONNECTION_STRING,
        SENDGRID_KEY,
        SENDGRID_ACCOUNT,
        SENDGRID_SENDER_ADDRESS,
        SENDGRID_PORT,
        SENDGRID_SERVER,

        SENDGRID_SENDER_DISPLAYNAME,
        NOTIFICATION_SETTING,
        RELATED_RECORDS_APP_CLIENT_SECRET,
        RELATED_RECORDS_APP_CLIENT_SECONDARY_SECRET,
        RECO_REDIS_CONNECTION_STRING,
        AOS_SERVICE_BUS_CONNECTION_STRING,
        TELEMETRY_CONNECTION_STRING,
        VERTEX_AI_PRIVATE_KEY,
        VECTOR_DB_CONNECTION_STRING,
        AZURE_OPEN_AI_API_KEY
    }

    /// <summary>
    /// storage 信息
    /// </summary>
    public enum RMStorageSettingKey
    {
        LOG_CONTAINER_NAME,
        REPORT_CONTAINER_NAME,
        REPORT_TEMP_FOLDER,
        JOB_CONTEXT_CONTAINER_NAME,
        RECORDS_HISTORY_STORAGE_CONNECTION_STRING_FULL,
        DEFAULT_STORAGE_CONNECTION_STRING,
        SHARED_STORAGE_CONNECTION_STRING,
        SHARED_STORAGE_CONTAINER_NAME,
        RECO_STORAGE_CONNECTION_STRING,
        RECO_PUBLIC_STORAGE_CONNECTION_STRING,
        DAO_JOB_REPORT_STORAGE_CONNECTION_STRING,
        DAO_JOB_REPORT_CONTAINER_NAME,
        SPECIAL_REGIONS_RESOURCES,
        //DEFAULT_GOOGLE_STORAGE_CONNECTION_STRING,
        OLD_AVE_STORAGE_CONNECTION_STRING_IN_21V,
    }
    public enum TheSpecialDCKey
    {
        SOUTHAFRICA
    }
    public class RMStorageSetting 
    {
        public static string SPECIAL_REGIONS_RESOURCES_DEFAULT_STORAGE_CONNECTION_STRING = "DEFAULT_STORAGE_CONNECTION_STRING";
        public static string SPECIAL_REGIONS_RESOURCES_SHARED_STORAGE_CONNECTION_STRING = "SHARED_STORAGE_CONNECTION_STRING";
        public static string SPECIAL_REGIONS_RESOURCES_JOB_QUEUE_NAME = "JOB_QUEUE_NAME";
    }
}
