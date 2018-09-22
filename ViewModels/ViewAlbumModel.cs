using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Pixurf.DataModels;

namespace Pixurf.ViewModels
{
    public class ViewAlbumModel
    {
        public ViewAlbumModel()
        {
            Contents = new List<Content>();
        }
        public int Album_ID { get; set; }

        [Display(Name = "Name"), Required]
        public string Name { get; set; }


        public string Description { get; set; }
        public System.DateTime Creation_Date { get; set; }
        public byte Status { get; set; }

        [Required, Display(Name = "Album Privacy")]
        public string Access { get; set; }

        public User User { get; set; }

        public List<Content> Contents { get; set; }
        public bool UserAuthenticated { get; set; }

    }
}