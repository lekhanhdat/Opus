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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    //类名拼写错误，需要改一下
    public class AveSPLiminitedWebPartManager
    {
        private AveSPItem mAveSPItem = null;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPLiminitedWebPartManager(AveSPItem aveSPItem)
        {
            mAveSPItem = aveSPItem;
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPLimitedWebPartManager.Export "))
            {
                if (AveUrlUtility.IsAspx(mAveSPItem.BaseItemInfo.ServerRelativeUrl, false) && mAveSPItem.BaseItemInfo.IsCurrentVersion)
                {
                    List<AveWebPartBaseInfo> webPartInfos = mAveSPItem.WebPartInfos;
                    if (webPartInfos != null && webPartInfos.Count > 0)
                    {
                        mLog.Info("[SAAS-34134]Backup {0} webparts from url:{1}, version:{2}", webPartInfos.Count, SensitiveLogExtension.FormatURLInLog(mAveSPItem?.BaseItemInfo?.ServerRelativeUrl), mAveSPItem.BaseItemInfo.Version);
                        output.WriteMetadata(AveMetadataType.DocWebPart, webPartInfos);
                    }
                    else
                    {
                        IAveLimitedWebPartManager webpartManager = mAveSPItem.ParentSite.ObjectModelFactory.CreateLimitedWebPartManager(mAveSPItem.ParentSite.SPSite, mAveSPItem.AveSPList.ParentWeb.SPWeb, mAveSPItem.BaseItemInfo.ServerRelativeUrl);
                        try
                        {
                            webPartInfos = webpartManager.GetWebParts(mAveSPItem.BaseItemInfo);
                        }
                        catch (Exception ex)
                        {
                            mLog.Log(AveLogLevel.WARN, "An error occurred when backup web parts, Page:{0}, Version:{1}. Reason:{2}.", SensitiveLogExtension.FormatURLInLog(mAveSPItem?.BaseItemInfo?.ServerRelativeUrl), mAveSPItem.BaseItemInfo.Version, ex);
                            throw;
                        }
                        if (webPartInfos != null)
                        {
                            if (webPartInfos.Count > 0)
                            {
                                mLog.Info("[SAAS-34134]Backup {0} webparts from url:{1}, version:{2}", webPartInfos.Count, mAveSPItem.BaseItemInfo.ServerRelativeUrl, mAveSPItem.BaseItemInfo.Version);
                            }
                            output.WriteMetadata(AveMetadataType.DocWebPart, webPartInfos);
                        }
                        //处理case：page增加version，将version上的所有webpart删除，将删除还原到目的端
                        else
                        {
                            try
                            {
                                AveWebPartBaseInfo baseInfo = new AveWebPartBaseInfo();
                                baseInfo.ID = Guid.Empty;
                                webPartInfos = new List<AveWebPartBaseInfo>();
                                webPartInfos.Add(baseInfo);
                                output.WriteMetadata(AveMetadataType.DocWebPart.ToString(), webPartInfos);
                                mLog.Info("The page:{0} does not have associated WebPart.", mAveSPItem.BaseItemInfo.ServerRelativeUrl);
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Export none webpart error. {0}", ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        internal List<AveWebPartBaseInfo> GetWebParts()
        {
            List<AveWebPartBaseInfo> webPartInfos = null;
            if (AveUrlUtility.IsAspx(mAveSPItem.BaseItemInfo.ServerRelativeUrl, false))
            {
                webPartInfos = mAveSPItem.WebPartInfos;
                //if (webPartInfos != null && webPartInfos.Count > 0)
                //{
                //    output.WriteMetadata(AveMetadataType.DocWebPart, webPartInfos);
                //}
                //else
                if (webPartInfos == null || webPartInfos.Count == 0)
                {
                    using (AvePerformanceScope ps = new AvePerformanceScope("Backup.AveSPLimitedWebPartManager.GetWebParts"))
                    {
                        using (
                            IAveLimitedWebPartManager webpartManager =
                                mAveSPItem.ParentSite.ObjectModelFactory.CreateLimitedWebPartManager(
                                    mAveSPItem.ParentSite.SPSite, mAveSPItem.AveSPList.ParentWeb.SPWeb,
                                    mAveSPItem.BaseItemInfo.ServerRelativeUrl))
                        {
                            try
                            {
                                webPartInfos = webpartManager.GetWebParts(mAveSPItem.BaseItemInfo);
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN,
                                         "An error occurred when backup web parts, Page:{0}, Version:{1}. Reason:{2}.",
                                         mAveSPItem.BaseItemInfo.ServerRelativeUrl, mAveSPItem.BaseItemInfo.Version, ex);
                                throw;
                            }
                        }
                        //if (webPartInfos != null)
                        //{
                        //    output.WriteMetadata(AveMetadataType.DocWebPart, webPartInfos);
                        //}
                        ////处理case：page增加version，将version上的所有webpart删除，将删除还原到目的端
                        //else
                        if (webPartInfos == null)
                        {
                            try
                            {
                                AveWebPartBaseInfo baseInfo = new AveWebPartBaseInfo();
                                baseInfo.ID = Guid.Empty;
                                webPartInfos = new List<AveWebPartBaseInfo>();
                                webPartInfos.Add(baseInfo);
                                //output.WriteMetadata(AveMetadataType.DocWebPart.ToString(), webPartInfos);
                                mLog.Info("The page:{0} does not have associated WebPart.", mAveSPItem.BaseItemInfo.ServerRelativeUrl);
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Export none webpart error. {0}", ex.ToString());
                            }
                        }
                    }
                }
            }

            return webPartInfos;
        }
    }

    // do not use any more
    //    public class AveSPWebPart
    //    {
    //        private AveSPDoc mAveSPDoc = null;
    //        private AveSPItem mAveSPItem = null;
    //        private AveSqlConnection mSqlConn = null;
    //        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

    //        public AveSPWebPart(AveSPDoc aveSPDoc)
    //        {
    //            mAveSPDoc = aveSPDoc;
    //            mSqlConn = aveSPDoc.AveSPItem.SqlConn;
    //            mAveSPItem = aveSPDoc.AveSPItem;
    //        }

    //        public void Export(IAveBackupStream output)
    //        {
    //            List<AveWebPartBaseInfo> webPartInfos = null;
    //            if ((webPartInfos = GetWebParts()) != null)
    //            {
    //                output.WriteMetadata(AveMetadataType.DocWebPart.ToString(), webPartInfos);
    //            }
    //        }

    //        public List<AveWebPartBaseInfo> GetWebParts()
    //        {
    //            //if (mAveSPItem.IsVersion)
    //            //{
    //            //    return null;
    //            //}
    //            ////TODO: Why this flag is not working?
    //            //if ((AveDocFlags.CONTAIN_WEBPART_DOC & mDocFlag) == 0)
    //            //{
    //            //    return;
    //            //}
    //            string cmdText =
    //@"SELECT wp.tp_ID,wp.tp_ListId,wp.tp_Type,wp.tp_Flags,wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_Version,wp.tp_PartOrder,wp.tp_ZoneID,
    //         wp.tp_IsIncluded,wp.tp_FrameState,wp.tp_View,wp.tp_WebPartTypeId,wp.tp_AllUsersProperties,wp.tp_PerUserProperties,
    //         wp.tp_Cache,wp.tp_UserID,wp.tp_Source,wp.tp_CreationTime,wp.tp_Size,wp.tp_Level,wp.tp_Deleted,wp.tp_HasFGP,
    //         wp.tp_ContentTypeId,wp.tp_PageVersion,wp.tp_Assembly,wp.tp_Class,wp.tp_WebPartIdProperty,l.tp_Title AS tp_ListTitle
    //FROM AllWebParts wp LEFT JOIN AllLists l
    //ON wp.tp_ListId=l.tp_Id WHERE wp.tp_SiteId=@SiteId AND wp.tp_PageUrlId=@Id AND wp.tp_Level=@Level AND wp.tp_PageVersion=@PageVersion order by wp.tp_PartOrder ASC";
    //            mSqlConn.ClearParameters();
    //            mSqlConn.AddParameter("@SiteId", mAveSPItem.SiteId);
    //            mSqlConn.AddParameter("@Id", mAveSPItem.Id);
    //            mSqlConn.AddParameter("@Level", mAveSPItem.Level);
    //            mSqlConn.AddParameter("@PageVersion", mAveSPItem.IsVersion ? mAveSPItem.Version : 0);
    //            List<AveWebPartBaseInfo> data = AveSqlUtility.GetDBRows<AveWebPartBaseInfo>(mSqlConn, cmdText, "tp_");

    //            if (data != null && data.Count > 0)
    //            {
    //                foreach (AveWebPartBaseInfo webPartInfo in data)
    //                {
    //                    try
    //                    {
    //                        //TODO:Set the List Title Value
    //                        //webPartInfo.ListTitle = "";
    //                        webPartInfo.ListTitle = GetListTitle(mAveSPDoc.ParentSite, webPartInfo.ListId);
    //                        SetWebPartPersonalization(webPartInfo);
    //                        SetWebPartLists(webPartInfo);
    //                    }
    //                    catch (Exception e)
    //                    {
    //                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while getting web parts. \n error message:{0}", e));
    //                    }
    //                }
    //                return data;
    //            }

    //            return null;
    //        }

    //        private string GetListTitle(AveSPSite aveSite, Guid listId)
    //        {
    //            string title = string.Empty;
    //            try
    //            {
    //                if (aveSite.ListIdTitleMapping.ContainsKey(listId))
    //                    title = aveSite.ListIdTitleMapping[listId];
    //                else
    //                {
    //                    mSqlConn.AddParameter("@ListId", listId);
    //                    string cmdText = "SELECT tp_Title FROM AllLists WHERE tp_Id=@ListId";
    //                    title = (string)mSqlConn.ExecuteScalar(cmdText);
    //                    aveSite.ListIdTitleMapping.Add(listId, title);
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get list title. list id:{0}\n error message:{1}", listId, e));
    //                //mLog.Warn("GetListTitle listId: " + listId + " error: " + e.ToString());
    //            }

    //            return title;
    //        }

    //        private void SetWebPartPersonalization(AveWebPartBaseInfo webPartInfo)
    //        {
    //            string cmdText =
    //@"SELECT tp_UserID,tp_PartOrder,tp_ZoneID,tp_IsIncluded,tp_FrameState,tp_PerUserProperties,tp_Cache,tp_Size,tp_Deleted
    //FROM Personalization where tp_SiteId=@SiteId AND tp_PageUrlID=@Id AND tp_WebPartID=@WebPartID";

    //            mSqlConn.AddParameter("@WebPartID", webPartInfo.ID);
    //            webPartInfo.Personalization = AveSqlUtility.GetDBRows<AvePersonalizationInfo>(mSqlConn, cmdText, "tp_");
    //        }

    //        private void SetWebPartLists(AveWebPartBaseInfo webPartInfo)
    //        {
    //            string cmdText =
    //@"SELECT wp.tp_WebId,wp.tp_UserID,wp.tp_Level, w.FullUrl AS tp_FullUrl
    //FROM WebPartLists wp LEFT JOIN Webs w ON wp.tp_WebId=w.Id WHERE tp_SiteId=@SiteId and tp_PageUrlID=@Id AND tp_WebPartID=@WebPartID
    //";
    //            mSqlConn.AddParameter("@WebPartID", webPartInfo.ID);
    //            webPartInfo.WebPartList = AveSqlUtility.GetDBRows<AveWebPartListInfo>(mSqlConn, cmdText, "tp_");
    //        }
    //    }
}