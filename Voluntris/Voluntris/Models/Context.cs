using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("ApplicationDbContext")
        {
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        static ApplicationDbContext() {
            Database.SetInitializer<ApplicationDbContext>(new IncialitzadorDb());
        }

        //Voluntari i Administrador heredan de User, i no els hi cal crear la llista. En tenen prou amb la que es crea automaticament amb l'User que herdea de identityUser
        //public DbSet<Voluntari> Voluntaris { get; set; }
        //public DbSet<Administrador> Administradors { get; set; }
        public DbSet<Delegacio> Delegacions { get; set; }
        public DbSet<Projecte> Projectes { get; set; }
        public DbSet<FranjaHoraria> FranjesHoraries { get; set; }
        public DbSet<Categoria> Categories { get; set; }
        public DbSet<VoluntarisEnFranjes> voluntarisEnFranjes { get; set; }

        //public DbSet<Voluntris.Models.Roles> IdentityRoles { get; set; }

        //public DbSet<Voluntris.ViewModels.RolesViewModel> RolesViewModels { get; set; }

        //public DbSet<Voluntris.ViewModels.EditRolesViewModel> EditRolesViewModels { get; set; }

        /*protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            //preparem una llista buida d'errors de validació
            var result = new DbEntityValidationResult(entityEntry, new List<DbValidationError>());

            //si estem validant una reserva
            if (entityEntry.Entity is VoluntarisEnFranjes && (entityEntry.State == EntityState.Added ||
                                                              entityEntry.State == EntityState.Modified))
            {

                VoluntarisEnFranjes voluntariEnFranja = entityEntry.Entity as VoluntarisEnFranjes;

                int numerMaxim = voluntariEnFranja.FranjaHorariaVF.NumeroMaxim;

                //int franjaHoraria = voluntariEnFranja.FranjaHorariaVFID;

                List<VoluntarisEnFranjes> voluntaris = voluntarisEnFranjes
                                                        .Where( v => v.FranjaHorariaVFID == voluntariEnFranja.FranjaHorariaVFID)
                                                        .ToList();
                                                                  

                //hi ha solapaments?
                //var q_solapaments = Reserves.Where(r => !(r.Sortida < reserva.Entrada || r.Entrada > reserva.Sortida));
                
           
                if (voluntaris.Count >= numerMaxim)
                {
                    result.ValidationErrors.Add(new DbValidationError("HoraInici", "uno o mas dias de los requeridos estan ocupados"));
                    //result.ValidationErrors.Add(new DbValidationError("Entrada", "uno o mas dias de los requeridos estan ocupados"));
                }

                //altres validacions sobre la reserva 
                //...

            }

            if (result.ValidationErrors.Any())
            {
                //hem afegit errors a la llista d'errors de validació, retornem la llista d'errors
                return result;
            }
            else
            {
                //no hi ha hagut errors de validació, fem les validacions automàtiques de la classe base
                return base.ValidateEntity(entityEntry, items);
            }
        }*/
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {   
            //Serveix per avisar de que serem nosaltres manualment qui moldejarem les taules de la bdd
            base.OnModelCreating(modelBuilder);


            // FranjaHoraria 1-n ----- 1-1 Projecte , un Projecte te 1 o n franjes i una franja es d'un projecte
            modelBuilder.Entity<FranjaHoraria>()
                .HasRequired(f => f.ProjecteFranjaHoraria)
                .WithMany( p => p.FrangesProjecte)
                .HasForeignKey( f => f.ProjecteFranjaHorariaID)
                .WillCascadeOnDelete(false);

            // Projecte 0-n ------- 1-1 Categoria
            //El withMany deberia ser opcional no ? porque un proyecto tien muchas categorias y una categoria tiene 0 o Muchos proyectos.
            //Dani: Las bases de datos no controlan si es 0 o 1. por eso en el diagrama ponemos una [Proj]-3o------X-[Cact]
            modelBuilder.Entity<Projecte>()
                .HasRequired(p => p.CategoriaProjecte)
                .WithMany(c => c.ProjectesCategoria)
                .HasForeignKey(p => p.CategoriaProjecteID)
                .WillCascadeOnDelete(false);

            //Projecte 0-n ------------- 1-1 Delegacio, Un Projecte te una Delegacio i una Delegacio te 0 o n Projectes
            modelBuilder.Entity<Projecte>()
             .HasRequired(p => p.DelegacioProjecte)
             .WithMany(d => d.ProjectesDelegacio)
             .HasForeignKey(p => p.DelegacioProjecteID)
             .WillCascadeOnDelete(false);

            //Projecte 0-n ------------- 1-1 Administrador, Un Projecte te una Administrador i un Administrador te 0 o n Projectes
            modelBuilder.Entity<Projecte>()
             .HasRequired(p => p.AdminstradorProjecte)
             .WithMany(a => a.ProjectesAdministrador)
             .HasForeignKey(p => p.AdminstradorProjecteID)
             .WillCascadeOnDelete(false);

            // Voluntari 1-n ---------- 0-n FranjaHoraria, un Voltunari te 0 o n FranjesVoluntaries i una Franja te 0 o n Voluntaris
            /*modelBuilder.Entity<Voluntari>()
                .HasMany(v => v.FrangesVoluntari)
                .WithMany(f => f.VoluntarisEnFranjaF)
                .Map(x =>
                 {
                     x.MapLeftKey("FranjaHorariaID");
                     x.MapRightKey("VoluntariID");
                     x.ToTable("VoluntarisEnFranjes");
                 });*/

            //Voluntari 0-n ---------- 1-1 Delegacio, una Delegacio te 0 o n Voluntaris i un Voluntar te una delagacio 
            modelBuilder.Entity<Voluntari>()
                .HasRequired(v => v.DelegacioVoluntari)
                .WithMany(d => d.VoluntarisDelegacio)
                .HasForeignKey(v => v.DelegacioVoluntariID)
                .WillCascadeOnDelete(false);

            //VoluntarisEnFranja 0-n ------------------ 1-1 FranjaHoraria, un VoluntariEnFranja te una FranjaHoraria i una FranjaHoraria te 0 o n VoluntariEnFranja
            modelBuilder.Entity<VoluntarisEnFranjes>()
                .HasRequired(v => v.FranjaHorariaVF)
                .WithMany(vf => vf.VoluntarisEnFranjaFH)
                .HasForeignKey(v => v.FranjaHorariaVFID)
                .WillCascadeOnDelete(false);
            //VoluntarisEnFranja 0-n ------------------ 1-1 Voluntari , un VoluntariEnFranja te una Voluntari i un Voluntari te 0 o n VoluntariEnFranja
            modelBuilder.Entity<VoluntarisEnFranjes>()
                .HasRequired(v => v.VoluntariVF)
                .WithMany(x => x.VoluntarisEnFranjesV)
                .HasForeignKey(v => v.VoluntariVFID)
                .WillCascadeOnDelete(false);
            
            //Categoria 1-n ------------ 0-n Voluntaris, una Categoria tiene 0 o n Voluntarios y un Voluntario tiene 1 o n Categorias
            modelBuilder.Entity<Categoria>()
                .HasMany(c => c.VoluntarisCategoria)
                .WithMany(v => v.CategoriesPreferides)
                .Map(x =>
                {
                    x.MapLeftKey("VoluntariID");
                    x.MapRightKey("CategoriaID");
                    x.ToTable("CategoriesEnVoluntaris");
                    
                });

            //Categoria 0-n --------- 1-1 Administrador, una Categoria te un Administrador y un Administrador te entre 0-n Categories
            modelBuilder.Entity<Categoria>()
                .HasRequired(c => c.AdministradorCategoria)
                .WithMany(a => a.CategoriesAdministrador)
                .HasForeignKey(c => c.AdministradorCategoriaID)
                .WillCascadeOnDelete(false);

            //Delegacio 1-1 ---------- 1-1 Administrador , Una Delecacio te un Admin i un Admin te una delegacio
            /*modelBuilder.Entity<Delegacio>()
                .HasKey(d => d.AdministradorDelegacioId);

            modelBuilder.Entity<Administrador>()
                .HasOptional(a => a.DelegacioAdministrador)
                .WithRequired(a => a.AdministradorDelegacio);*/


            //Delegacio 0-1 ---------- 1-1 Administrador , Una Delecacio te un Admin i un Admin te 0 o una delegacio
            modelBuilder.Entity<Delegacio>()
                .HasKey(d => d.AdministradorDelegacioID);

            modelBuilder.Entity<Delegacio>()
                .HasRequired(d => d.AdministradorDelegacio)
                .WithOptional(d => d.DelegacioAdministrador);
               
               
        }

        public System.Data.Entity.DbSet<Voluntris.Models.Rol> IdentityRoles { get; set; }

       // public System.Data.Entity.DbSet<Voluntris.ViewModels.EditRolesVoluntari> EditRolesVoluntaris { get; set; }

    }
}