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



//using System;
//using System.Collections.Generic;

//using AvePoint.Common;
//using AvePoint.GCommon;
//using AvePoint.Wrapper.Common;

//namespace AvePoint.Wrapper.Browse
//{
//    public class AveSPViewer
//    {
//        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveSPViewer));

//        #region GetWebApps

//        //public static List<AveWebAppDto> GetWebApps()
//        //{
//        //    return GetWebApps(AveObjectCheckState.All, null);
//        //}

//        //public static List<AveWebAppDto> GetWebApps(AveObjectCheckState checkState, HashSet<string> existWebApps)
//        //{
//        //    List<AveWebAppDto> webAppDtos = new List<AveWebAppDto>();
//        //    try
//        //    {
//        //        if (checkState == AveObjectCheckState.None)
//        //        {
//        //            return webAppDtos;
//        //        }
//        //        if (checkState == AveObjectCheckState.Self &&
//        //            (existWebApps == null || existWebApps.Count == 0))
//        //        {
//        //            return webAppDtos;
//        //        }
//        //        foreach (IAveWebApplication spWebApp in SPWebService.ContentService.WebApplications)
//        //        {
//        //            try
//        //            {
//        //                if (spWebApp.Status != SPObjectStatus.Online && spWebApp.Status != SPObjectStatus.Upgrading)
//        //                {
//        //                    continue;
//        //                }
//        //                string url = spWebApp.AlternateUrls.GetResponseUrl(SPUrlZone.Default).Uri.ToString();
//        //                if (checkState == AveObjectCheckState.Self
//        //                    && !existWebApps.Contains(url))
//        //                {
//        //                    continue;
//        //                }
//        //                AveWebAppDto webAppDto = new AveWebAppDto();
//        //                webAppDto.Url = url;
//        //                webAppDto.Name = webAppDto.Url;
//        //                webAppDtos.Add(webAppDto);
//        //            }
//        //            catch (Exception e)
//        //            {
//        //                mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get web application.\n error message:{0}", e));
//        //                //mLog.Error(e, "An error occurred while geting web application");
//        //            }
//        //        }
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get web application.\n error message:{0}", e));
//        //        //mLog.Error(e, "An error occurred while geting web applications");
//        //    }
//        //    webAppDtos.Sort();
//        //    return webAppDtos;
//        //}

//        #endregion GetWebApps

//        #region GetSites

//        //public static List<AveSiteDto> GetSites(AveSqlConnection sqlConn, string webAppUrl)
//        //{
//        //    return GetSites(sqlConn, webAppUrl, AveObjectCheckState.All, null);
//        //}

//        //public static List<AveSiteDto> GetSites(AveSqlConnection sqlConn, string webAppUrl, AveObjectCheckState checkState, HashSet<string> existSite)
//        //{
//        //    List<AveSiteDto> siteDtos = new List<AveSiteDto>();
//        //    try
//        //    {
//        //        if (checkState == AveObjectCheckState.None)
//        //        {
//        //            return siteDtos;
//        //        }
//        //        if (checkState == AveObjectCheckState.Self
//        //            && (existSite == null || existSite.Count == 0))
//        //        {
//        //            return siteDtos;
//        //        }
//        //        SPWebApplication spWebApp = SPWebApplication.Lookup(new Uri(webAppUrl));
//        //        if (spWebApp == null)
//        //        {
//        //            throw new AveException("Cannot find web application:{0}", webAppUrl);
//        //        }

//        //        string head = webAppUrl.StartsWith("http://") ? "http://" : "https://";
//        //        foreach (SPContentDatabase contentDatabase in spWebApp.ContentDatabases)
//        //        {
//        //            try
//        //            {
//        //                sqlConn.Open(contentDatabase.DatabaseConnectionString);
//        //                string cmdText = "SELECT Id,FullUrl FROM Webs WHERE SiteId not in (SELECT Id FROM Sites WHERE HostHeader is not null) And ParentWebId is null ORDER BY FullUrl";

//        //                using (SqlDataReader sqlReader = sqlConn.ExecuteReader(cmdText))
//        //                {
//        //                    while (sqlReader.Read())
//        //                    {
//        //                        try
//        //                        {
//        //                            Guid id = sqlReader.GetGuid(0);
//        //                            string url = webAppUrl + sqlReader.GetString(1);
//        //                            if (url.EndsWith("/"))
//        //                            {
//        //                                url = url.Substring(0, (url.Length - 1));
//        //                            }
//        //                            if (checkState == AveObjectCheckState.Self
//        //                                && !existSite.Contains(url))
//        //                            {
//        //                                continue;
//        //                            }
//        //                            AveSiteDto siteDto = new AveSiteDto();
//        //                            siteDto.Id = id;
//        //                            siteDto.Url = url;
//        //                            siteDto.Name = siteDto.Url;
//        //                            siteDto.ConnectionString = contentDatabase.DatabaseConnectionString;
//        //                            siteDtos.Add(siteDto);
//        //                        }
//        //                        catch (Exception e)
//        //                        {
//        //                            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get site.WebApp. webAppUrl:{0}\n error message:{1}", webAppUrl, e));
//        //                            //mLog.Warn(e, "An error occurred while geting site. WebApp:{0}", webAppUrl);
//        //                        }
//        //                    }
//        //                }

//        //                cmdText = "SELECT Id,HostHeader FROM Sites WHERE HostHeader is not null";
//        //                using (SqlDataReader sqlReader = sqlConn.ExecuteReader(cmdText))
//        //                {
//        //                    while (sqlReader.Read())
//        //                    {
//        //                        try
//        //                        {
//        //                            Guid id = sqlReader.GetGuid(0);
//        //                            string url = head + sqlReader.GetString(1);
//        //                            if (checkState == AveObjectCheckState.Self
//        //                                && !existSite.Contains(url))
//        //                            {
//        //                                continue;
//        //                            }
//        //                            AveSiteDto siteDto = new AveSiteDto();
//        //                            siteDto.Id = id;
//        //                            siteDto.Url = url;
//        //                            siteDto.Name = siteDto.Url;
//        //                            siteDto.ConnectionString = contentDatabase.DatabaseConnectionString;
//        //                            siteDtos.Add(siteDto);
//        //                        }
//        //                        catch (Exception e)
//        //                        {
//        //                            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get site(with HostHeader).  WebAppUrl:{0}\n error message:{1}", webAppUrl, e));
//        //                            //mLog.Warn(e, "An error occurred while geting site(with HostHeader). WebApp:{0}", webAppUrl);
//        //                        }
//        //                    }
//        //                }
//        //            }
//        //            catch (Exception e)
//        //            {
//        //                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while connecting to the content database. contentDatabase name:{0}\n error message:{1}", contentDatabase.Name, e));
//        //                //mLog.Warn(e, "An error occurred while connecting to content database:{0}", contentDatabase.Name);
//        //            }
//        //        }
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get sites. WebApp:{0}\n error message:{1}", webAppUrl, e));
//        //        //mLog.Error(e, "An error occurred while geting sites. WebApp:{0}", webAppUrl);
//        //    }
//        //    return siteDtos;
//        //}

//        #endregion GetSites

//        #region GetWebs

//        public static List<AveWebDto> GetWebs(AveSqlConnection sqlConn, Guid siteId)
//        {
//            return GetWebs(sqlConn, siteId, AveObjectCheckState.All, null);
//        }

//        public static List<AveWebDto> GetWebs(AveSqlConnection sqlConn, Guid siteId, AveObjectCheckState checkState, HashSet<string> existWebs)
//        {
//            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.Wrapper.Browse.AveSPViewer.GetWebs"))
//            {
//                List<AveWebDto> webDtos = new List<AveWebDto>();
//                try
//                {
//                    if (checkState == AveObjectCheckState.None)
//                    {
//                        return webDtos;
//                    }
//                    if (checkState == AveObjectCheckState.Self
//                        && (existWebs == null || existWebs.Count == 0))
//                    {
//                        return webDtos;
//                    }
//                    string cmdText = "SELECT Id,FullUrl,Title FROM Webs WHERE SiteId=@SiteId ORDER BY FullUrl";
//                    sqlConn.AddParameter("@SiteId", siteId);
//                    using (SqlDataReader sr = sqlConn.ExecuteReader(cmdText))
//                    {
//                        int len = -1;
//                        while (sr.Read())
//                        {
//                            try
//                            {
//                                Guid id = sr.GetGuid(0);
//                                string name = sr.GetString(1);
//                                if (len < 0)
//                                {
//                                    len = name.Length;
//                                    name = AveWebDto.ROOT_NAME;
//                                }
//                                else
//                                {
//                                    if (len > 0)
//                                    {
//                                        name = name.Substring(len + 1);
//                                    }
//                                }
//                                if (checkState == AveObjectCheckState.Self
//                                    && !existWebs.Contains(name))
//                                {
//                                    continue;
//                                }

//                                AveWebDto webDto = new AveWebDto();
//                                webDto.ID = id;
//                                webDto.Name = name;
//                                if (!sr.IsDBNull(2))
//                                {
//                                    webDto.Title = sr.GetString(2);
//                                }
//                                else
//                                {
//                                    webDto.Title = string.Empty;
//                                }
//                                webDtos.Add(webDto);
//                            }
//                            catch (Exception e)
//                            {
//                                mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get web. SiteId:{0}\n error message:{1}", siteId, e));
//                                //mLog.Warn(e, "An error occurred while geting web. SiteId:{0}", siteId);
//                            }
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get webs. Site Id:{0}\n error message:{1}", siteId, e));
//                    //mLog.Error(e, "An error occurred while geting webs. SiteId:{0}", siteId);
//                }
//                return webDtos;
//            }
//        }

//        #endregion GetWebs

//        //#region GetLists

//        //public static List<AveListDto> GetLists(AveSqlConnection sqlConn, Guid webId)
//        //{
//        //    return GetLists(sqlConn, webId, AveObjectCheckState.All, null);
//        //}

//        //public static List<AveListDto> GetLists(AveSqlConnection sqlConn, Guid webId, AveObjectCheckState checkState, HashSet<string> existLists)
//        //{
//        //    using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.Wrapper.Browse.AveSPViewer.GetLists"))
//        //    {
//        //        List<AveListDto> listDtos = new List<AveListDto>();
//        //        try
//        //        {
//        //            if (checkState == AveObjectCheckState.None)
//        //            {
//        //                return listDtos;
//        //            }
//        //            if (checkState == AveObjectCheckState.Self
//        //                && (existLists == null || existLists.Count == 0))
//        //            {
//        //                return listDtos;
//        //            }
//        //            string cmdText = "SELECT tp_ID,tp_Title,tp_BaseType,tp_ServerTemplate FROM AllLists WHERE tp_WebId=@WebId AND tp_DeleteTransactionId=0x ORDER BY tp_Title";
//        //            sqlConn.AddParameter("@WebId", webId);
//        //            using (SqlDataReader sr = sqlConn.ExecuteReader(cmdText))
//        //            {
//        //                while (sr.Read())
//        //                {
//        //                    try
//        //                    {
//        //                        Guid id = sr.GetGuid(0);
//        //                        string name = sr.GetString(1);
//        //                        if (checkState == AveObjectCheckState.Self
//        //                            && !existLists.Contains(name))
//        //                        {
//        //                            continue;
//        //                        }
//        //                        AveListDto listDto = new AveListDto();
//        //                        listDto.ID = id;
//        //                        listDto.Name = name;
//        //                        if (sr.GetInt32(2) == 0)
//        //                        {
//        //                            listDto.Type = AveObjectType.List;
//        //                        }
//        //                        else
//        //                        {
//        //                            listDto.Type = AveObjectType.DocList;
//        //                        }
//        //                        try
//        //                        {
//        //                            listDto.TemplateType = (AveListTemplateType)sr.GetInt32(3);
//        //                        }
//        //                        catch (Exception e)
//        //                        {
//        //                            mLog.Log(AveLogLevel.WARN, string.Format("Unknown list template. List Id:{0}, List title:{1}, list template:{2}\n error message:{3}", listDto.ID, listDto.Name, sr.GetInt32(3), e));
//        //                            //mLog.Warn(e, "Unknown list template. ListId:{0}, ListTitle:{1}, ListTemplate:{2}", listDto.Id, listDto.Name, sr.GetInt32(3));
//        //                            listDto.TemplateType = AveListTemplateType.InvalidType;
//        //                        }
//        //                        listDtos.Add(listDto);
//        //                    }
//        //                    catch (Exception e)
//        //                    {
//        //                        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get list. web id:{0}\n error message:{1}", webId, e));
//        //                        //mLog.Warn(e, "An error occurred while geting list. WebId:{0}", webId);
//        //                    }
//        //                }
//        //            }
//        //        }
//        //        catch (Exception e)
//        //        {
//        //            mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get lists. web id:{0}\n error message:{1}", webId, e));
//        //            //mLog.Error(e, "An error occurred while geting lists. WebId:{0}", webId);
//        //        }
//        //        return listDtos;
//        //    }
//        //}

//        //#endregion GetLists

//        #region GetFolders

//        public static List<AveFolderDto> GetFolders(AveSqlConnection sqlConn, Guid parentId)
//        {
//            return GetFolders(sqlConn, parentId, AveObjectCheckState.All, null);
//        }

//        public static List<AveFolderDto> GetFolders(AveSqlConnection sqlConn, Guid parentId, AveObjectCheckState checkState, HashSet<string> existFolders)
//        {
//            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.Wrapper.Browse.AveSPViewer.GetFolders"))
//            {
//                List<AveFolderDto> folderDtos = new List<AveFolderDto>();
//                try
//                {
//                    if (checkState == AveObjectCheckState.None)
//                    {
//                        return folderDtos;
//                    }
//                    if (checkState == AveObjectCheckState.Self
//                        && (existFolders == null || existFolders.Count == 0))
//                    {
//                        return folderDtos;
//                    }
//                    string cmdText = "SELECT Id,LeafName FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND DeleteTransactionId = 0x AND Type=1 AND IsCurrentVersion=1 ORDER BY LeafName";
//                    //sqlConn.AddParameter("@SiteId", siteId); If it need site id, please uncomment this line.
//                    sqlConn.AddParameter("@ParentId", parentId);
//                    using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
//                    {
//                        try
//                        {
//                            while (dr.Read())
//                            {
//                                Guid id = dr.GetGuid(0);
//                                string name = dr.GetString(1);
//                                if (checkState == AveObjectCheckState.Self
//                                    && !existFolders.Contains(name))
//                                {
//                                    continue;
//                                }
//                                AveFolderDto folderDto = new AveFolderDto();
//                                folderDto.ID = id;
//                                folderDto.Name = name;
//                                folderDtos.Add(folderDto);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get folder. ParentId:{0}\n error message:{1}", parentId, e));
//                            //mLog.Warn(e, "An error occurred while geting folder. ParentId:{0}", parentId);
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get folders. parentId:{0}\n error message:{1}", parentId, e));
//                    //mLog.Error(e, "An error occurred while geting folders. ParentId:{0}", parentId);
//                }
//                return folderDtos;
//            }
//        }

//        #endregion GetFolders

//        #region GetItems

//        public static List<AveItemDto> GetItems(AveSqlConnection sqlConn, Guid parentId)
//        {
//            return GetItems(sqlConn, parentId, AveObjectCheckState.All, null);
//        }

//        public static List<AveItemDto> GetItems(AveSqlConnection sqlConn, Guid parentId, AveObjectCheckState checkState, HashSet<string> existItems)
//        {
//            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.Wrapper.Browse.AveSPViewer.GetItems"))
//            {
//                List<AveItemDto> itemDtos = new List<AveItemDto>();
//                try
//                {
//                    if (checkState == AveObjectCheckState.None)
//                    {
//                        return itemDtos;
//                    }
//                    if (checkState == AveObjectCheckState.Self
//                        && (existItems == null || existItems.Count == 0))
//                    {
//                        return itemDtos;
//                    }
//                    string cmdText = "SELECT Id,LeafName FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND DeleteTransactionId = 0x AND Type=0 AND IsCurrentVersion=1 ORDER BY LeafName";
//                    //sqlConn.AddParameter("@SiteId", siteId); If it need site id, please uncomment this line.
//                    sqlConn.AddParameter("@ParentId", parentId);
//                    using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
//                    {
//                        try
//                        {
//                            while (dr.Read())
//                            {
//                                Guid id = dr.GetGuid(0);
//                                string name = dr.GetString(1);
//                                if (checkState == AveObjectCheckState.Self
//                                    && !existItems.Contains(name))
//                                {
//                                    continue;
//                                }
//                                AveItemDto itemDto = new AveItemDto();
//                                itemDto.ID = id;
//                                itemDto.Name = name;
//                                itemDtos.Add(itemDto);
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while get item. parentId:{0}\n error message:{1}", parentId, e));
//                            //mLog.Warn(e, "An error occurred while geting item. ParentId:{0}", parentId);
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get items. parentId:{0}\n error message:{1}", parentId, e));
//                    //mLog.Error(e, "An error occurred while geting items. ParentId:{1}", parentId);
//                }
//                return itemDtos;
//            }
//        }

//        #endregion GetItems

//        #region GetListRootFolder

//        public static AveFolderDto GetListRootFolder(AveSqlConnection sqlConn, Guid listId)
//        {
//            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.Wrapper.Browse.AveSPViewer.GetListRootFolder"))
//            {
//                AveFolderDto folderDto = null;
//                try
//                {
//                    string cmdText = "SELECT Id,LeafName FROM AllDocs ad INNER JONI AllLists al ON al.tp_ListId=@ListId AND ad.Id=al.tp_RootFolder AND DeleteTransactionId = 0x AND Type=0 AND IsCurrentVersion=1";
//                    sqlConn.AddParameter("@ListId", listId);
//                    using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
//                    {
//                        if (dr.Read())
//                        {
//                            folderDto = new AveFolderDto();
//                            folderDto.ID = dr.GetGuid(0);
//                            folderDto.Name = dr.GetString(1);
//                            folderDto.Level = 3; //Same as list level
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    mLog.Log(AveLogLevel.ERROR, string.Format("An error occurred while get list root folder. list id:{0}\n error message:{1}", listId, e));
//                    //mLog.Error(e, "An error occurred while geting list root folder. ListId:{0}", listId);
//                }
//                return folderDto;
//            }
//        }

//        #endregion GetListRootFolder
//    }
//}