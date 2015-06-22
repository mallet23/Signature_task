using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

class ProducerConsumerQueue 
    //: IDisposable
{
    //string filePath = "";


    //EventWaitHandle wh = new AutoResetEvent(false);
    //Thread worker;
    //object locker = new object();
    //Queue<string> tasks = new Queue<string>();

    //public ProducerConsumerQueue()
    //{
    //    worker = new Thread(Work);
    //    worker.Start();
    //}

    //public void EnqueueTask(string task)
    //{
    //    lock (locker)
    //        tasks.Enqueue(task);

    //    wh.Set();
    //}

    //public void Dispose()
    //{
    //    EnqueueTask(null);      // Сигнал Потребителю на завершение
    //    worker.Join();          // Ожидание завершения Потребителя
    //    wh.Close();             // Освобождение ресурсов
    //}

    //void Work()
    //{
    //    while (true)
    //    {
    //        string task = null;

    //        lock (locker)
    //        {
    //            if (tasks.Count > 0)
    //            {
    //                task = tasks.Dequeue();
    //                if (task == null)
    //                    return;
    //            }
    //        }

    //        if (task != null)
    //        {
    //            Console.WriteLine("Выполняется задача: " + task);
    //            Thread.Sleep(1000); // симуляция работы...
    //        }
    //        else
    //            wh.WaitOne();       // Больше задач нет, ждем сигнала...
    //    }
    //}



    readonly object listLock = new object();
    Queue<int> queue =  new Queue<int>();

    public void Produce(int text)
    {
        lock (listLock)
        {
            queue.Enqueue(text);

            // We always need to pulse, even if the queue wasn't
            // empty before. Otherwise, if we add several items
            // in quick succession, we may only pulse once, waking
            // a single thread up, even if there are multiple threads
            // waiting for items.            
            Monitor.Pulse(listLock);
        }
    }

    public int Consume()
    {
        lock (listLock)
        {
            // If the queue is empty, wait for an item to be added
            // Note that this is a while loop, as we may be pulsed
            // but not wake up before another thread has come in and
            // consumed the newly added object. In that case, we'll
            // have to wait for another pulse.
            while (queue.Count == 0)
            {
                // This releases listLock, only reacquiring it
                // after being woken up by a call to Pulse
                Monitor.Wait(listLock);
            }
            return queue.Dequeue();
        }
    }
}