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
using System.Web.Http.Cors;
using System.Text;
using System.Dynamic;
using System.Security.Cryptography;

namespace ExperTech_Api.Controllers
{
    public class UsersController : ApiController
    {
     
      [EnableCors(origins: "*", headers: "*", methods: "*")]
      //***************************************************************************Login
        [Route("api/User/Login")]
        [HttpPost]
        public object Login(User usr)
        {
            var hash = GenerateHash(ApplySomeSalt(usr.Password));
            User user = db.Users.Where(rr => rr.Username == usr.Username && rr.Password == hash).FirstOrDefault();
            dynamic toReturn = new ExpandoObject();

            if (user != null)
            {
                Guid g = Guid.NewGuid();
                user.SessionID = g.ToString();
                db.Entry(user).State = EntityState.Modified;

                db.SaveChanges();
                toReturn.Session = g.ToString();
                return toReturn;
            }
            toReturn.Error = "Incorrect username and password combination";
            return toReturn;
        }
        //************************************************************************check user role
        [Route("api/User/checkRole")]
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
        //**********************************************************************register employee admin
        [Route("api/User/Register")]
        [HttpPost]
        private dynamic RegisterEmployee(User Modell)
        {
            User UserObject = new User();
            UserObject.Username = generateUser();
            UserObject.Password = generatePassword(15);
            UserObject.RoleID = Modell.RoleID;
            Guid g = new Guid();
            UserObject.SessionID = g.ToString();
            db.SaveChanges();

            int UserID = db.Users.Where(zz => zz.Username == Modell.Username).Select(zz => zz.UserID).FirstOrDefault();

            if (Modell.RoleID == 3) //employee
            {
                foreach (Employee EmployeeData in Modell.Employees)
                {
                    db.Employees.Add(EmployeeData);
                    db.SaveChanges();

                }
            }
            else if (Modell.RoleID == 2) // admin
            {
                foreach (Admin AdminData in Modell.Admins)
                {
                    db.Admins.Add(AdminData);
                    db.SaveChanges();

                }
            }
            return "success";
        }
        //************************************************************generate username for registration
        private static string generateUser()
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


            return uname;
        }
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
        private dynamic RegisterEmployee(User Modell)
        {
            User UserObject = new User();
            UserObject.Username = generateUser();
            UserObject.Password = generatePassword(15);
            UserObject.RoleID = Modell.RoleID;
            Guid g = new Guid();
            UserObject.SessionID = g.ToString();
            db.SaveChanges();

            int UserID = db.Users.Where(zz => zz.Username == Modell.Username).Select(zz => zz.UserID).FirstOrDefault();

            if (Modell.RoleID == 3)
            {
                foreach (Employee EmployeeData in Modell.Employees)
                {
                    db.Employees.Add(EmployeeData);
                    db.SaveChanges();

                }
            }
            else if (Modell.RoleID == 2)
            {
                foreach (Admin AdminData in Modell.Admins)
                {
                    db.Admins.Add(AdminData);
                    db.SaveChanges();

                }
            }
            return "success";
        }
        //************************************************************************forgot password code
        [Route("api/User/ForgotPassword")]
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult PutUserMaster(User usr)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var hash = GenerateHash(ApplySomeSalt(usr.Password));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                User obj = db.Users.Find(usr.UserID);

                if (obj != null)
                {
                    obj.Password = hash;
                    Guid g = Guid.NewGuid();
                }
                int kr = this.db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(usr);
        }
        //****************************user details******************************
        [Route("api/User/getUser")]
        [HttpGet]
        public List<dynamic> getUser([FromBody] User forUser)
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getUserID(db.Users.ToList());
            // return getAdminID(db.Admins.ToList());
        }
        private List<dynamic> getUserID(List<User> forUser)
        {
            List<dynamic> dynamicUsers = new List<dynamic>();
            foreach (User username in forUser)
            {
                dynamic dynamicUser = new ExpandoObject();
                dynamicUser.UserID = username.UserID;
                dynamicUser.Username = username.Username;
                dynamicUser.Password = username.Password;
                dynamicUsers.Add(dynamicUser);
            }
            return dynamicUsers;
        }
        [Route("api/User/userSetup")]
        [HttpPut]
        public IHttpActionResult userSetup([FromBody] User forsetup)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
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
        //*********************************************************make sale payment*******************************************************
        [Route("api/User/salePayment")]
        [HttpPut]
        public object salePayment([FromBody]Sale sayle)
        {
            try
            {

                Sale findSale = db.Sales.Where(zz => zz.SaleID == sayle.SaleID).FirstOrDefault();
                findSale.StatusID = 1;
                findSale.Payment = sayle.Payment;
                findSale.PaymentTypeID = sayle.PaymentTypeID;
                findSale.Description = sayle.Description;
                db.SaveChanges();
                return findSale;

            }
            catch
            {
                return "error";
            }
        }
        //**********************************************************make booking paynent***************************************************
        [Route("api/User/bookingPayment")]
        [HttpPut]
        public object bookingPayment([FromBody] Booking bkings)
        {
            try
            {
                Booking findBookings = db.Bookings.Where(zz => zz.BookingID == bkings.BookingID).FirstOrDefault();
                findBookings.BookingID = 4;

                Sale sayle = new Sale();
                //booking, client array, sale

                //Client client = new Client();
                //client.Client = bkings.Client;
                //client.ClientID.ToString() = bkings.ClientID;
                db.SaveChanges();

                return findBookings;
            }
            catch
            {
                return "error";
            }
        }
        //****************************************************************************make sale payment
        [Route("api/User/salePayment")]
        [HttpPut]
        public object salePayment([FromBody]Sale sayle)
        {
            try
            {
                Sale findSale = db.Sales.Where(zz => zz.SaleID == sayle.SaleID).FirstOrDefault();
                findSale.StatusID = 1;
                findSale.Payment = sayle.Payment;
                findSale.PaymentTypeID = sayle.PaymentTypeID;
                db.SaveChanges();
                return findSale;
            }
            catch
            {
                return "error";
            }
        }
        //*********************************************************************make booking paynent
        [Route("api/User/bookingPayment")]
        [HttpPut]
        public object bookingPayment([FromBody] Booking bkings)
        {
            try
            {
                Booking findBookings = db.Bookings.Where(zz => zz.BookingID == bkings.BookingID).FirstOrDefault();
                findBookings.BookingID = 4;
                Sale sayle = new Sale();
                //booking, client array, sale
                db.SaveChanges();
                return findBookings;
            }
            catch
            {
                return "error";
            }
        }
        //*********************************************************************SP payment
        [Route("api/ServicePackage/getservicePackage")]
        [HttpGet]
        public dynamic getservicePackage()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getServicePackageID(db.ServicePackages.ToList());
        }
        private List<dynamic> getServicePackageID(List<ServicePackage> forSP)
        {
            List<dynamic> dynamicSPs = new List<dynamic>();
            foreach (ServicePackage spname in forSP)
            {
                dynamic dynamicSP = new ExpandoObject();
                dynamicSP.ServiceID = spname.ServiceID;
                dynamicSP.Service = spname.Service;
                dynamicSP.PackageID = spname.PackageID;
                dynamicSP.Name = spname.Name;
                dynamicSP.Price = spname.Price;
                dynamicSP.Quantity = spname.Quantity;

                dynamicSPs.Add(dynamicSP);
            }
            return dynamicSPs;
        }

        //**********************************************************************adding client package
        [Route("api/ClientPackage/activeSP")]
        [HttpPut]

        public void activeSP([FromBody] ClientPackage forSP)
        {
            db.Configuration.ProxyCreationEnabled = false;

            string sp = "Activate Service Package";
            DateTime Now = DateTime.Now;

            //*****************************************************************refiloeknowsbest   CAP!!!!!
            Sale sales = new Sale();
            sales.ClientID = forSP.Sale.ClientID;
            //sales.Decription = activeSP;      this is where the sale type is specific            
            sales.Payment = forSP.Sale.Payment;
            //sales.TypeID = forSP.Sale.TypeID;
            sales.StatusID = 2;
            sales.Date = Now;
            sales.Description = sp;
            db.Sales.Add(sales);
            db.SaveChanges();

            int SaleID = db.Sales.Where(zz => zz.ClientID == forSP.Sale.ClientID && zz.Description == sp).Select(zz => zz.SaleID).LastOrDefault();

            //*****************************************************************adding to client sdjfnjvn thingy
            ClientPackage CP = new ClientPackage();
            CP.SaleID = SaleID;
            CP.PackageID = forSP.ServicePackage.PackageID;
            CP.Date = Now;
            CP.ExpiryDate = Now.AddMonths(2);
            db.ClientPackages.Add(CP);
            db.SaveChanges();
            //instance whatwhat
            int loop = forSP.ServicePackage.Quantity;
            for (int j = 0; j <= loop; j++)
            {
                PackageInstance addInstance = new PackageInstance();
                addInstance.PackageID = CP.PackageID;
                addInstance.SaleID = CP.SaleID;
                addInstance.StatusID = 1;
                db.PackageInstances.Add(addInstance);
                db.SaveChanges();
            }
        }
        //*******************************************************read payment type**********************************************************************
        [Route("api/User/getPaymentType")]
        [HttpGet]

        public List<dynamic> getPaymentType()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getPaymentTypeID(db.PaymentTypes.ToList());
        }
        private List<dynamic> getPaymentTypeID(List<PaymentType> forPT)
        {
            List<dynamic> dynamicPTs = new List<dynamic>();
            foreach (PaymentType pt in forPT)
            {
                dynamic dynamicPT = new ExpandoObject();
                dynamicPT.PaymentTypeID = pt.PaymentTypeID;
                dynamicPT.Sales = pt.Sales;
                dynamicPT.Type = pt.Type;

                dynamicPTs.Add(dynamicPT);
            }
            return dynamicPTs;
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

        private ExperTechEntities db = new ExperTechEntities();

        // GET: api/Users
        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult GetUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserID)
            {
                return BadRequest();
            }

            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Users
        [ResponseType(typeof(User))]
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.UserID }, user);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserID == id) > 0;
        }
    }
}