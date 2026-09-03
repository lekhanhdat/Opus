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
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Restore
{
    /*
    /// <summary>
    /// Backup Content Type from Content Type Hub
    /// </summary>
    public class AveSPContentType
    {

        AveSPContentTypeHub parentHub;
        string contentTypeName;


        public AveSPContentType(AveSPContentTypeHub hub, string name)
        {
            this.contentTypeName = name;
            this.parentHub = hub;
        }

        public void Export(IAveBackupStream output)
        {
            AveContentTypeInfo info = GetContentTypeInfo();
            info.IsPublished = this.parentHub.Application.IsPublished(info.Id);
            if (!info.IsPublished)
            {
                info.IsUnPublished = this.parentHub.Application.IsUnPublished(info.Id);
            }
            output.WriteMetadata(AveMetadataType.ContentTypeHub, info);
        }

        public AveContentTypeInfo GetContentTypeInfo()
        {
            IAveContentType ct = this.parentHub.Site.RootWeb.ContentTypes[this.contentTypeName];
            bool backUpResourceFolder = true;

            AveContentTypeInfo ctInfo = new AveContentTypeInfo();
            ctInfo.Name = ct.Name;
            ctInfo.Id = ct.ID.ToString();
            try
            {
                ctInfo.ReadOnly = ct.ReadOnly;
                ctInfo.Description = ct.Description;
                ctInfo.FieldsSchemaXml = ct.Fields.SchemaXml;
                ctInfo.DocumentTemplate = ct.DocumentTemplate;
                ctInfo.Group = ct.Group;
                ctInfo.DisplayFormTemplateName = ct.DisplayFormTemplateName;
                ctInfo.DisplayFormUrl = ct.DisplayFormUrl;
                ctInfo.DocumentTemplateUrl = ct.DocumentTemplateUrl;
                ctInfo.EditFormTemplateName = ct.EditFormTemplateName;
                ctInfo.EditFormUrl = ct.EditFormUrl;
                ctInfo.Hidden = ct.Hidden;
                //ctInfo.NewDocumentControl = ct.NewDocumentControl;
                ctInfo.NewFormTemplateName = ct.NewFormTemplateName;
                ctInfo.NewFormUrl = ct.NewFormUrl;
                //ctInfo.RequireClientRenderingOnNew = ct.RequireClientRenderingOnNew;
                try
                {
                    ctInfo.ResourceFolder = ct.ResourceFolder != null ? ct.ResourceFolder.Url : null;
                }
                catch
                {
                    backUpResourceFolder = false;
                }
                ctInfo.SchemaXml = ct.SchemaXml;
                ctInfo.Scope = ct.Scope;
                ctInfo.Sealed = ct.Sealed;
                //ctInfo.Version = ct.Version;
                ctInfo.ParentName = ct.Parent.Name;

                try
                {
                    if (backUpResourceFolder)
                    {
                        foreach (IAveFile temFile in ct.ResourceFolder.Files)
                        {
                            ctInfo.ResourceFolderFiles.Add(new AveContentTypeFileInfo(temFile.Url, temFile.OpenBinary()));
                        }
                    }
                }
                catch (Exception e)
                {
                    // mLog.Log(AveLogLevel.WARN, "WP10BKAveSPCT351", ctInfo.Id, ctInfo.Name, e);
                }

                foreach (string str in ct.XmlDocuments)
                {
                    ctInfo.XmlDocuments.Add(str);
                }
            }
            catch (Exception e)
            {
                // mLog.Log(AveLogLevel.WARN, "WP10BKAveSPCT379", ctInfo.Id, ctInfo.Name, e);
            }
            return ctInfo;
        }

    }
     */

    public class AveSPContentTypeHub : RestoreableObject,IDisposable
    {
        protected static AveLogger mLog = AveLogger.GetInstance(typeof(AveSPContentTypeHub));
        IAveSite mSPSite;
        IAveMetadataServiceApplication mSPServiceApplication;
        AveObjectModelFactory mObjectFactory;
        bool needDispose;
        public  List<string> mIncludeCententTypes = new List<string>();

        public List<string> IncludeCententTypes
        {
            get
            {
                return this.mIncludeCententTypes;
            }
            set{mIncludeCententTypes =value;}
        }

        public IAveSite SPSite
        {
            get
            {
                if (this.mSPSite == null)
                {
                    Uri url = this.mSPServiceApplication.GetContentTypeSyndicationHubLocal();
                    if (url != null)
                    {
                        this.mSPSite = this.mObjectFactory.CreateSite(url.ToString());
                    }
                }
                return this.mSPSite;
            }
        }

        public IAveMetadataServiceApplication SPServiceApplication
        {
            get
            {
                return this.mSPServiceApplication;
            }
        }

        public AveSPContentTypeHub(AveObjectModelFactory fac, IAveMetadataServiceApplication application)
        {
            this.mObjectFactory = fac;
            this.mSPServiceApplication = application;
        }

        public AveSPContentTypeHub(AveObjectModelFactory fac, Guid applicationId)
            : this(fac, fac.CreateMetadataServiceApplication(applicationId))
        {
            needDispose = true;
        }

        //public void InitCTHubContentTypeCollection(AveSPContentTypeCollection ct, List<string> CTNames)
        //{
        //    try
        //    {
        //        foreach(IAveContentType cttemp in  mSPSite.RootWeb.ContentTypes)
        //        {
        //            if(CTNames.Contains(cttemp.Name))
        //            {
        //                ct.SPContentTypeCollection.Add(cttemp);
        //            }
        //        }
        //    }
        //    catch
        //    { }
        //}

        public void Dispose()
        {
            if (mSPSite != null)
            {
                mSPSite.Dispose();
            }
            if (needDispose && mSPServiceApplication != null)
            {
                mSPServiceApplication.Dispose();
            }
        }
    }

}
