using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Voluntris.Models;
using Microsoft.AspNet.Identity;

namespace Voluntris.Controllers
{
    [Authorize(Roles = "Root, RolAdministrador")]
    public class ProjectesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        AccionsComunesController accio = new AccionsComunesController();

        // GET: Projectes
        public ActionResult Index()
        {
            string idUsuari = User.Identity.GetUserId();

            var projectes = db.Projectes.Include(p => p.AdminstradorProjecte).Include(p => p.CategoriaProjecte).Include(p => p.DelegacioProjecte);

            string Rol = accio.getRolUsuari(idUsuari , false);

            if (Rol.Equals("RolAdministrador")){
                projectes = db.Projectes
                    .Include(p => p.CategoriaProjecte).Where( x => x.AdminstradorProjecteID == idUsuari);

            }
            //else if (Rol.Equals("RolVoluntari"))
            //{
                
            /*
                projectes = db.Projectes
                     .Include(p => p.CategoriaProjecte).Where(x => x.AdminstradorProjecteID == voluntari.DelegacioVoluntariID);
            }*/
            //Mirar si pongo el else , hay que tener cuidado ya que tenemos que controlar el root
            
            return View(projectes.ToList());
        }

        // GET: Projectes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projecte projecte = db.Projectes.Find(id);
            if (projecte == null)
            {
                return HttpNotFound();
            }

            string idUsuari = User.Identity.GetUserId();

            if (projecte.AdminstradorProjecteID == idUsuari || accio.esRoot(idUsuari) == true){
                return View(projecte);
            
            }else {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Projectes/Create
        public ActionResult Create()
        {
            //Per defecte
            //ViewBag.AdminstradorProjecteID = new SelectList(db.Users, "Id", "PasswordHash");UserName
            //ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID");

            //Les comentem per que ha de agafar el administrador actual
            //ViewBag.AdminstradorProjecteID = new SelectList(db.Users.OfType<Administrador>(), "Id", "UserName");
            string idUsuari = User.Identity.GetUserId();

            //ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria");
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories.Where( x => x.AdministradorCategoriaID == idUsuari ), "ID", "NomCategoria");

            //Les comentem per que ha de agafar la Dlegacio del Administrador actual
            //ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio");
            return View();
        }

        // POST: Projectes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public ActionResult Create([Bind(Include = "ID,NomProjecte,DescripcioProjecte,CategoriaProjecteID,AdminstradorProjecteID,DelegacioProjecteID,ImatgeProjecte")] Projecte projecte)
        public ActionResult Create([Bind(Include = "ID,NomProjecte,DescripcioProjecte,CategoriaProjecteID,ImatgeProjecte")] Projecte projecte)
        {

            string idUsuariActual = User.Identity.GetUserId();

            projecte.AdminstradorProjecteID = idUsuariActual;
            //Posem com a id delegació el mateix ide que el del administrador ja que la de lagació d'un Administrdor comparteix id amb ell.
            projecte.DelegacioProjecteID = idUsuariActual;

            if (ModelState.IsValid)
            {
                
                db.Projectes.Add(projecte);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.AdminstradorProjecteID = new SelectList(db.Users.OfType<Administrador>(), "Id", "UserName", projecte.AdminstradorProjecteID);
            //ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria", projecte.CategoriaProjecteID);
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories.Where(x => x.AdministradorCategoriaID == idUsuariActual), "ID", "NomCategoria");
            //ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", projecte.DelegacioProjecteID);
            return View(projecte);
        }

        // GET: Projectes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projecte projecte = db.Projectes.Find(id);
            if (projecte == null)
            {
                return HttpNotFound();
            }
            
            string idUsuariActual = User.Identity.GetUserId();

            if (projecte.AdminstradorProjecteID == idUsuariActual || accio.esRoot(idUsuariActual) == true)
            {
                //ViewBag.AdminstradorProjecteID = new SelectList(db.Users.OfType<Administrador>(), "Id", "UserName", projecte.AdminstradorProjecteID);
                //ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria", projecte.CategoriaProjecteID);
                ViewBag.CategoriaProjecteID = new SelectList(db.Categories.Where(x => x.AdministradorCategoriaID == idUsuariActual), "ID", "NomCategoria");
                //ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio", projecte.DelegacioProjecteID);
                return View(projecte);
            }
            else {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // POST: Projectes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,NomProjecte,DescripcioProjecte,CategoriaProjecteID,AdminstradorProjecteID,DelegacioProjecteID,ImatgeProjecte")] Projecte projecte)
        {

            if (ModelState.IsValid)
            {
                string idUsuariActual = User.Identity.GetUserId();
                if (idUsuariActual == projecte.AdminstradorProjecteID)
                {
                    db.Entry(projecte).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else {
                    return RedirectToAction("Index", "Home");
                }
  
            }
            //ViewBag.AdminstradorProjecteID = new SelectList(db.Users, "Id", "PasswordHash", projecte.AdminstradorProjecteID);
            ViewBag.AdminstradorProjecteID = new SelectList(db.Users.OfType<Administrador>(), "Id", "UserName", projecte.AdminstradorProjecteID);
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria", projecte.CategoriaProjecteID);
            //ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", projecte.DelegacioProjecteID);NomDelegacio
            ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio", projecte.DelegacioProjecteID);
            return View(projecte);
        }

        // GET: Projectes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Projecte projecte = db.Projectes.Find(id);
            if (projecte == null)
            {
                return HttpNotFound();
            }

            string idUsuariActual = User.Identity.GetUserId();
            if (idUsuariActual == projecte.AdminstradorProjecteID)
            {

                return View(projecte);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // POST: Projectes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Projecte projecte = db.Projectes.Find(id);

            string idUsuariActual = User.Identity.GetUserId();
            if (idUsuariActual == projecte.AdminstradorProjecteID)
            {
                db.Projectes.Remove(projecte);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
          
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
