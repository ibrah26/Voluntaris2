using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class Rol : IdentityRole
    {
        public Rol() : base() { }
        public Rol(string nomRol) : base(nomRol) { }
        public string Descripcio { get; set; }
    }
}