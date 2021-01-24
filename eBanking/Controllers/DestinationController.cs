using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using System;
using System.Net;
using System.Web.Mvc;
using PagedList;

namespace eBanking.Controllers
{
    
    [CustomAuth]
    public class DestinationController : Controller
    {

        private IDestinationRepository destination_repo;
        private UserMenuGenarator user_menu = new UserMenuGenarator();

        public DestinationController()
        {
            destination_repo = new DestinationRepository();
        }
        // GET: /Destination/
        public ActionResult Index(int? page, string currentFilter, string sortOrder, int? itemsPerPage)
        {
            try
            {
                int pageNumber = (page ?? 1);
                int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
                ViewBag.ItemsPerPage = pageSize;
                ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
                return View(destination_repo.GetAll().ToPagedList(pageNumber, pageSize));
            }
            catch (Exception)
            {
               
            }

            return View("Error");
            
        }

        // GET: /Destination/Details/5
        public ActionResult Details(int? id)
        {
    
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Destination destination =destination_repo.FindById(id);
         
            if (destination == null)
            {
                return HttpNotFound();
            }

            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View(destination);
        }

        // GET: /Destination/Create
        public ActionResult Create()
        {
            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View();
        }

        // POST: /Destination/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Destination destination)
        {
            if (ModelState.IsValid)
            {
                Destination hasExists = destination_repo.FindByName(destination.DestinationName);

                if (hasExists == null)
                {
                    bool add = destination_repo.Add(destination);

                    if (add == true)
                        return RedirectToAction("Index");
                    else
                        ModelState.AddModelError("", "Can not create currency.Please Try again.");
                }
                else
                    ModelState.AddModelError("", "Destination already exists.");
                
            }

            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View(destination);
        }

        // GET: /Destination/Edit/5
        public ActionResult Edit(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Destination destination =destination_repo.FindById(id);
            if (destination == null)
            {
                return HttpNotFound();
            }
            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View(destination);
        }

        // POST: /Destination/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Destination destination)
        {
            if (ModelState.IsValid)
            {
                Destination existingDestination = destination_repo.FindByName(destination.DestinationName);

                if (existingDestination != null)
                {
                    existingDestination.CountryCode = destination.CountryCode;
                    existingDestination.Description = destination.Description;
                    existingDestination.DestinationName = destination.DestinationName;
                    existingDestination.IsActive = destination.IsActive;
                    bool edit = destination_repo.Edit(existingDestination);
                    if (edit == true)
                        return RedirectToAction("Index");
                    else
                        ModelState.AddModelError("", "Edit Failed.Please Try again.");
                }
                else
                    ModelState.AddModelError("", "Destination does not exists.");
            
            }
            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View(destination);
        }

        // GET: /Destination/Delete/5
        public ActionResult Delete(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Destination destination =destination_repo.FindById(id);

            if (destination == null)
            {
                return HttpNotFound();
            }
            return View(destination);
        }

        // POST: /Destination/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Destination destination =destination_repo.Delete(id);


            if (destination != null)
                return RedirectToAction("Index");
            else
                ModelState.AddModelError("", "Delete Failed.Please Try again.");



            return View(destination);
        }

    }
}
