using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pixurf.Models
{
    public class StatusReport
    {
        public static readonly string Danger = "danger";
        public static readonly string Success = "success";
        public static readonly string Warning = "warning";
        public static readonly string Info = "info";

        public string Status { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
}