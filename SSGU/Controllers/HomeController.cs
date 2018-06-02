using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web.Security;
using System.Web.Mvc;
using SSGU.Models;
using X.PagedList;
using System.Collections;

namespace SSGU.Controllers
{


    public class HomeController : Controller
    {
        //private bool resp;
        //private DataTable tabela;
        public static List<DBAD> Usuario = new List<DBAD>();
        public SSGU.Util.ADMethodsAccountManagement ADMethods = new SSGU.Util.ADMethodsAccountManagement();

        //public DBAD usuarios = new DBAD();




        // private readonly string Grupo = ConfigurationManager.AppSettings["AdminApp"].ToString();
        [AuthorizeAD(Groups = "TI")]
        public ActionResult Index(string sortOrder, string currentFilter, string pesquisarString, string filtroAtivados, string currentAtivados, int pagina = 1)
        {
            bool ativos;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["UsuarioSortParm"] = String.IsNullOrEmpty(sortOrder) ? "usu_desc" : "usu_asc";
            ViewData["EmailSortParm"] = String.IsNullOrEmpty(sortOrder) ? "email_desc" : "email_asc";

            if (pesquisarString != null)
            {
                pagina = 1;
            }
            else
            {
                pesquisarString = currentFilter;
            }

            if (filtroAtivados == "Sim" || filtroAtivados == null)
            {
                ativos = true;
            }
            else
            {
                filtroAtivados = currentAtivados;
                ativos = false;
            }

            ViewBag.CurrentAtivados = filtroAtivados;
            ViewBag.CurrentFilter = pesquisarString;
            var tabela = CarregaUsuarios();
            var pesquisa = from s in tabela select s;

            if (ativos == false)
            {
                if (!String.IsNullOrEmpty(pesquisarString))
                {
                    pesquisa = pesquisa.Where(s => s.Usuario.Contains(pesquisarString)
                                           || s.Nome.Contains(pesquisarString)
                                           || s.Email.Contains(pesquisarString));
                }
            }
            else
            {
                pesquisa = pesquisa.Where(s => s.Ativo.Equals(true));
                if (!String.IsNullOrEmpty(pesquisarString))
                {
                    pesquisa = pesquisa.Where(s => s.Usuario.Contains(pesquisarString)
                                           || s.Nome.Contains(pesquisarString)
                                           || s.Email.Contains(pesquisarString)
                                           && s.Ativo.Equals(true));
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    pesquisa = pesquisa.OrderByDescending(s => s.Nome);
                    break;
                case "usu_desc":
                    pesquisa = pesquisa.OrderByDescending(s => s.Usuario);
                    break;
                case "usu_asc":
                    pesquisa = pesquisa.OrderBy(s => s.Usuario);
                    break;
                case "email_desc":
                    pesquisa = pesquisa.OrderByDescending(s => s.Email);
                    break;
                case "email_asc":
                    pesquisa = pesquisa.OrderBy(s => s.Email);
                    break;
                default:
                    pesquisa = pesquisa.OrderBy(s => s.Nome);
                    break;
            }

            DirectoryEntry de = new DirectoryEntry();
            DirectoryEntry dx = new DirectoryEntry("WinNT://" + Environment.UserDomainName + "/" + Environment.UserName);
            // objeto root = PeguoNoRaiz();
            string RootNode = de.Name.Substring(3);
            ViewBag.NomeUsuario = dx.Properties["fullname"].Value.ToString();
            if (tabela.Count != 0)
            {
                //int pageSize = 5;
                //int pageNumber = (page ?? 1);
                return View(pesquisa.ToPagedList(pagina, 5)); //ToPagedList(pageNumber,pageSize));
            }
            return View(pesquisa);
        }

        public ActionResult Detalhes(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }


            var resultado = CarregaUsuarios().Find(u => u.Id == id);
            if (resultado == null)
            {
                return HttpNotFound();
            }
            return PartialView(resultado);
        }

        public ActionResult About()
        {
            ViewBag.Message = "SSGU - SECONCI Sistema de Gestão de Usuários";

            /*  IQueryable<DBAD> data = from s in users
                                      group s by users.Ativo into Ativo
                                      select new DBAD()
                                      {

                                      }
                                      */
            /*  IQueryable<EnrollmentDateGroup> data = from student in db.Students
                                                     group student by student.EnrollmentDate into dateGroup
                                                     select new EnrollmentDateGroup()
                                                     {
                                                         EnrollmentDate = dateGroup.Key,
                                                         StudentCount = dateGroup.Count()
                                                     };
              return View(data.ToList());
          */
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult NaoAutorizado()
        {
            ViewBag.Message = "Sem Autorização";
            ViewBag.Autorizacao = false;

            return View();
        }

        public ArrayList CarregaGrupos()
        {
            string LDAP = "LDAP://DC=facincani,DC=local";
            DirectoryEntry deRoot = new DirectoryEntry(LDAP);
            DirectorySearcher deSrch = new DirectorySearcher(deRoot, "(&(objectClass=user)(objectCategory=person))");
            ArrayList memberof = new ArrayList();
            foreach (SearchResult oRes in deSrch.FindAll())
            {
                if (oRes.Properties.Contains("memberOf"))
                {
                    memberof.Add(oRes.Properties["memberOf"][0].ToString());
                }
            }
            return memberof;
        }

        public List<DBAD> CarregaUsuarios()
        {
            var lista = new List<DBAD>();
            // string LDAP = "LDAP://CN=Domain-DNS,CN=Schema,CN=Configuration,DC=facincani,DC=local";
            string LDAP = "LDAP://DC=facincani,DC=local";
            DirectoryEntry deRoot = new DirectoryEntry(LDAP);
            DirectorySearcher deSrch = new DirectorySearcher(deRoot, "(&(objectClass=user)(objectCategory=person))");
            /*deSrch.PropertiesToLoad.Add("cn");
            deSrch.PropertiesToLoad.Add("userPrincipalName");
            deSrch.PropertiesToLoad.Add("sAMAccountName");
            deSrch.PropertiesToLoad.Add("mail");
            deSrch.PropertiesToLoad.Add("userAccountControl");
            deSrch.PropertiesToLoad.Add("department");
            deSrch.PropertiesToLoad.Add("manager"); */
            deSrch.Sort.PropertyName = "sAMAccountName";

            int Id = 1;

            foreach (SearchResult oRes in deSrch.FindAll())
            {
                int flags;
                bool ativo;
                DBAD usuarios = new DBAD();
                usuarios.Id = Id;
                usuarios.Nome = oRes.Properties["cn"][0].ToString();
                usuarios.Usuario = oRes.Properties["sAMAccountName"][0].ToString();
                flags = (int)oRes.Properties["userAccountControl"][0];
                ativo = !Convert.ToBoolean(flags & 0x0002);
                usuarios.Ativo = ativo;
                //usuarios.Departamento = oRes.Properties["department"][0].ToString();
                if (oRes.Properties.Contains("manager"))
                {
                    usuarios.Gerente = oRes.Properties["manager"][0].ToString().Split(',')[0].Split('=')[1];
                }
                else { usuarios.Gerente = "Não Definido"; }

                if (oRes.Properties.Contains("department"))
                {
                    usuarios.Departamento = oRes.Properties["department"][0].ToString();
                }
                else { usuarios.Departamento = "Não Cadastrado"; }

                if (oRes.Properties.Contains("mail"))
                {
                    //safe to access now
                    usuarios.Email = oRes.Properties["mail"][0].ToString();
                }
                else { usuarios.Email = "Não Informado"; }

                if (oRes.Properties.Contains("title"))
                {
                    usuarios.Funcao = oRes.Properties["title"][0].ToString();
                }
                else { usuarios.Funcao = "Não Cadastrado"; }

                if (oRes.Properties.Contains("telephoneNumber"))
                {
                    usuarios.Ramal = oRes.Properties["telephoneNumber"][0].ToString();
                }
                else { usuarios.Ramal = "Não Cadastrado"; }


                lista.Add(usuarios);
                Id++;

            }
            return lista;
        }

        public ActionResult Unlock(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }


            var resultado = CarregaUsuarios().Find(u => u.Id == id);
            if (resultado == null)
            {
                return HttpNotFound();
            }

            int retorno = DesbloquearConta(resultado.Usuario);

            switch (retorno)
            {
                case 0:
                    ViewBag.Mensagem = "A Conta NÃO está bloqueada!";
                    ViewBag.Resp = "N";
                    break;
                case 1:
                    ViewBag.Mensagem = "A Conta foi Desbloqueada com sucesso!";
                    ViewBag.Resp = "S";
                    break;
                case 2:
                    ViewBag.Mensagem = "Ocorreu um ERRO, não foi possível determinar se a conta está bloqueada ou não!";
                    ViewBag.Resp = "E";
                    break;
                default:
                    ViewBag.Mensagem = "Ocorreu um ERRO, não foi possível determinar se a conta está bloqueada ou não!";
                    ViewBag.Resp = "E";
                    break;
            }

            return PartialView(resultado);
        }

        public ActionResult RSenha(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var resultado = CarregaUsuarios().Find(u => u.Id == id);
            if (resultado == null)
            {
                return HttpNotFound();
            }
            int retorno = ResetarConta(resultado.Usuario);
            switch (retorno)
            {
                case 0:
                    ViewBag.Mensagem = "Não foi possível localizar a conta no AD para resetar a senha";
                    ViewBag.Resp = "N";
                    break;
                case 1:
                    ViewBag.Mensagem = "A senha foi reinicializada com sucesso para Seconci@1234";
                    ViewBag.Resp = "S";
                    break;
                case 2:
                    ViewBag.Mensagem = "Ocorreu um ERRO, não foi possível determinar o erro e a senha não foi resetada!";
                    ViewBag.Resp = "E";
                    break;
                default:
                    ViewBag.Mensagem = "Ocorreu um ERRO, não foi possível determinar o erro e a senha não foi resetada!";
                    ViewBag.Resp = "E";
                    break;
            }
            return PartialView(resultado);
        }

        public ActionResult SeconSystem(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var resultado = CarregaUsuarios().Find(u => u.Id == id);
            if (resultado == null)
            {
                return HttpNotFound();
            }
            int retorno = SetarSeconsystem(resultado.Usuario);
            switch (retorno)
            {
                case 0:
                    ViewBag.Mensagem = "Usuário já possui o acesso ao SeconSystem";
                    ViewBag.Resp = "N";
                    break;
                case 1:
                    ViewBag.Mensagem = "Foi concedido o acesso ao sistema SeconSystem";
                    ViewBag.Resp = "S";
                    break;
                case 2:
                    ViewBag.Mensagem = "Ocorreu um ERRO, não foi possível determinar o tipo de erro!";
                    ViewBag.Resp = "E";
                    break;
                default:
                    ViewBag.Mensagem = "Ocorreu um ERRO, não foi possível determinar o tipo de erro!";
                    ViewBag.Resp = "E";
                    break;
            }
            return PartialView(resultado);
        }

        public ActionResult Grupos(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var resultado = CarregaUsuarios().Find(u => u.Id == id);
            if (resultado == null)
            {
                return HttpNotFound();
            }
            ArrayList retorno = ADMethods.GetUserGroups(resultado.Usuario);
            ArrayList todosGrupos = CarregaGrupos();
            ViewBag.Grupos = retorno;
            ViewBag.TGrupos = todosGrupos;
            return PartialView(resultado);
        }

        public int ResetarConta(string usuario)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, usuario);
            if (user != null)
            {
                try
                {
                    user.SetPassword("Seconci@1234");
                    user.ExpirePasswordNow();
                    user.Save();
                    return 1;
                }
                catch (Exception e)
                {
                    return 2;
                }
            }
            else { return 0; }
        }

        public int SetarSeconsystem(string usuario)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, usuario);
            if (user != null)
            {
                try
                {
                    if (ADMethods.AddUserToGroup(usuario,"SeconSystem"))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception e)
                {
                    return 2;
                }
            }
            else { return 0; }
        }

        public int DesbloquearConta(string usuario)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            // Localizar o usuário
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, usuario);
            if (user != null)
            {
                if (user.IsAccountLockedOut() || user.Enabled == false)
                {
                    // unlock user
                    user.UnlockAccount();
                    user.Enabled = true;
                    user.Save();
                    //user.Properties["description"].Value = "New description for user";
                    //user.CommitChanges();
                    return 1;
                }
                else { return 0; }
            }
            return 2;
        }


    }
}