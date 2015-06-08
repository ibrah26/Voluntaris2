using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Voluntris.Models;

namespace Voluntris.Controllers
{
    public class RolsApiController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/RolsApi
        /*public IQueryable<Rol> GetRoles()
        {
            return db.Roles.OfType<Rol>();
        }*/

        public IEnumerable<Rol> GetRoles() { 
        
           return ( from r in db.Roles.OfType<Rol>().ToList() select r);
        
        }

        public void NouRol(string nomRol, string Descripcio) {

            Rol rol = new Rol();

            rol.Name = nomRol;
            rol.Descripcio = Descripcio;
            rol.Id = Guid.NewGuid().ToString();;

            db.Roles.Add(rol);
        
        }

        // GET: api/RolsApi/5
        [ResponseType(typeof(Rol))]
        public IHttpActionResult GetRol(string id)
        {
            Rol rol = db.IdentityRoles.Find(id);
            if (rol == null)
            {
                return NotFound();
            }

            return Ok(rol);
        }

        // PUT: api/RolsApi/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutRol(string id, Rol rol)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != rol.Id)
            {
                return BadRequest();
            }

            db.Entry(rol).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RolExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/RolsApi
        [ResponseType(typeof(Rol))]
        public IHttpActionResult PostRol(Rol rol)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Roles.Add(rol);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (RolExists(rol.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = rol.Id }, rol);
        }

        // DELETE: api/RolsApi/5
        [ResponseType(typeof(Rol))]
        public IHttpActionResult DeleteRol(string nom)
        {

            Rol rol = db.IdentityRoles.Where(x => x.Name == nom).SingleOrDefault();

            //Rol rol = db.IdentityRoles.Find(id);

            if (rol == null)
            {
                return NotFound();
            }

            db.Roles.Remove(rol);
            db.SaveChanges();

            return Ok(rol);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RolExists(string id)
        {
            return db.Roles.Count(e => e.Id == id) > 0;
        }
    }
}