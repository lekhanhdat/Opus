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
using System.Text;
using AvePoint.GCommon.Contract.Media.Object;

namespace AvePoint.Wrapper.QueryService
{
    using AvePoint.Wrapper.Common;
    using System.Diagnostics.CodeAnalysis;

    //把Discover相关的逻辑拼装或引用common的sql放到此处，抽成方法，是否需要16把这些sql重写一遍再做考虑
    //Discover Query语句抽离原则，
    //1. 13中特有的语句，在QueryService中直接使用的，在16重写一遍，
    //2. DiscoverReader的语句，不能直接抽出方法或者写出固定语句的，先加todo,后续会再review下这种case
    //3. 语句在Wrapper Common中的，直接加方法，方法中返回对应的语句，后续会再review所有的方法
    [QueryCommandString(SPDatabaseVersion.SharePoint2016TAP1, QueryCommandType.Select)]
    internal static class SP2016DiscoverQueryStringSelect
    {
        private static readonly BusinessLayerForDiscover discoverCommon = new BusinessLayerForDiscover();

        #region 公共方法

        /// <summary>
        /// 拼接动态语句用的,添加一个query column doc.DirName
        /// </summary>
        /// <param name="commText"></param>
        /// <returns></returns>
        private static string AddAllDocsDirName(string commText)
        {
            return commText.Replace("FROM", ",doc.DirName FROM");
        }

        /// <summary>
        /// 拼接动态语句用的,添加一个query column doc.ParentId
        /// </summary>
        /// <param name="commText"></param>
        /// <returns></returns>
        private static string AddAllDocsParentId(string commText)
        {
            return commText.Replace("FROM", ",doc.ParentId FROM");
        }

        /// <summary>
        ///拼接动态语句用的,添加两个query column doc.ParentId doc.DirName
        /// </summary>
        /// <param name="commText"></param>
        /// <returns></returns>
        private static string AddAllDocsDirNameAndParentId(string commText)
        {
            return AddAllDocsDirName(AddAllDocsParentId(commText));
        }

        #endregion

        #region Discover

        /// <summary>
        /// 查询单个item上attachment信息
        /// </summary>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        public static string GetSingleItemAttachments_Select_AllDocs(IAveDiscoverReader discoverReader)
        {
            return discoverReader.GetSingleItemAttachmentsQueryString();
        }

        /// <summary>
        /// 查询某些只有URL 的folder 的属性信息
        /// </summary>
        /// <param name="discoverReader"></param>
        /// <param name="urlsCondition">这个条件是动态拼接的</param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private static string GetFolderPropertiesQueryWithDynamicUrlCondition_Select_AllDocs(IAveDiscoverReader discoverReader,string urlsCondition)
        {
            return AddAllDocsDirNameAndParentId(discoverReader.GetAllItemsInAllDocQueryString())
                        .Replace("@WHERE", DiscoverConditionString.FolderURLs)
                        .Replace("@Urls",urlsCondition);
        }

        /// <summary>
        /// @siteId,其他condition都是拼接的
        /// </summary>
        /// <param name="discoverReader"></param>
        /// <param name="needSearchFolders"></param>
        /// <param name="index"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static string GetFolderPropertiesByUrls_Select_AllDocs(IAveDiscoverReader discoverReader, List<AveItemObject> needSearchFolders, ref int index,int batchSize)
        {
            var urlCondition = new StringBuilder();
            for (var i = 0; index < needSearchFolders.Count && i < batchSize; i++)
            {
                var needSearchFolder = needSearchFolders[index++];
                string leafName;
                string dirName;
                AveUrlUtility.SplitUrl(needSearchFolder.FullUrl, out dirName, out leafName);
                urlCondition.Append("OR doc.DirName=N'" + dirName.Replace("'", "''") + "' AND doc.LeafName=N'" + leafName.Replace("'", "''") + "'");
            }
            var commText = GetFolderPropertiesQueryWithDynamicUrlCondition_Select_AllDocs(discoverReader, urlCondition.ToString().TrimStart('O', 'R'));
            return commText;
        }

        /// <summary>
        /// 根据UD表查询所有item version信息
        /// </summary>
        /// <param name="discoverReader"></param>
        /// <param name="condition">可能是动态拼接，可能是固定condition</param>
        /// <returns></returns>
        public static string GetItemVersions_Select_AllUserData(IAveDiscoverReader discoverReader, string condition)
        {
            var command = discoverReader.GetAllVersionsQueryString();
            if (discoverReader is AveExtenderDiscoverReader)
            {
                command = command.Replace("@WHERE", condition);
            }
            else
            {
                command = command.Replace("@WHERE", condition + " AND data.tp_RowOrdinal = 0 ");
            }
            return command;
        }

        /// <summary>
        /// 根据ParentId下一层所有item version信息
        /// </summary>
        /// <param name="includeRecycleBin"></param>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        public static string GetWebItemVersionsByParentId_Select_AllUserDataORAllVersions(bool includeRecycleBin, AveDiscoverReader discoverReader)
        {
            string condition;
            if (discoverReader is AveExtenderDiscoverReader)
            {
                condition = discoverReader.GetWebItemVersionCondition(includeRecycleBin);
            }
            else
            {
                condition = discoverReader.GetWebItemVersionCondition(includeRecycleBin) + " AND data.tp_RowOrdinal = 0 ";
            }
            return discoverReader.GetAllVersionsQueryString(includeRecycleBin).Replace("@WHERE", condition);
        }

        public static string GetListItemVersionsByParentId_Select_AllUserDataORAllVersions(bool includeRecycleBin, AveDiscoverReader discoverReader)
        {
            string condition;
            if (discoverReader is AveExtenderDiscoverReader)
            {
                condition = discoverReader.GetListItemVersionCondition(includeRecycleBin);
            }
            else
            {
                condition = discoverReader.GetListItemVersionCondition(includeRecycleBin) + " AND data.tp_RowOrdinal = 0 ";
            }
            return discoverReader.GetAllVersionsQueryString(includeRecycleBin).Replace("@WHERE", condition);
        }

        public static string GetListItemInfoByDocId_Select_AllDocs(IAveDiscoverReader discoverReader)
        {
            return AddAllDocsDirName(discoverReader.GetAllItemsInAllDocQueryString()).Replace("@WHERE", DiscoverConditionString.ListItemExits);
        }

        public static string GetDocumentInfoByName_Select_AllDocs(IAveDiscoverReader discoverReader)
        {
            return AddAllDocsParentId(discoverReader.GetAllItemsInAllDocQueryString()).Replace("@WHERE", DiscoverConditionString.DocumentExits);
        }

        /// <summary>
        /// only for extender,other module is empty
        /// </summary>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "docver")]
        public static string GetItemVersionsStubInfo_Select_AllDocs_AllDocVersions_DocsToStreams_DocStreams(IAveDiscoverReader discoverReader)
        {
            var commText = discoverReader.GetAllItemAndVersionsStubInfoQueryString();
            return commText.Replace("@WHEREAllDocs", DiscoverConditionString.StubInfoByIdForAllDocs)
                .Replace("@WHEREAllDocVersions", DiscoverConditionString.StubInfoByIdForAllDocVersions);
        }

        public static string GetSingleItemAttachmentsStubInfo_Select_AllDocs_DocsToStreams_DocStreams(IAveDiscoverReader discoverReader)
        {
            return discoverReader.GetAllAttachmentsStubInfoQueryString();
        }

        /// <summary>
        /// 根据ParentId查询folder下item和folder信息
        /// todo:wbhu,extender 10重写语句连表AllDocStreams，13没重写，调用会有问题，不确定extender13会不会调用
        /// </summary>
        /// <param name="includeRecycleBin"></param>
        /// <param name="isInSystemFolder"></param>
        /// <param name="discoverReader"></param>
        /// <returns></returns>
        public static string GetAllItemsByParentId_Select_AllDocs(bool includeRecycleBin, bool isInSystemFolder, IAveDiscoverReader discoverReader)
        {
            var baseString = discoverReader.GetAllItemsInAllDocQueryString();
            return baseString.Replace("@WHERE", isInSystemFolder
                ? includeRecycleBin ? DiscoverConditionString.WebItemsWithRecycleBin : DiscoverConditionString.WebItems
                : includeRecycleBin ? DiscoverConditionString.ListItemsWithRecycleBin : DiscoverConditionString.ListItems);
        }

        public static string GetAllItemAttachments_Select_AllDocs(bool includeRecycleBin, IAveDiscoverReader discoverReader)
        {
            return includeRecycleBin ? discoverReader.GetAttachmentsWithRecycleBinQueryString() : discoverReader.GetAttachmentsQueryString();
        }

        /// <summary>
        /// 等接口改后，可以传入DiscoverReader，在QueryService里处理数据的组装和sql语句拼接
        /// </summary>
        /// <param name="itemColumns"></param>
        /// <returns></returns>
        public static string GetListRootFolder_Select_AllLists_AllDocs(string itemColumns)
        {
            return AveDiscoverQueryString16.ListRootFolder.Replace("@Column", itemColumns);
        }

        public static string GetViewDocInfoByIds_Select_AllDocs(List<Guid> docIds,IAveDiscoverReader discoverReader)
        {
            var condition = string.Format(DiscoverConditionString.DocIdsFor13, AveQueryStringCommonUtility.GetCondByCommaSeparatedList(docIds));
            //由于IB的时候，需要查询比FB多查询个列  DirName, 所以在这里把DirName加入查询列里
            return AddAllDocsDirName(discoverReader.GetAllItemsInAllDocQueryString().Replace("@WHERE", condition));
        }

        public static string GetItemDocInfosByItemIdInAlert_Select_AllDocs(List<int> itemIds,string rootFolderUrl,IAveDiscoverReader discoverReader)
        {
            var dirNameCondition = $"{rootFolderUrl}%";
            var idCollectionString = AveQueryStringCommonUtility.GetCondByCommaSeparatedWithoutQuoteList(itemIds);
            var itemAlertCondition = string.Format(DiscoverConditionString.ItemDocLibRowIds, dirNameCondition, idCollectionString);
            //IB需要查询DirName。ParentId用来查询AUD表时，补全索引。
            return AddAllDocsDirNameAndParentId(discoverReader.GetAllItemsInAllDocQueryString()).Replace("@WHERE", itemAlertCondition);
        }

        public static string GetItemVersionsInUDByDocId_Select_AllUserData(AveDiscoverReader discoverReader,List<Guid> docIds)
        {
            string condition;
            if(discoverReader is  AveExtenderDiscoverReader)
            {
                condition = discoverReader.GetItemVersionsWithDocIdsCondition();
            }
            else
            {
                condition = discoverReader.GetItemVersionsWithDocIdsCondition() + " AND data.tp_RowOrdinal = 0 ";
            }
            return GetItemVersions_Select_AllUserData(discoverReader, condition);
        }

        public static string GetWebRootFolderInDocs_Select_AllDocs(IAveDiscoverReader discoverReader)
        {
            return AveDiscoverQueryString.WebRootFolder.Replace("@Column", discoverReader.GetItemColumns());
        }


        #endregion Discover
    }
}
