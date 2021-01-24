using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace eBanking.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {

        #region Repository,Context And Variable Declaration

        private IServiceRepository service_repo;
        private IDestinationRepository destination_repo;

        private IEnumerable<ServiceViewModel> ServiceList;
        private UserMenuGenarator user_menu = new UserMenuGenarator();
      
        public ServiceController()
        {
            //db = new eBankingDbContext();
            this.service_repo = new ServiceRepository();
            this.destination_repo = new DestinationRepository();
        }

        #endregion



        /*---------------------------------------------------------
        *  if db context is open then Dispose the context hear  
        *---------------------------------------------------------*/
        // GET: /Service/
        public ActionResult Index()
        {
            try
            {
                ServiceList = service_repo.GetAllToServiceVM(null,null);
               
                List<ServiceViewModel> stack = new List<ServiceViewModel>();
                foreach (var parent in ServiceList.Where(i => i.IsGroup == true))
                {
                    stack.Add(parent);
                    foreach (var child in ServiceList.Where(i => i.ParentId == parent.Id))
                        stack.Add(child);
                }


                if (stack != null)

                {
                
                    return View(stack);

                }
            }
            catch (Exception)
            {
                
            }

            return View("Error");
           
        }

        /*---------------------------------------------------------
       *  if db context is open then Dispose the context hear  
       *---------------------------------------------------------*/
        //public List<ServiceViewModel> ServiceTree(List<ServiceViewModel> items)
        //{
        //    items.ForEach(i => i.Children = items.Where(ch => ch.ParentId == i.Id).ToList());
        //    return items.Where(i => i.ParentId == 0).ToList(); //if parentId=null
        //    //if parentId=0 then return items.Where(i => i.ParentID == 0).ToList();
        //}



       /*---------------------------------------------------------
       *  if db context is open then Dispose the context hear  
       *---------------------------------------------------------*/
        // GET: /Service/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceViewModel detailsService = null;
            try
            {
                detailsService = service_repo.GetServiceVMbyId(id);

                if (detailsService != null)
                {
                    return View(detailsService);
                }
            }
            catch (Exception)
            {

            }
            return View("Not Found, Please try again");
        }

        /*---------------------------------------------------------
       *  if db context is open then Dispose the context hear  
       *---------------------------------------------------------*/
        // GET: /Service/Create
        public ActionResult Create()
        {
            try
            {
                ViewData["Parent"] = new SelectList(service_repo.GetAll(true).Where(x => x.IsGroup == true).ToList(), "Id", "Name");
                ViewData["Destination"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName");

                return View();
            }
            catch (Exception)
            { }
            return View("Error");
        }


       /*---------------------------------------------------------
       * used to create a new service using  service_repo.Add(service)
       *---------------------------------------------------------*/

        // POST: /Service/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Service service)
        {
            if (ModelState.IsValid)
            {
                bool add = service_repo.Add(service);

                if (add == true)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Can not create currency.Please Try again.");
           
                return RedirectToAction("Index");
            }

            ViewData["Parent"] = new SelectList(service_repo.GetAll(true).Where(x => x.ParentId == 0).ToList(), "Id", "Name");
            ViewData["Destination"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName");

            return View(service);
        }


        /*---------------------------------------------------------
       *  Display Edit page
       *---------------------------------------------------------*/
        // GET: /Service/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Service service = service_repo.FindById(id);

            if (service == null)
            {
                return HttpNotFound();
            }
            ViewData["Parent"] = new SelectList(service_repo.GetAll(true).Where(x => x.ParentId == 0).ToList(), "Id", "Name",service.ParentId);             
            ViewData["Destination"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName",service.DestinationId);

            return View(service);
        }

       /*---------------------------------------------------------
       *  Edit a service by given service entity
       *---------------------------------------------------------*/
        // POST: /Service/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Service service)
        {
            if (ModelState.IsValid)
            {
                bool edit = service_repo.Edit(service);

                if (edit == true)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Edit Failed.Please Try again.");
            
                return RedirectToAction("Index");
            }
            ViewData["Parent"] = new SelectList(service_repo.GetAll(true).Where(x => x.ParentId == 0).ToList(), "Id", "Name", service.ParentId);
            ViewData["Destination"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName", service.DestinationId);
            
            return View(service);
        }


        /*---------------------------------------------------------
       *  display service in details that wants to be delete  
       *---------------------------------------------------------*/
        // GET: /Service/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           
            Service service = service_repo.FindById(id);
            if (service != null)
            {

                try
                {
                    var deleteService = service_repo.GetServiceVMbyId(id);


                    if (deleteService != null)
                        return View(deleteService);

                }
                catch (Exception)
                {

                }
            }
           
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        /*---------------------------------------------------------
       *  Delete a service by given service Id using service_repo.Delete(id)  
       *---------------------------------------------------------*/

        // POST: /Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Service service = service_repo.Delete(id);

            if (service != null)
                return RedirectToAction("Index");
            else
            
                ModelState.AddModelError("", "Delete Failed.Please Try again.");

            var deleteService = service_repo.GetServiceVMbyId(id);

            
            return View(deleteService);

        }
    }
}
