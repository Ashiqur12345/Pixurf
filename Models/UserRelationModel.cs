using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pixurf.Models
{
    public class UserRelationModel
    {
        public static string FOLLOW = "Follow";
        public static string BLOCK = "Block";
        public static string NONE = "None";

        public string User { get; set; }
        public string TargetUser { get; set; }
        public string Action { get; set; }
    }
}