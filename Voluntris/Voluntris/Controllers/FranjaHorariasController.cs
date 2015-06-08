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
using System.Data.Entity.Validation;
using Voluntris.ViewModels;

namespace Voluntris.Controllers
{
    [Authorize(Roles = "Root, RolAdministrador, RolVoluntari")]
    public class FranjaHorariasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        AccionsComunesController accio = new AccionsComunesController();
     
        // GET: FranjaHorarias
        public ActionResult Index()
        {
            //var franjesHoraries = db.FranjesHoraries.Include(f => f.ProjecteFranjaHoraria);

            string idUsuari = User.Identity.GetUserId();

            string Rol = accio.getRolUsuari(idUsuari, false);

            if (Rol.Equals("RolAdministrador"))
            {
               List<FranjaHoraria> LlistafranjesHoraries = db.FranjesHoraries
                    .Where( f => f.ProjecteFranjaHoraria.AdminstradorProjecteID == idUsuari)
                    .Include(f => f.ProjecteFranjaHoraria).ToList();

               List<FranjaVoluntari> franjaVolutnari = getFranjaVoluntari(LlistafranjesHoraries);

               return View(franjaVolutnari.ToList());
            }
            else if (Rol.Equals("RolVoluntari"))
            {
                return RedirectToAction("Index", "Home");
            }

            List<FranjaHoraria> LlistafranjesHorariesRoot = db.FranjesHoraries
                                                 .Include(f => f.ProjecteFranjaHoraria).ToList();

            List<FranjaVoluntari> franjaVolutnariRoot = getFranjaVoluntari(LlistafranjesHorariesRoot);

            return View(franjaVolutnariRoot.ToList());
        }

        public List<FranjaVoluntari> getFranjaVoluntari(List<FranjaHoraria> franjaHoraria)
        {
            List<FranjaVoluntari> franjesVoluntaris = new List<FranjaVoluntari>();
            
            foreach (FranjaHoraria fh in franjaHoraria)
            {

                FranjaVoluntari franjaVoluntari = new FranjaVoluntari(fh, fh.VoluntarisEnFranjaFH.Count);

                franjesVoluntaris.Add(franjaVoluntari);

               // List<VoluntarisEnFranjes> voluntaris = db.voluntarisEnFranjes
                 //                                     .Where(v => v.FranjaHorariaVFID == vf.ID)
                   //                                   .ToList();
            }
            return franjesVoluntaris;

        }

        [Route(@"FranjaHorarias/{id:regex(\d+)}")]
        public ActionResult IndexDeUnProjecte(int id)
        {
            List<FranjaHoraria> franjesHoraries = db.FranjesHoraries.Include(f => f.ProjecteFranjaHoraria).Where( x => x.ProjecteFranjaHorariaID == id).ToList();

            //List<FranjaHoraria> franjesHoraries = db.FranjesHoraries.Include(f => f.ProjecteFranjaHoraria).ToList();

            string idUsuariActual = User.Identity.GetUserId();

            Projecte projecte = db.Projectes//.Where(x => x.AdminstradorProjecteID == idUsuariActual)
                                            .Where(x => x.ID == id)
                                            .FirstOrDefault();

            if(projecte.AdminstradorProjecteID == idUsuariActual){

                ViewBag.IDprojecte = id;
                return View(franjesHoraries.ToList());

            }

            return RedirectToAction("Index", "Home");
            //return View(franjesHoraries.ToList());
        }

        // GET: FranjaHorarias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FranjaHoraria franjaHoraria = db.FranjesHoraries.Find(id);
            if (franjaHoraria == null)
            {
                return HttpNotFound();
            }

            string idUsuari = User.Identity.GetUserId();

            if (franjaHoraria.ProjecteFranjaHoraria.AdminstradorProjecteID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                return View(franjaHoraria);
            }
            else {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: FranjaHorarias/Create
        public ActionResult Create()
        {
            string idUsuari = User.Identity.GetUserId();

            ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where( p => p.AdminstradorProjecteID == idUsuari), "ID", "NomProjecte");
            return View();
        }

        // POST: FranjaHorarias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,HoraInici,HoraFi,ProjecteFranjaHorariaID,ObservacionsFH,NumeroMinim,NumeroMaxim")] FranjaHoraria franjaHoraria)
        {
     
            return CreateFranjaUltimate(franjaHoraria);
                   // El metode abans era ActionResult
            //string idUsuari = User.Identity.GetUserId();

            //if (ModelState.IsValid)
            //{
            //    Projecte proj = db.Projectes.Where(x => x.ID == franjaHoraria.ProjecteFranjaHorariaID).SingleOrDefault();

            //    Administrador admin = proj.AdminstradorProjecte;

            //    if (admin.Id == idUsuari)
            //    {
            //        db.FranjesHoraries.Add(franjaHoraria);
            //        db.SaveChanges();
            //        return RedirectToAction("Index");
            //    }
            //    else
            //    {
            //        return RedirectToAction("Index", "Home");
            //    }

            //}
            //ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where(p => p.AdminstradorProjecteID == idUsuari), "ID", "NomProjecte");
            ////ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes, "ID", "NomProjecte", franjaHoraria.ProjecteFranjaHorariaID);
            //return View(franjaHoraria);
        }


        [Route(@"FranjaHorarias/Create/{idProjecte:regex(\d+)}")]
        public ActionResult CreateDeUnProjecte(int? idProjecte)
        {
            string idUsuari = User.Identity.GetUserId();

            Projecte projecte = db.Projectes.Where(x => x.ID == idProjecte)
                                            .Where(x => x.AdminstradorProjecteID == idUsuari).FirstOrDefault();

            if (projecte == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (idUsuari == projecte.AdminstradorProjecteID)
            {
                ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where(p => p.ID == idProjecte), "ID", "NomProjecte");
                //.Where(p => p.ID == idProjecte), "ID", "NomProjecte");
                return View();

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        //[HttpPost, ActionName("CreateDeUnProjecte")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDeUnProjecteX([Bind(Include = "ID,HoraInici,HoraFi,ProjecteFranjaHorariaID,ObservacionsFH,NumeroMinim,NumeroMaxim")] FranjaHoraria franjaHoraria)
        {
            return CreateFranjaUltimate(franjaHoraria);
            //string idUsuari = User.Identity.GetUserId();

            //if (ModelState.IsValid)
            //{
            //    Projecte proj = db.Projectes.Where(x => x.ID == franjaHoraria.ProjecteFranjaHorariaID).SingleOrDefault();

            //    Administrador admin = proj.AdminstradorProjecte;

            //    if (admin.Id == idUsuari)
            //    {
            //        db.FranjesHoraries.Add(franjaHoraria);
            //        db.SaveChanges();
            //        return RedirectToAction("Index");
            //    }
            //    else
            //    {
            //        return RedirectToAction("Index", "Home");
            //    }

            //}
            //ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where(p => p.AdminstradorProjecteID == idUsuari), "ID", "NomProjecte");
            ////ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes, "ID", "NomProjecte", franjaHoraria.ProjecteFranjaHorariaID);
            //return View(franjaHoraria);
        }

     
        public ActionResult CreateFranjaUltimate(FranjaHoraria franjaHoraria)
        {
            string idUsuari = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                Projecte proj = db.Projectes.Where(x => x.ID == franjaHoraria.ProjecteFranjaHorariaID).SingleOrDefault();

                Administrador admin = proj.AdminstradorProjecte;

                if (admin.Id == idUsuari)
                {
                    try {

                        db.FranjesHoraries.Add(franjaHoraria);
                        db.SaveChanges();
                        //return RedirectToAction("Index");  
                        return RedirectToAction("IndexDeUnProjecte", new { id = proj.ID }); 

                    }
                    catch (DbEntityValidationException ex)
                    {
                        //si si ha errors de validació a ValidateEntity els passem al model:
                        foreach (var error in ex.EntityValidationErrors.First().ValidationErrors)
                        {
                            this.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }
                    }
               
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

            }
            ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where(p => p.AdminstradorProjecteID == idUsuari), "ID", "NomProjecte");
            //ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes, "ID", "NomProjecte", franjaHoraria.ProjecteFranjaHorariaID);
            return View(franjaHoraria);
        }

        // GET: FranjaHorarias/Edit/5
        public ActionResult Edit(int? id)
        {
            string idUsuari = User.Identity.GetUserId();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FranjaHoraria franjaHoraria = db.FranjesHoraries.Find(id);
            if (franjaHoraria == null)
            {
                return HttpNotFound();
            }

            if(franjaHoraria.ProjecteFranjaHoraria.AdminstradorProjecteID == idUsuari){
                //ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes, "ID", "NomProjecte", franjaHoraria.ProjecteFranjaHorariaID);
                ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where(p => p.AdminstradorProjecteID == idUsuari), "ID", "NomProjecte");
                return View(franjaHoraria);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: FranjaHorarias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,HoraInici,HoraFi,ProjecteFranjaHorariaID,ObservacionsFH,NumeroMinim,NumeroMaxim")] FranjaHoraria franjaHoraria)
        {
            string idUsuari = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                Projecte proj = db.Projectes.Where(x => x.ID == franjaHoraria.ProjecteFranjaHorariaID).SingleOrDefault();

                Administrador admin = proj.AdminstradorProjecte;
                //MIRAR PER QUE NO PUC AGAFAR EL ID DEL ADMIN COM EN LES ALTRES FUNCIONS
                if (admin.Id == idUsuari)
                {
                    db.Entry(franjaHoraria).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes.Where(p => p.AdminstradorProjecteID == idUsuari), "ID", "NomProjecte");
            //ViewBag.ProjecteFranjaHorariaID = new SelectList(db.Projectes, "ID", "NomProjecte", franjaHoraria.ProjecteFranjaHorariaID);
            return View(franjaHoraria);
        }

        // GET: FranjaHorarias/Delete/5
        public ActionResult Delete(int? id)
        {
            string idUsuari = User.Identity.GetUserId();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FranjaHoraria franjaHoraria = db.FranjesHoraries.Find(id);
            if (franjaHoraria == null)
            {
                return HttpNotFound();
            }

            if (franjaHoraria.ProjecteFranjaHoraria.AdminstradorProjecteID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                return View(franjaHoraria);
            }

            return RedirectToAction("Index", "Home");
          
        }

        // POST: FranjaHorarias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FranjaHoraria franjaHoraria = db.FranjesHoraries.Find(id);

            string idUsuari = User.Identity.GetUserId();

            if (franjaHoraria.ProjecteFranjaHoraria.AdminstradorProjecteID == idUsuari || accio.esRoot(idUsuari) == true)
            {
                db.FranjesHoraries.Remove(franjaHoraria);
                db.SaveChanges();
                return RedirectToAction("Index");
            } else {
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
