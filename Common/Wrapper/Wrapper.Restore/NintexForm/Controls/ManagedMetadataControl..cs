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
using System.Xml;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.Wrapper.Restore.NintexForm.Online
{
    class ManagedMetadataControl : BaseControl
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAveTermStore termStore;
        private IAveTaxonomyGroup termGroup;
        private IAveTermSet termSet;
        public ManagedMetadataControl(AveSPWeb web, IAveList list, string contentTypeId, XmlNode controlNode, XmlNamespaceManager nsManager) :
            base(web, list, contentTypeId, controlNode, nsManager)
        {
        }
        public override void ProcessControl(bool isPost)
        {
            log.Warn("Start ensure metadata control.");
            if (!EnsureTermStore(isPost) ||
                !EnsureGroup(isPost) ||
                !EnsureTermSet(isPost) ||
                !EnsureTerm(isPost))
            {
                return;
            }
            UpdateControlTypeUniqueId();
        }
        private bool EnsureTermStore(bool isPost)
        {
            XmlNode termStoreNode;
            var termStoreId = TryGetNodeId("d3p1:TermStoreId", out termStoreNode);
            if (termStoreId != Guid.Empty)
            {
                if (mWeb.ParentSite.MetadataService.TermStoreIdMapping.ContainsKey(termStoreId))
                {
                    var destinationTermStoreId = mWeb.ParentSite.MetadataService.TermStoreIdMapping[termStoreId];
                    termStore = mWeb.ParentSite.SPSite.AveSPTaxonomySession.TermStores[destinationTermStoreId];
                    termStoreNode.InnerText = destinationTermStoreId.ToString();
                    return true;
                }
                else if (isPost)
                {
                    try
                    {
                        termStore = mWeb.ParentSite.SPSite.AveSPTaxonomySession.TermStores[termStoreId];
                    }
                    catch (Exception e)
                    {
                        log.Error("Can not find this term store during restore nintex form. TermStore Id: {0}, Error: {1}", termStoreId, e);
                    }
                    if (termStore == null)
                    {
                        throw new AveNintexFormPostException("TermStore", termStoreId.ToString(), contentTypeId);
                    }
                    return true;
                }
                else
                {
                    throw new AveNintexFormPostException("TermStore", termStoreId.ToString(), contentTypeId);
                }
            }
            else
            {
                log.Warn("Term store id is empty. ContentType Id : {0}", contentTypeId);
                return false;
            }
        }
        private bool EnsureGroup(bool isPost)
        {
            XmlNode termGroupNode;
            var termGroupId = TryGetNodeId("d3p1:GroupId", out termGroupNode);
            if (termGroupId != Guid.Empty)
            {
                if (mWeb.ParentSite.MetadataService.TermGroupIdMapping.ContainsKey(termGroupId))
                {
                    var destinationTermGroupId = mWeb.ParentSite.MetadataService.TermGroupIdMapping[termGroupId];
                    var newNode = mControlNode.OwnerDocument.CreateElement(termGroupNode.Prefix, "TermGroupId", termGroupNode.NamespaceURI);
                    newNode.InnerText = destinationTermGroupId.ToString();
                    termGroup = termStore.Groups[destinationTermGroupId];
                    mControlNode.RemoveChild(termGroupNode);
                    mControlNode.AppendChild(newNode);
                    return true;
                }
                else if (isPost)
                {
                    try
                    {
                        termGroup = termStore.Groups[termGroupId];
                    }
                    catch (Exception e)
                    {
                        log.Error("Can not find this term group during restore nintex form. TermStore Id: {0}, Error: {1}", termGroupId, e);
                    }
                    if (termGroup == null)
                    {
                        throw new AveNintexFormPostException("TermGroup", termGroupId.ToString(), contentTypeId);
                    }
                    return true;
                }
                else
                {
                    throw new AveNintexFormPostException("TermGroup", termGroupId.ToString(), contentTypeId);
                }
            }
            else
            {
                log.Warn("Term group id is empty. ContentType Id : {0}", contentTypeId);
                return false;
            }
        }
        private bool EnsureTermSet(bool isPost)
        {
            XmlNode termSetNode;
            var termSetId = TryGetNodeId("d3p1:TermSetId", out termSetNode);
            if (termSetId != Guid.Empty)
            {
                if (mWeb.ParentSite.MetadataService.TermSetIdMapping.ContainsKey(termSetId))
                {
                    var destinationTermSetId = mWeb.ParentSite.MetadataService.TermSetIdMapping[termSetId];
                    termSet = termGroup.TermSets[destinationTermSetId];
                    termSetNode.InnerText = destinationTermSetId.ToString();
                    return true;
                }
                else if (isPost)
                {
                    try
                    {
                        termSet = termGroup.TermSets[termSetId];
                    }
                    catch (Exception e)
                    {
                        log.Error("Can not find this term set during restore nintex form. TermSet Id: {0}, Error: {1}", termSetId, e);
                    }
                    if (termSet == null)
                    {
                        throw new AveNintexFormPostException("TermSet", termSetId.ToString(), contentTypeId);
                    }
                    return true;
                }
                else
                {
                    throw new AveNintexFormPostException("TermSet", termSetId.ToString(), contentTypeId);
                }
            }
            else
            {
                log.Warn("Term set id is empty. ContentType Id : {0}", contentTypeId);
                return false;
            }
        }
        private bool EnsureTerm(bool isPost)
        {
            XmlNode termNode;
            var termId = TryGetNodeId("d3p1:AnchorId", out termNode);
            if (termId != Guid.Empty)
            {
                if (mWeb.ParentSite.MetadataService.TermIdMapping.ContainsKey(termId))
                {
                    var destinationTermId = mWeb.ParentSite.MetadataService.TermIdMapping[termId];
                    termNode.InnerText = destinationTermId.ToString();
                }
                else if (isPost)
                {
                    try
                    {
                        var term = termSet.GetTerm(termId);
                    }
                    catch (Exception e)
                    {
                        log.Error("Can not find this term during restore nintex form. Term Id: {0}, Error: {1}", termId, e);
                    }
                    return true;
                }
                else
                {
                    throw new AveNintexFormPostException("Term", termId.ToString(), contentTypeId);
                }
            }
            return true;
        }
        private void UpdateControlTypeUniqueId()
        {
            XmlNode uniqueControlIdNode;
            var controlId = TryGetNodeId("d2p1:FormControlTypeUniqueId", out uniqueControlIdNode);
            if(controlId == new Guid("b612705d-96ee-4824-90e2-4f37ee78a36c"))
            {
                uniqueControlIdNode.InnerText = "4eac00c4-29da-43ec-b444-a102dfb20b68";
            }
        }
        private Guid TryGetNodeId(string nodePath, out XmlNode node)
        {
            node = GetPropertyNode(nodePath);
            var nodeString = node == null ? string.Empty : node.InnerText;
            if (AveTypeHelper.IsGuid(nodeString))
            {
                return new Guid(nodeString);
            }
            return Guid.Empty;
        }
        public override void AddControlNameSpace()
        {
            nsManager.AddNamespace("d3p1", "http://schemas.datacontract.org/2004/07/Nintex.Forms.SharePoint.FormControls._properties");
        }
    }
}
