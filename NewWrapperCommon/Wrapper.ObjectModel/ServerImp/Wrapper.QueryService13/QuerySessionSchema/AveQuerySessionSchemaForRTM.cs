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
using AvePoint.Wrapper.Common;
using System.Data.SqlClient;
using AvePoint.GCommon;
using System.Data;
using System.IO;
using System.Xml;
using System.Globalization;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.QueryService
{
    class AveQuerySessionSchemaForRTM : AveQuerySessionSchema, IAveQuerySessionSchema
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveQueryService));

        public AveQuerySessionSchemaForRTM(AveQueryWorker queryWorker)
            : base(queryWorker)
        { }

        #region Replicator

        public IAveQueryDataReader GetAllWebs()
        {
            string cmdText = @"Select Sites.Id as SiteID,
                                             Webs.Id as WebID,
                                             (case when Webs.Id=Sites.RootWebId Then CAST(1 AS bit) Else CAST(0 AS bit) End) as IsRootWeb,
                                             Webs.FullUrl,
                                             Hostheader
                                             From Sites With(NoLock) Inner Join Webs With(NoLock) On Webs.SiteId=Sites.Id Order By SiteID, IsRootWeb Desc;";
            try
            {
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmdText));
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public void GetNewWebsByContentDB(Dictionary<Guid, Guid> newWebs, DateTime startTime, DateTime endTime, StringBuilder sBuilder)
        {
            startTime = startTime.AddSeconds(-2.0);//避免时间误差造成遗漏。
            string cmdText = @"Select e.WebId, e.SiteId, w.FullUrl From EventCache as e with(nolock), Webs as w with(nolock)Where e.ObjectType=4 And e.EventType=4096
                                             And e.WebId=w.Id And e.EventTime Between @StartTime And @EndTime;";
            try
            {
                mQueryWorker.AddParameter("@StartTime", startTime);
                mQueryWorker.AddParameter("@EndTime", endTime);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        //ADO-10271不同sitecollection下可能有相同webid，如果用webid作为key会出现问题 //相同webid是存在于不同的contentDB中的，因此去掉
                        Guid webId = reader.GetGuid(0);
                        Guid siteId = reader.GetGuid(1);
                        string serverRelativeUrl = reader.GetString(2);
                        //string keyWebValue = webId.ToString()；// + serverRelativeUrl;
                        newWebs.Add(webId, siteId);
                        sBuilder.AppendFormat("\r\nFind new subWeb added. WebId: {0} WebName: {1} StartTime: {2} EndTime: {3}", webId, serverRelativeUrl, startTime, endTime);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public void GetAllWebsByContentDB(IAveContentDatabase dataBase, Dictionary<Guid, Guid> allWebs)
        {
            string cmdText = @"Select ID, SiteId, ParentWebId From Webs with (nolock);";
            try
            {
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(2))
                        {
                            allWebs.Add(reader.GetGuid(0), reader.GetGuid(1));
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

        }

        public IAveQueryDataReader GetAllEventReceivers(string assemblyFullName)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@AssemblyFullName", assemblyFullName);
            string cmdText = "Select SiteId, WebId, Type From EventReceivers With(NoLock) Where Assembly=@AssemblyFullName And SiteId In(Select Id From Sites With(NoLock));";
            try
            {
                return new AveQueryDataReader(mQueryWorker.ExecuteReader(cmdText));
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        #endregion

        #region CA

        public IAveQueryDataReader GetOrphanSite(string siteIdFilter, string appUrl, string appSuffix)
        {

            string text = @"select s.id,
                                  (case when s.hostheader is null then @appUrl+w.fullurl
                                   else @appSuffix + s.hostheader end) as url, w.title 
                                   from sites s with (nolock) 
                                   inner join webs w with (nolock) 
                                   on s.id=w.siteid and w.parentwebid is null {0}";

            string commandText = string.Format(text, siteIdFilter);
            try
            {
                using (SqlCommand cmd = mQueryWorker.CreateCommand())
                {
                    cmd.CommandText = commandText;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue(@"appSuffix", appSuffix);
                    cmd.Parameters.AddWithValue(@"appUrl", appUrl);
                    return new AveQueryDataReader(cmd.ExecuteReader());
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        #endregion

        #region Discover

        public AveWebObject QueryRootWeb(Guid siteId)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QueryRootWeb");

            AveWebObject rootWebObj = null;
            mQueryWorker.AddParameter("@SiteId", siteId);

            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.RootWeb))
                {
                    if (sr.Read())
                    {
                        Guid webId = sr.GetGuid(0);
                        try
                        {
                            rootWebObj = new AveWebObject()
                            {
                                WebID = webId,
                                Name = ".",
                                FullUrl = sr.GetString(1),
                                Title = sr.IsDBNull(2) ? string.Empty : sr.GetString(2)
                            };

                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from method QueryRootWeb.SiteId:{0}. WebId:{1}. ErrorMessage:{2}", siteId.ToString(), webId.ToString(), e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QueryRootWeb");
            }
            return rootWebObj;
        }

        public Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.GetSubWebs");

            Dictionary<Guid, AveWebObject> subWebObjs = new Dictionary<Guid, AveWebObject>();

            mQueryWorker.AddParameter("@siteId", siteId);
            mQueryWorker.AddParameter("@ParentId", parentWebId);

            int len = -1;
            string title = string.Empty;
            string fullUrl = string.Empty;
            string name = string.Empty;

            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.SubWebs))
                {
                    while (sr.Read())
                    {
                        Guid webId = sr.GetGuid(0);
                        try
                        {
                            fullUrl = sr.GetString(1);

                            title = sr.IsDBNull(2) ? string.Empty : sr.GetString(2);
                            if (len < 0)
                            {
                                len = fullUrl.Length;//root Web
                            }
                            else
                            {
                                name = fullUrl.Substring(len).TrimStart('/');
                                AveWebObject web = new AveWebObject
                                {
                                    WebID = webId,
                                    FullUrl = sr.GetString(1),
                                    Name = name,
                                    Title = sr.IsDBNull(2) ? string.Empty : sr.GetString(2)
                                };
                                subWebObjs.Add(webId, web);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Exception occur while access data from QueryWebRootFolder. SiteId:{0}. ParentWebId:{1}. CurrentWebId:{2}. ErrorMessage:{3}", siteId.ToString(), parentWebId.ToString(), webId.ToString(), e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.GetSubWebs");
            }
            return subWebObjs;
        }

        public void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.InitDiscoverWeb");

            mQueryWorker.AddParameter("@SiteId", webCache.SiteId);
            mQueryWorker.AddParameter("@FullUrl", webObj.FullUrl);

            try
            {
                int len = -1;
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.Web))
                {
                    while (sr.Read())
                    {
                        Guid webId = sr.GetGuid(0);
                        try
                        {
                            string fullUrl = sr.GetString(1);
                            string title = sr.GetString(2);

                            if (fullUrl.Equals(webObj.FullUrl))
                            {
                                webObj.WebID = webId;
                                webObj.FullUrl = fullUrl;
                                webObj.Title = title;
                                if (len > 0)
                                {
                                    webObj.Name = fullUrl.Substring(len).TrimStart('/');
                                }
                                else//Query Web Is Root Web
                                {
                                    webObj.Name = ".";
                                }
                            }
                            else//must root Web
                            {
                                len = sr.GetString(1).Length;
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from method InitDiscoverWeb.SiteId:{0}. WebId:{1}.  ErrorMessage:{2}", webCache.SiteId.ToString(), webCache.WebId.ToString(), e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.InitDiscoverWeb");
            }
        }

        public Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QuerySiteWebForFB");

            mQueryWorker.AddParameter("@SiteId", siteId);
            var webObjs = new Dictionary<Guid, AveWebObject>();
            try
            {
                int len = -1;
                string title = string.Empty;
                string fullUrl = string.Empty;
                string name = string.Empty;
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.Webs))
                {
                    while (sr.Read())
                    {
                        Guid webId = sr.GetGuid(0);
                        try
                        {
                            fullUrl = sr.GetString(1);

                            title = sr.IsDBNull(2) ? string.Empty : sr.GetString(2);
                            if (len < 0)
                            {
                                len = fullUrl.Length;
                                name = ".";
                            }
                            else
                            {
                                name = fullUrl.Substring(len).TrimStart('/');
                            }
                            AveWebObject web = new AveWebObject
                            {
                                WebID = webId,
                                Name = name,
                                Title = title,
                                FullUrl = fullUrl
                            };
                            webObjs.Add(webId, web);
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from method QuerySiteWebForFB. SiteId:{0}. WebId:{1}. ErrorMessage:{2}", siteId.ToString(), webId.ToString(), e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QuerySiteWebForFB");
            }
            return webObjs;
        }

        public void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject, ref AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QueryWebRootFolder");
            listObject = null;
            noPropertyFolders.Clear();
            mQueryWorker.AddParameter("@siteId", listCache.SiteId);
            mQueryWorker.AddParameter("@webId", listCache.WebId);
            try
            {
                string commText = ReplaceDirNameAndLeafName((string)mQueryWorker.ExecuteScalar(AveDiscoverQueryString.WebFullUrlById), AveDiscoverQueryString.WebRootFolder.Replace("@Column", discoverReader.GetItemColumns()));
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(commText))
                {
                    if (sr.Read())
                    {
                        try
                        {
                            discoverReader.ReadItemContent(rootFolderObject, sr);
                            rootFolderObject.ObjType = ItemType.Folder;
                            rootFolderObject.DirName = (string)sr["DirName"];
                            rootFolderObject.FullUrl = (rootFolderObject.DirName + "/" + rootFolderObject.LeafName).Trim('/');
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Exception occur while access data from QueryWebRootFolder. SiteId:{0}. WebId:{1}. ErrorMessage:{2}", listCache.SiteId.ToString(), listCache.WebId.ToString(), e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QueryWebRootFolder");
            }
        }

        public Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QueryWebForIB");

            Dictionary<Guid, AveWebObject> changeWebObjs = new Dictionary<Guid, AveWebObject>();
            mQueryWorker.AddParameter("@endTime", endTime);
            mQueryWorker.AddParameter("@startTime", startTime);
            mQueryWorker.AddParameter("@siteId", siteId);
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.WebChanged))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(1);
                            var ObjType = (ChangeObjectType)sr.GetValue(2);
                            Guid webId = sr.GetGuid(3);
                            AveWebObject webObj = null;
                            if (!changeWebObjs.ContainsKey(webId))
                            {
                                string title = string.Empty;
                                string fullUrl = string.Empty;
                                string name = string.Empty;
                                if (!sr.IsDBNull(7))
                                {
                                    fullUrl = sr.GetString(4);
                                    title = sr.GetString(5);

                                    if (sr.IsDBNull(6))
                                    {
                                        name = ".";
                                    }
                                    else
                                    {
                                        name = fullUrl.Substring(fullUrl.LastIndexOf('/') + 1).TrimStart('/');
                                    }
                                }
                                webObj = new AveWebObject
                                {
                                    WebID = webId,
                                    Name = name,
                                    FullUrl = fullUrl,
                                    Title = title
                                };
                                changeWebObjs.Add(webId, webObj);
                            }
                            webObj = changeWebObjs[webId];
                            if (NativeChangeType.Navigation == nativeChangeType)
                            {
                                webObj.NavigationChanged = true;
                            }
                            if (ObjType == ChangeObjectType.Web)
                            {
                                ChangeType preChange = webObj.ChangeType;
                                ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                                webObj.EventTime = sr.GetDateTime(0);

                                if (changeType == ChangeType.Delete && !sr.IsDBNull(8))
                                {
                                    webObj.FullUrl = sr.GetString(8);
                                }
                                if (preChange == ChangeType.Add)
                                {
                                    if (changeType == ChangeType.Delete)
                                    {
                                        webObj.ChangeType = ChangeType.Delete;
                                    }
                                }
                                else //"None or Edit", change to "Edit or Delete".
                                {
                                    webObj.ChangeType = changeType;
                                }
                                //提取web上删除Role与RoleAssignment事件的信息
                                if (nativeChangeType == NativeChangeType.RoleDelete || nativeChangeType == NativeChangeType.AssignmentDelete)
                                {
                                    if (sr.IsDBNull(9) && !sr.IsDBNull(10) && !sr.IsDBNull(11))
                                    {
                                        AveSecurityObject deleteSecurity = new AveSecurityObject();
                                        deleteSecurity.PrincipleId = -1;
                                        deleteSecurity.RoleId = sr.GetInt32(10);
                                        deleteSecurity.RoleName = sr.GetString(11);
                                        deleteSecurity.ObjectType = SecurityType.Role;
                                        deleteSecurity.EventTime = webObj.EventTime;
                                        webObj.DeleteSecurities.Add(deleteSecurity);
                                    }
                                    if (!sr.IsDBNull(9) && sr.IsDBNull(11))
                                    {
                                        AveSecurityObject deleteSecurity = new AveSecurityObject();
                                        deleteSecurity.PrincipleId = sr.GetInt32(9);
                                        deleteSecurity.RoleId = sr.IsDBNull(10) ? -1 : sr.GetInt32(10);
                                        deleteSecurity.ObjectType = SecurityType.Assignment;
                                        deleteSecurity.EventTime = webObj.EventTime;
                                        webObj.DeleteSecurities.Add(deleteSecurity);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebForIB. EventTime:{0}.  ErrorMessage:{1}. SiteId:{2}", sr.GetDateTime(0).ToString(), e.ToString(), siteId.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QueryWebForIB");
            }
            return changeWebObjs;
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QueryWebContentTypeForFB");
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", webId);
            Dictionary<byte[], AveContentTypeObject> contentTypes = new Dictionary<byte[], AveContentTypeObject>();
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.WebContentTypes))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            AveContentTypeObject contentType = new AveContentTypeObject
                            {
                                ContentTypeId = sr.GetValue(0) as byte[],
                                SchemaXml = sr.IsDBNull(1) ? string.Empty : sr.GetString(1),
                                Name = sr.IsDBNull(1) ? string.Empty : sr.GetString(2),
                                Scope = sr.GetString(3)
                            };
                            contentTypes.Add((byte[])sr["ContentTypeId"], contentType);
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from QueryWebContentTypeForFB. ErrorMessage:{0}", e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QueryWebContentTypeForFB");
            }
            return contentTypes;
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QueryWebContentTypeForFB");
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@FullUrl", serverRelativeUrl);
            Dictionary<byte[], AveContentTypeObject> contentTypes = new Dictionary<byte[], AveContentTypeObject>();
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.WebContentTypesFast))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            AveContentTypeObject contentType = new AveContentTypeObject
                            {
                                ContentTypeId = sr.GetValue(0) as byte[],
                                SchemaXml = sr.IsDBNull(1) ? string.Empty : sr.GetString(1),
                                Name = sr.IsDBNull(1) ? string.Empty : sr.GetString(2),
                                Scope = sr.GetString(3)
                            };
                            contentTypes.Add((byte[])sr["ContentTypeId"], contentType);
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from QueryWebContentTypeForFB. ErrorMessage:{0}", e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QueryWebContentTypeForFB");
            }
            return contentTypes;
        }

        public Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime)
        {
            AvePerformanceTimerPool.Start("AveDiscoverQuery.QueryWebContentTypeForIB");
            //给web添加column的时候用ContentTypeId取不到ContentType的信息
            Dictionary<byte[], AveContentTypeObject> contentTypeChanges = new Dictionary<byte[], AveContentTypeObject>();

            mQueryWorker.AddParameter("@endTime", endTime);
            mQueryWorker.AddParameter("@startTime", startTime);
            mQueryWorker.AddParameter("@siteId", siteId);
            mQueryWorker.AddParameter("@webId", webId);
            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(AveDiscoverQueryString.WebContentTypeChanged))
                {
                    while (sr.Read())
                    {
                        try
                        {
                            var nativeChangeType = (NativeChangeType)sr.GetValue(0);
                            ChangeType changeType = DiscoverUtility.GetChangeType(nativeChangeType);
                            var objType = (ChangeObjectType)sr.GetValue(1);
                            var contentTypeId = (byte[])sr.GetValue(3);
                            AveContentTypeObject contentTypeChange = null;

                            if (!IsContainContentTypeId(contentTypeChanges, contentTypeId, out contentTypeChange))
                            {
                                contentTypeChange = new AveContentTypeObject
                                {
                                    ContentTypeId = contentTypeId
                                };
                                if (objType == ChangeObjectType.Field)
                                {
                                    contentTypeChange.IsColumn = true;
                                }
                                contentTypeChanges.Add(contentTypeId, contentTypeChange);
                                if (!sr.IsDBNull(11))
                                {
                                    contentTypeChange.SchemaXml = sr.IsDBNull(7) ? string.Empty : sr.GetString(7);
                                    contentTypeChange.Name = sr.IsDBNull(8) ? string.Empty : sr.GetString(8);
                                }
                            }
                            if (contentTypeChange.ChangeType == ChangeType.Add)
                            {
                                if (changeType == ChangeType.Delete)
                                {
                                    RemoveContentType(contentTypeChanges, contentTypeId);
                                }
                            }
                            else
                            {
                                contentTypeChange.ChangeType = changeType;
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Log(AveLogLevel.WARN, "Error occur while access data from method QueryWebContentTypeForIB. EventTime:{0}.  ErrorMessage:{1}", sr.GetDateTime(4).ToString(), e.ToString());
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            finally
            {
                AvePerformanceTimerPool.Stop("AveDiscoverQuery.QueryWebContentTypeForIB");
            }
            return contentTypeChanges;
        }

        #endregion

        #region Backup/Restore

        public AveSiteSettingInfo GetSiteSettingFromSites(IAveSite site)
        {
            AveSiteSettingInfo info = new AveSiteSettingInfo();

            string cmdText = @"
        SELECT Id,NextUserOrGroupId,OwnerID,SecondaryContactID,Subscribed,TimeCreated,UsersCount,
               BWUsed,DiskUsed,SecondStageDiskUsed,QuotaTemplateID,DiskQuota,UserQuota,DiskWarning,DiskWarned,
               CurrentResourceUsage,AverageResourceUsage,ResourceUsageWarning,ResourceUsageMaximum,BitFlags,
               SecurityVersion,CertificationDate,DeadWebNotifyCount,PortalURL,PortalName,LastContentChange,
               LastSecurityChange,AuditFlags,InheritAuditFlags,UserInfoListId,UserIsActiveFieldRowOrdinal,
               UserIsActiveFieldColumnName,UserAccountDirectoryPath,RootWebId,HashKey,DomainGroupMapVersion,
               DomainGroupMapCacheVersion,DomainGroupMapCache,HostHeader,SubscriptionId
        FROM Sites With(nolock) WHERE Id=@SiteId";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", site.ID);
            AveQueryUtility.GetDBRow(info, mQueryWorker, cmdText);

            return info;
        }

        public long GetSiteSizeFromSites(IAveSite site)
        {
            long siteSize = 0;
            string cmdText = @"SELECT DiskUsed FROM Sites WITH(NOLOCK) WHERE Id=@SiteId";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", site.ID);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        siteSize = dr.GetInt64(0);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return siteSize;
        }

        /// <summary>
        /// 修改webs中author字段。
        /// 无API实现
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timeCreated"></param>
        /// <param name="timeLastModified"></param>
        /// <param name="version"></param>
        public void UpdateWebsAuthorByNative(int userId, Guid webId)
        {
            string updateWebs = "update Webs set Author =@UserID where Id=@WebID";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@UserID", userId);
            mQueryWorker.AddParameter("@WebID", webId);
            mQueryWorker.ExecuteNonQuery(updateWebs);
        }

        public AveSiteSettingInfo GetFullSiteSetting(IAveSite site)
        {
            AveSiteSettingInfo siteSettingInfo = new AveSiteSettingInfo();
            string cmdText = string.Empty;
            try
            {
                cmdText = @"SELECT SolutionId FROM Solutions With(nolock) WHERE SiteId = @SiteId";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", site.ID);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (siteSettingInfo.SolutionIdCollection == null)
                    {
                        siteSettingInfo.SolutionIdCollection = new List<Guid>();
                    }
                    while (dr.Read())
                    {
                        siteSettingInfo.SolutionIdCollection.Value.Add(dr.GetGuid(0));
                    }
                }
            }
            catch (SqlException queryException)
            {
                mLog.Log(AveLogLevel.ERROR, new AveQueryException(string.Format("Exception Error Code----{0}", queryException.Number), queryException).ToString());
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.ERROR, e.ToString());
            }

            cmdText = @"
        SELECT Id,NextUserOrGroupId,OwnerID,SecondaryContactID,Subscribed,
               TimeCreated,UsersCount,BWUsed,DiskUsed,SecondStageDiskUsed,
               QuotaTemplateID,DiskQuota,UserQuota,DiskWarning,DiskWarned,
               CurrentResourceUsage,AverageResourceUsage,ResourceUsageWarning,ResourceUsageMaximum,BitFlags,
               SecurityVersion,CertificationDate,DeadWebNotifyCount,PortalURL,PortalName,
               LastContentChange,LastSecurityChange,AuditFlags,InheritAuditFlags,UserInfoListId,
               UserIsActiveFieldRowOrdinal,UserIsActiveFieldColumnName,UserAccountDirectoryPath,RootWebId,HashKey,
               DomainGroupMapVersion,DomainGroupMapCacheVersion,DomainGroupMapCache,HostHeader,SubscriptionId
        FROM Sites With(nolock) WHERE Id=@SiteId";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        int BitFlags = dr.GetInt32(19);
                        siteSettingInfo.SyndicationEnabled = Ave2010SiteFlags.SyndicationEnabled(BitFlags);
                        if (!dr.IsDBNull(27))
                        {
                            siteSettingInfo.AuditFlags = dr.GetInt32(27);
                        }
                        else
                        {
                            siteSettingInfo.AuditFlags = null;
                        }
                        siteSettingInfo.UseAuditFlagCache = site.Audit.UseAuditFlagCache;
                        siteSettingInfo.TrimAuditLog = Ave2010SiteFlags.TrimAuditLog(BitFlags);
                        AveQueryUtility.GetDBRow(siteSettingInfo, dr, AveQueryUtility.GetFieldMap(typeof(AveSiteSettingInfo), string.Empty), 0);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            string cmdString = @"SELECT MetaInfo FROM Webs With(nolock) WHERE Id = @Id";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@Id", site.RootWeb.ID);
            byte[] buffer = (byte[])mQueryWorker.ExecuteScalar(cmdString);

            string metaInfo = AveCompressedUtility.GetTCompressedString(buffer);
            Dictionary<string, string> ProInMetaInfo = AveCompressedUtility.GetMetaInfoDictionary(metaInfo);
            if (ProInMetaInfo.ContainsKey("_auditlogtrimmingretention"))
            {
                siteSettingInfo.AuditLogTrimmingRetention = Int32.Parse(ProInMetaInfo["_auditlogtrimmingretention:SW"]);
            }
            else
            {
                siteSettingInfo.AuditLogTrimmingRetention = 0;
            }
            if (ProInMetaInfo.ContainsKey("_auditlogtrimmingcallout"))
            {
                siteSettingInfo.AuditLogTrimmingCallout = ProInMetaInfo["_auditlogtrimmingcallout:SW"];
            }
            else
            {
                siteSettingInfo.AuditLogTrimmingCallout = "";
            }
            if (ProInMetaInfo.ContainsKey("allowdesigner"))
            {
                siteSettingInfo.AllowDesigner = Int32.Parse(ProInMetaInfo["allowdesigner:SW"]) == 0 ? false : true;
            }
            else
            {
                siteSettingInfo.AllowDesigner = true;
            }
            if (ProInMetaInfo.ContainsKey("allowmasterpageediting"))
            {
                siteSettingInfo.AllowMasterPageEditing = Int32.Parse(ProInMetaInfo["allowmasterpageediting:SW"]) == 0 ? false : true;
            }
            else
            {
                siteSettingInfo.AllowMasterPageEditing = false;
            }
            if (ProInMetaInfo.ContainsKey("allowrevertfromtemplate"))
            {
                siteSettingInfo.AllowRevertFromTemplate = Int32.Parse(ProInMetaInfo["allowrevertfromtemplate:SW"]) == 0 ? false : true;
            }
            else
            {
                siteSettingInfo.AllowRevertFromTemplate = false;
            }

            return siteSettingInfo;
        }

        public int[] GetCollectionIdAndProviderId(Guid siteId)
        {
            int[] temId = new int[2];

            #region get collection id
            bool collectionIdInSites = false;
            string commandText = "SELECT * FROM syscolumns With(nolock) WHERE  ID=OBJECT_ID('Sites') AND Name  ='RbsCollectionId'";

            try
            {
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandText))
                {
                    if (sr.Read())
                    {
                        collectionIdInSites = true;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            if (collectionIdInSites)
            {
                try
                {
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    commandText = "SELECT RbsCollectionId FROM Sites With(nolock) WHERE Id=@SiteId";
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandText))
                    {
                        if (sr.Read())
                        {
                            temId[0] = sr.GetInt32(0);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (AveQueryException )
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
            else
            {
                commandText = @"
SELECT collection_id
FROM [mssqlrbs_resources].[rbs_internal_collections] With(nolock)
WHERE owning_application='Microsoft.SharePoint'
";
                try
                {
                    using (SqlDataReader sr = mQueryWorker.ExecuteReader(commandText))
                    {
                        if (sr.Read())
                        {
                            temId[0] = sr.GetInt32(0);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (AveQueryException )
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
            #endregion

            commandText = @"SELECT blob_store_id FROM [mssqlrbs_resources].[rbs_internal_blob_stores] With(nolock) WHERE blob_store_name=@ProviderName";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@ProviderName", AveRBSCommon.RBS_PROVIDER_NAME_SP2013);
                using (SqlDataReader sdr = mQueryWorker.ExecuteReader(commandText))
                {
                    if (sdr.Read())
                    {
                        temId[1] = sdr.GetInt16(0);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return temId;
        }

        public string GetPathByNative(Dictionary<string, object> parameters, string type)
        {
            string path = string.Empty;
            string cmdText = string.Empty;
            mQueryWorker.ClearParameters();
            foreach (string key in parameters.Keys)
            {
                mQueryWorker.AddParameter(key, parameters[key]);
            }
            if (type.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                cmdText = "Select FullUrl From Webs With(noLock)Where Id=@Id";
            }
            if (type.Equals("page", StringComparison.OrdinalIgnoreCase))
            {
                cmdText = "Select DirName+'/'+LeafName From AllDocs With(noLock)Where SiteId =@SiteId and DeleteTransactionId=0x and Id=@Id";
            }
            path = (string)mQueryWorker.ExecuteScalar(cmdText);

            return path;
        }

        public AveWebSettingInfo GetWebSettingFromWebs(IAveWeb web)
        {
            AveWebSettingInfo info = new AveWebSettingInfo();

            string cmdText = @"
        SELECT Author, Title, TimeCreated, Description, SecurityProvider, MetaInfo, MetaInfoVersion, LastMetadataChange, NavStructNextEid, 
               NextWebGroupId, DefTheme, AlternateCSSUrl, CustomizedCss, CustomJSUrl, AlternateHeaderUrl, DailyUsageData, DailyUsageDataVersion, 
               MonthlyUsageData, MonthlyUsageDataVersion, DayLastAccessed, Language, Locale, TimeZone, Time24, CalendarType, AdjustHijriDays, 
               ProvisionConfig, Flags,MasterUrl,CustomMasterUrl, Collation, RequestAccessEmail, SiteLogoUrl, SiteLogoDescription, AuditFlags, 
               InheritAuditFlags, Ancestry, AltCalendarType, CalendarViewOptions, WorkDayStartHour, WorkDayEndHour,WorkDays 
        FROM Webs With(nolock) WHERE Id=@WebId";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@WebId", web.ID);
            AveQueryUtility.GetDBRow(info, mQueryWorker, cmdText);

            return info;
        }

        public void SetWebPartLists(AveWebPartBaseInfo webPartInfo)
        {
            string cmdText =
                @"SELECT wp.tp_WebId,wp.tp_UserID,wp.tp_Level, w.FullUrl AS tp_FullUrl
                        FROM WebPartLists wp With(nolock) LEFT JOIN Webs w With(nolock) ON wp.tp_WebId=w.Id  WHERE tp_SiteId=@SiteId and tp_PageUrlID=@Id AND tp_Level=@Level AND tp_WebPartID=@WebPartID
                        ";
            mQueryWorker.AddParameter("@WebPartID", webPartInfo.ID);
            webPartInfo.WebPartList = AveQueryUtility.GetDBRows<AveWebPartListInfo>(mQueryWorker, cmdText, "tp_");
        }

        public Dictionary<Guid, string> GetALLWebTemplates(IAveSite site, uint lcid)
        {
            IAveWebTemplateCollection webTemplates = site.GetWebTemplates(lcid);
            if (webTemplates == null)
            {
                return null;
            }
            Dictionary<Guid, string> allWebTemplates = new Dictionary<Guid, string>();
            Guid webId = new Guid();
            int templateId = 0;
            short provisionConfig = 0;

            string cmdText = @"SELECT Id,WebTemplate,ProvisionConfig FROM Webs With(nolock) WHERE SiteId=@SiteId";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", site.ID);
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        webId = dr.GetGuid(0);
                        templateId = dr.GetInt32(1);
                        provisionConfig = dr.GetInt16(2);
                        string webTemplate = WebTemplateIdName(templateId, provisionConfig.ToString(), webTemplates);
                        allWebTemplates.Add(webId, webTemplate);
                    }
                }
            }
            catch (SqlException ex)
            {
                mLog.Log(AveLogLevel.ERROR, "Error occur while execute GetALLWebTemplates. ErrorMessage:{0}.", new AveQueryException(string.Format("Exception Error Code----{0}", ex.Number), ex).ToString());
            }
            catch (Exception e)
            {
                mLog.Log(AveLogLevel.ERROR, "Error occur while execute GetALLWebTemplates. ErrorMessage:{0}.", e.ToString());
            }
            return allWebTemplates;
        }

        public int GetSubWebCounts(Guid siteId, string serverRelativeUrl)
        {
            int count = 0;
            string cmdText = @"select FullUrl from Webs With(nolock) where SiteId=@SiteId";
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                using (SqlDataReader sr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (sr.Read())
                    {
                        string fullUrl = sr.GetString(0);

                        if (fullUrl.StartsWith(serverRelativeUrl, StringComparison.OrdinalIgnoreCase)
                            && !fullUrl.Equals(serverRelativeUrl, StringComparison.OrdinalIgnoreCase)
                            && (string.IsNullOrEmpty(serverRelativeUrl) || fullUrl.Substring(serverRelativeUrl.Length, 1).Equals("/", StringComparison.OrdinalIgnoreCase)))//考虑RootSiteCollection的情形
                        {
                            count++;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }

            return count;
        }

        public List<Guid> GetAllWebsGuidByNative(Guid siteId)
        {
            List<Guid> webIds = new List<Guid>();
            try
            {
                string cmdText = "SELECT Id FROM Webs With(nolock) WHERE SiteId=@SiteId";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (reader.Read())
                    {
                        Guid tmpWebGuid = reader.GetGuid(0);
                        if (!webIds.Contains(tmpWebGuid))
                        {
                            webIds.Add(tmpWebGuid);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return webIds;
        }

        public string GetWebPartsInGallery(Guid siteId)
        {
            string sCommandString = "SELECT L.tp_ID AS GalleryListId FROM Lists L With(nolock) JOIN Webs W With(nolock) ON L.tp_WebId = W.Id WHERE W.SiteId = @SiteID AND L.tp_Title = 'Web Part Gallery'";
            DataTable dataTable = new DataTable("Web part gallery list");
            try
            {
                SqlConnection connection = mQueryWorker.Connection;
                {
                    SqlCommand sqlCommand = new SqlCommand(sCommandString, connection);
                    sqlCommand.Parameters.Add(new SqlParameter("SiteID", siteId));
                    new SqlDataAdapter(sqlCommand).Fill(dataTable);
                }
                if (dataTable.Rows.Count <= 0)
                {
                    return null;
                }
                string str2 = dataTable.Rows[0]["GalleryListId"].ToString();
                DataTable table2 = new DataTable("Web part types");
                mQueryWorker.ClearParameters();

                string str3 = "SELECT nvarchar9 as WebPartName, nvarchar8 as Assembly, nvarchar7 as Title, nvarchar10 as Image, ntext2 as Description " + ", nvarchar3 as FileType, nvarchar11 as Category " + " FROM userData With(nolock) WHERE tp_ListId = @ListID";
                SqlCommand selectCommand = new SqlCommand(str3, mQueryWorker.Connection);
                selectCommand.Parameters.Add(new SqlParameter("ListID", str2));
                new SqlDataAdapter(selectCommand).Fill(table2);
                if (table2.Rows.Count <= 0)
                {
                    return null;
                }
                StringWriter w = new StringWriter(new StringBuilder());
                XmlWriter writer2 = new XmlTextWriter(w);
                writer2.WriteStartElement("WebPartGallery");
                foreach (DataRow row in table2.Rows)
                {
                    writer2.WriteStartElement("WebPart");
                    writer2.WriteAttributeString("Name", row["WebPartName"].ToString());
                    writer2.WriteAttributeString("Assembly", row["Assembly"].ToString());
                    writer2.WriteAttributeString("Title", row["Title"].ToString());
                    writer2.WriteAttributeString("Description", row["Description"].ToString());
                    writer2.WriteAttributeString("Image", row["Image"].ToString());
                    writer2.WriteAttributeString("FileType", row["FileType"].ToString());
                    writer2.WriteAttributeString("Category", row["Category"].ToString());
                    writer2.WriteEndElement();
                }
                writer2.WriteEndElement();
                writer2.Flush();
                return w.ToString();
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        public Dictionary<Guid, Guid> ReloadHiddenWebProperty(Guid siteId, AveWebSettingInfo webSettingInfo, List<Dictionary<string, string>> siteManagedMappings, AveSiteInfo sourceSiteInfo, string destSiteUrl, Dictionary<Guid, Guid> webIdMapping)
        {
            Dictionary<Guid, Guid> hiddenWebMapping = new Dictionary<Guid, Guid>();
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                string pamater = string.Empty;
                Dictionary<Guid, string> hiddenWeb = new Dictionary<Guid, string>();
                hiddenWeb = webSettingInfo.NavigationWebAndPage.Value["Hidden"]["web"];
                foreach (Guid Id in hiddenWeb.Keys)
                {
                    if (!webIdMapping.ContainsKey(Id))
                    {
                        string cmdText = "Select Id From Webs With(noLock)Where SiteId=@SiteId And ";
                        string webUrl = hiddenWeb[Id];
                        webUrl = AveReplaceProcessor.UrlReplace(hiddenWeb[Id], siteManagedMappings, new ReplaceOption(true), sourceSiteInfo, destSiteUrl);
                        if (webUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            webUrl = webUrl.Substring(1);
                        }
                        pamater = "FullUrl='" + webUrl + "'";
                        using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText + pamater))
                        {
                            if (dr.Read())
                            {
                                Guid WebId = dr.GetGuid(0);
                                if (!hiddenWebMapping.ContainsKey(Id))
                                {
                                    hiddenWebMapping[Id] = WebId;
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return hiddenWebMapping;
        }

        public bool IsConflictWithRecycle(Guid siteId, string webUrl)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@FullUrl", webUrl);

            const string cmdText = @"SELECT Id FROM Webs With(nolock) WHERE SiteId =@SiteId AND FullUrl=@FullUrl";
            bool isConflict = false;
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    if (dr.HasRows)
                    {
                        isConflict = true;
                    }
                }
            }
            catch(Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.ConflictWithRecycleError, ex);
                isConflict = false;
            }
            return isConflict;
        }

        public Guid GetWebId(Guid siteId, string url)
        {
            Guid id = Guid.Empty;
            try
            {
                string text = "SELECT Id FROM Webs With(nolock) WHERE FullUrl=@Url";
                mQueryWorker.AddParameter("@Url", url.Trim('/'));
                if (!siteId.Equals(Guid.Empty))
                {
                    text = text + " and SiteId=@SiteId";
                    mQueryWorker.AddParameter("@SiteId", siteId);
                }
                using (SqlDataReader reader = mQueryWorker.ExecuteReader(text))
                {
                    if (reader.Read())
                    {
                        id = reader.GetGuid(0);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new AveQueryException(ex);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return id;
        }

        public void GetSubWebsAndPageInfo(Dictionary<string, Object> parameters, Dictionary<string, Dictionary<Guid, string>> websAndPages, string type)
        {
            mQueryWorker.ClearParameters();
            foreach (string key in parameters.Keys)
            {
                mQueryWorker.AddParameter(key, parameters[key]);
            }
            if (!websAndPages.ContainsKey(type))
            {
                websAndPages.Add(type, new Dictionary<Guid, string>());
            }
            try
            {
                if (type.Equals("web", StringComparison.OrdinalIgnoreCase))
                {
                    string cmdText = "select FullUrl,Id from webs With(noLock) where SiteId=@SiteId And ParentWebId=@ParentWebId";
                    using (SqlDataReader rd = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (rd.Read())
                        {
                            string fullUrl = rd["FullUrl"].ToString();
                            Guid webId = new Guid(rd["Id"].ToString());
                            if (!websAndPages[type].ContainsKey(webId))
                            {
                                websAndPages[type].Add(webId, fullUrl);
                            }
                        }
                    }
                }
                if (type.Equals("page", StringComparison.OrdinalIgnoreCase))
                {
                    string cmdText = "select DirName,LeafName,Id from AllDocs With(noLock) where SiteId=@SiteId and DeleteTransactionId=0x and ListId=@ListId and LeafName like '%.aspx' and DoclibRowId is not null";
                    using (SqlDataReader rd = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (rd.Read())
                        {
                            string dirName = rd["DirName"].ToString();
                            string leafName = rd["LeafName"].ToString();
                            string fullUrl = dirName + "/" + leafName;
                            Guid pageItemId = new Guid(rd["Id"].ToString());
                            if (!websAndPages[type].ContainsKey(pageItemId))
                            {
                                websAndPages[type].Add(pageItemId, fullUrl);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetSubWebsAndPageInfoError, ex);
            }
        }

        public long GetWebSize(IAveWeb web)
        {
            long webSize = 0;
            string cmdText =
@"SELECT SUM(PartSize), SiteSize.WebId
        FROM
        (
        	SELECT PartSize, WebId FROM
        	(
        		SELECT ISNULL
        		(
        			SUM
        			(
        				CAST
        					(ISNULL(Size, 0) AS BIGINT) +
        				CAST
        					(ISNULL(MetaInfoSize, 0) AS BIGINT) +
        				CAST
        					(FileFormatMetaInfoSize AS BIGINT) +
        				CAST
        					(ISNULL(UnVersionedMetaInfoSize,0) AS BIGINT) +
        				CAST
        					(152 AS BIGINT)
        			), 0
        		) AS PartSize, WebId
        		FROM
        		(                
        			SELECT
        				WebId, Size, MetaInfoSize, FileFormatMetaInfoSize, UnVersionedMetaInfoSize
        			FROM
        				AllDocs WITH (NOLOCK, INDEX=AllDocs_ParentId)
        			WHERE
        				SiteId = @SiteId AND
        				DeleteTransactionId = 0x
        		) AS AD GROUP BY WebId
        	) Docs_NoLock_Site
        	UNION ALL
        	SELECT PartSize, WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(AFF.BlobSize, 0)) AS BIGINT))),0)) AS PartSize, WebId
        		FROM
        		(
        			SELECT
        				*
        			FROM
        				AllDocs WITH (NOLOCK, INDEX=AllDocs_ParentId)
        			WHERE
        				SiteId = @SiteId AND
        				DeleteTransactionId = 0x
        		) AS AD 
        		CROSS APPLY
        		(
        			SELECT
        				*
        			FROM
        				AllFileFragments WITH (NOLOCK, INDEX=AllFileFragments_PartId_UCI)
        			WHERE
        					DocId = AD.Id
        		) AS AFF GROUP BY WebId
        	)AllFileFragments_NoLock_DocId
        	UNION ALL
        	SELECT WL.PartSize, WL.WebId FROM 
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(DATALENGTH(L.tp_ContentTypes), 0) + ISNULL(DATALENGTH(L.tp_Fields), 0)) AS BIGINT))),0)) AS PartSize, L.tp_WebId AS WebId
        		FROM
        		(
        			SELECT
        				*
        			FROM
        				Webs WITH (NOLOCK, INDEX=Webs_SiteIdParent)
        			WHERE
        				SiteId = @SiteId
        		) AS W
        		CROSS APPLY
        		(
        			SELECT
        				*
        			FROM
        				AllLists WITH (NOLOCK, INDEX=AllLists_PK)
        			WHERE
        				tp_WebId = W.Id AND
        				tp_DeleteTransactionId = 0x
        		) AS L GROUP BY tp_WebId
        	)WL
        	UNION ALL
        	SELECT UD_AL.PartSize, UD_AL.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(tp_Size, 0)) AS BIGINT))),0)) AS PartSize, tp_WebId AS WebId
        		FROM
        		(
        			SELECT
        				AUD.tp_Size, AL.tp_WebId
        			FROM
        				AllUserData AUD WITH (NOLOCK, INDEX=AllUserData_ParentId), AllLists AL WITH(NOLOCK)
        			WHERE
        				AUD.tp_SiteId = @SiteId AND
        				AUD.tp_DeleteTransactionId = 0x AND
        				(AUD.tp_IsCurrentVersion = CONVERT(BIT, 0) OR AUD.tp_IsCurrentVersion = CONVERT(BIT, 1)) AND
        				AUD.tp_ListId = AL.tp_ID
        		) AS UD GROUP BY tp_WebId
        	)UD_AL
        	UNION ALL
            SELECT AWP_AL.PartSize, AWP_AL.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(AWP.tp_Size, 0)) AS BIGINT))),0)) AS PartSize, AWP.WebId
        		FROM
        		(
        			SELECT
        				AW_T.tp_Deleted, AW_T.WebId, AW_T.tp_Size, AW_T.tp_ListId, AW_T.tp_SiteId
        			FROM
        			(
        				SELECT AD.WebId, tp_Deleted, tp_Size, tp_ListId, tp_SiteId 
        				FROM
        					AllWebParts AW WITH (INDEX=PageUrlID_FK), AllDocs AD WITH(NOLOCK)
        				WHERE
        					AW.tp_PageUrlID = AD.Id AND
        					AW.tp_SiteId = @SiteId AND 
        					AW.tp_Deleted = CONVERT(BIT, 0)
        			) AW_T
        		) AS AWP
        		WHERE AWP.tp_Deleted = CONVERT(BIT, 0) GROUP BY AWP.WebId		
        	)AWP_AL
        	UNION ALL
        	SELECT P_AL.PartSize, P_AL.WebId FROM 
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(P.tp_Size, 0)) AS BIGINT))),0)) AS PartSize, tp_WebId AS WebId
        		FROM
        		(
        			SELECT
        				PL.tp_Size, PL.tp_Deleted, AW.tp_ListId, AL.tp_WebId
        			FROM
        				Personalization AS PL WITH (NOLOCK, INDEX=Personalization_PK), AllWebParts AS AW WITH (INDEX=PageUrlID_FK), AllLists AL WITH(NOLOCK)
        			WHERE
        				PL.tp_SiteId = @SiteId AND
        				PL.tp_WebPartID = Aw.tp_ID AND
        				AW.tp_ListId = AL.tp_ID
        		) AS P
        		WHERE P.tp_Deleted = CONVERT(BIT,0) GROUP BY tp_WebId
        	)P_AL
        	UNION ALL
        	SELECT Cmd.PartSize, Cmd.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(CM.Size, 0)) AS BIGINT))),0)) AS PartSize, AD.WebId AS WebId
        		FROM ComMd CM, AllDocs AD
        		WHERE CM.SiteId = @SiteId and DocId=AD.Id GROUP BY WebId
        	)Cmd
        	UNION ALL
        	SELECT CT_Data.PartSize, CT_Data.WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(CT_T.Size, 0)) AS BIGINT))),0)) AS PartSize, CT_T.Id AS WebId
        		FROM
        		(
        			SELECT
        				CT.Size, Web.Id
        			FROM
        				ContentTypes CT WITH (NOLOCK, INDEX=ContentTypes_SiteClassCTId), Webs Web WITH(NOLOCK) 
        			WHERE
        				CT.SiteId = @SiteId and (Web.FullUrl=CT.Scope or CT.Scope='') and Web.SiteId = @SiteId
        		) AS CT_T GROUP BY CT_T.Id		
        	) CT_Data
        	UNION ALL
        	SELECT R_Data.PartSize, R_Data.WebId FROM
        	(
        		SELECT ISNULL(SUM(R.Size),0) AS PartSize, WebId
        		FROM
        		(
        			SELECT
        				*
        			FROM
        				RecycleBin WITH (NOLOCK, INDEX=RecycleBin_SiteBinWebUser)
        			WHERE
        				SiteId = @SiteId AND
        				BinId = 1
        		) AS R GROUP BY WebId
        	) R_Data
        	UNION ALL
        	SELECT PartSize, WebId FROM
        	(
        		SELECT (ISNULL((SUM(CAST((ISNULL(Size, 0) + ISNULL(MetaInfoSize, 0)) AS BIGINT))),0)) AS PartSize, WebId
        		FROM
        		(
        			SELECT
        				ADV.Size, ADV.MetaInfoSize, AD.WebId
        			FROM
        				AllDocVersions ADV WITH (NOLOCK, INDEX=AllDocVersions_PK), AllDocs AD
        			WHERE
        				ADV.SiteId = @SiteId AND
        				ADV.DeleteTransactionId = 0x AND
        				ADV.Id = AD.Id
        		) AS ADV_AD
        		GROUP BY WebId
        	)ADV_AD_Data
        ) SiteSize
        GROUP BY SiteSize.WebId";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", web.Site.ID);
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(1) && dr.GetGuid(1) == web.ID)
                        {
                            webSize = dr.GetInt64(0);
                        }
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return webSize;
        }

        public string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
            string cmdText = @"select ResourceDir,Definition from ContentTypes With(nolock)
                                       where SiteId=@SiteId and Class=1 and ContentTypeId=@ContentTypeId";
            string name = null;
            string definition = string.Empty;

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ContentTypeId", contentTypeId);

            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        name = dr["ResourceDir"] as string;
                        try
                        {
                            if (!dr.IsDBNull(1))
                            {
                                definition = dr["Definition"] as string;
                                XmlDocument xDoc = new XmlDocument();
                                xDoc.InnerXml = definition;
                                XmlElement root = (XmlElement)xDoc.ChildNodes[0];
                                if (root.HasAttribute("Name"))
                                {
                                    name = root.Attributes["Name"].Value;//使用xml中的Name作为ContentType的真实名字
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("Get ContentType realName error, Exception:{0}", e.ToString());
                        }
                        break;
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException )
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return name;
        }

        public void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList)
        {
            AveContentTypeInfo rootCTInfo = contentTypeInfo;
            try
            {
                for (int i = 0; i < parentIdList.Count; i++)
                {
                    AveContentTypeInfo ctInfo = null;
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@ContentTypeId", parentIdList[i]);
                    string cmdText = @"select ContentTypeId,Scope,Version,Definition,ResourceDir,SolutionId,IsFromFeature from ContentTypes With(nolock)
                                       where SiteId=@SiteId and Class=1 and ContentTypeId=@ContentTypeId";

                    string parentName = null;
                    using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            try
                            {
                                if (dr.IsDBNull(4))
                                {
                                    continue;
                                }
                                parentName = dr["ResourceDir"] as string;
                                if (dr.IsDBNull(3))
                                {
                                    continue;
                                }
                                ctInfo = new AveContentTypeInfo();
                                ctInfo.Name = dr["ResourceDir"] as string;
                                if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Name = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                ctInfo.Scope = dr["Scope"] as string;
                                string definition = dr["Definition"] as string;
                                XmlDocument xDoc = new XmlDocument();
                                xDoc.InnerXml = definition;
                                XmlElement root = (XmlElement)xDoc.ChildNodes[0];

                                if (root.HasAttribute("Name"))
                                {
                                    ctInfo.Name = root.Attributes["Name"].Value;//使用xml中的Name作为ContentType的真实名字
                                }
                                if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Name = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                ctInfo.Id = root.Attributes["ID"].Value;
                                ctInfo.ReadOnly = root.HasAttribute("ReadOnly") && root.Attributes["ReadOnly"].Value == "TRUE";
                                ctInfo.Description = root.HasAttribute("Description") ? root.Attributes["Description"].Value : "";
                                if (ctInfo.Description.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Description = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Description, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                string fieldRefs = root["FieldRefs"] != null ? root["FieldRefs"].InnerXml : "";
                                fieldRefs = "<Fields>" + fieldRefs + "</Fields>";
                                ctInfo.FieldsSchemaXml = fieldRefs;
                                ctInfo.ResourceFolder = root["Folder "] != null ? root["Folder "].Attributes["TargetName"].Value : "";
                                ctInfo.DocumentTemplate = root["DocumentTemplate"] != null ? root["DocumentTemplate"].Attributes["TargetName"].Value : "";
                                if ((ctInfo.ResourceFolder.Length > 0) && ctInfo.DocumentTemplate.StartsWith(ctInfo.ResourceFolder))
                                {
                                    int startIndex = ctInfo.DocumentTemplate.LastIndexOf('/') + 1;
                                    ctInfo.DocumentTemplate = ctInfo.DocumentTemplate.Substring(startIndex, ctInfo.DocumentTemplate.Length - startIndex);
                                }
                                ctInfo.Group = root.Attributes["Group"].Value;
                                if (ctInfo.Group.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Group = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Group, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                ctInfo.Hidden = root.HasAttribute("Hidden") && root.Attributes["Hidden"].Value == "TRUE";
                                break;
                            }
                            catch (Exception ex)
                            {
                                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetContentTypeError, ex);
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(parentName))
                    {
                        rootCTInfo.ParentName = parentName;
                    }

                    if (ctInfo != null)
                    {
                        rootCTInfo.ParentContentTypeInfo = ctInfo;
                        rootCTInfo.ParentName = ctInfo.Name;
                        rootCTInfo = ctInfo;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetContentTypeError, ex);
            }
        }

        public AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope)
        {
            AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            if (scope.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                scope = scope.Substring(1);
            }
            try
            {
                mQueryWorker.AddParameter("@Scope", scope);
                string cmdText = @"select ContentTypeId,Scope,Version,Definition,ResourceDir,SolutionId,IsFromFeature from ContentTypes With(nolock)
                                       where SiteId=@SiteId and Class=1 and Scope=@Scope";

                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        try
                        {
                            if (dr.IsDBNull(3))
                            {
                                continue;
                            }
                            AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                            ctInfo.Name = dr["ResourceDir"] as string;
                            ctInfo.Scope = dr["Scope"] as string;
                            string definition = dr["Definition"] as string;
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.InnerXml = definition;
                            XmlElement root = (XmlElement)xDoc.ChildNodes[0];

                            if (root.HasAttribute("Name"))
                            {
                                ctInfo.Name = root.Attributes["Name"].Value;
                            }
                            if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                            {
                                ctInfo.Name = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                            }
                            ctInfo.Id = root.Attributes["ID"].Value;
                            ctInfo.ReadOnly = root.HasAttribute("ReadOnly") && root.Attributes["ReadOnly"].Value == "TRUE";
                            ctInfo.Description = root.HasAttribute("Description") ? root.Attributes["Description"].Value : "";
                            if (ctInfo.Description.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                            {
                                ctInfo.Description = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Description, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                            }
                            string fieldRefs = root["FieldRefs"] != null ? root["FieldRefs"].InnerXml : "";
                            fieldRefs = "<Fields>" + fieldRefs + "</Fields>";
                            ctInfo.FieldsSchemaXml = fieldRefs;
                            ctInfo.ResourceFolder = root["Folder "] != null ? root["Folder "].Attributes["TargetName"].Value : "";
                            ctInfo.DocumentTemplate = root["DocumentTemplate"] != null ? root["DocumentTemplate"].Attributes["TargetName"].Value : "";
                            if ((ctInfo.ResourceFolder.Length > 0) && ctInfo.DocumentTemplate.StartsWith(ctInfo.ResourceFolder))
                            {
                                int startIndex = ctInfo.DocumentTemplate.LastIndexOf('/') + 1;
                                ctInfo.DocumentTemplate = ctInfo.DocumentTemplate.Substring(startIndex, ctInfo.DocumentTemplate.Length - startIndex);
                            }
                            ctInfo.Group = root.Attributes["Group"].Value;
                            if (ctInfo.Group.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                            {
                                ctInfo.Group = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(ctInfo.Group, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                            }
                            ctInfo.Hidden = root.HasAttribute("Hidden") && root.Attributes["Hidden"].Value == "TRUE";
                            ctInfo.ResourceFolder = root["Folder"] != null ? root["Folder"].Attributes["TargetName"].Value : null;
                            if (root.HasAttribute("RequireClientRenderingOnNew") && "false".Equals(root.GetAttribute("RequireClientRenderingOnNew"), StringComparison.OrdinalIgnoreCase))
                            {
                                ctInfo.RequireClientRenderingOnNew = false;
                            }
                            else
                            {
                                ctInfo.RequireClientRenderingOnNew = true;
                            }

                            if (root.HasAttribute("NewDocumentControl"))
                            {
                                ctInfo.NewDocumentControl = root.GetAttribute("NewDocumentControl");
                            }
                            if (root["XmlDocuments"] != null)
                            {
                                foreach (XmlNode node in root["XmlDocuments"].ChildNodes)
                                {
                                    string temp = AveCompressedUtility.GetStringFromBase64String(node.InnerText);
                                    ctInfo.XmlDocuments.Add(temp);
                                }
                            }

                            infos.ContentTypes.Add(ctInfo);
                        }
                        catch (Exception ex)
                        {
                            mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetContentTypeError, ex);
                            //mLog.Log(AveLogSeverity.Warn, "WP10BKAveCTCO126", e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetContentTypeError, ex);
            }
            return infos;
        }

        public List<string> GetFields(Guid siteId, string scope)
        {
            List<string> fields = new List<string>();
            try
            {
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                if (scope.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    scope = scope.Substring(1);
                }
                mQueryWorker.AddParameter("@Scope", scope);
                string cmdText = @"select Definition from ContentTypes With(nolock) where 
                                        SiteId=@SiteId and Class=0 and Scope=@Scope and Definition is not null";

                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        if (dr.IsDBNull(0))
                        {
                            continue;
                        }
                        string definition = dr["Definition"] as string;
                        fields.Add(definition);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetFieldsError, ex);
                //mLog.Log(AveLogSeverity.Warn, "WP10BKAveSPFC614", siteId, scope, e);
            }
            return fields;
        }

        public bool GetFieldInSiteChildren(string scope, Guid siteId, Guid fieldId)
        {
            string cmdText = string.Empty;
            try
            {
                cmdText = @"SELECT count(ContentTypeId) FROM contenttypes With(nolock)
                                WHERE siteid=@SiteId And Class=0 AND cast(ContentTypeId as uniqueidentifier)=@FieldId 
                                        AND  Scope like @Scope";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@FieldId", fieldId);
                mQueryWorker.AddParameter("@Scope", scope + "/%");
                if (((int)mQueryWorker.ExecuteScalar(cmdText)) > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.GetFieldsError, ex);
                //SharePoint RTM contentType table does not have deleteTransactionId column
                cmdText = @"SELECT count(ContentTypeId) FROM contenttypes With(nolock)
                                WHERE siteid=@SiteId And Class=0 AND cast(ContentTypeId as uniqueidentifier)=@FieldId 
                                        AND  Scope like @Scope";
                if (((int)mQueryWorker.ExecuteScalar(cmdText)) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetWebCTNameById(Guid siteId, string contentTypeId)
        {
            string ctName = string.Empty;

            string cmdText = "SELECT ResourceDir FROM ContentTypes With(nolock) WHERE SiteId=@SiteId AND Class=1 AND ContentTypeId=" + contentTypeId;
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            ctName = (string)mQueryWorker.ExecuteScalar(cmdText);

            return ctName;
        }

        public bool CheckContentTypeExist(Guid siteId, byte[] ctId)
        {
            try
            {
                string cmdTxt = @"SELECT COUNT(ContentTypeId) FROM ContentTypes With(nolock) WHERE SiteId=@SiteId AND Class=1 AND ContentTypeId=@ContentTypeId";
                mQueryWorker.ClearParameters();
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@ContentTypeId", ctId);

                if (((int)mQueryWorker.ExecuteScalar(cmdTxt)) > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, WrapperQueryServiceResource.CheckContentTypeError, ex);
            }
            return false;
        }

        #endregion
    }
}
