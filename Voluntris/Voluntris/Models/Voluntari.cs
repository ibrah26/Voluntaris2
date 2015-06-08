using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class Voluntari : User
    {
        public virtual DateTime DataNeixement { get; set; }
        public virtual String DelegacioVoluntariID { get; set; }
        public virtual Delegacio DelegacioVoluntari { get; set; }
        public virtual ICollection<VoluntarisEnFranjes> VoluntarisEnFranjesV { get; set; }
        public virtual ICollection<Categoria> CategoriesPreferides { get; set; }
        public virtual string ImatgeVoluntari { get; set; }

        //Real ment un voluntari s'hapunta a una franja i no pas a un projecte
        //public virtual ICollection<Projecte> ProjectesVoluntari { get; set; }
    }
}