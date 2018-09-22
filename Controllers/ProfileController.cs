using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Pixurf.DataModels;
using Pixurf.Models;
using Pixurf.ViewModels;

namespace Pixurf.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // GET: Profile
        [AllowAnonymous]
        public ActionResult Index(string ID)
        {
            List<StatusReport> reports = new List<StatusReport>();

            if (ID.IsNullOrWhiteSpace())
            {
                if (User.Identity.IsAuthenticated) ID = User.Identity.GetUserId();
            }

            if (!ID.IsNullOrWhiteSpace())
            {
                using (PixurfDBContext db = new PixurfDBContext())
                {
                    User user = db.Users.Find(ID);

                    if (user != null)
                    {
                        UserRelationship relationship = new UserRelationship();
                        ViewUserModel userModel = new ViewUserModel
                        {
                            User_ID = user.User_ID,
                            Name = user.Name,
                            UserName = user.UserName,
                            About_Me = user.About_Me,
                            Admin = user.Admin,
                            Country = user.Country,
                            Email = user.Email,
                            Joining_Date = user.Joining_Date,
                            PhoneNumber = user.PhoneNumber,
                            Pro_Pic_ID = user.Pro_Pic_ID,
                            Status = user.Status,
                            Followers = relationship.NoOfFollowers(user.User_ID)
                        };

                        if (User.Identity.IsAuthenticated)
                        {
                            if (User.Identity.GetUserId().Equals(ID)) userModel.MyProfile = true;
                        }
                        

                        userModel.Albums = this.GetViewableAlbums(user.User_ID);
                        userModel.PopularContents = this.GetViewableContents(user.User_ID);


                        return View(userModel);
                    }
                }
            }



            reports.Add(new StatusReport
            {
                Title = "Error",
                Description = "Profile not found",
                Status = StatusReport.Danger
            });
            Session["Reports"] = reports;
            return RedirectToAction("Index", "Home");
        }

        
        private List<Content> GetViewableContents(string ownerId)
        {
            List<Content> contents = new List<Content>();
            string viewerId = "";
            if (User.Identity.IsAuthenticated) viewerId = User.Identity.GetUserId();

            using (PixurfDBContext db = new PixurfDBContext())
            {
                if (viewerId.IsNullOrWhiteSpace())
                {
                    //Only the public contents
                    contents = db.Contents.Where(a => a.User_ID == ownerId && a.Access == "Public" && a.Status == 1).Take(4).ToList();
                }
                else
                {
                    User viewer = db.Users.Find(viewerId);

                    if (viewer != null)
                    {
                        if (viewer.Admin)
                        {
                            contents = db.Contents.Where(a => a.User_ID == ownerId).Take(4).ToList();
                        }
                        else if (viewer.User_ID == ownerId)
                        {
                            contents = db.Contents.Where(a => a.User_ID == ownerId && a.Status == 1).Take(4).ToList();
                        }
                        else
                        {
                            //Handle followers
                            UserRelationship relationship = new UserRelationship();
                            if (relationship.Following(ownerId, viewerId))
                            {
                                contents = db.Contents
                                    .Where(a => a.User_ID == ownerId && a.Status == 1 &&
                                                (a.Access == "Public" || a.Access == "Follower")).Take(4).ToList();
                            }
                            else
                            {
                                contents = db.Contents
                                    .Where(a => a.User_ID == ownerId && a.Status == 1 &&
                                                a.Access == "Public").Take(4).ToList();
                            }
                        }
                    }
                }
            }


            return contents;
        }

        private ViewAlbumsModel GetViewableAlbums(string ownerId)
        {
            ViewAlbumsModel albums = new ViewAlbumsModel();

            string viewerId = "";
            if (User.Identity.IsAuthenticated) viewerId = User.Identity.GetUserId();

            using (PixurfDBContext db = new PixurfDBContext())
            {
                if (viewerId.IsNullOrWhiteSpace())
                {
                    //Only the public contents
                    albums.Albums = db.Albums.Where(a => a.User_ID == ownerId && a.Access == "Public" && a.Status == 1).Include(a => a.Contents).Take(10).ToList();
                }
                else
                {
                    User viewer = db.Users.Find(viewerId);

                    if (viewer != null)
                    {
                        if (viewer.Admin)
                        {
                            albums.Albums = db.Albums.Where(a => a.User_ID == ownerId).Include(a => a.Contents).Take(10).ToList();
                        }
                        else if (viewer.User_ID == ownerId)
                        {
                            albums.Albums = db.Albums.Where(a => a.User_ID == ownerId && a.Status == 1).Include(a => a.Contents).Take(10).ToList();
                        }
                        else
                        {
                            //Handle followers
                            UserRelationship relationship = new UserRelationship();
                            if (relationship.Following(ownerId, viewerId))
                            {
                                albums.Albums = db.Albums.Where(a => a.User_ID == ownerId && a.Status == 1 && (a.Access == "Public" || a.Access == "Follower")).Include(a => a.Contents).Take(10).ToList();
                            }
                            else
                            {
                                albums.Albums = db.Albums
                                    .Where(a => a.User_ID == ownerId && a.Status == 1 &&
                                                a.Access == "Public").Include(a => a.Contents).Take(10).ToList();
                            }
                        }
                    }
                }
            }


            return albums;
        }


        public ActionResult Manage()
        {
            return RedirectToAction("Index", "Manage");
        }

        public ActionResult Settings()
        {
            throw null;
        }

        public ActionResult Edit()
        {
            return View();
        }
    }


    
}