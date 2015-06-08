using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class Projecte
    {
        public virtual int ID { get; set; }
        public virtual string NomProjecte { get; set; }
        public virtual string DescripcioProjecte { get; set; }
        public virtual int CategoriaProjecteID { get; set; }
        public virtual Categoria CategoriaProjecte { get; set; }
        public virtual string AdminstradorProjecteID { get; set; }
        public virtual Administrador AdminstradorProjecte { get; set; }
        public virtual string DelegacioProjecteID { get; set; }
        public virtual Delegacio DelegacioProjecte { get; set; }
        public virtual ICollection<FranjaHoraria> FrangesProjecte { get; set; }
        public virtual string ImatgeProjecte { get; set; }
        //Real ment el projecte te franges i les franges tenen voluntaris
        //public virtual ICollection<Voluntari> VoluntarisProjecte { get; set; }
      
       
    }
}