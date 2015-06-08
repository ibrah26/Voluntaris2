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
using System.IO;
using Microsoft.AspNet.Identity.Owin;
using Voluntris.Controllers;


namespace Voluntris.Controllers
{

    public class VoluntarisUserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        AccionsComunesController accio = new AccionsComunesController();

        public string getUsuariActual()
        {
            string idUsuari;
            return idUsuari = User.Identity.GetUserId();
        }

        // GET: VoluntarisUser
        [Authorize(Roles = "RolVoluntari")]
        public ActionResult Index()
        {
            string idUsuari = getUsuariActual();

            if(idUsuari != null){

                var users = db.Users.OfType<Voluntari>()
                                    .Where(x => x.Id == idUsuari)
                                    .Include(v => v.DelegacioVoluntari);
                return View(users.ToList());
            }
            return accio.redireccioAlHome();
        }
       
        // GET: VoluntarisUser/Details/5
        [Authorize(Roles = "RolVoluntari")]
        public ActionResult Details()
        {
            string idUsuari = getUsuariActual();

            if (idUsuari == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voluntari voluntari = db.Users.OfType<Voluntari>()
                                          .Where(x => x.Id == idUsuari).SingleOrDefault();

            if (voluntari == null)
            {
                return HttpNotFound();
            }

            var voluntarisEnfranjesQuHaAsistit =  db.voluntarisEnFranjes.Include(v => v.FranjaHorariaVF)
                                                          .Include(v => v.VoluntariVF)
                                                          .Include( v => v.FranjaHorariaVF.ProjecteFranjaHoraria)
                                                          .Where(v => v.VoluntariVFID == idUsuari)
                                                          .Where(v => v.HaAssistitVF == true);


            var voluntarisEnfranjesDelFutur= db.voluntarisEnFranjes.Include(v => v.FranjaHorariaVF)
                                                          .Include(v => v.VoluntariVF)
                                                          .Include(v => v.FranjaHorariaVF.ProjecteFranjaHoraria)
                                                          .Where(v => v.VoluntariVFID == idUsuari)
                                                          .Where(v => v.FranjaHorariaVF.HoraInici > DateTime.Now);

            List<VoluntarisEnFranjes> voluntarisEnFranjes = db.voluntarisEnFranjes.Include(v => v.FranjaHorariaVF)
                                                                                  .Where( v => v.VoluntariVFID == idUsuari)
                                                                                  .Where(v => v.HaAssistitVF == true)
                                                                                  .ToList();

            TimeSpan total = new TimeSpan();

            foreach (VoluntarisEnFranjes vf in voluntarisEnFranjes)
            {
                //var minuts = vf.FranjaHorariaVF.HoraFi.Minute - vf.FranjaHorariaVF.HoraInici.Minute;

                TimeSpan hores = vf.FranjaHorariaVF.HoraFi - vf.FranjaHorariaVF.HoraInici;

                total = total + hores;
            }

            ViewBag.Hores = total.TotalHours;

            ViewBag.FranjesHaAssitit = voluntarisEnfranjesQuHaAsistit;
            ViewBag.FranjesFutur = voluntarisEnfranjesDelFutur;

            return View(voluntari);
        }

        // GET: VoluntarisUser/Create
        public ActionResult Create()
        {
            string idUsuari = getUsuariActual();
            if (idUsuari == null)
            {
                ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio");
                return View();
            }
 
            return  accio.redireccioAlHome();
        }


       // POST: VoluntarisUser/Create
       // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
       // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
       [HttpPost]
       [ValidateAntiForgeryToken]
       public ActionResult Create([Bind(Include = "Id,PasswordHash,PhoneNumber,Email,EmailConfirmed,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DataNeixement,DelegacioVoluntariID,ImatgeVoluntari")] Voluntari voluntari)
       {
           string idUsuari = User.Identity.GetUserId();

           if (ModelState.IsValid && idUsuari == null)
           {
                   /**************  IMATGE *************/
                   HttpPostedFileBase file = Request.Files["fileuploadImage"];

                   if (file != null)
                   {
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

                   accio.canviPaswd(voluntari).Wait();
                   return RedirectToAction("Index");
           }

           ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "NomDelegacio", voluntari.DelegacioVoluntariID);
           return View(voluntari);

          /* if (ModelState.IsValid)
           {
               db.Users.Add(voluntari);
               db.SaveChanges();
               return RedirectToAction("Index");
           }

           ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", voluntari.DelegacioVoluntariID);
           return View(voluntari);*/
       }

       
     // GET: VoluntarisUser/Edit/5
     [Authorize(Roles = "RolVoluntari")]
     public ActionResult Edit(string id)
     {
         //string idUsuari = getUsuariActual();
         string idUsuari = getUsuariActual();

         if (idUsuari == null)
         {
             return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         //Voluntari voluntari = db.Users.Find(id);

         Voluntari voluntari = db.Users.OfType<Voluntari>()
                              .Where(x => x.Id == idUsuari).SingleOrDefault();
         
         if (voluntari == null)
         {
             return HttpNotFound();
         }
         ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions.Where( x => x.ID == voluntari.DelegacioVoluntariID)
                                                        , "AdministradorDelegacioID", "NomDelegacio", voluntari.DelegacioVoluntariID);
         return View(voluntari);
     }

     // POST: VoluntarisUser/Edit/5
     // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
     // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
     [HttpPost]
     [ValidateAntiForgeryToken]
     public ActionResult Edit([Bind(Include = "Id,PasswordHash,PhoneNumber,Email,EmailConfirmed,SecurityStamp,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DataNeixement,DelegacioVoluntariID,ImatgeVoluntari")] Voluntari voluntari)
     {
         if (ModelState.IsValid)
         {
             /**************  IMATGE *************/
             HttpPostedFileBase file = Request.Files["fileuploadImage"];

             if (file != null)
             {
                 // write your code to save image
                 string uploadPath = Server.MapPath("~/Content/images/");

                 string nomImatgeId = Guid.NewGuid().ToString();

                 string extencioImatge = Path.GetExtension(uploadPath + file.FileName);


                 file.SaveAs(uploadPath + nomImatgeId + extencioImatge);

                 voluntari.ImatgeVoluntari = nomImatgeId + extencioImatge;
             }

             /**************  IMATGE *************/
             db.Entry(voluntari).State = EntityState.Modified;
             db.SaveChanges();
             return RedirectToAction("Details", "VoluntarisUser");
         }
         ViewBag.DelegacioVoluntariID = new SelectList(db.Delegacions, "AdministradorDelegacioID", "ID", voluntari.DelegacioVoluntariID);
         return View(voluntari);
     }
    /*
     // GET: VoluntarisUser/Delete/5
     public ActionResult Delete(string id)
     {
         if (id == null)
         {
             return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }
         Voluntari voluntari = db.Users.Find(id);
         if (voluntari == null)
         {
             return HttpNotFound();
         }
         return View(voluntari);
     }
      *

     // POST: VoluntarisUser/Delete/5
     [HttpPost, ActionName("Delete")]
     [ValidateAntiForgeryToken]
     public ActionResult DeleteConfirmed(string id)
     {
         Voluntari voluntari = db.Users.Find(id);
         db.Users.Remove(voluntari);
         db.SaveChanges();
         return RedirectToAction("Index");
     }
      */
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
