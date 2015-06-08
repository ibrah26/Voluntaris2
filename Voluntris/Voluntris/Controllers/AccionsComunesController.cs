using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Voluntris.Models;
using Microsoft.AspNet.Identity.Owin;

using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;




namespace Voluntris.Controllers
{
    public class AccionsComunesController : Controller
    {

        public  ApplicationDbContext db = new ApplicationDbContext();
        // GET: AccionsComunes
        public ActionResult Index()
        {
            return View();
        }

        public string getUsuariActual()
        {
            string idUsuari;
            return idUsuari = User.Identity.GetUserId();
        }

        public  string getRolUsuari(string name, bool perNom)
        {
            //var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            
            //IdentityRole role = roleManager.FindByName(roleName);

            // var rolDeUsuaris = (from r in db.Users.Include("Roles") where r.Id == user.Id select r).ToList();
            //User usuariSelec = (from r in db.Users where r.UserName == name select r).SingleOrDefault();
            //IdentityRole rolEscollit = (from x in db.IdentityRoles where x.Name == roleName select x).SingleOrDefault();

            User user;

            if (perNom == true)
            {
                //user = userManager.FindById(name);
                user = getUsuariPerNom(name);
            }
            else {
                //user = userManager.FindByName(name);
                user = getUsuariPerId(name);
            }

            if (user != null){

                var LlistaRoles = user.Roles.ToList();
                foreach (var r in LlistaRoles)
                {
                    //r.RoleId
                    
                    IdentityRole role = roleManager.FindById(r.RoleId);

                    return role.Name;
                }
            }

            return "";
        }

        public  User getUsuariPerNom(string name) {
            User user;
            return  user = (from r in db.Users where r.UserName == name select r).SingleOrDefault();
        }

        public  User getUsuariPerId(string id)
        {
            User user;
            return user = (from r in db.Users where r.Id == id select r).SingleOrDefault();
        }

        public bool esRoot(string id){

            string rol = getRolUsuari(id, false);

            if (rol.Equals("Root"))
            {
                return true;
            }
            else {
                return false;
            }
        }

        public bool esAdmin(string id){

            string rol = getRolUsuari(id, false);

            if (rol.Equals("RolAdministrador"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool EsVoluntari(string id){

            string rol = getRolUsuari(id, false);

            if (rol.Equals("RolVoluntari"))
            {
                return true;
            }else{
                return false;
            }
        }

        public Administrador getAdministrador(string id){

            Administrador administrador = db.Users.OfType<Administrador>()
                .Where(v => v.Id == id)
                .Include(a => a.DelegacioAdministrador).SingleOrDefault();

            return administrador;
        }

        public Voluntari getVoluntariPerId(string id){

            Voluntari voluntari = db.Users.OfType<Voluntari>()
                .Where(v => v.Id == id)
                .Include(a => a.DelegacioVoluntari).SingleOrDefault();

            return voluntari;
        }

        public ActionResult redireccioAlHome()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task canviPaswd(Voluntari administrador)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            UserStore<User> store = new UserStore<User>(context);
            UserManager<User> UserManager = new UserManager<User>(store);
            UserManager.RemovePassword(administrador.Id);
            await UserManager.AddPasswordAsync(administrador.Id, administrador.PasswordHash).ConfigureAwait(false);

        }
    }
}