using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Pixurf.ViewModels
{
    public class ViewUploadContentModel
    {
        [Display(Name = "Album Name")]
        public string AlbumName { get; set; }
       
        [Required]
        [Display(Name = "Content Privacy")]
        public string Access { get; set; }
        
        [Required]
        [DisplayName("Files")]
        public IEnumerable<HttpPostedFileBase> ImageFiles { get; set; }

        
    }
}