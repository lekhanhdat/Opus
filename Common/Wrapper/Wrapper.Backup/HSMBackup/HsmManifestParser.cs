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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

#nullable enable

namespace HsmBackup.Shared
{
    public static class HsmBackupLoader
    {
        private const string ManifestFileName = "Manifest.xml";

        public static HsmBackupDiagnostics TestPackage(string packageFolder)
        {
            if (string.IsNullOrWhiteSpace(packageFolder))
            {
                throw new ArgumentException("Package folder path is required.", nameof(packageFolder));
            }

            var diagnostics = new HsmBackupDiagnostics();
            if (!Directory.Exists(packageFolder))
            {
                diagnostics.Warnings.Add($"Package folder not found: {packageFolder}.");
                return diagnostics;
            }

            var manifestPath = Path.Combine(packageFolder, "MetaData", ManifestFileName);
            diagnostics.XmlLoaded[ManifestFileName] = File.Exists(manifestPath);
            if (!diagnostics.XmlLoaded[ManifestFileName])
            {
                diagnostics.Warnings.Add($"Manifest file missing at {manifestPath}.");
                return diagnostics;
            }

            try
            {
                var files = HsmManifestParser.FindFilesByWebAndListId(manifestPath, Guid.Empty, Guid.Empty);
                diagnostics.ManifestFileCount = files.Count;

                var listItemCount = files
                    .Select(f => f.ListItem)
                    .Where(li => li != null)
                    .Select(li => li!.DocId ?? li!.Id ?? Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .Count();

                if (listItemCount == 0)
                {
                    listItemCount = HsmManifestParser.FindListItemsByWebAndListId(manifestPath, Guid.Empty, Guid.Empty).Count;
                }

                diagnostics.ManifestListItemCount = listItemCount;
            }
            catch (Exception ex)
            {
                diagnostics.Warnings.Add($"Failed to parse manifest: {ex.Message}");
            }

            return diagnostics;
        }
    }

    public sealed class HsmBackupDiagnostics
    {
        public Dictionary<string, bool> XmlLoaded { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Warnings { get; } = new();
        public int ManifestFileCount { get; internal set; }
        public int ManifestListItemCount { get; internal set; }
    }

    public static class HsmManifestParser
    {
        private const string ObjectTypeFile = "SPFile";
        private const string ObjectTypeListItem = "SPListItem";
        private static readonly XNamespace ManifestNamespace = "urn:deployment-manifest-schema";
        internal static readonly IReadOnlyDictionary<string, HsmManifestField> EmptyFields =
            new ReadOnlyDictionary<string, HsmManifestField>(new Dictionary<string, HsmManifestField>(0, StringComparer.OrdinalIgnoreCase));

        public static List<HsmManifestFile> FindFilesByWebAndListId(string manifestPath, Guid webId, Guid listId)
        {
            return EnumerateFilesByWebAndListId(manifestPath, webId, listId).ToList();
        }

        public static IEnumerable<HsmManifestFile> EnumerateFilesByWebAndListId(string manifestPath, Guid webId, Guid listId)
        {
            var normalizedManifestPath = EnsureManifestPath(manifestPath);
            var pendingFilesByGuid = new Dictionary<Guid, HsmManifestFile>();
            var pendingFilesByComposite = new Dictionary<(Guid ListId, int ItemId), HsmManifestFile>();
            var pendingListItemsByGuid = new Dictionary<Guid, HsmManifestListItem>();
            var pendingListItemsByComposite = new Dictionary<(Guid ListId, int ItemId), HsmManifestListItem>();

            foreach (var spObject in EnumerateSpObjects(normalizedManifestPath))
            {
                var objectType = spObject.Attribute("ObjectType")?.Value;
                if (string.Equals(objectType, ObjectTypeFile, StringComparison.OrdinalIgnoreCase))
                {
                    var file = ParseFile(spObject);
                    if (file == null || !MatchesGuid(file.ParentWebId, webId) || !MatchesGuid(file.ListId, listId))
                    {
                        continue;
                    }

                    if (TryAttachListItem(file, pendingListItemsByGuid, pendingListItemsByComposite))
                    {
                        yield return file;
                        continue;
                    }

                    AddPendingFile(file, pendingFilesByGuid, pendingFilesByComposite);
                    continue;
                }

                if (!string.Equals(objectType, ObjectTypeListItem, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var listItem = ParseListItem(spObject);
                if (listItem == null || !MatchesGuid(listItem.ParentWebId, webId) || !MatchesGuid(listItem.ParentListId, listId))
                {
                    continue;
                }

                if (TryResolvePendingFile(listItem, pendingFilesByGuid, pendingFilesByComposite, out var matchedFile) && matchedFile != null)
                {
                    matchedFile.ListItem = listItem;
                    yield return matchedFile;
                    continue;
                }

                AddPendingListItem(listItem, pendingListItemsByGuid, pendingListItemsByComposite);
            }

            foreach (var file in pendingFilesByGuid.Values)
            {
                yield return file;
            }
        }

        public static List<HsmManifestListItem> FindListItemsByWebAndListId(string manifestPath, Guid webId, Guid listId)
        {
            return EnumerateListItems(EnsureManifestPath(manifestPath), item =>
                MatchesGuid(item.ParentWebId, webId) && MatchesGuid(item.ParentListId, listId)).ToList();
        }

        private static IEnumerable<HsmManifestFile> EnumerateFiles(string manifestPath, Func<HsmManifestFile, bool> predicate)
        {
            foreach (var spObject in EnumerateSpObjects(manifestPath, ObjectTypeFile))
            {
                var file = ParseFile(spObject);
                if (file != null && predicate(file))
                {
                    yield return file;
                }
            }
        }

        private static IEnumerable<HsmManifestListItem> EnumerateListItems(string manifestPath, Func<HsmManifestListItem, bool> predicate)
        {
            foreach (var spObject in EnumerateSpObjects(manifestPath, ObjectTypeListItem))
            {
                var listItem = ParseListItem(spObject);
                if (listItem != null && predicate(listItem))
                {
                    yield return listItem;
                }
            }
        }

        private static IEnumerable<XElement> EnumerateSpObjects(string manifestPath)
        {
            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Ignore,
                CloseInput = true
            };

            using var reader = XmlReader.Create(manifestPath, settings);
            reader.MoveToContent();

            while (!reader.EOF)
            {
                if (reader.NodeType != XmlNodeType.Element || !string.Equals(reader.LocalName, "SPObject", StringComparison.Ordinal))
                {
                    reader.Read();
                    continue;
                }

                if (XElement.ReadFrom(reader) is XElement element)
                {
                    yield return element;
                }
            }
        }

        private static IEnumerable<XElement> EnumerateSpObjects(string manifestPath, string objectType)
        {
            foreach (var element in EnumerateSpObjects(manifestPath))
            {
                var type = element.Attribute("ObjectType")?.Value;
                if (!string.Equals(type, objectType, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return element;
            }
        }

        private static HsmManifestFile? ParseFile(XElement spObject)
        {
            var fileElement = spObject.Element(ManifestNamespace + "File");
            if (fileElement == null)
            {
                return null;
            }

            var fieldDictionary = ParseFields(fileElement.Element(ManifestNamespace + "Fields"));
            MergeFields(fieldDictionary, ParseProperties(fileElement.Element(ManifestNamespace + "Properties")));
            var file = new HsmManifestFile
            {
                Id = ReadGuid(fileElement, "Id"),
                Name = ReadString(fileElement, "Name"),
                Url = ReadString(fileElement, "Url"),
                ParentWebId = ReadGuid(fileElement, "ParentWebId"),
                ParentWebUrl = ReadString(fileElement, "ParentWebUrl"),
                ListId = ReadNullableGuid(fileElement, "ListId"),
                ParentId = ReadNullableGuid(fileElement, "ParentId"),
                ListItemIntId = ReadNullableInt(fileElement, "ListItemIntId"),
                InDocumentLibrary = ReadBool(fileElement, "InDocumentLibrary"),
                FileValue = ReadString(fileElement, "FileValue"),
                Version = ReadString(fileElement, "Version"),
                Author = ReadString(fileElement, "Author"),
                ModifiedBy = ReadString(fileElement, "ModifiedBy"),
                TimeCreated = ReadNullableDateTime(fileElement, "TimeCreated"),
                TimeLastModified = ReadNullableDateTime(fileElement, "TimeLastModified")
            };

            file.Versions.AddRange(ParseFileVersions(fileElement.Element(ManifestNamespace + "Versions")));
            file.SetFields(fieldDictionary.Count == 0
                ? EmptyFields
                : new ReadOnlyDictionary<string, HsmManifestField>(fieldDictionary));
            return file;
        }

        private static HsmManifestListItem? ParseListItem(XElement spObject)
        {
            var listItemElement = spObject.Element(ManifestNamespace + "ListItem");
            if (listItemElement == null)
            {
                return null;
            }

            var parentListId = ReadNullableGuid(listItemElement, "ParentListId")
                               ?? ReadNullableGuid(spObject, "ParentId");
            var parentWebId = ReadNullableGuid(listItemElement, "ParentWebId")
                              ?? ReadNullableGuid(spObject, "ParentWebId");
            var parentFolderId = ReadNullableGuid(listItemElement, "ParentFolderId")
                                 ?? ReadNullableGuid(spObject, "ParentFolderId");

            var listItem = new HsmManifestListItem
            {
                Id = ReadNullableGuid(listItemElement, "Id"),
                DocId = ReadNullableGuid(listItemElement, "DocId"),
                IntId = ReadNullableInt(listItemElement, "IntId"),
                ParentListId = parentListId,
                ParentWebId = parentWebId,
                ParentFolderId = parentFolderId,
                Name = ReadString(listItemElement, "Name"),
                Url = ReadString(listItemElement, "Url")
            };

            if (string.IsNullOrWhiteSpace(listItem.Url))
            {
                listItem.Url = spObject.Attribute("Url")?.Value ?? string.Empty;
            }

            listItem.FileUrl = ReadString(listItemElement, "FileUrl");
            listItem.DirName = ReadString(listItemElement, "DirName");
            listItem.Version = ReadString(listItemElement, "Version");
            listItem.Author = ReadString(listItemElement, "Author");
            listItem.ModifiedBy = ReadString(listItemElement, "ModifiedBy");
            listItem.Created = ReadNullableDateTime(listItemElement, "TimeCreated");
            listItem.Modified = ReadNullableDateTime(listItemElement, "TimeLastModified");

            var versions = ParseListItemVersions(listItemElement.Element(ManifestNamespace + "Versions"));
            listItem.Versions.AddRange(versions);
            listItem.ApplyCurrentFields();
            return listItem;
        }

        private static List<HsmManifestFileVersion> ParseFileVersions(XElement? versionsElement)
        {
            var result = new List<HsmManifestFileVersion>();
            if (versionsElement == null)
            {
                return result;
            }

            foreach (var versionElement in versionsElement.Elements(ManifestNamespace + "File"))
            {
                var fieldDictionary = ParseFields(versionElement.Element(ManifestNamespace + "Fields"));
                MergeFields(fieldDictionary, ParseProperties(versionElement.Element(ManifestNamespace + "Properties")));
                var version = new HsmManifestFileVersion
                {
                    Version = ReadString(versionElement, "Version"),
                    FileValue = ReadString(versionElement, "FileValue"),
                    Author = ReadString(versionElement, "Author"),
                    ModifiedBy = ReadString(versionElement, "ModifiedBy"),
                    Created = ReadNullableDateTime(versionElement, "TimeCreated"),
                    Modified = ReadNullableDateTime(versionElement, "TimeLastModified"),
                    Fields = fieldDictionary.Count == 0
                        ? EmptyFields
                        : new ReadOnlyDictionary<string, HsmManifestField>(fieldDictionary)
                };
                result.Add(version);
            }

            return result;
        }

        private static List<HsmManifestListItemVersion> ParseListItemVersions(XElement? versionsElement)
        {
            var result = new List<HsmManifestListItemVersion>();
            if (versionsElement == null)
            {
                return result;
            }

            foreach (var versionElement in versionsElement.Elements(ManifestNamespace + "ListItem"))
            {
                var fields = ParseFields(versionElement.Element(ManifestNamespace + "Fields"));
                MergeFields(fields, ParseProperties(versionElement.Element(ManifestNamespace + "Properties")));
                var version = new HsmManifestListItemVersion
                {
                    Version = ReadString(versionElement, "Version"),
                    Id = ReadNullableGuid(versionElement, "Id"),
                    DocId = ReadNullableGuid(versionElement, "DocId"),
                    IntId = ReadNullableInt(versionElement, "IntId"),
                    Created = ReadNullableDateTime(versionElement, "TimeCreated"),
                    Modified = ReadNullableDateTime(versionElement, "TimeLastModified"),
                    Fields = new ReadOnlyDictionary<string, HsmManifestField>(fields)
                };
                result.Add(version);
            }

            return result;
        }

        private static Dictionary<string, HsmManifestField> ParseFields(XElement? fieldsElement)
        {
            var result = new Dictionary<string, HsmManifestField>(StringComparer.OrdinalIgnoreCase);
            if (fieldsElement == null)
            {
                return result;
            }

            foreach (var fieldElement in fieldsElement.Elements(ManifestNamespace + "Field"))
            {
                var field = new HsmManifestField
                {
                    Name = ReadString(fieldElement, "Name"),
                    Id = fieldElement.Attribute("ID")?.Value ?? fieldElement.Attribute("Id")?.Value,
                    Value = ReadString(fieldElement, "Value"),
                    Value2 = ReadString(fieldElement, "Value2"),
                    Type = ReadString(fieldElement, "Type")
                };

                if (!string.IsNullOrEmpty(field.Name))
                {
                    result[field.Name] = field;
                }
            }

            return result;
        }

        private static Dictionary<string, HsmManifestField> ParseProperties(XElement? propertiesElement)
        {
            var result = new Dictionary<string, HsmManifestField>(StringComparer.OrdinalIgnoreCase);
            if (propertiesElement == null)
            {
                return result;
            }

            foreach (var propertyElement in propertiesElement.Elements(ManifestNamespace + "Property"))
            {
                var field = new HsmManifestField
                {
                    Name = propertyElement.Attribute("Name")?.Value ?? string.Empty,
                    Id = propertyElement.Attribute("ID")?.Value ?? propertyElement.Attribute("Id")?.Value,
                    Value = propertyElement.Attribute("Value")?.Value,
                    Value2 = propertyElement.Attribute("Value2")?.Value,
                    Type = propertyElement.Attribute("Type")?.Value
                };

                if (!string.IsNullOrEmpty(field.Name))
                {
                    result[field.Name] = field;
                }
            }

            return result;
        }

        private static void MergeFields(IDictionary<string, HsmManifestField> target, IDictionary<string, HsmManifestField> source)
        {
            if (source.Count == 0)
            {
                return;
            }

            foreach (var pair in source)
            {
                target[pair.Key] = pair.Value;
            }
        }

        private static void AttachListItems(IList<HsmManifestFile> files, IList<HsmManifestListItem> listItems)
        {
            if (listItems.Count == 0)
            {
                return;
            }

            var byGuid = new Dictionary<Guid, HsmManifestListItem>();
            var byComposite = new Dictionary<(Guid ListId, int ItemId), HsmManifestListItem>();

            foreach (var listItem in listItems)
            {
                if (listItem.DocId.HasValue && listItem.DocId.Value != Guid.Empty)
                {
                    byGuid[listItem.DocId.Value] = listItem;
                }

                if (listItem.Id.HasValue && listItem.Id.Value != Guid.Empty)
                {
                    byGuid[listItem.Id.Value] = listItem;
                }

                if (listItem.ParentListId.HasValue && listItem.IntId.HasValue)
                {
                    byComposite[(listItem.ParentListId.Value, listItem.IntId.Value)] = listItem;
                }
            }

            foreach (var file in files)
            {
                if (byGuid.TryGetValue(file.Id, out var listItem))
                {
                    file.ListItem = listItem;
                    continue;
                }

                if (file.ListId.HasValue && file.ListItemIntId.HasValue &&
                    byComposite.TryGetValue((file.ListId.Value, file.ListItemIntId.Value), out listItem))
                {
                    file.ListItem = listItem;
                }
            }
        }

        private static void AddPendingFile(HsmManifestFile file, IDictionary<Guid, HsmManifestFile> pendingFilesByGuid, IDictionary<(Guid ListId, int ItemId), HsmManifestFile> pendingFilesByComposite)
        {
            pendingFilesByGuid[file.Id] = file;

            if (file.ListId.HasValue && file.ListItemIntId.HasValue)
            {
                pendingFilesByComposite[(file.ListId.Value, file.ListItemIntId.Value)] = file;
            }
        }

        private static bool TryAttachListItem(HsmManifestFile file, IDictionary<Guid, HsmManifestListItem> pendingListItemsByGuid, IDictionary<(Guid ListId, int ItemId), HsmManifestListItem> pendingListItemsByComposite)
        {
            if (pendingListItemsByGuid.TryGetValue(file.Id, out var listItem))
            {
                pendingListItemsByGuid.Remove(file.Id);
                RemovePendingListItemComposite(listItem, pendingListItemsByComposite);
                file.ListItem = listItem;
                return true;
            }

            if (file.ListId.HasValue && file.ListItemIntId.HasValue &&
                pendingListItemsByComposite.TryGetValue((file.ListId.Value, file.ListItemIntId.Value), out listItem))
            {
                pendingListItemsByComposite.Remove((file.ListId.Value, file.ListItemIntId.Value));
                RemovePendingListItemGuid(listItem, pendingListItemsByGuid);
                file.ListItem = listItem;
                return true;
            }

            return false;
        }

        private static void AddPendingListItem(HsmManifestListItem listItem, IDictionary<Guid, HsmManifestListItem> pendingListItemsByGuid, IDictionary<(Guid ListId, int ItemId), HsmManifestListItem> pendingListItemsByComposite)
        {
            if (listItem.DocId.HasValue && listItem.DocId.Value != Guid.Empty)
            {
                pendingListItemsByGuid[listItem.DocId.Value] = listItem;
            }

            if (listItem.Id.HasValue && listItem.Id.Value != Guid.Empty)
            {
                pendingListItemsByGuid[listItem.Id.Value] = listItem;
            }

            if (listItem.ParentListId.HasValue && listItem.IntId.HasValue)
            {
                pendingListItemsByComposite[(listItem.ParentListId.Value, listItem.IntId.Value)] = listItem;
            }
        }

        private static bool TryResolvePendingFile(HsmManifestListItem listItem, IDictionary<Guid, HsmManifestFile> pendingFilesByGuid, IDictionary<(Guid ListId, int ItemId), HsmManifestFile> pendingFilesByComposite, out HsmManifestFile? matchedFile)
        {
            if (listItem.DocId.HasValue && listItem.DocId.Value != Guid.Empty && pendingFilesByGuid.TryGetValue(listItem.DocId.Value, out var fileByDocId))
            {
                matchedFile = fileByDocId;
                pendingFilesByGuid.Remove(listItem.DocId.Value);
                RemovePendingFileComposite(matchedFile, pendingFilesByComposite);
                return true;
            }

            if (listItem.Id.HasValue && listItem.Id.Value != Guid.Empty && pendingFilesByGuid.TryGetValue(listItem.Id.Value, out var fileById))
            {
                matchedFile = fileById;
                pendingFilesByGuid.Remove(listItem.Id.Value);
                RemovePendingFileComposite(matchedFile, pendingFilesByComposite);
                return true;
            }

            if (listItem.ParentListId.HasValue && listItem.IntId.HasValue && pendingFilesByComposite.TryGetValue((listItem.ParentListId.Value, listItem.IntId.Value), out var fileByComposite))
            {
                matchedFile = fileByComposite;
                pendingFilesByComposite.Remove((listItem.ParentListId.Value, listItem.IntId.Value));
                pendingFilesByGuid.Remove(matchedFile.Id);
                return true;
            }

            matchedFile = null;
            return false;
        }

        private static void RemovePendingFileComposite(HsmManifestFile file, IDictionary<(Guid ListId, int ItemId), HsmManifestFile> pendingFilesByComposite)
        {
            if (file.ListId.HasValue && file.ListItemIntId.HasValue)
            {
                pendingFilesByComposite.Remove((file.ListId.Value, file.ListItemIntId.Value));
            }
        }

        private static void RemovePendingListItemComposite(HsmManifestListItem listItem, IDictionary<(Guid ListId, int ItemId), HsmManifestListItem> pendingListItemsByComposite)
        {
            if (listItem.ParentListId.HasValue && listItem.IntId.HasValue)
            {
                pendingListItemsByComposite.Remove((listItem.ParentListId.Value, listItem.IntId.Value));
            }
        }

        private static void RemovePendingListItemGuid(HsmManifestListItem listItem, IDictionary<Guid, HsmManifestListItem> pendingListItemsByGuid)
        {
            if (listItem.DocId.HasValue && listItem.DocId.Value != Guid.Empty)
            {
                pendingListItemsByGuid.Remove(listItem.DocId.Value);
            }

            if (listItem.Id.HasValue && listItem.Id.Value != Guid.Empty)
            {
                pendingListItemsByGuid.Remove(listItem.Id.Value);
            }
        }

        private static string EnsureManifestPath(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath))
            {
                throw new ArgumentException("Manifest path is required.", nameof(manifestPath));
            }

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException($"Manifest file not found: {manifestPath}.", manifestPath);
            }

            return manifestPath;
        }

        private static bool MatchesGuid(Guid actual, Guid expected)
        {
            return expected == Guid.Empty || actual == expected;
        }

        private static bool MatchesGuid(Guid? actual, Guid expected)
        {
            return expected == Guid.Empty || (actual.HasValue && actual.Value == expected);
        }

        private static Guid ReadGuid(XElement element, string attributeName)
        {
            return Guid.TryParse(element.Attribute(attributeName)?.Value, out var result) ? result : Guid.Empty;
        }

        private static Guid? ReadNullableGuid(XElement element, string attributeName)
        {
            return Guid.TryParse(element.Attribute(attributeName)?.Value, out var result) ? result : null;
        }

        private static int? ReadNullableInt(XElement element, string attributeName)
        {
            return int.TryParse(element.Attribute(attributeName)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
        }

        private static bool ReadBool(XElement element, string attributeName)
        {
            return bool.TryParse(element.Attribute(attributeName)?.Value, out var result) && result;
        }

        private static DateTime? ReadNullableDateTime(XElement element, string attributeName)
        {
            return DateTime.TryParse(element.Attribute(attributeName)?.Value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result)
                ? result
                : null;
        }

        private static string ReadString(XElement element, string attributeName)
        {
            return element.Attribute(attributeName)?.Value ?? string.Empty;
        }
    }

    public sealed class HsmManifestFile
    {
        private IReadOnlyDictionary<string, HsmManifestField> parsedFields = HsmManifestParser.EmptyFields;

        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public Guid ParentWebId { get; init; }
        public string ParentWebUrl { get; init; } = string.Empty;
        public Guid? ListId { get; init; }
        public Guid? ParentId { get; init; }
        public int? ListItemIntId { get; init; }
        public bool InDocumentLibrary { get; init; }
        public string? FileValue { get; init; }
        public string? Version { get; init; }
        public string? Author { get; init; }
        public string? ModifiedBy { get; init; }
        public DateTime? TimeCreated { get; init; }
        public DateTime? TimeLastModified { get; init; }
        public List<HsmManifestFileVersion> Versions { get; } = new();
        public HsmManifestListItem? ListItem { get; internal set; }
        public IReadOnlyDictionary<string, HsmManifestField> Fields => parsedFields.Count > 0
            ? parsedFields
            : ListItem?.Fields ?? HsmManifestParser.EmptyFields;

        internal void SetFields(IReadOnlyDictionary<string, HsmManifestField> fields)
        {
            parsedFields = fields ?? HsmManifestParser.EmptyFields;
        }
    }

    public sealed class HsmManifestFileVersion
    {
        public string? Version { get; init; }
        public string? FileValue { get; init; }
        public string? Author { get; init; }
        public string? ModifiedBy { get; init; }
        public DateTime? Created { get; init; }
        public DateTime? Modified { get; init; }
        public IReadOnlyDictionary<string, HsmManifestField> Fields { get; init; } =
            HsmManifestParser.EmptyFields;
    }

    public sealed class HsmManifestListItem
    {
        private readonly Dictionary<string, HsmManifestField> currentFields = new(StringComparer.OrdinalIgnoreCase);

        public Guid? Id { get; init; }
        public Guid? DocId { get; init; }
        public int? IntId { get; init; }
        public Guid? ParentListId { get; init; }
        public Guid? ParentWebId { get; init; }
        public Guid? ParentFolderId { get; init; }
        public string? Name { get; init; }
        public string? Url { get; set; }
        public string? FileUrl { get; set; }
        public string? DirName { get; set; }
        public string? Version { get; set; }
        public string? Author { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public List<HsmManifestListItemVersion> Versions { get; } = new();
        public IReadOnlyDictionary<string, HsmManifestField> Fields => currentFields;

        internal void ApplyCurrentFields()
        {
            if (Versions.Count == 0)
            {
                return;
            }

            var targetVersion = string.IsNullOrWhiteSpace(Version)
                ? Versions.Last()
                : Versions.FirstOrDefault(v => string.Equals(v.Version, Version, StringComparison.OrdinalIgnoreCase))
                  ?? Versions.Last();

            currentFields.Clear();
            foreach (var field in targetVersion.Fields)
            {
                currentFields[field.Key] = field.Value;
            }
        }
    }

    public sealed class HsmManifestListItemVersion
    {
        public string? Version { get; init; }
        public Guid? Id { get; init; }
        public Guid? DocId { get; init; }
        public int? IntId { get; init; }
        public DateTime? Created { get; init; }
        public DateTime? Modified { get; init; }
        public IReadOnlyDictionary<string, HsmManifestField> Fields { get; init; } =
            new ReadOnlyDictionary<string, HsmManifestField>(new Dictionary<string, HsmManifestField>(0, StringComparer.OrdinalIgnoreCase));
    }

    public sealed class HsmManifestField
    {
        public string Name { get; init; } = string.Empty;
        public string? Id { get; init; }
        public string? Value { get; init; }
        public string? Value2 { get; init; }
        public string? Type { get; init; }
    }
}
