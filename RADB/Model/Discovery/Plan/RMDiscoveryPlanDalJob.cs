using AvePoint.RA.Contract.Discovery.Job;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AvePoint.RA.DB.Model.Discovery.Plan
{
    [Table("RMDiscoveryPlanDalJob")]

    public class RMDiscoveryPlanDalJob : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("mainJobId")]
        public string MainJobId { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [Column(TypeName = "bit")]
        [JsonProperty("needToReRegisterTags")]
        public bool NeedToReRegisterTags { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("sitesCount")]
        public int SitesCount { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("lastModifiedTime")]
        public long LastModifiedTime { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("dalJobId")]
        public Guid DalJobId { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("status")]
        public RMDalJobStatus Status { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("extension")]
        public string Extension { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
