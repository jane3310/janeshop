using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Janeshop.Models
{
    public class Cart
    {
        public string product_id { get; set; }
        public string product_name { get; set; }
        public Nullable<double> price { get; set; }
        public Nullable<int> qty { get; set; }
        public Nullable<double> order_bill { get; set; }
        public string store_id { get; set; }
        public string store_name { get; set; }
        //-------------------------------
        public TBL_PRODUCT tbl_product { get; set; }
        //-------------------------------
        public Nullable<double> store_bill { get; set; }
        public string status { get; set; }
        public Nullable<double> order_price_sab { get; set; }
        public Nullable<double> order_bill_sab { get; set; }
    }
}