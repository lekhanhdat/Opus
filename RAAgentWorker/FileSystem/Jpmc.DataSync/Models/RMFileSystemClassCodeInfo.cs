using AvePoint.GCommon.Contract.CommonFilter;
using System;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemClassCode
    {
        public Guid Id { get; set; }

        public string Name { set; get; }

        public string CountryCode { set; get; }

        public int RetentionType { set; get; }

        public long StartDate { set; get; }

        public long EndTime { set; get; }

        public int PolicyValueUnit { set; get; }

        public int PolicyValueNumber { set; get; }

        public bool Exists => !string.IsNullOrWhiteSpace(Name);

        public RMFileSystemClassCode Clone()
        {
            return new RMFileSystemClassCode
            {
                Id = this.Id,
                Name = this.Name,
                CountryCode = this.CountryCode,
                RetentionType = this.RetentionType,
                StartDate = this.StartDate,
                EndTime = 0,
                PolicyValueUnit = this.PolicyValueUnit,
                PolicyValueNumber = this.PolicyValueNumber,
            };
        }
    }

    public enum RMFileSystemClassCodeRetentionType
    {
        None = 0,
        Event = 1,
        Flat = 2
    }

    public class RMFileSystemClassCodeUnitInfo
    {
        public int Unit { get; set; }

        public PolicyValueUnit UnitType { get; set; }
    }
}
