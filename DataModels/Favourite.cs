//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pixurf.DataModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class Favourite
    {
        public string User_ID { get; set; }
        public int Content_ID { get; set; }
        public System.DateTime Creation_Date { get; set; }
    
        public virtual Content Content { get; set; }
        public virtual User User { get; set; }
    }
}
