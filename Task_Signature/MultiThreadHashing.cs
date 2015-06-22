using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Task_Signature
{
    class MultiThreadHashing
    {
        public string filePath { get; private set; }
        int _curentBlock = 0;
        public long blockLength { get; private set; }
        int blockCount;
        string[] hashes;

        public int processorCount = 1;

        ProducerConsumerQueue queue = new ProducerConsumerQueue();



        public MultiThreadHashing(string filePath_, int blockLenght_ = 102400, int processorCount_ = 0)
        {
            

            filePath = filePath_;
            blockLength = blockLenght_;
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                blockCount = (int)(stream.Length / blockLenght_);
            }

            if (processorCount_ < 0)
            {
                processorCount = (Environment.ProcessorCount > blockCount ? 
                    blockCount : Environment.ProcessorCount);
            }
            else
            {
                processorCount = processorCount_;
            }

            Console.WriteLine("Количество болков" + blockCount.ToString());
            hashes = new string[blockCount];

            if (!File.Exists(filePath))
            {
                throw new Exception("File is not exist!");
            }
        }



        public string[] ComputeHash()
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                

                Thread[] thread = new Thread[processorCount];

                for (int j = 0; j < processorCount; j++)
                {
                    thread[j] = new Thread(ThreadWork);
                    thread[j].Name = "Thread " + j.ToString();
                    thread[j].Start();
                }
                ThreadWork();

                for (int j = 0; j < processorCount; j++)
                {
                    thread[j].Join();
                }
            }
            return hashes;

        }

        object Lock = new object();
        void ThreadWork()
        {
            byte[] buffer;
            byte[] oldBuffer;
            int bytesRead;
            int oldBytesRead;
            long totalBytesRead = 0;
            
            int lenght = (int)(blockLength / 1024);
            //   int i = 0;

            using (FileStream stream = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                do
                {
                    int currentBlock = 0;
                    lock (Lock)
                    {
                        if (_curentBlock >= blockCount)
                            break;
                        currentBlock = _curentBlock;
                        _curentBlock++;
                    }
                    stream.Position = currentBlock * blockLength;

                    using (HashAlgorithm hashAlgorithm = MD5.Create())
                    {


                        buffer = new byte[1024];

                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        totalBytesRead += bytesRead;
                        for (int i = 0; i < lenght; i++)
                        {
                            oldBytesRead = bytesRead;
                            oldBuffer = buffer;

                            buffer = new byte[1024];
                            bytesRead = stream.Read(buffer, 0, buffer.Length);

                            totalBytesRead += bytesRead;

                            if (i == lenght - 1)
                            {
                                hashAlgorithm.TransformFinalBlock(oldBuffer, 0, oldBytesRead);
                            }
                            else
                            {
                                hashAlgorithm.TransformBlock(oldBuffer, 0, oldBytesRead, oldBuffer, 0);
                            }
                        }
                        string result = BitConverter.ToString(hashAlgorithm.Hash).Replace("-", String.Empty);
                        hashes[currentBlock] = Thread.CurrentThread.Name + " выводит " + result;
               //         Console.WriteLine(hashes[currentBlock]);
                  //      Console.WriteLine(currentBlock);
                    }
                } while (_curentBlock < blockCount);
            }



        }
    }
}
