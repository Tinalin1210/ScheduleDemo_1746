using Quartz;  //讓你設定和管理定時任務的工具。
using Quartz.Impl; //提供了具體的類別來幫助你創建和啟動任務調度器
using System;   
using System.Globalization;  //幫助處理這些格式的差異。
using System.Threading.Tasks;  //用來支援 異步程式設計。異步程式可以讓程式在等待某些耗時操作時不會停下來，這樣可以提高效能。

namespace ScheduleDemo_1746
{
    internal class Program
    {
        private static TimeSpan specificTime;   // 指定時間

        private static async Task Main(string[] args)
        {
            int intervalInSeconds;  // 間隔數
            // 使用 StdSchedulerFactory 創建並啟動了 Quartz 調度器（Scheduler）
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            //啟動調度器
            await scheduler.Start();

            while (true)
            {
                Console.WriteLine("請輸入間隔時間（秒）：");

                // 使用 TryParse 確保輸入為正整數
                if (int.TryParse(Console.ReadLine(), out intervalInSeconds) && intervalInSeconds > 0)
                {
                    break;  // 成功轉換並且是正整數，跳出循環
                }
                else
                {
                    Console.WriteLine("請重新輸入正整數!!!");
                }
            }


            while (true)
            {
                Console.WriteLine("請輸入指定時間（HH:mm:ss）：");
                string inputTime = Console.ReadLine();  // 讀取使用者輸入的時間

                DateTime currentDateTime = DateTime.Now;  // 取當下時間
                string RunTimeString = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");   //格式化成一個字串，格式為 yyyy-MM-dd HH:mm:ss

                // 支援 12 小時制或 24 小時制
                if (TimeSpan.TryParseExact(inputTime, "hh\\:mm\\:ss", null, out specificTime) || TimeSpan.TryParseExact(inputTime, "HH\\:mm\\:ss", null, out specificTime))
                {
                    Console.WriteLine($"你輸入的指定時間是：{specificTime}");

                
                    DateTime RunTime = DateTime.Parse(RunTimeString); //會將這個字串轉換成對應的 DateTime 物件 它會在格式化後形成字串作為判斷條件來設置排程的執行時間。

                    // 設定間隔排程
                    var intervalJob = JobBuilder.Create<PrintSeconds>()  // 使用自定義的 PrintSeconds
                        .WithIdentity("intervalJob")   // 設定這個排程任務的唯一識別名稱
                        .Build();  // 建立排程

                    var intervalTrigger = TriggerBuilder.Create() //這是觸發 intervalJob 任務的條件，它設定任務的執行頻率。
                        .WithIdentity("intervalTrigger")
                        .StartNow()  // 即刻開始
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(intervalInSeconds)  // 設定間隔時間
                            .RepeatForever())  //這個設定會使任務重複執行，直到程式結束或手動停止。
                        .Build(); // 建立排程

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
