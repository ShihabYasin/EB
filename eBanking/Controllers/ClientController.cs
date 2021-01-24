using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eBanking.Interface;
using eBanking.Abstract;

namespace eBanking.Controllers
{
    public class ClientController : Controller
    {
        //
        // GET: /Client/
        public ActionResult Index()
        {
            return View();
        }

        public bool ClientIsActive(string Version)
        {
            IAdminRepository admin_repo = new AdminRepository();
            return admin_repo.ClientIsActive(Version);
        }
	}
}