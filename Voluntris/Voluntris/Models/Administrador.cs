using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Voluntris.Models
{
    public class Administrador : User
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        public virtual String DelegacioAdministradorID { get; set; }
        public virtual Delegacio DelegacioAdministrador { get; set; }
        public virtual ICollection<Categoria> CategoriesAdministrador { get; set; }
        public virtual ICollection<Projecte> ProjectesAdministrador { get; set; }
        public virtual string ImatgeAdministrador { get; set; }
        /*public virtual ICollection<Roles> RolsAseleccionar { get {


            ICollection roles = new ICollection(db.Roles, "ID", "Name");

            return null;
        } }*/

    }
}