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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using Microsoft365.Authentication;

namespace Microsoft.SharePoint.Client
{
    public static class ListExtension
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(ListExtension));

        public delegate void ItemsRetrieving(int currentCount, int totalCount);

        private static string queryIdXml = "<View Scope=\"RecursiveAll\"><Query><OrderBy Override=\"TRUE\"><FieldRef Name=\"ID\" /></OrderBy></Query><QueryOptions><QueryThrottleMode>Override</QueryThrottleMode></QueryOptions><RowLimit>{0}</RowLimit></View>";

        public static SPOFolder LoadAllItemIds(this List list, int rowLimit, ItemsRetrieving itemsRetrieving)
        {
            list.Context.Load(list, (List l) => l.RootFolder.ServerRelativeUrl, (List l) => l.ItemCount);
            list.Context.ExecuteQuery();
            ContentIterator contentIterator = new ContentIterator(list.Context);
            SPOFolder rootFolder = SPOFolder.BuildRootFolder(new(), new(), list.RootFolder.ServerRelativeUrl);
            int currentCount = 0;
            contentIterator.ProcessListItems(list, new CamlQuery
            {
                ViewXml = string.Format(queryIdXml, rowLimit)
            }, delegate (ListItemCollection items)
            {
                items.RetrieveItems().Retrieve("Id", "FileRef", "FileLeafRef", "FileSystemObjectType");
            }, delegate (ListItemCollection items)
            {
                if (itemsRetrieving != null)
                {
                    currentCount += items.Count;
                    itemsRetrieving(currentCount, list.ItemCount);
                }

                AnalyzeListItems(items, rootFolder);
            }, (ListItemCollection items, Exception ex) => true);

            return rootFolder;
        }

        /// <summary>
        /// 拼接Folder/Item结构
        /// </summary>
        /// <param name="items"></param>
        /// <param name="rootFolder"></param>
        private static void AnalyzeListItems(ListItemCollection items, SPOFolder rootFolder)
        {
            foreach (var item in items)
            {
                var serverRelativeUrl = (string)item.FieldValues["FileRef"];
                var name = (string)item.FieldValues["FileLeafRef"];

                var parentFolder = rootFolder;
                var frUrl = serverRelativeUrl.Substring(rootFolder.Name.Length, serverRelativeUrl.Length - rootFolder.Name.Length - name.Length - 1);
                mLogger.Info($"AnalyzeListItems. ObjectId:{item.Id}.ObjectServerRelativeUrl:{frUrl}.");
                var parentFoldersName = frUrl.Split(new String[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parentFoldersName.Length; i++)
                {
                    var folderName = parentFoldersName[i];
                    SPOFolder tempFolder = parentFolder.SubFolders.GetByName(folderName);

                    if (tempFolder == null)
                    {
                        tempFolder = SPOFolder.BuildUnRootFolder(parentFolder, folderName, -1);
                        parentFolder.SubFolders.Add(tempFolder);
                    }
                    parentFolder = tempFolder;
                }

                var id = item.Id;
                if (item.FileSystemObjectType == FileSystemObjectType.File)
                {
                    var spoItem = new SPOItem()
                    {
                        Id = id,
                        Name = name
                    };
                    parentFolder.Items.Add(spoItem);
                }
                else
                {
                    var spoFolder = parentFolder.SubFolders.GetByName(name);
                    if (spoFolder == null)
                    {
                        spoFolder = SPOFolder.BuildUnRootFolder(parentFolder, name, id);
                        parentFolder.SubFolders.Add(spoFolder);
                    }
                    else
                    {
                        spoFolder.Id = id;
                    }
                }
            }
        }

    }

    public class ContentIterator
    {
        public delegate void ItemsProcessor(ListItemCollection items);

        public delegate void ItemsRetriever(ListItemCollection items);

        public delegate bool ItemsProcessorErrorCallout(ListItemCollection items, Exception e);

        private readonly ClientRuntimeContext _context;

        public ContentIterator(ClientRuntimeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _context = context;
        }

        public void ProcessListItems(List list, CamlQuery camlQuery, ItemsRetriever itemsRetriever, ItemsProcessor itemsProcessor, ItemsProcessorErrorCallout errorCallout)
        {
            ListItemCollectionPosition listItemCollectionPosition2 = (camlQuery.ListItemCollectionPosition = null);
            Dictionary<long, ObjectPath> dictionary = new Dictionary<long, ObjectPath>(_context.ReadObjectPaths());
            while (true)
            {
                ListItemCollection items2 = list.GetItems(camlQuery);
                _context.Load(items2, (ListItemCollection items) => items.ListItemCollectionPosition, (ListItemCollection items) => items.Include((ListItem item) => item.Id));
                itemsRetriever?.Invoke(items2);
                _context.ExecuteQuery();
                try
                {
                    itemsProcessor(items2);
                }
                catch (Exception e)
                {
                    if (errorCallout == null || errorCallout(items2, e))
                    {
                        throw;
                    }
                }

                if (items2.ListItemCollectionPosition == null)
                {
                    break;
                }

                string pagingInfo = items2.ListItemCollectionPosition.PagingInfo;
                string[] array = pagingInfo.Split(new char[1] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> list2 = new List<string>();
                string[] array2 = array;
                foreach (string text in array2)
                {
                    if (text.Contains("Paged=") || text.Contains("p_ID="))
                    {
                        list2.Add(text);
                    }
                }

                pagingInfo = string.Join("&", list2.ToArray());
                items2.ListItemCollectionPosition.PagingInfo = pagingInfo;
                camlQuery.ListItemCollectionPosition = items2.ListItemCollectionPosition;
                _context.WriteObjectPaths(new Dictionary<long, ObjectPath>(dictionary));
            }
        }
    }

    public static class ClientContextExtension
    {
        private static Func<ClientRuntimeContext, Dictionary<long, ObjectPath>> readContextObjectPaths;

        private static Action<ClientRuntimeContext, Dictionary<long, ObjectPath>> writeContextObjectPaths;

        public static Dictionary<long, ObjectPath> ReadObjectPaths(this ClientRuntimeContext context)
        {
            if (readContextObjectPaths == null)
            {
                readContextObjectPaths = TypeInvoker.CreateGetter<ClientRuntimeContext, Dictionary<long, ObjectPath>>(typeof(ClientRuntimeContext).GetField("m_objectPaths", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
            }

            return readContextObjectPaths(context);
        }

        public static void WriteObjectPaths(this ClientRuntimeContext context, Dictionary<long, ObjectPath> value)
        {
            if (writeContextObjectPaths == null)
            {
                writeContextObjectPaths = TypeInvoker.CreateSetter<ClientRuntimeContext, Dictionary<long, ObjectPath>>(typeof(ClientRuntimeContext).GetField("m_objectPaths", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
            }

            writeContextObjectPaths(context, value);
        }
    }

    internal static class TypeInvoker
    {
        public static Func<TObjType, TValueType> CreateGetter<TObjType, TValueType>(FieldInfo field)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(field.ReflectedType.FullName + ".get_" + field.Name, typeof(TValueType), new Type[1] { typeof(TObjType) }, restrictedSkipVisibility: true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, field);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<TObjType, TValueType>)dynamicMethod.CreateDelegate(typeof(Func<TObjType, TValueType>));
        }

        public static Action<TObjType, TValueType> CreateSetter<TObjType, TValueType>(FieldInfo field)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(field.ReflectedType.FullName + ".set_" + field.Name, null, new Type[2]
            {
                typeof(TObjType),
                typeof(TValueType)
            }, restrictedSkipVisibility: true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Stfld, field);
            iLGenerator.Emit(OpCodes.Ret);
            return (Action<TObjType, TValueType>)dynamicMethod.CreateDelegate(typeof(Action<TObjType, TValueType>));
        }

        public static Func<TInstanceType> CreateObjInstance<TInstanceType>(ConstructorInfo ctorInfo)
        {
            Type typeFromHandle = typeof(TInstanceType);
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeFromHandle, Type.EmptyTypes, restrictedSkipVisibility: true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.DeclareLocal(typeFromHandle);
            iLGenerator.Emit(OpCodes.Newobj, ctorInfo);
            iLGenerator.Emit(OpCodes.Stloc_0);
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<TInstanceType>)dynamicMethod.CreateDelegate(typeof(Func<TInstanceType>));
        }

        public static Func<TArguType, TInstanceType> CreateObjInstance<TArguType, TInstanceType>(ConstructorInfo ctorInfo)
        {
            Type typeFromHandle = typeof(TInstanceType);
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeFromHandle, new Type[1] { typeof(TArguType) }, restrictedSkipVisibility: true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.DeclareLocal(typeFromHandle);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Newobj, ctorInfo);
            iLGenerator.Emit(OpCodes.Stloc_0);
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<TArguType, TInstanceType>)dynamicMethod.CreateDelegate(typeof(Func<TArguType, TInstanceType>));
        }

        public static Func<TArguType1, TArguType2, TInstanceType> CreateObjInstance<TArguType1, TArguType2, TInstanceType>(ConstructorInfo ctorInfo)
        {
            Type typeFromHandle = typeof(TInstanceType);
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeFromHandle, new Type[2]
            {
                typeof(TArguType1),
                typeof(TArguType2)
            }, restrictedSkipVisibility: true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.DeclareLocal(typeFromHandle);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Newobj, ctorInfo);
            iLGenerator.Emit(OpCodes.Stloc_0);
            iLGenerator.Emit(OpCodes.Ldloc_0);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<TArguType1, TArguType2, TInstanceType>)dynamicMethod.CreateDelegate(typeof(Func<TArguType1, TArguType2, TInstanceType>));
        }
    }
}