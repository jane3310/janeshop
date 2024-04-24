using PagedList;
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
using Newtonsoft.Json;

namespace Janeshop.Controllers
{
    public class StoreController : Controller
    {
        // GET: Store
        private DB_JANESHOPEntities1 db = new DB_JANESHOPEntities1();
        public ActionResult Index(int? i)
        {
            if (Session["Ids"] == null)
               {
                   Response.Redirect("~/Home/Index");
               }

            List<DataPoint> dataPoints = new List<DataPoint>();

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_ORDER> tbl_order = db.TBL_ORDER.ToList();

            foreach (var item in tbl_product.Where(x => x.store_id == (string)Session["Ids"]))
            {
                dataPoints.Add(new DataPoint(item.product_name, (int)item.sale));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            ViewBag.Order = tbl_order.Where(x => x.store_id == (string)Session["Ids"]).Count();
            ViewBag.OrderShow = tbl_order.Where(x => x.store_id == (string)Session["Ids"] && x.status == 2).Count();
            ViewBag.OrderSend = tbl_order.Where(x => x.store_id == (string)Session["Ids"] && x.status == 4).Count();
            ViewBag.PriceTotal = tbl_order.Where(x => x.store_id == (string)Session["Ids"] && x.status == 4).Sum(x => x.total_bill);

            return View();
        }
        public ActionResult BankCreate()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult BankCreate(TBL_BANK model)
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

            TBL_BANK tbl_bank = new TBL_BANK()
            {
                bank_id = ViewBag.gen_id,
                store_id = (string)Session["Ids"],
                bank_name = model.bank_name,
                bank_number = model.bank_number,
                name = model.name,
                status = 0
            };

            if (ModelState.IsValid)
            {
                db.Entry(dbGen).State = EntityState.Modified;
                db.TBL_BANK.Add(tbl_bank);
                db.SaveChanges();

                return RedirectToAction("BankShow", "Store");
            }

            return View();
        }
        public ActionResult BankEdit(string b_id)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            TBL_BANK tbl_bank = db.TBL_BANK.Find(b_id);

            if (tbl_bank == null)
            {
                return HttpNotFound();
            }

            return View(tbl_bank);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BankEdit(TBL_BANK model)
        {
            TBL_BANK tbl_bank = new TBL_BANK { bank_id = model.bank_id };

            db.TBL_BANK.Attach(tbl_bank);

            tbl_bank.bank_id = model.bank_id;
            tbl_bank.bank_name = model.bank_name;
            tbl_bank.bank_number = model.bank_number;
            tbl_bank.name = model.name;

            db.SaveChanges();
            return RedirectToAction("BankShow", "Store");
        }

        [ActionName("BankDelete")]
        public ActionResult DeleteBankConfirmed(string b_id)
        {
            try
            {
                TBL_BANK tbl_bank = db.TBL_BANK.Find(b_id);
                db.TBL_BANK.Remove(tbl_bank);
                db.SaveChanges();

                return RedirectToAction("BankShow", "Store");
            }
            catch (Exception)
            {
                return RedirectToAction("BankShow", "Store");
            }
        }
        public ActionResult BankShow()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string s_id = (string)Session["Ids"];

            List<TBL_BANK> tbl_bank = db.TBL_BANK.Where(x => x.store_id == s_id).ToList();

            return View(tbl_bank);
        }
        public ActionResult SalePrint(int? i)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string s_id = (string)Session["Ids"];
            DateTime DateM1 = Convert.ToDateTime(TempData["D1"]);
            DateTime DateM2 = Convert.ToDateTime(TempData["D2"]);

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.Where(x => x.store_id == s_id && x.status == 4).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join c in tbl_user on o.customer_id equals c.customer_id into tbl2
                         from c in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_owner = s, tbl_order = o, tbl_user = c };

            if (TempData["D1"] != null || TempData["D2"] != null)
            {
                return View(OrShow.Where(x => x.tbl_order.order_time >= DateM1 && x.tbl_order.order_time <= DateM2).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
            else
            {
                return View(OrShow.OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
        }
        [HttpGet]
        public ActionResult SaleShow(int? i,string M1,string M2)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            DateTime DateM1 = Convert.ToDateTime(M1);
            DateTime DateM2 = Convert.ToDateTime(M2);

            string s_id = (string)Session["Ids"];

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.Where(x => x.store_id == s_id && x.status == 4).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join c in tbl_user on o.customer_id equals c.customer_id into tbl2
                         from c in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_owner = s, tbl_order = o, tbl_user = c };

            if(M1 != null || M2 != null)
            {
                TempData["d1"] = DateM1;
                TempData["d2"] = DateM2;
                return View(OrShow.Where(x => x.tbl_order.order_time >= DateM1 && x.tbl_order.order_time <= DateM2).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
            else
            {
                TempData.Remove("d1");
                TempData.Remove("d2");
                return View(OrShow.OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));
            }
        }
        public ActionResult OrderShow(int? i)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string s_id = (string)Session["Ids"];

            List<TBL_ORDER> tbl_order = db.TBL_ORDER.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_USER> tbl_user = db.TBL_USER.ToList();

            var OrShow = from o in tbl_order
                         join s in tbl_owner on o.store_id equals s.store_id into tbl1
                         from s in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join c in tbl_user on o.customer_id equals c.customer_id into tbl2
                         from c in tbl2.DefaultIfEmpty(new TBL_USER())
                         select new OrderView { tbl_owner = s, tbl_order = o, tbl_user = c };

            return View(OrShow.Where(x => x.tbl_order.store_id == s_id && x.tbl_order.status == 2 || x.tbl_order.status == 3).ToList().OrderByDescending(x => x.tbl_order.order_id).ToPagedList(i ?? 1, 10));

        }
        public ActionResult PictureEdit(string p_id)
        {
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.Where(x => x.product_id == p_id).ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            //    string Id = (string)Session["Id"];

            var pdShow = from p in tbl_product
                         join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl1
                         from pt in tbl1.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                         select new Product { tbl_product = p, tbl_product_picture = pt };

            foreach (var pShow in pdShow)
            {
                ViewBag.picture_id = pShow.tbl_product_picture.picture_id;
                ViewBag.product_id = pShow.tbl_product.product_id;
                ViewBag.product_name = pShow.tbl_product.product_name;
                ViewBag.product_detail = pShow.tbl_product.product_detail;
                ViewBag.price = pShow.tbl_product.price;

                List<string> picture = pShow.tbl_product_picture.picture_url.Split(new char[] { ',' }).ToList();

                foreach(var pic in picture)
                {
                    ViewBag.picture = pic;
                }         
            }       
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PictureEdit(TBL_PRODUCT_PICTURE model, HttpPostedFileBase[] picture)
        {
            string path = uploadimgfile(picture);

            if (ModelState.IsValid)
            {
                TBL_PRODUCT_PICTURE tbl_product_picture = new TBL_PRODUCT_PICTURE { picture_id = model.picture_id };

                db.TBL_PRODUCT_PICTURE.Attach(tbl_product_picture);

                if (path != "")
                {
                    tbl_product_picture.picture_url = path;
                }

                db.SaveChanges();
                return RedirectToAction("ProductShow", "Store");
            }

            return View();
        }

        public ActionResult OrderShowDetail(string o_id, int? cd)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            ViewBag.cd = cd;

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
                ViewBag.customer_name = pShow.tbl_user.fname +""+ pShow.tbl_user.lname;
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

            return View(orderShow.Where(x => x.tbl_order.order_id == o_id).ToList());
        }
        public ActionResult StoreDetail()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string Id = (string)Session["Ids"];
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            return View(tbl_owner.Where(x => x.store_id == Id).ToList());
        }
        public ActionResult StoreEdit()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string Id = (string)Session["Ids"];
            TBL_OWNER tbl_owner = db.TBL_OWNER.Find(Id);

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
            dbObject.email = model.email;
            if (path != "")
            {
                dbObject.picture = path;
            }

            db.SaveChanges();
            return RedirectToAction("StoreDetail", "Store");
        }
        public string uploadimgfileStore(HttpPostedFileBase file)
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
        public ActionResult TypeCreate()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult TypeCreate(TBL_PRODUCT_TYPE model, HttpPostedFileBase[] picture)
        {
            DB_JANESHOPEntities1 dbG = new DB_JANESHOPEntities1();

            string path = uploadimgfiletype(picture);

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

                TBL_PRODUCT_TYPE tbl_product_type = new TBL_PRODUCT_TYPE()
                {
                    type_id = ViewBag.gen_id,
                    type_name = model.type_name,
                    store_id = (string)Session["Ids"],
                    picture = path,
                    status = "1"
                };
                if (ModelState.IsValid)
                {
                    db.Entry(dbGen).State = EntityState.Modified;
                    db.TBL_PRODUCT_TYPE.Add(tbl_product_type);
                    db.SaveChanges();

                    return RedirectToAction("Success", "Store");
                }

            return View();
        }
        public ActionResult TypeEdit(string t_id)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            TBL_PRODUCT_TYPE tbl_product_type = db.TBL_PRODUCT_TYPE.Find(t_id);

            ViewBag.picture = tbl_product_type.picture;

            if (tbl_product_type == null)
            {
                return HttpNotFound();
            }

            return View(tbl_product_type);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TypeEdit(TBL_PRODUCT_TYPE model, HttpPostedFileBase[] picture)
        {
            string path = uploadimgfiletype(picture);
            TBL_PRODUCT_TYPE tbl_product_type = new TBL_PRODUCT_TYPE { type_id = model.type_id };

            db.TBL_PRODUCT_TYPE.Attach(tbl_product_type);

            tbl_product_type.type_id = model.type_id;
            tbl_product_type.type_name = model.type_name;
            if (path != "-1")
            {
                tbl_product_type.picture = path;
            }

            db.SaveChanges();
            return RedirectToAction("Success", "Store");
        }

        [ActionName("TypeDelete")]
        public ActionResult DeleteConfirmed(string t_id)
        {
            try
            {
                TBL_PRODUCT_TYPE tbl_product_type = db.TBL_PRODUCT_TYPE.Find(t_id);
                db.TBL_PRODUCT_TYPE.Remove(tbl_product_type);
                db.SaveChanges();

                return RedirectToAction("Success","Store");
            }
            catch (Exception )
            {
                return RedirectToAction("TypeShow", "Store");
            }
        }
        public ActionResult TypeShow()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string s_id = (string)Session["Ids"];

            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.Where(x => x.store_id == s_id).ToList();

            return View(tbl_product_type);
        }
        public ActionResult ProductCreate()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string s_id = (string)Session["Ids"];

            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.Where(x=>x.store_id==s_id).ToList();
            ViewBag.product_type = new SelectList(tbl_product_type, "type_id", "type_name");

            return View();
        }
        public ActionResult Success(string scheck)
        {
            ViewBag.scheck = scheck;

            return View();
        }
        [HttpPost]
        public ActionResult OrderSend(TBL_ORDER model,FormCollection c,int? sale)
        {
            if (ModelState.IsValid)
            {
                int i = 0;
                var p_id = c.GetValues("product_id");
                var qty = c.GetValues("qty");

                for (i = 0; i < p_id.Count(); i++)
                {
                    DB_JANESHOPEntities1 dbP = new DB_JANESHOPEntities1();

                    List<TBL_PRODUCT> tbl_products = dbP.TBL_PRODUCT.ToList();
                    var checkpro = tbl_products.Where(x => x.product_id == p_id[i]).ToList();

                    foreach (var itemp in checkpro)
                    {
                        sale = (int?)itemp.sale;
                    }

                    TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = p_id[i] };
                    db.TBL_PRODUCT.Attach(tbl_product);
                    tbl_product.sale = Convert.ToInt32(sale) + Convert.ToInt32(qty[i]);
                }
           
                TBL_ORDER tbl_order = new TBL_ORDER { order_id = model.order_id };

                db.TBL_ORDER.Attach(tbl_order);
                tbl_order.order_send = DateTime.Now;
                tbl_order.status = 3;

                db.SaveChanges();
                return RedirectToAction("Index", "Store");
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult ProductCreate(TBL_PRODUCT model, HttpPostedFileBase[] picture)
        {
            string path = uploadimgfile(picture);
            if (path.Equals("-1"))
            {
                ViewBag.error = "Image could not be uploaded...";
            }
            else
            {
                DB_JANESHOPEntities1 dbGen = new DB_JANESHOPEntities1();
             
                var lastgen = dbGen.TBL_GEN_ID.OrderByDescending(x => x.pk_id).FirstOrDefault();
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

                if (model.IsCheck == true){
                   ViewBag.cb = 1;
                }
                else
                {
                   ViewBag.cb = 0;
                }

                TBL_GEN_ID dbGenTrans = new TBL_GEN_ID()
                {
                    pk_id = 1,
                    transaction_running_id = ViewBag.gen_id
                };

                TBL_PRODUCT tbl_product = new TBL_PRODUCT()
                {
                    product_id = ViewBag.gen_id,
                    product_name = model.product_name,
                    store_id = (string)Session["Ids"],
                    type_id = model.type_id,
                    price = model.price,
                    product_detail = model.product_detail,
                    amount = model.amount,
                    sale = 0,
                    status = (int)ViewBag.cb                  
                };

                TBL_PRODUCT_PICTURE tbl_product_picture = new TBL_PRODUCT_PICTURE()
                {
                    product_id = ViewBag.gen_id,
                    picture_url = path
                };

                if (ModelState.IsValid)
                {
                    db.Entry(dbGenTrans).State = EntityState.Modified;
                    db.TBL_PRODUCT.Add(tbl_product);
                    db.TBL_PRODUCT_PICTURE.Add(tbl_product_picture);
                    db.SaveChanges();

                    return RedirectToAction("Success", "Store",new { scheck="1"});
                }
            }
            return View();
        }
        public string uploadimgfile(HttpPostedFileBase[] file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();

            if (file[0] != null)
            {
                List<string> data1 = new List<string>();
                foreach (HttpPostedFileBase photo in file)
                {
                    string filename = Path.GetFileName(photo.FileName);
                    photo.SaveAs(Server.MapPath("~/Images/Product/" + filename));
                    data1.Add("~/Images/Product/" + filename);
                }
                path = string.Join(",", data1.ToArray());
            }

            return path;
        }
        public string uploadimgfiletype(HttpPostedFileBase[] file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();

            if (file[0] != null)
            {
                List<string> data1 = new List<string>();
                foreach (HttpPostedFileBase photo in file)
                {
                    string filename = Path.GetFileName(photo.FileName);
                    photo.SaveAs(Server.MapPath("~/Images/Type/" + filename));
                    data1.Add("~/Images/Type/" + filename);
                }
                path = string.Join(",", data1.ToArray());
            }

            return path;
        }
        public ActionResult ProductAmount()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ids"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id).ToList();

            return View(pView);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductAmount(TBL_PRODUCT model)
        {
            TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = model.product_id };

            db.TBL_PRODUCT.Attach(tbl_product);

            tbl_product.amount = model.amount;

            db.SaveChanges();
            return RedirectToAction("ProductAmount", "Store");
        }
        public ActionResult ProductView()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ids"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id).ToList();

            return View(pView);
        }
        public ActionResult ProductPrint()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ids"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id).ToList();

            return View(pView);
        }
        public ActionResult ProductShow()
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ids"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id).ToList();

            return View(pView);
        }
        public ActionResult ProductShowDetail(string p_id)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            string s_id = (string)Session["Ids"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt };

            var pView = proView.Where(x => x.tbl_product.store_id == s_id && x.tbl_product.product_id == p_id).ToList();

            return View(pView);
        }
        public ActionResult ProductEdit(string p_id)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            TBL_PRODUCT tbl_product = db.TBL_PRODUCT.Find(p_id);

            string s_id = (string)Session["Ids"];

            List<TBL_PRODUCT_TYPE> tbl_type = db.TBL_PRODUCT_TYPE.Where(x => x.store_id == s_id).ToList();
            ViewBag.product_type = new SelectList(tbl_type, "type_id", "type_name");

            if (tbl_product == null)
            {
                return HttpNotFound();
            }

            return View(tbl_product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductEdit(TBL_PRODUCT model)
        {
            TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = model.product_id };

            if (model.IsCheck == true)
            {
                ViewBag.cb = 1;
            }
            else
            {
                ViewBag.cb = 0;
            }

            db.TBL_PRODUCT.Attach(tbl_product);

            tbl_product.product_id = model.product_id;
            tbl_product.type_id = model.type_id;
            tbl_product.product_name = model.product_name;
            tbl_product.product_detail = model.product_detail;
            tbl_product.price = model.price;
            tbl_product.status = (int)ViewBag.cb;

            db.SaveChanges();
            return RedirectToAction("Success", "Store", new { scheck = "1" });
        }

        [ActionName("ProductDelete")]
        public ActionResult DeleteProConfirmed(string p_id)
        {
            try
            {
                TBL_PRODUCT tbl_product = db.TBL_PRODUCT.Find(p_id);
                db.TBL_PRODUCT.Remove(tbl_product);
                db.SaveChanges();

                return RedirectToAction("ProductShow", "Store");
            }
            catch (Exception)
            {
                return RedirectToAction("ProductShow", "Store");
            }
        }
        public JsonResult GetProTypeList()
        {
            string s_id = (string)Session["Ids"];
            db.Configuration.ProxyCreationEnabled = false;
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.Where(x => x.store_id == s_id).ToList();
            return Json(tbl_product_type, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Autherize(TBL_OWNER userModel)
        {
            if (ModelState.IsValid)
            {
                db.Database.Connection.Open();
                var userDetails = db.TBL_OWNER.Where(x => x.email == userModel.email && x.password == userModel.password).FirstOrDefault();
                if (userDetails != null)
                {
                    Session["Ids"] = userDetails.store_id;
                    Session["names"] = userDetails.store_name;
                    Session["pictures"] = userDetails.picture;

                    return RedirectToAction("Index", "Store");
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
            string Id = (string)Session["Ids"];
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}