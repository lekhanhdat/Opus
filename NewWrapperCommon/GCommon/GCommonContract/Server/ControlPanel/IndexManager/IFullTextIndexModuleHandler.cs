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
using System.Linq;
using System.Text;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.IndexManager
{
    public interface IFullTextIndexModuleHandler
    {
        /// <summary>
        /// 创建Full Text Index Job,同时更新Full Text Index Data的信息.
        /// </summary>
        void CreateFullTextIndexJob(string profileId);

        /// <summary>
        /// pause Full Text Index job.
        /// </summary>
        /// <param name="fullTextIndexJobId">Full Text Index Job Id</param>
        void PauseFullTextIndexJob(string fullTextIndexJobId);

        /// <summary>
        /// 向Media发送数据,开始进行Full Text Index的处理.
        /// </summary>
        /// <param name="FullTextIndexJobId">Full Text Index Job Id</param>
        /// <param name="mediaInfo">需要处理该Job的Media Service</param>
        /// <param name="fullTextDto">GUI数据</param>
        void SendFullTextIndexData(string fullTextIndexJobId, ServiceDto mediaInfo, FullTextIndexPolicyDto fullTextDto);

        /// <summary>
        /// 根据job状态,获取maxCount个job的信息.
        /// </summary>
        /// <param name="jobState">job状态</param>
        /// <param name="maxCount">获取job的个数</param>
        /// <param name="isAsc">true:升序,false:降序</param>
        /// <param name="profileId">profile id</param>
        /// <returns></returns>
        List<SubJobDto> GetFullTextIndexJob(JobState jobState, int maxCount, bool isAsc, string profileId);

        /// <summary>
        /// 获取当前media的所有状态是jobState的full text index job
        /// </summary>
        /// <param name="jobState">job状态</param>
        /// <param name="mediaServiceId">media service id</param>
        /// <returns></returns>
        List<SubJobDto> GetFullTextIndexJob(JobState jobState, string mediaServiceId);

        /// <summary>
        /// 如果waitting状态的full text index job没有crawl profile,则进行failed处理.
        /// </summary>
        void FailedNoCrawlProfileWaittingJob();

        /// <summary>
        /// 检查是否有正在运行的Fjob在使用当前的profile.running[waitting, running, deleting]
        /// </summary>
        /// <param name="crawlProfileId"></param>
        /// <returns></returns>
        bool CheckRunningCrawlProfileId(string crawlProfileId);

        /// <summary>
        /// 判断crawl profile是否正在被使用.full text index的各种状态.
        /// </summary>
        /// <param name="crawlProfileId"></param>
        /// <returns></returns>
        bool CheckUsingCrawlProfileId(string crawlProfileId);

        /// <summary>
        /// 根据crawl profile获取该profile下的full text index data存放的full path.
        /// </summary>
        /// <param name="crawlProfileId"></param>
        /// <returns></returns>
        FullTextIndexDownloadDto GetFullTextIndexDownloadDto(string crawlProfileId);
    }
}