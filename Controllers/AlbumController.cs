using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Pixurf.DataModels;
using Pixurf.Models;
using Pixurf.ViewModels;
using Microsoft.Ajax.Utilities;

namespace Pixurf.Controllers
{
    [Authorize]
    public class AlbumController : System.Web.Mvc.Controller
    {
        // GET: Index
        // Get All Albums
        // So Far Complete
        public ActionResult Index()
        {
            List<StatusReport> reports = new List<StatusReport>();

            using (PixurfDBContext db = new PixurfDBContext())
            {
                string uid = User.Identity.GetUserId();
                User user = db.Users.Find(uid);

                if (user != null)
                {
                    ViewAlbumsModel allAlbumsModelModel = new ViewAlbumsModel();
                    List<Album> albums = null;
                    if (user.Admin)
                    {
                        albums = db.Albums.Where(a => a.User_ID == uid).Include(a => a.Contents).Take(10).ToList();
                    }
                    else
                    {
                        albums = db.Albums.Where(a => a.User_ID == uid && a.Status == 1).Include(a => a.Contents)
                            .Take(10).ToList();
                    }

                    foreach (Album album in albums)
                    {
                        allAlbumsModelModel.Albums.Add(album);
                    }

                    if(reports.Count > 0) Session["Reports"] = reports;
                    return View(allAlbumsModelModel);
                }
            }
            
            reports.Add(new StatusReport {
                Title = "Error !",
                Description = "User id not found.",
                Status = StatusReport.Danger
            });
                
            Session["Reports"] = reports;
            return RedirectToAction("Index", "Home");
        }
        
        // Get Album with id
        // So Far Complete
        [AllowAnonymous]
        public ActionResult View(int id)
        {
            List<StatusReport> reports = new List<StatusReport>();
            
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Album album = db.Albums.Find(id);
                if (album == null)
                {
                    StatusReport report = new StatusReport
                    {
                        Title = "Error",
                        Description = "Album not found",
                        Status = StatusReport.Info
                    };

                    reports.Add(report);

                    Session["Reports"] = reports;
                    return RedirectToAction("Index", "Home");
                }

                ViewAlbumModel albumModel = new ViewAlbumModel
                        {
                            Album_ID = album.Album_ID,
                            Name = album.Name,
                            Description = album.Description,
                            User = db.Users.Find(album.User_ID),
                            Access = album.Access,
                            Status = album.Status,
                            Creation_Date = album.Creation_Date,
                            UserAuthenticated = false
                        };

                if (album.Status == 1)
                {
                    if (album.User_ID == User.Identity.GetUserId())
                    {
                        if (reports.Count > 0) Session["Reports"] = reports;
                        albumModel.Contents = album.Contents.ToList();
                        albumModel.UserAuthenticated = true;
                        return View(albumModel);
                    }

                    UserRelationship relationship = new UserRelationship();


                    //Private
                    if (album.Access == "Private")
                    {
                        StatusReport report = new StatusReport
                        {
                            Title = "Access Denied",
                            Description = "This Album is not accessible",
                            Status = StatusReport.Warning
                        };

                        reports.Add(report);

                        Session["Reports"] = reports;
                        return RedirectToAction("Index", "Home");
                    }
                    //Follower
                    else if (album.Access == "Follower")
                    {
                        if (relationship.Following(album.User_ID, User.Identity.GetUserId()))
                        {
                            if (reports.Count > 0) Session["Reports"] = reports;
                            var temp = album.Contents.Where(c => c.Access != "Private").ToList();
                            albumModel.Contents = temp;
                            return View(albumModel);
                        }
                        else
                        {
                            StatusReport report = new StatusReport
                            {
                                Title = "Access Denied",
                                Description = "Only Followers can view this Album",
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
                        if (relationship.Following(album.User_ID, User.Identity.GetUserId()))
                        {
                            if (reports.Count > 0) Session["Reports"] = reports;
                            var temp = album.Contents.Where(c => c.Access != "Private").ToList();
                            albumModel.Contents = temp;
                            return View(albumModel);
                        }
                        else if (!relationship.Blocked(album.User_ID, User.Identity.GetUserId()))
                        {
                            if (reports.Count > 0) Session["Reports"] = reports;
                            var temp = album.Contents.Where(c => c.Access == "Public").ToList();
                            albumModel.Contents = temp;
                            return View(albumModel);
                        }
                        

                        StatusReport report = new StatusReport
                        {
                            Title = "Error",
                            Description = "Album not found",
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
                        albumModel.UserAuthenticated = true;
                        return View(albumModel);
                    }
                    else
                    {
                        StatusReport report = new StatusReport
                        {
                            Title = "Error",
                            Description = "Album not found",
                            Status = StatusReport.Warning
                        };

                        reports.Add(report);

                        Session["Reports"] = reports;
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        [HttpGet]
        // So Far Complete
        public ActionResult AddNew()
        {
            return View();
        }

        [HttpPost]
        // So Far Complete
        public ActionResult AddNew(ViewAlbumModel model)
        {
            List<StatusReport> reports = new List<StatusReport>();

            string userId = User.Identity.GetUserId();
            if (model != null && !model.Name.IsNullOrWhiteSpace() && !model.Access.IsNullOrWhiteSpace())
            {
                using (PixurfDBContext db = new PixurfDBContext())
                {
                    //Find the Album
                    Album album = db.Albums.Where(a => a.Name == model.Name && a.Status != 0 && a.User_ID == userId).OrderByDescending(a=>a.Creation_Date).ToList().FirstOrDefault();

                    //Create the album if its null
                    if (album == null)
                    {
                        //Create an album
                        album = new Album
                        {
                            Name = model.Name,
                            Creation_Date = DateTime.Now,
                            User_ID = userId,
                            Access = model.Access,
                            Status = 1,
                            Description = model.Description
                        };
                        db.Albums.Add(album);
                        
                        try
                        {
                            db.SaveChanges();
                        
                            reports.Add( new StatusReport
                            {
                                Title = "Success",
                                Description = "Album "+album.Name+" Successfully Created.",
                                Status = StatusReport.Success
                            });
                            Session["Reports"] = reports;
                        }
                        catch (Exception e)
                        {
                            reports.Add( new StatusReport
                            {
                                Title = "Error",
                                Description = "Something went wrong",
                                Status = StatusReport.Danger
                            });
                            Session["Reports"] = reports;
                            Console.WriteLine(e);
                        }
                    }
                    else
                    {
                        //Already Exists
                        reports.Add(new StatusReport
                        {
                            Title = "Failed",
                            Description = "Album " + album.Name + " Already Exists.",
                            Status = StatusReport.Warning
                        });
                        Session["Reports"] = reports;
                    }
                    return RedirectToAction("View", new {id = album.Album_ID});
                }
            }
            return View(model);
        }

        public ActionResult SlideShow(int id)
        {
            ViewAlbumModel albumModel;
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Album album = db.Albums.Find(id);
                albumModel = new ViewAlbumModel
                {
                    Album_ID = album.Album_ID,
                    Name = album.Name,
                    Description = album.Description,
                    User = db.Users.Find(album.User_ID),
                    Access = album.Access,
                    Status = album.Status,
                    Creation_Date = album.Creation_Date,
                    Contents = album.Contents.ToList()
                };
            }

            return View(albumModel);
        }

        // Delete Album with id
        // So Far Complete
        //public ActionResult Delete(int id)
        //{
        //    return Delete(id, "");
        //}

        [Route("Album/Delete/{id}/{All?}")]
        public ActionResult Delete(int id, string all)


        {
            bool deleteAll = false || (all != null && all.Equals("All"));

            List<StatusReport> reports = new List<StatusReport>();
            using (PixurfDBContext db = new PixurfDBContext())
            {
                Album album = db.Albums.Find(id);
                string userId = User.Identity.GetUserId();


                if (album == null)
                {
                    reports.Add(new StatusReport
                    {
                        Title = "Failed",
                        Description = "Album not found",
                        Status = StatusReport.Warning
                    });
                    Session["Reports"] = reports;
                    return RedirectToAction("Index","Album");
                }
                
            
                // check if user own's the content && not already deleted
                if (album.Status == 1 && album.User_ID == userId)
                {
                    if (album.Name == "@System Generated Album@" && album.Contents.Count > 0)
                    {
                        reports.Add(new StatusReport
                        {
                            Title = "Failed",
                            Description = "This Album can't be deleted as it's not empty.",
                            Status = StatusReport.Warning
                        });
                        Session["Reports"] = reports;
                        return RedirectToAction("Index", "Album");
                    }

                    album.Status = 0;

                    if (deleteAll)
                    {
                        foreach (Content content in album.Contents)
                        {
                            content.Status = 0;
                        }
                    }
                    else
                    {
                        //Find the Default Album
                        Album defaultAlbum = db.Albums.FirstOrDefault(a => a.Name == "@System Generated Album@" && a.User_ID == userId);

                        //Create the album if its null
                        if (defaultAlbum == null)
                        {
                            defaultAlbum = new Album
                            {
                                Name = "@System Generated Album@",
                                Creation_Date = DateTime.Now,
                                User_ID = userId,
                                Status = 1,
                                Access = "Private"
                            };
                        }
                        foreach (Content content in album.Contents)
                        {
                            content.Album = defaultAlbum;
                        }
                        db.Albums.Add(defaultAlbum);
                    }

                    try
                    {
                        db.SaveChanges();

                        reports.Add(new StatusReport
                        {
                            Title = "Success",
                            Description = "Album " + album.Name + " successfully deleted.",
                            Status = StatusReport.Success
                        });
                        Session["Reports"] = reports;
                    }
                    catch (Exception e)
                    {
                        reports.Add(new StatusReport
                        {
                            Title = "Error",
                            Description = "Something went wrong",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;
                        Console.WriteLine(e);
                    }
                    return RedirectToAction("Index", "Album");
                }
                else
                {
                    var user = db.Users.Find(User.Identity.GetUserId());
                    if (user != null && user.Admin)
                    {
                        if (album.Name == "@System Generated Album@" && album.Contents.Count > 0)
                        {
                            reports.Add(new StatusReport
                            {
                                Title = "Failed",
                                Description = "This Album can't be deleted as it's not empty.",
                                Status = StatusReport.Warning
                            });
                            Session["Reports"] = reports;
                            return RedirectToAction("Index", "Album");
                        }

                        if (deleteAll)
                        {
                            List<Content> contents = album.Contents.ToList();
                            foreach (Content content in contents)
                            {
                                db.Contents.Remove(content);
                            }
                        }
                        else
                        {
                            //Find the Default Album
                            Album defaultAlbum =
                                db.Albums.FirstOrDefault(
                                    a => a.Name == "@System Generated Album@" && a.User_ID == album.User_ID);

                            //Create the album if its null
                            if (defaultAlbum == null)
                            {
                                defaultAlbum = new Album
                                {
                                    Name = "@System Generated Album@",
                                    Creation_Date = DateTime.Now,
                                    User_ID = album.User_ID,
                                    Status = 1
                                };
                                db.Albums.Add(album);
                            }
                            foreach (Content content in album.Contents)
                            {
                                content.Album = defaultAlbum;
                            }
                        }

                        db.Albums.Remove(album);

                        try
                        {
                            db.SaveChanges();
                            reports.Add(new StatusReport
                            {
                                Title = "Success",
                                Description = "Album " + album.Name + " has been deleted permanently.",
                                Status = StatusReport.Success
                            });
                            Session["Reports"] = reports;
                            //Redirect to target users album index
                            return RedirectToAction("Index", "Home");

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            reports.Add(new StatusReport
                            {
                                Title = "Success",
                                Description = "Album " + album.Name + " Creation Successful.",
                                Status = StatusReport.Success
                            });
                            Session["Reports"] = reports;
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        reports.Add(new StatusReport
                        {
                            Title = "Error",
                            Description = "Album not found",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;
                    
                        return RedirectToAction("Index", "Album");
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult Edit(ViewAlbumModel model)
        {
            List<StatusReport> reports = new List<StatusReport>();

            string userId = User.Identity.GetUserId();
            if (model != null && !model.Name.IsNullOrWhiteSpace() && !model.Access.IsNullOrWhiteSpace())
            {
                using (PixurfDBContext db = new PixurfDBContext())
                {
                    //Find the Album
                    Album album = db.Albums.Where(a => a.Album_ID == model.Album_ID)
                        .OrderByDescending(a => a.Creation_Date).ToList().FirstOrDefault();

                    if (album == null)
                    {
                        reports.Add(new StatusReport
                        {
                            Title = "Error",
                            Description = "Album not found",
                            Status = StatusReport.Danger
                        });
                        Session["Reports"] = reports;
                        return RedirectToAction("Index");
                    }
                    User user = db.Users.Find(userId);
                    if ( (album.User_ID == userId && album.Status == 1) || ( user != null && user.Admin))
                    {
                        if (album.Name == model.Name && album.Description == model.Description &&
                            album.Access == model.Access)
                        {
                            reports.Add(new StatusReport
                            {
                                Title = "Success",
                                Description = "No change is made",
                                Status = StatusReport.Success
                            });
                            Session["Reports"] = reports;
                            return RedirectToAction("View", "Album", new {id = album.Album_ID});

                        }
                        else
                        {
                            album.Name = model.Name;
                            album.Description = model.Description;
                            album.Access = model.Access;

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
                                return RedirectToAction("View", "Album", new {id = album.Album_ID});
                                
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
            return View(model);
        }
    }
}