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
using System.Data.Entity;

namespace ExperTech_Api.Controllers
{
    public class AdminController : ApiController
    {
        public ExperTechEntities db = new ExperTechEntities();

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/Admin/getAdmin")]
        [System.Web.Mvc.HttpGet]

        //********************************read admin*****************************************
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
            if (!ModelState.IsValid)
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
        //*******************************final delete client*****************************
        //[Route("api/Client/clientDelete")]
        //[HttpPut]
        //public object clientDelete(int clients)
        //{
        //    try
        //    {
        //        User usrOBJ = db.Users.Where(rr => rr.UserID == clients).FirstOrDefault();
        //        db.Users.Remove(usrOBJ);
        //        db.SaveChanges();

        //        Client findclient = db.Clients.Where(zz => zz.ClientID == clients).FirstOrDefault();
        //        findclient.Deleted = true;
        //        db.SaveChanges();
        //        return "success";
        //    }
        //    catch (Exception err)
        //    {
        //        return"failed";
        //    }
        //}
        //****************************final admin delete******************************
        //[Route("api/Admin/adminDelete")]
        //[HttpPut]
        //public object employeeDelete(int admins)
        //{
        //    try
        //    {
        //        User usrOBJ = db.Users.Where(rr => rr.UserID == admins).FirstOrDefault();
        //        db.Users.Remove(usrOBJ);
        //        db.SaveChanges();

        //        Admin findemployee = db.Admins.Where(zz => zz.AdminID == admins).FirstOrDefault();
        //        findemployee.Deleted = true;
        //        db.SaveChanges();
        //        return "success";
        //    }
        //    catch (Exception err)
        //    {
        //        return "failed";
        //    }
        //}
        //******************************delete service package*****************************
        //[Route("api/ServicePackage/DeleteServicePackage")]
        //[HttpDelete]
        //public dynamic DeleteServicePackage(int PackageID)
        //{
        //    try
        //    {
        //        ServicePackage spobject = db.ServicePackages.Where(rr => rr.PackageID == PackageID).FirstOrDefault();
        //        db.ServicePackages.Remove(spobject);
        //        db.SaveChanges();
        //        return "success";
        //    }
        //    catch
        //    {
        //        return "failed";
        //    }
        //}
        //**************************Read Company information*****************************
        [Route("api/Admin/getCompany")]
        [HttpGet]

        public List<dynamic> getCompany()
        {
            db.Configuration.ProxyCreationEnabled = false;

            return getCompanyID(db.CompanyInfoes.ToList());
        }
        private List<dynamic> getCompanyID(List<CompanyInfo> forCompany)
        {
            List<dynamic> dynamicCompanies = new List<dynamic>();
            foreach (CompanyInfo cname in forCompany)
            {
                dynamic dynamicCompany = new ExpandoObject();
                dynamicCompany.InfoID = cname.InfoID;
                dynamicCompany.Name = cname.Name;
                dynamicCompany.Address = cname.Address;
                dynamicCompany.ContactNo = cname.ContactNo;

                dynamicCompanies.Add(dynamicCompany);
            }
            return dynamicCompanies;
        }
        //******************************update company info**************************
        [Route("api/Admin/updateCompany")]
        [HttpPut]
        public dynamic updateCompany([FromBody] CompanyInfo forCompany)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                CompanyInfo information = db.CompanyInfoes.Find(forCompany.InfoID);
                if (information != null)
                {
                    information.Name = forCompany.Name;
                    information.Address = forCompany.Address;
                    information.ContactNo = forCompany.ContactNo;
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
            return Ok(forCompany);
        }
        //**********************delete company info*********************************
        [Route("api/Admin/deleteCompany")]
        [HttpDelete]
        public object deleteCompany([FromBody] CompanyInfo forCompany)
        {
            try
            {
                if (forCompany != null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    CompanyInfo companyRThings = db.CompanyInfoes.Where(zz => zz.InfoID == forCompany.InfoID).FirstOrDefault();

                    db.CompanyInfoes.Remove(companyRThings);
                    db.SaveChanges();
                    return "success";
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }
        //*******************************add company********************************
        [Route("api/Admin/addCompany")]
        [HttpPost]
        public List<dynamic> addCompany([FromBody] List<CompanyInfo> forCompany)
        {
            try
            {
                if (forCompany != null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.CompanyInfoes.AddRange(forCompany);
                    db.SaveChanges();

                    return getCompany();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        [Route("api/Admin/GetBookings")]
        [HttpGet]
        public dynamic GetBookings()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> getList = db.Bookings.Where(zz => zz.StatusID == 4).ToList();
            return getBookingsList(getList);

        }

       
       

        private dynamic getBookingsList(List<Booking> Modell)
        {
            List<dynamic> getList = new List<dynamic>();
            foreach (Booking items in Modell)
            {
                string name = db.Clients.Where(zz => zz.ClientID == items.ClientID).Select(zz => zz.Name).FirstOrDefault();
                string surname = db.Clients.Where(zz => zz.ClientID == items.ClientID).Select(zz => zz.Surname).FirstOrDefault();
                string status = db.BookingStatus.Where(zz => zz.StatusID == items.StatusID).Select(zz => zz.Status).FirstOrDefault();
                int DateID = db.EmployeeSchedules.Where(zz => zz.BookingID == items.BookingID).Select(zz => zz.DateID).FirstOrDefault();
                int ServiceID = db.BookingLines.Where(zz => zz.BookingID == items.BookingID).Select(zz => zz.ServiceID).FirstOrDefault();
                int? OptionID = db.BookingLines.Where(zz => zz.BookingID == items.BookingID).Select(zz => zz.OptionID).FirstOrDefault();

                dynamic listObject = new ExpandoObject();
                listObject.BookingID = items.BookingID;
                listObject.ClientID = items.ClientID;
                listObject.Client = name + " " + surname;              
                listObject.Status = status;              
                listObject.Date = db.Dates.Where(zz => zz.DateID == DateID).Select(zz => zz.Date1).FirstOrDefault();
                listObject.ServiceID = ServiceID;
                
                if(OptionID != null)
                {
                    string ServiceName = db.Services.Where(zz => zz.ServiceID == ServiceID).Select(zz => zz.Name).FirstOrDefault();
                    string OptionName = db.ServiceOptions.Where(zz => zz.OptionID == OptionID).Select(zz => zz.Name).FirstOrDefault();
                    
                    listObject.Service = ServiceName + "(" + OptionName + ")";
                    listObject.Price = db.ServicePrices.Where(zz => zz.ServiceID == ServiceID && zz.OptionID == OptionID).Select(zz => zz.Price).FirstOrDefault();
                }
                else
                {
                    string ServiceName = db.Services.Where(zz => zz.ServiceID == ServiceID).Select(zz => zz.Name).FirstOrDefault();
                    listObject.Service = ServiceName;
                    listObject.Price = db.ServicePrices.Where(zz => zz.ServiceID == ServiceID).Select(zz => zz.Price).FirstOrDefault();
                }
                getList.Add(listObject);

                List<Sale> findSales = db.Sales.Where(zz => zz.ClientID == items.ClientID && zz.SaleTypeID == 3).ToList();
                int PackageID = db.ServicePackages.Where(zz => zz.ServiceID == ServiceID).Select(zz => zz.PackageID).FirstOrDefault();

                bool hasPackage = false;
                foreach (Sale saleitems in findSales)
                {
                    DateTime today = DateTime.Now;
                    ClientPackage findPackage = db.ClientPackages.Include(zz => zz.ServicePackage).Include(zz => zz.PackageInstances).Where(zz => zz.SaleID == saleitems.SaleID &&
                    zz.PackageID == PackageID && zz.ExpiryDate > today.Date).FirstOrDefault();

                   
                    if (findPackage != null)
                    {
                        hasPackage = true;
                        dynamic PackageDetails = new ExpandoObject();
                        PackageDetails.PackageID = findPackage.PackageID;
                        string PName = findPackage.ServicePackage.Description;
                        int InstancesLeft = findPackage.PackageInstances.Where(zz => zz.StatusID == 1).Count();
                        PackageDetails.Name = PName + "(" + InstancesLeft.ToString() + " uses left)";
                        listObject.PackageDetails = PackageDetails;
                        break;
                    }
                  
                }

                listObject.HasPackage = hasPackage;

            }
            return getList;
        }

        [Route("api/Admin/GetAllTimes")]
        [HttpGet]
        public dynamic GetAllTimes()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return db.Timeslots.ToList();
        }

        [Route("api/Admin/GetCompanyInfo")]
        [HttpGet]
        public dynamic GetCompanyInfo()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return db.CompanyInfoes.Where(zz => zz.InfoID == 1).FirstOrDefault();
        }

        [Route("api/Admin/GetSocials")]
        [HttpGet]
        public dynamic GetSocials()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return db.SocialMedias.ToList();
        }

        [Route("api/Admin/updateTimes")]
        [HttpPost]
        public dynamic updateTimes([FromBody]List<Timeslot>Modell)
        {
            if(Modell.Count != 0)
            {
                try
                {
                    for (int j = 0; j < Modell.Count; j++)
                    {
                        Timeslot findSlot = db.Timeslots.Where(zz => zz.TimeID == Modell[j].TimeID).FirstOrDefault();
                        if (findSlot != null)
                        {
                            findSlot.Available = Modell[j].Available;
                            db.SaveChanges();
                        }

                        List<EmployeeSchedule> findSchedule = db.EmployeeSchedules.Where(zz => zz.TimeID == Modell[j].TimeID).ToList();
                        for(int k=0; k <findSchedule.Count; k++)
                        {
                            EmployeeSchedule getSchedge = findSchedule[k];
                            if (Modell[j].Available == false)
                            {
                                if (getSchedge.StatusID == 1)
                                {
                                    getSchedge.StatusID = 2;
                                    db.SaveChanges();
                                }

                            }
               
                        }

                    }

                    return "success";
                }
                catch(Exception err)
                {
                    return err.Message;
                }
               


            }
            else
            {
                return "Timeslot not found";
            }
           
        }


        //      BookingID: null,
        //  Client : null,
        //  Status: null,
        //  BookingLines:
        //    [{
        //      ServiceID: null,
        //      OptionID: null,
        //      Service:null,
        //      Option:null,
        //    }],
        //    EmployeeSchedule: [
        //      {
        //          Date: null,
        //          StartTime: null,
        //          EndTime: null,
        //          Employee: null,
        //      }
        //  ],
        //  DateRequesteds:
        //    [{
        //      Date: null,
        //      StartTime: null,
        //    }],


        //  BookingNotes:  
        //    [{
        //      Notes: null,
        //    }]

        //}

        //**************************************read socials*************************************
        //[Route("api/CompanyInfo/getSocials")]
        //[HttpGet]
        //public List<dynamic> getSocials()
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    return getsocialMediaID(db.SocialMedias.ToList());
        //}

        //private List<dynamic> getsocialMediaID(List<SocialMedia> forSM)
        //{
        //    List<dynamic> dynamicSMs = new List<dynamic>();
        //    foreach (SocialMedia smname in forSM)
        //    {
        //        dynamic dynamicSM = new ExpandoObject();
        //        dynamicSM.SocialID = smname.SocialID;
        //        dynamicSM.Name = smname.Name;
        //        dynamicSM.Link = smname.Link;

        //        dynamicSMs.Add(dynamicSM);
        //    }
        //    return getSocials();
        //}

        //************************************update socials******************************
        //[Route("api/CompanyInfo/updateSocials")]
        //[HttpPut]
        //public IHttpActionResult updateSocials([FromBody] SocialMedia forSM)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    try
        //    {
        //        SocialMedia sm = db.SocialMedias.Find(forSM.SocialID);

        //        if (sm != null)
        //        {
        //            sm.Name = forSM.Name;
        //            sm.Link = forSM.Link;
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return Ok(forSM);
        //}

        //***********************************delete socials**********************************
        //[Route("api/CompanyInfo/deleteSocials")]
        //[HttpDelete]

        //public object deleteSocials([FromBody] SocialMedia forSM)
        //{
        //    try
        //    {
        //        if (forSM != null)
        //        {
        //            db.Configuration.ProxyCreationEnabled = false;
        //            SocialMedia sm = db.SocialMedias.Where(rr => rr.SocialID == forSM.SocialID).FirstOrDefault();
        //            db.SocialMedias.Remove(sm);
        //            db.SaveChanges();
        //            return "success";
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        //*************************add socials********************************
        //[Route("api/Admins/addSocials")]
        //[HttpPost]
        //public object addSocials([FromBody] SocialMedia forSM)
        //{
        //    try
        //    {
        //        if (forSM != null)
        //        {
        //            db.Configuration.ProxyCreationEnabled = false;
        //            db.SocialMedias.Add(forSM);
        //            db.SaveChanges();

        //            return "success";
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception err) 
        //    {
        //        return err.Message;
        //    }
        //}
    }
 }
