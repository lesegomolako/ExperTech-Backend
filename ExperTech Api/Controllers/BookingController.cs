using System;
using System.Collections.Generic;
using System.Dynamic;
using ExperTech_Api.Models;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace ExperTech_Api.Controllers
{
    public class BookingController : ApiController
    {
        ExperTechEntities1 db = new ExperTechEntities1();



        [System.Web.Http.Route("api/Booking/getALLemployees")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLemployees()
        {

            db.Configuration.ProxyCreationEnabled = false;
            return getEmployeeReturnList(db.Employees.ToList());

        }
        private List<dynamic> getEmployeeReturnList(List<Employee> Foremp)
        {
            List<dynamic> dymanicEmployees = new List<dynamic>();
            foreach (Employee emPLOYEE in Foremp)
            {
                dynamic dynamicemployee = new ExpandoObject();
                dynamicemployee.EmployeeID = emPLOYEE.EmployeeID;
                dynamicemployee.Name = emPLOYEE.Name;
                dynamicemployee.Surname = emPLOYEE.Surname;
                dynamicemployee.ContactNo = emPLOYEE.ContactNo;
                dynamicemployee.Email = emPLOYEE.Email;

                dymanicEmployees.Add(dynamicemployee);
            }
            return dymanicEmployees;
        }
        [System.Web.Http.Route("api/Booking/getALLservices")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLservices()
        {

            db.Configuration.ProxyCreationEnabled = false;
            return getServiceeReturnList(db.Services.Include(zz => zz.ServicePhotoes).ToList());

        }
        private List<dynamic> getServiceeReturnList(List<Service> Forservice)
        {
            List<dynamic> dymanicServicess = new List<dynamic>();
            foreach (Service SERVICES in Forservice)
            {
                dynamic dynamicservice = new ExpandoObject();
                dynamicservice.ServiceID = SERVICES.ServiceID;
                dynamicservice.TypeID = SERVICES.TypeID;
                dynamicservice.Name = SERVICES.Name;
                dynamicservice.Description = SERVICES.Description;
                dynamicservice.Duration = SERVICES.Duration;

                List<dynamic> Photos = new List<dynamic>();
                foreach (ServicePhoto items in SERVICES.ServicePhotoes)
                {
                    dynamic newObject = new ExpandoObject();
                    newObject.ServiceID = items.ServiceID;
                    newObject.Photo = items.Photo;

                    Photos.Add(newObject);
                }
                dynamicservice.Photo = Photos;
                dymanicServicess.Add(dynamicservice);
            }
            return dymanicServicess;
        }
        [System.Web.Http.Route("api/Booking/getALLservicestype")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLservicestype()
        {

            db.Configuration.ProxyCreationEnabled = false;
            return getServiceeTypeReturnList(db.ServiceTypes.ToList());

        }
        private List<dynamic> getServiceeTypeReturnList(List<ServiceType> Forservicetype)
        {
            List<dynamic> dymanicServicestypes = new List<dynamic>();
            foreach (ServiceType SERVICESTYPE in Forservicetype)
            {
                dynamic dynamicservicetype = new ExpandoObject();
                dynamicservicetype.TypeID = SERVICESTYPE.TypeID;
                dynamicservicetype.Name = SERVICESTYPE.Name;
                dynamicservicetype.Description = SERVICESTYPE.Description;

                dymanicServicestypes.Add(dynamicservicetype);
            }
            return dymanicServicestypes;
        }
        [System.Web.Http.Route("api/Booking/getALLservicesoption")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLservicesoption()
        {

            db.Configuration.ProxyCreationEnabled = false;
            return getServiceeOptionReturnList(db.ServiceOptions.ToList());

        }
        private List<dynamic> getServiceeOptionReturnList(List<ServiceOption> Forserviceoption)
        {
            List<dynamic> dymanicServicesoptions = new List<dynamic>();
            foreach (ServiceOption SERVICESOPTION in Forserviceoption)
            {
                dynamic dynamicserviceoption = new ExpandoObject();
                dynamicserviceoption.OptionID = SERVICESOPTION.OptionID;
                dynamicserviceoption.Name = SERVICESOPTION.Name;
                dynamicserviceoption.Duration = SERVICESOPTION.Duration;

                dymanicServicesoptions.Add(dynamicserviceoption);
            }
            return dymanicServicesoptions;
        }
        [System.Web.Http.Route("api/Booking/getALLservicespictures")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLservicespictures()
        {

            db.Configuration.ProxyCreationEnabled = false;
            return getServiceePhotosReturnList(db.ServicePhotoes.ToList());

        }
        private List<dynamic> getServiceePhotosReturnList(List<ServicePhoto> Forservicephoto)
        {
            List<dynamic> dymanicServicesphotos = new List<dynamic>();
            foreach (ServicePhoto SERVICESPHOTO in Forservicephoto)
            {
                dynamic dynamicserviceoption = new ExpandoObject();
                dynamicserviceoption.PhotoID = SERVICESPHOTO.PhotoID;
                dynamicserviceoption.ServiceID = SERVICESPHOTO.ServiceID;
                dynamicserviceoption.Photo = SERVICESPHOTO.Photo;

                dymanicServicesphotos.Add(dynamicserviceoption);
            }
            return dymanicServicesphotos;
        }

        //*********************************Refiloe's stuff****************************

        [System.Web.Http.Route("api/Booking/getSchedge")]
        [HttpGet]
        public dynamic getSchedge()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Schedule> findSchedule = db.Schedules.Include(zz => zz.Date).Include(zz => zz.Timeslot).ToList();
            return GetSchedule(findSchedule);
        }

        private dynamic GetSchedule(List<Schedule> Modell)
        {
            List<Date> Dates = db.Dates.ToList();
            List<dynamic> getList = new List<dynamic>();
            dynamic result = new ExpandoObject();

            for (int j = 0; j < Dates.Count; j++)
            {
                dynamic newObject = new ExpandoObject();
                newObject.DateID = Dates[j].DateID;

                newObject.Dates = Dates[j].Date1;
                List<dynamic> getTimes = new List<dynamic>();

                foreach (Schedule Items in Modell)
                {
                    if (Items.DateID == Dates[j].DateID)
                    {
                        dynamic TimeObject = new ExpandoObject();
                        TimeObject.TimeID = Items.TimeID;
                        TimeObject.StartTime = Items.Timeslot.StartTime;
                        TimeObject.EndTime = Items.Timeslot.EndTime;
                        getTimes.Add(TimeObject);
                    }
                }
                newObject.Times = getTimes;
                getList.Add(newObject);
            }

            return getList;
        }


        [Route("api/Booking/getTimes")]
        [HttpGet]
        public List<Timeslot> getTimes()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return db.Timeslots.ToList();
        }


        [System.Web.Http.Route("api/Booking/getClientBooking")]
        [System.Web.Mvc.HttpGet]
        public List<Booking> getClientBooking()
        {

            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> booking = db.Bookings.ToList();
            return booking;

        }
        //View booking request 
        [System.Web.Http.Route("api/Bookings/ViewClientBooking")]
        [HttpGet]
        public List<dynamic> ViewClientBooking(int ClientID)
        {

            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> booking = db.Bookings.Include(ii => ii.EmployeeSchedules).Include(ll => ll.BookingStatu)
                .Include(zz => zz.BookingNotes).Include(hh => hh.BookingLines).Where(zz => zz.ClientID == ClientID).ToList();
            Debug.Write("Bookings", booking.ToString());
            return getClientBooking(booking);

        }

        private List<dynamic> getClientBooking(List<Booking> forBooking)
        {
            List<dynamic> dymanicBookings = new List<dynamic>();
            foreach (Booking bookings in forBooking)
            {
                dynamic obForBooking = new ExpandoObject();
                obForBooking.BookingID = bookings.BookingID;
                obForBooking.Status = bookings.BookingStatu.Status;
                obForBooking.Notes = getBoookingNotes(bookings);
                obForBooking.EmployeeSchedule = getEmployeeSchedules(bookings);
                obForBooking.BookingStatus = getBookingstatus(bookings.BookingStatu);
                obForBooking.Bookingline = getBookingline(bookings);

                dymanicBookings.Add(obForBooking);
            }
            return dymanicBookings;
        }

        private dynamic getEmployeeSchedules(Booking forBooking)
        {
            List<dynamic> dynamicemployeeschedule = new List<dynamic>();
            foreach (EmployeeSchedule schedue in forBooking.EmployeeSchedules)
            {
                dynamic Schedge = new ExpandoObject();
                Schedge.date = db.Dates.Where(zz => zz.DateID == schedue.DateID).Select(zz => zz.Date1).FirstOrDefault();

                //Schedge.EmpType = getEmployee(schedue);

            }

            return dynamicemployeeschedule;
        }

        private dynamic getSchedules(Schedule Schedge)
        {
            dynamic myObject = new ExpandoObject();
            myObject.Date = Schedge.Date.Date1;
            myObject.StartTime = Schedge.Timeslot.StartTime;
            myObject.EndTime = Schedge.Timeslot.EndTime;
            return myObject;
        }

        private dynamic getEmployee(EmployeeServiceType EmpType)
        {
            dynamic Emp = new ExpandoObject();
            Emp.Employee = EmpType.Employee.Name;
            Emp.ServiceType = EmpType.ServiceType.Name;
            return Emp;
        }

        private dynamic getBookingstatus(BookingStatu stat)
        {
            dynamic status = new ExpandoObject();
            status.Employee = stat.StatusID;
            status.Status = stat.Status;
            return status;
        }
        private dynamic getBoookingNotes(Booking note)
        {
            List<dynamic> bnote = new List<dynamic>();
            foreach (BookingNote NOTE in note.BookingNotes)
            {
                dynamic notes = new ExpandoObject();
                notes.Notes = NOTE.Note;
                bnote.Add(notes);
            }

            return bnote;
        }
        private dynamic getBookingline(Booking line)
        {
            List<dynamic> ine = new List<dynamic>();
            foreach (BookingLine bookinglien in line.BookingLines)
            {
                dynamic lines = new ExpandoObject();
                Debug.Write("<= GETTING SERVICE ID", "#" + bookinglien.ServiceID.ToString() + "#");
                Debug.Write("<= GETTING LINE ID", bookinglien.LineID.ToString());
                lines.Service = db.Services.Where(zz => zz.ServiceID == bookinglien.ServiceID).Select(zz => zz.Name).FirstOrDefault();
                lines.ServiceOption = db.ServiceOptions.Where(zz => zz.OptionID == bookinglien.OptionID).Select(zz => zz.Name).FirstOrDefault();
                ine.Add(lines);
            }
            return ine;

        }

        //the one the admin does
        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Booking/ConfirmClientBooking")]
        public IHttpActionResult ConfirmClientBooking(int BookingID)
        {
            ExperTechEntities1 db = new ExperTechEntities1();
            db.Configuration.ProxyCreationEnabled = false;

            Booking bookings = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
            bookings.StatusID = 2;

            db.SaveChanges();

            return Ok(bookings);
        }


        ////advise on booking 
        //[System.Web.Mvc.HttpPut]
        //[System.Web.Http.Route("api/Booking/AdviseOnBooking")]
        //public dynamic AdviseOnBooking()
        //{
        //}


        //Request Booking
        [HttpPost]
        [Route("api/Bookings/RequestBooking")]
        public dynamic RequestBooking([FromBody]Booking booking)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                Booking newBooking = new Booking();
                newBooking.ClientID = booking.ClientID;
                newBooking.StatusID = 1;
                newBooking.ReminderID = 1;
                db.Bookings.Add(newBooking);
                db.SaveChanges();
                db.Entry(newBooking).GetDatabaseValues();

                int BookingID = newBooking.BookingID;

                return SaveBooking(booking, BookingID);

            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        private dynamic SaveBooking(Booking Modell, int BookingID)
        {
            try
            {
                foreach (EmployeeSchedule items in Modell.EmployeeSchedules)
                {
                    EmployeeSchedule findSchedule = db.EmployeeSchedules.Where(zz => zz.EmployeeID == items.EmployeeID
                                                                                && zz.TimeID == items.TimeID && zz.DateID == items.DateID).FirstOrDefault();

                    findSchedule.BookingID = BookingID;
                    findSchedule.StatusID = 2;
                    db.SaveChanges();
                }
                //if(Modell.Client.Sales.)
                foreach (BookingLine items in Modell.BookingLines)
                {
                    BookingLine line = new BookingLine();
                    line.BookingID = items.BookingID;
                    line.ServiceID = items.ServiceID;
                    if (items.OptionID != null)
                        line.OptionID = items.OptionID;

                    db.BookingLines.Add(line);
                    db.SaveChanges();
                }


                foreach (BookingNote items in Modell.BookingNotes)
                {
                    BookingNote notes = new BookingNote();
                    notes.Note = items.Note;
                    notes.BookingID = BookingID;
                }

                return "success";

            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

    }
}
