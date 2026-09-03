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



using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Service
{
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
    [ServiceContract]
    public interface IAEDSearchService
    {
        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [OperationContract]
        SearchResultMessage ExcuteQuery(QueryMessage msg);  //lptian

        /// <summary>
        /// 安装dll，并建立基本映射。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [OperationContract]
        VersionSettingResultMessage EnableVersionSearch(VersionSettingMessage msg);//ywhe

        [OperationContract]
        bool ExcuteOfflineQuery(QueryMessage msg);

        /// <summary>
        /// 添加Crawled property 和managed propery映射。
        /// </summary>
        /// <param name="msg"></param>
        [OperationContract]
        MappingResult AddMappings(MappingMessage msg);//xlliu

        /// <summary>
        /// 以web为单位，获取所有的column，Crawled property，Managed property。
        /// </summary>
        /// <param name="msg"></param>
        [OperationContract]
        List<PropertyMapping> GetMappings(MappingMessage msg);//xlliu


        // [OperationContract]
        // CrawlResult StartFullCrawl(CrawlSettingMessage msg); //xlliu
        // [OperationContract]
        // CrawlResult StartIncrementalCrawl(CrawlSettingMessage msg);//xlliu

        [OperationContract]
        void RestartSearchService(EDBaseMessage msg); //xlliu
        [OperationContract]
        void RestartIIS(EDBaseMessage msg); //xlliu
        [OperationContract]
        bool CheckorStartService(EDBaseMessage msg);

        // ------------ CrawlSettings start ------------
        [OperationContract]
        CrawlSettingResultMessage LoadSSAList(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage InstallSSA(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage UnInstallSSA(CrawlSettingMessage msg);

        [OperationContract]
        CrawlSettingResultMessage LoadContentSourceBySSA(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage DeleteContentSource(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage EditContentSource(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage SaveContentSource(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage StartIncrementalCrawl(CrawlSettingMessage msg);
        [OperationContract]
        CrawlSettingResultMessage StartFullCrawl(CrawlSettingMessage msg);

        [OperationContract]
        CrawlSettingResultMessage GetAvailableWebAppUrl(CrawlSettingMessage msg);


        [OperationContract]
        CrawlSettingResultMessage RetrieveContentSourceStatus(CrawlSettingMessage message);

    }
}
