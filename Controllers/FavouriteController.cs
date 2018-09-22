using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Pixurf.DataModels;
using Pixurf.Models;
using Microsoft.Ajax.Utilities;

namespace Pixurf.Controllers
{
    public class FavouriteController : ApiController
    {

        [System.Web.Http.HttpGet]
        //[Route("api/{controller}/favourite/{action}/{id}")]
        public FavouriteStatus Get(int id)
        {
            FavouriteStatus status = new FavouriteStatus { Status = false};

            if (User.Identity.IsAuthenticated)
            {
                string uid = User.Identity.GetUserId();
                using (PixurfDBContext db = new PixurfDBContext())
                {
                    status.Status = db.Favourites.Any(f => f.User_ID == uid && f.Content_ID == id);
                }

            }
            return status;
        }

        [System.Web.Http.HttpPut]
        public FavouriteStatus Set(int id)
        {
            FavouriteStatus status = new FavouriteStatus {Status = false};

            if (User.Identity.IsAuthenticated)
            {
                string uid = User.Identity.GetUserId();

                using (PixurfDBContext db = new PixurfDBContext())
                {
                    Favourite favourite = db.Favourites.FirstOrDefault(f => f.User_ID == uid && f.Content_ID == id);
                    Content content = db.Contents.Find(id);
                    if (content != null)
                    {
                        UserRelationship userRelationship = new UserRelationship();
                        
                        if ((content.User_ID == uid)
                            || ( userRelationship.Following(content.User_ID, uid) && content.Access != "Private")
                            || ( ! userRelationship.Blocked(content.User_ID, uid) && content.Access == "Public"))
                        {

                            if (favourite != null)
                            {
                                db.Favourites.Remove(favourite);
                                try
                                {
                                    db.SaveChanges();
                                    status.Status = false;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    status.Status = true;
                                }
                            }
                            else
                            {
                                db.Favourites.Add(new Favourite
                                {
                                    Content = content,
                                    User_ID = uid,
                                    Creation_Date = DateTime.Now
                                });
                                try
                                {
                                    db.SaveChanges();
                                    status.Status = true;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                        }
                    }
                }
            }
            return status;
        }
    }


    public class FavouriteStatus
    {
        public bool Status { get; set; }
    }
}
