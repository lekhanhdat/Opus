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

#region using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using AvePoint.Media.Storage.Util;
using System.Reflection;
using System.IO;
using AvePoint.GCommon;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
#endregion

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Inner.XConfiguration.#.cctor()", MessageId = "Stor")]
namespace AvePoint.Media.Storage.Inner
{
    /// <summary>
    /// 只是为了利用配置文件实现storage device的热插拔
    /// </summary>
    public class XConfiguration
    {
        /// <summary>
        /// 这里存储的是vim_name, vim class的键值对.
        /// </summary>
        /// 
        Dictionary<string, VIMInfo> vims = new Dictionary<string, VIMInfo>();
        bool loaded = false;
        private AveLogger logger = AveLogger.GetInstance(typeof(XConfiguration));

        public Dictionary<string, VIMInfo> Vims { get { return vims; } }

        public bool Loaded { get { return loaded; } }

        private static readonly string CFG_XML_IF_LOAD_FAILED = 
            @"<VIMS>" +
            "<VIM name=\"fs_vim\" assembly=\"StorageFS\" type=\"AvePoint.Media.Storage.FS.FSVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"ibm_vim\" assembly=\"StorageFS\" type=\"AvePoint.Media.Storage.FS.FSVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"nfs_vim\" assembly=\"StorageFS\" type=\"AvePoint.Media.Storage.FS.FSVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"ftp_vim\" assembly=\"StorageFTP\" type=\"AvePoint.Media.Storage.FTP.FTPVIM\" />" +
            "<VIM name=\"tsm_vim\" assembly=\"StorageTSM\" type=\"AvePoint.Media.Storage.TSM.TSMVIM\" />" +
            "<VIM name=\"centera_vim\" assembly=\"StorageCentera\" type=\"AvePoint.Media.Storage.Centera.CenteraVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"rackspace_vim\" assembly=\"StorageCloudRackspace\" type=\"AvePoint.Media.Storage.Cloud.Rackspace.RackspaceVIM\" />" +
            "<VIM name=\"azure_vim\" assembly=\"StorageCloudAzure\" type=\"AvePoint.Media.Storage.Cloud.Azure.AzureVIM\" />" +
            "<VIM name=\"amazon_vim\" assembly=\"StorageCloudAmazon\" type=\"AvePoint.Media.Storage.Cloud.Amazon.AmazonVIM\" />" +
            "<VIM name=\"s3compatible_vim\" assembly=\"StorageCloudS3Compatible\" type=\"AvePoint.Media.Storage.S3Compatible.S3CompatibleVIM\" />" +
            "<VIM name=\"atmos_vim\" assembly=\"StorageCloudAtmos\" type=\"AvePoint.Media.Storage.Cloud.Atmos.AtmosVIM\" />" +
            "<VIM name=\"dropbox_vim\" assembly=\"StorageCloudDropbox\" type=\"AvePoint.Media.Storage.Cloud.Dropbox.DropboxVIM\" />" +
            "<VIM name=\"castor_vim\" assembly=\"StorageCAStor\" type=\"AvePoint.Media.Storage.CAStor.CAStorVIM\" IsCacheSpaceInfo =\"false\"  IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"hcp_vim\" assembly=\"StorageHCP\" type=\"AvePoint.Media.Storage.HCP.HCPVIM\" IsCacheSpaceInfo =\"false\"  IsCheckFreeSpace =\"true\"/>"+
            "<VIM name=\"" + "mirrorFS_vim".ToLower(CultureInfo.InvariantCulture) + "\" assembly=\"StorageMirrorFS\" type=\"AvePoint.Media.Storage.MirrorFS.MirrorFSVIM\" />" + 
            "<VIM name=\"netapp_lun_vim\" assembly=\"StorageNetApp\" type=\"AvePoint.Media.Storage.NetApp.NetAppVIM\"/>" +
            "<VIM name=\"netapp_cifs_vim\" assembly=\"StorageNetApp\" type=\"AvePoint.Media.Storage.NetApp.NetAppVIM\"/>" +
            "<VIM name=\"netapp_nfs_vim\" assembly=\"StorageNetApp\" type=\"AvePoint.Media.Storage.NetApp.NetAppVIM\"/>" +
            "<VIM name=\"caringo_vim\" assembly=\"StorageCAStor\" type=\"AvePoint.Media.Storage.CAStor.CAStorVIM\"/>" +
            "<VIM name=\"egnyte_vim\" assembly=\"StorageEgnyte\" type=\"AvePoint.Media.Storage.Egnyte.EgnyteVIM\" />" +
            "<VIM name=\"box_vim\" assembly=\"StorageBox\" type=\"AvePoint.Media.Storage.Box.BoxVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"skydrive_vim\" assembly=\"StorageSkyDrive\" type=\"AvePoint.Media.Storage.OneDrive.OneDriveVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"googledrive_vim\" assembly=\"StorageGoogleDrive\" type=\"AvePoint.Media.Storage.GoogleDrive.GoogleDriveVIM\" IsCacheSpaceInfo =\"false\" IsCheckFreeSpace =\"true\"/>" +
            "<VIM name=\"objectatmos_vim\" assembly=\"StorageCloudObjectAtmos\" type=\"AvePoint.Media.Storage.Cloud.ObjectAtmos.ObjectAtmosVIM\" />" +
            "<VIM name=\"cleversafe_vim\" assembly=\"StorageCloudCleversafe\" type=\"AvePoint.Media.Storage.Cloud.Cleversafe.CleversafeVIM\" />" +
            "</VIMS>";
        /// <summary>
        /// Loads the specified vim configuration file.
        /// </summary>
        /// <param name="cfgFile">etc\storage_cfg.xml</param>
        public void load(string cfgFile)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                string cfgFileFullPath = null;
                try
                {
                    string folder = ExecutorContext.BinDirectory;
                    cfgFileFullPath = Path.Combine(folder, cfgFile);
                    xmlDoc.Load(cfgFileFullPath);
                }
                catch(Exception t)
                {
                    logger.Error("Load Storage Configuration file Failed.[Full Path]:" + cfgFileFullPath + ", " + t.Message + ". Load from hard code again.", t);
                    xmlDoc.LoadXml(CFG_XML_IF_LOAD_FAILED);
                }
                
                XmlNodeList nodes = xmlDoc.GetElementsByTagName("VIM");
                VIMInfo vim = null;
                foreach (XmlNode vimElement in nodes)
                {
                    vim = new VIMInfo();
                    vim.Name = vimElement.Attributes["name"].Value;
                    vim.DllFile = vimElement.Attributes["assembly"].Value;
                    vim.Type = vimElement.Attributes["type"].Value;
                    if (vimElement.Attributes.GetNamedItem("IsCacheSpaceInfo") != null)
                    {
                        vim.IsCacheSpaceInfo = bool.Parse(vimElement.Attributes["IsCacheSpaceInfo"].Value);
                    }
                    if (vimElement.Attributes.GetNamedItem("IsCheckFreeSpace") != null)
                    {
                        vim.IsCheckFreeSpace = bool.Parse(vimElement.Attributes["IsCheckFreeSpace"].Value);
                    }
                    if (vimElement.Attributes.GetNamedItem("CheckFreeSpaceIntervalTime") != null)
                    {
                        vim.CheckFreeSpaceIntervalTime = int.Parse(vimElement.Attributes["CheckFreeSpaceIntervalTime"].Value);
                    }
                    vims.Add(vim.Name, vim);
                }
                this.loaded = true;
            }
            catch (Exception e)
            {
                throw new VIMLoadException("Error : Load Vim from config file : " + cfgFile + " Failed.", e);
            }
        }

        /// <summary>
        /// Gets the VIM Info from vims.
        /// </summary>
        /// <param name="name">The vim name, e.g. fs_vim, ftp_vim, etc.</param>
        /// <returns></returns>
        public VIMInfo GetVIMInfo(string name)
        {
            if (loaded)
            {
                if (vims.ContainsKey(name))
                {
                    return vims[name];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new XException("Error: Unloaded Storage Config XML.");
            }
        }
    }

    public class VIMInfo
    {
        public string Name { get; set; }
        public string DllFile { get; set; }
        public string Type { get; set; }

        public bool IsCacheSpaceInfo { get; set; }
        public bool IsCheckFreeSpace { get; set; }

        private int checkFreeSpaceIntervalTime = 1000 * 60 * 60; //1小时
        public int CheckFreeSpaceIntervalTime
        {
            get { return checkFreeSpaceIntervalTime; }
            set { checkFreeSpaceIntervalTime = value; }
        }
    }
}
