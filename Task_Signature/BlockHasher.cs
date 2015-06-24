using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Task_Signature
{
    class BlockHasher
    {
        /// <summary>
        /// Стандартная область входного массива
        ///     байтов. (4 Мб)
        /// </summary>
        public static int DefaultBlockSize = 4194304;

        /// <summary>
        /// Алгоритмы для хеширования
        /// </summary>
        public enum HashAlgorithms
        {
            KeyedHashAlgorithm,
            MD5,
            RIPEMD160,
            SHA1,
            SHA256,
            SHA384,
            SHA512
        }

        /// <summary>
        /// Ввести размер области входного массива
        ///     байтов(блоков)
        /// </summary>
        /// <param name="bufferSize">размер блока в байтах</param>
        public void SetBufferSize(int bufferSize)
        {
            if (bufferSize > 0)
                _bufferSize = bufferSize;
            else
                throw new Exception("Ошибка! Размер буфера"
                    + " не может быть отрицательным!");
        }
        /// <summary>
        /// Получить размер области входного массива
        ///     байтов(блоков)
        /// </summary>
        public int GetBufferSize()
        {
            return _bufferSize;
        }
        // Размер входного массива байтов
        private int _bufferSize;

        /// <summary>
        /// Создать алгоритм хеширования
        /// </summary>
        public static HashAlgorithm CreateHashAlgorithm(HashAlgorithms algorithm)
        {
            switch (algorithm)
            {
                case HashAlgorithms.MD5:
                    return MD5.Create();

                case HashAlgorithms.SHA1:
                    return SHA1.Create();

                case HashAlgorithms.KeyedHashAlgorithm:
                    return KeyedHashAlgorithm.Create();

                case HashAlgorithms.RIPEMD160:
                    return RIPEMD160.Create();

                case HashAlgorithms.SHA256:
                    return SHA256.Create();

                case HashAlgorithms.SHA384:
                    return SHA384.Create();

                case HashAlgorithms.SHA512:
                    return SHA512.Create();

                default:
                    throw new Exception("Алгоритма хеширования в данной "
                        + "реализации не существует!");
            }
        }

        /// <summary>
        /// Получить текущий алгоритм хеширования
        /// </summary>
        /// <returns>Текущий алгоритм хеширования</returns>
        public HashAlgorithms GetCurrentAlgorithm()
        {
            return _hashAlgorithm;
        }
        // Алгоритм хеширования
        private HashAlgorithms _hashAlgorithm = HashAlgorithms.MD5;


        /// <summary>
        /// Хеширование по блокам
        /// BlockHasher(BlockHasher.DefaultBlockSize, HashAlgorithms.MD5)
        /// </summary>
        public BlockHasher() :
            this(BlockHasher.DefaultBlockSize, HashAlgorithms.MD5) { }

        /// <summary>
        /// Хеширование по блокам
        /// BlockHasher(BlockHasher.DefaultBlockSize, algorithm_)
        /// </summary>
        /// <param name="algorithm_">Алгоритм хеширования</param>
        public BlockHasher(HashAlgorithms algorithm_) :
            this(BlockHasher.DefaultBlockSize, algorithm_) { }


        /// <summary>
        /// Хеширование по блокам
        /// </summary>
        /// <param name="bufferSize_">Размер область входного массива байтов.</param>
        /// <param name="algorithm_">Алгоритм хеширования</param>
        public BlockHasher(int bufferSize_,
            HashAlgorithms algorithm_ = HashAlgorithms.MD5)
        {
            SetBufferSize(bufferSize_);
            _hashAlgorithm = algorithm_;
        }

        #region Hash From Stream
        /// <summary>
        /// Вычисляет хэш-значение для заданного объекта Stream.
        ///    с размером блока равным BlockHasher.DefaultBlockSize
        /// </summary>
        /// <param name="stream">Поток, по которому хешируем</param>
        /// <returns>Хеш строкой</returns>
        public string ComputeStringHash(Stream stream)
        {
            return ComputeStringHash(stream, BlockHasher.DefaultBlockSize);
        }

        /// <summary>
        /// Вычисляет хэш-значение для заданного объекта Stream.
        /// </summary>
        /// <param name="stream">Поток, по которому хешируем</param>
        /// <param name="blockLenght">Размер блоков для хеширования</param>
        /// <returns>Хеш строкой</returns>
        public string ComputeStringHash(Stream stream, long blockLenght)
        {
            byte[] hash = ComputeHash(stream, blockLenght);

            return ByteHashToString(hash);
        }

        /// <summary>
        /// Вычисляет хэш-значение для заданного объекта Stream.
        /// с размером блока равным BlockHasher.DefaultBlockSize
        /// </summary>
        /// <param name="stream">Поток, по которому хешируем</param>
        /// <returns>Вычисляемый хэш-код.</returns>
        public byte[] ComputeHash(Stream stream)
        {
            return ComputeHash(stream, BlockHasher.DefaultBlockSize);
        }

        /// <summary>
        /// Вычисляет хэш-значение для заданного объекта Stream.
        /// </summary>
        /// <param name="stream">Поток, по которому хешируем</param>
        /// <param name="blockLenght">Размер блоков для хеширования</param>
        /// <returns>Вычисляемый хэш-код.</returns>
        public byte[] ComputeHash(Stream stream, long blockLenght)
        {
            if (stream == null)
            {
                throw new Exception("Ошибка! Не задан поток данных для хеш функции!");
            }

            byte[] hash;
            byte[] buffer;
            int bytesRead;
            long totalBytesRead = 0;
            int blockSize = blockLenght > (long)_bufferSize ?
                            _bufferSize : unchecked((int)blockLenght);

            using (HashAlgorithm hashAlgo = CreateHashAlgorithm(_hashAlgorithm))
            {
                do
                {
                    buffer = new byte[blockSize];

                    bytesRead = stream.Read(buffer, 0, blockSize);
                    totalBytesRead += bytesRead;

                    if (totalBytesRead >= blockLenght || bytesRead == 0)
                    {
                        // Последний блок не дополняется нулями
                        hashAlgo.TransformFinalBlock(buffer, 0, bytesRead);
                    }
                    else
                    {
                        hashAlgo.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    }
                } while (totalBytesRead < blockLenght && bytesRead != 0);

                hash = hashAlgo.Hash;
            }
            return hash;
        }
        #endregion

        #region Hash from byte array
        /// <summary>
        /// Вычисляет хэш-значение для заданной области заданного массива байтов.
        /// </summary>
        /// <param name="text">Входные данные, для которых вычисляется хэш-код.</param>
        /// <returns>Вычисляемый хэш-код.</returns>
        public string ComputeStringHash(byte[] text)
        {
            byte[] hash = ComputeHash(text);
            return ByteHashToString(hash);
        }

        /// <summary>
        /// Вычисляет хэш-значение для заданной области заданного массива байтов.
        /// </summary>
        /// <param name="text">Входные данные, для которых вычисляется хэш-код.</param>
        /// <returns>Вычисляемый хэш-код.</returns>
        public byte[] ComputeHash(byte[] text)
        {
            HashAlgorithm hashAlgo = CreateHashAlgorithm(_hashAlgorithm);
            return hashAlgo.ComputeHash(text);
        }

        /// <summary>
        /// Вычисляет хэш-значение для заданной области заданного массива байтов.
        /// </summary>
        /// <param name="text">Входные данные, для которых вычисляется хэш-код.</param>
        /// <param name="offset">Смещение в массиве байтов, 
        /// начиная с которого следует использовать данные.</param>
        /// <param name="count">Число байтов в массиве для использования в качестве данных.</param>
        /// <returns>Вычисляемый хэш-код.</returns>
        public string ComputeStringHash(byte[] text, int offset, int count)
        {
            HashAlgorithm hashAlgo = CreateHashAlgorithm(_hashAlgorithm);
            byte[] hash = hashAlgo.ComputeHash(text, offset, count);
            return ByteHashToString(hash);
        }

        /// <summary>
        /// Вычисляет хэш-значение для заданной области заданного массива байтов.
        /// </summary>
        /// <param name="text">Входные данные, для которых вычисляется хэш-код.</param>
        /// <param name="offset">Смещение в массиве байтов, 
        /// начиная с которого следует использовать данные.</param>
        /// <param name="count">Число байтов в массиве для использования в качестве данных.</param>
        /// <returns>Вычисляемый хэш-код.</returns>
        public byte[] ComputeHash(byte[] text, int offset, int count)
        {
            HashAlgorithm hashAlgo = CreateHashAlgorithm(_hashAlgorithm);
            return hashAlgo.ComputeHash(text, offset, count);
        }
        #endregion

        /// <summary>
        /// Преобразование byte array в строковый формат.
        /// </summary>
        /// <param name="hash">Хеш в байтах</param>
        /// <returns>Строковое значение хеша</returns>
        public static string ByteHashToString(byte[] hash)
        {
            return BitConverter.ToString(hash);
        }
    }
}
