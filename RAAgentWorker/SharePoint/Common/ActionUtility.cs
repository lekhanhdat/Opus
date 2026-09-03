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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace AvePoint.RA.SharePointOnPrem.Common
{
    public class ActionUtility
    {
        //private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected static readonly AveLogger mLog = AveLogger.GetInstance(typeof(ActionUtility));
        private AveObjectModelFactory CurrentModelFactory;
        private static ActionUtility actions = null;
        //private Guid mRecordFeatureId = new Guid("da2e115b-07e4pro-49d9-bb2c-35e93bb9fca9");
        private static object mLock = new object();
        public static ActionUtility GetInstance(AveObjectModelFactory objectModelFactory)
        {
            if (actions == null)
            {
                lock (mLock)
                {
                    if (actions == null)
                    {
                        actions = new ActionUtility(objectModelFactory);
                    }
                }
            }
            return actions;
        }
        private IAveORecords Record
        {
            get
            {
                IAveORecords records = CurrentModelFactory.CreateRecords();
                return records;
            }
        }
        private ActionUtility(AveObjectModelFactory objectModelFactory)
        {
            CurrentModelFactory = objectModelFactory;
        }
        /// <summary>
        /// TO DO I18N later ylgu///
        /// To Do get archived by & Archive time later
        /// remove the un use code.
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="tagContentInfos"></param>
        public void CreateTagContent(IAveListItem item, List<TagContentInfo> tagContentInfos)
        {
            using (var performance = new AgentPerformanceScope("Records.CreateTag", addToStatistics: true))
            {
                try
                {
                    System.Globalization.CultureInfo cultureInfo = item.Web.LanguageCulture;
                    ArrayList allColumn = new ArrayList();
                    foreach (TagContentInfo info in tagContentInfos)
                    {
                        AveFieldType type = new AveFieldType();
                        string columnName = info.ColumnName;
                        object value = info.Value;
                        switch (info.Type)
                        {
                            case TagContentInfoType.Text:
                                type = AveFieldType.Text;
                                break;
                            case TagContentInfoType.Number:
                                type = AveFieldType.Number;
                                break;
                            case TagContentInfoType.DateTime:
                                type = AveFieldType.DateTime;
                                //The TagContentInfoType.DateTime type : must get vaule  by info.DateTime. ADO-83876
                                //Office365添加时间类型column，会把默认上传的时间统一转换成当前SharePoint时区的时间。
                                //Local添加时间类型column，当前上传什么时间，SharePoint显示的就是什么时间。//TO DO ylgu..
                                //if (mConfig.sharePointType != SharePointType.Office365)
                                //{
                                IAveTimeZone webTimeZone = item.Web.RegionalSettings.TimeZone;
                                value = webTimeZone.UTCToLocalTime(info.DateTime);
                                //}
                                //else
                                //{
                                //    value = info.DateTime;
                                //}
                                break;
                            case TagContentInfoType.Boolean:
                                type = AveFieldType.Boolean;
                                if (info.Value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = true;
                                }
                                else if (info.Value.Equals("no", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = false;
                                }
                                else
                                {
                                    throw new Exception("The value of YES/NO column info is invalid");
                                }
                                break;
                            case TagContentInfoType.Archived://TO DO ylgu
                                                             //columnName = "Archived (Yes/No column)";
                                                             //if (cultureInfo == null)
                                                             //{
                                columnName = "Archived";
                                //}
                                //else
                                //{
                                //    columnName = TAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchived", cultureInfo);
                                //}
                                type = AveFieldType.Boolean;
                                value = true;
                                break;
                            case TagContentInfoType.ArchivedBy:
                                //columnName = "Archived By";
                                columnName = "Archived By";
                                //columnName = TAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchivedBy", cultureInfo);
                                type = AveFieldType.User;
                                value = GetArchivedByUser(item.ParentList.ParentWeb.Site, item.ParentList.ParentWeb.CurrentUser.LoginName);
                                break;
                            case TagContentInfoType.ArchivedDate:
                                //columnName = "Archived Time";
                                columnName = "Archived Time";
                                //columnName = TAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchivedTime", cultureInfo);
                                type = AveFieldType.DateTime;
                                //if (mConfig.sharePointType == SharePointType.Office365)
                                //{
                                //    value = GetArchivedTime();
                                //}
                                //else
                                //{
                                //Office365添加时间类型column，会把默认上传的时间统一转换成当前SharePoint时区的时间。
                                //Local添加时间类型column，当前上传什么时间，SharePoint显示的就是什么时间。
                                IAveTimeZone webTimeZone1 = item.Web.RegionalSettings.TimeZone;
                                value = webTimeZone1.UTCToLocalTime(DateTime.UtcNow);
                                //}
                                break;
                            default:
                                throw new Exception("The type of tag content info is invalid");
                        }
                        allColumn.Add(columnName);
                        lock (mLock)
                        {
                            if (!item.Fields.ContainsField(columnName))
                            {
                                try
                                {
                                    item.Fields.Add(columnName, type, false);
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("retry item add column failed,{0}.", e.ToString());
                                    item = item.ParentList.GetItemById(item.ID);
                                    if (!item.Fields.ContainsField(columnName))
                                    {
                                        item.Fields.Add(columnName, type, false);
                                    }
                                }
                            }
                        }
                        item[columnName] = value;
                    }
                    try
                    {
                        //ADO-191952 Local simulate not keep column value.
                        //if (mConfig.sharePointType == SharePointType.Office365)
                        //{
                        //    item.SystemUpdate();
                        //}
                        //else
                        //{
                        //    item.SystemUpdate(false);
                        //}
                        item.SystemUpdateForRecords();
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("retry item systemUpdate failed,{0}.", e.ToString());
                        item.SystemUpdateForRecords();
                        //ADO-191952 Local simulate not keep column value.
                        //if (mConfig.sharePointType == SharePointType.Office365)
                        //{
                        //    item.SystemUpdate();
                        //}
                        //else
                        //{
                        //    item.SystemUpdate(false);
                        //}
                    }
                    foreach (string column in allColumn)
                    {
                        if (item.ParentList.Fields.ContainsField(column))
                        {
                            IAveField field = item.ParentList.Fields[column];
                            if (!item.ParentList.DefaultView.ViewFields.Exists(field.InternalName))
                            {
                                if (column == "Archived")//TAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchived", cultureInfo))
                                {
                                    field.DefaultValue = false.ToString();
                                }
                                item.ParentList.DefaultView.ViewFields.Add(field);
                                item.ParentList.DefaultView.Update();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while creating tag content:{0}.", e.ToString());
                    throw;
                }
            }
        }
        /// <summary>
        /// TO DO Remove locker ?? ylgu
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="itemName"></param>
        /// <param name="needReload"></param>
        public void DeclareItem(IAveListItem listItem, string itemName, bool needReload = false)
        {            
            using (var performance = new AgentPerformanceScope("KeepData.DeclareItem", addToStatistics: true))
            {
                Guid lockKey = Guid.Empty;
                //if (mConfig.isRAJob)
                //{
                //    lockKey = RecordsSettingLocker.GetLocker(listItem.ParentList.ParentWeb.Site.ID.ToString());
                //    mLog.Info("Get Lock item name {0}", itemName);
                //}
                try
                {
                    #region change the active feature logic to init site collection setting in base
                    //try
                    //{
                    //    lock (mLock)
                    //    {
                    //        var mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
                    //        if (listItem.ParentList.ParentWeb.Site.Features[mRecordFeatureId] == null)
                    //        {
                    //            listItem.ParentList.ParentWeb.Site.Features.Add(mRecordFeatureId, true);
                    //        }
                    //    }
                    //}
                    //catch (InvalidOperationException ex)
                    //{
                    //    mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                    //    throw;
                    //}
                    //catch (Exception ex)
                    //{
                    //    mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                    //}
                    #endregion
                    //IAveListItem listItem = null;
                    //Office365Declare 文件Undeclare后需要reload Web并重新获取一次ListItem对象才能正确判断当前Item是否declare文件。
                    if (needReload)
                    {
                        listItem.ParentList.ParentWeb.ReloadWeb();
                        listItem = listItem.ParentList.ParentWeb.Lists[listItem.ParentList.ID].GetItemById(listItem.ID);
                    }

                    if (!listItem.FieldValues.ContainsKey("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E"))
                    {
                        mLog.Info("Reload Item for Records Enforce retention confict {0}", listItem.ID);
                        listItem.ParentList.ParentWeb.ReloadWeb();
                        listItem = listItem.ParentList.GetItemById(listItem.ID);
                    }
                    var isRecord = listItem.CheckIsRecord();
                    if (isRecord)
                    {
                        if (listItem.IsBlockDeleteOnlyRecord())
                        {
                            mLog.Info("Current status is not declare record block edit and delete need declare again {0}", listItem.Url.LogBase64());
                            Record.UndeclareItemAsRecord(listItem);
                            Record.DeclareItemAsRecord(listItem);
                        }
                    }
                    else
                    {
                        mLog.Info("Declared Record {0}", listItem.Url.LogBase64());
                        Record.DeclareItemAsRecord(listItem);
                    }

                    mLog.Debug("Declare item Successfully:{0}", itemName.LogBase64());

                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while declaring item, Item Name:{0}, Error Message:{1}", itemName.LogBase64(), e.ToString());
                    try
                    {
                        Thread.Sleep(1000);
                        mLog.Info($"Retry update item {listItem.Url.LogBase64()}");
                        listItem.ParentList.ParentWeb.ReloadWeb();
                        listItem = listItem.ParentList.GetItemById(listItem.ID);
                        Record.DeclareItemAsRecord(listItem);
                    }
                    catch (Exception ee)
                    {
                        mLog.Info($"Retry declared item failed {ee.ToString()}");
                        throw;
                    }
                }
                finally
                {
                    //if (mConfig.isRAJob)
                    //{
                    //    RecordsSettingLocker.ReleaseLocker(listItem.ParentList.ParentWeb.Site.ID.ToString(), lockKey);
                    //    mLog.Info("Release lock item {0}", itemName);
                    //}
                    //if (isListChange && mConfig.sharePointType != SharePointType.Office365)
                    //{
                    //    try
                    //    {
                    //        mLog.Debug("List First Item should Reload.ItemName:{0}", itemName);
                    //        listItem.ParentList.ParentWeb.ReloadWeb();
                    //        listItem.ParentList.Reload();
                    //        isListChange = false;
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        mLog.Info(" An Error Occur while Declare Item Reload Web,List.", e.ToString());
                    //    }
                    //}
                }
            }
        }
        public void UndeclareItem(IAveListItem listItem)
        {
            try
            {
                Record.UndeclareItemAsRecord(listItem);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting the record:{0}", e.ToString());
                throw;
            }
        }

        private string GetArchivedByUser(IAveSite site, string currentUser)
        {
            string loginName = string.Empty;
            string result = string.Empty;

            //if (currentUser.ToString().StartsWith("i:0#", StringComparison.OrdinalIgnoreCase))
            //{
            //    loginName = currentUser.ToString();
            //}
            //else
            //{
            //    loginName = "i:0#.w|" + currentUser.ToString();
            //}
            //IAveUser user = null;
            //if (site.RootWeb.SiteAdministrators != null && site.RootWeb.SiteAdministrators.GetByLoginName(loginName) != null)
            //{
            //    user = site.RootWeb.SiteAdministrators[loginName];
            //}
            //else if (site.RootWeb.AllUsers != null && site.RootWeb.AllUsers.GetByLoginName(loginName) != null)
            //{
            //    user = site.RootWeb.AllUsers[loginName];
            //}
            ////ADO-176766 如果Agent Account在Site Collection中不存在则Add User到当前Site Collection中，如果Add不进去则赋值成System Account.
            //if (user == null)
            //{
            //    try
            //    {
            //        user = site.RootWeb.EnsureUser(loginName);
            //        //为了防止EnsureUser后User为空的情况.
            //        if (user == null)
            //        {
            //            user = site.SystemAccount;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        mLog.Warn("Can't get archiver by user :{0} from site :{1} Message:{2}. ", loginName, site.Url, ex.ToString());
            //        user = site.SystemAccount;
            //    }
            //}
            IAveUser user = site.RootWeb.CurrentUser;
            result = user.ID.ToString() + ";#" + user.Name;
            return result;
        }

    }
}
