using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Pixurf.DataModels;

namespace Pixurf.ViewModels
{
    public class ViewUserModel
    {
        public ViewUserModel()
        {
            PopularContents = new List<Content>();
            MyProfile = false;
        }

        [Display(Name = "User Id")]
        public string User_ID { get; set; }
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "About Me")]
        public string About_Me { get; set; }

        [Display(Name = "Joining Date")]
        public Nullable<System.DateTime> Joining_Date { get; set; }
        public string Country { get; set; }

        [Display(Name = "Profile Picture Id")]
        public string Pro_Pic_ID { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }

        [Display(Name = "Administrator")]
        public bool Admin { get; set; }

        public ViewAlbumsModel Albums { get; set; }

        [Display(Name = "Popular Contents")]
        public List<Content> PopularContents { get; set; }

        [Display(Name = "Followed By")]
        public int Followers { get; set; }
        public bool MyProfile { get; set; }
    }
}