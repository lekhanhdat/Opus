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




namespace AvePoint.Media.ClassicStorage
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.Media.ClassicStorage.Inner;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.StorageApi;

    #endregion

    /// <summary>
    /// 是一个简易包装的Factory， 用于初始化一个XLibrary or XSystem， 如果初始化的是XLibrary，里面操作的就是多个XSystem，类似Loigcal Device跟Physical Device的关系；如果初始化的是XSystem，则对应的就是Physical Device。也就是说XFactory 是对Logical Device and Physical Device不同层面的暴露.
    /// </summary>
    public class XFactory
    {

        public static XConfiguration cfg = new XConfiguration();
        private static object locker = new object();
        static Dictionary<string, Assembly> vimAsses = new Dictionary<string, Assembly>();

        public static IXSystemCommon InstanceSystem(string xriString)
        {
            XRI xri = XRI.ValueOf(xriString);
            IVIM vim = LoadVIM(xri.VIM);
            IXSystemCommon sys = vim.CreateSystem(xriString, null);
            return sys;
        }

        public static XLibraryCommon InstanceLibrary(List<string> xris)
        {
            XLibraryCommon lib = new XLibraryCommon();
            XRI xri;
            IVIM vim;
            foreach (string xriStr in xris)
            {
                xri = XRI.ValueOf(xriStr);
                vim = LoadVIM(xri.VIM);
                lib.AddVIM(xriStr, vim);
            }
            return lib;
        }

        /// <summary>
        /// 根据不同的device来创建不同的Xri，仅限于Agent端使用
        /// 0   FS
        /// 1   Centera
        /// 2   TSM
        /// 3   FTP
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="location"></param>
        /// <param name="username"></param>
        /// <param name="port"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string CreateXri(int deviceType, string location, string username, string password, int port, params string[] parameters)
        {

            switch (deviceType)
            {
                case 0://FS
                    return string.Format(XConst.MEDIASTORAGE_PROTOCOL + "fs_vim?location={0}&name={1}&secret={2}&" + "IS".ToLower(CultureInfo.InvariantCulture) + "validate=false&creation=true", location, username, XRI.ValueEncode(password));
                case 3://ftp
                    return string.Format(XConst.MEDIASTORAGE_PROTOCOL + "ftp_vim?host={0}&port={1}&name={2}&secret={3}", location, port, username, XRI.ValueEncode(password));
                default:
                    throw new NotImplementedException("Unknown device type:" + deviceType);
            }
        }
        public static string CreateXri(params string[] parameters)
        {
            string xri = string.Empty;
            int deviceType = int.Parse(parameters[0]);
            int subType = int.Parse(parameters[1]);
            //string location = parameters[2];
            //string username = parameters[3];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = XRI.ValueEncode(parameters[i]);
            }
            switch (deviceType)
            {
                case 0://FS
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "fs_vim?location={0}&name={1}&secret={2}", parameters[2], parameters[3], parameters[4]);
                    break;
                case 1://ftp
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "ftp_vim?host={0}&port={1}&name={2}&secret={3}", parameters[2], parameters[3], parameters[4], parameters[5]);
                    break;

                case 2://tsm
                    string ClassString = "MCLASS".ToLower(CultureInfo.InvariantCulture);
                    if (subType == 0)
                    {
                        xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "TSM_VIM?COMMMETHOD=".ToLower(CultureInfo.InvariantCulture) + "TCPIP".ToLower(CultureInfo.InvariantCulture) + "&address={0}&port={1}&node={2}&" + ClassString + "={3}&secret={4}", parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                        xri = xri.Replace("&" + ClassString + "=&", "&");
                    }
                    else
                    {
                        xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "TSM_VIM?COMMMETHOD=V6TCPIP&ADDRESS={0}&PORT={1}&NODE={2}&".ToLower(CultureInfo.InvariantCulture) + ClassString + "={3}&secret={4}", parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                        xri = xri.Replace("&" + ClassString + "=&", "&");
                    }
                    break;
                case 3://centera
                    if (subType == 0)
                    {
                        xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "centera_vim?AUTHTYPE=n/SAUTH&address={0}&name={1}&secret={2}", parameters[2], parameters[3], parameters[4]);
                    }
                    else
                    {
                        xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "centera_vim?AUTHTYPE=pea&address={0}&PAEAUTH={1}&PAEU={2}&PAEPSECRET={3}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5]);
                    }
                    break;
                case 401://amazon
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "amazon_vim?region={0}&" + "C".ToLower(CultureInfo.InvariantCulture) + "type={1}&BUCKETNAME={2}&name={3}&secret={4}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                    break;
                case 402://rackspace
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "rackspace_vim?CDN={0}&" + "C".ToLower(CultureInfo.InvariantCulture) + "type={1}&CONTAINERNAME={2}&name={3}&secret={4}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                    break;
                case 403://azure
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "azure_vim?ACCESSPOINT={0}&CDNED={1}&" + "C".ToLower(CultureInfo.InvariantCulture) + "type={2}&CONTAINERNAME={3}&name={4}&secret={5}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7]);
                    if (parameters[8] != String.Empty)
                    {
                        xri = xri + "&CDNGUID=".ToLower(CultureInfo.InvariantCulture) + parameters[8];
                    }
                    break;
                case 404://atmos
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "atmos_vim?ACCESSPOINT={0}&" + "C".ToLower(CultureInfo.InvariantCulture) + "type={1}&CONTAINERNAME={2}&name={3}&secret={4}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5], parameters[6]);
                    break;
                case 405://att
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "att_vim?" + "C".ToLower(CultureInfo.InvariantCulture) + "type={0}&CONTAINERNAME={1}&name={2}&secret={3}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5]);
                    break;
                case 5://dell
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "castor_vim?WITHREMOTECLUSTER={0}&ACCESSMODE={1}&COMPRESSTYPE={2}&PRIMARYNODE={3}&PRIMARYPORT={4}&CLUSTERNAME={5}&CRPUBLISHER={6}&CRPUBLISHERPORT={7}&REMOTECSNHOST={8}&REMOTECSNPORT={9}&REPLICASNUMBER={10}&DEFERCOMPRESSION={11}&SCSPPROXYHOST={12}&SCSPPROXYPORT={13}&REMOTECLUSTERNAME={14}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[4], parameters[5], parameters[6], parameters[7], parameters[8], parameters[9], parameters[10], parameters[11], parameters[12], parameters[13], parameters[14], parameters[15], parameters[16]);
                    xri = xri.Replace("&CRPUBLISHER=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&CRPUBLISHERPORT=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&REMOTECSNHOST=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&REMOTECSNPORT=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&DEFERCOMPRESSION=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&SCSPPROXYHOST=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&SCSPPROXYPORT=&".ToLower(CultureInfo.InvariantCulture), "&");
                    xri = xri.Replace("&REMOTECLUSTERNAME=&", "&");
                    if (xri.EndsWith("=", StringComparison.CurrentCulture))
                    {
                        int p = xri.LastIndexOf("&", StringComparison.CurrentCulture);
                        xri = xri.Substring(0, p);

                    }
                    break;
                case 14: //google
                    xri = string.Format(XConst.MEDIASTORAGE_PROTOCOL + "google_vim?accesspoint={0}&" + "C".ToLower(CultureInfo.InvariantCulture) + "containername={1}&name={2}&secret={3}".ToLower(CultureInfo.InvariantCulture), parameters[2], parameters[3], parameters[0], parameters[1]);
                    break;
                default:
                    throw new NotImplementedException("Unknown device type:" + deviceType);
            }

            return xri;
        }
        public static string CreateXri(int deviceType, string location, string domain, string username, string password, int port, params string[] parameters)
        {
            string userFullName = username;
            if (!string.IsNullOrEmpty(domain))
            {
                userFullName = string.Format("{0}\\{1}", domain, username);
            }

            return CreateXri(deviceType, location, userFullName, password, port, parameters);
        }

        #region 快速实例
        public static XLibraryCommon InstanceDemoLibrary()
        {
            List<string> xris = new List<string>();
            //xris.Add(@"docave-xam://fs_vim?location=\\10.2.6.38\docave\fs1&name=storage\administrator&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE")));
            return InstanceLibrary(xris);
        }

        public static XLibraryCommon InstanceDemoLibrary(int deviceType)
        {
            List<string> xris = new List<string>();
            switch (deviceType)
            {
                case 0://fs
                    // xris.Add(@"docave-xam://fs_vim?location=\\10.2.6.38\docave\fs1&name=storage\administrator&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE")));
                    //xris.Add(@"docave-xam://fs_vim?location=\\10.2.6.38\docave\fs2&name=storage\administrator&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE")));
                    break;
                case 1://centera
                    //xris.Add(@"docave-xam://centera_vim?address=128.221.200.60,128.221.200.61,128.221.200.63&authType=ns&name=profile3&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("profile3")));
                    break;
                case 2://tsm
                    //xris.Add(@"docave-xam://tsm_vim?commMethod=tcpip&address=10.2.6.40&port=1500&node=yxjin&secret= " + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1q")));
                    break;
                case 3://ftp
                    //xris.Add(@"docave-xam://ftp_vim?host=10.2.6.40&port=21&name=yxjin&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE")));
                    break;
                case 4://cloud
                    break;
                case 5://netapp
                    break;
                case 6://castor
                    //xris.Add(@"docave-xam://castor_vim?cas.caringo.com");
                    break;
                default:
                    throw new NotImplementedException("Unknown device type");
            }


            return InstanceLibrary(xris);
        }

        public static IXSystemCommon InstanceDemoSystem()
        {
            return null;
        }

        public static IXSystemCommon InstanceDemoSystem(int deviceType)
        {
            switch (deviceType)
            {
                case 0://fs
                    return null;//InstanceSystem(@"docave-xam://fs_vim?location=\\10.2.6.38\docave\fs1&name=storage\administrator&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE")));
                case 1://centera
                    return null;//InstanceSystem(@"docave-xam://centera_vim?address=128.221.200.60,128.221.200.61,128.221.200.63&authType=n/sAuth&name=profile3&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("profile3")));
                case 2://tsm
                    return null;//InstanceSystem(@"docave-xam://tsm_vim?commMethod=tcpip&address=10.2.6.40&port=1500&node=yxjin&secret= " + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1q")));
                case 3://ftp
                    return null;//InstanceSystem(@"docave-xam://ftp_vim?host=10.2.6.40&port=21&name=yxjin&secret=" + XRI.ValueEncode(EncryptionFactory.GetDefaultEncryption().EncryptedString("1qaz2wsxE")));
                case 4://cloud
                    break;
                case 5://netapp
                    break;
                case 6://castor
                    return null;//InstanceSystem(@"docave-xam://castor_vim?cas.caringo.com");
                default:
                    throw new NotImplementedException("Unknown device type");
            }
            return null;
        }
        #endregion

        #region XFeature for GUI



        public static List<string> GetAllSubFeatures()
        {
            StorageFeature sf;
            List<IXFeature> allFeatures = GetFeatureObjects(FeatureType.DocAveGUI);

            List<IXFeature> allSubFeatures = new List<IXFeature>();

            List<string> featureXMLs = new List<string>();
            foreach (IXFeature fe in allFeatures)
            {
                sf = fe as StorageFeature;
                if (sf.Features.Count == 1 && sf.Features[0].ChildFeatures.Count > 0 && sf.Features[0].HasSparrow)
                {
                    foreach (FeatureUnit fu in sf.Features[0].ChildFeatures)
                    {
                        IXFeature storageFeature = new StorageFeature();
                        StorageType storageType = new StorageType();
                        storageType.Display = fu.DisplayName;
                        storageType.Index = fu.Index;
                        storageType.Vim = new List<string>() { fu.Vim };
                        storageType.Value = fu.Value;
                        storageFeature.Type = storageType;
                        storageFeature.Features = fu.ChildFeatures;
                        allSubFeatures.Add(storageFeature);
                    }
                }
                else
                {
                    allSubFeatures.Add(fe);
                }
            }

            foreach (IXFeature fe in allSubFeatures)
            {
                sf = fe as StorageFeature;
                string xml = FeatureUtility.Serialize(sf);
                featureXMLs.Add(xml);
            }
            return featureXMLs;
        }

        public static List<StorageFeature> GetAllFeatureObjects(FeatureType featureType)
        {
            ReadStorageConfigOnce();
            List<StorageFeature> allFeatures = new List<StorageFeature>();
            IVIM vim;
            foreach (KeyValuePair<string, VIMInfo> entity in cfg.Vims)
            {
                try
                {
                    vim = LoadVIM(entity.Value.Name);
                    foreach (StorageFeature feature in vim.GetFeatureObj((int)featureType))
                    {
                        if (feature != null)
                        {
                            if (!allFeatures.Contains(feature))
                            {
                                allFeatures.Add(feature);
                            }
                        }
                    }
                }
                catch (Exception t)
                {
                    Trace.TraceWarning(t.ToString());
                }
            }
            return allFeatures;
        }

        private static List<IXFeature> GetFeatureObjects(FeatureType featureType)
        {
            return GetFeatureObjects(featureType, "en");
        }

        private static List<IXFeature> GetFeatureObjects(FeatureType featureType, string culture = "en")
        {
            ReadStorageConfigOnce();
            List<IXFeature> allFeatures = new List<IXFeature>();
            IVIM vim;
            //IXFeature feature;

            foreach (KeyValuePair<string, VIMInfo> entity in cfg.Vims)
            {
                try
                {
                    vim = LoadVIM(entity.Value.Name);

                    foreach (StorageFeature feature in vim.GetFeatureObj((int)featureType, culture))
                    {
                        //feature = sf as IXFeature;
                        if (feature != null)
                        {
                            if (!allFeatures.Contains(feature))
                            {
                                allFeatures.Add(feature);
                            }
                        }
                    }
                }
                catch (Exception t)
                {
                    Trace.TraceWarning(t.ToString());
                }
            }
            allFeatures.Sort(new CaseInsensitiveSortMode());
            return allFeatures;
        }

        private class CaseInsensitiveSortMode : IComparer<IXFeature>
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer<IXFeature>.Compare(IXFeature x, IXFeature y)
            {
                List<String> jumpQueueList = new List<String> {  };

                if (x.Type.Value.Equals(y.Type.Value))
                {
                    return 0;
                }
                foreach (String key in jumpQueueList)
                {
                    if (key.Equals(x.Type.Value))
                    {
                        return -1;
                    }
                    else if (key.Equals(y.Type.Value))
                    {
                        return 1;
                    }
                }
                return ((new CaseInsensitiveComparer()).Compare(x.Type.Display, y.Type.Display));
            }
        }

        public static List<string> GetAllFeatures(FeatureType featureType)
        {
            try
            {
                StorageFeature sf;
                List<IXFeature> allFeatures = GetFeatureObjects(featureType);
                List<string> featureXMLs = new List<string>();
                foreach (IXFeature fe in allFeatures)
                {
                    sf = fe as StorageFeature;
                    string xml = FeatureUtility.Serialize(sf);
                    featureXMLs.Add(xml);
                }
                return featureXMLs;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<string> GetAllFeatures(FeatureType featureType, string culture)
        {
            try
            {
                StorageFeature sf;
                List<IXFeature> allFeatures = GetFeatureObjects(featureType, culture);
                List<string> featureXMLs = new List<string>();
                foreach (IXFeature fe in allFeatures)
                {
                    sf = fe as StorageFeature;
                    string xml = FeatureUtility.Serialize(sf);
                    featureXMLs.Add(xml);
                }
                return featureXMLs;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<string> GetAllFeatures()
        {
            try
            {
                StorageFeature sf;
                List<IXFeature> allFeatures = GetFeatureObjects(FeatureType.DocAveGUI);
                List<string> featureXMLs = new List<string>();
                foreach (IXFeature fe in allFeatures)
                {
                    sf = fe as StorageFeature;
                    string xml = FeatureUtility.Serialize(sf);
                    featureXMLs.Add(xml);
                }
                return featureXMLs;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        public static string CacheLocation { get; set; }

        private static void ReadStorageConfigOnce()
        {
            if (!cfg.Loaded)
            {
                lock (locker)
                {
                    if (!cfg.Loaded)
                    {
                        cfg.load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media.Storage/Storage.config"));
                    }
                }
            }
        }

        private static IVIM LoadVIM(string vimName)
        {
            lock (locker)
            {
                ReadStorageConfigOnce();
                VIMInfo info = null;

                try
                {
                    info = cfg.GetVIMInfo(vimName);
                }
                catch (Exception e)
                {
                    throw new VIMLoadException("Can not find vim info in Storage.config : " + info.Name, e);
                }
                if (info == null)
                {
                    throw new VIMLoadException("Can not find vim info in Storage.config : " + info.Name);
                }

                //Assembly ass = null;
                //if (vimAsses.ContainsKey(info.Name))
                //{
                //    ass = vimAsses[info.Name];
                //}
                //else
                //{
                //    ass = Assembly.Load(info.DllFile);
                //    vimAsses[info.Name] = ass;
                //}

                //if (ass == null)
                //{
                //    throw new VIMLoadException("Can not load vim dll : " + info.DllFile);
                //}
                var vim = Activator.CreateInstance(Type.GetType(info.Type)) as IVIM;
                if (vim == null)
                {
                    throw new VIMLoadException("Can not instance vim : " + info.Name);
                }
                return vim;
            }

        }

        public static Assembly GetAssembly(string vimName)
        {
            lock (locker)
            {
                ReadStorageConfigOnce();
                VIMInfo info = null;
                try
                {
                    info = cfg.GetVIMInfo(vimName);
                }
                catch (Exception e)
                {
                    throw new VIMLoadException("Can not find vim info in Storage.config : " + info.Name, e);
                }
                if (info == null)
                {
                    throw new VIMLoadException("Can not find vim info in Storage.config : " + info.Name);
                }

                Assembly ass = null;
                if (vimAsses.ContainsKey(info.Name))
                {
                    ass = vimAsses[info.Name];
                }
                else
                {
                    ass = Assembly.Load(info.DllFile);
                    vimAsses[info.Name] = ass;
                }

                if (ass == null)
                {
                    throw new VIMLoadException("Can not load vim dll : " + info.DllFile);
                }
                return ass;
            }
        }
        /// <summary>
        /// 0   FS
        /// 1   FTP
        /// 2   TSM
        /// 3   Centera
        /// 4   Cloud
        /// 406 HCP  
        /// 407 SkyDrive
        /// 410 GoogleDrive
        /// 5   DellDX
        /// 6   MirrorFS
        /// 701 CIFS
        /// 702 LUN
        /// </summary>
        internal static int[] TypesSOExtemderSupported
        {
            get
            {
                if (typesSOExtenderSupported == null)
                {
                    try
                    {
                        List<IXFeature> features = GetFeatureObjects(FeatureType.DocAveGUI);
                        List<int> types = new List<int>();
                        foreach (IXFeature feature in features)
                        {
                            if (!feature.Type.SoExtenderNotSupported)
                            {

                                types.Add(feature.Type.Index);
                            }
                        }
                    }
                    catch (Exception t)
                    {
                        Trace.TraceWarning(t.ToString());
                    }
                    if (typesSOExtenderSupported == null || typesSOExtenderSupported.Length == 0)
                    {
                        typesSOExtenderSupported = new int[] { 0, 3, 4, 401, 402, 403, 404, 405, 406, 407, 5, 6, 7, 8, 701, 702, 8, 9 ,10, 11 };
                    }

                }
                return typesSOExtenderSupported;
            }
            //get
            //{
            //    return new int[] { 0, 3, 4, 406, 5, 6 };
            //}
        }

        private static int[] typesSOArchiveSupported;
        private static int[] typesSOExtenderSupported;

        internal static int[] TypesSOArchiveSupported
        {
            get
            {
                if (typesSOArchiveSupported == null)
                {
                    try
                    {
                        List<IXFeature> features = GetFeatureObjects(FeatureType.DocAveGUI);
                        List<int> types = new List<int>();
                        foreach (IXFeature feature in features)
                        {
                            if (!feature.Type.SoArchiverNotSupported)
                            {

                                types.Add(feature.Type.Index);
                            }
                        }
                    }
                    catch (Exception t)
                    {
                        Trace.TraceWarning(t.ToString());
                    }
                    if (typesSOArchiveSupported == null || typesSOArchiveSupported.Length == 0)
                    {
                        typesSOArchiveSupported = new int[] { 0, 1, 2, 3, 4, 401, 402, 403, 404, 405, 406, 407, 5, 6, 7, 701, 702, 8, 9 ,10, 11 };
                    }

                }
                return typesSOArchiveSupported;
            }
        }
    }

    public interface IResourcePool<T, TK> : IDisposable
    {
        T borrowObject(TK key);

        void ReturnObject(TK key, T obj);

    }

    public interface IResourceProvider<T, TK> where T : XObject
    {
        T NewInstance(TK str);

        void Dispose(T resource);
    }

    public class ResourceTag<T> where T : XObject
    {
        private T resource;
        private bool inUse;

        public ResourceTag(T r)
        {
            this.resource = r;
        }

        public bool InUse
        {
            get { return inUse; }
            set { this.inUse = value; }
        }
        public T Resource
        {
            get { return resource; }
            set { this.resource = value; }
        }
    }
}
