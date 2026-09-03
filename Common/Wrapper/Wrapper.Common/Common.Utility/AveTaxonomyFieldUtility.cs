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
using System.Text.RegularExpressions;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Contract.CodeReview;
using System.Linq;

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
                return field;
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
                return field;
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
                throw new Exception("term set " + termSetName + " does not exist");
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
                fieldInternalName = web?.Fields.Add(taxonomyField).InternalName;
            }
            else
            {
                fieldInternalName = list?.Fields.Add(taxonomyField).InternalName;
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
            SetFieldValue(item, dic, true, LCID, null);
        }

        public static void SetFieldValue(IAveListItem item, Dictionary<string, string> dic, int LCID, Dictionary<Guid, Guid> termIdMapping)
        {
            SetFieldValue(item, dic, true, LCID, termIdMapping);
        }

        public static void SetFieldValue(IAveListItem item, Dictionary<string, string> dic, bool ForceAddTerm, int LCID, Dictionary<Guid, Guid> termIdMapping)
        {
            foreach (string fieldName in dic.Keys)
            {
                IAveField field = item.ParentList.Fields.GetField(fieldName);
                IAveTaxonomyField tField = field as IAveTaxonomyField;
                IAveTaxonomySession session = (item.ParentList.ParentWeb.Site).AveSPTaxonomySession;
                IAveTermStore termStore = null;
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
                        continue;
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
                string[] termHiberarchy = null;
                //TaxonomyFieldValueCollection values = item[fieldName] as TaxonomyFieldValueCollection;
                List<IAveTerm> terms = new List<IAveTerm>();
                foreach (string termName in termNames)
                {
                    if (string.IsNullOrEmpty(termName))
                    {
                        continue;
                    }
                    IAveTerm term = null;
                    termHiberarchy = null;
                    string tName = termName.StartsWith("#", StringComparison.Ordinal) ? termName.Substring(1) : termName;
                    try
                    {
                        if (tName.Contains("|"))
                        {
                            try
                            {
                                Guid tTermId = Guid.Empty;
                                string[] temp = tName.Split('|');
                                if (temp.Length == 2)
                                {
                                    tName = temp[0];
                                    tTermId = new Guid(temp[1]);
                                    if (termIdMapping != null && termIdMapping.ContainsKey(tTermId))
                                    {
                                        tTermId = termIdMapping[tTermId];
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
                                }
                            }
                            catch(Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByIdError, e.ToString());
                            }
                        }
                        //'<'表示term的层次关系。
                        else if (tName.Contains("<"))
                        {
                            termHiberarchy = tName.Split('<');
                            term = termSet?.Terms[termHiberarchy[0]];
                            for (int i = 1; i < termHiberarchy.Length; i++)
                            {
                                if (string.IsNullOrEmpty(termHiberarchy[i]))
                                {
                                    continue;
                                }
                                term = term.Terms[NormalizeName(termHiberarchy[i])];
                            }
                        }
                        //else
                        //{
                        //    continue;
                        //}
                        if (term == null && termSet != null)
                        {
                            try
                            {
                                if (endTerm == null)
                                {
                                    term = termSet.Terms[NormalizeName(tName)];
                                }
                                else
                                {
                                    term = endTerm.Terms[NormalizeName(tName)];
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, e.ToString());
                                //DOC-78396 使用此方法刷新对象
                                IAveTermCollection ts = termSet.GetTerms(NormalizeName(tName).Trim(), true);
                                if (endTerm == null)
                                {
                                    term = termSet.Terms[NormalizeName(tName)];
                                }
                                else
                                {
                                    term = endTerm.Terms[NormalizeName(tName)];
                                }
                            }
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        if (ForceAddTerm && termSet != null)
                        {
                            if (termHiberarchy != null && termHiberarchy.Length > 0)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(termHiberarchy[0]))
                                    {
                                        continue;
                                    }
                                    term = termSet.Terms[NormalizeName(termHiberarchy[0])];
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, e.ToString());
                                    //DOC-78396
                                    try
                                    {
                                        term = termSet.CreateTerm(NormalizeName(termHiberarchy[0]).Trim(), LCID, Guid.NewGuid());
                                        termSet.TermStore.CommitAll();
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCCreateTermError, ex.ToString());
                                        //DOC-78396 使用此方法刷新对象
                                        IAveTermCollection ts = termSet.GetTerms(NormalizeName(termHiberarchy[0]).Trim(), true);
                                        if (endTerm == null)
                                        {
                                            term = termSet.Terms[NormalizeName(termHiberarchy[0]).Trim()];
                                        }
                                        else 
                                        {
                                            term = endTerm.Terms[NormalizeName(termHiberarchy[0]).Trim()];
                                        }
                                    }
                                }
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
                                    catch(Exception e)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, e.ToString());
                                        term = term.CreateTerm(NormalizeName(termHiberarchy[i]).Trim(), LCID, Guid.NewGuid());
                                        term.TermStore.CommitAll();
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
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
                        }
                    }
                    if (term != null)
                    {
                        terms.Add(term);
                        //如果field不允许多值，没有必要找多个term了。
                        if (!tField.AllowMultipleValues)
                        {
                            break;
                        }
                    }
                    //TaxonomyFieldValue value = new TaxonomyFieldValue(field);
                    //value.TermGuid = myTerm.Id.ToString();
                    //value.Label = myTerm.Name;
                    //values.Add(value);
                }
                if (submit)
                {
                    try
                    {
                        termStore.CommitAll();
                        submit = false;
                    }
                    catch(Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCClearTermError, e.ToString());
                        terms.Clear();
                        foreach (string termName in termNames)
                        {
                            if (string.IsNullOrEmpty(termName))
                            {
                                continue;
                            }
                            try
                            {
                                //DOC-78396 使用此方法刷新对象
                                IAveTermCollection ts = termSet.GetTerms(NormalizeName(termName).Trim(), true);
                                if (endTerm == null)
                                {
                                    terms.Add(termSet.Terms[NormalizeName(termHiberarchy[0]).Trim()]);
                                }
                                else
                                {
                                    terms.Add(endTerm.Terms[NormalizeName(termHiberarchy[0]).Trim()]);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, ex.ToString());
                            }
                        }
                    }
                }
                if (tField.AllowMultipleValues)
                {
                    IAveTaxonomyFieldValueCollection taxValueCollection = tField.TaxonomyFieldValueCollection;
                    //new TaxonomyFieldValueCollection(tField);
                    foreach (IAveTerm tTerm in terms)
                    {
                        if (tTerm != null)
                        {
                            int effectiveLcid = LCID;
                            string text = tTerm.GetDefaultLabel(effectiveLcid) + "|" + tTerm.ID;
                            IAveTaxonomyFieldValue value2 = tField.TaxonomyFieldValue;
                            //new TaxonomyFieldValue(tField);
                            value2.PopulateFromLabelGuidPair(text);
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
                        string text = terms[0].GetDefaultLabel(effectiveLcid) + "|" + terms[0].ID;
                        IAveTaxonomyFieldValue taxValue = tField.TaxonomyFieldValue;
                        //new TaxonomyFieldValue(tField);
                        taxValue.PopulateFromLabelGuidPair(text);
                        item[tField.ID] = taxValue;
                        item[tField.TextField] = taxValue.ToString();
                    }
                }
            }
        }

        public static void UpdateTaxonomyValue(IAveListItem item, Dictionary<string, string> dic, bool ForceAddTerm, int LCID, bool increaseVersion, Dictionary<Guid, Guid> termIdMapping)
        {
            SetFieldValue(item, dic, ForceAddTerm, LCID, termIdMapping);

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
            return Guid.Empty;
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
                        catch(Exception e)
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
            catch(Exception e)//no exception should be thrown.(Luo Qinglong)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermGroupNameError, e.ToString());
                return string.Empty;
            }
            return string.Empty;
        }

        public static Guid GetTermGroupId(IAveSite site, ref Guid termStoreId, string groupName)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                if (termStoreId != Guid.Empty)
                {
                    IAveTermStore termStore = session.TermStores[termStoreId];
                    foreach (IAveTaxonomyGroup tGroup in termStore.Groups)
                    {
                        if (tGroup.Name == groupName)
                        {
                            return tGroup.ID;
                        }
                    }
                }
                else
                {
                    foreach (IAveTermStore tTermStore in session.TermStores)
                    {
                        foreach (IAveTaxonomyGroup tGroup in tTermStore.Groups)
                        {
                            if (tGroup.Name == groupName)
                            {
                                termStoreId = tGroup.TermStore.ID;
                                return tGroup.ID;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermGroupIdError, e.ToString());
                return Guid.Empty;
            }
            return Guid.Empty;
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
                        IAveTermSet termSet = termStore.GetTermSet(termSetId);
                        if (termSet == null)
                        {
                            log.Info($"GetTermSetName GetTermSet:{termSetId} null.TermStoreId:{termStoreId}.TermStoreGroupsCount:{termStore.Groups.Count}.Can not find the termSet.");
                            //return ProcessTermStore(site, termStore, termSetId, ref termStoreId, ref groupId, false);
                            return string.Empty;
                        }
                        else
                        {
                            return termSet.Name;
                        }
                    }
                    else
                    {
                        var tTermStore = session.TermStores.FirstOrDefault();
                        log.Info($"GetTermSetName TermStore is null:{termStoreId}. FirstOrDefault TermStoreId:{tTermStore.ID}.NewTermStoreGroupsCount:{tTermStore.Groups.Count}.");
                        IAveTermSet termSet = tTermStore.GetTermSet(termSetId);
                        if (termSet == null)
                        {
                            log.Info($"GetTermSetName GetTermSet:{termSetId} null.FirstOrDefault.TermStoreId:{tTermStore.ID}. Can not find the termSet.");
                            //return ProcessTermStore(site, tTermStore, termSetId, ref termStoreId, ref groupId, true);
                            return string.Empty;
                        }
                        else
                        {
                            return termSet.Name;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
            }//no need to log
            return string.Empty;
        }


        private static string ProcessTermStore(IAveSite site, IAveTermStore termStore, Guid termSetId, ref Guid termStoreId, ref Guid groupId, bool needGiveValue)
        {
            foreach (IAveTaxonomyGroup tGroup in termStore.Groups)
            {
                try
                {
                    if (tGroup.IsSiteCollectionGroup && !tGroup.SiteCollectionAccessIds.Contains(site.ID))
                    {
                        continue;
                    }
                    IAveTermSet termSet = tGroup.TermSets[termSetId];
                    if (termSet != null)
                    {
                        termStoreId = needGiveValue ? termSet.TermStore.ID : termStoreId;
                        groupId = tGroup.ID;
                        return termSet.Name;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetNameError, e.ToString());
                }
            }
            return string.Empty;
        }
       

        public static Guid GetTermSetId(IAveSite site, ref Guid termStoreId, ref Guid groupId, string termSetName)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = null;
                IAveTaxonomyGroup group = null;
                IAveTermSet termSet = null;
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
                        termSet = group.TermSets[termSetName];
                        return termSet.ID;
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
                    }
                    IAveTermSetCollection termSets = session.GetTermSets(termSetName, session.TermStores[0].DefaultLanguage);
                    log.Info("get term sets count:{0} with store id:{1} group id:{2}, term set name:{3}", termSets.Count, termStoreId, groupId, termSetName);
                    if (termStoreId != Guid.Empty)
                    {
                        foreach (IAveTermSet tTermSet in termSets)
                        {
                            if (tTermSet.TermStore.ID == termStoreId)
                            {
                                if (termStore != null && tTermSet.Group != null && tTermSet.Group.IsSiteCollectionGroup && termStore.GetTermSet(tTermSet.ID) == null) // && !tTermSet.Group.SiteCollectionAccessIds.Contains(site.ID))   client API doesn't support "SiteCollectionAccessIds", so we need to check if the termSet is available
                                {
                                    continue;
                                }
                                return tTermSet.ID;
                            }
                        }
                    }
                    else
                    {
                        if (termSets.Count > 0)
                        {
                            for (int i = 0; i < termSets.Count; i++)
                            {
                                if (termStore != null && termSets[i].Group != null && termSets[i].Group.IsSiteCollectionGroup && termStore.GetTermSet(termSets[i].ID) == null) // && !tTermSet.Group.SiteCollectionAccessIds.Contains(site.ID))   client API doesn't support "SiteCollectionAccessIds", so we need to check if the termSet is available
                                {
                                    continue;
                                }
                                if (termStoreId == Guid.Empty)
                                {
                                    termStoreId = termSets[i].TermStore.ID;
                                }
                                return termSets[i].ID;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetGroupIdError, e.ToString());
            }
            return Guid.Empty;
        }

        public static string GetTermName(IAveSite site, Guid termStoreId, Guid termSetId, Guid termId)
        {
            try
            {
                IAveTaxonomySession session = GetTaxonomySession(site);
                IAveTermStore termStore = session.TermStores[termStoreId];
                IAveTerm term = termStore.GetTerm(termSetId, termId);
                return term.Name;
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
            catch(Exception e)
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
                        string termName = values[i].Substring(1, values[i].IndexOf('|')).TrimEnd('|');
                        string strTermId = values[i].Substring(values[i].IndexOf('|') + 1);
                        Guid termId = new Guid(strTermId);
                        if (termIdMapping != null && termIdMapping.ContainsKey(termId))
                        {
                            termId = termIdMapping[termId];
                        }
                        try
                        {
                            term = termSet.GetTerm(termId);
                            if (term == null)
                            {
                                term = termSet.Terms[termName];
                            }
                        }
                        catch(Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByIdError, e.ToString());
                            try
                            {
                                term = termSet.Terms[termName];
                            }
                            catch(Exception ex)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermSetByNameError, ex.ToString());
                            }
                        }
                        if (term == null)
                        {	//由于RestoreUsedTermOnly的存在，不能保证在reset时目的端一定存在需要用到的term，不存在则创建。
                            try
                            {
                                term = termSet.CreateTerm(termName, termStore.DefaultLanguage, new Guid());
                                if (termIdMapping != null && !termIdMapping.ContainsKey(termId))
                                {
                                    termIdMapping?.Add(termId, term.ID);
                                }
                            }
                            catch(Exception e) 
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCAddtermError, e.ToString());
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
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCResetTaxnomyFieldDefValueError, e.ToString());
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
            return trimSpacesRegex.Replace(termName, " ").Replace('&', tempChar);

        }

        #region add for RevIM term path
        public static IAveTermStore GetTermStore(IAveTaxonomyField field, IAveTaxonomySession session, ref int LCID)
        {
            IAveTermStore termStore = null;
            Guid sspId = Guid.Empty;
            if (field.SspId == Guid.Empty && !field.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
            {
                object customProperty = field.GetCustomProperty("SspId");
                if (customProperty != null)
                {
                    sspId = new Guid(customProperty.ToString());
                }
            }
            else
            {
                sspId = field.SspId;
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
        #endregion
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
            catch (ArgumentOutOfRangeException e)
            {
                log.Warn(@$"fail find term,ex:{e}");
            }
            //TryGetTermByName 经过修改ADO-172523，不抛异常，将创建逻辑拿到外面
            if (term == null && forceAddTerm && termSet != null && !String.IsNullOrEmpty(tName))
            {
                term = CreateNotExistTerm(termSet, endTerm, tName, LCID, ref submit);
                log.Debug("Force Add Term. Term Name:{0}", tName);
            }
            return term;
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
            var res = term.Terms.Where(subTerm => subTerm.Name.Equals(tName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            return res ?? term.Terms.Select(subTerm => GetTermInTerm(tName, subTerm)).FirstOrDefault(result => result != null);
        }
    }

    public class TaxonomyFieldCreationInformation
    {
        public string DisplayName = string.Empty;
        public bool AllowMultipleValues = true;
    }
}