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
using System.IO;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public abstract class AveLanguage
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public abstract string Export();

        public abstract void Export(IAveBackupStream stream);

        protected AveLanguageInfo BuildLanguageInfo(AveLanguageProcesser processor, uint LCID)
        {
            string coreFileName = string.Empty;
            if (processor.ResourceFileMapping.ContainsKey(LCID))
            {
                coreFileName = (string)processor.ResourceFileMapping[LCID];
            }
            else
            {
                System.Globalization.CultureInfo cul = new System.Globalization.CultureInfo((int)LCID, false);
                coreFileName = "core." + cul.Name + ".resx";
                mLog.Debug("Get Language file: \t" + coreFileName);
            }
            string srcLanguageFile = Path.Combine(processor.ResXRootPath, coreFileName);
            string languageContent = File.Exists(srcLanguageFile)? File.ReadAllText(srcLanguageFile) : string.Empty;
            AveLanguageInfo LanguageInfo = new AveLanguageInfo();
            LanguageInfo.LanguageContent = languageContent;
            LanguageInfo.LanguageLCD = LCID;
            return LanguageInfo;
        }

        public static AveLanguage CreateInstance(object obj)
        {
            AveLanguage instance = null;

            string type = obj.GetType().Name;
            switch (type)
            {
                case "AveSPSite":
                    instance = new AveSiteLanguage((AveSPSite)obj);
                    break;
                case "AveSPWeb":
                    instance = new AveWebLanguage((AveSPWeb)obj);
                    break;
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }

            return instance;
        }
    }

    public class AveSiteLanguage : AveLanguage
    {
        private AveSPSite mAveSPSite;

        public AveSiteLanguage(AveSPSite aveSite)
        {
            mAveSPSite = aveSite;
        }

        public AveLanguageInfo GetLanguageInfo()
        {
            AveLanguageProcesser processor = mAveSPSite.LanguageProcessor;
            uint LCID = mAveSPSite.SPSite.RootWeb.Language;
            return BuildLanguageInfo(processor, LCID);
        }

        public override string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.LanguageFile.ToString(), GetLanguageInfo());
        }

        public override void Export(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.LanguageInfo"))
            {
                stream.WriteMetadata(AveMetadataType.LanguageFile, GetLanguageInfo());
            }
        }
    }

    public class AveWebLanguage : AveLanguage
    {
        private AveSPWeb mAveSPWeb;

        public AveWebLanguage(AveSPWeb aveWeb)
        {
            mAveSPWeb = aveWeb;
        }

        public AveLanguageInfo GetLanguageInfo()
        {
            AveLanguageProcesser processor = mAveSPWeb.ParentSite.LanguageProcessor;
            uint LCID = mAveSPWeb.SPWeb.Language;
            return BuildLanguageInfo(processor, LCID);
        }

        public override string Export()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.LanguageFile.ToString(), GetLanguageInfo());
        }

        public override void Export(IAveBackupStream stream)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.LanguageInfo"))
            {
                stream.WriteMetadata(AveMetadataType.LanguageFile, GetLanguageInfo());
            }
        }
    }
}