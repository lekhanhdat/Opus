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
using System.IO;
using System.IO.Compression;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource;


namespace LS.BinarySerialization.Replacer
{
    public delegate string ModifyLoginEventHandler(object sender, string origLogin);

    public delegate string ModifyListIdEventHandler(object sender, string origListId);

    public delegate string ModifyEmailAddressEventHandler(object sender, string origEmailAddress);

    public delegate string ModifyContentTypeIdEventHandler(object sender, string origContentTypeId);


    public class LSMemberDataInfo
    {
        public object OldValue
        {
            get;
            set;
        }

        public object NewValue
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public int Length
        { get; set; }

        public string Profix
        { get; set; }

        public string DependencyPropertyName
        { get; set; }

        public LSMemberDataInfo(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            Profix = LSBinarySerReplacer.ProfixOfActivityMember;
        }

        public LSMemberDataInfo(object oldValue, object newValue, string profix)
        {
            OldValue = oldValue;
            NewValue = newValue;
            Profix = profix;
        }

        public LSMemberDataInfo(object oldValue, object newValue, string profix, string propName)
        {
            OldValue = oldValue;
            NewValue = newValue;
            Profix = profix;
            DependencyPropertyName = propName;
        }

    }

    public class LSMemberDataInfoEx
    {
        private Dictionary<string, LSMemberDataInfo> mMemberDataInfoCollection;
        public Dictionary<string, LSMemberDataInfo> MemberDataInfoCollection
        {
            get
            {
                if (mMemberDataInfoCollection == null)
                    mMemberDataInfoCollection = new Dictionary<string, LSMemberDataInfo>();
                return mMemberDataInfoCollection;
            }
        }

        private List<string> mDependencyPropertyNames;
        public List<string> DependencyPropertyNames
        {
            get
            {
                if (mDependencyPropertyNames == null)
                    mDependencyPropertyNames = new List<string>();
                return mDependencyPropertyNames;
            }
        }

        public void Dispose()
        {
            if (mMemberDataInfoCollection != null)
            {
                mMemberDataInfoCollection.Clear();
                mMemberDataInfoCollection = null;
            }
            if (mDependencyPropertyNames != null)
            {
                mDependencyPropertyNames.Clear();
                mDependencyPropertyNames = null;
            }

        }
    }

    public sealed class LSBinarySerReplacer
    {

        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public const string ProfixOfActivityMember = "LS";
        public const string ProfixOfSetVariable = "LS#SetVar";
        public const string ProfixOfDependencyProperty = "LS#DependProp";

        #region Events
        public static event ModifyLoginEventHandler ModifyLoginEvent;
        internal static string RaiseModifyLoginEvent(string origValue)
        {
            string newValue = origValue;
            if (ModifyLoginEvent != null)
            {
                newValue = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ModifyLoginEvent(null, origValue);
            }
            return newValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origValue"></param>
        /// <param name="extraInfo">schema is ID,DisplayName,Notes,Email</param>
        /// <returns></returns>
        internal static string RaiseModifyLoginEvent(string origValue, List<string> extraInfo)
        {
            string newValue = origValue;
            if (ModifyLoginEvent != null)
            {
                newValue = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ModifyLoginEvent(extraInfo, origValue);
            }
            return newValue;
        }

        public static event ModifyEmailAddressEventHandler ModifyEmailAddressEvent;
        internal static string RaiseEmailAddressEvent(string origValue)
        {
            if (string.IsNullOrEmpty(origValue))
                return origValue;

            if (origValue.IndexOf('@') < 0)
                return origValue;

            string newValue = origValue;
            if (ModifyEmailAddressEvent != null)
            {
                newValue = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ModifyEmailAddressEvent(null, origValue);
            }
            return newValue;
        }

        public static event ModifyListIdEventHandler ModifyListIdEvent;
        internal static string RaiseModifyListIdEvent(string origValue)
        {
            if (string.IsNullOrEmpty(origValue))
                return origValue;

            string newValue = origValue;
            if (ModifyListIdEvent != null)
            {
                newValue = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ModifyListIdEvent(null, origValue);
            }
            return newValue;
        }

        public static event ModifyContentTypeIdEventHandler ModifyContentTypeIdEvent;
        internal static string RaiseModifyContentTypeIdEvent(string origValue)
        {
            if (string.IsNullOrEmpty(origValue))
                return origValue;

            string newValue = origValue;
            if (ModifyContentTypeIdEvent != null)
            {
                newValue = LS.BinarySerialization.Replacer.LSBinarySerReplacer.ModifyContentTypeIdEvent(null, origValue);
            }
            return newValue;
        }

        #endregion


        public static bool Execute(byte[] inData, Dictionary<string, object> dictionary, out byte[] outData)
        {
            try
            {
                outData = null;
                MemoryStream stream = null;
                stream = new MemoryStream(inData);

                LSBinaryFormatter formatter = new LSBinaryFormatter();
                formatter.Deserialize(stream);
                stream.Close();

                LSObjectNodeAnalyze analyzeProc = new LSObjectNodeAnalyze(formatter.lsObjectNodeCollection);
                analyzeProc.Analyze();

                using (LSReplaceValues replaceProc = new LSReplaceValues(inData, dictionary, analyzeProc))
                {
                    outData = replaceProc.ReplaceNodeValues(true);
                }
                return true;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.ExcyteDeSerializeError, e.ToString());
                outData = null;
                return false;
            }

        }

        public static bool ExecuteWithCompress(byte[] inData, Dictionary<string, object> dictionary, out byte[] outData, bool isCompress)
        {
            outData = null;

            #region Decompress Instance Data
            byte[] decompressedData = new byte[0];
            MemoryStream tempStream = new MemoryStream(inData);
            tempStream.Position = 0L;
            byte[] temp = new byte[4096];
            if (isCompress)
            {
                using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
                {
                    try
                    {
                        int readLen;
                        while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                        {
                            LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            else
            {
                decompressedData = inData;
            }
            #endregion

            if (Execute(decompressedData, dictionary, out outData))
            {
                using (MemoryStream stream2 = new MemoryStream(outData.Length))
                {
                    using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress, true))
                    {
                        stream3.Write(outData, 0, outData.Length);
                    }
                    outData = stream2.GetBuffer();
                    Array.Resize<byte>(ref outData, Convert.ToInt32(stream2.Length));
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
