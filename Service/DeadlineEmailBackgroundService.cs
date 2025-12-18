using PorjectManagement.Controllers;
using PorjectManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace PorjectManagement.Service
{
    public class DeadlineEmailBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DeadlineEmailBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LabProjectManagementContext>();
                var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

                var now = DateTime.Now;
                var target = now.AddHours(24);

                var assignments = db.TaskAssignments
                    .Include(a => a.Task)
                    .Include(a => a.User)
                    .Where(a =>
                        !a.DeadlineMailSent &&
                        a.Task.Deadline >= target.AddMinutes(-5) &&
                        a.Task.Deadline <= target.AddMinutes(5))
                    .ToList();

                foreach (var a in assignments)
                {
                    var body = $@"
                        <h3>⏰ Task Deadline Reminder</h3>
                        <p>Hello <b>{a.User.FullName}</b>,</p>
                        <p>Your task <b>{a.Task.Title}</b> will be due in <b>24 hours</b>.</p>
                        <p><b>Deadline:</b> {a.Task.Deadline:dd/MM/yyyy HH:mm}</p>
                        <hr/>
                        <p>Lab Management System</p>
                    ";

                    await emailSender.SendAsync(
                        a.User.Email,
                        "⏰ Task Deadline – 24 Hours Left",
                        body
                    );

                    a.DeadlineMailSent = true;
                }

                await db.SaveChangesAsync(stoppingToken);

                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }


}
