using Quartz;// 引用 Quartz 套件中的核心功能。
using Quartz.Impl;// 引用 Quartz.Impl 命名空間，它包含了 Quartz 排程器的具體實現。
using System;// 引用 System 命名空間，提供了許多基本類別和功能。
using System.Reflection.Metadata;
using System.Threading.Tasks;// 引用 System.Threading.Tasks 命名空間，這是 C# 中處理異步操作的標準庫。


namespace ScheduleDemo_1746
{
    internal class Program
    {
        private static TimeSpan specificTime;   // 指定時間
        private static DateTimeOffset nextRunTime;  // 下一次指定的時間

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

                // 支援 12 小時制或 24 小時制
                if (TimeSpan.TryParseExact(inputTime, "hh\\:mm\\:ss", null, out specificTime) || TimeSpan.TryParseExact(inputTime, "HH\\:mm\\:ss", null, out specificTime))
                {
                    Console.WriteLine($"你輸入的指定時間是：{specificTime}");
                    break;  // 正確輸入後跳出循環
                }
                else
                {
                    Console.WriteLine("時間格式無效，請輸入正確的時間（例如:10:30:00）");
                }
            }

            // 設定間隔排程
            var intervalJob = JobBuilder.Create<PrintSeconds>()  // 使用自定義的 PrintJob
                .WithIdentity("intervalJob")   // 設定這個排程任務的唯一識別名稱
                .Build();  // 建立排程

            // 用來創建一個新的觸發器（Trigger）。Trigger 是 Quartz 用來決定何時觸發排程任務的機制。
            var intervalTrigger = TriggerBuilder.Create()
                .WithIdentity("intervalTrigger")
                .StartNow()  // 即刻開始
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(intervalInSeconds)  // 設定間隔時間
                    .RepeatForever())
                .Build();

            // 將 intervalJob 與 intervalTrigger 註冊到排程器中
            await scheduler.ScheduleJob(intervalJob, intervalTrigger);


            var specificTimeJob = JobBuilder.Create<PrintTimes>()  // 使用自定義的 PrintJob
                .WithIdentity("specificTimeJob")   // 設定工作名稱
                .Build();

            var specificTimeTrigger = TriggerBuilder.Create()
                .WithIdentity("specificTimeTrigger")    // 觸發器設置一個唯一的名稱。
                .StartAt(nextRunTime)  // 設定下次執行時間
                .Build();

            await scheduler.ScheduleJob(specificTimeJob, specificTimeTrigger);


            // 等待排程執行，防止程式提前結束
            Console.WriteLine("按任意鍵退出...");
            Console.ReadKey();
        }
    }

    // 定義排程任務的具體操作
    public class PrintTimes : IJob
    {
        //實作 IJob 介面中的 Execute 方法，這是每次排程觸發時執行的代碼。async 表示這個方法是異步執行的，能夠處理耗時操作。
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[{DateTime.Now}] 記錄指定時間打印！");

            // 程序會等到這個已經完成的任務結束後才會繼續執行。
            // `Task.CompletedTask` 表示此任務已經完成，並且返回一個已經完成的 Task 對象
            await Task.CompletedTask;
        }
    }

    public class PrintSeconds : IJob
    {
        //實作 IJob 介面中的 Execute 方法，這是每次排程觸發時執行的代碼。async 表示這個方法是異步執行的，能夠處理耗時操作。
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[{DateTime.Now}] 記錄打印");
            await Task.CompletedTask;
        }
    }
}
