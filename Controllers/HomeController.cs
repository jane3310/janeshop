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
    public class HomeController : Controller
    {
        private DB_JANESHOPEntities1 db = new DB_JANESHOPEntities1();
        public ActionResult Index(int? i, string search, string type_id,string district_id)
        {
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_DISTRICT> tbl_district = db.TBL_DISTRICT.ToList();

            ViewBag.type = new SelectList(tbl_product_type, "type_id", "type_name");
            ViewBag.District = new SelectList(tbl_district, "district_id", "district_name");

        //    string s_id = (string)Session["Id"];

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          join o in tbl_owner on p.store_id equals o.store_id into tbl3
                          from o in tbl3.DefaultIfEmpty(new TBL_OWNER())
                          join d in tbl_district on o.district_id equals d.district_id into tbl4
                          from d in tbl4.DefaultIfEmpty(new TBL_DISTRICT())
                          select new Product { tbl_product_type = t,tbl_district = d,tbl_owner = o, tbl_product = p, tbl_product_picture = pt };

            if (search != null && type_id != null && district_id != null)
            {
                return View(proView.Where(x => x.tbl_product_type.type_id == type_id && x.tbl_district.district_id == district_id && x.tbl_product.product_name.StartsWith(search) || search == null).ToPagedList(i ?? 1, 12));
            }
            else if (search != null && type_id != null && district_id == null)
            {
                return View(proView.Where(x => x.tbl_product_type.type_id == type_id && x.tbl_product.product_name.StartsWith(search) || search == null).ToPagedList(i ?? 1, 12));
            }
            else if (search != null && type_id == null && district_id != null)
            {
                return View(proView.Where(x => x.tbl_district.district_id == district_id && x.tbl_product.product_name.StartsWith(search) || search == null).ToPagedList(i ?? 1, 12));
            }
            else if (search == null && type_id != null && district_id != null)
            {
                return View(proView.Where(x => x.tbl_product_type.type_id == type_id && x.tbl_district.district_id == district_id ).ToPagedList(i ?? 1, 12));
            }
            else
            {
                return View(proView.ToPagedList(i ?? 1, 12));
            }
        }
        public ActionResult BestSeller()
        {
            DateTime date = DateTime.Today;
    
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.Where(x=>x.sale > 0).ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();
            List<TBL_ORDER_DETAIL> tbl_order_detail = db.TBL_ORDER_DETAIL.Where(x=>x.sale_date.Value.Year==date.Year && x.sale_date.Value.Month == date.Month).ToList();

            ViewBag.type = new SelectList(tbl_product_type, "type_id", "type_name");

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          join od in tbl_order_detail on p.product_id equals od.product_id into tbl3
                          from od in tbl3.DefaultIfEmpty(new TBL_ORDER_DETAIL())
                          join o in tbl_owner on p.store_id equals o.store_id into tbl4
                          from o in tbl4.DefaultIfEmpty(new TBL_OWNER())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt, tbl_order_detail = od, tbl_owner = o };

            return View(proView.GroupBy(x => x.tbl_order_detail.product_id).Select(y => y.FirstOrDefault()).ToList());
        }
        public ActionResult Raregoods(int? i, string search, string type_id)
        {
            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.Where(x=>x.status == 1).ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();
            List<TBL_PRODUCT_PICTURE> tbl_product_picture = db.TBL_PRODUCT_PICTURE.ToList();
            List<TBL_OWNER> tbl_owner = db.TBL_OWNER.ToList();

            ViewBag.type = new SelectList(tbl_product_type, "type_id", "type_name");

            var proView = from p in tbl_product
                          join t in tbl_product_type on p.type_id equals t.type_id into tbl1
                          from t in tbl1.DefaultIfEmpty(new TBL_PRODUCT_TYPE())
                          join pt in tbl_product_picture on p.product_id equals pt.product_id into tbl2
                          from pt in tbl2.DefaultIfEmpty(new TBL_PRODUCT_PICTURE())
                          join o in tbl_owner on p.store_id equals o.store_id into tbl3
                          from o in tbl3.DefaultIfEmpty(new TBL_OWNER())
                          select new Product { tbl_product_type = t, tbl_product = p, tbl_product_picture = pt, tbl_owner = o };

            return View(proView.ToPagedList(i ?? 1, 20));
        }
        public ActionResult ProductShowDetail(string p_id)
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

            var pView = proView.Where(x => x.tbl_product.product_id == p_id).ToList();

            return View(pView);
        }
        public ActionResult ProductDetail(string p_id)
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

            var pView = proView.Where(x => x.tbl_product.product_id == p_id).ToList();

            return View(pView);
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult GetData()
        {
            int male = db.TBL_PRODUCT.Where(x => x.sale == 0).Count();
            int female = db.TBL_ORDER_DETAIL.Where(x => x.product_id == "SHOP202110000000023").Count();
            int other = db.TBL_PRODUCT.Where(x => x.product_id == "SHOP202110000000023").Count();
            Ratio obj = new Ratio();
            obj.Male = male;
            obj.Female = female;
            obj.Other = other;

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public class Ratio
        {
            public int Male { get; set; }
            public int Female { get; set; }
            public int Other { get; set; }
        }
        public ActionResult About()
        {
            List<DataPoint> dataPoints = new List<DataPoint>();

            List<TBL_PRODUCT> tbl_product = db.TBL_PRODUCT.ToList();
            List<TBL_PRODUCT_TYPE> tbl_product_type = db.TBL_PRODUCT_TYPE.ToList();

            foreach (var item in tbl_product)
            {
                dataPoints.Add(new DataPoint(item.product_name, (int)item.sale));
            }
/*
            dataPoints.Add(new DataPoint("NXP", 14));
            dataPoints.Add(new DataPoint("Infineon", 10));
            dataPoints.Add(new DataPoint("Renesas", 9));
            dataPoints.Add(new DataPoint("STMicroelectronics", 8));
            dataPoints.Add(new DataPoint("Texas Instruments", 7));
            dataPoints.Add(new DataPoint("Bosch", 5));
            dataPoints.Add(new DataPoint("ON Semiconductor", 4));
            dataPoints.Add(new DataPoint("Toshiba", 3));
            dataPoints.Add(new DataPoint("Micron", 3));
            dataPoints.Add(new DataPoint("Osram", 2));
            dataPoints.Add(new DataPoint("Others", 35));
*/
            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

            return View();
        }
    }
}