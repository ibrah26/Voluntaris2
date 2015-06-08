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
using Voluntris.ViewModels;

namespace Voluntris.Controllers
{
    [Authorize(Roles = "RolVoluntari")]
    public class ProjectesUserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        AccionsComunesController accio = new AccionsComunesController();

        // GET: ProjectesUser
        public ActionResult Index()
        {
            string idUsuari = User.Identity.GetUserId();

            //Voluntari voluntari = db.Users.OfType<Voluntari>().Where( x => x.Id == idUsuari).FirstOrDefault();
            Voluntari voluntari = accio.getVoluntariPerId(idUsuari);

            var projectes = db.Projectes.Include(p => p.AdminstradorProjecte)
                                        .Include(p => p.CategoriaProjecte)
                                        .Include(p => p.DelegacioProjecte)
                                        .Where( p => p.DelegacioProjecteID == voluntari.DelegacioVoluntariID);
            return View(projectes.ToList());
        }

        public string getUsuariActual() {
            string idUsuari = User.Identity.GetUserId();
            
            return idUsuari;
        }

        // GET: ProjectesUser/Details/5
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

            string IdUsuari = getUsuariActual();

            Voluntari voluntari = accio.getVoluntariPerId(IdUsuari);

            if(projecte.DelegacioProjecteID == voluntari.DelegacioVoluntariID){
                List<FranjaHoraria> franjesDelProjecte = db.FranjesHoraries.Where(x => x.ProjecteFranjaHorariaID == id).ToList();

                //ViewBag.Franjes = franjesDelProjecte;

                ViewBag.IdProjecte = projecte.ID;

                List<FranjaHoraria> LlistaFranjesSelec = getFranjesDelActualVoluntariDeUnProjecte(projecte.ID);

                List<FranjaVoluntari> franjesVoluntari = CrearFranjesVoluntari(franjesDelProjecte, LlistaFranjesSelec);

                ViewBag.Franjes = franjesVoluntari;

                //ViewBag.franjesSelec = LlistaFranjesSelec;

                return View(projecte);
            }

            //return accio.redireccioAlHome();
            return RedirectToAction("Index");
        }

        //get franjes del projecte que el actual usuari ja esta apuntat
        public List<FranjaHoraria> getFranjesDelActualVoluntariDeUnProjecte(int idProjecte)
        {

            string idUsuari = User.Identity.GetUserId();

            List<VoluntarisEnFranjes> VoluntariEnFranjaDeUnProjecte = db.voluntarisEnFranjes.Where(x => x.VoluntariVFID == idUsuari)
                                                                                            .Where(x => x.FranjaHorariaVF.ProjecteFranjaHorariaID == idProjecte).ToList();

            List<FranjaHoraria> franjesHorariesSelecDelVoluntari = new List<FranjaHoraria>();

            foreach (VoluntarisEnFranjes VolEnFranj in VoluntariEnFranjaDeUnProjecte)
            {

                franjesHorariesSelecDelVoluntari.Add(VolEnFranj.FranjaHorariaVF);
            }

            return franjesHorariesSelecDelVoluntari;
        }

        public List<FranjaVoluntari> CrearFranjesVoluntari(List<FranjaHoraria> franjesDelProjecte, List<FranjaHoraria> LlistaFranjesSelec)
        {

            //Enviar la lista esta a la vista, una vez a lli mirar el bool y si es true checked y si no lo es no 
            
            List<FranjaVoluntari> franjesVoluntari = new List<FranjaVoluntari>();

            foreach(FranjaHoraria fh in franjesDelProjecte){

                bool existeix = GetSiExisteixAlaLlistaSelec(LlistaFranjesSelec, fh);

                List<VoluntarisEnFranjes> voluntaris = db.voluntarisEnFranjes
                                                                 .Where(v => v.FranjaHorariaVFID == fh.ID)
                                                                 .ToList();
                if(existeix == true){

                    FranjaVoluntari franjaVoluntari = new FranjaVoluntari(true , fh, voluntaris.Count);

                    franjesVoluntari.Add(franjaVoluntari);
                
                }else{

                    FranjaVoluntari franjaVoluntari = new FranjaVoluntari(false, fh, voluntaris.Count);

                    franjesVoluntari.Add(franjaVoluntari);
                }
            
            }

            return franjesVoluntari;
        }

        public bool GetSiExisteixAlaLlistaSelec(List<FranjaHoraria> LlistaFranjesSelec, FranjaHoraria franjaHoraria)
        { 
            bool existeix = false;
        
            foreach(FranjaHoraria fh in LlistaFranjesSelec){
                
                if(fh.ID == franjaHoraria.ID){
                    existeix = true;
                }
            }

            return existeix;
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(params string[] franjesSeleccionades)
        {
            string idUsuari = User.Identity.GetUserId();

            if(ModelState.IsValid){

                if (franjesSeleccionades != null) { 
                    
                    foreach(var r  in franjesSeleccionades){

                        //return RedirectToAction("Create", "VoluntarisEnFranjes", new { FranjaHorariaVFID = r});
                    }
                }
            }

            return View();
        }*/

        /*
        // GET: ProjectesUser/Create
        public ActionResult Create()
        {
            ViewBag.AdminstradorProjecteID = new SelectList(db.Users, "Id", "PasswordHash");
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria");
            ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID");
            return View();
        }

        // POST: ProjectesUser/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,NomProjecte,DescripcioProjecte,CategoriaProjecteID,AdminstradorProjecteID,DelegacioProjecteID,ImatgeProjecte")] Projecte projecte)
        {
            if (ModelState.IsValid)
            {
                db.Projectes.Add(projecte);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AdminstradorProjecteID = new SelectList(db.Users, "Id", "PasswordHash", projecte.AdminstradorProjecteID);
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria", projecte.CategoriaProjecteID);
            ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", projecte.DelegacioProjecteID);
            return View(projecte);
        }*/

        /*
        // GET: ProjectesUser/Edit/5
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
            ViewBag.AdminstradorProjecteID = new SelectList(db.Users, "Id", "PasswordHash", projecte.AdminstradorProjecteID);
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria", projecte.CategoriaProjecteID);
            ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", projecte.DelegacioProjecteID);
            return View(projecte);
        }

        // POST: ProjectesUser/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,NomProjecte,DescripcioProjecte,CategoriaProjecteID,AdminstradorProjecteID,DelegacioProjecteID,ImatgeProjecte")] Projecte projecte)
        {
            if (ModelState.IsValid)
            {
                db.Entry(projecte).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AdminstradorProjecteID = new SelectList(db.Users, "Id", "PasswordHash", projecte.AdminstradorProjecteID);
            ViewBag.CategoriaProjecteID = new SelectList(db.Categories, "ID", "NomCategoria", projecte.CategoriaProjecteID);
            ViewBag.DelegacioProjecteID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", projecte.DelegacioProjecteID);
            return View(projecte);
        }

        // GET: ProjectesUser/Delete/5
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
            return View(projecte);
        }

        // POST: ProjectesUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Projecte projecte = db.Projectes.Find(id);
            db.Projectes.Remove(projecte);
            db.SaveChanges();
            return RedirectToAction("Index");
        }*/

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
