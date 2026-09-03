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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken.Object;
using AvePoint.GCommon.Utility.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.GCommon.Utility.AppProfile
{
    public class Office365AppProfileUtil
    {
        //public static string GetMd5Url(string src, string dest)
        //{
        //    var urlText = src.ToLower() + "_" + dest.ToLower();
        //    var urlMd5 = MD5Encrypt(urlText);
        //    return urlMd5;
        //}
        //public static string MD5Encrypt(string strText)
        //{
        //    MD5 md5 = new MD5CryptoServiceProvider();
        //    byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
        //    return System.Text.Encoding.Default.GetString(result);
        //}
        public const string PASSWORD_RETURN_VALUE = "A!v@E#$p";

        public static Office365AppProfileModel ConvertToModel(ProfileDto profile)
        {
            if (profile != null)
            {
                var content = profile.Content as Office365AppProfileContent;
                if (content != null)
                {
                    return new Office365AppProfileModel()
                    {
                        Id = profile.Id,
                        UserName = content.UserName,
                        AppType = content.Type,
                        TenantId = content.TenantId,
                        AppId = content.ApplicationId,
                        CertificatePath = content.CertificatePath,
                        Password = PASSWORD_RETURN_VALUE,
                        Status = profile.AppProfileState,
                        AppName = profile.Name,
                        AzureRegion = content.AzureRegion,
                        Certificate = content.Certificate,
                        AppErrorType = (AppErrorType)content.AppErrorType,
                        RedirectURL = content.RedirectURL,
                        ModifiedTime = content.ModifiedTime,
                        AdminUrl = content.AdminUrl,
                        SaveWithoutAuth = content.SaveWithoutAuth
                    };
                }
            }
            return null;
        }

        public static ProfileDto ConvertToDto(Office365AppProfileModel model)
        {
            var content = new Office365AppProfileContent()
            {
                ApplicationId = model.AppId,
                UserName = model.UserName,
                Type = model.AppType,
                TenantId = model.TenantId,
                Certificate = model.Certificate != null ? model.Certificate : null,
                Password = string.IsNullOrEmpty(model.Password) ? string.Empty : CspCommunicationWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(model.Password)),
                CertificatePath = model.CertificatePath,
                AzureRegion = model.AzureRegion,
                AppErrorType = (int)model.AppErrorType,
                RedirectURL = model.RedirectURL,
                ModifiedTime = model.ModifiedTime,
                AdminUrl = model.AdminUrl,
                SaveWithoutAuth = model.SaveWithoutAuth
            };
            var profile = new ProfileDto()
            {
                Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id,
                AppProfileState = model.Status,
                Name = model.AppName,
                Type = ProfileType.Office365AppToken,
                Content = content
            };
            return profile;
        }
    }
}
