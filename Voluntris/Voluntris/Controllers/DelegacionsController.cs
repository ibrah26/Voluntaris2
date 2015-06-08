using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Voluntris.Models;

namespace Voluntris.Controllers
{
    public class DelegacionsController : Controller
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();
        [Authorize(Roles = "Root")]
        // GET: Delegacions
        public ActionResult Index()
        {
            var delegacions = db.Delegacions.Include(d => d.AdministradorDelegacio);
            return View(delegacions.ToList());
        }

        // GET: Delegacions/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delegacio delegacio = db.Delegacions.Find(id);
            if (delegacio == null)
            {
                return HttpNotFound();
            }
            return View(delegacio);
        }

        // GET: Delegacions/Create
        public ActionResult Create()
        {
            //IQueryable<Administrador> users = db.Users.OfType<Administrador>().Include(a => a.DelegacioAdministrador);
            ViewBag.AdministradorDelegacioID = new SelectList(db.Users.OfType<Administrador>()
                                                                        //.Where( x => x.DelegacioAdministradorID == null), "Id", "PasswordHash");
                                                                        .Where(x => x.DelegacioAdministradorID == null), "Id", "Email");
            return View();
        }

        // POST: Delegacions/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AdministradorDelegacioID,NomDelegacio")] Delegacio delegacio)
        {
            if (ModelState.IsValid)
            {
                delegacio.ID = delegacio.AdministradorDelegacioID;
                db.Delegacions.Add(delegacio);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AdministradorDelegacioID = new SelectList(db.Users, "Id", "PasswordHash", delegacio.AdministradorDelegacioID);
            return View(delegacio);
        }

        // GET: Delegacions/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delegacio delegacio = db.Delegacions.Find(id);
            if (delegacio == null)
            {
                return HttpNotFound();
            }
            ViewBag.AdministradorDelegacioID = new SelectList(db.Users, "Id", "PasswordHash", delegacio.AdministradorDelegacioID);
            return View(delegacio);
        }

        // POST: Delegacions/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AdministradorDelegacioID,ID,NomDelegacio")] Delegacio delegacio)
        {
            if (ModelState.IsValid)
            {
                db.Entry(delegacio).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AdministradorDelegacioID = new SelectList(db.Users, "Id", "PasswordHash", delegacio.AdministradorDelegacioID);
            return View(delegacio);
        }

        // GET: Delegacions/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delegacio delegacio = db.Delegacions.Find(id);
            if (delegacio == null)
            {
                return HttpNotFound();
            }
            return View(delegacio);
        }

        // POST: Delegacions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Delegacio delegacio = db.Delegacions.Find(id);
            db.Delegacions.Remove(delegacio);
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
    }
}
