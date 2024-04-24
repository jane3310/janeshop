using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Janeshop.Models;

namespace Janeshop.Controllers
{
    public class RegisterController : Controller
    {
        private DB_JANESHOPEntities1 db = new DB_JANESHOPEntities1();
        // GET: Register
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NewCreate()
        {
            return View();
        }
        [HttpPost]
        public JsonResult CheckEmail(string userdata)
        {
            System.Threading.Thread.Sleep(200);
            var SearchData = db.TBL_USER.Where(x => x.email == userdata).SingleOrDefault();
            if (SearchData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewCreate(CusRegister model)
        {
            DB_JANESHOPEntities1 dbG = new DB_JANESHOPEntities1();

            var lastgen = dbG.TBL_GEN_ID.OrderByDescending(x => x.pk_id).FirstOrDefault();
            var year = DateTime.Now.ToString("yyyy", new CultureInfo("en-US"));
            var month = DateTime.Now.ToString("MM", new CultureInfo("en-US"));

            if (lastgen == null)
            {
                ViewBag.gen_id = "SHOP" + year + month + "000000001";
            }
            else
            {
                var lastyear = lastgen.transaction_running_id.Substring(4, 4).ToString();
                var lastmonth = lastgen.transaction_running_id.Substring(8, 2).ToString();
                if (year == lastyear)
                {
                    if (month == lastmonth)
                    {
                        ViewBag.gen_id = "SHOP" + year + month + (Convert.ToInt32(lastgen.transaction_running_id.Substring(10, lastgen.transaction_running_id.Length - 10)) + 1).ToString("D9");
                    }
                    else
                    {
                        ViewBag.gen_id = "SHOP" + year + month + "000000001";
                    }
                }
                else
                {
                    ViewBag.gen_id = "SHOP" + year + month + "000000001";
                }
            }

            TBL_GEN_ID dbGen = new TBL_GEN_ID()
            {
                pk_id = 1,
                transaction_running_id = ViewBag.gen_id
            };

            TBL_USER tbl_user = new TBL_USER()
            {
                customer_id = ViewBag.gen_id,
                title_id = model.title_id,
                fname = model.fname,
                lname = model.lname,
                mobile = model.mobile,
                password = model.password,
                cus_status = "1",
                email=model.email,
                address=model.address,
                picture = "~/Images/user.jpg"
            };

            if (ModelState.IsValid)
            {
                db.Entry(dbGen).State = EntityState.Modified;
                db.TBL_USER.Add(tbl_user);
                db.SaveChanges();

                return RedirectToAction("Success", "Register");
            }
            return View(tbl_user);
        }
        public ActionResult Success()
        {
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();

            }
            base.Dispose(disposing);
        }
    }
}