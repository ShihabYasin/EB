using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using System;
using System.Net;
using System.Web.Mvc;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class StatusController : Controller
    {
        private IStatusRepository status_repo;
        private Variable _variable = new Variable();


        public StatusController()
        {
            this.status_repo = new StatusRepository();
        }

        

        // GET: /Status/
        public ActionResult Index()
        {
            try
            {
                return View(status_repo.GetAll());
            }
            catch (Exception)
            { 
            
            }

            return View("Error");
        }

        // GET: /Status/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatusMsg statusmsg =status_repo.FindById(id);
            if (statusmsg == null)
            {
                return View("Error");
            }
            return View(statusmsg);
        }

        // GET: /Status/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Status/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StatusMsg statusmsg)
        {
            if (ModelState.IsValid)
            {
                bool add = status_repo.Add(statusmsg);
                
                if(add==true)
                   return RedirectToAction("Index");
            }

            return View(statusmsg);
        }

        // GET: /Status/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatusMsg statusmsg =status_repo.FindById(id);
            if (statusmsg == null)
            {
                return View("Error");
            }
            return View(statusmsg);
        }

        // POST: /Status/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StatusMsg statusmsg)
        {
            if (ModelState.IsValid)
            {
                bool edit = status_repo.Edit(statusmsg);
          
                if(edit==true)
                  return RedirectToAction("Index");
            }
            return View(statusmsg);
        }

        // GET: /Status/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatusMsg statusmsg =status_repo.FindById(id);
            if (statusmsg == null)
            {
                return View();
            }
            return View(statusmsg);
        }

        // POST: /Status/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            StatusMsg statusmsg =status_repo.Delete(id);

            if (statusmsg != null)
                return RedirectToAction("Index");
            else
            {
                ModelState.AddModelError("","Delete Failed!");
               
            }
            return View(statusmsg);

        }
    }
}
