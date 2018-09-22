using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pixurf.DataModels;
using Pixurf.ViewModels;

namespace Pixurf.Controllers
{
    public class HomeController : System.Web.Mvc.Controller
    {
        public ActionResult Index()
        {
            
            using (PixurfDBContext db = new PixurfDBContext())
            {
                List<Content> contents = db.Contents.Where(c => c.Access == "Public" && c.Status == 1).OrderByDescending(c => c.Creation_Date).Take(1).ToList();
                if (contents.Count > 0)
                {
                    int contentId = contents[0].Content_ID;
                    Content content = db.Contents.Find(contentId);

                    if (content != null)
                    {
                        ViewContentModel vcm = new ViewContentModel
                        {
                            Content_ID = content.Content_ID,
                            Title = content.Title,
                            Description = content.Description,
                            Album = content.Album,
                            Path = content.Path,
                            User = content.User,
                            Access = content.Access,
                            Creation_Date = content.Creation_Date,
                            Status = content.Status,
                            Type = content.Type
                        };

                        return View(vcm);
                    }
                }
            }

            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Pixurf Inc.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact page.";

            return View();
        }
    }
}