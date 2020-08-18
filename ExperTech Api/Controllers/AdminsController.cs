using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ExperTech_Api.Models;
using System.Dynamic;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;

namespace ExperTech_Api.Controllers
{
    public class AdminsController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Admins/getAdmin")]
        [System.Web.Mvc.HttpGet]

        public List<dynamic> getAdmin()
        {
            ExperTechEntities db = new ExperTechEntities();

            db.Configuration.ProxyCreationEnabled = false;

            return getAdminID(db.Admins.ToList());
        }
        //read admin
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

                dynamicAdmins.Add(dynamicAdmin);
            }
            return dynamicAdmins;
        }

        //insert admin
        [Route("api/Admins/addAdmin")]
        [System.Web.Mvc.HttpPost]

        public object addAdmin([FromBody] dynamic forAdmin)
        {
            ExperTechEntities db = new ExperTechEntities();

            Admin one = new Admin();            //admin object
            db.Configuration.ProxyCreationEnabled = false;
            var hash = GenerateHash(ApplySomeSalt(forAdmin.Password));

            User two = new User();              //user object
            two.Username = forAdmin.Username;
            two.Password = hash;
            two.RoleID = 2;

            Guid g = Guid.NewGuid();

            db.Users.Add(two);
            db.SaveChanges();

            one.Name = forAdmin.Name;
            one.Surname = forAdmin.Surname;
            one.ContactNo = forAdmin.ContactNo;
            one.Email = forAdmin.Email;
            one.UserID = forAdmin.UserID;
            db.Admins.Add(one);
            db.SaveChanges();

            return one;
        }

        //update admin
        [Route("api/Admins/updateAdmin")]
        [System.Web.Mvc.HttpPost]

        public IHttpActionResult PutUserMaster([FromBody] dynamic forAdmin)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Admin adminzz = db.Admins.Find(forAdmin.Admin.AdminID);

                if (adminzz != null)
                {
                    adminzz.Name = forAdmin.Name;
                    adminzz.Surname = forAdmin.Surname;
                    adminzz.Email = forAdmin.Email;
                    adminzz.ContactNo = forAdmin.ContactNo;
                    adminzz.UserID = forAdmin.UserID;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forAdmin);
        }

        //delete admin
        [Route("api/Admins/deleteAdmin")]
        [HttpDelete]
        public object deleteAdmin([FromBody] Admin forAdmin)
        {
            if (forAdmin != null)
            {
                ExperTechEntities db = new ExperTechEntities();
                db.Configuration.ProxyCreationEnabled = false;

                Admin adminThings = db.Admins.Where(rr => rr.AdminID == forAdmin.AdminID).FirstOrDefault();
                User userThings = db.Users.Where(rr => rr.UserID == forAdmin.UserID).FirstOrDefault();

                db.Admins.Remove(adminThings);
                db.Users.Remove(userThings);
                db.SaveChanges();

                return getAdmin();
            }
            else
            {
                return null;
            }
        }
        //read client
        [Route("api/Admins//getClient")]
        [HttpGet]
        public List<dynamic> getClient()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getClientID(db.Clients.ToList());
        }
        private List<dynamic> getClientID(List<Client> forClient)
        {
            List<dynamic> dynamicClients = new List<dynamic>();
            foreach(Client clientname in forClient)
            {
                dynamic dynamicClient = new ExpandoObject();
                dynamicClient.ClientID = clientname.ClientID;
                dynamicClient.Name = clientname.Name;
                dynamicClient.Surname = clientname.Surname;
                dynamicClient.ContactNo = clientname.ContactNo;
                dynamicClient.Email = clientname.Email;

                dynamicClients.Add(dynamicClient);
            }
            return dynamicClients;

        }

        //delete admin
        [Route("api/Admins/deleteClient")]
        [HttpDelete]
        public List<dynamic> deleteClient([FromBody] Client forClient)
        {
            if (forClient != null)
            {
                db.Configuration.ProxyCreationEnabled = false;

                Client clientThings = db.Clients.Where(rr => rr.ClientID == forClient.ClientID).FirstOrDefault();
                User userThings = db.Users.Where(rr => rr.UserID == forClient.UserID).FirstOrDefault();

                db.Clients.Remove(clientThings);
                db.Users.Remove(userThings);
                db.SaveChanges();

                return getClient();
            }
            else
            {
                return null;
            }
        }
        public static string ApplySomeSalt(string input)
        {
            return input + "plokijuhygwaesrdtfyguhmnzxnvhfjdkslaowksjdienfhvbg";
        }
        public static string GenerateHash(string inputStr)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputStr);
            byte[] hash = sha256.ComputeHash(bytes);

            return getStringFromHash(hash);
        }
        public static string getStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int k = 0; k < hash.Length; k++)
            {
                result.Append(hash[k].ToString("X2"));
            }
            return result.ToString();
        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(rr => rr[random.Next(rr.Length)]).ToArray());
        }

        //registration email
        [Route("api/Admins/sendEmail")]
        [System.Web.Mvc.HttpGet]
        public void sendEmail()
        {
            string clientEmail = "";
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("hairexhilartion@gmail.com");
                message.To.Add(new MailAddress(clientEmail));
                message.Subject = "Hair Exhiliration & Beauty";
                message.IsBodyHtml = false;
                message.Body = "you did it, lmao";
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("hairexhilartion@gmail.com", "@Exhilaration1");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [Route("api/Admins/generateUser")]
        [HttpPost]
        public static string generateUser([FromBody] User forUser)
        {
            string uname = "";

            char[] lower = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            char[] upper = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

            int low = lower.Length;
            int up = upper.Length;

            Random random = new Random();

            uname += lower[random.Next(0, low)].ToString();
            uname += lower[random.Next(0, up)].ToString();

            uname += upper[random.Next(0, low)].ToString();
            uname += upper[random.Next(0, up)].ToString();

            User usr = new User();
            usr.Username = forUser.Username;

            return uname;

        }
        [Route("api/Admins/generatePassword")]
        [HttpPost]
        public string generatePassword(int Length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rndm = new Random();
            while(0 < Length--)
            {
                res.Append(valid[rndm.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}