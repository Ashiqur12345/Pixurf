using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pixurf.DataModels;

namespace Pixurf.Models
{
    public class UserRelationship
    {
        private PixurfDBContext db;
        public UserRelationship()
        {
            db = new PixurfDBContext();
        }

        public bool Following(String userId, String followerId)
        {
            User_Relation relation = 
                db.User_Relations.FirstOrDefault(
                    r => r.User_ID == followerId && r.Related_User_ID == userId && r.Status == "Follow");
            if (relation != null)
            {
                return true;
            }
            return false;
        }

        public User_Relation Following(String userId, String followerId, bool getReference)
        {
            User_Relation relation =
                db.User_Relations.FirstOrDefault(
                    r => r.User_ID == followerId && r.Related_User_ID == userId && r.Status == "Follow");
            return relation;
        }

        public bool Blocked(String userId, String blockedId)
        {
            User_Relation relation = 
                db.User_Relations.FirstOrDefault(
                    r => r.User_ID == userId && r.Related_User_ID == blockedId && r.Status == "Block");
            if (relation != null)
            {
                return true;
            }
            return false;
        }

        public User_Relation Blocked(String userId, String blockedId, bool getReference)
        {
            User_Relation relation =
                db.User_Relations.FirstOrDefault(
                    r => r.User_ID == userId && r.Related_User_ID == blockedId && r.Status == "Block");
            return relation;
        }

        public int NoOfFollowers(String userId)
        {
            return db.User_Relations.Where(r => r.Related_User_ID == userId && r.Status == "Follow").ToList().Count;
        }

        public List<string> GetFollowedPeoplesId(string userId)
        {
            List<string> ids = new List<string>();

            foreach (User_Relation userRelation in db.User_Relations
                .Where(r => r.Related_User_ID == userId && r.Status == "Follow").ToList())
            {
                ids.Add(userRelation.Related_User_ID);
            }

            return ids;
        }
    }
}