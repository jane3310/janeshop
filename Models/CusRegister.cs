using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Janeshop.Models
{
    public class CusRegister
    {
        public string customer_id { get; set; }
        [Required(ErrorMessage = "กรุณาระบุ เลือกคำนำหน้า")]
        public string title_id { get; set; }
        [Required(ErrorMessage = "กรุณาระบุ ชื่อ")]
        public string fname { get; set; }
        [Required(ErrorMessage = "กรุณาระบุ สกุล")]
        public string lname { get; set; }
        [Required(ErrorMessage = "กรุณาระบุ เบอร์โทรศัพท์")]
        [MinLength(10, ErrorMessage = "กรุณาระบุ เบอร์โทรไม่น้อยกว่า 10 ตัว")]
        [MaxLength(10, ErrorMessage = "กรุณาระบุ เบอร์โทรไม่เกิน 10 ตัว")]
        public string mobile { get; set; }
        [Required(ErrorMessage = "กรุณาระบุ รหัสผ่าน")]
        [MinLength(4, ErrorMessage = "กรุณาระบุ รหัสผ่านไม่น้อยกว่า 4 ตัว")]
        [MaxLength(10, ErrorMessage = "กรุณาระบุ รหัสผ่านไม่เกิน 10 ตัว")]
        public string password { get; set; }
        public bool cus_status { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string LoginErrorMessage { get; internal set; }
        [NotMapped]
        [Required(ErrorMessage = "กรุณาระบุ รหัสผ่าน")]
        [CompareAttribute("password", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
        public string Confirmpassword { get; set; }
    }
}