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




namespace AvePoint.Media.ClassicStorage.Inner
{
    #region using directives
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// VIM 用于获取直接面向存储介质的API的对象, 以实现上层逻辑与真实介质的链接, 只应该在具体存储介质层面实现, 例如FSVIM。
    /// </summary>
    public interface IVIM
    {
        /// <summary>
        /// create a  system
        /// </summary>
        /// <param name="xri"><paramref name="XRI">XRI</paramref></param>
        /// <param name="parentSystem">the parent system</param>
        /// <returns></returns>
        IXSystemCommon CreateSystem(string xri, AbstractXSystem parentSystem);

        List<string> GetFeatureXML(int type);

        List<StorageFeature> GetFeatureObj(int type);

        List<StorageFeature> GetFeatureObj(int type, string culture);

    }

    public abstract class AbstractVIM : IVIM
    {
        public virtual IXSystemCommon CreateSystem(string xri, AbstractXSystem parentSystem)
        {
            throw new NotImplementedException("Not Implemented In this layer.");
        }

        public virtual List<string> GetFeatureXML(int type)
        {
            throw new NotImplementedException("Not Implemented In this layer.");
        }

        public virtual List<StorageFeature> GetFeatureObj(int type)
        {
            throw new NotImplementedException("Not Implemented In this layer.");
        }

        public virtual List<StorageFeature> GetFeatureObj(int type, string culture)
        {
            return GetFeatureObj(type);
        }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class VIMAttribute : Attribute
    {
        private string name;
        private Type vimType;
        public VIMAttribute(string name, Type vim)
        {
            this.name = name;
            this.vimType = vim;
        }
    }

    public class VIMName
    {
        public const string FS = "fs_vim";
        public const string NETAPP_CIFS = "netapp_cifs_vim";
        public const string NETAPP_LUN = "netapp_lun_vim";
        public const string MirrorFS = "mirrorfs_vim";
        public const string FTP = "ftp_vim";
        public const string TSM = "tsm_vim";
        public const string Centera = "centera_vim";
        public const string Rackspace = "rackspace_vim";
        public const string Azure = "azure_vim";
        public const string Amazon = "amazon_vim";
        public const string Atmos = "atmos_vim";
        public const string ATT = "att_vim";
        public const string Dropbox = "dropbox_vim";
        public const string CAStor = "castor_vim";
        public const string HCP = "hcp_vim";
        public const string Caringo = "caringo_vim";
        public const string Box = "box_vim";
        public const string SkyDrive = "skydrive_vim";
        public const string GoogleDrive = "googledrive_vim";
        public const string SFTP = "sftp_vim";
        public const string NETAPP_ALTA_VAULT = "netapp_alta_vault_vim";
        public const string S3Compatible = "s3compatible_vim";
    }

}