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

namespace Voluntris.Controllers
{
    [Authorize(Roles = "RolVoluntari")]
    public class VoluntarisEnFranjesUserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        AccionsComunesController accio = new AccionsComunesController();

        public string getUsuariActual() { 
            string idUsuari;
            return idUsuari = User.Identity.GetUserId();
        }

        // GET: VoluntarisEnFranjesUser
        public ActionResult Index()
        {
            //var voluntarisEnFranjes = db.voluntarisEnFranjes.Include(v => v.FranjaHorariaVF).Include(v => v.VoluntariVF);
            //return View(voluntarisEnFranjes.ToList());

            string idUsuari = getUsuariActual();

            var voluntarisEnFranjes = db.voluntarisEnFranjes.Include(v => v.FranjaHorariaVF)
                                                          .Include(v => v.VoluntariVF)
                                                          .Where(v => v.VoluntariVFID == idUsuari);

            return View(voluntarisEnFranjes.ToList());
        }


        // GET: VoluntarisEnFranjesUser/Details/5
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
            if (idUsuari == voluntarisEnFranjes.VoluntariVFID)
            {
                return View(voluntarisEnFranjes);
            }
            else {
                return accio.redireccioAlHome();
            }
          
        }

        /*
        // GET: VoluntarisEnFranjesUser/Create
        public ActionResult Create()
        {
            ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries, "ID", "ObservacionsFH");
            ViewBag.VoluntariVFID = new SelectList(db.Users, "Id", "PasswordHash");
            return View();
        }
        */
        // POST: VoluntarisEnFranjesUser/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,FranjaHorariaVFID,VoluntariVFID,HaAssistitVF,ObservacionsVF")] VoluntarisEnFranjes voluntarisEnFranjes, int idProjecte ,params int[] franjesSeleccionades)
        {
            // 1.-Fer una selecció de les franjes de un projecte OK!!
            // 1.1- Veure les que hem pasen per el post i insertarles. Les que no s'hagin enviat vol dir que no estan marcades
            //      Per tant les haig de cercar y eliminar a la bdd OK!!

            string idUsuari = getUsuariActual();

            if (ModelState.IsValid)
            {
                Projecte projecte = db.Projectes.Find(idProjecte);

                List<FranjaHoraria> franjesProjecte = projecte.FrangesProjecte.ToList();

                if (franjesSeleccionades != null)
                {
                    foreach (var r in franjesSeleccionades)
                    {
                        List<FranjaHoraria> franjesDeseleccionades = new List<FranjaHoraria>();

                        franjesDeseleccionades = getLlistaFranjesDeseleccionades(franjesProjecte, franjesSeleccionades);

                        EliminarFranjesVoluntari(franjesDeseleccionades, idUsuari);

                        bool JaEstaApuntat =  MirarSiJaEstaApuntatAlaFranja(r, idUsuari);

                        if(JaEstaApuntat == false){

                            FranjaHoraria franja = db.FranjesHoraries.Where(x => x.ID == r).SingleOrDefault();

                            int numerMaxim = franja.NumeroMaxim;

                            //int franjaHoraria = voluntariEnFranja.FranjaHorariaVFID;

                            List<VoluntarisEnFranjes> voluntaris = db.voluntarisEnFranjes
                                                                    .Where(v => v.FranjaHorariaVFID == franja.ID)
                                                                    .ToList();

                            if (voluntaris.Count >= numerMaxim){

                                return RedirectToAction("InfoTecnicNoActiu");
                            } else {
                                //Itero per les id de les franges seleccionases amb un checkbox a la vista Detail de ProjecteUserController
                                //igualo el id de la franja a la FranjaHoraria de VoluntariEnFranja
                                voluntarisEnFranjes.FranjaHorariaVFID = r;
                                //igualo el VoluntariId de la classe  VoluntarisEnFranjes amb el actual usuari
                                voluntarisEnFranjes.VoluntariVFID = idUsuari;
                                db.voluntarisEnFranjes.Add(voluntarisEnFranjes);
                                db.SaveChanges();
                            }
                        }
                    }
                }else {
                    EliminarFranjesVoluntari(franjesProjecte, idUsuari);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult InfoTecnicNoActiu()
        {
            TempData["warning"] = "Aquesta Franja Horaria esta coberta, prova amb un altre ;)";
            //ViewBag.hola = "hola";
            //Tecnic tecnic = db.Tecnics.Find(id);
            return RedirectToAction("Index" , "ProjectesUser");
        }

        public List<FranjaHoraria> getLlistaFranjesDeseleccionades(List<FranjaHoraria> franjesHoraries, int[] franjesSeleccionades)
        {

            List<FranjaHoraria> franjesDeseleccionades = new List<FranjaHoraria>();
            
            foreach(FranjaHoraria fh in franjesHoraries){

                bool existeix = MirarSiFranjaEstaDinsDeFranjesSeleccionades(franjesSeleccionades, fh);

                if( existeix == false){

                    franjesDeseleccionades.Add(fh);
                }
            }

            return franjesDeseleccionades;
        }

        public bool MirarSiFranjaEstaDinsDeFranjesSeleccionades(int[] franjesSeleccionades, FranjaHoraria fh)
        {
            bool existeix = false;

            foreach (int idfh in franjesSeleccionades)
            {
                if(idfh == fh.ID){

                    existeix = true;
                }
            }
            return existeix;
        }

        public void EliminarFranjesVoluntari(List<FranjaHoraria> franjesProjecte, string IdUsuari)
        { 
            foreach(FranjaHoraria fh in franjesProjecte){

                VoluntarisEnFranjes voluntarisEnFranjesDesSeleccionades = db.voluntarisEnFranjes.Where(x => x.FranjaHorariaVFID == fh.ID)
                                                                                                .Where(x => x.VoluntariVFID == IdUsuari).SingleOrDefault();

                if(voluntarisEnFranjesDesSeleccionades != null){
                    
                    db.voluntarisEnFranjes.Remove(voluntarisEnFranjesDesSeleccionades);

                    //MIRAR si es necessari guardar a qui, sha d'e avisar al usuari que s'han borrat franjes
                    db.SaveChanges();
                }
            }
        }

        public bool MirarSiJaEstaApuntatAlaFranja(int idFranjaSleccionada, string idUsuari) {

            VoluntarisEnFranjes voluntariEnFranjaSeleccionat = db.voluntarisEnFranjes.Where(x => x.FranjaHorariaVFID == idFranjaSleccionada)
                                                                                     .Where(x => x.VoluntariVFID == idUsuari).SingleOrDefault();

            if (voluntariEnFranjaSeleccionat == null)
            {
                return false;
            }
            else {
                return true;
            }
        }

        // GET: VoluntarisEnFranjesUser/Edit/5
        /*public ActionResult Edit(int? id)
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

            if (voluntarisEnFranjes.VoluntariVFID == idUsuari)
            {
                ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries
                    .Where( f => f.ProjecteFranjaHorariaID == voluntarisEnFranjes.FranjaHorariaVF.ProjecteFranjaHorariaID),
                    "ID", "HoraInici", voluntarisEnFranjes.FranjaHorariaVFID);
                //ViewBag.VoluntariVFID = new SelectList(db.Users, "Id", "PasswordHash", voluntarisEnFranjes.VoluntariVFID);
                return View(voluntarisEnFranjes);
            }

            return accio.redireccioAlHome();

        }

        // POST: VoluntarisEnFranjesUser/Edit/5
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
            ViewBag.FranjaHorariaVFID = new SelectList(db.FranjesHoraries, "ID", "ObservacionsFH", voluntarisEnFranjes.FranjaHorariaVFID);
            ViewBag.VoluntariVFID = new SelectList(db.Users, "Id", "PasswordHash", voluntarisEnFranjes.VoluntariVFID);
            return View(voluntarisEnFranjes);
        }*/

        // GET: VoluntarisEnFranjesUser/Delete/5
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
            if(getUsuariActual() == voluntarisEnFranjes.VoluntariVFID){
                return View(voluntarisEnFranjes);
            }

            return accio.redireccioAlHome();
        }

        // POST: VoluntarisEnFranjesUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            VoluntarisEnFranjes voluntarisEnFranjes = db.voluntarisEnFranjes.Find(id);

            if(voluntarisEnFranjes != null){

                if (voluntarisEnFranjes.VoluntariVFID == getUsuariActual())
                {
                    db.voluntarisEnFranjes.Remove(voluntarisEnFranjes);
                    db.SaveChanges();
                }
            }

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
