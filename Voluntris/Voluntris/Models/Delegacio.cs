using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class Delegacio
    {
        public virtual string ID { get; set; }
        [Display(Name = "Nom Delegació")]
        public virtual string NomDelegacio { get; set; }
        public virtual String AdministradorDelegacioID { get; set; }
        [Display(Name = "Administrador")]
        public virtual Administrador AdministradorDelegacio { get; set; }
        public virtual ICollection<Projecte> ProjectesDelegacio { get; set; }
        public virtual ICollection<Voluntari> VoluntarisDelegacio { get; set; }

    }
}