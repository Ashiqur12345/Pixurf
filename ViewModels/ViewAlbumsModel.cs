using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pixurf.DataModels;

namespace Pixurf.ViewModels
{
    public class ViewAlbumsModel
    {
        public ViewAlbumsModel()
        {
            Albums = new List<Album>();
        }
        public List<Album> Albums { get; set; }
    }
}