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
using System.Web;

namespace ExperTech_Api.Controllers
{
    public class EmployeeController : ApiController
    {
        public ExperTechEntities db = new ExperTechEntities();

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/Employees/getEmployee")]
        [HttpGet]

        //*****************************read employee*************************************
        public List<dynamic> getEmployee()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getEmployeeID(db.Employees.ToList());
        }

        private List<dynamic> getEmployeeID(List<Employee> forEmployee)
        {
            List<dynamic> dynamicEmployees = new List<dynamic>();
            foreach (Employee employeename in forEmployee)
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
        //*************************read employee availability details*********************
        [Route("api/Employees/getTime")]
        [HttpGet]
        public dynamic getTime()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Timeslot> findTime = db.Timeslots.ToList();
            return findTime;
        }

        [Route("api/Employee/getDate")]
        [HttpGet]
        public List<Date> getDate()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Date> findDate = db.Dates.ToList();
            return findDate;
        }

        //***********************************************view availability******************************************************
        [Route("api/Emoloyee/ViewAvailability")]
        [HttpGet]
        public List<dynamic> ViewAvailability()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return ViewAvailabilityID(db.EmployeeSchedules.ToList());
        }

        private List<dynamic> ViewAvailabilityID(List<EmployeeSchedule> forES)
        {
            List<dynamic> dynamicESs = new List<dynamic>();
            foreach(EmployeeSchedule esname in forES)
            {
                dynamic dynamicES = new ExpandoObject();
                dynamicES.TimeID = esname.TimeID;
                dynamicES.Booking = esname.Booking;
                dynamicES.BookingID = esname.BookingID;
                dynamicES.DateID = esname.DateID;
                dynamicES.Schedule = esname.Schedule;
                dynamicES.ScheduleStatu = esname.ScheduleStatu;
                dynamicES.Employee = esname.Employee;
                dynamicES.EmployeeID = esname.EmployeeID;

                dynamicESs.Add(dynamicES);
            }
            return dynamicESs;
        }
        //********************************************final availability******************************************************
        public class Availably
        {
            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }
            public int StartTimeID { get; set; }
            public int EndTimeID { get; set; }

            public int Avail { get; set; }


        }
        [Route("api/Employee/EmployeeAvailability")]
        [HttpPost]
        public dynamic EmployeeAvailability([FromBody]Availably Stuff)
        {
            //var httpRequest = HttpContext.Current.Request;

            try
            {
                DateTime StartDate = Stuff.StartDate;
                DateTime EndDate = Stuff.EndDate;


                int Avail = Stuff.Avail;

                //TimeSpan StartTime = Convert.ToDateTime(Stuff.StartTime);
                //TimeSpan EndTime = Convert.ToDateTime(Stuff.EndTime);

                int StartDateID = db.Dates.Where(zz => zz.Date1 == StartDate.Date).Select(zz => zz.DateID).FirstOrDefault();
                int EndDateID = db.Dates.Where(zz => zz.Date1 == EndDate.Date).Select(zz => zz.DateID).FirstOrDefault();

                int StartTimeID = Stuff.StartTimeID;
                int EndTimeID = Stuff.EndTimeID;

                for (int j = StartDateID; j < EndDateID; j++)
                {
                    List<EmployeeSchedule> findSchedule = db.EmployeeSchedules.Where(zz => zz.DateID == j).ToList();
                    for (int kk = StartTimeID; kk < EndTimeID; kk++)
                    {
                        for (int k = 0; k < findSchedule.Count; k++)
                        {
                            if (Avail == 1)
                            {

                                if (findSchedule[k].TimeID == kk)
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
                                if (findSchedule[k].TimeID == kk)
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
                }
                return Ok("success");
            }
            catch
            {
                throw;
            }
        }
        //*******************************************update employee type*********************************************
        [Route("api/Employees/updateEST")]
        [HttpPut]
        public object updateEST([FromBody] ServiceType forEST)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
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
    }
}
