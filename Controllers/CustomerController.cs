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
    public class CustomerController : Controller
    {
        private DB_JANESHOPEntities1 db = new DB_JANESHOPEntities1();
        // GET: Customer
        public ActionResult Index(string t_id)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            List<TBL_PRODUCT_TYPE> product_type = db.TBL_PRODUCT_TYPE.ToList();
            ViewBag.product_type = new SelectList(product_type, "type_id", "type_name");

            var proShow = from p in tbl_product
                          join o in tbl_owner on p.store_id equals o.store_id into tbl1
                          from o in tbl1.DefaultIfEmpty(new TBL_OWNER())
                          join pp in tbl_product_picture on p.product_id equals pp.product_id into tbl2
                          from pp in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          join pt in tbl_product_type on p.type_id equals pt.type_id into tbl3
                          from pt in tbl3.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          select new Product { tbl_owner = o, tbl_product = p, tbl_product_picture = pp, tbl_product_type = pt };

            //foreach (Product ss in proShow)
            //{
            //    ViewBag.Store_name = ss.tbl_owner.store_name;
            //    ViewBag.Store_id = ss.tbl_owner.store_id;
            //}

            if (t_id != null)
            {
                var pShow = proShow.Where(x => x.tbl_product_type.type_name == t_id).ToList();
                return View(pShow);
            }
            else
            {
                return View(proShow);
            }
        }
        public ActionResult CustomerDetail()
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string Id = (string)Session["Id"];
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            return View(tbl_user.Where(x => x.customer_id == Id).ToList());
        }
        public ActionResult CustomerEdit()
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string Id = (string)Session["Id"];
            TBL_USER tbl_user = db.TBL_USER.Find(Id);

            if (tbl_user == null)
            {
                return HttpNotFound();
            }

            return View(tbl_user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerEdit(TBL_USER model)
        {
            TBL_USER dbObject = new TBL_USER { customer_id = model.customer_id };

            db.TBL_USER.Attach(dbObject);

            dbObject.customer_id = model.customer_id;
            dbObject.fname = model.fname;
            dbObject.lname = model.lname;
            dbObject.mobile = model.mobile;
            dbObject.password = model.password;
            dbObject.address = model.address;
            dbObject.email = model.email;

            db.SaveChanges();

            return RedirectToAction("CustomerDetail", "Customer");
        }
        public ActionResult Billsend(string o_id)
        {
            List<TBL_ORDER> tbl_order = db.TBL_ORDER.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
     
            var orShow = from o in tbl_order
                         join w in tbl_owner on o.store_id equals w.store_id into tbl1
                         from w in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         select new Product { tbl_owner = w, tbl_order = o };

            foreach(var pShow in orShow.Where(x => x.tbl_order.order_id == o_id))
            {
                ViewBag.order_id = pShow.tbl_order.order_id;
                ViewBag.store_name = pShow.tbl_owner.store_name;              
                ViewBag.order_date = pShow.tbl_order.order_date;
                ViewBag.order_time = pShow.tbl_order.order_time;
                ViewBag.total_bill = pShow.tbl_order.total_bill;

                ViewBag.stord_id = pShow.tbl_owner.store_id;
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Billsend(TBL_ORDER model, HttpPostedFileBase picture)
        {
                string path = uploadimgfileBill(picture);

            if (ModelState.IsValid)
            {
                 TBL_ORDER tbl_order = new TBL_ORDER { order_id = model.order_id };

                db.TBL_ORDER.Attach(tbl_order);

                if (path != "")
                {
                    tbl_order.picture = path;
                }
                tbl_order.order_date = DateTime.Now;
                tbl_order.status = 2;

                db.SaveChanges();
                return RedirectToAction("OrderShow", "Customer");
                }

         //   TempData.Keep();

            return View(model);
        }
        public string uploadimgfileBill(HttpPostedFileBase file)
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
                        path = Path.Combine(Server.MapPath("~/Images/Customer"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Images/Customer/" + random + Path.GetFileName(file.FileName);
                    }
                    catch (Exception )
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
        public ActionResult OrderShow(string option, string search, int? i)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string c_id = (string)Session["Id"];

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.Where(x=>x.customer_id==c_id).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         select new OrderView { tbl_owner = s, tbl_order = o };

            return View(OrShow.Where(x=> x.tbl_order.status == 1 || x.tbl_order.status == 2 || x.tbl_order.status == 3).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
        }
        [HttpPost]
      //  [ValidateAntiForgeryToken]
        public ActionResult OrderDelete(OrderView model,FormCollection c,int? qty, int? amount)
        {
            if (ModelState.IsValid)
            {          
                int i;
                var od_id = c.GetValues("order_detail_id");
            //    var cr_id = c.GetValues("cart_id");
                var p_id = c.GetValues("product_id");

                for (i = 0; i < od_id.Count(); i++)
                {
                    DB_JANESHOPEntities1 dbP = new DB_JANESHOPEntities1();
                    DB_JANESHOPEntities1 dbD = new DB_JANESHOPEntities1();

                    List<TBL_PRODUCT> tbl_products = dbP.TBL_PRODUCT.ToList();
                    var checkpro = tbl_products.Where(x => x.product_id == p_id[i]).ToList();

                    foreach (var itemp in checkpro)
                    {
                        amount = (int?)itemp.amount;
                    }

                    List<TBL_ORDER_DETAIL> tbl_order_detail = dbD.TBL_ORDER_DETAIL.ToList();
                    var checkdetail = tbl_order_detail.Where(x => x.order_detail_id == od_id[i]).ToList();

                    foreach (var item in checkdetail)
                    {
                        qty = (int?)item.qty;
                    }

                    TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = p_id[i] };
                    db.TBL_PRODUCT.Attach(tbl_product);
                    tbl_product.amount = Convert.ToInt32(amount) + Convert.ToInt32(qty);
              
                    TBL_ORDER_DETAIL tbl_od = db.TBL_ORDER_DETAIL.Find(od_id[i]);
                    db.TBL_ORDER_DETAIL.Remove(tbl_od);
                }

                TBL_ORDER tbl_order = db.TBL_ORDER.Find(model.order_id);
                db.TBL_ORDER.Remove(tbl_order);
                db.SaveChanges();

                return RedirectToAction("OrderShow", "Customer");
            }

           return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderConfirm(OrderView model)
        {
            if (ModelState.IsValid)
            {
                TBL_ORDER tbl_order = new TBL_ORDER { order_id = model.order_id };

                db.TBL_ORDER.Attach(tbl_order);
                tbl_order.order_pickup = DateTime.Now;
                tbl_order.status = 4;

                db.SaveChanges();
                return RedirectToAction("OrderShow", "Customer");
            }

            return View(model);
        }
        public ActionResult OrderShowDetail(string o_id, int? CD)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            ViewBag.Check = CD;

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.ToList();
            List<TBL_ORDER_DETAIL> tbl_order_detail = db.TBL_ORDER_DETAIL.ToList();
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join u in tbl_user on o.customer_id equals u.customer_id into tbl2
                         from u in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_order = o, tbl_owner = s, tbl_user = u };

            IEnumerable<OrderView> OrdShows = OrShow.Where(x => x.tbl_order.order_id == o_id);

            foreach (OrderView pShow in OrdShows)
            {
                ViewBag.order_id = pShow.tbl_order.order_id;
                ViewBag.store_name = pShow.tbl_owner.store_name;
                ViewBag.order_date = pShow.tbl_order.order_date;
                ViewBag.order_time = pShow.tbl_order.order_time;
                ViewBag.total_bill = pShow.tbl_order.total_bill;
                ViewBag.place_name = pShow.tbl_user.address;
                ViewBag.status = pShow.tbl_order.status;
            }

            var orderShow = from o in tbl_order
                            join od in tbl_order_detail on o.order_id equals od.order_id into tbl1
                            from od in tbl1.DefaultIfEmpty(new TBL_ORDER_DETAIL())
                            join p in tbl_product on od.product_id equals p.product_id into tbl2
                            from p in tbl2.DefaultIfEmpty(new TBL_PRODUCT())
                            select new OrderView { tbl_order = o, tbl_order_detail = od, tbl_product = p };

            ViewBag.orderdetail = tbl_order_detail.Where(x => x.order_id == o_id).ToList();

            return View(orderShow.Where(x => x.tbl_order.order_id == o_id).ToList());
        }
        public ActionResult StoreShow(string search, int? i)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

        //    TempData.Remove("total");
        //    TempData.Remove("Cart");

            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            TempData.Keep();

            return View(tbl_owner.ToList().ToPagedList(i ?? 1, 10));
        }
        public ActionResult ProductShow(string search, int? i, string s_id, string t_id)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            List<TBL_PRODUCT_TYPE> product_type = db.TBL_PRODUCT_TYPE.Where(x => x.store_id == s_id).ToList();
            ViewBag.product_type = new SelectList(product_type, "type_id", "type_name");

            var proShow = from p in tbl_product
                          join o in tbl_owner on p.store_id equals o.store_id into tbl1
                          from o in tbl1.DefaultIfEmpty(new TBL_OWNER())
                          join pp in tbl_product_picture on p.product_id equals pp.product_id into tbl2
                          from pp in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          join pt in tbl_product_type on p.type_id equals pt.type_id into tbl3
                          from pt in tbl3.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          select new Product { tbl_owner = o, tbl_product = p, tbl_product_picture = pp, tbl_product_type = pt };

            foreach (Product ss in proShow)
            {
                ViewBag.Store_name = ss.tbl_owner.store_name;
            }

            if (t_id != null)
            {
                var pShow = proShow.Where(x=>x.tbl_owner.store_id==s_id && x.tbl_product_type.type_name==t_id).ToList();
                return View(pShow);
            }
            else
            {
                var pShow = proShow.Where(x => x.tbl_owner.store_id == s_id).ToList();
                return View(pShow);
          
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Autherize(TBL_USER userModel)
        {
            if (ModelState.IsValid)
            {
                db.Database.Connection.Open();
                var userDetails = db.TBL_USER.Where(x => x.email == userModel.email && x.password == userModel.password).FirstOrDefault();
                if (userDetails != null)
                {
                    Session["Id"] = userDetails.customer_id;
                    Session["name"] = userDetails.fname;

                    return RedirectToAction("Index", "Customer");
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
            string Id = (string)Session["Id"];
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}