using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace SSGU.Models
{
    public class DBAD
    {
        [Display(Name = "Identificador")]
        public int Id { get; set; }

        [Display(Name = "Nome Completo")]
        public string Nome { get; set; }

        [Display(Name = "Login")]
        public string Usuario { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Conta Ativada")]
        public bool Ativo { get; set; }

        [Display(Name = "Lotado no Departamento")]
        public string Departamento { get; set; }

        [Display(Name = "Gerente Responsável")]
        public string Gerente { get; set; }

        [Display(Name = "Função")]
        public string Funcao { get; set; }

        [Display(Name = "Ramal")]
        public string Ramal { get; set; }
    }

    
}