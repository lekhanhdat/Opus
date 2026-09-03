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



using AvePoint.Common;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace RAGoogle.Restore
{
    public class ArchiveConfigFileInfo
    {
        public XmlElement ArchiveDel { get; private set; }
        public static readonly string SOConfigurationFileName = "AgentCommonStorageENV.cfg";

        public static readonly string SOStopJobFlag = "JobStop.cmd";

        public static string SOConfigurationFilePath
        {
            get { return System.IO.Path.Combine(AveEnv.AgentDataPath, "SP2010/Arch", SOConfigurationFileName); }
        }
        public ArchiveConfigFileInfo()
        {
            XmlDocument envDoc = new XmlDocument();
            envDoc.Load(SOConfigurationFilePath);
            ArchiveDel = (XmlElement)envDoc.DocumentElement.SelectSingleNode("Archive");
        }
        private string GetConfigFile(string key)
        {
            return ArchiveDel.GetAttribute(key);
        }
        public ArchiveConfigFileInfo ItemConflictType { get; set; }

        //if reture true :  delete the ApproveDB
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "keepapprovedb is an attribute of xml")]
        public bool KeepApproveDB()
        {
            if (GetConfigFile("keepapprovedb") != string.Empty && GetConfigFile("keepapprovedb").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //if reture true : Do not delete the item 
        public bool KeepDocument(string itemName)
        {
            for (int i = 0; i < GetConfigFile("skip").Split(' ').Count(); i++)
            {
                if (GetConfigFile("skip").Split(' ')[i] != string.Empty && itemName.EndsWith(GetConfigFile("skip").Split(' ')[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        //if reture true : Do not delete the container structure 
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "keepsharepointstructure is an attribute of xml")]
        public bool KeepContainerStructure()
        {
            if (GetConfigFile("keepsharepointstructure") != string.Empty && GetConfigFile("keepsharepointstructure").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InverseInterpolatio()
        {
            if (GetConfigFile("DependenceConflictOperation") != string.Empty && GetConfigFile("DependenceConflictOperation").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ItemConflictOverWrite()
        {
            if (GetConfigFile("ConflictOption") != string.Empty && GetConfigFile("ConflictOption").Equals("3", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ItemConflictAppend()
        {
            if (GetConfigFile("ConflictOption") != string.Empty && GetConfigFile("ConflictOption").Equals("4", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ItemConflictSkip()
        {
            if (GetConfigFile("ConflictOption") != string.Empty && GetConfigFile("ConflictOption").Equals("2", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listtemplate is an attribute of xml")]
        public void GetListTemplate(ref List<int> listTemplate)
        {
            for (int i = 0; i < GetConfigFile("listtemplate").Split(' ').Count(); i++)
            {
                listTemplate.Add(int.Parse(GetConfigFile("listtemplate").Split(' ')[i]));
            }
        }
    }

}
