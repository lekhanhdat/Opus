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

namespace AvePoint.Media.Storage
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    #endregion

    /// <summary>
    /// 包含device验证信息的一个对象, 用于直接操作Directory, File, Stream等, 层次上对应于一个Physical Device.
    /// </summary>
    public interface IXSystem : IDisposable
    {
        #region public properties

        /// <summary>
        /// 以字符串形式显示System类型,如FS的type为"FSSystem"
        /// </summary>
        /// <value>The type.</value>
        String Type { get; }

        /// <summary>
        /// 系统id(physical device id)
        /// </summary>
        String SystemID { get; }

        /// <summary>
        /// 系统name(physical device name)
        /// </summary>
        String SystemName { get; }

        /// <summary>
        /// 根据device的配置信息，返回一个key,
        /// 用来判断不同的system的配置信息是否相同.
        /// </summary>
        String SystemKey { get; }

        /// <summary>
        /// 系统location
        /// FSSYstem:本地路径或网络路径
        /// Others: URL或者IP.....
        /// </summary>
        String SystemLocation { get; }

        /// <summary>
        /// 当前Directory.
        /// </summary>
        //XDirectoryInfo CurrentDirectory { get; }

        /// <summary>
        /// 表示当前介质的健康状况，是否能够访问。是真实介质状态
        /// </summary>
        XSystemHealth SystemHealth { get; set; }

        /// <summary>
        /// 当前系统是否在线，是DocAve系统的状态
        /// </summary>
        XSystemStatus SystemStatus { get; set; }

        /// <summary>
        /// 当前系统用途，存放all/data/index
        /// </summary>
        XSystemUsage SystemUsage { get; }

        /// <summary>
        /// 是否可直接访问系统, 主要是约定可用于绝对直接读写的System。
        /// 如果是FS location则为true;其他类型的location都为false
        /// </summary>
        Boolean IsDirectSystem { get; }

        /// <summary>
        /// System总容量, 单位byte
        /// </summary>
        UInt64 TotalSpace { get; }

        /// <summary>
        /// System已经使用的空间, 单位byte
        /// </summary>
        UInt64 TotalUsedSpace { get; }

        /// <summary>
        /// System总的剩余空间, 单位byte
        /// </summary>
        UInt64 TotalFreeSpace { get; }

        /// <summary>
        /// 系统是否达到了设置的容量阀值，如果达到了，就表示满了，不能再向其中写数据
        /// </summary>
        Boolean IsFull { get; }

        /// <summary>
        /// device实际可用的磁盘空间-配置physical device时设置的阈值
        /// </summary>
        UInt64 AvailableSpace { get; }

        /// <summary>
        /// 属性集合，可以用来保存定制的属性信息
        /// </summary>
        Hashtable Properties { get; }

        /// <summary>
        /// 介质上所存文件的形式: FileLevel or BlockLevel，主要是media使用来确定哪种数据格式
        /// </summary>
        FileBlockType SupportedFileType { get; set; }

        /// <summary>
        /// Storage device的类型: namespace（有文件夹层次结构的）或object（一个文件对应一个id，不能一层层list的）
        /// namespace 类型有netshare，netapp, FTP，TSM, amazon s3, azure, at&t, 
        /// object 类型有 Box, EMC cetera, Dell DX, google drive, one drive
        /// </summary>
        StorageInterfaceType StorageInterfaceType { get; }

        /// <summary>
        /// 介质属性
        /// </summary>
        XRI XriObject { get; set; }

        /// <summary>
        /// 定义查找可用XSystem的条件, 支持自定义，主要在使用XLibrary对象时有意义，找出一组physical device中符合自己需求的device
        /// </summary>
        Predicate<IXSystem> FindCondition { set; get; }

        /// <summary>
        /// 介质是否支持自动切换数据块,true为支持;false为不支持
        /// </summary>
        Boolean IsSupportAutoChangeDataBlock { set; get; }

        /// <summary>
        /// 介质是否支持检查device剩余空间,true为支持;false为不支持
        /// 目前只有TSM device会返回false
        /// </summary>
        Boolean IsSupportAutoCheck { get; }

        #endregion

        #region common function

        /// <summary>
        /// 在调用open方法时，我们会对device的配置进行验证，同时会返回一个StorageOpenValidResult
        /// 该对象中包含了device的一些信息，如果剩余空间等。
        /// </summary>
        /// <returns>打开一个介质的返回结果</returns>
        StorageOpenValidResult Open();

        /// <summary>
        /// 初始化IXSystem
        /// </summary>
        /// <param name="featureCustomized">
        /// 有些功能对特殊的介质需要定制特性.比如Extender跟Hold数据存在EMC的Centera上时是一个Blob一个Clip.而其他功能像Item则是多个Blob在同一个Clip.对应该的上层逻辑是有要求的,如果是新的功能需要用到Storage API,请联系API Developer确认相关注意
        /// </param>
        /// <returns>打开一个介质的返回结果</returns>
        StorageOpenValidResult Open(FeatureCustomized featureCustomized);

        /// <summary>
        /// 将windows不支持的长路径通过symbollink的方式转换为短路径
        /// </summary>
        /// <param name="symlinkPath">转换后的路径</param>
        /// <param name="targetPath">原路径</param>
        /// <returns>If the function succeeds, the return value is nonzero.If the function fails, the return value is zero.</returns>
        Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath);

        /// <summary>
        /// 如果需要在运行时验证已经存在的System。可以调用这个方法。如果device存在异常，返回结果的Message属性会记录异常具体原因
        /// （例如device连不上，参数有问题，用户名密码不对等等），不会throw exception，
        /// </summary>
        /// <returns>该device的读写性,以及device的相关信息</returns>
        StorageOpenValidResult Validate();

        /// <summary>
        /// 在System调用完成之后，调用该方法，进行一些回收操作。
        /// google drive 这里不需要释放资源
        /// TSM 这个方法中会关闭之前打开的所有session
        /// </summary>
        void Close();

        /// <summary>
        /// 该方法作用于Object类型介质,将上传的数据块返回的object id插入到对应的index记录中。目前只是给media service使用
        /// 非object类型device，直接返回 result.NeedCommit = true;
        /// </summary>
        /// <typeparam name="T">index的类型</typeparam>
        /// <param name="indexList">需要插入的index集合</param>
        /// <param name="result">上传数据返回的结果</param>
        /// <param name="propertyInfo">文件的属性信息</param>
        void MergeStorageInfo<T>(List<T> indexList, StorageResult result, PropertyInfo propertyInfo);

        /// <summary>
        /// 打开指定路径上的 FileStream，具有读/写访问权限。
        /// </summary>
        /// <param name="info">表示文件相关信息的一个对象</param>
        /// <param name="fileMode">FileMode 值，用于指定在文件不存在时是否创建该文件，并确定是保留还是覆盖现有文件的内容。</param>
        /// <returns>类型：AvePoint.Media.Storage.XStream以指定模式打开的指定路径上的FileStream</returns>
        /// <Exceptions cref="AuthenticationFailedException">  
        ///   AuthenticationFailedException
        ///     The access requested is not permitted by the operating system for the specified
        ///     path, such as when access is Write or ReadWrite and the file or directory
        ///     is set for read-only access.
        /// </Exceptions>
        /// <Exceptions cref="System.NotSupportedException">
        /// 如果是BOX device，file mode参数传append或者truncate会抛出此异常</Exceptions>
        XStream OpenStream(StorageInfo info, FileMode fileMode);

        /// <summary>
        /// 这个文件主要是用于上传，目前还不支持续写。
        /// </summary>
        /// <param name="commitStream">当前要上传到device上的数据流</param>
        /// <param name="info">当前要上传的文件的相关信息</param>
        /// <returns>上传结果</returns>
        /// <Exceptions cref="NotEnoughFreeSpaceException">抛出NotEnoughFreeSpaceException异常，目的端磁盘空间不足</Exceptions>
        StorageResult CommitStream(Stream commitStream, StorageInfo info);

        /// <summary>
        /// 在想要获取一个Directory的相关信息调用该方法。
        /// 在Directory不存在时，我们会根据mode判断是不是要进行创建操作。
        /// google drive 没有实现这个方法，不能创建文件夹
        /// </summary>
        /// <param name="dirInfo">文件夹的路径</param>
        /// <param name="mode">用于指定在文件夹不存在时是否创建该文件夹。</param>
        /// <returns>当前文件夹的相关信息</returns>
        /// <exception cref="MethodNotSupportForReadOnlyDeviceException">如果磁盘是只读的抛出MethodNotSupportForReadOnlyDeviceException异常</exception>
        XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode);

        /// <summary>
        /// 获取一个文件的相关信息，主要包含文件大小
        /// </summary>
        /// <param name="dirInfo">文件的路径</param>
        /// <returns>如果文件不存在返回null;如果文件存在则返回文件相关信息</returns>
        /// <exception cref="MethodNotSupportForReadOnlyDeviceException">如果是readonlydevice 抛异常MethodNotSupportForReadOnlyDeviceException异常</exception>
        XFileInfo OpenFile(StorageInfo fileInfo);

        /// <summary>
        /// 删除整个文件夹（包括子文件夹）及其下面的文件，如果实例化的是XLibrary对象，此方法会遍历所有physical device，删除每个physical device上的这个文件夹
        /// TSM会删除所有highname以参数开头的文件。
        /// google drive 不会进行真实的删除操作
        /// </summary>
        /// <param name="info">要删除的文件夹的位置</param>
        /// <returns>删除的结果，其中DeletedFileSize属性会返回删除数据的总大小</returns>
        StorageDeleteResult DeleteDirectory(StorageInfo info);

        /// <summary>
        /// 删除指定的文件，，如果实例化的是XLibrary对象，此方法会遍历所有physical device，删除每个physical device上的这个文件
        /// </summary>
        /// <param name="info">要删除的文件的所在位置和文件名的信息</param>
        /// <returns>删除的结果，其中DeletedFileSize属性会返回删除文件的大小</returns>
        StorageDeleteResult DeleteFile(StorageInfo info);

        /// <summary>
        /// 获取指定的文件夹下的所有子文件夹,仅向下展开一层。该方法通过会首先判断使用Client类型(FSClient或者AlphaFSClient)
        /// 如果使用FSClient中遇到长路径，会自动跳转出并使用AlphaFSClient继续获取文件夹信息
        /// 如果AlphaFSClient获取文件夹信息时出现异常，异常被Catch并输出错误Log，方法继续获取下一个文件夹信息
        /// google drive 没有实现此方法，无法list 文件夹
        /// </summary>
        /// <param name="dirInfo">要获取的文件夹信息的父层文件夹</param>
        /// <returns>需要获取的文件夹信息集合。如果有长路径文件夹，在获取文件夹属性时异常，则该文件夹被跳过，继续下一个文件夹，可能出现获取文件夹数与实际数量不一致的情况</returns>
        List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo);

        /// <summary>
        /// 获取指定的文件夹下的所有文件,仅向下展开一层。该方法通过会首先判断使用Client类型(FSClient或者AlphaFSClient)
        /// 如果使用FSClient中遇到长路径，会自动跳转出并使用AlphaFSClient继续获取文件信息
        /// 如果AlphaFSClient获取文件信息时出现异常，异常被Catch并输出错误Log，方法继续获取下一个文件信息
        /// google drive 没有实现此方法，无法list 文件
        /// </summary>
        /// <param name="dirInfo">要获取的文件信息的父层文件夹</param>
        /// <returns>
        /// 需要获取的文件信息集合。如果有长路径文件，在获取文件属性时异常，则该文件被跳过，继续下一个文件，可能出现获取文件数与实际数量不一致的情况
        /// </returns>
        List<XFileInfo> ListFiles(StorageInfo dirInfo);
        
        IEnumerable<List<XFileInfo>> GetFilesInBatch(StorageInfo dirInfo, int batchSize);

        IEnumerable<List<XDirectoryInfo>> GetDirectoriesInBatch(StorageInfo dirInfo, int batchSize);

        /// <summary>
        /// 获取指定的文件夹下的文件和子文件夹，仅往下展开一层。该方法通过会首先判断使用Client类型(FSClient或者AlphaFSClient)
        /// 如果使用FSClient中遇到长路径，会自动跳转出并使用AlphaFSClient继续获取文件信息
        /// 如果AlphaFSClient获取文件或文件夹信息时出现异常，异常被Catch并输出错误Log，方法继续获取下一个文件或文件夹信息
        /// google drive 没有实现此方法，无法list 文件和文件夹
        /// </summary>
        /// <param name="dirInfo">要获取的文件以及文件夹信息的父层文件夹</param>
        /// <returns>包含需要获取的文件信息集合以及文件夹信息集合的对象实例。如果有长路径文件，在获取文件属性时异常，则该文件被跳过，继续下一个文件，可能出现获取文件数与实际数量不一致的情况</returns>
        StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo);


        /// <summary>
        ///  该方法目前仅在Amazon，Atmos、Azure、RackSpace类型Device中实现，带有缓冲策略的获取指定的文件夹下的文件和子文件夹，仅往下展开一层
        /// </summary>
        /// <param name="dirInfo">要获取的文件以及文件夹信息的父层文件夹</param>
        /// <returns>包含需要获取的文件信息集合以及文件夹信息集合的对象实例</returns>
        StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo);

        /// <summary>
        /// 判断指定文件夹是否存在，如果运行中出现异常，则直接throw
        /// google drive没有实现此方法
        /// </summary>
        /// <param name="info">文件夹信息，包括HighName、LowName</param>
        /// <returns>True or False</returns>
        Boolean DirectoryExists(StorageInfo info);

        /// <summary>
        /// 单纯判断文件是否存在，如果运行中出现异常，则直接throw
        /// </summary>
        /// <param name="info">文件夹，包括HighName、LowName</param>
        /// <returns>True or False</returns>
        Boolean FileExists(StorageInfo info);

        /// <summary>
        /// 将指定的文件复制到目标文件夹下（同一个system之内）。
        /// google drive没有实现此方法
        /// </summary>
        /// <param name="sourceFileInfo"></param>
        /// <param name="targetFileInfo"></param>
        /// <param name="isOverWrite"></param>
        /// <returns>StorageCopyResult中IsCopied属性判断是否Copy完全成功,Copy过程出错不抛异常,Message属性中包含异常信息</returns>
        StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        /// <summary>
        /// 将指定的文件剪切到目标文件夹下。
        /// google drive 和 TSM 没有实现此方法
        /// </summary>
        /// <param name="sourceFileInfo"></param>
        /// <param name="targetFileInfo"></param>
        /// <param name="isOverWrite"></param>
        /// <returns>StorageMoveResult中IsMoved属性判断是否Move完全成功,Move过程中出错不抛异常,Message属性中包含失败原因</returns>
        StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        /// <summary>
        /// 将指定的文件夹及其下面的内容剪切到目标文件夹下。
        /// google drive 和 TSM 没有实现此方法
        /// </summary>
        /// <param name="sourceDirInfo"></param>
        /// <param name="targetDirInfo"></param>
        /// <param name="isOverWrite"></param>
        /// <returns>StorageMoveResult中IsMove属性判断是否Move完全成功,或其他System Exception</returns>
        StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite);

        /// <summary>
        /// 将指定的文件夹及其下面的内容复制到目标文件夹下（不同system之间）。
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="destSystem"></param>
        /// <param name="destFile"></param>
        /// <param name="isOverWrite"></param>
        /// <returns>StorageMoveResult中IsCopy属性判断是否Copy完全成功,或其他System Exception</returns>
        StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite);

        /// <summary>
        /// Move file from one system to another system.（目的端和源端使用相同的文件名，文件路径）
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="destSystem"></param>
        /// <returns>StorageMoveResult中IsMove属性判断是否Move完全成功,当源端目的端不是同一个Drive时,Copy到目的端失败时抛出Exception,或其他System Exception</returns>
        StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem);

        /// <summary>
        /// Move file from one system to another system.（目的端可以使用不同的文件名，文件路径）
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="destSystem"></param>
        /// <param name="destFile"></param>
        /// <returns>StorageMoveResult中IsMove属性判断是否Move完全成功,当源端目的端不是同一个Drive,Copy到目的端失败时抛出Exception,或其他System Exception</returns>
        StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile);

        /// <summary>
        /// 此方法用于获取对数据真实写入的Devcie的描述信息
        /// </summary>
        /// <returns>FileMode是Open的时候返回null</returns>
        List<String> GetUsedSystemsDuringWritten();

        /// <summary>
        /// 此方法用于获取包含被删除数据的Devcie的描述信息
        /// </summary>
        /// <returns>DeleteFile或者DeleteDirectory未成功删除时返回null</returns>
        List<String> GetUsedSystemsDuringDeletion();

        #region 计算netshare吞吐量及IOPS
        /// <summary>
        /// 获取吞吐量及IOPS, only for Netshare
        /// </summary>
        /// <param name="type">IO type. Random or Sequential</param>
        /// <param name="WriteRatio">Percentage of write requests to issue (default = 0, 100% read).</param>
        /// <param name="BlokeSize">Block size in bytes or KiB, MiB, or GiB (default = 64K)</param>
        /// <returns></returns>
        XPerformanceResult GetDevicePerformance(IOType type, int writeRatio = 0, string blokeSize = "64k");
        #endregion

        #region box 专属
        /// <summary>
        /// 此方法用于给文件创建分享链接，暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件的StorageInfo信息</param>
        /// <param name="accessMode">分享的范围权限，Open表示所有人，Company表示公司用户，Collaborators表示合作者</param>
        /// <param name="canDownload">分享文件是否支持下载</param>
        /// <returns>返回开启分享链接功能的一个文件的相关信息</returns>
        XFileInfo CreateFileSharedLink(StorageInfo info, AcessMode accessMode, Boolean canDownload);

        /// <summary>
        /// 此方法用于给文件夹创建分享链接，暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件夹的StorageInfo信息</param>
        /// <param name="accessMode">分享的范围权限，Open表示所有人，Company表示公司用户，Collaborators表示合作者</param>
        /// <param name="canDownload">分享文件夹是否支持下载</param>
        /// <returns>返回开启分享链接功能的一个文件夹的相关信息</returns>
        XDirectoryInfo CreateFolderSharedLink(StorageInfo info, AcessMode accessMode, Boolean canDownload);

        /// <summary>
        /// 此方法用于给关闭文件分享链接功能，暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件的StorageInfo信息</param>
        /// <returns>返回被关闭分享链接功能的一个文件的相关信息</returns>
        XFileInfo DisableFileSharedLink(StorageInfo info);

        /// <summary>
        /// 此方法用于给关闭文件分享链接功能，暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件的StorageInfo信息</param>
        /// <returns>返回被关闭分享链接功能的一个文件夹的相关信息</returns>
        XDirectoryInfo DisableFolderSharedLink(StorageInfo info);

        /// <summary>
        /// 此方法用于Lock文件，使文件不能再被操作，暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件的StorageInfo信息</param>
        /// <returns>如果Lock文件成功返回True，否则抛异常</returns>
        Boolean LockFile(StorageInfo info);

        /// <summary>
        /// 此方法用于解锁被Lock的文件暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件的StorageInfo信息</param>
        /// <returns>如果Unlock文件成功返回True，否则抛异常</returns>
        Boolean UnlockFile(StorageInfo info);

        /// <summary>
        /// 此方法用于获取一个File的相关信息，并返回包含Tags属性的的文件信息，暂时只有Box支持
        /// </summary>
        /// <param name="info">要操作文件的StorageInfo信息</param>
        /// <returns>返回包含Tags属性的的文件信息</returns>
        XFileInfo OpenFileWithTags(StorageInfo info);

        #endregion

        #endregion
    }
}
