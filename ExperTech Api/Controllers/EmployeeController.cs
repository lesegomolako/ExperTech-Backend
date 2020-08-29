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
    public class EmployeeController : ApiController
    {
        public ExperTechEntities db = new ExperTechEntities();

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/Employees/getEmployee")]
        [System.Web.Mvc.HttpGet]

        //*************************************read emplo*************************************
        public List<dynamic> getEmployee()
        {
            ExperTechEntities db = new ExperTechEntities();
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
        //***************************************employee availability****************************************
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
        //*******************************************update employee type*********************************************
        [Route("api/Employees/updateEST")]
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
    }
}
