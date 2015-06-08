using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class VoluntarisEnFranjes
    {
        public virtual int ID { get; set; }
        public virtual int FranjaHorariaVFID { get; set; }
        public virtual FranjaHoraria FranjaHorariaVF { get; set; }
        public virtual string VoluntariVFID { get; set; }
        public virtual Voluntari VoluntariVF { get; set; }
        public virtual Boolean  HaAssistitVF { get; set; }
        public virtual string ObservacionsVF { get; set; }
    
    }
}