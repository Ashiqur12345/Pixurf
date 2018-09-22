using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Pixurf.DataModels;
using Pixurf.ViewModels;
using Microsoft.Ajax.Utilities;
using Pixurf.Models;

namespace Pixurf.Controllers
{
    [Authorize]
    public class ContentController : Controller
    {
        // GET: Content
        public ActionResult Index()
        {
            return RedirectToAction("Index","Album");
        }
        
        /// Work Space ///
        /// 
        public ActionResult Delete(int id)
        {
            List<StatusReport> reports = new List<StatusReport>();

            
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Content content = db.Contents.Find(id);
                string userId = User.Identity.GetUserId();

                if (content == null)
                {
                    reports.Add(new StatusReport
                    {
                        Title = "Failed",
                        Description = "Content not found",
                        Status = StatusReport.Danger
                    });
                    Session["Reports"] = reports;
                    return RedirectToAction("Index", "Content");
                }
                // check if user own's the content and not already deleted

                if (content.Status == 1 && content.User_ID == userId)
                {
                    content.Status = 0;
                    try
                    {
                        db.SaveChanges();
                        reports.Add(new StatusReport
                        {
                            Title = "Success",
                            Description = "Content succesfully deleted.",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;

                        return RedirectToAction("View", "Album", new { id = content.Album_ID });
                        //Redirect to album instead//
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        reports.Add(new StatusReport
                        {
                            Title = "Error",
                            Description = "Something went wrong",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;
                        return RedirectToAction("View", "Album", new { id = content.Album_ID });
                    }
                }


                var user = db.Users.Find(userId);
                if (user != null && user.Admin)
                {
                    try
                    {
                        //Remove the file and it thumbnail from disk
                        string uploadRoot = "~/UserUploads/";

                        //Content & Thumbnail Save Loaction
                        var strings = WebConfigurationManager.AppSettings.GetValues("UserUploadRoot");
                        if (strings != null && strings.Length > 0) uploadRoot = strings.First();

                        string contentPath = uploadRoot + content.User_ID + "/" + content.Path;
                        contentPath = Server.MapPath(contentPath);
                        if (System.IO.File.Exists(contentPath))
                        {
                            System.IO.File.Delete(contentPath);
                        }

                        string thumbPath = uploadRoot + content.User_ID + "/thumbs/thumb_" + content.Path;
                        thumbPath = Server.MapPath(thumbPath);
                        if (System.IO.File.Exists(thumbPath))
                        {
                            System.IO.File.Delete(thumbPath);
                        }

                        db.Contents.Remove(content);
                        db.SaveChanges();

                        reports.Add(new StatusReport
                        {
                            Title = "Success",
                            Description = "Content permanently deleted.",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;

                        return RedirectToAction("View", "Album", new { id = content.Album_ID });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        reports.Add(new StatusReport
                        {
                            Title = "Error",
                            Description = "Something went wrong",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;
                        return RedirectToAction("View", "Album", new { id = content.Album_ID });
                    }
                }

            }
            return RedirectToAction("Index", "Album");
        }

        public FileResult Download(int id)
        {
            string uploadRoot = "~/UserUploads/";

            var strings = WebConfigurationManager.AppSettings.GetValues("UserUploadRoot");
            if (strings != null)
            {
                uploadRoot = strings.First();
            }
            var dir = Server.MapPath(uploadRoot);

            using (PixurfDBContext db = new PixurfDBContext())
            {
                Content content = db.Contents.Find(id);
                string userId = User.Identity.GetUserId();
                User user = db.Users.Find(userId);

                if (content != null && (content.User_ID == userId && content.Status == 1 || user != null && user.Admin))
                {

                    var path = Path.Combine(dir, content.User_ID + "\\" + content.Path); //validate the path for security or use other means to generate the path.
                    string fileName = content.Title + Path.GetExtension(content.Path);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    //return File(path, "image/jpeg");
                }
                else
                {
                    return null;
                }

            }
            
        }
        
        [HttpPost]
        public ActionResult Edit(ViewContentModel model)
        {
            List<StatusReport> reports = new List<StatusReport>();

            string userId = User.Identity.GetUserId();

            if (model != null && 
                !model.Title.IsNullOrWhiteSpace() && 
                !model.Access.IsNullOrWhiteSpace() && 
                !model.Access.IsNullOrWhiteSpace())
            {
                using (PixurfDBContext db = new PixurfDBContext())
                {
                    //Find the Content
                    Content content = db.Contents.Find(model.Content_ID);

                    if (content == null)
                    {
                        reports.Add(new StatusReport
                        {
                            Title = "Error",
                            Description = "Content not found",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;
                        return RedirectToAction("Index","Home");
                    }

                    User user = db.Users.Find(userId);
                    if ((content.User_ID == userId && content.Status == 1) || (user != null && user.Admin))
                    {
                        if (content.Title == model.Title && content.Description == model.Description &&
                            content.Access == model.Access && content.Album.Name == model.AlbumName)
                        {
                            reports.Add(new StatusReport
                            {
                                Title = "Success",
                                Description = "No change is made",
                                Status = StatusReport.Info
                            });
                            Session["Reports"] = reports;
                            return RedirectToAction("View", "Content", new { id = content.Content_ID });

                        }
                        else
                        {
                            content.Title = model.Title;
                            content.Description = model.Description;
                            content.Access = model.Access;

                            Album album = null;
                            if (!model.AlbumName.IsNullOrWhiteSpace() && content.Album.Name != model.AlbumName)
                            {
                                //Find the Album
                                album = db.Albums.FirstOrDefault(a => a.Name == model.AlbumName && a.User_ID == userId);

                                //Create the album if its null
                                if (album == null)
                                {
                                    album = new Album { Name = model.AlbumName, Creation_Date = DateTime.Now, User_ID = userId, Access = model.Access, Status = 1 };
                                    db.Albums.Add(album);
                                }
                                content.Album = album;
                            }

                            try
                            {
                                db.SaveChanges();
                                reports.Add(new StatusReport
                                {
                                    Title = "Success",
                                    Description = "Changes saved",
                                    Status = StatusReport.Success
                                });
                                Session["Reports"] = reports;
                                return RedirectToAction("View", "Content", new { id = content.Content_ID });

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return RedirectToAction("Index");
                            }
                        }
                    }
                }
            }

            reports.Add(new StatusReport
            {
                Title = "Error",
                Description = "Fill the forms properly.",
                Status = StatusReport.Danger
            });
            Session["Reports"] = reports;
            return View(model);
        }
        
        private ViewContentModel GetContent(int id)
        {
            /////////////////////Handle deleted Content, private content
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Content content = db.Contents.Find(id);

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
                    
                    if (content.Status != 0 || content.User.Admin)
                    {
                        //User gets it if not deleted
                        if (content.User_ID == User.Identity.GetUserId())
                        {
                            vcm.UserAuthenticated = true;
                            return vcm;
                        }

                        if (content.User_ID != User.Identity.GetUserId())
                        {

                            if (content.Access == "Public") return vcm;

                            else if (content.Access == "Follower")
                            {
                                string loggInUserId = User.Identity.GetUserId();
                                string contentOwnerId = content.User_ID;

                                User_Relation relation =
                                    db.User_Relations.FirstOrDefault(
                                        r => r.User_ID == loggInUserId && r.Related_User_ID == contentOwnerId);
                                if (relation != null)
                                {
                                    //Check for blocked user
                                    return vcm;
                                }

                            }
                        }
                    }
                }
            }
            return null;
        }

        [HttpGet]
        public ActionResult Favourites()
        {
            List<StatusReport> reports = new List<StatusReport>();
            using (PixurfDBContext db = new PixurfDBContext())
            {
                string userId = User.Identity.GetUserId();
                List<Favourite> favourites = db.Favourites.Where(f => f.User_ID == userId).ToList();

                List<Content> favContents = new List<Content>();
                foreach (Favourite favourite in favourites)
                {
                    favContents.Add(db.Contents.Find(favourite.Content_ID));
                }

                
                return View(favContents);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult View(int id)
        {
            List<StatusReport> reports = new List<StatusReport>();
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Content content = db.Contents.Find(id);

                if (content == null)
                {
                    reports.Add(new StatusReport
                    {
                        Title = "None",
                        Description = "Content not found",
                        Status = StatusReport.Warning
                    });
                    Session["Reports"] = reports;
                    return RedirectToAction("Index", "Home");
                }

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
                
                if (content.Status == 1)
                {

                    if (content.User_ID == User.Identity.GetUserId())
                    {
                        if (reports.Count > 0) Session["Reports"] = reports;

                        vcm.UserAuthenticated = true;
                        return View(vcm);
                    }

                    UserRelationship relationship = new UserRelationship();
                    //Private
                    if (content.Access == "Private")
                    {
                        StatusReport report = new StatusReport
                        {
                            Title = "Access Denied",
                            Description = "This content is not accessible",
                            Status = StatusReport.Warning
                        };

                        reports.Add(report);

                        Session["Reports"] = reports;
                        return RedirectToAction("Index", "Home");
                    }
                    //Follower
                    else if (content.Access == "Follower")
                    {
                        if (relationship.Following(content.User_ID, User.Identity.GetUserId()))
                        {
                            if (reports.Count > 0) Session["Reports"] = reports;
                            return View(vcm);
                        }
                        else
                        {
                            StatusReport report = new StatusReport
                            {
                                Title = "Access Denied",
                                Description = "Only Followers can view this content",
                                Status = StatusReport.Warning
                            };

                            reports.Add(report);

                            Session["Reports"] = reports;
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    //Not Blocked
                    else
                    {
                        if (!relationship.Blocked(content.User_ID, User.Identity.GetUserId()))
                        {
                            if (reports.Count > 0) Session["Reports"] = reports;
                            return View(vcm);
                        }
                        StatusReport report = new StatusReport
                        {
                            Title = "Error",
                            Description = "Content not found",
                            Status = StatusReport.Warning
                        };

                        reports.Add(report);

                        Session["Reports"] = reports;
                        return RedirectToAction("Index", "Home");
                    }


                }
                else
                {
                    User user = db.Users.Find(User.Identity.GetUserId());
                    if (user != null && user.Admin)
                    {
                        if (reports.Count > 0) Session["Reports"] = reports;
                        return View(vcm);
                    }
                    else
                    {
                        StatusReport report = new StatusReport
                        {
                            Title = "Error",
                            Description = "Content not found",
                            Status = StatusReport.Warning
                        };

                        reports.Add(report);

                        Session["Reports"] = reports;
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }


        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(ViewUploadContentModel vUploadContentsModel)
        {
            List<StatusReport> reports = new List<StatusReport>();

            if (vUploadContentsModel.ImageFiles != null)
            {
                foreach (var image in vUploadContentsModel.ImageFiles)
                {
                    //check for file type
                    if (!isValid(image))
                    {
                        reports.Add(new StatusReport
                        {
                            Status = StatusReport.Danger,
                            Title = "Invalid File Type",
                            Description = "File couldn't be saved as it's not an Image"
                        });

                    }
                    else
                    {
                        reports.Add(SaveContent(image, vUploadContentsModel.Access, vUploadContentsModel.AlbumName));
                    }
                }
            }
            else
            {
                //Handle The Exception
                reports.Add(new StatusReport
                {
                    Status = StatusReport.Danger,
                    Title = "Error",
                    Description = "Something went wrong."
                });
            }

            Session["Reports"] = reports;
            return View();
        }

        private StatusReport SaveContent(HttpPostedFileBase image, string access, string albumName)
        {
            string userId = User.Identity.GetUserId();
            string dateTime = DateTime.Now.Millisecond.ToString();
            string fileName = userId + dateTime + Path.GetExtension(image.FileName);
            string uploadRoot = "~/UserUploads/";

            //Content & Thumbnail Save Loaction
            var strings = WebConfigurationManager.AppSettings.GetValues("UserUploadRoot");
            if (strings != null && strings.Length > 0) uploadRoot = strings.First();
            string contentPathWithRoot = uploadRoot + userId + "/";
            if (!Directory.Exists(Server.MapPath(contentPathWithRoot))) Directory.CreateDirectory(Server.MapPath(contentPathWithRoot));

            string thumbPathWithRoot = uploadRoot + userId + "/thumbs/";
            if (!Directory.Exists(Server.MapPath(thumbPathWithRoot))) Directory.CreateDirectory(Server.MapPath(thumbPathWithRoot));
            string thumbName = "thumb_" + fileName;
            string thumbFullPath = Path.Combine(Server.MapPath(thumbPathWithRoot), thumbName);

            string fullFileName = Path.Combine(Server.MapPath(contentPathWithRoot), fileName);
            try
            {
                image.SaveAs(fullFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new StatusReport { Status = StatusReport.Danger, Title = "Save Failed", Description = image.FileName };
            }

            //update database
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Content content = new Content
                {
                    Access = access,
                    Title = Path.GetFileNameWithoutExtension(image.FileName),
                    Path = fileName,
                    Type = image.ContentType,
                    User_ID = userId,
                    Status = 1,
                    Creation_Date = DateTime.Now
                };

                //update album or create and update album
                //Add to album
                string alName = albumName;
                Album album;

                if (!alName.IsNullOrWhiteSpace())
                {
                    //Find the Album
                    album = db.Albums.FirstOrDefault(a => a.Name == alName && a.User_ID == userId);

                    //Create the album if its null
                    if (album == null)
                    {
                        album = new Album { Name = alName, Creation_Date = DateTime.Now, User_ID = userId, Access = access, Status = 1 };
                        db.Albums.Add(album);
                    }
                }
                else
                {
                    album = new Album { Name = "@System Generated Album@", Creation_Date = DateTime.Now, User_ID = userId, Status = 1 };
                    db.Albums.Add(album);
                }

                album.Contents.Add(content);


                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {

                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine(@"Entity of type ""{0}"" in state ""{1}"" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine(@"- Property: ""{0}"", Value: ""{1}"", Error: ""{2}""",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage);
                        }
                    }
                    //throw;
                    return new StatusReport
                    {
                        Status = StatusReport.Danger,
                        Title = "Save Failed",
                        Description = image.FileName
                    };
                }
            }

            // Save a Thumbnail of the Image
            ImageProcessor imageProcessor = new ImageProcessor();

            Image thumb = imageProcessor.CreateThumbnail(Image.FromStream(image.InputStream, true, true));

            thumb.Save(thumbFullPath, ImageFormat.Bmp);



            return new StatusReport
            {
                Status = StatusReport.Success,
                Title = "Successfully Saved",
                Description = image.FileName
            };



        }

        private bool isValid(HttpPostedFileBase image)
        {
            return image != null && (image.ContentType.Equals("image/jpg") ||
                                     image.ContentType.Equals("image/png") ||
                                     image.ContentType.Equals("image/gif") ||
                                     image.ContentType.Equals("image/jpeg"));
        }

	    [AllowAnonymous]
        public ActionResult Stumble()
        {

            using (PixurfDBContext db = new PixurfDBContext())
            {
                if (User.Identity.IsAuthenticated)
                {
                    string userId = User.Identity.GetUserId();
                    UserRelationship relationship = new UserRelationship();
                    List<string> followedPeoplesId = relationship.GetFollowedPeoplesId(userId);

                    List<Content> contents = db.Contents.Where(c => //c.User_ID != userId &&
                                                          (c.Access == "Public" || (followedPeoplesId.Contains(c.User_ID) && ( c.Access != "Public" || c.Access != "Follower"))))
                        .OrderBy(c => Guid.NewGuid()).Take(1).ToList();

                    if (contents.Count > 0)
                    {
                        int contentId = contents[0].Content_ID;
                        return RedirectToAction("View", "Content", new { id = contentId });
                    }
                    
                }
                else
                {
                    List<Content> contents = db.Contents.Where(c => c.Access == "Public").OrderBy(c => Guid.NewGuid()).Take(1).ToList();
                    if (contents.Count > 0)
                    {
                        int contentId = contents[0].Content_ID;
                        return RedirectToAction("View", "Content", new {id = contentId});
                    }
                }

                //Error Report
                StatusReport report = new StatusReport
                {
                    Title = "Error",
                    Description = "Something went wrong. Please try again",
                    Status = StatusReport.Warning
                };

                List<StatusReport> reports = new List<StatusReport>();
                reports.Add(report);

                Session["Reports"] = reports;
                return RedirectToAction("Index", "Home");
            }
        }
    }
}