using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Task_Signature
{
    class Program
    {
        static void Main(string[] args)
        {
           // Task_Signature.Hash g = new Task_Signature.Hash();
            Console.WriteLine("Enter path:");
            var path = Console.ReadLine();

           //// var processorCount = Environment.ProcessorCount;
           // using (Stream stream = File.OpenRead(path))            
           // {
           //      var processorCount = Environment.ProcessorCount;
           //      var buffer = new byte[stream.Length / processorCount];
           //     stream.Read(buffer, 0, buffer.Length);               
                
           //     Thread[] thread = new Thread[processorCount];
           //     for (int j = 0; j < processorCount; j++)
           //     {
           //         stream.Read(buffer, 0, buffer.Length);
           //         thread[j] = new Thread(Hash.Compute);
           //         thread[j].Name = "Thread " + j.ToString();
           //         thread[j].Start(stream);
           //     }
           // }

            TimeSpan ts;
            string elapsedTime;

            // single 
            MultiThreadHashing hash1 = new MultiThreadHashing(path);

            Stopwatch stopWatch1 = new Stopwatch();
            stopWatch1.Start();
            hash1.ComputeHash();
            stopWatch1.Stop();

            ts = stopWatch1.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed " + hash1.processorCount + " : " + elapsedTime);

            // 2
            MultiThreadHashing hash2 = new MultiThreadHashing(path, 102400, 2);

            Stopwatch stopWatch2 = new Stopwatch();
            stopWatch2.Start();
            hash2.ComputeHash();
            stopWatch2.Stop();

            ts = stopWatch2.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed " + hash2.processorCount + " : " + elapsedTime);

            // 4
            MultiThreadHashing hash4 = new MultiThreadHashing(path, 102400, 4);

            Stopwatch stopWatch4 = new Stopwatch();
            stopWatch4.Start();
            hash4.ComputeHash();
            stopWatch4.Stop();

            ts = stopWatch4.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed " + hash4.processorCount + " : " + elapsedTime);

            //   Hash.ComputeFilesMD5(path);

            // 8
            Stopwatch stopWatch = new Stopwatch();

            MultiThreadHashing hash = new MultiThreadHashing(path, 102400, -1);

            stopWatch.Start();
            hash.ComputeHash();
            stopWatch.Stop();

            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed " + hash.processorCount + " : " + elapsedTime);

            // 16
            Stopwatch stopWatch16 = new Stopwatch();

            MultiThreadHashing hash16 = new MultiThreadHashing(path, 102400, 16);

            stopWatch16.Start();
            hash16.ComputeHash();
            stopWatch16.Stop();

            ts = stopWatch16.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed " + hash16.processorCount + " : " + elapsedTime);

            // 32
            Stopwatch stopWatch32 = new Stopwatch();

            MultiThreadHashing hash32 = new MultiThreadHashing(path, 102400, 32);

            stopWatch32.Start();
            hash32.ComputeHash();
            stopWatch32.Stop();

            ts = stopWatch16.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("Speed " + hash32.processorCount + " : " + elapsedTime);
            
            //for (int i = 0; i < result.Length; i++  )
            //    Console.WriteLine"N" + i + " : "+(result[i]);


          //  var result = g.BackgroundWorker_DoWork(path);// g.BackgroundWorker_DoWork(path);
           // Console.WriteLine("N" + " : " + result);
            Console.ReadLine();
        }
    }
}
