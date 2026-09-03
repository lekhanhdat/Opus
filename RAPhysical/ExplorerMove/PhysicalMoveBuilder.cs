using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.API;
using System;
using System.Collections.Generic;

public class PhysicalMoveBuilder
{
    private readonly Dictionary<Guid, Record> _boxCache = new Dictionary<Guid, Record>();
    private readonly Dictionary<Guid, Record> _folderCache = new Dictionary<Guid, Record>();

    private readonly IExplorerDao _explorerDao;

    public PhysicalMoveBuilder(IExplorerDao explorerDao)
    {
        _explorerDao = explorerDao;
    }

    public string BuildDestinationPath(Guid destinationLocatinId,Guid destinationBoxId,Guid destinationFolderId)
    {
        var destinationPath = new PhysicalLocation(destinationLocatinId).DirPath;
        destinationPath = JobReportUtility.ReplaceRootLocationName(destinationPath);
        if (destinationBoxId != Guid.Empty)
        {
            if (!_boxCache.TryGetValue(destinationBoxId, out var box))
            {
                box = _explorerDao.GetPhysicalRecordById(destinationBoxId);
                _boxCache.TryAdd(destinationBoxId, box);
            }

            if (!string.IsNullOrEmpty(box?.LeafName))
            {
                destinationPath += "/" + box.LeafName;
            }
        }

        if (destinationFolderId != Guid.Empty)
        {
            if (!_folderCache.TryGetValue(destinationFolderId, out var folder))
            {
                folder = _explorerDao.GetPhysicalRecordById(destinationFolderId);
                _folderCache.TryAdd(destinationFolderId, folder);
            }

            if (!string.IsNullOrEmpty(folder?.LeafName))
            {
                destinationPath += "/" + folder.LeafName;
            }
        }

        return destinationPath;
    }
}