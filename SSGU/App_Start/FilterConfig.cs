using System;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SSGU
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

    public class AuthorizeAD : AuthorizeAttribute
    {
        public string Groups { get; set; }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string NomeDominio = ConfigurationManager.AppSettings["Dominio"].ToString();
            
            if (base.AuthorizeCore(httpContext))
            {
                /* Return true immediately if the authorization is not 
                locked down to any particular AD group */
                if (String.IsNullOrEmpty(Groups))
                    return true;

                // Get the AD groups
                var groups = Groups.Split(',').ToList();

                // Verify that the user is in the given AD group (if any)
                var context = new PrincipalContext(
                                      ContextType.Domain,
                                      NomeDominio);

                var userPrincipal = UserPrincipal.FindByIdentity(
                                       context,
                                       IdentityType.SamAccountName,
                                       httpContext.User.Identity.Name);

                foreach (var group in groups)
                    if (userPrincipal.IsMemberOf(context,
                         IdentityType.Name,
                         group))
                        return true;
            }
            return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                //var url = new UrlHelper(filterContext.RequestContext);
                //var NaoPode = url.Content("~/NaoAutorizado.cshtml");
                filterContext.HttpContext.Response.Redirect(urlHelper.Action("NaoAutorizado", "Home"), true);
            }
        }

    }


}

