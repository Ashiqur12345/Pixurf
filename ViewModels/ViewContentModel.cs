using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Pixurf.DataModels;

namespace Pixurf.ViewModels
{
    public class ViewContentModel
    {
        public int Content_ID { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Path { get; set; }

        [Display(Name = "Privacy")]
        public string Access { get; set; }
        public string Type { get; set; }

        public byte Status { get; set; }
        [Display(Name = "Upload Date")]
        public System.DateTime Creation_Date { get; set; }

        public User User { get; set; }
        public Album Album { get; set; }

        public bool UserAuthenticated { get; set; }

        public string User_Id { get; set; }

        [Display(Name = "Album Name")]
        public string AlbumName { get; set; }
    }
}