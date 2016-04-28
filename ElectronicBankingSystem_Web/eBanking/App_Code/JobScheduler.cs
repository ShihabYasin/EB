using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Quartz;
using Quartz.Impl;

using System.Net;
using System.Net.Mail;
using System.Data.Entity;

namespace eBanking.App_Code
{
    public static class Variable2
    {
        public static bool Check { get; set; }
    
    }
    public class JobScheduler
    {
       public static IScheduler scheduler =  StdSchedulerFactory.GetDefaultScheduler();
       
        public static void Start()
        {

            Variable2.Check = true;
            //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
           
            IJobDetail job = JobBuilder.Create<UpdatePendingStatusJob>().Build();
            
            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInMinutes(5)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                    
                  )
                .Build();

            IJobDetail DCReturn = JobBuilder.Create<ReturnDistributorCommissionJob>().Build();
            ITrigger DCReturnTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                (s =>
                    //s.WithIntervalInMinutes(5)
                    s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                ).Build();
           


            scheduler.ScheduleJob(job, trigger);
            scheduler.ScheduleJob(DCReturn,DCReturnTrigger);
        }
    }

    public class UpdatePendingStatusJob:IJob
    {
        private SMSDR_Helper helperobj = new SMSDR_Helper();
        public void Execute(IJobExecutionContext context)
        {
            if (Variable2.Check == true)
            {
                helperobj.UpdateStatusOfPending();
            }
            //helperobj.EzzeRequestsUpdate();
        }
    }
    public class ReturnDistributorCommissionJob:IJob
    {
        private SMSDR_Helper helperObj = new SMSDR_Helper();
        public void Execute(IJobExecutionContext context)
        {
            helperObj.ReturnDistributorCommission();
        }
    }

    //public class EmailJob : IJob
    //{
    //   // private ApplicationDbContext db = new ApplicationDbContext();
    //    public void Execute(IJobExecutionContext context)
    //    {


    //        using (var message = new MailMessage("user@gmail.com", "user@live.co.uk"))
    //        {
    //            message.Subject = "Test";
    //            message.Body = "Test at " + DateTime.Now;
    //            using (SmtpClient client = new SmtpClient
    //            {
    //                EnableSsl = true,
    //                Host = "smtp.gmail.com",
    //                Port = 587,
    //                Credentials = new NetworkCredential("user@gmail.com", "password")
    //            })
    //            {
    //                client.Send(message);
    //            }
    //        }

    //        IEnumerable<Schedular> schedulerList = db.Schedulars.Where(x => x.Status == 1).Select(x => x).ToList();

    //        foreach (var scheduler in schedulerList)
    //        {
    //            scheduler.Status = 0;
    //            db.Entry(scheduler).State = EntityState.Modified;
    //            db.SaveChanges();

    //        }

    //    }
    //}
}