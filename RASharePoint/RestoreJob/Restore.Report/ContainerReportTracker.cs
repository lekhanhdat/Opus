using System;
using System.Collections.Generic;

namespace AvePoint.Item.Restore
{
    internal sealed class ContainerReportTracker
    {
        private readonly object _syncRoot = new object();
        private readonly HashSet<string> _reportedContainers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal bool TryExecuteOnce(AveRestoreReportDto reportDto, Action reportAction)
        {
            if (reportDto == null)
            {
                throw new ArgumentNullException(nameof(reportDto));
            }

            if (reportAction == null)
            {
                throw new ArgumentNullException(nameof(reportAction));
            }

            string reportKey = CreateReportKey(reportDto);
            if (reportKey == null)
            {
                reportAction();
                return true;
            }

            lock (_syncRoot)
            {
                if (_reportedContainers.Contains(reportKey))
                {
                    return false;
                }

                reportAction();
                _reportedContainers.Add(reportKey);
                return true;
            }
        }

        private static string CreateReportKey(AveRestoreReportDto reportDto)
        {
            string reportType = reportDto.Type?.Trim();
            string identity = !string.IsNullOrWhiteSpace(reportDto.PathMD5)
                ? $"md5:{reportDto.PathMD5.Trim()}"
                : NormalizePath(reportDto.SourcePath);
            identity ??= NormalizePath(reportDto.Path);

            return string.IsNullOrWhiteSpace(reportType) || string.IsNullOrWhiteSpace(identity)
                ? null
                : $"{reportType}|{identity}";
        }

        private static string NormalizePath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? null
                : path.Trim().TrimEnd('/');
        }
    }
}
