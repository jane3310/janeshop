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
using PagedList;

namespace Janeshop.Controllers
{
    public class AdminController : Controller

    {
        private DB_JANESHOPEntities1 db = new DB_JANESHOPEntities1();
        // GET: Admin
        public ActionResult Index()
        {
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ida"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id).ToList();

            return View(proView);
        }
        public ActionResult ProductPrint()
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ida"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id).ToList();

            return View(pView);
        }
        public ActionResult SalePrint(int? i)
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string s_id = (string)TempData["s_id"];
            DateTime DateM1 = Convert.ToDateTime(TempData["D1"]);
            DateTime DateM2 = Convert.ToDateTime(TempData["D2"]);

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.Where(x => x.status == 4).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join c in tbl_user on o.customer_id equals c.customer_id into tbl2
                         from c in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_owner = s, tbl_order = o, tbl_user = c };

            if (TempData["D1"] != null || TempData["D2"] != null || TempData["s_id"] != null)
            {
                return View(OrShow.Where(x => x.tbl_order.order_time >= DateM1 && x.tbl_order.order_time <= DateM2 && x.tbl_owner.store_id == s_id).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
            else
            {
                return View(OrShow.OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
        }
        [HttpGet]
        public ActionResult SaleShow(int? i, string M1, string M2,string s_id)
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            DateTime DateM1 = Convert.ToDateTime(M1);
            DateTime DateM2 = Convert.ToDateTime(M2);

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.Where(x => x.status == 4).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            ViewBag.store_name = new SelectList(tbl_owner, "store_id", "store_name");

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join c in tbl_user on o.customer_id equals c.customer_id into tbl2
                         from c in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_owner = s, tbl_order = o, tbl_user = c };

            if (M1 != null || M2 != null || s_id != null)
            {
                TempData["d1"] = DateM1;
                TempData["d2"] = DateM2;
                TempData["s_id"] = s_id;
                return View(OrShow.Where(x => x.tbl_order.order_time >= DateM1 && x.tbl_order.order_time <= DateM2 && x.tbl_owner.store_id == s_id).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));

            }
            else
            {
                TempData.Remove("d1");
                TempData.Remove("d2");
                TempData.Remove("s_id");
                return View(OrShow.OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
        }
        public ActionResult OrderShow(int? i, string M1, string M2, string s_id)
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            DateTime DateM1 = Convert.ToDateTime(M1);
            DateTime DateM2 = Convert.ToDateTime(M2);

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.Where(x => x.status == 2 || x.status == 3).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            ViewBag.store_name = new SelectList(tbl_owner, "store_id", "store_name");

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join c in tbl_user on o.customer_id equals c.customer_id into tbl2
                         from c in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_owner = s, tbl_order = o, tbl_user = c };

            if (s_id != null)
            {
                return View(OrShow.Where(x => x.tbl_owner.store_id == s_id).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
            else
            {
                return View(OrShow.OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
        }
        public ActionResult ProductShow(string s_id)
        {
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            ViewBag.store_name = new SelectList(tbl_owner, "store_id", "store_name");

            if(s_id != null)
            {
                ViewBag.CNumber = proView.Where(x => x.tbl_product.store_id == s_id).Count();
                return View(proView.Where(x => x.tbl_product.store_id == s_id).ToList());
            }
            else
            {
                ViewBag.CNumber = proView.Count();
                return View(proView);
            }
        }
        public ActionResult CustomerShow()
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();
            ViewBag.CNumber = tbl_user.Count();

            return View(tbl_user);
        }
        public ActionResult StoreShow()
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_DISTRICT> tbl_district = db.TBL_DISTRICT.ToList();

            var sView = from o in tbl_owner
                          join d in tbl_district on o.district_id equals d.district_id into tbl1
                          from d in tbl1.DefaultIfEmpty(new TBL_DISTRICT())
                          select new Store { tbl_district = d, tbl_owner = o};

            ViewBag.CNumber = tbl_owner.Count();

            return View(sView);
        }
        public ActionResult DistrictCreate()
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult DistrictCreate(TBL_DISTRICT model)
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

                TBL_DISTRICT tbl_district = new TBL_DISTRICT()
                {
                    district_id = ViewBag.gen_id,
                    district_name = model.district_name
                };
                if (ModelState.IsValid)
                {
                    db.Entry(dbGen).State = EntityState.Modified;
                    db.TBL_DISTRICT.Add(tbl_district);
                    db.SaveChanges();

                    return RedirectToAction("index", "Admin");
                }

            return View();
        }
        public ActionResult DistrictShow()
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_DISTRICT> tbl_district = db.TBL_DISTRICT.ToList();

            return View(tbl_district);
        }
        public ActionResult DistrictEdit(string d_id)
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            TBL_DISTRICT tbl_district = db.TBL_DISTRICT.Find(d_id);

            if (tbl_district == null)
            {
                return HttpNotFound();
            }

            return View(tbl_district);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DistrictEdit(TBL_DISTRICT model)
        {
            TBL_DISTRICT tbl_district = new TBL_DISTRICT { district_id = model.district_id };

            db.TBL_DISTRICT.Attach(tbl_district);

            tbl_district.district_id = model.district_id;
            tbl_district.district_name = model.district_name;

            db.SaveChanges();
            return RedirectToAction("DistrictShow", "Admin");
        }

        [ActionName("DistrictDelete")]
        public ActionResult DeleteConfirmed(string d_id)
        {
            try
            {
                TBL_DISTRICT tbl_district = db.TBL_DISTRICT.Find(d_id);
                db.TBL_DISTRICT.Remove(tbl_district);
                db.SaveChanges();

                return RedirectToAction("DistrictShow", "Admin");
            }
            catch (Exception)
            {
                return RedirectToAction("DistrictShow", "Admin");
            }
        }
        public ActionResult StoreCreate()
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_DISTRICT> tbl_district = db.TBL_DISTRICT.ToList();
            ViewBag.district = new SelectList(tbl_district, "district_id", "district_name");

            return View();
        }
        [HttpPost]
        public ActionResult StoreCreate(TBL_OWNER model, HttpPostedFileBase picture)
        {
            string path = uploadimgfileStore(picture);
            if (path.Equals("-1"))
            {
                ViewBag.error = "ไฟล์รูปภาพไม่สามารถอัพโหลดได้..";
            }
            else
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

                TBL_OWNER tbl_owner = new TBL_OWNER()
                {
                    store_id = ViewBag.gen_id,
                    store_name = model.store_name,
                    address = model.address,
                    district_id = model.district_id,
                    mobile = model.mobile,
                    email = model.email,
                    password = model.password,
                    picture = path,
                    status = "1"
                };
                if (ModelState.IsValid)
                {
                    db.Entry(dbGen).State = EntityState.Modified;
                    db.TBL_OWNER.Add(tbl_owner);
                    db.SaveChanges();

                    return RedirectToAction("index", "Admin");
                }
            }
            return View();
        }
        public string uploadimgfileStore(HttpPostedFileBase file)//--เอารูปภาพเข้ามาเก็บใน Images
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {
                        path = Path.Combine(Server.MapPath("~/Images/Store"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Images/Store/" + random + Path.GetFileName(file.FileName);
                    }
                    catch (Exception)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('เพิ่มไฟล์นามสกุล jpg ,jpeg or png เท่านั้น.....');</script>");
                }
            }
            else
            {
                Response.Write("<script>alert('กรุณาเพิ่มรูปภาพประกอบ');</script>");
                path = "";
            }
            return path;
        }
        public ActionResult StoreEdit(string s_id,string d_id)
        {
            if (Session["Ida"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            if (s_id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TBL_OWNER tbl_owner = db.TBL_OWNER.Find(s_id);

            List<TBL_DISTRICT> tbl_district = db.TBL_DISTRICT.ToList();
            ViewBag.district = new SelectList(tbl_district, "district_id", "district_name");

            if (tbl_owner == null)
            {
                return HttpNotFound();
            }

            return View(tbl_owner);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StoreEdit(TBL_OWNER model, HttpPostedFileBase picture)
        {
            string path = uploadimgfileStore(picture);
            TBL_OWNER dbObject = new TBL_OWNER { store_id = model.store_id };

            db.TBL_OWNER.Attach(dbObject);

            dbObject.store_id = model.store_id;
            dbObject.store_name = model.store_name;
            dbObject.mobile = model.mobile;
            dbObject.password = model.password;
            dbObject.address = model.address;
            dbObject.district_id = model.district_id;
            dbObject.email = model.email;
            if (path != "")
            {
                dbObject.picture = path;
            }

            db.SaveChanges();
            return RedirectToAction("StoreShow", "Admin");
        }
      
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Autherize(TBL_ADMIN userModel)
        {
            if (ModelState.IsValid)
            {
                db.Database.Connection.Open();
                var userDetails = db.TBL_ADMIN.Where(x => x.email == userModel.email && x.password == userModel.password ).FirstOrDefault();
                if (userDetails != null)
                {
                    Session["Ida"] = userDetails.admin_id;
                    Session["namea"] = userDetails.name;
                   
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    userModel.LoginErrorMessage = "อีเมล์หรือรหัสผ่านผิด";
                    return View("Login", userModel);
                }
            }
            return View("Index");
        }
        public ActionResult LogOut()
        {
            string Id = (string)Session["Ida"];
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}