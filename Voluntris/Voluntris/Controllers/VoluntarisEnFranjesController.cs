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
    public class VoluntarisEnFranjesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        AccionsComunesController accio = new AccionsComunesController();

        public string getUsuariActual()
        {
            string idUsuari;
            return idUsuari = User.Identity.GetUserId();
        }

        // GET: VoluntarisEnFranjes
        public ActionResult Index()
        {
            var voluntarisEnFranjes = db.voluntarisEnFranjes.Include(v => v.FranjaHorariaVF)
                                                          .Include(v => v.VoluntariVF);
                                                          //.Include(v => v.IdentityUser.UserNam);
            string idUsuari = User.Identity.GetUserId();

            if (accio.esAdmin(idUsuari) == true)
            {
                Administrador admin = accio.getAdministrador(idUsuari);

                voluntarisEnFranjes = db.voluntarisEnFranjes.Where(v => v.FranjaHorariaVF.ProjecteFranjaHoraria.DelegacioProjecteID == admin.Id)
                                                            .Include(v => v.FranjaHorariaVF)
                                                            .Include(v => v.VoluntariVF);
                                                            //.Where(v => v.VoluntariVF.DelegacioVoluntariID == admin.DelegacioAdministradorID);

            }

            return View(voluntarisEnFranjes.ToList());
        }

        // GET: VoluntarisEnFranjes/Details/5
        public ActionResult Details(int? id)
        {
            string idUsuari = getUsuariActual();
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VoluntarisEnFranjes voluntarisEnFranjes = db.voluntarisEnFranjes.Find(id);
            if (voluntarisEnFranjes == null)
            {
                return HttpNotFound();
            }
            if(accio.esAdmin(idUsuari)== true){

                Administrador administrador = accio.getAdministrador(idUsuari);

                if (voluntarisEnFranjes.FranjaHorariaVF.ProjecteFranjaHoraria.DelegacioProjecteID == administrador.DelegacioAdministradorID)
                {
                    return View(voluntarisEnFranjes);
                }
                else {
                    accio.redireccioAlHome();
                }
            }

            return View(voluntarisEnFranjes);
        }

        // GET: VoluntarisEnFranjes/Create
        public ActionResult Create()
        {//ObservacionsFH

            string idUsuari = getUsuariActual();

            if(accio.esAdmin(idUsuari) == true){

                Administrador administardor = accio.getAdministrador(idUsuari);

                ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries.Where( x => x.ProjecteFranjaHoraria.DelegacioProjecteID == administardor.Id), "ID", "HoraInici");
                ViewBag.VoluntariVFID = new SelectList(db.Users.OfType<Voluntari>().Where( x => x.DelegacioVoluntariID == administardor.Id), "Id", "UserName");

                return View();
            }

            ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries, "ID", "HoraInici");
            ViewBag.VoluntariVFID = new SelectList(db.Users.OfType<Voluntari>(), "Id", "UserName");
            return View();
        }

        //QUEDA MIRAR EL POST, DEL CREATE, hAY QUE mirar de que si existe o no, pude ser que sea mejor con un check box

        // POST: VoluntarisEnFranjes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,FranjaHorariaVFID,VoluntariVFID,HaAssistitVF,ObservacionsVF")] VoluntarisEnFranjes voluntarisEnFranjes, params int[] franjesSeleccionades)
        {
            string idUsuari = getUsuariActual();

            if (ModelState.IsValid)
            {
                if (accio.esAdmin(idUsuari) == true)
                {
                    Administrador administrador = accio.getAdministrador(idUsuari);

                    if (voluntarisEnFranjes.FranjaHorariaVF.ProjecteFranjaHoraria.AdminstradorProjecteID == administrador.Id)
                    {
                        db.voluntarisEnFranjes.Add(voluntarisEnFranjes);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                       
                        return accio.redireccioAlHome();
                    }
                }else {
                    db.voluntarisEnFranjes.Add(voluntarisEnFranjes);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            if (accio.esAdmin(idUsuari) == true)
            {
                Administrador administrador = accio.getAdministrador(idUsuari);
                ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries.Where(x => x.ProjecteFranjaHoraria.DelegacioProjecteID == administrador.DelegacioAdministradorID), "ID", "HoraInici");
                ViewBag.VoluntariVFID = new SelectList(db.Users.OfType<Voluntari>().Where(x => x.DelegacioVoluntariID == administrador.DelegacioAdministradorID), "Id", "UserName");
                return View(voluntarisEnFranjes);
            }
         
            
            /*Borrar si funciona
             * 
             * string idUsuari = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                if (franjesSeleccionades != null)
                {
                    foreach (var r in franjesSeleccionades)
                    {
                        //Itero per les id de les franges seleccionases amb un checkbox a la vista Detail de ProjecteUserController
                        //igualo el id de la franja a la FranjaHoraria de VoluntariEnFranja
                        voluntarisEnFranjes.FranjaHorariaVFID = r;
                        //igualo el VoluntariId de la classe  VoluntarisEnFranjes amb el actual usuari
                        voluntarisEnFranjes.VoluntariVFID = idUsuari;
                        db.voluntarisEnFranjes.Add(voluntarisEnFranjes);
                        db.SaveChanges();
                    }

                    return RedirectToAction("Index");
                }
            }
           
            return RedirectToAction("Index", "Home");
             **/

    

            ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries, "ID", "HoraInici", voluntarisEnFranjes.FranjaHorariaVFID);
            ViewBag.VoluntariVFID = new SelectList(db.Users.OfType<Voluntari>(), "Id", "UserName", voluntarisEnFranjes.VoluntariVFID);
            return View(voluntarisEnFranjes);
        }

        // GET: VoluntarisEnFranjes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VoluntarisEnFranjes voluntarisEnFranjes = db.voluntarisEnFranjes.Find(id);
            if (voluntarisEnFranjes == null)
            {
                return HttpNotFound();
            }
            ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries, "ID", "HoraInici", voluntarisEnFranjes.FranjaHorariaVFID);
            ViewBag.VoluntariVFID = new SelectList(db.Users.OfType<Voluntari>(), "Id", "UserName", voluntarisEnFranjes.VoluntariVFID);
            return View(voluntarisEnFranjes);
        }

        // POST: VoluntarisEnFranjes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,FranjaHorariaVFID,VoluntariVFID,HaAssistitVF,ObservacionsVF")] VoluntarisEnFranjes voluntarisEnFranjes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(voluntarisEnFranjes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries, "ID", "HoraInici", voluntarisEnFranjes.FranjaHorariaVFID);
            ViewBag.VoluntariVFID = new SelectList(db.Users.OfType<Voluntari>(), "Id", "UserName", voluntarisEnFranjes.VoluntariVFID);
            return View(voluntarisEnFranjes);
        }

        // GET: VoluntarisEnFranjes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VoluntarisEnFranjes voluntarisEnFranjes = db.voluntarisEnFranjes.Find(id);
            if (voluntarisEnFranjes == null)
            {
                return HttpNotFound();
            }
            return View(voluntarisEnFranjes);
        }

        // POST: VoluntarisEnFranjes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            VoluntarisEnFranjes voluntarisEnFranjes = db.voluntarisEnFranjes.Find(id);
            db.voluntarisEnFranjes.Remove(voluntarisEnFranjes);
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
