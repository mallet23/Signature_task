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
        /// <summary>
        /// Стандартный размер блока (1 Мб)
        /// </summary>
        public static readonly int DEFAULT_BLOCK_LENGHT = 1024;

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string filePath { get; private set; }

        /// <summary>
        /// Размер блока
        /// </summary>
        public int blockLength { get; private set; }

        /// <summary>
        /// Количество процессоров
        /// </summary>
        public int processorCount { get; private set; }

        // Количество блоков        
        int blockCount;
        // Текущий блок
        int _curentBlock = 0;
        // Массив Хешей
        string[] hashes;
        // Блокиратор для _curentBlock
        object Lock = new object();

        #region Constructors
        /// <summary>
        /// Мультипоточное хеширование с использованием
        ///     процессов равных количеству логических процессоров
        /// </summary>
        /// <param name="filePath_">Путь к файлу</param>
        public MultiThreadHashing(string filePath_) :
            this(filePath_, DEFAULT_BLOCK_LENGHT) { }

        /// <summary>
        /// Мультипоточное хеширование с использованием
        ///     процессов равных количеству логических процессоров
        /// </summary>
        /// <param name="filePath_">Путь к файлу</param>
        /// <param name="blockLenght_">Размер блока</param>
        public MultiThreadHashing(string filePath_,
            int blockLenght_ ) :
            this(filePath_, 0, blockLenght_)
        {
            processorCount = (Environment.ProcessorCount > blockCount ?
                    blockCount : Environment.ProcessorCount);
        }

        /// <summary>
        /// Мультипоточное хеширование
        /// </summary>
        /// <param name="filePath_">Путь к файлу</param>
        /// <param name="processCount_">Количество дополнительно создаваемых процессов</param>
        /// <param name="blockLenght_">Размер блока</param>        
        public MultiThreadHashing(string filePath_,
            int processCount_,
            int blockLenght_)
        {
            var file = new FileInfo(filePath_);
            if (!file.Exists)
            {
                throw new Exception("Файл не найден!");
            }
            blockCount = (int)(Math.Ceiling((double)file.Length / blockLenght_));
            filePath = filePath_;           

            if (processCount_ < 0)
            {
                throw new Exception("Количество процессов не может быть меньше 0!"); ;
            }
            processorCount = processCount_;

            blockLength = blockLenght_;
            Console.WriteLine("Количество блоков" + blockCount.ToString());
            hashes = new string[blockCount];
        }

        #endregion


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


        void ThreadWork()
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                var hashAlgorithm = new BlockHasher();
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
                    stream.Position = (long)currentBlock * blockLength;

                    hashes[currentBlock] = hashAlgorithm.ComputeStringHash(stream, blockLength);

                } while (_curentBlock < blockCount);
            }



        }
    }
}
