using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facebook.Models
{
    public class Login
    {
        public string email { get; set; }
        public string password { get; set; }
        public bool save{ get; set; }
    }
}