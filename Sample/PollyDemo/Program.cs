using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Polly;
using Polly.Timeout;

namespace PollyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            Console.WriteLine("This is an example of use of the Polly policies.");
            Console.WriteLine("Here we will combine the Retry and the TimeOut policies.");
            Console.WriteLine("\r\nPress 'c' to cancel operation...");

            Task.Factory.StartNew(async () => await ExecuteTask(ct));

            //要求停止
            char ch = Console.ReadKey().KeyChar;
            if (ch == 'c' || ch == 'C')
            {
                tokenSource.Cancel();
                Console.WriteLine("\nTask cancellation requested.");
            }

            Console.WriteLine("\r\nEnd of program, press any key...");
            Console.ReadKey();
        }

        private static async Task ExecuteTask(CancellationToken cancellationToken)
        {
            var maxRetryAttempts = 10;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);
            var timeoutInSec = 30;

            //重试策略
            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures,
                (exception, timespan, retryCount, context) => ManageRetryException(exception, timespan, retryCount, context));

            //超时策略
            var timeoutPolicy = Policy.TimeoutAsync(timeoutInSec, TimeoutStrategy.Pessimistic,
                 (context, timeSpan, task) => ManageTimeoutException(context, timeSpan, task));

            var policyWrap = Policy.WrapAsync(retryPolicy, timeoutPolicy);

            //执行暂态任务
            await policyWrap.ExecuteAsync(async (ct, dic) =>
            {
                Console.WriteLine("\r\nExecuting task...");
                //var result = await FailedOperation(ct);
            },
            new Dictionary<string, object>() { { "ExecuteOperation", "Operation description..." } },
            cancellationToken);
        }

        //private static async Task<bool> FailedOperation(object ct)
        //{
        //    //return await Task.Delay(1000).f;
        //}

        private static Task ManageTimeoutException(Context context, TimeSpan timeSpan, Task task)
        {
            throw new NotImplementedException();
        }

        private static void ManageRetryException(Exception exception, TimeSpan timespan, object retryCount, object context)
        {
            throw new NotImplementedException();
        }
    }
}
