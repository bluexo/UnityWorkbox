using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Arthas.Network
{
    public class ByteBuffer
    {
        private byte[] data;
        private int readerIndex;
        private int writerIndex;
        private int markReader;
        private int markWriter;

        /**
        * 初始化
        **/
        public ByteBuffer(int capacity)
        {
            data = new byte[capacity];
            readerIndex = 0;
            writerIndex = 0;
            markReader = 0;
            markWriter = 0;
        }

        public ByteBuffer(byte[] content)
        {
            data = content;
            readerIndex = 0;
            writerIndex = content.Length;
            markReader = 0;
            markWriter = 0;
        }

        private ByteBuffer()
        {

        }
        /**
            *  容量
            **/
        public int Capacity()
        {
            return data.Length;
        }

        /**
            * 扩容
            */
        public ByteBuffer Capacity(int nc)
        {
            lock (this)
            {
                if (nc > data.Length)
                {
                    byte[] newData = new byte[data.Length + nc];
                    Array.Copy(data, 0, newData, 0, data.Length);
                    data = newData;
                }
                return this;
            }
        }

        /**
            * 清除掉所有标记
            * @return 
        **/
        public ByteBuffer Clear()
        {
            readerIndex = 0;
            writerIndex = 0;
            markReader = 0;
            markWriter = 0;
            return this;
        }
        /**
            * 拷贝
            **/
        public ByteBuffer Copy()
        {
            ByteBuffer item = new ByteBuffer(data.Length);
            Array.Copy(data, item.data, data.Length);
            item.readerIndex = readerIndex;
            item.writerIndex = writerIndex;
            item.markReader = markReader;
            item.markWriter = markWriter;
            return item;
        }

        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public ByteBuffer Duplicate()
        {
            ByteBuffer item = new ByteBuffer
            {
                readerIndex = readerIndex,
                writerIndex = writerIndex,
                markReader = markReader,
                markWriter = markWriter,
                data = data
            };
            return item;
        }

        /**
            * 获取一个字节
            **/
        public byte GetByte(int index)
        {
            if (index < data.Length)
            {
                return data[index];
            }
            return 0;
        }

        /**
            * 读取四字节整形F
            **/
        public int GetInt(int index)
        {
            if (index + 3 < data.Length)
            {
                int ret = data[index] << 24;
                ret |= data[index + 1] << 16;
                ret |= data[index + 2] << 8;
                ret |= data[index + 3];
                return ret;
            }
            return 0;
        }

        /**
            * 读取两字节整形
            **/
        public short GetShort(int index)
        {
            if (index + 1 < data.Length)
            {
                short r1 = (short)(data[index] << 8);
                short r2 = data[index + 1];
                short ret = (short)(r1 | r2);
                return ret;
            }
            return 0;
        }

        /**
            * 标记读
            **/
        public ByteBuffer MarkReaderIndex()
        {
            markReader = readerIndex;
            return this;
        }

        /**
            * 标记写
            **/
        public ByteBuffer MarkWriterIndex()
        {
            markWriter = writerIndex;
            return this;
        }

        /**
            * 可写长度
            **/
        public int MaxWritableBytes()
        {
            return data.Length - writerIndex;
        }

        /**
            * 读取一个字节
            **/
        public byte ReadByte()
        {
            if (readerIndex < writerIndex)
            {
                byte ret = data[readerIndex++];
                return ret;
            }
            return 0;
        }

        /**
            * 读取多个字节
            **/
        public void ReadBytes(byte[] dst, int dstIndex, int length)
        {
            if (dst == null)
            {
                throw new NullReferenceException();
            }

            if (dst.Length < dstIndex + length)
            {
                throw new IndexOutOfRangeException();
            }

            if (writerIndex - readerIndex < length)//可读范围小于长度
            {
                throw new IndexOutOfRangeException("not readable length.");
            }

            Array.Copy(data, readerIndex, dst, dstIndex, length);
            readerIndex += length;
        }

        /**
            * 读取四字节整形
            **/
        public int ReadInt()
        {
            if (readerIndex + 3 < writerIndex)
            {
                int ret = (int)(((data[readerIndex++]) << 24) & 0xff000000);
                ret |= (((data[readerIndex++]) << 16) & 0x00ff0000);
                ret |= (((data[readerIndex++]) << 8) & 0x0000ff00);
                ret |= (((data[readerIndex++])) & 0x000000ff);
                return ret;
            }
            return 0;
        }

        /**
            * 读取两个字节的整形
            **/
        public short ReadShort()
        {
            if (readerIndex + 1 < writerIndex)
            {
                int h = data[readerIndex++];
                int l = data[readerIndex++] & 0x000000ff;
                int len = ((h << 8) & 0x0000ff00) | (l);
                return (short)len;
            }
            return 0;
        }

        /**
            * 可读字节数
            **/
        public int ReadableBytes()
        {
            return writerIndex - readerIndex;
        }

        /**
            * 读指针
            **/
        public int ReaderIndex()
        {
            return readerIndex;
        }

        /**
            * 移动读指针
            **/
        public ByteBuffer ReaderIndex(int readerIndex)
        {
            if (readerIndex <= writerIndex)
            {
                this.readerIndex = readerIndex;
            }
            return this;
        }

        /**
            * 重置读指针
            **/
        public ByteBuffer ResetReaderIndex()
        {
            if (markReader <= writerIndex)
            {
                readerIndex = markReader;
            }
            return this;
        }

        /**
            * 重置写指针
            **/
        public ByteBuffer ResetWriterIndex()
        {
            if (markWriter >= readerIndex)
            {
                writerIndex = markWriter;
            }
            return this;
        }

        /**
            * 设置字节
            **/
        public ByteBuffer SetByte(int index, byte value)
        {
            if (index < data.Length)
            {
                data[index] = value;
            }
            return this;
        }

        /**
            * 设置字节
            **/
        public ByteBuffer SetBytes(int index, byte[] src, int from, int len)
        {
            if (index + len <= len)
            {
                Array.Copy(src, from, data, index, len);
            }
            return this;
        }

        /**
            * 设置读写指针
            **/
        public ByteBuffer SetIndex(int readerIndex, int writerIndex)
        {
            if (readerIndex >= 0 && readerIndex <= writerIndex && writerIndex <= data.Length)
            {
                this.readerIndex = readerIndex;
                this.writerIndex = writerIndex;
            }
            return this;
        }

        /**
            * 设置四字节整形
            **/
        public ByteBuffer SetInt(int index, int value)
        {
            if (index + 4 <= data.Length)
            {
                data[index++] = (byte)((value >> 24) & 0xff);
                data[index++] = (byte)((value >> 16) & 0xff);
                data[index++] = (byte)((value >> 8) & 0xff);
                data[index++] = (byte)(value & 0xff);
            }
            return this;
        }

        /**
            * 设置两字节整形
            **/
        public ByteBuffer SetShort(int index, short value)
        {
            if (index + 2 <= data.Length)
            {
                data[index++] = (byte)((value >> 8) & 0xff);
                data[index++] = (byte)(value & 0xff);
            }
            return this;
        }

        /**
            * 略过一些字节
            **/
        public ByteBuffer SkipBytes(int length)
        {
            if (readerIndex + length <= writerIndex)
            {
                readerIndex += length;
            }
            return this;
        }

        /**
            * 剩余的可写字节数
            **/
        public int WritableBytes()
        {
            return data.Length - writerIndex;
        }

        /**
            * 写入一个字节
            * 
            **/
        public ByteBuffer WriteByte(byte value)
        {
            Capacity(writerIndex + 1);
            data[writerIndex++] = value;
            return this;
        }

        /**
            * 写入四字节整形
            **/
        public ByteBuffer WriteInt(int value)
        {
            Capacity(writerIndex + 4);
            data[writerIndex++] = (byte)((value >> 24) & 0xff);
            data[writerIndex++] = (byte)((value >> 16) & 0xff);
            data[writerIndex++] = (byte)((value >> 8) & 0xff);
            data[writerIndex++] = (byte)(value & 0xff);
            return this;
        }

        /**
            * 写入两字节整形
            **/
        public ByteBuffer WriteShort(short value)
        {
            Capacity(writerIndex + 2);
            data[writerIndex++] = (byte)((value >> 8) & 0xff);
            data[writerIndex++] = (byte)(value & 0xff);
            return this;
        }

        /**
            * 写入一部分字节
            **/
        public ByteBuffer WriteBytes(ByteBuffer src)
        {
            int sum = src.writerIndex - src.readerIndex;
            if (sum > 0)
            {
                Capacity(writerIndex + sum);
                Array.Copy(src.data, src.readerIndex, data, writerIndex, sum);
                writerIndex += sum;
                src.readerIndex += sum;
            }
            return this;
        }

        /**
            * 写入一部分字节
            **/
        public ByteBuffer WriteBytes(ByteBuffer src, int len)
        {
            if (len > 0)
            {
                Capacity(writerIndex + len);
                Array.Copy(src.data, src.readerIndex, data, writerIndex, len);
                writerIndex += len;
                src.readerIndex += len;
            }
            return this;
        }

        /**
            * 写入一部分字节
            **/
        public ByteBuffer WriteBytes(byte[] src)
        {
            int sum = src.Length;
            Capacity(writerIndex + sum);
            if (sum > 0)
            {
                Array.Copy(src, 0, data, writerIndex, sum);
                writerIndex += sum;
            }
            return this;
        }

        /**
            * 写入一部分字节
            **/
        public ByteBuffer WriteBytes(byte[] src, int off, int len)
        {
            int sum = len;
            if (sum > 0)
            {
                Capacity(writerIndex + sum);
                Array.Copy(src, off, data, writerIndex, sum);
                writerIndex += sum;
            }
            return this;
        }

        /**
            * 读取utf字符串
            **/
        public string ReadUTF8()
        {
            short len = ReadShort(); // 字节数
            byte[] charBuff = new byte[len]; //
            Array.Copy(data, readerIndex, charBuff, 0, len);
            readerIndex += len;
            return Encoding.UTF8.GetString(charBuff);
        }

        /**
            * 写入utf字符串
            * 
            **/
        public ByteBuffer WriteUTF8(string value)
        {
            byte[] content = Encoding.UTF8.GetBytes(value.ToCharArray());
            int len = content.Length;
            Capacity(writerIndex + len + 2);
            WriteShort((short)len);
            Array.Copy(content, 0, data, writerIndex, len);
            writerIndex += len;
            return this;
        }

        /**
            * 写指针
            **/
        public int WriterIndex()
        {
            return writerIndex;
        }

        /**
            * 移动写指针
            **/
        public ByteBuffer WriterIndex(int writerIndex)
        {
            if (writerIndex >= readerIndex && writerIndex <= data.Length)
            {
                this.writerIndex = writerIndex;
            }
            return this;
        }

        /**
            * 原始字节数组
            **/
        public byte[] GetRaw()
        {
            return data;
        }
    }
}
