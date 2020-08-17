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

namespace ExperTech_Api.Controllers
{
    public class BookingController : ApiController
    {
        ExperTechEntities db = new ExperTechEntities();



        //View scheule 
        [System.Web.Http.Route("api/Booking/getClientBooking")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> getClientBooking()
        {

            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> booking = db.Bookings.Include(ii => ii.EmployeeSchedules).Include(ll => ll.BookingStatu)
                .Include(zz => zz.BookingNotes).Include(hh => hh.BookingLines).ToList();
            return getClientBooking(booking);

        }
        //View booking request 
        [System.Web.Http.Route("api/Booking/ViewClientBooking")]
        [System.Web.Mvc.HttpGet]
        public List<dynamic> ViewClientBooking(int id)
        {

            db.Configuration.ProxyCreationEnabled = false;
            List<Booking> booking = db.Bookings.Include(ii => ii.EmployeeSchedules).Include(ll => ll.BookingStatu)
                .Include(zz => zz.BookingNotes).Include(hh => hh.BookingLines).Where(zz => zz.BookingID == id).ToList();
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
                Schedge.Schedule = getSchedules(schedue.Schedule);
                Schedge.EmpType = getEmployee(schedue.EmployeeServiceType);

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
                notes.Notes = NOTE.Notes;
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
                lines.Service = bookinglien.Service.Name;
                lines.ServiceOption = bookinglien.ServiceOption.Name;
                ine.Add(lines);
            }
            return ine;

        }

        //the one the admin does
        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Booking/ConfirmClientBooking")]
        public IHttpActionResult ConfirmClientBooking(int BookingID)
        {

            db.Configuration.ProxyCreationEnabled = false;

            Booking bookings = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();
            bookings.StatusID = 2;

            db.SaveChanges();

            return Ok(bookings);
        }


        //advise on booking 
        [System.Web.Mvc.HttpPut]
        [System.Web.Http.Route("api/Booking/AdviseOnBooking")]
        public dynamic AdviseOnBooking()
        {
        }


        //Request Booking
        [HttpPost]
        [Route("api/Booking/RequestBooking")]
        public dynamic RequestBooking(Booking booking)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                Booking newBooking = new Booking();
                newBooking.ClientID = booking.ClientID;
                newBooking.StatusID = 1;
                newBooking.ReminderID = 1;
                db.SaveChanges();

                int BookingID = db.Bookings.Where(zz => zz.ClientID == booking.ClientID).Select(zz => zz.BookingID).LastOrDefault();

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
                    EmployeeSchedule findSchedule = db.EmployeeSchedules.Where(zz => zz.EmployeeID == items.EmployeeID && zz.TypeID == items.TypeID
                                                                                && zz.TimeID == items.TimeID && zz.DateID == items.DateID).FirstOrDefault();

                    findSchedule.BookingID = BookingID;
                    findSchedule.StatusID = 2;
                    db.SaveChanges();
                }
                if (Modell.Client.Sales.)
                    foreach (BookingLine items in Modell.BookingLines)
                    {
                        BookingLine line = new BookingLine();
                        line.BookingID = items.BookingID;
                        line.ServiceID = items.ServiceID;
                        line.OptionID = items.OptionID;

                        db.BookingLines.Add(line);
                        db.SaveChanges();
                    }


                foreach (BookingNote items in Modell.BookingNotes)
                {
                    BookingNote notes = new BookingNote();
                    notes.Notes = items.Notes;
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
