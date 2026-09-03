using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.JPMC
{
    [DataContract]
    public class RCCReportRequest
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("nodes")]
        public List<RCCNode> Nodes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("connGroupId")]
        public Guid ConnGroupId;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("connectionId")]
        public Guid ConnectionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("jpmcId")]
        public string JPMCId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("level")]
        public int Level;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("timeRange")]
        public RCCReportTimeRange TimeRange { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("isMyhub")]
        public bool IsMyHub { get; set; } = false;

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public bool IsDaylight { get; set; }
    }

    [DataContract]
    public class RCCNode
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("id")]
        public Guid Id;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("fullPath")]
        public string FullPath;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("name")]
        public string Name;
    }

    [DataContract]
    public class RCCReportTimeRange
    {
        private const string DateFormat = "yyyy/M/d HH:mm";

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("presetType")]
        public int PresetType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("startDateTicks")]
        public long StartDateTicks { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("endDateTicks")]
        public long EndDateTicks { get; set; }

        public (DateTime start, DateTime end) Resolve(TimeZoneInfo timeZone = null)
        {
            timeZone ??= TimeZoneInfo.Utc;

            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            return PresetType switch
            {
                1 => (nowLocal, nowLocal.AddMonths(3)),
                2 => (nowLocal, nowLocal.AddMonths(6)),
                3 => (nowLocal, nowLocal.AddYears(1)),
                _ => (ParseDate(StartDate), GetEndOfMinute(ParseDate(EndDate)))
            };
        }
        private static DateTime GetEndOfMinute(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 59, date.Kind);
        }
        private static readonly string[] AllowedDateFormats =
            {
                "yyyy/M/d HH:mm",
                "yyyy/M/d H:m",
                "yyyy/MM/dd HH:mm",
                "yyyy/MM/dd H:m",
                "yyyy/M/d H:mm",
                "yyyy/M/d HH:m"
            };
        public (string startStr, string endStr) ResolveFormatted(string exactFormat)
        {
            var (startDt, endDt) = Resolve();

            return (startDt.ToString(exactFormat), endDt.ToString(exactFormat));
        }
        private static DateTime ParseDate(string dateStr)
        {
            if (DateTime.TryParseExact(
                    dateStr,
                    AllowedDateFormats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime result))
            {
                return result;
            }

            throw new FormatException($"Invalid date format: '{dateStr}'. Expected standard formats like 'yyyy/M/d HH:mm' or 'yyyy/M/d H:m'");
        }
    }
}