using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Voluntris.Models
{
    public class FranjaHoraria : IValidatableObject
    {
        public virtual int ID { get; set; }
        [DataType(DataType.DateTime)]
        public virtual DateTime HoraInici { get; set; }
        [DataType(DataType.DateTime)]
        public virtual DateTime HoraFi { get; set; }
        public virtual int ProjecteFranjaHorariaID { get; set; }
        public virtual Projecte ProjecteFranjaHoraria { get; set; }
        public virtual ICollection<VoluntarisEnFranjes> VoluntarisEnFranjaFH { get; set; }
        public virtual string ObservacionsFH { get; set; }
        public virtual int NumeroMinim { get; set; }
        public virtual int NumeroMaxim { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if(this.HoraInici >= this.HoraFi){
                yield return new ValidationResult("La data fi ha de ser major que la data inici", new[] { "HoraFi" });
            }

            if(this.HoraInici < DateTime.Now  ){
                yield return new ValidationResult("La data inici ha de ser major que avui", new[] { "HoraInici" });
            }

            if (this.HoraFi < DateTime.Now)
            {
                yield return new ValidationResult("La data fi ha de ser major que avui", new[] { "HoraFi" });
            }

            if (this.NumeroMinim > this.NumeroMaxim)
            {

                yield return new ValidationResult("El Numero Minim de Voluntaris a de ser anferior o igual al maxim", new[] { "NumeroMinim" });
            }

            if(this.NumeroMinim <= 0){
                yield return new ValidationResult("El Numero Minim de Voluntaris a de ser superior a 0", new[] { "NumeroMinim" });
            }

            if (this.NumeroMaxim <= 0)
            {

                yield return new ValidationResult("El Numero Maxim de Voluntaris a de ser superior a 0", new[] { "NumeroMaxim" });
            }

            if (this.NumeroMaxim < this.NumeroMinim)
            {

                yield return new ValidationResult("El Numero Maxim de Voluntaris a de ser major o igual  al minim", new[] { "NumeroMaxim" });
            }
        }
    }
}