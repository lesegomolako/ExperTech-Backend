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
using System.Dynamic;

namespace ExperTech_Api.Controllers
{

    public class EmployeeController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Employee/getEmployee")]
        [System.Web.Mvc.HttpGet]
        //**********************************************************read employee**********************************************************
        public List<dynamic> getEmployee()
        {
            ExperTechEntities db = new ExperTechEntities();
            db.Configuration.ProxyCreationEnabled = false;
            return getEmployeeID(db.Employees.ToList());
        }
        private List<dynamic> getEmployeeID(List<Admin> forEmployee)
        {
            List<dynamic> dynamicEmployees = new List<dynamic>();
            foreach (Admin employeename in forEmployee)
            {
                dynamic dynamicEmployee = new ExpandoObject();
                dynamicEmployee.EmployeeID = employeename.EmployeeID;
                dynamicEmployee.Name = employeename.Name;
                dynamicEmployee.Surname = employeename.Surname;
                dynamicEmployee.ContactNo = employeename.ContactNo;
                dynamicEmployee.Email = employeename.Email;

                dynamicEmployees.Add(dynamicEmployee);
            }
            return dynamicEmployees;
        }
        //****************************************************************Read client***********************************************
        [Route("api/Client/getClient")]
        [HttpGet]
        public List<dynamic> getClient()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getClientID(db.Clients.ToList());
        }
        private List<dynamic> getClientID(List<Client> forClient)
        {
            List<dynamic> dynamicClients = new List<dynamic>();
            foreach (Client clientname in forClient)
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

        //**************************************************update employee**************************************
        [Route("api/Employee/updateEmployee")]
        [HttpPost]
        public IHttpActionResult PutUserMaster([FromBody] dynamic forEmployee)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Client emplo = db.Employees.Find(forEmployee.Employee.EmployeeID);

                if (emplo != null)
                {
                    emplo.Name = forEmployee.Name;
                    emplo.Surname = forEmployee.Surname;
                    emplo.Email = forEmployee.Email;
                    emplo.ContactNo = forEmployee.ContactNo;
                    emplo.UserID = forEmployee.UserID;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forEmployee);
        }
        //*********************************Delete employee****************************
        [Route("api/Employee/deleteEmployee")]
        [HttpDelete]
        public List<dynamic> deleteEmployee([FromBody] Admin forEmployee)
        {
            if (forEmployee != null)
            {
                ExperTechEntities db = new ExperTechEntities();
                db.Configuration.ProxyCreationEnabled = false;

                Admin employeeThings = db.Employees.Where(rr => rr.EmployeeID == forEmployee.EmployeeID).FirstOrDefault();
                User userThings = db.Users.Where(rr => rr.UserID == forEmployee.UserID).FirstOrDefault();

                db.Employees.Remove(employeeThings);
                db.Users.Remove(userThings);
                db.SaveChanges();

                return getEmployee();
            }
            else
            {
                return null;
            }
        }
        //*********************************employee availability****************************
        [Route("api/Employee/getSchedule")]
        [System.Web.Mvc.HttpGet]
        public dynamic getSchedule()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Schedule> findSchedule = db.Schedules.Include(zz => zz.Timeslot).Include(zz => zz.Date).ToList();
            return DisplayList(findSchedule);
        }
        private List<dynamic> DisplayList(List<Schedule> Modell)
        {
            List<dynamic> TimesList = new List<dynamic>();
            foreach (Schedule Items in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.Dates = Items.Date.Date1;
                newObject.StartTimes = Items.Timeslot.StartTime;
                newObject.EndTimes = Items.Timeslot.EndTime;
                TimesList.Add(newObject);
            }
            return TimesList;
        }

        //********************************************************************************read employee type****************************************************************************************

        [Route("api/Employee/getEmployeeType")]
        [HttpGet]
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

        //********************************************************************************update employee type****************************************************************************************
        [Route("api/Employee/updateEST")]
        [HttpPut]
        public object updateEST([FromBody] ServiceType forEST)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                ServiceType serviceType = db.ServiceTypes.Find(forEST.TypeID);   //lol

                if (serviceType != null)
                {
                    serviceType.TypeID = forEST.TypeID;
                    serviceType.Name = forEST.Name;
                    serviceType.Description = forEST.Description;

                    db.SaveChanges();

                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forEST);

        }

        //***********************************************final availability******************************************************
        [Route("api/Employee/EmployeeAvailability")]
        [HttpPost]
        public dynamic EmployeeAvailability(dynamic Stuff)
        {
            //var httpRequest = HttpContext.Current.Request;

            try
            {
                DateTime StartDate = Convert.ToDateTime(Stuff.StartDate);
                DateTime EndDate = Convert.ToDateTime(Stuff.EndDate);

                int Avail = Convert.ToInt32(Stuff.Availabilness);

                TimeSpan StartTime = Convert.ToDateTime(Stuff.StartTime);
                TimeSpan EndTime = Convert.ToDateTime(Stuff.EndTime);

                int StartDateID = db.Dates.Where(zz => zz.Date1 == StartDate).Select(zz => zz.DateID).FirstOrDefault();
                int EndDateID = db.Dates.Where(zz => zz.Date1 == EndDate).Select(zz => zz.DateID).FirstOrDefault();

                int StartTimeID = db.Timeslots.Where(zz => zz.StartTime == StartTime).Select(zz => zz.TimeID).FirstOrDefault();
                int EndTimeID = db.Timeslots.Where(zz => zz.EndTime == EndTime).Select(zz => zz.TimeID).FirstOrDefault();

                for (int j = StartDateID; j < EndDateID; j++)
                {
                    List<EmployeeSchedule> findSchedule = db.EmployeeSchedules.Where(zz => zz.DateID == j).ToList();
                    for (int k = StartTimeID; k < EndTimeID; k++)
                    {
                        if (Avail == 1)
                        {
                            if (findSchedule[k].TimeID == k)
                            {
                                findSchedule[k].StatusID = 1;
                                db.SaveChanges();
                            }
                            else
                            {
                                findSchedule[k].StatusID = 2;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            if (findSchedule[k].TimeID == k)
                            {
                                findSchedule[k].StatusID = 2;
                                db.SaveChanges();
                            }
                            else
                            {
                                findSchedule[k].StatusID = 1;
                                db.SaveChanges();
                            }
                        }
                    }
                }

                return Ok("success");
            }
            catch
            {
                throw;
            }
        }

        //*********************************employee availability****************************
        [Route("api/Employee/getTime")]
        [HttpGet]
        public dynamic getTime()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Timeslot> findTime = db.Timeslots.ToList();
            return DisplayTime(findTime);
        }

        [Route("api/Employee/getDate")]
        public List<Date> getDate()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Date> findDate = db.Dates.ToList();
            return findDate;
        }
        private List<dynamic> DisplayTime(List<Timeslot> models)
        {
            List<dynamic> time = new List<dynamic>();
            foreach(Timeslot items in models)
            {
                dynamic newobject = new ExpandoObject();
                newobject.TimeID = items.TimeID;
                newobject.StartTime = items.StartTime;
                newobject.EndTime = items.EndTime;
                time.Add(newobject);
            }
            return time;
        }


    }
}
