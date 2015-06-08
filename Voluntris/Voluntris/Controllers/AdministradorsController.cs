using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voluntris.Models;
using Microsoft.AspNet.Identity.Owin;


namespace Voluntris.Controllers
{
    [Authorize(Roles = "Root")]
    public class AdministradorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administradors
        public ActionResult Index()
        {
            //var users = db.Users.Include(a => a.DelegacioAdministrador);
            IQueryable<Administrador> users = db.Users.OfType<Administrador>().Include(a => a.DelegacioAdministrador)
                                                                            .Include(x => x.Roles);
                                                                            //  .Include("Roles");
            return View(users.ToList());
        }

        // GET: Administradors/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Administrador administrador = db.Users.Find(id);
            Administrador administrador = (Administrador)(db.Users.Find(id));
            if (administrador == null)
            {
                return HttpNotFound();
            }
            return View(administrador);
        }

        // GET: Administradors/Create
        public ActionResult Create()
        {
            ViewBag.Id = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID");
            return View();
        }

        // POST: Administradors/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,PasswordHash,PhoneNumber,Email,EmailConfirmed,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DelegacioAdministradorID,ImatgeVoluntari")] Administrador administrador)
        //Haig d'intentar fer que el Admin pugui tenir un UserName,
        //La DelegacioAdministradorID no s'escull aqui sino es quan es crea la delagació que esocllim quin es el seu administrador
        public ActionResult Create([Bind(Include = "PasswordHash,Email,PhoneNumber,UserName")] Administrador administrador)
        {
            if (ModelState.IsValid)
            {
                administrador.Id = Guid.NewGuid().ToString();
                administrador.DelegacioAdministradorID = null;
                db.Users.Add(administrador);
                db.SaveChanges();

                canviPaswd(administrador).Wait();
                return RedirectToAction("Index");
            }

            ViewBag.Id = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", administrador.Id);
            return View(administrador);
        }

        private async Task canviPaswd(Administrador administrador)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            UserStore<User> store = new UserStore<User>(context);
            UserManager<User> UserManager = new UserManager<User>(store);
            //String newPassword = "Pepepe3!"; //"<PasswordAsTypedByUser>";
            UserManager.RemovePassword(administrador.Id);
            await UserManager.AddPasswordAsync(administrador.Id, administrador.PasswordHash).ConfigureAwait(false);

        }
        // GET: Administradors/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Administrador administrador = (Administrador)(db.Users.Find(id));
            if (administrador == null)
            {
                return HttpNotFound();
            }

            List<IdentityRole> ListaRoles = (from r in db.Roles select r).ToList();

            ViewBag.rolnou = ListaRoles;

            //Agafo el rol d'usuari , si en te el poso al viewBag per que a la vista surti marcat

            string rolUsuari = getRolUsuari(administrador.UserName);

            ViewBag.rolUsuariAgafat = rolUsuari;
            
            ViewBag.Id = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", administrador.Id);
            return View(administrador);
        }

        public string getRolUsuari(string name)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            //IdentityRole role = roleManager.FindByName(roleName);
            User user = userManager.FindByName(name);

            if (user != null)
            {

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

        // POST: Administradors/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PasswordHash,PhoneNumber,Email,EmailConfirmed,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DelegacioAdministradorID,ImatgeVoluntari")] Administrador administrador, FormCollection radioButtons)
        //public ActionResult Edit([Bind(Include = "Id,Email,UserName,")] Administrador administrador, FormCollection radioButtons)
        {
            if (ModelState.IsValid)
            {
                var valors = radioButtons.GetValue("rolRadio");

                var nomRol =  valors.AttemptedValue;

                //Agafem el id del usuari i el nom del rol (Els hem d'insertar a la NetRolesUsers)

                //administrador.Id;

                InsertarUserRoles(administrador.UserName, nomRol);

                db.Entry(administrador).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", administrador.Id);
            return View(administrador);
        }

        public void InsertarUserRoles(string name, string roleName)
        {

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            IdentityRole role = roleManager.FindByName(roleName);
            User user = userManager.FindByName(name);

            if(role != null && user != null){

                // Add user  to Role  if not already added
                var rolesForUser = userManager.GetRoles(user.Id);
                if (!rolesForUser.Contains(role.Name))
                {
                   // var rolDeUsuaris = (from r in db.Users.Include("Roles") where r.Id == user.Id select r).ToList();
                    //User usuariSelec = (from r in db.Users where r.UserName == name select r).SingleOrDefault();
                    //IdentityRole rolEscollit = (from x in db.IdentityRoles where x.Name == roleName select x).SingleOrDefault();

                    var LlistaRoles = user.Roles.ToList();
                    foreach (var r in LlistaRoles)
                    {
                        user.Roles.Remove(r);
                        
                    }

                    //userManager.GetRoles(user.Id).Remove();                  //rolesForUser.SingleOrDefault().Remove();

                    var result = userManager.AddToRole(user.Id, role.Name);
                }
            }   
        }

        // GET: Administradors/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Administrador administrador = (Administrador)(db.Users.Find(id));
            if (administrador == null)
            {
                return HttpNotFound();
            }
            return View(administrador);
        }

        // POST: Administradors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Administrador administrador = (Administrador)(db.Users.Find(id));
            db.Users.Remove(administrador);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public static string HashPassword(string password)
        {
            /*byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);*/

            ///////////////////////////////////////////

            byte[] salt;
            byte[] buffer2;

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            // specify that we want to randomly generate a 20-byte salt
            using (var deriveBytes = new Rfc2898DeriveBytes(password, 20))
            {
                 salt = deriveBytes.Salt;
                 buffer2 = deriveBytes.GetBytes(20);  // derive a 20-byte key

                // save salt and key to database
            }
            
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
            
        }
    }
}
