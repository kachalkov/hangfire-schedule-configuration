﻿using System;
using System.Configuration;
using System.Linq.Expressions;
using Hangfire.Common;
using Hangfire.States;

namespace Hangfire.ScheduleConfiguration
{
    public class TaskSection : ConfigurationSection
    {
        [ConfigurationProperty("tasks", IsRequired = true)]
        public TaskList Tasks
        {
            get { return (TaskList) this["tasks"]; }

            set { this["tasks"] = value; }
        }

        public void Initialise()
        {
            var manager = new RecurringJobManager();

            foreach (TaskEntry task in this.Tasks.Items)
            {
                var type = Type.GetType(task.ServiceType);
                if (type != null)
                {
                    var method = type.GetMethod(task.Method);
                    var job = new Job(type, method);

                    manager.AddOrUpdate(task.Name, job, task.Cron);
                }
            }
        }

        public void Enqueue()
        {
            var client = new BackgroundJobClient(JobStorage.Current);

            foreach (TaskEntry task in this.Tasks.Items)
            {
                var type = Type.GetType(task.ServiceType);
                if (type != null)
                {
                    var method = type.GetMethod(task.Method);
                    var job = new Job(type, method);

                    client.Create(job, new EnqueuedState("default"));
                }
            }
        }

    }
}