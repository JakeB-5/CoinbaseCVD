using CBCVD.Processor;
using CBCVD.Processor.Workers;
using Discord;
using Discord.WebSocket;
using Quartz;
using Quartz.Impl;
using ScottPlot;
using SkiaSharp;
using Color = ScottPlot.Color;


// var processor = new CVDProcessor();
// var cvds = await processor.Run();

// await CBCVD.Processor.Messaging.Telegram.SendMessage(cvds);
// _ = Task.Run(() =>
// {
//     var bot = CBCVD.Processor.Messaging.Discord.Instance;
// });

StdSchedulerFactory factory = new();
IScheduler scheduler = await factory.GetScheduler();
await scheduler.Start();

var job = JobBuilder.Create<CVDJob>()
    .WithIdentity("CVDJob")
    .Build();

ITrigger trigger = TriggerBuilder.Create()
    .WithIdentity("CVDTrigger")
    .StartNow()
    .WithCronSchedule("0 1 0/1 ? * * *")
    .Build();

await scheduler.ScheduleJob(job, trigger);



// Console.WriteLine("Scheduler Start");
// Console.ReadLine();
await Task.Delay(-1);

return;
