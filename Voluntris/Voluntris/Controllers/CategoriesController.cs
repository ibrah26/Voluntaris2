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
    public class CategoriesController : Controller
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();
        AccionsComunesController accio = new AccionsComunesController();

        [Authorize(Roles = "Root, RolAdministrador")]
        // GET: Categories
        public ActionResult Index()
        {
            //Inciialitzem per crar el usuari SuperRoot ( SHA DE CANVIAR !!!!!!!!!!!!)
            IncialitzadorDb.Inicialitzador(db);
           
            var categories = db.Categories.Include(c => c.AdministradorCategoria);

            string idUsuari = User.Identity.GetUserId();

            string rolUsuari = accio.getRolUsuari(idUsuari, false);


            if (rolUsuari.Equals("Root")){

                return View(categories.ToList());

            }else if (rolUsuari.Equals("RolAdministrador")){
                categories = db.Categories.Include(c => c.AdministradorCategoria).Where( x => x.AdministradorCategoriaID == idUsuari);

                return View(categories.ToList());
            }else{
                return RedirectToAction("Index", "Home");
            }

        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categoria categoria = db.Categories.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            string idUsuari = User.Identity.GetUserId();

            if (categoria.AdministradorCategoriaID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                return View(categoria);
            }else{
                return RedirectToAction("Index", "Home");
            }
     
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            //ViewBag.AdministradorCategoriaID = new SelectList(db.Users.OfType<Administrador>(), "Id", "UserName");
            return View();
        }

        // POST: Categories/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,NomCategoria,DescripcioCategoria,AdministradorCategoriaID,ImatgeCategoria")] Categoria categoria)
        {

            string idUsuari = User.Identity.GetUserId();

            categoria.AdministradorCategoriaID = idUsuari;

            if (ModelState.IsValid)
            {
                db.Categories.Add(categoria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.AdministradorCategoriaID = new SelectList(db.Users.OfType<Administrador>(), "Id", "UserName", categoria.AdministradorCategoriaID);
            return View(categoria);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categoria categoria = db.Categories.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }
            //ViewBag.AdministradorCategoriaID = new SelectList(db.Users, "Id", "PasswordHash", categoria.AdministradorCategoriaID);
            
            string idUsuari = User.Identity.GetUserId();

            if (categoria.AdministradorCategoriaID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                return View(categoria);
            }
            else {
                return RedirectToAction("Index", "Home");
            }
            
         
        }

        // POST: Categories/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,NomCategoria,DescripcioCategoria,AdministradorCategoriaID,ImatgeCategoria")] Categoria categoria)
        {

            if (ModelState.IsValid)
            {
                string idUsuari = User.Identity.GetUserId();

                if (categoria.AdministradorCategoriaID == idUsuari || accio.esRoot(idUsuari) == true)
                {

                    db.Entry(categoria).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else {
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.AdministradorCategoriaID = new SelectList(db.Users, "Id", "PasswordHash", categoria.AdministradorCategoriaID);
            return View(categoria);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categoria categoria = db.Categories.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            string idUsuari = User.Identity.GetUserId();

            if (categoria.AdministradorCategoriaID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                return View(categoria);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Categoria categoria = db.Categories.Find(id);

            string idUsuari = User.Identity.GetUserId();

            if (categoria.AdministradorCategoriaID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                db.Categories.Remove(categoria);
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
