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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    public interface IAveCentralAdminQueryService
    {
        #region Central Admin

        /// <summary>
        /// 获取数据库所在磁盘的剩余空间和该数据库占用空间和可用空间,无API实现.
        /// </summary>
        /// <param name="usedSize">DB使用空间，单位MB</param>
        /// <param name="freeSize">DB可用空间，单位MB</param>
        /// <param name="diskFreesize">磁盘剩余空间，单位MB</param>
        void GetDBSize(out double usedSize, out double freeSize, out double diskFreesize);

        /// <summary>
        /// 获取SQL 服务器所在机器的HostName,API方式有缺陷，有API实现.
        /// </summary>
        /// <returns></returns>
        string GetDBServerName();

        /// <summary>
        /// 根据传入的siteIdFilter 条件查询出对应的site 信息，以IAveQueryDataReader形式返回查询结果
        /// </summary>
        /// <param name="siteIdFilter"></param>
        /// <param name="appUrl"></param>
        /// <param name="appSuffix"></param>
        /// <returns></returns>
        IAveQueryDataReader GetOrphanSite(string siteIdFilter, string appUrl, string appSuffix);

        /// <summary>
        /// 根据webapp Id获取一个WebApp下所有SiteCollection的Id拼接的query条件,效率考虑，有API实现.（查询ConfigDB） 
        /// </summary>
        /// <param name="webAppId"></param>
        /// <returns></returns>
        string GetSiteIds(Guid webAppId);

        /// <summary>
        /// 获取无权限的Users和Groups,无API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <param name="searchUsers"></param>
        /// <returns></returns>
        IAveQueryDataReader GetSiteNoPermissionAccounts(Guid siteId, Guid scopeId, List<string> searchUsers);

        /// <summary>
        /// 查询Web下最顶端Navigation关联的Document信息
        /// 效率考虑，有API实现.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        string GetDocNameFromDB(Guid siteId, Guid webId);

        /// <summary>
        /// 将Orphan Site删除到回收站，无API实现.
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="itemId"></param>
        void RecycleOrphanSiteInDB(IAveContentDatabase dataBase, string itemId);
        void DeleteOrphanSiteInDB(IAveContentDatabase dataBase, string itemId);
        /// <summary>
        /// 得到web里所有的page的相对url(不包括subsite中的)，无API实现
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        [Obsolete("not used any more, will be removed later, use GetAllPageInWeb instead.")]
        List<string> GetAllPageOfWeb(IAveWeb web);
        /// <summary>
        /// 效率考虑，暂无API实现，可以用于获取当前web下所有page,不包括subsite中page
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="isCurrentVersion"></param>
        /// <returns>key is docId,value is url</returns>
        Dictionary<Guid, string> GetAllPageInWeb(Guid siteId, Guid webId, bool isCurrentVersion = true);
        /// <summary>
        /// 递归调用GetAllPagesByParentId，可能存在效率问题
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        List<string> GetAllPage(Guid siteId, Guid parentId);

        /// <summary>
        /// 效率考虑，暂无API实现，可以用于获取folder下的所有page,
        /// Note：如果传入的是webUrl，那么获取的是web以及web的subsite中的所有page
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentUrl">example sites/de/subEnglish/Shared Documents,  DirName格式 or API ServerRelativeUrl</param>
        /// <returns></returns>
        Dictionary<Guid, string> GetAllPage(Guid siteId, string parentUrl);

        /// <summary>
        /// 经过修改，当前此方法07与其他模块的处理方式不同。
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webPartKey"></param>
        /// <param name="webpartNameTemp"></param>
        /// <returns></returns>
        IAveQueryDataReader WebAddWebPartMessageHandler(Guid siteId, string webPartKey, string webpartNameTemp);

        /// <summary>
        /// 获取某一scope下的重复文件信息，无API实现
        /// </summary>
        /// <param name="siteIds"></param>
        /// <param name="webIds"></param>
        /// <param name="excludeFileNames"></param>
        /// <param name="fileNamePattern"></param>
        /// <param name="includeFileExtensions"></param>
        /// <param name="searchFile"></param>
        /// <param name="searchAttachment"></param>
        /// <returns></returns>
        IAveQueryDataReader SearchDuplicateFiles(List<string> siteIds, List<string> webIds, List<string> excludeFileNames, string fileNamePattern, List<string> includeFileExtensions, bool searchFile, bool searchAttachment);

        #endregion
    }
}
