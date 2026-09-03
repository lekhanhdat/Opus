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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.JobContextDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public class FailedObjectBase
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object lockObj = new object();
        private FailedObjCollection mTempFolders = null;
        private FailedObjCollection mChildren = new FailedObjCollection();

        public FailedObjectBase() { }

        #region Member

        [Column("COL_NAME")]
        public string Name { get; set; }

        [Column("COL_ID")]
        public string Id { get; set; }

        /// <summary>
        /// 0:succeed  1:failed
        /// </summary>
        [Column("COL_STATUS")]
        public bool IsFailed { get; set; }

        /// <summary>
        /// 是否需要对container做full backup，主要针对discover失败情况的处理
        /// </summary>
        [Column("COL_FLAG")]
        public int Flag { get; set; }

        /// <summary>
        /// failed count
        /// </summary>
        [Column("COL_FAILED_COUNT")]
        public int FailedCount { get; set; }

        [Column("COL_TYPE")]
        public string Type { get; set; }

        /// <summary>
        /// site: Url, web: ServerRelativeUrl, List: Name, Folder: Name, Item: ID, Document: Name
        /// </summary>
        [Column("COL_PATH")]
        public string Path { get; set; }

        //[Column("COL_PLANID")]
        //public string PlanId { get; set; }

        [Column("COL_JOBID")]
        public string JobId { get; set; }

        //[Column("COL_CYCLEID")]
        //public string CycleId { get; set; }

        [Column("COL_JOBTYPE")]
        public string JobType { get; set; }

        /// <summary>
        /// 是否已经删除
        /// </summary>
        [Column("COL_BACKUP_TYPE")]
        public int BackupType { get; set; }

        [Column("COL_PATH_MD5")]
        public string PathMD5 { get; set; }

        [Column("COL_PARENT_PATH_MD5")]
        public string ParentPathMD5 { get; set; }

        [Column("COL_APP_DATA_NAME")]
        public string AppDataName { get; set; }

        [Column("COL_IS_APP_DATA")]
        public bool IsAppData { get; set; }

        #endregion


        public FailedObjCollection Children
        {
            get
            {
                return mChildren;
            }
        }

        public List<string> ChildrenName
        {
            get
            {
                return Children.Select(n => n.Name).ToList();
            }
        }

        public FailedObjCollection Items
        {
            get
            {
                return new FailedObjCollection(Children.Where(t => t.Type == AveConstants.TYPE_LISTITEM.ToString()));
            }
        }

        public FailedObjCollection Folders
        {
            get
            {
                return new FailedObjCollection(Children.Where(t => t.Type == AveConstants.TYPE_FOLDER.ToString()));
            }
        }

        public FailedObjCollection PassFolders  //路过folder集合，IB query过程中不能将有嵌套的folder直接remove掉，所以需要一个临时集合，在IB query中确认过的不需要后续再重新确认
        {
            get
            {
                if (mTempFolders == null)
                {
                    mTempFolders = new FailedObjCollection(Children.Where(t => t.Type == AveConstants.TYPE_FOLDER.ToString()));
                }
                return mTempFolders;
            }
        }

        public FailedObjectBase Parent { get; private set; }

        public void AddChild(FailedObjectBase failedObj)
        {
            if (Children.Contains(failedObj.Name))
            {
                return;
            }
            failedObj.Parent = this;
            Children.Add(failedObj);
        }

        public void AppendChild(FailedObjectBase obj)
        {
            try
            {
                lock (lockObj)
                {
                    Queue<FailedObjectBase> queue = FailedCache.GetContainerStructure(obj);
                    queue.Enqueue(obj);
                    AppendChild(queue);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("append obj failed. obj name:{0}, obj type:{1}, error message:{2}", obj.Name, obj.Type, e.ToString());
            }
        }

        public void AppendChild(Queue<FailedObjectBase> queue)
        {
            while (queue.Count > 0)
            {
                FailedObjectBase temp = queue.Dequeue();
                FailedObjectBase obj;
                if (Children.TryGetChild(temp.Name, out obj))
                {
                    temp = obj;
                }
                else
                {
                    AddChild(temp);
                }
                temp.AppendChild(queue);
            }
        }

        public FailedObjectBase GetChild(string name)
        {
            return Children.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public FailedObjectBase GetParentByMD5(string parentPathMD5)
        {
            FailedObjectBase parent = Children.FirstOrDefault(o => string.Equals(o.PathMD5, parentPathMD5, StringComparison.OrdinalIgnoreCase));
            if (parent != null)
            {
                return parent;
            }
            else
            {
                foreach (FailedObjectBase child in Children )
                {
                    parent = child.GetParentByMD5(parentPathMD5);
                    if (parent !=null)
                    {
                        return parent;
                    }
                }
            }
            return null;
        }
    }

    public class FailedObjCollection : List<FailedObjectBase>
    {
        public FailedObjCollection()
        { }

        public FailedObjCollection(IEnumerable<FailedObjectBase> list) : base(list)
        { }

        public bool Contains(string name)
        {
            return this.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool TryGetChild(string name, out FailedObjectBase obj)
        {
            obj = null;
            if (this.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                obj = this.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetChildById(string id, out FailedObjectBase obj)
        {
            obj = null;
            if (this.Any(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                obj = this.FirstOrDefault(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FailedCache
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static List<FailedObjectBase> mFolderCache = new List<FailedObjectBase>();
        public static FailedObjectBase Root;
        private static FailedObjectBase WebObj;
        private static FailedObjectBase ListObj;

        public static void SwitchList(FailedObjectBase listObj)
        {
            ListObj = listObj;
        }

        public static void SwitchWeb(FailedObjectBase webObj)
        {
            WebObj = webObj;
        }

        public static string GetParentPath(string path)
        {
            var index = path.TrimEnd('/').LastIndexOf('/');
            string parentFullPath = null;
            if (index > 0)
            {
                parentFullPath = path.Substring(0, index);
            }
            else
            {
                throw new System.NotSupportedException(string.Format("The parent path of full path:{0} is invalid.", path));
            }
            return parentFullPath;
        }

        /// <summary>
        /// 存储路过的folder结构
        /// </summary>
        /// <param name="folderObj"></param>
        public static void AddFolder(FailedObjectBase folderObj)
        {
            if (mFolderCache.Count > 0)
            {
                var parentFullPath = folderObj.Path;

                var index = parentFullPath.LastIndexOf('/');

                if (index > 0)
                {
                    parentFullPath = parentFullPath.Substring(0, index);
                }
                else
                {
                    throw new System.NotSupportedException(string.Format("The full path:{0} is invalid.", parentFullPath));
                }

                while (mFolderCache.Count > 0)
                {
                    var lastPath = mFolderCache[mFolderCache.Count - 1].Path;

                    if (lastPath.Equals(parentFullPath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    mFolderCache.RemoveAt(mFolderCache.Count - 1);
                }
            }

            mFolderCache.Add(folderObj);
        }

        /// <summary>
        /// 获取item的parent folder结构
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Queue<FailedObjectBase> GetContainerStructure(FailedObjectBase current)
        {
            string type = current.Type;
            string parentpath = current.Path;
            if (type == AveConstants.TYPE_FOLDER.ToString())
            {
                parentpath = GetParentPath(current.Path);
            }

            Queue<FailedObjectBase> temp = new Queue<FailedObjectBase>();
            if (type != AveConstants.TYPE_WEB.ToString())
            {
                temp.Enqueue(WebObj);
            }
            if (type == AveConstants.TYPE_FOLDER.ToString() || type == AveConstants.TYPE_DOCUMENT.ToString() || type == AveConstants.TYPE_LISTITEM.ToString())
            {
                temp.Enqueue(ListObj);
                foreach (FailedObjectBase obj in mFolderCache)
                {
                    if (type == AveConstants.TYPE_FOLDER.ToString() 
                        && !string.Equals(obj.Path, parentpath, StringComparison.OrdinalIgnoreCase)
                        && obj.Path.StartsWith(parentpath, StringComparison.OrdinalIgnoreCase))
                    {
                        log.Info("Skip node {0} while generate structure for node {1}",obj.Path,current.Path);
                        continue;
                    }
                    temp.Enqueue(obj);
                }
            }
            return temp;
        }

        public static void Remove()
        {
            if (mFolderCache.Count > 0)
            {
                mFolderCache.RemoveAt(mFolderCache.Count - 1);
            }
        }

        public static void Clear()
        {
            mFolderCache.Clear();
        }
    }
}
