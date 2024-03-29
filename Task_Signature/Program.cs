﻿using System;
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
        static int Main(string[] args)
        {
            // При ручном прерывании программы: сообщаем об этом
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Interrupt);

            Console.WriteLine("Enter path:");
            var path = Console.ReadLine();
            Console.WriteLine("Enter block size (bytes):");
            long blockSize = 0;
            if (!Int64.TryParse(Console.ReadLine(), out blockSize))
            {
                Console.WriteLine("Не верное значение! " +
                    "Размер блока будет по умолчанию: " +
                    MultiThreadBlockHashing.DEFAULT_BLOCK_LENGHT + " байт.");
                blockSize = MultiThreadBlockHashing.DEFAULT_BLOCK_LENGHT;
            }


            TimeSpan ts;
            string elapsedTime;

            try
            {

                //// single 
                //MultiThreadBlockHashing hash1 = new MultiThreadBlockHashing(path, 0, blockSize);

                //Stopwatch stopWatch1 = new Stopwatch();
                //stopWatch1.Start();
                //hash1.ComputeHash();
                //stopWatch1.Stop();

                //ts = stopWatch1.Elapsed;
                //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);
                //Console.WriteLine("Speed " + hash1.processorCount + " : " + elapsedTime);

                //// 2
                //MultiThreadBlockHashing hash2 = new MultiThreadBlockHashing(path, 1, blockSize);

                //Stopwatch stopWatch2 = new Stopwatch();
                //stopWatch2.Start();
                //hash2.ComputeHash();
                //stopWatch2.Stop();

                //ts = stopWatch2.Elapsed;
                //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);
                //Console.WriteLine("Speed " + hash2.processorCount + " : " + elapsedTime);

                //// 4
                //MultiThreadBlockHashing hash4 = new MultiThreadBlockHashing(path, 3, 102400);

                //Stopwatch stopWatch4 = new Stopwatch();
                //stopWatch4.Start();
                //hash4.ComputeHash();
                //stopWatch4.Stop();

                //ts = stopWatch4.Elapsed;
                //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);
                //Console.WriteLine("Speed " + hash4.processorCount + " : " + elapsedTime);

                // Auto
                //    Stopwatch stopWatch = new Stopwatch();
                Console.WriteLine("Начинаем...");
                MultiThreadBlockHashing hash = new MultiThreadBlockHashing(path, blockSize);

                //     stopWatch.Start();
                var result = hash.ComputeHash();

                //   stopWatch.Stop();              

                //     ts = stopWatch.Elapsed;
                //     elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //      ts.Hours, ts.Minutes, ts.Seconds,
                //      ts.Milliseconds / 10);
                //     Console.WriteLine("Speed " + hash.processorCount + " : " + elapsedTime);

                //// 16
                //Stopwatch stopWatch16 = new Stopwatch();

                //MultiThreadBlockHashing hash16 = new MultiThreadBlockHashing(path, 15, blockSize);

                //stopWatch16.Start();
                //hash16.ComputeHash();
                //stopWatch16.Stop();

                //ts = stopWatch16.Elapsed;
                //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);
                //Console.WriteLine("Speed " + hash16.processorCount + " : " + elapsedTime);

                //// 32
                //Stopwatch stopWatch32 = new Stopwatch();

                //MultiThreadBlockHashing hash32 = new MultiThreadBlockHashing(path, 31, blockSize);

                //stopWatch32.Start();
                //hash32.ComputeHash();
                //stopWatch32.Stop();

                //ts = stopWatch16.Elapsed;
                //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                //ts.Hours, ts.Minutes, ts.Seconds,
                //ts.Milliseconds / 10);
                //Console.WriteLine("Speed " + hash32.processorCount + " : " + elapsedTime);

                //проверяем ошибки, если были - выводим
                if (hash.exceptions.Any())
                {
                    Console.WriteLine("В процессе работы были зафиксированы " +
                        "следующие ошибки:");
                    foreach (var ex in hash.exceptions)
                    {
                        Console.WriteLine("Поток " + ex.Key + ": " + ex.Value.Message);
                    }
                }

                if (!hash.IsSuccess())
                {
                    Console.WriteLine("Не удалось взять хеш значение!");
                    return 1;
                }

                for (int i = 0; i < result.Length; i++)
                    Console.WriteLine("N" + i + " : " + result[i]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка! " + e.Message);

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Сообщение при ручном прерывании программы
        /// </summary>
        protected static void Interrupt(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Операция хэширования была прервана.");
            Console.WriteLine("Была нажата клавиша: {0}", args.SpecialKey);
        }

    }
}
