using System;
using System.CodeDom;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Pixurf.DataModels;
using Pixurf.Models;

namespace Pixurf.Controllers
{
    public class RelationController : ApiController
    {
        [System.Web.Http.HttpGet]
        //[Route("api/{controller}/follow/{action}/{id}")]
        public RelationStatus Get(string id)
        {
            RelationStatus status = new RelationStatus {Type = RelationStatus.Follow};
            if (User.Identity.IsAuthenticated && !id.IsNullOrWhiteSpace())
            {
                string uid = User.Identity.GetUserId();
                UserRelationship userRelationship = new UserRelationship();
                status.Status = userRelationship.Following(id, uid);
                
            }
            return status;
        }

        [System.Web.Http.HttpPut]
        public RelationStatus Set(string id)
        {
            RelationStatus status = new RelationStatus { Type = RelationStatus.Follow };

            if (User.Identity.IsAuthenticated && !id.IsNullOrWhiteSpace())
            {
                string uid = User.Identity.GetUserId();

                using (PixurfDBContext db = new PixurfDBContext())
                {
                    User followingUser = db.Users.Find(id);
                    if (followingUser != null)
                    {
                        UserRelationship userRelationship = new UserRelationship();
                        bool following = userRelationship.Following(id, uid);
                        if (following)
                        {
                            db.User_Relations.Remove(db.User_Relations.FirstOrDefault(r => r.User_ID == uid && r.Related_User_ID == id && r.Status == "Follow"));

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
                            bool isBlocked = userRelationship.Blocked(id, uid);
                            if (!isBlocked)
                            {
                                User_Relation relation = new User_Relation
                                {
                                    User_ID = uid,
                                    Related_User_ID = id,
                                    Status = "Follow"
                                };

                                db.User_Relations.Add(relation);

                                try
                                {
                                    db.SaveChanges();
                                    status.Status = true;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    status.Status = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                status.Status = false;
            }
            return status;
        }
    }


    public class RelationStatus
    {
        public static readonly string Follow = "Follow";
        public static readonly string Block = "Block";

        public bool Status { get; set; }
        public string Type { get; set; }
    }
}
