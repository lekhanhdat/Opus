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
using AvePoint.Wrapper.Common;
using System.Text.RegularExpressions;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    [AveCodeReview("2012/03/06", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveTaxonomyFieldUtility
    {
        public static int DefaultLCID = -1;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// make sure the mapped field does exist. if not, will be created.
        /// </summary>
        /// <param name="fieldName">the full path of term set</param>
        /// <param name="fullPath"></param>
        public static IAveField EnsureListTaxonomyField(IAveList list, string fieldName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            try
            {
                IAveField field = list.Fields.GetField(fieldName);
                if ((string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)) || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                string internalName = CreateListTaxonomyField(list, fieldName, fullPath, LCID, info);
                return list.Fields.GetFieldByInternalName(internalName);
            }
        }

        public static IAveField EnsureListTaxonomyField(IAveList list, string fieldInternalName, string fieldDisplayName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            IAveField field = null;
            if (!string.IsNullOrEmpty(fieldInternalName))
            {
                try
                {
                    field = list.Fields.GetFieldByInternalName(fieldInternalName);
                    if ((string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)) || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        return field;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                catch (ArgumentException)
                {
                    string internalName = CreateListTaxonomyField(list, fieldInternalName, fullPath, LCID, info);
                    field = list.Fields.GetFieldByInternalName(internalName);
                    if (!string.IsNullOrEmpty(fieldDisplayName) && !field.Title.Equals(fieldDisplayName))
                    {
                        field.Title = fieldDisplayName;
                        field.Update();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(fieldDisplayName))
            {
                try
                {
                    field = list.Fields[fieldDisplayName];
                    if ((string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)) || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        return field;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                catch (ArgumentException)
                {
                    string internalName = CreateListTaxonomyField(list, fieldDisplayName, fullPath, LCID, info);
                    return list.Fields.GetFieldByInternalName(internalName);
                }
            }
            return field;
        }
        public static IAveField EnsureWebTaxonomyField(IAveWeb web, string fieldInternalName, string fieldDisplayName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            IAveField field = null;
            if (!string.IsNullOrEmpty(fieldInternalName))
            {
                try
                {
                    field = web.Fields.GetFieldByInternalName(fieldInternalName);
                    if ((string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)) || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        return field;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                catch (ArgumentException)
                {
                    string internalName = CreateWebTaxonomyField(web, fieldInternalName, fullPath, LCID, info);
                    field = web.Fields.GetFieldByInternalName(internalName);
                    if (!string.IsNullOrEmpty(fieldDisplayName) && !field.Title.Equals(fieldDisplayName))
                    {
                        field.Title = fieldDisplayName;
                        field.Update();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(fieldDisplayName))
            {
                try
                {
                    field = web.Fields[fieldDisplayName];
                    if ((string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)) || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        return field;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                catch (ArgumentException)
                {
                    string internalName = CreateWebTaxonomyField(web, fieldDisplayName, fullPath, LCID, info);
                    return web.Fields.GetFieldByInternalName(internalName);
                }
            }
            return field;
        }

        public static IAveField EnsureWebTaxonomyField(IAveWeb web, string fieldName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            try
            {
                IAveField field = web.Fields.GetField(fieldName);
                if ((string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)) || string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch (ArgumentException)
            {
                string internalName = CreateWebTaxonomyField(web, fieldName, fullPath, LCID, info);
                return web.Fields.GetFieldByInternalName(internalName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName">the full path of term set</param>
        /// <param name="fullPath"></param>
        public static string CreateWebTaxonomyField(IAveWeb web, string fieldName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            return CreateTaxonomyField(web, fieldName, fullPath, LCID, info);
        }

        internal static IAveTaxonomySession GetTaxonomySession(object obj)
        {
            IAveTaxonomySession session = null;
            if (obj is IAveSite)
            {
                session = (obj as IAveSite).AveSPTaxonomySession;
            }
            else if (obj is IAveWeb)
            {
                session = ((obj as IAveWeb).Site).AveSPTaxonomySession;
            }
            else if (obj is IAveList)
            {
                session = ((obj as IAveList).ParentWeb.Site).AveSPTaxonomySession;
            }

            return session;
        }
        // return taxonomyField internalName
        internal static string CreateTaxonomyField(object spObj, string fieldName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            string[] path = fullPath.Split(';');
            string groupName = path[0];
            string termSetName = path[1];

            IAveWeb web = null;
            IAveList list = null;
            bool isSPWeb = false;
            IAveTermSet termSet = null;
            IAveTaxonomySession session = GetTaxonomySession(spObj);
            if (session == null)
            {
                throw new Exception("Cannot get taxonomy session");
            }

            IAveTermSetCollection termSets = null;
            foreach (IAveTermStore termStore in session.TermStores)
            {
                if (LCID < 0)
                {
                    LCID = termStore.WorkingLanguage;
                }
                termSets = termStore.GetTermSets(termSetName, LCID);
                foreach (IAveTermSet tSet in termSets)
                {
                    if (tSet.Group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    {
                        termSet = tSet;
                        break;
                    }
                }
                if (termSet != null)
                {
                    break;
                }
            }
            //IAveTermSetCollection termSets = session.GetTermSets(termSetName, LCID);
            //foreach (IAveTermSet tSet in termSets)
            //{
            //    if (tSet.Group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        termSet = tSet;
            //        break;
            //    }
            //}

            if (termSet == null)
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_TermSetNotExist, termSetName);
            }

            IAveTaxonomyField taxonomyField = null;
            if (spObj is IAveWeb)
            {
                web = spObj as IAveWeb;
                isSPWeb = true;
                taxonomyField = web.Fields.CreateNewField("TaxonomyFieldType", fieldName) as IAveTaxonomyField;
            }
            else if (spObj is IAveList)
            {
                list = spObj as IAveList;
                taxonomyField = list.Fields.CreateNewField("TaxonomyFieldType", fieldName) as IAveTaxonomyField;
            }

            PrepareForCreation(taxonomyField, info, termSet);
            string fieldInternalName = string.Empty;
            if (isSPWeb)
            {
                fieldInternalName = web.Fields.Add(taxonomyField).InternalName;
            }
            else
            {
                fieldInternalName = list.Fields.Add(taxonomyField).InternalName;
            }

            web = null;
            list = null;
            return fieldInternalName;
        }

        public static string CreateListTaxonomyField(IAveList list, string fieldName, string fullPath, int LCID, TaxonomyFieldCreationInformation info)
        {
            return CreateTaxonomyField(list, fieldName, fullPath, LCID, info);
        }

        internal static void PrepareForCreation(IAveTaxonomyField taxonomyField, TaxonomyFieldCreationInformation info, IAveTermSet termSet)
        {
            taxonomyField.AnchorId = Guid.Empty;
            taxonomyField.CreateValuesInEditForm = false;
            taxonomyField.Open = false;
            taxonomyField.SspId = termSet.TermStore.ID;
            taxonomyField.TermSetId = termSet.ID;
            taxonomyField.TargetTemplate = string.Empty;
            taxonomyField.AllowMultipleValues = info.AllowMultipleValues;
        }

        public static void SetFieldValue(IAveListItem item, Dictionary<string, string> dic, int LCID)
        {
            SetFieldValue(item, dic, true, LCID, null, null);
        }

        public static void SetFieldValue(IAveListItem item, Dictionary<string, string> dic, int LCID, Dictionary<Guid, Guid> termIdMapping)
        {
            SetFieldValue(item, dic, true, LCID, termIdMapping, null);
        }

        public static void SetFieldValue(IAveListItem item, Dictionary<string, string> dic, bool forceAddTerm, int LCID, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping)
        {
            Dictionary<Guid, IAveTerm> termCache = item.ParentList.ParentWeb.Site.TermIdCache;
            foreach (string fieldName in dic.Keys)
            {
                IAveField field = item.ParentList.Fields.GetField(fieldName);
                IAveTaxonomyField tField = field as IAveTaxonomyField;
                IAveTaxonomySession session = (item.ParentList.ParentWeb.Site).AveSPTaxonomySession;
                IAveTermStore termStore = GetTermStore(field, session, ref LCID);
                if (termStore == null)
                {
                    continue;
                }
                IAveTermSet termSet = null;
                if (tField.TermSetId != Guid.Empty && termStore != null)
                {
                    termSet = termStore.GetTermSet(tField.TermSetId);
                }
                IAveTerm endTerm = null;
                if (tField.AnchorId != Guid.Empty && termSet != null)
                {
                    endTerm = termSet.GetTerm(tField.AnchorId);
                }

                bool submit = false;
                HashSet<String> termNames = new HashSet<string>(dic[fieldName].Split(';'), StringComparer.OrdinalIgnoreCase);
                List<IAveTerm> terms = new List<IAveTerm>();
                foreach (string termName in termNames)
                {
                    string tName = termName.StartsWith("#", StringComparison.Ordinal) ? termName.Substring(1) : termName;
                    if (string.IsNullOrEmpty(tName) || string.IsNullOrEmpty(tName.Trim()))
                    {
                        continue;
                    }
                    var term = FindTerm(tName, LCID, forceAddTerm, endTerm, termSet, tField, session, termCache, termIdMapping, mergedTermIdMapping, termStore, ref submit);
                    if (term != null)
                    {
                        terms.Add(term);
                        //如果field不允许多值，没有必要找多个term了。
                        if (!tField.AllowMultipleValues)
                        {
                            break;
                        }
                    }
                }

                if (submit)
                {
                    try
                    {
                        termStore.CommitAll();
                        submit = false;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCClearTermError, e.ToString());
                    }
                }
                SetTaxonomyValueToItem(item, tField, terms, LCID);
            }
        }

        public static IAveTerm FindTerm(string tName, int LCID, bool forceAddTerm, IAveTerm endTerm, IAveTermSet termSet, IAveTaxonomyField tField, IAveTaxonomySession session, Dictionary<Guid, IAveTerm> termCache, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping, IAveTermStore termStore, ref bool submit)
        {
            IAveTerm term = null;
            Guid tTermId = Guid.Empty;
            string[] termHiberarchy = null;
            try
            {
                if (tName.Contains("|"))
                {
                    try
                    {
                        string[] temp = tName.Split('|');
                        if (temp.Length == 2)
                        {
                            tName = temp[0];
                            tTermId = new Guid(temp[1]);
                            term = TryGetTermById(termCache, termIdMapping, mergedTermIdMapping, tTermId, tField, termSet, session, termStore);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Debug(WrapperCommonResource.AWCGetTermByIdError, e);
                    }
                }
                //'<'表示term的层次关系。
                else if (tName.Contains("<"))
                {
                    //在这个case中，目前无法获取到term的guid，所以无法利用缓存提高效率，可能存在效率问题。
                    termHiberarchy = tName.Split('<');
                    bool needContinue = false;
                    term = FindTermForColumnMapping(session, tField, termSet, endTerm, tName, ref needContinue);
                    if (needContinue)
                    {
                        return null;
                    }
                }
                if (term == null && termSet != null)
                {
                    term = TryGetTermByName(termCache, tName, endTerm, termSet, tTermId);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            //TryGetTermByName 经过修改ADO-172523，不抛异常，将创建逻辑拿到外面
            if (term == null && forceAddTerm && termSet != null && !String.IsNullOrEmpty(tName))
            {
                term = CreateNotExistTerm(termSet, endTerm, tName, LCID, ref submit);
                log.Debug("Force Add Term. Term Name:{0}", tName);
            }
            return term;
        }

        public static IAveTerm TryGetTermById(Dictionary<Guid, IAveTerm> termCache, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping, Guid tTermId, IAveTaxonomyField tField, IAveTermSet termSet, IAveTaxonomySession session, IAveTermStore termStore)
        {
            IAveTerm term = null;
            if (!termCache.ContainsKey(tTermId))
            {
                if (termIdMapping != null && termIdMapping.ContainsKey(tTermId))
                {
                    tTermId = termIdMapping[tTermId];
                }
                else if (mergedTermIdMapping != null)  //ADO-148478:FindTermById还要考虑term的mergedTermIds属性中元素
                {
                    foreach (var pair in mergedTermIdMapping)
                    {
                        if (pair.Value.Contains(tTermId))
                        {
                            tTermId = pair.Key;
                            break;
                        }
                    }
                }
                if (termSet != null)
                {
                    term = termSet.GetTerm(tTermId);
                    //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                    if (term == null && tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
                    {
                        foreach (IAveTermStore tStore in session.TermStores)
                        {
                            if (term == null)
                            {
                                term = tStore.GetTerm(tTermId);
                            }
                        }
                    }
                }
                else
                {
                    term = termStore.GetTerm(tTermId);
                }
                if (!termCache.ContainsKey(tTermId) && tTermId != Guid.Empty && term != null)
                {
                    termCache.Add(tTermId, term);
                }
            }
            else
            {
                term = termCache[tTermId];
            }
            return term;
        }

        public static IAveTerm TryGetTermByName(Dictionary<Guid, IAveTerm> termCache, string tName, IAveTerm endTerm, IAveTermSet termSet, Guid tTermId)
        {
            IAveTerm term = GetTermByName(tName, endTerm, termSet);
            if (term != null && tTermId != Guid.Empty && !termCache.ContainsKey(tTermId))
            {
                termCache.Add(tTermId, term);
            }
            return term;
        }

        private static IAveTerm GetTermByName(string tName, IAveTerm endTerm, IAveTermSet termSet)
        {
            IAveTerm term = null;
            try
            {
                return term = endTerm == null ? GetTermInTermSet(NormalizeName(tName), termSet) : GetTermInTerm(NormalizeName(tName), endTerm);
            }
            catch (Exception e)
            {
                log.Debug(WrapperCommonResource.AWCGetTermByNameError, e);
                //DOC-78396 使用此方法刷新对象
                var ts = termSet.GetTerms(NormalizeName(tName).Trim(), true);
                return term = endTerm == null ? GetTermInTermSet(NormalizeName(tName), termSet) : GetTermInTerm(NormalizeName(tName), endTerm);
            }
        }

        /// <summary>
        /// Find Term in termset by term name. return first one.
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="termSet"></param>
        /// <returns></returns>
        private static IAveTerm GetTermInTermSet(string tName, IAveTermSet termSet)
        {
            var collection = termSet.GetTerms(tName, true);
            return collection != null && collection.Count > 0 ? collection[0] : null;
        }

        /// <summary>
        /// Find term in a term. Breadth first
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        private static IAveTerm GetTermInTerm(string tName, IAveTerm term)
        {
            foreach (var subTerm in term.Terms.Where(subTerm => subTerm.Name.Equals(tName, StringComparison.OrdinalIgnoreCase)))
            {
                return subTerm;
            }
            return term.Terms.Select(subTerm => GetTermInTerm(tName, subTerm)).FirstOrDefault(result => result != null);
        }
        public static IAveTermStore GetTermStore(IAveField field, IAveTaxonomySession session, ref int LCID)
        {
            IAveTermStore termStore = null;
            IAveTaxonomyField tField = field as IAveTaxonomyField;
            Guid sspId = Guid.Empty;
            if (tField.SspId == Guid.Empty && !tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
            {
                object customProperty = field.GetCustomProperty("SspId");
                if (customProperty != null)
                {
                    sspId = new Guid(customProperty.ToString());
                }
            }
            else
            {
                sspId = tField.SspId;
            }
            if (sspId != Guid.Empty)
            {
                try
                {
                    termStore = session.TermStores[sspId];
                }
                catch (Exception ex)
                {
                    //如果原端的field使用的service不在被原端引用，也就是说mms没有被还原，该field的原端属性无法替换，这个sspid也是原端的Id，这时在目的端无法找到
                    //为了保障其他的mms field属性的正确还原，添加try catch，跳过该field的还原
                    log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCSetFieldValueError, sspId, ex.ToString());
                    return null;
                }
            }
            else
            {
                termStore = session.DefaultKeywordsTermStore;
                if (termStore == null)
                {
                    termStore = session.DefaultSiteCollectionTermStore;
                }
                if (termStore == null)
                {
                    termStore = session.TermStores[0];
                }
            }
            if (LCID < 0)
            {
                LCID = termStore.WorkingLanguage;
            }
            if (termStore != null && !termStore.Languages.Contains(DefaultLCID))
            {
                DefaultLCID = termStore.WorkingLanguage;
                LCID = DefaultLCID;
            }
            return termStore;
        }

        private static void SetTaxonomyValueToItem(IAveListItem item, IAveTaxonomyField tField, List<IAveTerm> terms, int LCID)
        {
            if (tField.AllowMultipleValues)
            {
                IAveTaxonomyFieldValueCollection taxValueCollection = tField.TaxonomyFieldValueCollection;
                foreach (IAveTerm tTerm in terms)
                {
                    if (tTerm != null)
                    {
                        int effectiveLcid = LCID;
                        //string text = tTerm.GetDefaultLabel(effectiveLcid) + "|" + tTerm.ID;
                        IAveTaxonomyFieldValue value2 = tField.TaxonomyFieldValue;
                        value2.TermGuid = tTerm.ID.ToString();
                        value2.Label = tTerm.GetDefaultLabel(effectiveLcid);
                        //value2.PopulateFromLabelGuidPair(text);
                        taxValueCollection.Add(value2);
                    }
                }
                item[tField.ID] = taxValueCollection;
                item[tField.TextField] = taxValueCollection.ToString();
            }
            else
            {
                if (terms.Count > 0)
                {
                    int effectiveLcid = LCID;
                    //string text = terms[0].GetDefaultLabel(effectiveLcid) + "|" + terms[0].ID;
                    IAveTaxonomyFieldValue taxValue = tField.TaxonomyFieldValue;
                    taxValue.TermGuid = terms[0].ID.ToString();
                    taxValue.Label = terms[0].GetDefaultLabel(effectiveLcid);
                    //taxValue.PopulateFromLabelGuidPair(text);
                    item[tField.ID] = taxValue;
                    item[tField.TextField] = taxValue.ToString();
                }
            }
        }

        public static IAveTerm FindTermForColumnMapping(IAveTaxonomySession session, IAveTaxonomyField tField, IAveTermSet termSet, IAveTerm endTerm, string tName, ref bool needContinue)
        {
            IAveTerm term = null;
            string[] termHiberarchy = tName.Split('<');
            if (tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
            {
                string groupName = termHiberarchy[0];
                string termSetName = termHiberarchy[1];
                IAveTermSet tmpSet = null;
                foreach (IAveTermStore tStore in session.TermStores)
                {
                    try
                    {
                        tmpSet = tStore.Groups[groupName].TermSets[termSetName];
                        break;
                    }
                    catch (Exception ex)
                    {
                        log.Info("Can not find term group or term set. group name:{0} term set name:{1}. Exception:{2}", groupName, termSetName, ex.ToString());
                    }
                }
                if (tmpSet != null)
                {
                    try
                    {
                        term = tmpSet.Terms[termHiberarchy[2]];
                        for (int i = 3; i < termHiberarchy.Length; i++)
                        {
                            if (string.IsNullOrEmpty(termHiberarchy[i]))
                            {
                                continue;
                            }
                            term = term.Terms[NormalizeName(termHiberarchy[i])];
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Info("Can not find term. term name:{0} . Exception:{1}", tName, ex.ToString());
                    }
                }
                if (term == null)
                {
                    needContinue = true;
                }
            }
            else
            {
                if (endTerm == null)
                {
                    term = termSet.Terms[NormalizeName(tName)];
                }
                else
                {
                    term = endTerm.Terms[NormalizeName(tName)];
                }
                for (int i = 1; i < termHiberarchy.Length; i++)
                {
                    if (string.IsNullOrEmpty(termHiberarchy[i]))
                    {
                        continue;
                    }
                    term = term.Terms[NormalizeName(termHiberarchy[i])];
                }
            }
            return term;
        }

        public static IAveTerm CreateNotExistTerm(IAveTermSet termSet, IAveTerm endTerm, string tName, int LCID, ref bool submit)
        {
            IAveTerm term = null;
            string[] termHiberarchy = tName.Split('<');
            try
            {
                if (tName.Contains(";"))//ADO-86650
                {
                    if (endTerm != null)
                    {
                        return endTerm;
                    }
                    return null;
                }
                if (endTerm == null)
                {
                    term = termSet.Terms[NormalizeName(termHiberarchy[0])];
                }
                else
                {
                    term = endTerm.Terms[NormalizeName(termHiberarchy[0])];
                }
            }
            catch (Exception ex)
            {
                try
                {
                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, ex.ToString());
                    if (endTerm == null)
                    {
                        term = termSet.CreateTerm(NormalizeName(termHiberarchy[0]), LCID, Guid.NewGuid());
                    }
                    else
                    {
                        term = endTerm.CreateTerm(NormalizeName(termHiberarchy[0]), LCID, Guid.NewGuid());
                    }
                    term.TermStore.CommitAll();
                }
                catch (UnauthorizedAccessException)
                {
                    if (!termSet.IsOpenForTermCreation)
                    {
                        object sharedTermSet = AveAssemblyUtility.GetPropertyValue(termSet, "SharedTermSet");
                        if (sharedTermSet != null)
                        {
                            AveAssemblyUtility.SetFieldValue(sharedTermSet, "isOpen", true);
                            if (endTerm == null)
                            {
                                term = termSet.CreateTerm(tName, LCID, Guid.NewGuid());
                            }
                            else
                            {
                                term = endTerm.CreateTerm(tName, LCID, Guid.NewGuid());
                            }
                            submit = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            if (termHiberarchy.Length > 1)
            {
                for (int i = 1; i < termHiberarchy.Length; i++)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(termHiberarchy[i]))
                        {
                            continue;
                        }
                        term = term.Terms[NormalizeName(termHiberarchy[i])];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, e.ToString());
                        term = term.CreateTerm(NormalizeName(termHiberarchy[i]).Trim(), LCID, Guid.NewGuid());
                        term.TermStore.CommitAll();
                    }
                }
            }
            return term;
        }

        public static void UpdateTaxonomyValue(IAveListItem item, Dictionary<string, string> dic, bool ForceAddTerm, int LCID, bool increaseVersion, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping)
        {
            SetFieldValue(item, dic, ForceAddTerm, LCID, termIdMapping, mergedTermIdMapping);

            item.SystemUpdate(increaseVersion);
        }

        public static string GetTermStoreName(IAveSite site, Guid termStoreId)
        {
            IAveTaxonomySession session = GetTaxonomySession(site);
            foreach (IAveTermStore termStore in session.TermStores)
            {
                if (termStore.ID == termStoreId)
                {
                    return termStore.Name;
                }
            }
            return string.Empty;
        }

        public static Guid GetTermStoreId(IAveSite site, string termStoreName)
        {
            IAveTaxonomySession session = GetTaxonomySession(site);
            foreach (IAveTermStore termStore in session.TermStores)
            {
                if (termStore.Name == termStoreName)
                {
                    return termStore.ID;
                }
            }
            IAveTermStore tStore = null;
            tStore = session.DefaultSiteCollectionTermStore;
            if (tStore == null)
            {
                tStore = session.DefaultKeywordsTermStore;
            }
            if (tStore == null && session.TermStores.Count > 0)
            {
                tStore = session.TermStores[0];
            }
            return tStore == null ? Guid.Empty : tStore.ID;
            //return Guid.Empty;
        }

        public static string GetTermGroupName(IAveSite site, ref Guid termStoreId, Guid groupId)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = null;
                IAveTaxonomyGroup group = null;
                if (termStoreId != Guid.Empty)
                {
                    termStore = session.TermStores[termStoreId];
                    if (termStore != null)
                    {
                        group = termStore.GetGroup(groupId);
                    }
                }
                else
                {
                    foreach (IAveTermStore tTermStore in session.TermStores)
                    {
                        try
                        {
                            group = tTermStore.GetGroup(groupId);
                            break;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermGroupByIdError, e.ToString());
                            group = null;
                        }
                    }
                }
                if (group != null)
                {
                    termStoreId = group.TermStore.ID;
                    return group.Name;
                }
            }
            catch (Exception e)//no exception should be thrown.(Luo Qinglong)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermGroupNameError, e.ToString());
                return string.Empty;
            }
            return string.Empty;
        }

        public static Guid GetTermGroupId(IAveSite site, ref Guid termStoreId, string groupName)
        {
            Guid groupID = Guid.Empty;
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                if (termStoreId != Guid.Empty)
                {
                    IAveTermStore termStore = session.TermStores[termStoreId];
                    IAveTaxonomyGroup group = termStore.Groups[groupName];
                    if (group != null)
                    {
                        groupID = group.ID;
                    }
                }
                if (groupID == Guid.Empty && session.DefaultSiteCollectionTermStore != null)
                {
                    try
                    {
                        IAveTermStore termStore = session.DefaultSiteCollectionTermStore;
                        IAveTaxonomyGroup group = termStore.Groups[groupName];
                        if (group != null)
                        {
                            groupID = group.ID;
                            termStoreId = termStore.ID;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Debug("An error occurred while getting term group from term store. Term store name is {0}. Error message: {1}", session.DefaultSiteCollectionTermStore.Name, e.ToString());
                    }

                }
                if (groupID == Guid.Empty)
                {
                    foreach (IAveTermStore tTermStore in session.TermStores)
                    {
                        try
                        {
                            IAveTaxonomyGroup group = tTermStore.Groups[groupName];
                            if (group != null)
                            {
                                groupID = group.ID;
                                termStoreId = tTermStore.ID;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Debug("An error occurred while getting term group from term store. Term store name is {0}. Error message: {1}", tTermStore.Name, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermGroupIdError, e.ToString());
            }
            return groupID;
        }

        public static string GetTermSetName(IAveSite site, ref Guid termStoreId, ref Guid groupId, Guid termSetId)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = null;
                IAveTaxonomyGroup group = null;
                if (termStoreId != Guid.Empty)
                {
                    try
                    {
                        termStore = session.TermStores[termStoreId];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
                    }
                }
                if (groupId != Guid.Empty && termStore != null)
                {
                    group = termStore.GetGroup(groupId);
                }
                if (group != null)
                {
                    try
                    {
                        return group.TermSets[termSetId].Name;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
                    }
                }
                else
                {
                    if (termStore != null)
                    {
                        return ProcessTermStore(site, termStore, termSetId, ref termStoreId, ref groupId, false);
                    }
                    else
                    {
                        foreach (IAveTermStore tTermStore in session.TermStores)
                        {
                            return ProcessTermStore(site, tTermStore, termSetId, ref termStoreId, ref groupId, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
            }//no need to log
            return string.Empty;
        }


        private static string ProcessTermStore(IAveSite site, IAveTermStore termStore, Guid termSetId, ref Guid termStoreId, ref Guid groupId, bool needGiveValue)
        {
            string result = string.Empty;
            try
            {
                var termSet = termStore.GetTermSet(termSetId);
                if (termSet == null)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                }
                termStoreId = needGiveValue ? termSet.TermStore.ID : termStoreId;
                groupId = termSet.Group.ID;
                result = termSet.Name;
            }
            catch (Exception e)
            {
                log.Warn("Cannot find term set by id:{0}, details:{1}", termSetId, e.ToString());
            }
            return result;
            //foreach (IAveTaxonomyGroup tGroup in termStore.Groups)
            //{
            //    try
            //    {
            //        if (tGroup.IsSiteCollectionGroup && !tGroup.SiteCollectionAccessIds.Contains(site.ID))
            //        {
            //            continue;
            //        }
            //        IAveTermSet termSet = tGroup.TermSets[termSetId];
            //        termStoreId = needGiveValue ? termSet.TermStore.ID : termStoreId;
            //        groupId = tGroup.ID;
            //        return termSet.Name;
            //    }
            //    catch (Exception e)
            //    {
            //        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
            //    }
            //}
            //return string.Empty;
        }

        /// <summary>
        /// find term set by name and id, find the best match one
        /// </summary>
        /// <param name="site"></param>
        /// <param name="context"></param>
        /// <param name="termSetName"></param>
        /// <returns></returns>
        internal static IAveTermSet FindTermSet(IAveSite site,TaxonomyContext context, string termSetName)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = null;
                IAveTaxonomyGroup group = null;

                if (context.SspId != Guid.Empty)
                {
                    try
                    {
                        termStore = session.TermStores[context.SspId];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
                    }
                }
                if (context.GroupId != Guid.Empty && termStore != null)
                {
                    group = termStore.GetGroup(context.GroupId);
                }
                if (group != null)
                {
                    try
                    {
                        return group.TermSets[termSetName];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
                    }
                }
                else
                {
                    if (termStore != null && !termStore.Languages.Contains(DefaultLCID))
                    {
                        DefaultLCID = termStore.WorkingLanguage;
                    };
                    return GetTermSetByOrder(site, session, termSetName,context);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetGroupIdError, e.ToString());
            }
            return null;
        }

        internal static Guid GetTermSetId(IAveSite site, TaxonomyContext context, string termSetName)
        {
            var termSet = FindTermSet(site, context, termSetName);
            if (termSet != null)
            {
                return termSet.ID;
            }
            return Guid.Empty;
        }

        public static Guid GetTermSetId(IAveSite site, ref Guid termStoreId, ref Guid groupId, string termSetName)
        {
            var taxonomyContext = new TaxonomyContext
            {
                SspId = termStoreId,
                GroupId = groupId
            };
            GetTermSetId(site, taxonomyContext, termSetName);
            termStoreId = taxonomyContext.SspId;
            groupId = taxonomyContext.GroupId;
            return taxonomyContext.TermSetId;
        }

        /// <summary>
        /// 以一定顺序获取Term Set。(SameIdTerm > Local Term Set > Global Term Set > Access Site Collection Term Set. 详见：ADO-134879 )
        /// </summary>
        /// <param name="site"></param>
        /// <param name="session"></param>
        /// <param name="termSetName"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static IAveTermSet GetTermSetByOrder(IAveSite site, IAveTaxonomySession session, string termSetName, TaxonomyContext context)
        {
            IAveTermSet resultTermSet = null;
            IAveTermSet destinationSiteColectionLocalTermSet = null;
            IAveTermSet destinationGlobalTermSet = null;
            IAveTermSet destinationAccessSiteColectionTermSet = null;
            IAveTermSetCollection termSets = session.GetTermSets(termSetName, session.TermStores[0].DefaultLanguage);
            foreach (var termSet in termSets)
            {
                if (context.SspId != Guid.Empty && context.SspId != termSet.TermStore.ID)
                {
                    continue;
                }
                
                if (termSet.Group != null && termSet.Group.IsSiteCollectionGroup) //Local Term Set
                {
                    if (!termSet.Group.SiteCollectionAccessIds.Contains(site.ID) &&
                        (termSet.Group.SiteCollectionReadOnlyAccessUrls == null || !termSet.Group.SiteCollectionReadOnlyAccessUrls.Contains(site.Url)))
                    {
                        continue;
                    }
                    var group = termSet.TermStore.GetSiteCollectionGroup(site, false);
                    if (group != null && group.ID == termSet.Group.ID) //找到本Site Collection的Local Term Set
                    {
                        destinationSiteColectionLocalTermSet = termSet;
                        break;
                    }
                    else
                    {
                        destinationAccessSiteColectionTermSet = termSet;
                    }
                }
                else
                {
                    //ADO-173350 不同site collection 的local termset 要区分，不能只用name 和id确认
                    if (context.TermSetId != Guid.Empty && context.TermSetId == termSet.ID)
                    {
                        //找Name,Id都match的,就是需要的TermSet
                        resultTermSet = termSet;
                        break;
                    }
                    destinationGlobalTermSet = termSet;
                }
            }

            if (resultTermSet == null)
            {
                if (destinationSiteColectionLocalTermSet != null)
                {
                    resultTermSet = destinationSiteColectionLocalTermSet;
                }
                else if (destinationGlobalTermSet != null)
                {
                    resultTermSet = destinationGlobalTermSet;
                }
                else if (destinationAccessSiteColectionTermSet != null)
                {
                    resultTermSet = destinationAccessSiteColectionTermSet;
                }
                else
                {
                    throw new AveNotSupportedException(AveInternalResourceKey.Wrapper_Exception_Restore_TermSetNotFound, termSetName);
                }
            }
            context.SspId = resultTermSet.TermStore.ID;
            return resultTermSet;
        }

        public static string GetTermName(IAveSite site, Guid termStoreId, Guid termSetId, Guid termId)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = session.TermStores[termStoreId];
                IAveTerm term = termStore.GetTerm(termSetId, termId);
                //ADO-156216：把默认语言的DefalutLabel作为termName
                string termName = term.GetDefaultLabel(termStore.DefaultLanguage);
                while (term.Parent != null)
                {
                    term = term.Parent;
                    termName = term.Name + ";" + termName;
                }
                return termName;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermNameError, e.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// 为了支持field mapping里的changtometadata，改过可以按照路径获取多层term
        /// </summary>
        /// <param name="site"></param>
        /// <param name="termStoreId"></param>
        /// <param name="termSetId"></param>
        /// <param name="termName"></param>
        /// <returns></returns>
        public static Guid GetTermId(IAveSite site, Guid termStoreId, Guid termSetId, string termNames)
        {
            try
            {
                Guid g = Guid.Empty;
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = session.TermStores[termStoreId];
                IAveTermSet termSet = termStore.GetTermSet(termSetId);
                string[] terms = termNames.Split(';');
                IAveTerm term = null;
                foreach (string termName in terms)
                {
                    if (!string.IsNullOrEmpty(termName))
                    {
                        if (term == null)
                        {
                            term = termSet.Terms[termName];
                            g = term.ID;
                        }
                        else
                        {
                            term = term.Terms[termName];
                            g = term.ID;
                        }
                    }
                }
                return g;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermIdError, e.ToString());
                return Guid.Empty;
            }
        }

        public static bool ResetTaxnomyFieldDefaultValue(IAveSite site, IAveTaxonomyField field, string defaultValue, Dictionary<Guid, Guid> termIdMapping)
        {
            if (string.IsNullOrEmpty(defaultValue))
            {
                return false;
            }
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = session.TermStores[field.SspId];
                IAveTermSet termSet = termStore.GetTermSet(field.TermSetId);
                IAveTerm term = null;
                string[] values = defaultValue.Split(';');
                IAveTaxonomyFieldValueCollection taxonomyValues = null;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i].StartsWith("#", StringComparison.OrdinalIgnoreCase) && values[i].IndexOf('|') > 0)
                    {
                        term = null;
                        string termName = values[i].Substring(1, values[i].IndexOf('|') - 1);
                        string strTermId = values[i].Substring(values[i].IndexOf('|') + 1);
                        Guid termId = new Guid(strTermId);
                        if (termIdMapping != null && termIdMapping.ContainsKey(termId))
                        {
                            termId = termIdMapping[termId];
                        }
                        try
                        {
                            term = termSet.GetTerm(termId);
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByIdError, e);
                        }
                        finally
                        {
                            if (term == null)
                            {
                                try
                                {
                                    term = termSet.Terms[termName];
                                }
                                catch (Exception ex)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetByNameError, ex);
                                }
                            }
                        }
                        if (term == null)
                        {	//由于RestoreUsedTermOnly的存在，不能保证在reset时目的端一定存在需要用到的term，不存在则创建。
                            try
                            {
                                term = termSet.CreateTerm(termName, termStore.DefaultLanguage, Guid.NewGuid());
                                if (!termIdMapping.ContainsKey(termId))
                                {
                                    termIdMapping.Add(termId, term.ID);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCAddtermError, e);
                            }//不需要处理该异常
                        }
                        if (term != null)
                        {
                            IAveTaxonomyFieldValue taxonomyValue = field.TaxonomyFieldValue;
                            string labelGuidPair = term.GetDefaultLabel(termStore.DefaultLanguage) + "|" + term.ID.ToString();
                            taxonomyValue.PopulateFromLabelGuidPair(labelGuidPair);
                            if (taxonomyValues == null)
                            {
                                taxonomyValues = field.TaxonomyFieldValueCollection;
                            }
                            taxonomyValues.Add(taxonomyValue);
                            if (!field.AllowMultipleValues)
                            {
                                break;
                            }
                        }
                    }
                }
                if (taxonomyValues != null && taxonomyValues.Count > 0)
                {
                    if (field.AllowMultipleValues)
                    {
                        field.DefaultValue = field.GetValidatedString(taxonomyValues);
                    }
                    else
                    {
                        field.DefaultValue = field.GetValidatedString(taxonomyValues[0]);
                    }
                }
                else
                {
                    field.DefaultValue = "";
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCResetTaxnomyFieldDefValueError, e);
                field.DefaultValue = "";
            }
            return true;
        }
        //此方法通过反编译TaxonomyItem.NormalizeName(string name)方法得到，去check并得到新的termName.
        public static string NormalizeName(string termName)
        {
            if (termName == null)
            {
                return null;
            }
            Regex trimSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            char tempChar = (char)0xff06;
            return trimSpacesRegex.Replace(termName, " ").Replace('&', tempChar).Trim();

        }
    }

    public class TaxonomyFieldCreationInformation
    {
        public string DisplayName = string.Empty;
        public bool AllowMultipleValues = true;
    }

    internal class TaxonomyContext
    {
        public Guid SspId { get; set; }
        public Guid GroupId { get; set; }
        public Guid TermSetId { get; set; }
        public Guid AnchorId { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}][{2}][{3}]", SspId, GroupId, TermSetId, AnchorId);
        }
    }
}