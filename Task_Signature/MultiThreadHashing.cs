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
        public static int DEFAULT_BLOCK_LENGHT = 1024;

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string filePath { get; private set; }

        /// <summary>
        /// Размер блока
        /// </summary>
        public long blockLength { get; private set; }

        /// <summary>
        /// Количество процессоров
        /// </summary>
        public int processorCount { get; private set; }

        /// <summary>
        /// Ошибки, возникающие в потоках. 
        /// Имя потока : ошибка
        /// </summary>      
        public Queue<KeyValuePair<string, Exception>> exceptions { get; private set; }

        // Количество блоков        
        int blockCount;
        // Текущий блок
        int _currentBlock = 0;
        // Массив Хешей
        string[] hashes;
        // Блокиратор для _currentBlock
        object Lock = new object();
        // Блоки, не захешированные из за ошибки        
        Queue<int> looseBlocks = new Queue<int>();


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
        ///     или количеству блоков
        /// </summary>
        /// <param name="filePath_">Путь к файлу</param>
        /// <param name="blockLenght_">Размер блока</param>
        public MultiThreadHashing(string filePath_,
            long blockLenght_) :
            this(filePath_, 0, blockLenght_)
        {
            processorCount = Environment.ProcessorCount > blockCount ?
                    blockCount : (Environment.ProcessorCount - 1);
        }

        /// <summary>
        /// Мультипоточное хеширование
        /// </summary>
        /// <param name="filePath_">Путь к файлу</param>
        /// <param name="processCount_">Количество дополнительно создаваемых процессов</param>
        /// <param name="blockLenght_">Размер блока</param>        
        public MultiThreadHashing(string filePath_,
            int processCount_,
            long blockLenght_)
        {
            var file = new FileInfo(filePath_);

            #region Exceptions
            if (!file.Exists)
            {
                throw new Exception("Файл не найден!");
            }
            if (blockLenght_ > file.Length)
            {
                throw new Exception("Размер блока(" + blockLenght_
                    + ") превышает размер файла (" + file.Length + ")!");
            }
            if (processCount_ < 0)
            {
                throw new Exception("Количество процессов не может быть меньше 0!");
            }
            if (blockLenght_ <= 0)
            {
                throw new Exception("Размер блока не может быть меньше или равен 0!");
            }

            blockCount = (int)(Math.Ceiling((double)(file.Length / blockLenght_)));
            if (blockCount <= 0)
            {
                throw new Exception("Количество блоков не может быть меньше или равен 0!");
            }
            #endregion

            filePath = filePath_;
            processorCount = processCount_;

            blockLength = blockLenght_;
            hashes = new string[blockCount];
        }

        #endregion

        /// <summary>
        /// Вычислить хеш значения
        /// </summary>
        /// <returns></returns>
        public string[] ComputeHash()
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                Thread.CurrentThread.Name = "Main Thread";
                Thread[] thread = new Thread[processorCount];

                exceptions = new Queue<KeyValuePair<string, Exception>>();
                looseBlocks = new Queue<int>();

                for (int j = 0; j < processorCount; j++)
                {
                    thread[j] = new Thread(ThreadWork);
                    thread[j].Name = "Thread " + (j + 1).ToString();
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


        // Работа для потока. Хеширование.    
        private void ThreadWork()
        {
            int currentBlock = 0;
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open,
                    FileAccess.Read, FileShare.Read))
                {
                    var hashAlgorithm = new BlockHasher();
                    do
                    {
                        currentBlock = 0;
                        lock (Lock)
                        {
                            if (_currentBlock >= blockCount)
                                break;
                            currentBlock = _currentBlock;
                            _currentBlock++;
                        }
                        stream.Position = (long)currentBlock * blockLength;

                        hashes[currentBlock] = hashAlgorithm.ComputeStringHash(stream, blockLength);

                    } while (_currentBlock < blockCount);
                    TryHashLooseBlocks(stream, hashAlgorithm);
                }
            }
            catch (ThreadAbortException)
            {
                //это исключение возникает, 
                //  если главный поток хочет завершить приложение
                //          просто выходим из цикла, и завершаем выполнение
                return;
            }
            catch (Exception ex)
            {
                //в процессе работы возникло исключение
                // заносим ошибку в очередь ошибок, 
                // предварительно залочив ее
                lock (looseBlocks)
                {
                    exceptions
                        .Enqueue(new KeyValuePair<string, Exception>
                            (Thread.CurrentThread.Name, ex));
                    if (currentBlock < blockCount)
                        looseBlocks.Enqueue(currentBlock);
                }
                return;
            }
        }


        // Пытаемся завершить хеширование блоков, на которых произошла ошибка        
        private void TryHashLooseBlocks(Stream stream, BlockHasher hashAlgorithm)
        {
            while (looseBlocks.Any())
            {
                int looseBlock = 0;
                lock (looseBlocks)
                {
                    if (looseBlocks.Any())
                    {
                        looseBlock = looseBlocks.Dequeue();
                    }
                    else
                        return;
                }
                try
                {
                    stream.Position = (long)looseBlock * blockLength;
                    hashes[looseBlock] = hashAlgorithm.ComputeStringHash(stream, blockLength);
                }
                catch (Exception ex)
                {
                    lock (looseBlocks)
                    {
                        // Если не получилось, то возвращаем обратно
                        looseBlocks.Enqueue(looseBlock);
                    }
                }
            }
        }

        /// <summary>
        /// Проверка успешного хеширования.
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess()
        {
            // Если остались необработанные блоки
            if (looseBlocks.Any())
                return false;
            return true;
        }
    }
}
