using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ExperTech_Api.Models;
using System.Dynamic;
using System.Web.Http.Cors;
using System.Data.Entity;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ExperTech_Api.Controllers
{
    public class BookingController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();

        public class Stuff
        {
            public int BookingID { get; set; }

            public int EmployeeID { get; set; }
            public int RequestedID { get; set; }
            public int TimeID { get; set; }

            public DateTime Date { get; set; }

        }

        [Route("api/Booking/AdviseBooking")]
        [HttpPost]
        public dynamic AdviseBooking([FromBody]Stuff Booking)
        {
            db.Configuration.ProxyCreationEnabled = false;
            dynamic toReturn = new ExpandoObject();
            try
            {
                int BookingID = Booking.BookingID;
                int EmployeeID = (int)Booking.EmployeeID;
                int RequestedID = Booking.RequestedID;
                DateTime getDate =  Convert.ToDateTime(Booking.Date);
                int TimeID = (int)Booking.TimeID;

                int DateID = db.Dates.Where(zz => zz.Date1 == getDate).Select(zz => zz.DateID).FirstOrDefault();


                EmployeeSchedule findSlot = db.EmployeeSchedules.Where(zz => zz.EmployeeID == EmployeeID && zz.DateID == DateID && zz.TimeID == TimeID).FirstOrDefault();
                findSlot.BookingID = BookingID;
                findSlot.StatusID = 3;
               

                Booking findBooking = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
                findBooking.StatusID = 2;
                findBooking.AdviseExpiry = DateTime.Now.AddDays(1);
               

                DateRequested findRequest = db.DateRequesteds.Where(zz => zz.RequestedID == RequestedID).FirstOrDefault();
                db.DateRequesteds.Remove(findRequest);
                db.SaveChanges();

                SendAdviseEmail(BookingID);

                return "success";

            }
            catch (Exception err)
            {
                return err.Message;
            }
            
        }

        private void SendAdviseEmail(int BookingID)
        {
            try
            {
                Booking findBooking = db.Bookings.Include(zz => zz.BookingLines).Include(zz => zz.Client)
                    .Include(zz => zz.EmployeeSchedules).Where(zz => zz.BookingID == BookingID).FirstOrDefault();

                var f = findBooking.BookingLines.ToArray();
                var e = findBooking.EmployeeSchedules.ToArray();

                int ServiceID = f[0].ServiceID;
                int? OptionID = f[0].OptionID;

                int EmployeeID = e[0].EmployeeID;
                int DateID = e[0].DateID;
                int TimeiD = e[0].TimeID;

                string name = findBooking.Client.Name + " " + findBooking.Client.Surname;
                string email = findBooking.Client.Email;

                string Service;

                if (OptionID != null)
                {
                    string findService = db.Services.Find(ServiceID).Name;
                    string findOption = db.ServiceOptions.Find(OptionID).Name;

                    Service = findService + " (" + findOption + ")";
                }
                else
                {
                    Service = db.Services.Find(ServiceID).Name;
                }

                string Employee = db.Employees.Find(EmployeeID).Name;
                string Date = (db.Dates.Find(DateID).Date1).ToLongDateString();
                Timeslot getTimes = db.Timeslots.Find(TimeiD);
                string Time = getTimes.StartTime.ToString() + "-" + getTimes.EndTime;




                string body = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/EmailTemplates/AdviseBooking.html"));
                body = body.Replace("#Service#", Service).Replace("#Name#", name).Replace("#Employee#", Employee)
                    .Replace("#Date#", Date).Replace("#Time#", Time);

                UserController.Email(body, email, "Booking Request");
            }
            catch(Exception err)
            {
                Console.WriteLine(err.Message);
            }

        }


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
                //dynamicemployee.Surname = emPLOYEE.Surname;
                //dynamicemployee.ContactNo = emPLOYEE.ContactNo;
                //dynamicemployee.Email = emPLOYEE.Email;
                //dynamicemployee.TypeID = db.EmployeeServiceTypes.Where(zz =>)

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

                dymanicServicestypes.Add(dynamicservicetype);
            }
            return dymanicServicestypes;
        }
        [System.Web.Http.Route("api/Booking/getALLservicesoption")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getALLservicesoption()
        {

            db.Configuration.ProxyCreationEnabled = false;
            return getServiceeOptionReturnList(db.ServiceTypeOptions.Include(zz => zz.ServiceOption).ToList());

        }
        private List<dynamic> getServiceeOptionReturnList(List<ServiceTypeOption> Forserviceoption)
        {
            List<dynamic> dymanicServicesoptions = new List<dynamic>();
            foreach (ServiceTypeOption SERVICESOPTION in Forserviceoption)
            {
                dynamic dynamicserviceoption = new ExpandoObject();
                dynamicserviceoption.ServiceID = SERVICESOPTION.ServiceID;
                dynamicserviceoption.OptionID = SERVICESOPTION.OptionID;
                dynamicserviceoption.Name = SERVICESOPTION.ServiceOption.Name;
                //dynamicserviceoption.Duration = SERVICESOPTION.Duration;

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
            return db.Timeslots.Where(zz => zz.Available == true).ToList();
        }

        private void BookingExpiry(int BookingID)
        {

            Booking findBooking = db.Bookings.Find(BookingID);
            var OldStatus = findBooking.StatusID;
            findBooking.StatusID = 5;

            EmployeeSchedule findSchedule = db.EmployeeSchedules.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
            var OldID = findBooking.BookingID;
            var OldEmpStatus = findBooking.StatusID;
            findSchedule.BookingID = null;
            findSchedule.StatusID = 1;

            //send SMS

            //AdminAuditTrail makeAudit = new AdminAuditTrail();
            //makeAudit.

            db.SaveChanges();
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
        public List<dynamic> ViewClientBooking(string SessionID)
        {
            User findUser = db.Users.Include(zz => zz.Clients).Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser != null)
            {
                int ClientID = findUser.Clients.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.ClientID).FirstOrDefault();
                db.Configuration.ProxyCreationEnabled = false;
                List<Booking> booking = db.Bookings.Include(ii => ii.EmployeeSchedules).Include(ll => ll.BookingStatu)
                    .Include(zz => zz.BookingNotes).Include(hh => hh.BookingLines).Include(zz => zz.DateRequesteds).Where(zz => zz.ClientID == ClientID).OrderByDescending(zz => zz.BookingID).ToList();
                //Debug.Write("Bookings", booking.ToString());
                return getClientBooking(booking);
            }
           else
            {
                dynamic toReturn = new ExpandoObject();
                return toReturn.Error = "Session is invalid";
            }

        }

        [System.Web.Http.Route("api/Bookings/ViewBookings")]
        [HttpGet]
        public dynamic ViewBookings(string SessionID)
        {
            User findUser = db.Users.Include(zz => zz.Clients).Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                int ClientID = findUser.Clients.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.ClientID).FirstOrDefault();
                db.Configuration.ProxyCreationEnabled = false;
                List<Booking> booking = db.Bookings.Include(ii => ii.EmployeeSchedules).Include(ll => ll.BookingStatu)
                    .Include(zz => zz.BookingNotes).Include(hh => hh.BookingLines).Include(zz => zz.DateRequesteds).Where(zz => zz.ClientID == ClientID).OrderByDescending(zz => zz.BookingID).ToList();
                //Debug.Write("Bookings", booking.ToString());
                return getClientBooking(booking);
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                return toReturn.Error = "Session is invalid";
            }

        }



        private dynamic getClientBooking(List<Booking> forBooking)
        {
            List<dynamic> dymanicBookings = new List<dynamic>();
            foreach (Booking bookings in forBooking)
            {
                DateTime today = DateTime.Now;
                if (bookings.StatusID == 2 && today > bookings.AdviseExpiry)
                {
                    BookingExpiry(bookings.BookingID);
                }

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

                    DateTime getDate = db.Dates.Where(zz => zz.DateID == schedule.DateID).Select(zz => zz.Date1).FirstOrDefault();
                    Timeslot getTimes = db.Timeslots.Where(zz => zz.TimeID == schedule.TimeID).FirstOrDefault();

                    SchedgeObject.Date = getDate;
                    SchedgeObject.StartTime = getTimes.StartTime;
                    SchedgeObject.EndTime = getTimes.EndTime;
                    SchedgeObject.Employee = db.Employees.Where(zz => zz.EmployeeID == schedule.EmployeeID).Select(zz => zz.Name).FirstOrDefault();

                    DateTime makeDT = (DateTime)getDate + (TimeSpan)getTimes.StartTime;
                    DateTime DayBefore = makeDT.Subtract(new TimeSpan(24, 0, 0));
                    

                    bool canCancel = false;
                    if(today < DayBefore )
                    {
                        canCancel = true;
                    }

                    SchedgeObject.canCancel = canCancel;

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
                    obForBooking.DateRequesteds = DateRequested;

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

        //private dynamic getEmployeeSchedules(Booking forBooking)
        //{
        //    List<dynamic> dynamicemployeeschedule = new List<dynamic>();
        //    foreach (EmployeeSchedule schedue in forBooking.EmployeeSchedules)
        //    {
        //        dynamic Schedge = new ExpandoObject();
        //        Schedge.Date = db.Dates.Where(zz => zz.DateID == schedue.DateID).Select(zz => zz.Date1).FirstOrDefault();
        //        Timeslot Times = db.Timeslots.Where(zz => zz.TimeID == schedue.TimeID).FirstOrDefault();
        //        Schedge.StartTime = Times.StartTime;
        //        Schedge.EndTime = Times.EndTime;

        //        //Schedge.EmpType = getEmployee(schedue);

        //    }

        //    return dynamicemployeeschedule;
        //}

        //private dynamic getSchedules(Schedule Schedge)
        //{
        //    dynamic myObject = new ExpandoObject();
        //    myObject.Date = Schedge.Date.Date1;
        //    myObject.StartTime = Schedge.Timeslot.StartTime;
        //    myObject.EndTime = Schedge.Timeslot.EndTime;
        //    return myObject;
        //}

        //private dynamic getEmployee(EmployeeServiceType EmpType)
        //{
        //    dynamic Emp = new ExpandoObject();
        //    Emp.Employee = EmpType.Employee.Name;
        //    Emp.ServiceType = EmpType.ServiceType.Name;
        //    return Emp;
        //}


        //private dynamic getBoookingNotes(Booking note)
        //{
        //    List<dynamic> bnote = new List<dynamic>();
        //    foreach (BookingNote NOTE in note.BookingNotes)
        //    {
        //        dynamic notes = new ExpandoObject();
        //        notes.Notes = NOTE.Note;
        //        bnote.Add(notes);
        //    }

        //    return bnote;
        //}
        //private dynamic getBookingline(Booking line)
        //{
        //    List<dynamic> ine = new List<dynamic>();
        //    foreach (BookingLine bookinglien in line.BookingLines)
        //    {
        //        dynamic lines = new ExpandoObject();
        //        Debug.Write("<= GETTING SERVICE ID", "#" + bookinglien.ServiceID.ToString() + "#");
        //        Debug.Write("<= GETTING LINE ID", bookinglien.LineID.ToString());
        //        lines.Service = db.Services.Where(zz => zz.ServiceID == bookinglien.ServiceID).Select(zz => zz.Name).FirstOrDefault();
        //        lines.ServiceOption = db.ServiceOptions.Where(zz => zz.OptionID == bookinglien.OptionID).Select(zz => zz.Name).FirstOrDefault();
        //        ine.Add(lines);
        //    }
        //    return ine;

        //}




        //[System.Web.Http.Route("api/Booking/getClientBookingdetials")]
        //[System.Web.Mvc.HttpGet]
        //public List<dynamic> getClientBookingdetials()
        //{
        //    ExperTechEntities7 db = new ExperTechEntities7();
        //    db.Configuration.ProxyCreationEnabled = false;
        //    List<Booking> clientbooking = db.Bookings.Include(zz => zz.EmployeeSchedules).Include(dd => dd.BookingLines).Include(cc => cc.BookingStatu)
        //        .Include(ee => ee.BookingNotes).Include(dd => dd.DateRequesteds).ToList();
        //    return getClientBookingsdetails(clientbooking);

        //}
        //private List<dynamic> getClientBookingsdetails(List<Booking> forBooking)
        //{
        //    List<dynamic> dymanicBookings = new List<dynamic>();
        //    foreach (Booking booking in forBooking)
        //    {
        //        dynamic obForBooking = new ExpandoObject();
        //        obForBooking.BookingID = booking.BookingID;
        //        obForBooking.ClientID = booking.ClientID;
        //        obForBooking.StatusID = booking.StatusID;
        //        obForBooking.ReminderID = booking.ReminderID;
        //        obForBooking.BookingLine = getBookingLinezs(booking.BookingLines);
        //        obForBooking.EmployeeScheduless = getEmployeeScheduless(booking);


        //        dymanicBookings.Add(obForBooking);
        //    }
        //    return dymanicBookings;
        //}

        //private dynamic getBookingLinezs(BookingLine Modell)
        //{
        //    dynamic line = new ExpandoObject();
        //    line.BookingID = Modell.BookingID;
        //    line.ServiceID = Modell.ServiceID;
        //    line.OptionID = Modell.OptionID;

        //    line.Service = db.Services.Where(xx => xx.ServiceID == Modell.ServiceID).Select(zz => zz.Name).FirstOrDefault();
        //    line.ServiceOption = db.ServiceOptions.Where(xx => xx.OptionID == Modell.OptionID).Select(zz => zz.Name).FirstOrDefault();
        //    return line;

        //}
        //private dynamic getEmployeeScheduless(EmployeeSchedule empsched)
        //{


        //    dynamic dynamicEmployeeschedule = new ExpandoObject();
        //    dynamicEmployeeschedule.BookingID = empsched.BookingID;
        //    dynamicEmployeeschedule.EmployeeID = empsched.EmployeeID;
        //    dynamicEmployeeschedule.TimeID = empsched.TimeID;
        //    dynamicEmployeeschedule.DateID = empsched.DateID;
        //    dynamicEmployeeschedule.EmployeeID = empsched.EmployeeID;
        //    dynamicEmployeeschedule.StatusID = empsched.StatusID;
        //    dynamicEmployeeschedule.Employee = getEmployee(empsched.Employee);
        //    dynamicEmployeeschedule.EmployeeSchedule = getEmployeeSchedule(empsched.);
        //    return dynamicEmployeeschedule;
        //}
        //private dynamic getEmployee(Employee forBooking)
        //{

        //        dynamic dynamicEmployees = new ExpandoObject();
        //        dynamicEmployees.EmployeeID = forBooking.EmployeeID;
        //        dynamicEmployees.Name = forBooking.Name;



        //    return dynamicEmployees;
        //}
        //private dynamic getEmployeeSchedule(EmployeeSchedule forBooking)
        //{

        //    dynamic dynamicEmployees = new ExpandoObject();
        //    dynamicEmployees.EmployeeID = forBooking.EmployeeID;
        //    dynamicEmployees.Name = forBooking.Name;



        //    return dynamicEmployees;
        //}

        //the one the admin does
        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Booking/ConfirmClientBookings")]
        public IHttpActionResult ConfirmClientBooking(int BookingID)
        {

            db.Configuration.ProxyCreationEnabled = false;

            Booking bookings = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
            bookings.StatusID = 4;

            db.SaveChanges();

            return Ok(bookings);
        }


        //Request Booking
        [HttpPost]
        [Route("api/Bookings/RequestBooking")]
        public dynamic RequestBooking([FromBody] Booking booking, string SessionID)
        {
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                int ClientID = db.Clients.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.ClientID).FirstOrDefault();
                if (ClientID != 0)
                {
                    try
                    {
                        db.Configuration.ProxyCreationEnabled = false;
                        Booking newBooking = new Booking();
                        newBooking.ClientID = ClientID;
                        newBooking.StatusID = 1;
                        newBooking.ReminderID = 1;
                        db.Bookings.Add(newBooking);
                        db.SaveChanges();


                        int BookingID = newBooking.BookingID;

                        return SaveBooking(booking, BookingID);

                    }
                    catch (Exception err)
                    {
                        return err.Message;
                    }
                }
                else
                {
                    return "Client not found";
                }
            }
            else
            {
                return "Session is no longer valid";
            }
        }

        [Route("api/Bookings/NoShow")]
        [HttpGet]
        public dynamic NoShow(string SessionID, int BookingID)
        {
            bool findUser = UserController.CheckUser(SessionID);
            if (findUser)
            {
                try
                {
                    Booking findBooking = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
                    findBooking.StatusID = 7;
                    db.SaveChanges();
                    return "success";
                }
                catch (Exception err)
                {
                    return err.Message;
                }
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return toReturn;
            }
        }

        [Route("api/Bookings/DeleteBooking")]
        [HttpDelete]
        public dynamic DeleteBooking(int BookingID)
        {
            Booking findBooking = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
            db.Bookings.Remove(findBooking);
            db.SaveChanges();
            return "success";
        }

        public class MBClass
        {
            public DateTime StartDate { get; set; }
            public int ClientID { get; set; }
            public int ServiceID { get; set; }
            public int OptionID { get; set; }
            public int EmployeeID { get; set; }
            public int TimeID { get; set; }
            public string Notes { get; set; }
            public string SessionID { get; set; }
        }

        [Route("api/Bookings/MakeBooking")]
        [HttpPost]
        public dynamic MakeBooking([FromBody]MBClass Bookings)
        {
            string SessionID = Bookings.SessionID;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                try
                {
                    var Date = Bookings.StartDate;
                    int getDateID = db.Dates.Where(zz => zz.Date1 == Bookings.StartDate.Date).Select(zz => zz.DateID).FirstOrDefault();
                    if (getDateID != 0)
                    {
                        Booking saveBooking = new Booking();
                        saveBooking.ClientID = Bookings.ClientID;
                        saveBooking.StatusID = 4;
                        saveBooking.ReminderID = 1;
                        db.Bookings.Add(saveBooking);
                        db.SaveChanges();
                        int BookingID = saveBooking.BookingID;

                        EmployeeSchedule saveSlot = db.EmployeeSchedules.Where(zz => zz.EmployeeID == Bookings.EmployeeID && zz.DateID == getDateID && zz.TimeID == Bookings.TimeID).FirstOrDefault();
                        saveSlot.BookingID = BookingID;
                        saveSlot.StatusID = 3;
                        db.SaveChanges();
                    

                        BookingLine saveLine = new BookingLine();
                        saveLine.BookingID = BookingID;
                        saveLine.ServiceID = Bookings.ServiceID;
                        if (Bookings.OptionID != 0)
                        {
                            saveLine.OptionID = Bookings.OptionID;
                        }
                        db.BookingLines.Add(saveLine);
                        db.SaveChanges();

                        BookingNote saveNotes = new BookingNote();
                        if (Bookings.Notes != null)
                        {
                            saveNotes.Note = Bookings.Notes;
                            saveNotes.BookingID = BookingID;
                            db.BookingNotes.Add(saveNotes);
                            db.SaveChanges();
                        }
                       
                    }
                    else
                    {
                        return "Booking details are invalid";
                    }
                   
                }
                catch
                {
                    return "client details are invalid";
                }
                return "success";
            }
            else
            {
                return "Session is no longer valid";
            }
        }



        private dynamic SaveBooking(Booking Modell, int BookingID)
        {
            try
            {
                foreach (DateRequested items in Modell.DateRequesteds)
                {
                    items.BookingID = BookingID;
                    items.Date = items.Date.AddDays(1);
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
                    if (items.Note != null)
                    {
                        BookingNote notes = new BookingNote();
                        notes.Note = items.Note;
                        notes.BookingID = BookingID;
                        db.BookingNotes.Add(notes);
                        db.SaveChanges();
                    }
                }

                return "success";

            }
            catch (Exception err)
            {
                return err.Message;
            }
        }


        //[System.Web.Http.Route("api/Booking/getReminder")]
        //[System.Web.Mvc.HttpGet]
        //public dynamic getReminder(int id)
        //{

        //    db.Configuration.ProxyCreationEnabled = false;
        //    return db.Reminders.Where(zz => zz.ReminderID == id).Where(ii=> ii.TypeID ==  2).FirstOrDefault();

        //}
        //private List<dynamic> getReminderReturnList(List<Reminder> ForReminder)
        //{
        //    List<dynamic> dymanicReminders = new List<dynamic>();
        //    foreach (Reminder reminder in ForReminder)
        //    {
        //        dynamic dynamicReminder = new ExpandoObject();
        //        dynamicReminder.ReminderID = reminder.ReminderID;
        //        dynamicReminder.ReminderType = reminder.ReminderType;
        //        dynamicReminder.Text = reminder.Text;

        //        dymanicReminders.Add(dynamicReminder);
        //    }
        //    return dymanicReminders;
        //}
    }
}
