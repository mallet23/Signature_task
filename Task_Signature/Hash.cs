using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Management;
using System.Collections;

namespace Task_Signature
{
    class Hash
    {

        readonly object listLock = new object();
        Queue queue = new Queue();

        public void Produce(object o)
        {
            lock (listLock)
            {
                queue.Enqueue(o);

                // We always need to pulse, even if the queue wasn't
                // empty before. Otherwise, if we add several items
                // in quick succession, we may only pulse once, waking
                // a single thread up, even if there are multiple threads
                // waiting for items.            
                Monitor.Pulse(listLock);
            }
        }

        public object Consume()
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




        void blockHash(byte[] block)
        {
            // Init
            MD5 md5 = MD5.Create();
            int offset = 0;

            // For each block:
            offset += md5.TransformBlock(block, 0, block.Length, block, 0);

            // For last block:
            md5.TransformFinalBlock(block, 0, block.Length);

            // Get the has code
            byte[] hash = md5.Hash;
        }
        public byte[] BackgroundWorker_DoWork(string path)
        {

            byte[] buffer;
            byte[] oldBuffer;
            int bytesRead;
            int oldBytesRead;
            long size;
            long totalBytesRead = 0;
            int i = 0;

            using (Stream stream = File.OpenRead(path))
            using (HashAlgorithm hashAlgorithm = MD5.Create())
            {
                
                size = stream.Length;
                buffer = new byte[4096000];

                bytesRead = stream.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;
                do
                    {
                        oldBytesRead = bytesRead;
                        oldBuffer = buffer;

                        buffer = new byte[4096000];
                        bytesRead = stream.Read(buffer, 0, buffer.Length);

                        totalBytesRead += bytesRead;

                        if (bytesRead == 0)
                        {
                            hashAlgorithm.TransformFinalBlock(oldBuffer, 0, oldBytesRead);
                        }
                        else
                        {
                            hashAlgorithm.TransformBlock(oldBuffer, 0, oldBytesRead, oldBuffer, 0);
                        }
                        i++;


                    } while (bytesRead != 0);
                
                string result = BitConverter.ToString(hashAlgorithm.Hash).Replace("-", String.Empty);
                Console.WriteLine("N" + i.ToString() + " : " + result);
                return hashAlgorithm.Hash;

            }

        }


        public static void Compute(object stream)
        {
            byte[] text = new byte[((Stream)stream).Length / 8];
            ((Stream)stream).Read(text,0,text.Length);
            MD5 md5 = MD5.Create();
            byte[] Sum = md5.ComputeHash(text);
            string result = BitConverter.ToString(Sum).Replace("-", String.Empty);
            Console.WriteLine(Thread.CurrentThread.Name + " выводит " + result);
       //     Thread.Sleep(0);
        }

        public static void ComputeFilesMD5(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] filebytes = new byte[102400];
                fs.Read(filebytes, 0, filebytes.Length);
                byte[] Sum = md5.ComputeHash(filebytes);
                string result = BitConverter.ToString(Sum).Replace("-", String.Empty);
                Console.WriteLine(result);
            }
        }
    }
}
