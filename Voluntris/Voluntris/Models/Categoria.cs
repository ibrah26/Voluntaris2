using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class Categoria
    {
        public virtual int ID { get; set; }
        public virtual string NomCategoria { get; set; }
        public virtual string DescripcioCategoria { get; set; }
        public virtual string AdministradorCategoriaID { get; set; }
        public virtual Administrador AdministradorCategoria { get; set; }
        public virtual ICollection<Projecte> ProjectesCategoria { get; set; }
        public virtual ICollection<Voluntari> VoluntarisCategoria { get; set; }
        public virtual string ImatgeCategoria { get; set; }
    }
}