using Quartz;
using Quartz.Impl;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ScheduleDemo_1746
{
    internal class Program
    {
        private static TimeSpan specificTime;   // 指定時間

        private static async Task Main(string[] args)
        {
            int intervalInSeconds;    // 間隔秒

            // 使用 StdSchedulerFactory 創建並啟動了 Quartz 調度器（Scheduler）
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            while (true)
            {
                Console.WriteLine("請輸入間隔時間（秒）：");

                if (int.TryParse(Console.ReadLine(), out intervalInSeconds) && intervalInSeconds > 0)  // int.TryParse 返回一個 bool，out intervalInSeconds 接收轉換後的整數結果
                {
                    break;  // 是正整數就跳出
                }
                else
                {
                    Console.WriteLine("請重新輸入要為正整數!!!");
                }
            }

            while (true)
            {
                Console.WriteLine("請輸入指定時間（HH:mm:ss）：");
                string inputTime = Console.ReadLine();  // 讀取使用者輸入的時間
                DateTime currentDateTime = DateTime.Now;  // 取當下時間
                string RunTimeString = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");  

                // 支援 12 小時制或 24 小時制
                if (TimeSpan.TryParseExact(inputTime, "hh\\:mm\\:ss", null, out specificTime) || TimeSpan.TryParseExact(inputTime, "HH\\:mm\\:ss", null, out specificTime))
                {
                    Console.WriteLine($"你輸入的指定時間是：{specificTime}");

                    // Convert the string RunTime to DateTime
                    DateTime RunTime = DateTime.Parse(RunTimeString);  // Convert to DateTime

                    // 設定間隔排程
                    var intervalJob = JobBuilder.Create<PrintSeconds>()  // 使用自定義的 PrintSeconds
                        .WithIdentity("intervalJob")   // 設定這個排程任務的唯一識別名稱
                        .Build();  // 建立排程

                    var intervalTrigger = TriggerBuilder.Create()
                        .WithIdentity("intervalTrigger")
                        .StartNow()  // 即刻開始
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(intervalInSeconds)  // 設定間隔時間
                            .RepeatForever())
                        .Build();

                    // 將 intervalJob 與 intervalTrigger 註冊到排程器中
                    await scheduler.ScheduleJob(intervalJob, intervalTrigger);

                    var specificTimeJob = JobBuilder.Create<PrintTimes>()  // 使用自定義的 PrintTimes
                        .WithIdentity("specificTimeJob")   // 設定工作名稱
                        .Build();

                    var specificTimeTrigger = TriggerBuilder.Create()
                        .WithIdentity("specificTimeTrigger")    // 觸發器設置一個唯一的名稱。
                        .StartAt(RunTime)  // 設定下次執行時間，這裡 RunTime 是 DateTime 類型
                        .Build();

                    await scheduler.ScheduleJob(specificTimeJob, specificTimeTrigger);
                    break;  // 正確輸入後跳出循環
                }
                else
                {
                    Console.WriteLine("時間格式無效，請輸入正確的時間（例如:10:30:00）");
                }
            }

            // 等待排程執行，防止程式提前結束
            Console.WriteLine("按任意鍵退出...");
            Console.ReadKey();
        }
    }

    //實作 IJob 介面中的 Execute 方法，這是每次排程觸發時執行的代碼。async 表示這個方法是異步執行的，能夠處理耗時操作。
    //Task.CompletedTask 程序會等到這個已經完成的任務結束後才會繼續執行。，並且返回一個已經完成的 Task 對象
    public class PrintTimes : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[{DateTime.Now}] 記錄指定時間打印！");
            await Task.CompletedTask;
        }
    }

    public class PrintSeconds : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[{DateTime.Now}] 記錄打印");
            await Task.CompletedTask;
        }
    }
}
