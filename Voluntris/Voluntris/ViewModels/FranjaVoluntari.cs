using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Voluntris.Models;

namespace Voluntris.ViewModels
{
    public class FranjaVoluntari
    {
        public bool estaSelec { get; set; }

        public FranjaHoraria frajahoraria { get; set; }

        public int numeroVoluntarisApuntats { get; set; }

        public bool estaComplert { get; set; }

        public FranjaVoluntari()
        {

        }

        public FranjaVoluntari(bool selec, FranjaHoraria fra, int num) {

            this.estaSelec = selec;
            this.frajahoraria = fra;
            this.numeroVoluntarisApuntats = num;
        }

        public FranjaVoluntari(FranjaHoraria fra, int num)
        {
            this.frajahoraria = fra;
            this.numeroVoluntarisApuntats = num;

            if(numeroVoluntarisApuntats >= fra.NumeroMaxim  ){
                this.estaComplert = true;
            }
        }



    }
}