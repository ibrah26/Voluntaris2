using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Voluntris.Models;


namespace Voluntris.ViewModels
{
    public class RolesViewModel
    {
        public string Id { get; set; }
        //Validacio que controla que no vol res en blanc
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Nom Rol")]
        public string Nom { get; set; }
        public string Descripcio { get; set; }
    }

    public class EditRolesViewModel
    {
        public string Id {get; set;}
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        public string UserName { get; set; }
        public virtual string PhoneNumber { get; set; }

    }
}