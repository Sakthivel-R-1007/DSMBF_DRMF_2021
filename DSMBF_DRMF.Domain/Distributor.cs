using System;
using System.Collections.Generic;

namespace DSMBF_DRMF.Domain
{
    public class Distributor : Entity<Int64>
    {
        public Guid GUID { get; set; }
        public string AccountCode { get; set; }
        public string CompanyName_EN { get; set; }
        public string CompanyName_SC { get; set; }
        public string DeliveryAddress { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Area { get; set; }
        public string ProgramTypeName { get; set; }
        public IList<ProgramType> ProgramTypes { get; set; }
        public Int64 TotalCount { get; set; }
        public Int64 RowNum { get; set; }

    }
}
