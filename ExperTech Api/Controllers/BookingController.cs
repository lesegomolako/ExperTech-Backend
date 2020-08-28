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
                .Include(zz => zz.BookingNotes).Include(hh => hh.BookingLines).Include(zz => zz.DateRequesteds).Where(zz => zz.ClientID == ClientID).ToList();
            //Debug.Write("Bookings", booking.ToString());
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

                List<dynamic> notesList = new List<dynamic>();
                foreach (BookingNote notes in bookings.BookingNotes)
                {
                    dynamic notesObject = new ExpandoObject();
                    notesObject.Notes = notes.Note;

                    notesList.Add(notesObject);

                }
                if (notesList.Count != 0)
                    obForBooking.Notes = notesList;

                List<dynamic> EmpSchedule = new List<dynamic>();
                foreach (EmployeeSchedule schedule in bookings.EmployeeSchedules)
                {
                    dynamic SchedgeObject = new ExpandoObject();
                    SchedgeObject.Date = db.Dates.Where(zz => zz.DateID == schedule.DateID).Select(zz => zz.Date1).FirstOrDefault();
                    SchedgeObject.StartTime = db.Timeslots.Where(zz => zz.TimeID == schedule.TimeID).Select(zz => zz.StartTime).FirstOrDefault();
                    SchedgeObject.EndTime = db.Timeslots.Where(zz => zz.TimeID == schedule.TimeID).Select(zz => zz.EndTime).FirstOrDefault();
                    SchedgeObject.Employee = db.Employees.Where(zz => zz.EmployeeID == schedule.EmployeeID).Select(zz => zz.Name).FirstOrDefault();

                    EmpSchedule.Add(SchedgeObject);
                }
                if (EmpSchedule.Count != 0)
                    obForBooking.EmployeeSchedule = EmpSchedule;

                List<dynamic> DateRequested = new List<dynamic>();
                foreach (DateRequested requested in bookings.DateRequesteds)
                {
                    dynamic requestedDate = new ExpandoObject();
                    requestedDate.Date = db.DateRequesteds.Where(zz => zz.Date == requested.Date).Select(zz => zz.Date).FirstOrDefault();
                    requestedDate.StartTime = db.DateRequesteds.Where(zz => zz.StartTime == requested.StartTime).Select(zz => zz.StartTime).FirstOrDefault();



                    DateRequested.Add(requestedDate);
                }
                if (DateRequested.Count != 0)
                    obForBooking.DateRequested = DateRequested;

                List<dynamic> BookingLine = new List<dynamic>();
                foreach (BookingLine line in bookings.BookingLines)
                {
                    dynamic LineObject = new ExpandoObject();
                    LineObject.Service = db.Services.Where(zz => zz.ServiceID == line.ServiceID).Select(zz => zz.Name).FirstOrDefault();

                    LineObject.Option = db.ServiceOptions.Where(zz => zz.OptionID == line.OptionID).Select(zz => zz.Name).FirstOrDefault();


                    BookingLine.Add(LineObject);

                }
                if (BookingLine.Count != 0)
                    obForBooking.BookingLines = BookingLine;


                dymanicBookings.Add(obForBooking);
            }
            return dymanicBookings;
        }

        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Booking/ConfirmClientBookings")]
        public IHttpActionResult ConfirmClientBooking(int BookingID)
        {

            db.Configuration.ProxyCreationEnabled = false;

            Booking bookings = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
            bookings.StatusID = 2;

            db.SaveChanges();

            return Ok(bookings);
        }




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
                foreach (DateRequested items in Modell.DateRequesteds)
                {
                    items.BookingID = BookingID;

                    db.DateRequesteds.Add(items);

                    //findSchedule.BookingID = BookingID;
                    //findSchedule.StatusID = 2;
                    db.SaveChanges();
                }
                //if(Modell.Client.Sales.)
                foreach (BookingLine items in Modell.BookingLines)
                {
                    BookingLine line = new BookingLine();
                    line.BookingID = BookingID;
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
