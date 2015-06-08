using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Voluntris.Models;
using Microsoft.AspNet.Identity.Owin;
using System.IO;

namespace Voluntris.Controllers
{
    [Authorize(Roles = "Root, RolAdministrador")]
    public class VoluntarisController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        AccionsComunesController accio = new AccionsComunesController();

        // GET: Voluntaris
        public ActionResult Index()
        {
            string idUsuari = User.Identity.GetUserId();

            //var users = db.Users.Include(v => v.DelegacioVoluntari);
            IQueryable<Voluntari> users = db.Users.OfType<Voluntari>().Include(a => a.DelegacioVoluntari);

            if(accio.esAdmin(idUsuari) == true){
                 users = db.Users.OfType<Voluntari>()
                    .Where(u => u.DelegacioVoluntariID == idUsuari)
                    .Include(a => a.DelegacioVoluntari);
            }
      
            return View(users.ToList());
        }

        // GET: Voluntaris/Details/5
        public ActionResult Details(string id)
        {
            string idUsuari = User.Identity.GetUserId();
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voluntari voluntari = (Voluntari)(db.Users.Find(id));
            if (voluntari == null)
            {
                return HttpNotFound();
            }
            if(voluntari.DelegacioVoluntariID == idUsuari){
                return View(voluntari);
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Voluntaris/Create
        public ActionResult Create()
        {
            string idUsuari = User.Identity.GetUserId();

            //ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID");

            if (accio.esAdmin(idUsuari) == true)
            {
                ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions.Where( x => x.ID == idUsuari), "AdministradorDelegacioID", "NomDelegacio");
            }
            else if (accio.esRoot(idUsuari) == true)
            {
                ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio");
            }

            return View();
        }

        // POST: Voluntaris/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,PasswordHash,PhoneNumber,Email,EmailConfirmed,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DataNeixement,DelegacioVoluntariID,ImatgeVoluntari")] Voluntari voluntari)
        public ActionResult Create([Bind(Include = "PasswordHash,PhoneNumber,Email,UserName,DataNeixement,DelegacioVoluntariID")] Voluntari voluntari)
        {
            string idUsuari = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                if (idUsuari == voluntari.DelegacioVoluntariID || accio.esRoot(idUsuari))
                {
                    /**************  IMATGE *************/
                    HttpPostedFileBase file = Request.Files["fileuploadImage"];

                    if(file != null){
                        // write your code to save image
                        string uploadPath = Server.MapPath("~/Content/images/");

                        string nomImatgeId = Guid.NewGuid().ToString();

                        string extencioImatge = Path.GetExtension(uploadPath + file.FileName);


                        file.SaveAs(uploadPath + nomImatgeId + extencioImatge);

                        voluntari.ImatgeVoluntari = nomImatgeId + extencioImatge;
                    }

                    /**************  IMATGE *************/
                
                    voluntari.Id = Guid.NewGuid().ToString();
                    voluntari.AccessFailedCount = 0;
                    //voluntari.Agegir rol voluntari
                    
                   
                    db.Users.Add(voluntari);
                    db.SaveChanges();

                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                    userManager.AddToRole(voluntari.Id, "RolVoluntari");



                    canviPaswd(voluntari).Wait();
                    return RedirectToAction("Index");
                } else {
                    return RedirectToAction("Index", "Home");
                }
                
            }

            ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio", voluntari.DelegacioVoluntariID);
            return View(voluntari);
        }

        private async Task canviPaswd(Voluntari administrador)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            UserStore<User> store = new UserStore<User>(context);
            UserManager<User> UserManager = new UserManager<User>(store);
            UserManager.RemovePassword(administrador.Id);
            await UserManager.AddPasswordAsync(administrador.Id, administrador.PasswordHash).ConfigureAwait(false);

        }

        // GET: Voluntaris/Edit/5
        public ActionResult Edit(string id)
        {
            string idUsuari = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voluntari voluntari = (Voluntari)(db.Users.Find(id));
            if (voluntari == null)
            {
                return HttpNotFound();
            }
            if (accio.esRoot(idUsuari))
            {
                ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio", voluntari.DelegacioVoluntariID);
                return View(voluntari);
            }else if (idUsuari == voluntari.DelegacioVoluntariID ){
                ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions.Where(x => x.ID == idUsuari), "AdministradorDelegacioID", "NomDelegacio", voluntari.DelegacioVoluntariID);
                return View(voluntari);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: Voluntaris/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PasswordHash,PhoneNumber,Email,EmailConfirmed,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DataNeixement,DelegacioVoluntariID,ImatgeVoluntari")] Voluntari voluntari)
        {
            string idUsuari = User.Identity.GetUserId();
            
            if (ModelState.IsValid)
            {
                if (voluntari.DelegacioVoluntariID == idUsuari || accio.esRoot(idUsuari)){
                    db.Entry(voluntari).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                } else {
                    return RedirectToAction("Index", "Home");
                }
               
            }
            ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio", voluntari.DelegacioVoluntariID);
            return View(voluntari);
        }

        // GET: Voluntaris/Delete/5
        public ActionResult Delete(string id)
        {
            string idUsuari = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voluntari voluntari = (Voluntari)(db.Users.Find(id));
            if (voluntari == null)
            {
                return HttpNotFound();
            }
            if(idUsuari == voluntari.DelegacioVoluntariID || accio.esRoot(idUsuari)){
                return View(voluntari);
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: Voluntaris/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            string idUsuari = User.Identity.GetUserId();
            Voluntari voluntari = (Voluntari)(db.Users.Find(id));

            if (idUsuari == voluntari.DelegacioVoluntariID || accio.esRoot(idUsuari))
            {
                db.Users.Remove(voluntari);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
