using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Dynamic;
using ExperTech_Api.Models;
using System.Web.Http.Cors;

namespace ExperTech_Api.Controllers
{
    public class AdminsController : ApiController
    {
        public ExperTechEntities db = new ExperTechEntities();

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/Admin/getAdmin")]
        [System.Web.Mvc.HttpGet]

        //***********************************************read admin************************************************
        public List<dynamic> getAdmin()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getAdminID(db.Admins.ToList());
        }
        private List<dynamic> getAdminID(List<Admin> forAdmin)
        {
            List<dynamic> dynamicAdmins = new List<dynamic>();
            foreach (Admin adminname in forAdmin)
            {
                dynamic dynamicAdmin = new ExpandoObject();
                dynamicAdmin.AdminID = adminname.AdminID;
                dynamicAdmin.Name = adminname.Name;
                dynamicAdmin.Surname = adminname.Surname;
                dynamicAdmin.ContactNo = adminname.ContactNo;
                dynamicAdmin.Email = adminname.Email;

                dynamicAdmins.Add(dynamicAdmin);
            }
            return dynamicAdmins;
        }
        //******************************update admin*****************************************
        [Route("api/Admin/updateAdmin")]
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult updateAdmin([FromBody] Admin forAdmin)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Admin adminzz = db.Admins.Find(forAdmin.AdminID);

                if (adminzz != null)
                {
                    adminzz.Name = forAdmin.Name;
                    adminzz.Surname = forAdmin.Surname;
                    adminzz.Email = forAdmin.Email;
                    adminzz.ContactNo = forAdmin.ContactNo;
                    adminzz.UserID = forAdmin.UserID;

                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forAdmin);
        }
        //**********************************delete ADMIN*****************************************
        [Route("api/Admin/deleteAdmin")]
        [HttpDelete]
        public List<dynamic> deleteAdmin([FromBody] Admin forAdmin)
        {
            if (forAdmin != null)
            {
                ExperTechEntities db = new ExperTechEntities();
                db.Configuration.ProxyCreationEnabled = false;

               // Admin adminThings = db.Admins.Where(rr => rr.AdminID == forAdmin.AdminID).FirstOrDefault();
                User userThings = db.Users.Where(rr => rr.UserID == forAdmin.UserID).FirstOrDefault();

               // db.Admins.Remove(adminThings);
                db.Users.Remove(userThings);
                db.SaveChanges();

                return getAdmin();
            }
            else
            {
                return null;
            }
        }
    }
}
