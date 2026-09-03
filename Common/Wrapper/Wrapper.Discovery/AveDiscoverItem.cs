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
using System.Linq;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Discovery
{

    public class AveDiscoverItem : AveDiscoverFilterBase, IAveDiscoverObjectInfo, IDisposable
    {
        internal AveItemCache ItemCache { get; set; }

        #region IAveDiscoverObjectInfo Members
        internal AveItemObject Obj;
        public int? ID { get { return Obj.ID; } set { Obj.ID = value; } }
        public Guid DocID { get { return Obj.DocID; } set { Obj.DocID = value; } }
        public Guid tp_GUID { get { return Obj.tp_GUID; } set { Obj.tp_GUID = value; } }
        public ChangeType ChangeType { get { return Obj.ChangeType; } set { Obj.ChangeType = value; } }
        public bool isRename { get { return Obj.isRename; } set { Obj.isRename = value; } }
        public ItemType ObjType { get { return Obj.ObjType; } set { Obj.ObjType = value; } }
        public string SourceName { get { return Obj.SourceName; } set { Obj.SourceName = value; } }
        public string FullUrl { get { return Obj.FullUrl; } set { Obj.FullUrl = value; } }
        public string ItemName { get { return Obj.ItemName; } set { Obj.FullUrl = value; } }
        public int Size { get { return Obj.Size; } set { Obj.Size = value; } }
        public string Author { get { return Obj.Author; } set { Obj.Author = value; } }
        public string Editor { get { return Obj.Editor; } set { Obj.Editor = value; } }

        //Indicates if the current is a built-in system object or not
        public bool IsSystemObject { get { return Obj.IsSystemObject; } }

        //add for SAAS-27045
        public long Length { get { return Obj.Length; } set { Obj.Length = value; } }
        public string CreatedBy { get { return Obj.CreatedBy; } set { Obj.CreatedBy = value; } }

        public string ModifyBy { get { return Obj.ModifyBy; } set { Obj.ModifyBy = value; } }
        public DateTime TimeLastModified { get { return Obj.TimeLastModified; } set { Obj.TimeLastModified = value; } }
        public string DirName { get { return Obj.DirName; } set { Obj.DirName = value; } }
        public string LeafName { get { return Obj.LeafName; } set { Obj.LeafName = value; } }
        public byte Level { get { return Obj.Level; } set { Obj.Level = value; } }
        public int Uiversion { get { return Obj.Uiversion; } set { Obj.Uiversion = value; } }
        public string UiVersionString { get { return Obj.UiVersionString; } set { Obj.UiVersionString = value; } }
        public bool IsCurrentVersion { get { return Obj.IsCurrentVersion; } set { Obj.IsCurrentVersion = value; } }
        public Guid ParentID { get { return Obj.ParentID; } set { Obj.ParentID = value; } }
        public byte Type { get { return Obj.Type; } set { Obj.Type = value; } }
        public DateTime TimeCreated { get { return Obj.TimeCreated; } set { Obj.TimeCreated = value; } }
        public int? DocFlags { get { return Obj.DocFlags; } set { Obj.DocFlags = value; } }
        public byte[] RbsId { get { return Obj.RbsId; } set { Obj.RbsId = value; } }
        public DateTime EventTime { get { return Obj.EventTime; } set { Obj.EventTime = value; } }
        public int? CheckoutUserId { get { return Obj.CheckoutUserId; } set { Obj.CheckoutUserId = value; } }
        public bool HasStream { get { return Obj.HasStream; } set { Obj.HasStream = value; } }
        public bool? Hidden { get { return Obj.Hidden; } set { Obj.Hidden = value; } }
        public int QueryType { get { return Obj.QueryType; } set { Obj.QueryType = value; } }
        public byte[] Content { get { return Obj.Content; } set { Obj.Content = value; } }
        public bool ItemPermissionChanged { get { return Obj.ItemPermissionChanged; } set { Obj.ItemPermissionChanged = value; } }
        public Guid ViewId { get { return Obj.ViewId; } set { Obj.ViewId = value; } }
        public List<AveSecurityObject> DeleteRoleAssignments { get { return Obj.DeleteRoleAssignments; } set { Obj.DeleteRoleAssignments = value; } }//存放permission的删除事件
        public string SPChangeType { get { return Obj.SPChangeType; } set { Obj.SPChangeType = value; } }
        public Dictionary<string, object> ItemProperties { get { return Obj.ItemProperties; } }
        public bool HasGetLAT { get { return Obj.HasGetLAT; } set { Obj.HasGetLAT = value; } }
        public DateTime LastAccessTime { get { return Obj.LastAccessTime; } set { Obj.LastAccessTime = value; } }
        private bool realFilterVersion = false;
        public bool RealFilterVersion
        {
            get { return realFilterVersion; }
        }

        #endregion

        public AveDiscoverItem() { }

        internal AveDiscoverItem(AveDiscoverFilterBase filter) : base(filter) { }

        public List<AveAlertObject> GetChangeAlerts()
        {
            return Obj.AlertObjs.Values.ToList();
        }

        public List<AveSecurityObject> GetChangeSecuritys()
        {
            var result = new List<AveSecurityObject>();
            foreach (var list in ItemCache.GetChangeSecuritys().Values)
            {
                result.AddRange(list);
            }
            return result;
        }

        /// <summary>
        /// 由于IB不能精确到Version，所以任何时候该方法都是返回该Item的所有version
        /// </summary>
        /// <returns></returns>
        public List<AveVersionObject> GetVersions()
        {
            return GetFilterVersions(Obj.VersionObjs);
        }

        public List<AveVersionObject> GetStubVersions()
        {
            return Obj.VersionObjs;
        }

        /// <summary>
        /// 由于IB需要精确到Attachment，所以 该方法分两种情况
        /// 当是IB的时候：该方法只得到改变的Attachment
        /// 当是FB的时候：该方法会得到所有的Attachment
        /// </summary>
        /// <returns></returns>
        public List<AveItemObject> GetAttachments()
        {
            return GetFilterAttachments(Obj.AttachmentObjs);
        }

        /// <summary>
        /// 提供一个根据discoverItem获取attachment的方法，主要针对IB，replicator需要将item
        /// 所有的attachmentdiscover出来，原有方法只discover出来变化的attachment，如果其他模块
        /// 有类似需求，可以在正常IBdiscover出item后调用该方法获取item的全部attachment信息。
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listRootUrl"></param>
        /// <returns></returns>
        public List<AveItemObject> GetAttachmentsForRP(Guid siteId, string listRootUrl)
        {
            Obj.AttachmentObjs.Clear();
            this.ItemCache.Query.QueryAttachmentByItemObj(siteId, listRootUrl, this.Obj);
            return GetFilterAttachments(Obj.AttachmentObjs);
        }

        private List<AveVersionObject> GetFilterVersions(List<AveVersionObject> versions)
        {
            if (this.ObjType == ItemType.Item)
            {
                return FilteredVersion(PolicyLevel.Item, versions);
            }
            else
            {
                return FilteredVersion(PolicyLevel.Document, versions);
            }
        }

        public IAveUser GetUserInfoById(int userId)
        {
            return this.ItemCache.AveWeb.SiteUsers.GetByID(userId);
        }

        private List<AveVersionObject> FilteredVersion(PolicyLevel policyLevel, List<AveVersionObject> versions)
        {
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && (ResultMode.HasMode(FilterResultMode.FilterHidden) || this.ID.HasValue))
            {
                List<FilterPolicy> policies = FilterPolicies.Where(policy => policy.Level == policyLevel && policy.Rule is VersionsRule).ToList();
                if (policies.Count != 0)
                {
                    List<AveVersionObject> result = new List<AveVersionObject>();
                    var policyResult = new Dictionary<int, bool>();
                    BoolExpressionAnalyser expressionAnalyser = BoolExpressionAnalyser.GetInstance(FilterExpressions[policyLevel]);
                    //SAAS-8956,由于最后一个version一定会还原,在进行lastNVersion filter时，默认将最后一个version计算在filter的统计内,将versionSequenceNo初值改为1.
                    int versionSequenceNo = 1;
                    int majorVersionSequenceNo = 0;
                    for (int index = 0; index < versions.Count; ++index)
                    {
                        if (versions[index].Uiversion == this.Uiversion)
                        {
                            result.Add(versions[index]);
                            continue;
                        }
                        bool isMajorVersion = versions[index].Uiversion % 512 == 0;
                        bool isApproved = versions[index].Level == 1;
                        foreach (FilterPolicy policy in policies)
                        {
                            switch (policy.Condition)
                            {
                                case PolicyCondition.OnlyLastNVersions:
                                    int lastVersionCount = int.Parse(policy.Value.Value1);
                                    policyResult[policy.SequenceNo] = lastVersionCount > versionSequenceNo;
                                    break;
                                case PolicyCondition.OnlyLastMajorNVersions:
                                    int leaveLastMajorVersionCount = int.Parse(policy.Value.Value1);
                                    policyResult[policy.SequenceNo] = isMajorVersion && leaveLastMajorVersionCount > majorVersionSequenceNo;
                                    break;
                                case PolicyCondition.OnlyMajorVersions:
                                    policyResult[policy.SequenceNo] = isMajorVersion;
                                    break;
                                case PolicyCondition.OnlyApproved:
                                    policyResult[policy.SequenceNo] = isApproved;
                                    break;
                                case PolicyCondition.OnlyMionrVersions:
                                case PolicyCondition.ExceptLastNVersions:
                                case PolicyCondition.MajorAndMintorVersions:
                                    //TO DO Log
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (expressionAnalyser.Caculate(policyResult))
                        {
                            result.Add(versions[index]);
                        }
                        ++versionSequenceNo;
                        majorVersionSequenceNo += isMajorVersion ? 1 : 0;
                    }

                    if (result.Count == versions.Count)
                    {
                        realFilterVersion = false;
                    }
                    else
                    {
                        realFilterVersion = true;
                    }
                    return result;
                }
            }
            realFilterVersion = false;
            return versions;
        }

        private List<AveItemObject> GetFilterAttachments(List<AveItemObject> attachments)
        {
            if (HasFilterWithLevel(PolicyLevel.Attachment) && ResultMode.HasMode(FilterResultMode.Trim))
            {
                return attachments.Where(attachemnt =>
                    {
                        try
                        {
                            return this.FilterEngine.IsQualified(this.GetFilterAttachmentInfo(this.FilterPolicies, attachemnt.LeafName));
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperDiscoverResource.AWDGetFilterAttachmentsError, e.ToString());
                            return false;
                        }
                    }).ToList();
            }
            return attachments;
        }

        public bool IsVersionQualified(int uiVersion)
        {
            return GetFilterVersions(this.Obj.VersionObjs).Exists(
                new Predicate<AveVersionObject>((versionObj) =>
                {
                    return versionObj.Uiversion == uiVersion;
                }));
        }

        public bool IsAttachmentQualified(string attachmentName)
        {
            return this.FilterEngine.IsQualified(this.GetFilterAttachmentInfo(this.FilterPolicies, attachmentName));
        }

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            if (ObjType == ItemType.Item)
            {
                IAveListItem item = null;
                if (this.ID.HasValue)
                {
                    item = this.ItemCache.AveWeb.GetListItem(this.FullUrl, this.ItemCache.ListId, this.ID.Value);
                }
                else
                {
                    item = this.ItemCache.AveWeb.GetListItem(this.FullUrl, this.ItemCache.ListId, this.DocID);
                }
                return FilterAnalyser.SetVersionAlwaysTrue(policies, FilterAnalyser.GetItemFilterInfo(policies, item));
            }
            else
            {
                Tuple<DocumentInfo, List<FilterPolicy>> filterInfoResult= FilterAnalyser.GetDocumentFilterInfo(policies, this);

                if (filterInfoResult.Item2 != null && filterInfoResult.Item2.Count > 0)
                {

                    IAveFile file = null;
                    IAveListItem item = null;
                    string fileUrl = this.FullUrl.Trim('/');
                    int relateUrlLength = this.ItemCache.AveWeb.ServerRelativeUrl.Trim('/').Length;
                    if (relateUrlLength > 0)//not root site collection
                    {
                        fileUrl = fileUrl.Substring(relateUrlLength + 1);
                    }
                    file = this.ItemCache.AveWeb.GetFile(fileUrl);
                    try
                    {
                        if ((file.Exists && file.Level != AveFileLevel.Checkout && this.CheckoutUserId != null) || (!file.Exists && this.CheckoutUserId != null))
                        {
                            IAveUser checkOutUser = this.ItemCache.AveWeb.SiteUsers.GetByID((int)this.CheckoutUserId);
                            IAveUserToken currToken = checkOutUser.UserToken;
                            AveObjectModelFactory siteFactory = AveObjectModelFactory.CreateObjectModelFactory(String.Empty, null, AveContextKind.Auto);
                            using (IAveSite checkOutSite = siteFactory.CreateSite(this.ItemCache.AveSite.ID, currToken))
                            {
                                using (IAveWeb checkOutWeb = checkOutSite.OpenWeb(this.ItemCache.AveWeb.ID))
                                {
                                    file = checkOutWeb.GetFile(fileUrl);
                                }
                            }
                        }
                        item = file.Exists ? file.Item : null;
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "Cannot get item for document:{0}, Full Url:{1}. Reason:{2}.", fileUrl, FullUrl, ex);
                    }
                    if (!file.Exists)
                    {
                        try
                        {
                            log.Log(AveLogLevel.DEBUG, "Cannot get item from document:{0}. FullUrl:{1}.", fileUrl, FullUrl);
                            item = this.ItemCache.AveWeb.GetListItem(this.FullUrl, this.ItemCache.ListId, this.DocID);
                            file = item.File == null ? file : item.File;
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, "Cannot get item for from web:{0}. Reason:{1}.", FullUrl, ex);
                        }
                    }

                    FilterAnalyser.GetDocumentFilterInfo(filterInfoResult.Item1, filterInfoResult.Item2, file, item);
                }

                return FilterAnalyser.SetVersionAlwaysTrue(policies, filterInfoResult.Item1);
            }
        }

        #endregion

        #region For Archiver/Extender
        IAveListItem currentItem;
        public IAveListItem CurrentItem
        {
            get
            {
                if (currentItem == null)
                {
                    if (this.ID.HasValue && this.ID.Value > 0)
                    {
                        using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.CurrentItem.GetListItem"))
                        {
                            currentItem = this.ItemCache.AveWeb.GetListItem(this.FullUrl, this.ItemCache.ListId, this.ID.Value);
                        }
                    }
                    else
                    {
                        currentItem = null; //this.ItemCache.AveWeb.GetListItem(this.FullUrl, this.ItemCache.ListId, this.DocID);
                    }
                }
                return currentItem;
            }
        }

        public ObjectInfoBase GetVersionObjectInfo(List<FilterPolicy> policies, int UIVersion)
        {
            IAveListItem item = CurrentItem;// this.ItemCache.ParentFolder.ParentWeb.AveWeb.GetListItem(this.FullUrl);
            if (this.ObjType == ItemType.Item)
            {
                return FilterAnalyser.GetItemVersionFilterInfo(policies, item, UIVersion);
            }
            else
            {
                return FilterAnalyser.GetDocumentVersionFilterInfo(policies, item, UIVersion);
            }
        }

        public ObjectInfoBase GetFilterAttachmentInfo(List<FilterPolicy> policies, string attachementName)
        {
            IAveListItem item = CurrentItem;// this.ItemCache.ParentFolder.ParentWeb.AveWeb.GetListItem(this.FullUrl);
            foreach (IAveAttachment attachemnt in item.Attachments)
            {
                if (attachemnt.FileName == attachementName)
                {
                    return FilterAnalyser.GetAttachmentFilterInfo(policies, this.ItemCache.AveWeb.GetFile(item.Attachments.UrlPrefix + attachementName), item);
                }
            }
            return null;
        }

        /// <summary>
        /// 该方法仅在StubItem才应该调用，返回最高UIVersion最为CurrentVersion
        /// </summary>
        public int GetCurrentUIVersion()
        {
            return this.ItemCache.Query.GetCurrentUIVersion(this.ItemCache.SiteId, this.ParentID, this.DocID);
        }


        #endregion

        public void Dispose()
        {
            ItemCache = null;
            currentItem = null;
            if (this.Obj != null)
            {
                this.Obj.Dispose();
                this.Obj = null;
            }
        }
    }


    /// <summary>
    /// 处理Bool表达式运算结果（1 And 2 Or (3 And 4)），注意默认是没有括号时，从左到右计算，不是And优先
    /// </summary>
    internal class BoolExpressionAnalyser
    {
        const char AndSymbol = '&';
        const char OrSymbol = '|';
        static Dictionary<string, BoolExpressionAnalyser> analysers = new Dictionary<string, BoolExpressionAnalyser>();
        public static BoolExpressionAnalyser GetInstance(string expression)
        {
            lock (analysers)
            {
                if (!analysers.ContainsKey(expression))
                {
                    analysers.Add(expression, new BoolExpressionAnalyser(expression));
                }
                return analysers[expression];
            }
        }

        class BoolDelegateGenerator
        {
            private static readonly Type[] _DelegateCtorSignature;
            private static ModuleBuilder module;
            private static ModuleBuilder Module
            {
                get
                {
                    if (module == null)
                    {
                        AssemblyName name = new AssemblyName("TempAssembly");
                        CustomAttributeBuilder[] assemblyAttributes = new CustomAttributeBuilder[] { new CustomAttributeBuilder(typeof(System.Security.SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0]) };
                        var _myAssembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, assemblyAttributes);
                        module = _myAssembly.DefineDynamicModule(name.Name);
                        //_myAssembly.DefineVersionInfoResource();
                    }
                    return module;
                }
            }
            private static Dictionary<int, Type> existDeletes;
            static BoolDelegateGenerator()
            {
                _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };
                existDeletes = new Dictionary<int, Type>();
                existDeletes.Add(1, typeof(Func<bool, bool>));
                existDeletes.Add(2, typeof(Func<bool, bool, bool>));
                existDeletes.Add(3, typeof(Func<bool, bool, bool, bool>));
                existDeletes.Add(4, typeof(Func<bool, bool, bool, bool, bool>));
            }

            public static Type GetBoolDelegate(int paraLength)
            {
                lock (existDeletes)
                {
                    if (!existDeletes.ContainsKey(paraLength))
                    {
                        existDeletes.Add(paraLength, MakeBoolDelegate(paraLength));
                    }
                    return existDeletes[paraLength];
                }
            }

            private static Type MakeBoolDelegate(int paraLength)
            {
                Type returnType = typeof(bool);
                Type[] parameterTypes = new Type[paraLength];
                for (int i = 0; i < paraLength; ++i)
                {
                    parameterTypes[i] = typeof(bool);
                }
                TypeBuilder builder = Module.DefineType("B" + paraLength, TypeAttributes.AutoClass | TypeAttributes.Sealed | TypeAttributes.Public, typeof(MulticastDelegate));
                builder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
                builder.DefineMethod("Invoke", MethodAttributes.VtableLayoutMask | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, returnType, parameterTypes).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
                return builder.CreateType();
            }
        }

        bool isEmptyExpression;
        List<int> parameterNumbers;
        Delegate calc;

        private BoolExpressionAnalyser(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                isEmptyExpression = true;
                return;
            }
            expression = expression.Replace(" ", "").ToLower().Replace("and", AndSymbol.ToString()).Replace("or", OrSymbol.ToString());//(1 Or 2 And 3)  => 1|2&3
            parameterNumbers = expression.Split(new char[] { AndSymbol, OrSymbol, '(', ')' }, StringSplitOptions.RemoveEmptyEntries).Select(strNo => int.Parse(strNo)).ToList();

            var paras = new Dictionary<int, ParameterExpression>();
            Expression calcExpression = GetBlockExpression(expression, paras);
            calc = Expression.Lambda(BoolDelegateGenerator.GetBoolDelegate(parameterNumbers.Count), calcExpression, parameterNumbers.Select(number => paras[number]).ToArray()).Compile();
        }

        private Expression GetBlockExpression(string expression, Dictionary<int, ParameterExpression> paras)
        {
            if (expression.IndexOfAny(new char[] { AndSymbol, OrSymbol }) < 0)
            {
                expression = expression.Trim('(', ')');
                return GetParaExpression(expression, paras);
            }
            var splitBlocks = SplitBlocks(expression);
            Expression curExpression = GetBlockExpression(splitBlocks[0], paras);
            for (int i = 1; i + 1 < splitBlocks.Count; i += 2)
            {
                Expression rightExpression = GetBlockExpression(splitBlocks[i + 1], paras);
                curExpression = splitBlocks[i][0] == AndSymbol ? Expression.And(curExpression, rightExpression) : Expression.Or(curExpression, rightExpression);
            }
            return curExpression;
        }

        private ParameterExpression GetParaExpression(string expression, Dictionary<int, ParameterExpression> paras)
        {
            var para = Expression.Parameter(typeof(bool), "b" + expression);
            paras.Add(int.Parse(expression), para);
            return para;
        }

        private List<string> SplitBlocks(string expression)
        {
            var results = new List<string>();
            while (expression.Contains('('))
            {
                int startIndex = expression.IndexOf('(');
                results.AddRange(GetSimpleBlocks(expression.Substring(0, startIndex)));
                int endIndex = startIndex;
                int leftCount = 1;
                while (leftCount > 0)
                {
                    endIndex = expression.IndexOfAny(new char[] { '(', ')' }, endIndex + 1);
                    leftCount += expression[endIndex] == '(' ? 1 : -1;
                }
                results.Add(expression.Substring(startIndex + 1, endIndex - startIndex - 1));
                expression = expression.Substring(endIndex + 1);
            }
            results.AddRange(GetSimpleBlocks(expression));
            return results;
        }

        private string[] GetSimpleBlocks(string expression)
        {
            if (expression.Length == 0)
            {
                return new string[0];
            }
            expression = expression.Replace(AndSymbol.ToString(), "_&_");
            expression = expression.Replace(OrSymbol.ToString(), "_|_");
            return expression.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool Caculate(Dictionary<int, bool> parameters)
        {
            if (isEmptyExpression)
            {
                return true;
            }
            return (bool)calc.DynamicInvoke(parameterNumbers.Select(number => parameters.ContainsKey(number) ? (object)parameters[number] : true).ToArray());
        }
    }
}
