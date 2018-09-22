using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Pixurf.DataModels;
using Pixurf.Models;
using Pixurf.ViewModels;

namespace Pixurf.Controllers
{
    [AllowAnonymous]
    public class SearchController : Controller
    {
        // GET: Search

        public ActionResult Index(string query,string category)
        {
            ViewSearchModel model = new ViewSearchModel
            {
                Query = "" + query,
                Target = "" + category
            };

            using (PixurfDBContext db = new PixurfDBContext() )
            {

                //Retrieve Users
                if (category == null ||  category.Equals("People") || category.Equals("All"))
                {
                    var queryable = db.Users.Where(user => user.Name.Contains(query) ||
                                                           (query.Contains("@") && user.Email.Contains(query)));
                    UserRelationship relationship = new UserRelationship();
                    foreach (User user in queryable)
                    {
                        model.Users.Add(new ViewPeopleSearch { Id = user.User_ID, Name = user.Name, Email = user.Email, NoofFollowers = relationship.NoOfFollowers(user.User_ID)});

                        if (model.Users.Count >= 5)break;
                    }
                }
                //Retrieve Contents
                if (category == null || category.Equals("Content") || category.Equals("All"))
                {
                    var queryable = db.Contents.Where(c => c.Title.Contains(query) || c.Description.Contains(query));

                    foreach (Content content in queryable)
                    {
                        bool add = false;
                        if (content.Access == "Public")
                        {
                            add = true;
                        }
                        else if (User.Identity.IsAuthenticated)
                        {
                            string uid = User.Identity.GetUserId();

                            if (content.User_ID.Equals(uid)) add = true;
                            else
                            {
                                UserRelationship relationship = new UserRelationship();
                                if (content.Access == "Follower" && relationship.Following(content.User_ID, uid)) add = true;
                                // if not blocked
                                    // lol..... what about non logged in users :P
                            }
                        }
                        if (add) model.Contents.Add(new ViewContentSearch { Id = content.Content_ID, Title = content.Title, Description = content.Description, Path = content.Path, OwnerId = content.User_ID, OwnerName = content.User.Name, CreationDate = content.Creation_Date });

                        if (model.Contents.Count >= 4)break;
                    }
                }

                //Retrieve Albums
                if (category == null || category.Equals("Album") || category.Equals("All"))
                {
                    var queryable = db.Albums.Where(a => a.Name.Contains(query) || a.Description.Contains(query));

                    foreach (Album album in queryable)
                    {
                        bool add = false;
                        if (album.Access == "Public")
                        {
                            add = true;
                        }
                        else if (User.Identity.IsAuthenticated)
                        {
                            string uid = User.Identity.GetUserId();

                            if (album.User_ID.Equals(uid)) add = true;
                            else
                            {
                                UserRelationship relationship = new UserRelationship();
                                if (album.Access == "Follower" && relationship.Following(album.User_ID, uid)) add = true;
                                // if not blocked
                                // lol..... what about non logged in users :P
                            }
                        }

                        if (add) model.Albums.Add(new ViewAlbumSearch {Id = album.Album_ID, Title = album.Name, OwnerId = album.User_ID, OwnerName = album.User.Name, CreationDate = album.Creation_Date});

                        if (model.Albums.Count >= 5) break;
                    }
                }
            }

            ViewBag.Title = ""+query;
            return View(model);
        }

        // GET: Tag
        [Route("Tag/{tag}")]
        public ActionResult Tag(string tag)
        {
            ViewSearchModel model = new ViewSearchModel
            {
                Query = "" + tag,
            };

            using (PixurfDBContext db = new PixurfDBContext())
            {

                
                IQueryable<Content> queryableContents = db.Contents.Where(c => c.Title.Contains("#"+ tag) || c.Description.Contains("#" + tag));

                foreach (Content content in queryableContents)
                {
                    bool add = false;
                    if (content.Access == "Public")
                    {
                        add = true;
                    }
                    else if (User.Identity.IsAuthenticated)
                    {
                        string uid = User.Identity.GetUserId();

                        if (content.User_ID.Equals(uid)) add = true;
                        else
                        {
                            UserRelationship relationship = new UserRelationship();
                            if (content.Access == "Follower" && relationship.Following(content.User_ID, uid)) add = true;
                            // if not blocked
                            // lol..... what about non logged in users :P
                        }
                    }
                    if (add) model.Contents.Add(new ViewContentSearch { Id = content.Content_ID, Title = content.Title, Description = content.Description, Path = content.Path, OwnerId = content.User_ID, OwnerName = content.User.Name, CreationDate = content.Creation_Date });

                    if (model.Contents.Count >= 4) break;
                }


                //Retrieve Albums

                IQueryable<Album> queryableAlbums = db.Albums.Where(a => a.Name.Contains("#" + tag) || a.Description.Contains("#" + tag));

                foreach (Album album in queryableAlbums)
                {
                    bool add = false;
                    if (album.Access == "Public")
                    {
                        add = true;
                    }
                    else if (User.Identity.IsAuthenticated)
                    {
                        string uid = User.Identity.GetUserId();

                        if (album.User_ID.Equals(uid)) add = true;
                        else
                        {
                            UserRelationship relationship = new UserRelationship();
                            if (album.Access == "Follower" && relationship.Following(album.User_ID, uid)) add = true;
                            // if not blocked
                            // lol..... what about non logged in users :P
                        }
                    }

                    if (add) model.Albums.Add(new ViewAlbumSearch { Id = album.Album_ID, Title = album.Name, OwnerId = album.User_ID, OwnerName = album.User.Name, CreationDate = album.Creation_Date });

                    if (model.Albums.Count >= 5) break;
                    
                }
            }

            ViewBag.Title = "#" + tag;
            return View(model);
        }



        [Route("Search/Keys/{key}")]
        public string Keys(string key)
        {

            if (key.IsNullOrWhiteSpace()) return null;

            List<string> list = new List<string> {"Storing", "Data", "on", "The", "Client", "with", "LocalStorage", "zabcdez", "xabcdex", "yabcdey" };
            List<string> list2 = new List<string>();


            foreach (string s in list)
            {
                if(s.Contains(key))list2.Add(new string(s.ToCharArray()));
            }

            var j = new JavaScriptSerializer().Serialize(list2);

            return j;
        }
    }
}