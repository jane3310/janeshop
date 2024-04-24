using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Janeshop.Models;
using Newtonsoft.Json;

namespace Janeshop.Controllers
{
    public class OrderController : Controller
    {
        public DB_JANESHOPEntities1 db = new DB_JANESHOPEntities1();
        // GET: Order
        public ActionResult Index(int? i)
        {
            if (Session["Ids"] == null)
            {
                Response.Redirect("~/Home/Index");
            }         
            return View();
        }
        public ActionResult AdtoCart(string p_id)
        {
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            var proShow = from p in tbl_product
                          join o in tbl_owner on p.store_id equals o.store_id into tbl1
                          from o in tbl1.DefaultIfEmpty(new TBL_OWNER())
                          join pp in tbl_product_picture on p.product_id equals pp.product_id into tbl2
                          from pp in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          join pt in tbl_product_type on p.type_id equals pt.type_id into tbl3
                          from pt in tbl3.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          select new Product { tbl_owner = o, tbl_product = p, tbl_product_picture = pp, tbl_product_type = pt };

            foreach (Product pS in proShow.Where(x => x.tbl_product.product_id == p_id).ToList())
            {
                ViewBag.store_id = pS.tbl_owner.store_id;
                ViewBag.store_name = pS.tbl_owner.store_name;
                ViewBag.product_id = pS.tbl_product.product_id;
                ViewBag.product_name = pS.tbl_product.product_name;
                ViewBag.product_detail = pS.tbl_product.product_detail;
                ViewBag.product_price = Convert.ToString(pS.tbl_product.price);
            }

            var proShows = from p in tbl_product
                                  join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl1
                                  from pt in tbl1.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                                  select new Product { tbl_product_picture = pt, tbl_product = p };

            foreach(var item in proShow.Where(x=>x.tbl_product.product_id == p_id))
            {
                List<string> result = item.tbl_product_picture.picture_url.Split(new char[] { ',' }).ToList();

                foreach (var photo in result)
                {
                    if (item.tbl_product_picture.picture_url != null)
                    {
                        ViewBag.picture = photo;
                    }
                    else
                    {
                    }
                }
            }

            TempData.Keep();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdtoCart(TBL_CART model,int? qtyc, string cr_id, string p_id,int? amount)
        {
            DB_JANESHOPEntities1 dbG = new DB_JANESHOPEntities1();
            DB_JANESHOPEntities1 dbC = new DB_JANESHOPEntities1();
            DB_JANESHOPEntities1 dbP = new DB_JANESHOPEntities1();

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

            string c_id = (string)Session["Id"];

            List<TBL_PRODUCT> tbl_products = dbP.TBL_PRODUCT.ToList();
            var checkpro = tbl_products.Where(x => x.product_id == model.product_id).ToList();

            foreach(var itemp in checkpro)
            {
                amount = (int?)itemp.amount;
            }

            List<TBL_CART> tbl_carts = dbC.TBL_CART.ToList();
            var checkcart = tbl_carts.Where(x => x.customer_id == c_id && x.store_id == model.store_id && x.product_id == model.product_id).ToList();
            var checkcart2 = tbl_carts.Where(x => x.customer_id == c_id && x.store_id == model.store_id && x.product_id == model.product_id).FirstOrDefault();

            foreach (var item in checkcart)
            {
                cr_id = item.cart_id;
                p_id = item.product_id;
                qtyc = item.qty;
            }

            if (checkcart2 != null) 
            {
                if (ModelState.IsValid) 
                {
                    TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = p_id };
                    TBL_CART tbl_cart = new TBL_CART { cart_id = cr_id };

                    db.TBL_PRODUCT.Attach(tbl_product);
                    tbl_product.amount = Convert.ToInt32(amount) - Convert.ToInt32(model.qty);

                    db.TBL_CART.Attach(tbl_cart);

                    var qtys = (int?)model.qty + qtyc;

                    tbl_cart.cart_id = cr_id;
                    tbl_cart.qty = qtys;
                    tbl_cart.pricetotal = model.price * qtys; 

                    db.SaveChanges();

                    return RedirectToAction("Success", "Order");
                }
            }
            else
            {   
                TBL_CART tbl_cart = new TBL_CART()
                {
                    cart_id = ViewBag.gen_id,
                    customer_id = c_id,
                    store_id = model.store_id,
                    product_id = model.product_id,
                    qty = (int?)model.qty,
                    pricetotal = model.price * (int?)model.qty,
                    status = 1
                };

                TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = model.product_id };
                db.TBL_PRODUCT.Attach(tbl_product);
                tbl_product.amount = Convert.ToInt32(amount) - Convert.ToInt32(model.qty);         
  
                if (ModelState.IsValid)
                {                  
                    db.Entry(dbGen).State = EntityState.Modified;
                    db.TBL_CART.Add(tbl_cart);
                    db.SaveChanges();
                 
                    return RedirectToAction("Success", "Order");
                }
            }

            return View();
        }
        public ActionResult CartShow(string cr_id)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }

            string c_id = (string)Session["Id"];

            List<TBL_CART> tbl_cart = db.TBL_CART.Where(x=>x.customer_id == c_id).ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            var OrShow = from c in tbl_cart
                         join o in tbl_owner on c.store_id equals o.store_id into tbl1
                         from o in tbl1.DefaultIfEmpty(new TBL_OWNER())
                         join p in tbl_product on c.product_id equals p.product_id into tbl2
                         from p in tbl2.DefaultIfEmpty(new TBL_PRODUCT())
                         join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl3
                         from pt in tbl3.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                         select new OrderView { tbl_owner = o, tbl_cart = c, tbl_product = p, tbl_product_picture = pt};
            
            return View(OrShow);
        }
        public ActionResult CheckCart(string s_id)
        {
            if (Session["Id"] == null)
            {
                Response.Redirect("~/Home/Index");
            }
            string c_id = (string)Session["Id"];

            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_CART> tbl_cart = db.TBL_CART.Where(x => x.customer_id == c_id ).ToList();
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();

            foreach(TBL_OWNER s in tbl_owner.Where(x=>x.store_id==s_id))
            {
                ViewBag.store_id = s.store_id; 
                ViewBag.store_name = s.store_name;
            }

            var CartShow = from c in tbl_cart
                           join o in tbl_owner on c.store_id equals o.store_id into tbl1
                           from o in tbl1.DefaultIfEmpty(new TBL_OWNER())
                           join p in tbl_product on c.product_id equals p.product_id into tbl2
                           from p in tbl2.DefaultIfEmpty(new TBL_PRODUCT())
                           join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl3
                           from pt in tbl3.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                           select new OrderView { tbl_cart = c, tbl_owner = o, tbl_product = p, tbl_product_picture = pt };

            var CShow = CartShow.Where(x => x.tbl_cart.store_id == s_id);

            ViewBag.pricetotal = CShow.Sum(x => x.tbl_cart.pricetotal);

            return View(CShow);
        }
        [HttpPost]
        public ActionResult CheckCart(TBL_OWNER model, FormCollection c)
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

            ViewBag.customer_id = Session["Id"];


            TBL_ORDER tbl_order = new TBL_ORDER()
            {
                order_id = ViewBag.gen_id,
                customer_id = ViewBag.customer_id,
                store_id = model.store_id,
                order_date = DateTime.Now,
                order_time = DateTime.Now,
                total_bill = model.total_bill,
                status = 1
            };

            if (ModelState.IsValid)
            {
                int i = 0;
                var cr_id = c.GetValues("cart_id");
                var pr_id = c.GetValues("product_id");
                var pri = c.GetValues("price");
                var c_qty = c.GetValues("qty");
                
                for (i = 0; i < cr_id.Count(); i++)
                {               
                    TBL_ORDER_DETAIL bs = new TBL_ORDER_DETAIL()
                    {
                        order_detail_id = cr_id[i],
                        product_id = pr_id[i],
                        order_id = ViewBag.gen_id,
                        sale_date = DateTime.Now,
                        order_price = Convert.ToInt32(pri[i]),
                        qty = Convert.ToInt32(c_qty[i]),
                        order_bill = Convert.ToInt32(pri[i]) * Convert.ToInt32(c_qty[i])
                    };

                    TBL_CART tbl_cart = db.TBL_CART.Find(cr_id[i]);

                    db.TBL_ORDER_DETAIL.Add(bs);
                    db.TBL_CART.Remove(tbl_cart);                  
                }

                db.Entry(dbGen).State = EntityState.Modified;
                db.TBL_ORDER.Add(tbl_order);
                db.SaveChanges();

                return RedirectToAction("Success", "Order");
            }
            return View();
        }

        [ActionName("CartDelete")]
        public ActionResult DeleteConfirmed(string cr_id, string ss_id,string p_id, int? amount, int? qty)
        {
            DB_JANESHOPEntities1 dbP = new DB_JANESHOPEntities1();
            DB_JANESHOPEntities1 dbC = new DB_JANESHOPEntities1();

            List<TBL_PRODUCT> tbl_products = dbP.TBL_PRODUCT.ToList();
            var checkpro = tbl_products.Where(x => x.product_id == p_id).ToList();

            foreach (var itemp in checkpro)
            {
                amount = (int?)itemp.amount;
            }

            List<TBL_CART> tbl_carts = dbC.TBL_CART.ToList();
            var checkcart = tbl_carts.Where(x => x.cart_id == cr_id).ToList();

            foreach (var item in checkcart)
            {
                qty = (int?)item.qty;
            }

            TBL_PRODUCT tbl_product = new TBL_PRODUCT { product_id = p_id };
            db.TBL_PRODUCT.Attach(tbl_product);
            tbl_product.amount = Convert.ToInt32(amount) + Convert.ToInt32(qty);

            try
            {
                TBL_CART tbl_cart = db.TBL_CART.Find(cr_id);
                db.TBL_CART.Remove(tbl_cart);
                db.SaveChanges();

                return RedirectToAction("CheckCart", "Order",new { s_id = ss_id});
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Customer");
            }
        }
        public ActionResult Success()
        {
            TempData.Keep();
            return View();
        }
    }
}