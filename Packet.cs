using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Fluxify.Networking
{
    /// <summary>
    /// Sunucudan istemciye gönderilen paket türleri.
    /// </summary>
    public enum ServerPackets
    {
        Welcome = 1,
        SpawnPlayer,
        PlayerPosition,
        PlayerRotation
    }

    /// <summary>
    /// İstemciden sunucuya gönderilen paket türleri.
    /// </summary>
    public enum ClientPackets
    {
        WelcomeReceived = 1,
        PlayerMovement
    }

    /// <summary>
    /// Veri paketi oluşturmak ve okumak için kullanılan sınıf.
    /// </summary>
    public class Packet : IDisposable
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        /// <summary>
        /// Boş bir paket oluşturur (ID olmadan).
        /// </summary>
        public Packet()
        {
            buffer = new List<byte>();
            readPos = 0;
        }

        /// <summary>
        /// Belirli bir ID'ye sahip bir paket oluşturur (Gönderim için kullanılır).
        /// </summary>
        /// <param name="_id">Paket ID'si.</param>
        public Packet(int _id)
        {
            buffer = new List<byte>();
            readPos = 0;
            Write(_id);
        }

        /// <summary>
        /// Veriden paket oluşturur (Alma için kullanılır).
        /// </summary>
        /// <param name="_data">Pakete eklenecek baytlar.</param>
        public Packet(byte[] _data)
        {
            buffer = new List<byte>();
            readPos = 0;
            SetBytes(_data);
        }

        #region Fonksiyonlar
        /// <summary>
        /// Paketin içeriğini ayarlar ve okunmaya hazır hale getirir.
        /// </summary>
        /// <param name="_data">Pakete eklenecek baytlar.</param>
        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        /// <summary>
        /// Paketin başına içeriğin uzunluğunu ekler.
        /// </summary>
        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }

        /// <summary>
        /// Verilen int'i paketin başına ekler.
        /// </summary>
        /// <param name="_value">Eklenen int değeri.</param>
        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Paketin içeriğini byte array olarak alır.
        /// </summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>
        /// Paketin içeriğinin uzunluğunu alır.
        /// </summary>
        public int Length()
        {
            return buffer.Count;
        }

        /// <summary>
        /// Paketin içerisinde okunmamış verinin uzunluğunu alır.
        /// </summary>
        public int UnreadLength()
        {
            return Length() - readPos;
        }

        /// <summary>
        /// Paket örneğini tekrar kullanmak için sıfırlar.
        /// </summary>
        /// <param name="_shouldReset">Paketin sıfırlanıp sıfırlanmayacağı.</param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else
            {
                readPos -= 4; // Son okunan int'i "okunmamış" durumuna getir
            }
        }
        #endregion

        #region Veri Yazma
        /// <summary>
        /// Bir byte'ı pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen byte.</param>
        public void Write(byte _value)
        {
            buffer.Add(_value);
        }

        /// <summary>
        /// Bir byte array'ini pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen byte array.</param>
        public void Write(byte[] _value)
        {
            buffer.AddRange(_value);
        }

        /// <summary>
        /// Bir short'u pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen short.</param>
        public void Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Bir int'i pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen int.</param>
        public void Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Bir long'u pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen long.</param>
        public void Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Bir float'u pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen float.</param>
        public void Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Bir bool'u pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen bool.</param>
        public void Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Bir string'i pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen string.</param>
        public void Write(string _value)
        {
            Write(_value.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(_value));
        }

        /// <summary>
        /// Bir Vector3'ü pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen Vector3.</param>
        public void Write(Vector3 _value)
        {
            Write(_value.X);
            Write(_value.Y);
            Write(_value.Z);
        }

        /// <summary>
        /// Bir Quaternion'ı pakete ekler.
        /// </summary>
        /// <param name="_value">Eklenen Quaternion.</param>
        public void Write(Quaternion _value)
        {
            Write(_value.X);
            Write(_value.Y);
            Write(_value.Z);
            Write(_value.W);
        }
        #endregion

        #region Veri Okuma
        /// <summary>
        /// Paketten bir byte okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte _value = readableBuffer[readPos];
                if (_moveReadPos)
                {
                    readPos += 1;
                }
                return _value;
            }
            else
            {
                throw new Exception("Byte türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten belirli bir uzunluktaki byte array'ini okur.
        /// </summary>
        /// <param name="_length">Okunacak byte array'in uzunluğu.</param>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte[] _value = buffer.GetRange(readPos, _length).ToArray();
                if (_moveReadPos)
                {
                    readPos += _length;
                }
                return _value;
            }
            else
            {
                throw new Exception("Byte array türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir short okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                short _value = BitConverter.ToInt16(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 2;
                }
                return _value;
            }
            else
            {
                throw new Exception("Short türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir int okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                int _value = BitConverter.ToInt32(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 4;
                }
                return _value;
            }
            else
            {
                throw new Exception("Int türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir long okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                long _value = BitConverter.ToInt64(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 8;
                }
                return _value;
            }
            else
            {
                throw new Exception("Long türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir float okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                float _value = BitConverter.ToSingle(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 4;
                }
                return _value;
            }
            else
            {
                throw new Exception("Float türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir bool okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                bool _value = BitConverter.ToBoolean(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 1;
                }
                return _value;
            }
            else
            {
                throw new Exception("Bool türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir string okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt();
                string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length);
                if (_moveReadPos && _value.Length > 0)
                {
                    readPos += _length;
                }
                return _value;
            }
            catch
            {
                throw new Exception("String türünde değer okunamadı!");
            }
        }

        /// <summary>
        /// Paketten bir Vector3 okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public Vector3 ReadVector3(bool _moveReadPos = true)
        {
            return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        /// <summary>
        /// Paketten bir Quaternion okur.
        /// </summary>
        /// <param name="_moveReadPos">Okuma pozisyonunu taşıyıp taşımamak.</param>
        public Quaternion ReadQuaternion(bool _moveReadPos = true)
        {
            return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }
        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
