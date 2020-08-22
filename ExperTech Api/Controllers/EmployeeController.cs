using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExperTech_Api.Models;
using System.Text;
using System.Net.Mail;
using System.Dynamic;

namespace ExperTech_Api.Controllers
{

    public class EmployeeController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        [Route("api/Employee/getEmployee")]
        [System.Web.Mvc.HttpGet]
        //read employee
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
        //read client
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

        //update employee
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
                Employee emplo = db.Employees.Find(forEmployee.Employee.EmployeeID);

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
        //delete employee
        [Route("api/Employee/deleteEmployee")]
        [HttpDelete]
        public List<dynamic> deleteEmployee([FromBody] Employee forEmployee)
        {
            if (forEmployee != null)
            {
                ExperTechEntities db = new ExperTechEntities();
                db.Configuration.ProxyCreationEnabled = false;

                Employee employeeThings = db.Employees.Where(rr => rr.EmployeeID == forEmployee.EmployeeID).FirstOrDefault();
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


    }
}
