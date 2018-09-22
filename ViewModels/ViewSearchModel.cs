using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pixurf.DataModels;

namespace Pixurf.ViewModels
{
    public class ViewSearchModel
    {
        public ViewSearchModel()
        {
            Albums = new List<ViewAlbumSearch>();
            Contents = new List<ViewContentSearch>();
            Users = new List<ViewPeopleSearch>();
        }
        public List<ViewAlbumSearch> Albums { get; set; }
        public List<ViewContentSearch> Contents { get; set; }
        public List<ViewPeopleSearch> Users { get; set; }
        public string Query { get; set; }
        public string SortBy { get; set; }
        public string Target { get; set; }
    }


    public class ViewAlbumSearch
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string SortBy { get; set; }
        public string Description { get; set; }
    }

    public class ViewContentSearch
    {
        public ViewContentSearch()
        {
            Tags = new List<string>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public System.DateTime CreationDate { get; set; }
        public List<string> Tags { get; set; }
        public string SortBy { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
    }

    public class ViewPeopleSearch
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string SortBy { get; set; }
        public int NoofFollowers { get; set; }
    }

    public class Sort
    {
        public static readonly string Default = "Default";
        public static readonly string Date = "Date";
        public static readonly string Name = "Name";
        public static readonly string Popularity = "Popular";

    }
}