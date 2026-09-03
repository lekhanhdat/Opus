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



namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using System.Collections.Generic;

    #endregion

    class CenteraResourcePool : IResourcePool<CenteraPoolResourceTag, String>
    {
        static Object poolLocker = new Object();
        static Object borrowLocker = new Object();
        static Object returnLocker = new Object();
        CenteraPoolProvider provider;
        static CenteraResourcePool pool;
        Dictionary<String, CenteraPoolResourceTag> resources;

        private CenteraResourcePool(CenteraPoolProvider provider)
        {
            this.provider = provider;
            this.resources = new Dictionary<String, CenteraPoolResourceTag>();
        }

        private object locker = new object();
        #region ResourcePool<FPPool> Resources;


        /// <summary>
        /// borrowObject, return the same Pool Instance if the connection String is same.
        /// Increasing performance : To increase performance in a multithreaded environment, the API should share one FPPoolRef by all threads instead of one FPPoolRef per thread.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CenteraPoolResourceTag borrowObject(String key)
        {
            if (resources.ContainsKey(key))
            {
                resources[key].IncrementUsed();
                return resources[key];
            }
            lock (locker)
            {
                if (resources.ContainsKey(key))
                {
                    resources[key].IncrementUsed();
                    return resources[key];
                }
                FPPool pool = provider.NewInstance(key);
                CenteraPoolResourceTag newTag = new CenteraPoolResourceTag(pool);
                resources.Add(key, newTag);
                return newTag;
            }
        }

        public void ReturnObject(String key, CenteraPoolResourceTag obj)
        {
            if (resources.ContainsKey(key))
            {
                resources[key].DecreaseUsed();
            }
        }

        public static IResourcePool<CenteraPoolResourceTag, String> Instance
        {
            get
            {
                if (pool == null)
                {
                    lock (poolLocker)
                    {
                        if (pool == null)
                        {
                            CenteraPoolProvider provider = new CenteraPoolProvider();
                            pool = new CenteraResourcePool(provider);
                        }
                    }
                }
                return pool;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    class CenteraPoolResourceTag : ResourceTag<FPPool>
    {

        static Object Locker = new Object();
        int used;

        public CenteraPoolResourceTag(FPPool pool) : base(pool)
        {
            InUse = true;
            IncrementUsed();
        }

        public int Used
        {
            get { return used; }
        }

        public void IncrementUsed()
        {
            lock (Locker)
            {
                used++;
            }
        }
        public void DecreaseUsed()
        {
            lock (Locker)
            {
                used--;
            }
        }
    }

    class StorageInfoUtil
    {
        public static String ClipId2StorageInfo(String clipId)
        {
            return String.Format("<StorageInfo clipId=\"{0}\"/>", clipId);
        }

        public static String StorageInfo2ClipId(String storageInfo)
        {
            //to complete
            int begin = storageInfo.IndexOf('"');
            int end = storageInfo.LastIndexOf('"');
            return storageInfo.Substring(begin + 1, end - begin - 1);
            //try
            //{
            //    MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(storageInfo));
            //    var dataContractSerializer = new DataContractSerializer(Type.GetType("AvePoint.Media.Storage.Centera.Inner.StorageInfo"));
            //    Inner.StorageInfo info = dataContractSerializer.ReadObject(stream) as Inner.StorageInfo;

            //    Inner.StorageInfo si = SerializerHelper.DeserializeFromXmlString<Inner.StorageInfo>(storageInfo);
            //    si = FeatureUtility.Deserialize<Inner.StorageInfo>(storageInfo);
            //    si = SerializerHelper.DeserializeFromXmlStringWithoutDecalaring<Inner.StorageInfo>(storageInfo);
            //    si = SerializerHelper.DeserializeByDataContractSerializer<Inner.StorageInfo>(storageInfo);
            //    return info.clipId;
            //}
            //catch (Exception e)
            //{
            //    //TODO completed exception handle
            //    return null;
            //}
        }
        
    }

    class CenteraPoolProvider : IResourceProvider<FPPool, String>
    {
        public CenteraPoolProvider()
        {
            //全局只允许设定这三个属性

            //FPPool.RegisterApplication("StoneTestApp01012123", "001.001");
            FPPool.SetGlobalOption(FPOption.RETRYCOUNT, 10);
            
            FPPool.SetGlobalOption(FPOption.RETRYSLEEP, 30 * 1000);
            FPPool.SetGlobalOption(FPOption.MAXCONNECTIONS, 999);
        }
       
        #region IResourceProvider<FPPool> Members

        public void Dispose(FPPool resource)
        {
            resource.Close();
        }

        #endregion

        #region IResourceProvider<FPPool,String> Members

        public FPPool NewInstance(String str)
        {
            FPPool pool = new FPPool(str);
            return pool;
        }

        #endregion
    }
}
