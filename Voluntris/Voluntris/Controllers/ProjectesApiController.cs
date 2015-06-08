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
    public class ProjectesApiController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/ProjectesApi
        public IQueryable<Projecte> GetProjectes()
        {
            return db.Projectes;
        }

        // GET: api/ProjectesApi/5
        [ResponseType(typeof(Projecte))]
        public IHttpActionResult GetProjecte(int id)
        {
            Projecte projecte = db.Projectes.Find(id);
            if (projecte == null)
            {
                return NotFound();
            }

            return Ok(projecte);
        }

        // PUT: api/ProjectesApi/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProjecte(int id, Projecte projecte)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != projecte.ID)
            {
                return BadRequest();
            }

            db.Entry(projecte).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjecteExists(id))
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

        // POST: api/ProjectesApi
        [ResponseType(typeof(Projecte))]
        public IHttpActionResult PostProjecte(Projecte projecte)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Projectes.Add(projecte);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = projecte.ID }, projecte);
        }

        // DELETE: api/ProjectesApi/5
        [ResponseType(typeof(Projecte))]
        public IHttpActionResult DeleteProjecte(int id)
        {
            Projecte projecte = db.Projectes.Find(id);
            if (projecte == null)
            {
                return NotFound();
            }

            db.Projectes.Remove(projecte);
            db.SaveChanges();

            return Ok(projecte);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProjecteExists(int id)
        {
            return db.Projectes.Count(e => e.ID == id) > 0;
        }
    }
}