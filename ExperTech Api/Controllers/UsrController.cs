using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Dynamic;
using ExperTech_Api.Models;
using System.Net.Mail;


namespace ExperTech_Api.Controllers
{
    public class UsrController : ApiController
    {
        public ExperTechEntities db = new ExperTechEntities();
        //**********************************************check role*******************************************************
        [Route("api/Usr/checkRole")]
        [HttpPost]
        public dynamic checkRole(dynamic seshin)
        {

            string sessions = seshin.token;
            db.Configuration.ProxyCreationEnabled = false;
            var user = db.Users.Where(rr => rr.SessionID == sessions).FirstOrDefault();

            if (user != null)
            {
                if (user.RoleID == 1) // client
                {
                    return "client";
                }
                else if (user.RoleID == 2) // admin
                {
                    return "admin";
                }
                else if (user.RoleID == 3) //employee
                {
                    return "employee";
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //dynamic toReturn = new ExpandoObject();
                //toReturn.Error = "Guid is not valid";
                return "error";
            }
        }
        //*******************************************user setup*******************************************
        [Route("api/Usr/userSetup")]
        [HttpPut]
        public IHttpActionResult userSetup([FromBody] User forsetup)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                User usr = db.Users.Find(forsetup.UserID);

                if (usr != null)
                {
                    usr.Username = forsetup.Username;
                    usr.Password = forsetup.Password;

                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forsetup);

        }
        //**************************************employee availability************************************************
        [Route("api/Usr/getTime")]
        [HttpGet]
        public dynamic getTime()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Timeslot> findTime = db.Timeslots.ToList();
            return findTime;
        }

        [Route("api/Usr/getDate")]
        [HttpGet]
        public List<Date> getDate()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Date> findDate = db.Dates.ToList();
            return findDate;
        }

        //***************************************read employee type*************************************
        [Route("api/Employee/getEmployeeType")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getEmployeeType()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getEmployeeTypeID(db.ServiceTypes.ToList());
        }
        private List<dynamic> getEmployeeTypeID(List<ServiceType> forEST)
        {
            List<dynamic> dymaminEmplType = new List<dynamic>();
            foreach (ServiceType ESTname in forEST)
            {
                dynamic dynamicEST = new ExpandoObject();
                dynamicEST.TypeID = ESTname.TypeID;
                dynamicEST.Name = ESTname.Name;
                dynamicEST.Description = ESTname.Description;

                dymaminEmplType.Add(dynamicEST);
            }
            return dymaminEmplType;
        }

        //*********************************************registration stuff************************************************
        public static void Email(string SessionID, string Email)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("hairexhilartion@gmail.com");
                message.To.Add(new MailAddress(Email));
                message.Subject = "Exhiliration Hair & Beauty Registration";
                message.IsBodyHtml = false;
                message.Body = "Click the link below to setup account:" + "\n"+  "http://localhost:4200/setup?SessionID=" + SessionID;
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

            }
        }
        //***********************************register employee and admin*************************************************
        [Route("api/Usr/RegisterEA")]
        [HttpPost]
        public dynamic RegisterEA(User Modell)
        {
            User UserObject = new User();
            UserObject.Username = Modell.Username;
            UserObject.Password = Modell.Password;
            UserObject.RoleID = Modell.RoleID;
           // Guid g = new Guid();
            Guid g = Guid.NewGuid();
            UserObject.SessionID = g.ToString();
            db.Users.Add(UserObject);
            db.SaveChanges();
            db.Entry(UserObject).GetDatabaseValues();

            int UserID = UserObject.UserID;
            string SessionID = UserObject.SessionID;

            if (Modell.RoleID == 3) //employee
            {
                foreach (Employee EmployeeData in Modell.Employees)
                {
                    EmployeeData.UserID = UserID;
                    db.Employees.Add(EmployeeData);
                    db.SaveChanges();
                    Email(SessionID, EmployeeData.Email);
                }
            }
            else if (Modell.RoleID == 2) // admin
            {
                foreach (Admin AdminData in Modell.Admins)
                {
                    AdminData.UserID = UserID;
                    db.Admins.Add(AdminData);
                    db.SaveChanges();
                    Email(SessionID, AdminData.Email);
                }
            }
            return "success";
        }

        private static string generateUser()
        {
            string uname = "";

            char[] lower = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            char[] upper = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

            int low = lower.Length;
            int up = upper.Length;

            var random = new Random();

            uname += lower[random.Next(0, low)].ToString();
            uname += lower[random.Next(0, up)].ToString();

            uname += upper[random.Next(0, low)].ToString();
            uname += upper[random.Next(0, up)].ToString();

            return uname;
        }
        //********************************************generate password*****************************************
        [Route("api/User/generatePassword")]
        [HttpPost]
        public string generatePassword(int Length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rndm = new Random();
            while (0 < Length--)
            {
                res.Append(valid[rndm.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}
