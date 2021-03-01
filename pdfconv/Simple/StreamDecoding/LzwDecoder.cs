#region Copyright notice
// Following code (except some refactorings) was taken from PDFsharp project
// https://github.com/empira/PDFsharp/blob/master/src/PdfSharp/Pdf.Filters/LzwDecode.cs

//
// Authors:
//   David Stephensen
//
// Copyright (c) 2005-2019 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;

namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Decodes compressed data using LZW algorithm
    /// </summary>
    public class LzwDecoder : IStreamDecoder
    {        
        private readonly int[] _andTable = { 511, 1023, 2047, 4095 };
        private byte[][] _stringTable;
        private byte[] _data;

        private int _tableIndex, _bitsToGet = 9;
        private int _bytePointer;
        private int _nextData = 0;
        private int _nextBits = 0;

        /// <summary>
        /// Decode LZW compressed data
        /// </summary>
        /// <param name="inputData">Compresed data bytes</param>
        /// <returns>Decompressed data bytes</returns>
        public byte[] Decode(byte[] inputData)
        {
            var outputStream = new MemoryStream();

            InitializeDictionary();

            _data = inputData;
            _bytePointer = 0;
            _nextData = 0;
            _nextBits = 0;
            int code, oldCode = 0;
            byte[] str;

            while ((code = GetNextCode()) != 257)
            {
                if (code == 256)
                {
                    InitializeDictionary();
                    code = GetNextCode();
                    if (code == 257) { break; }

                    outputStream.Write(_stringTable[code], 0, _stringTable[code].Length);
                    oldCode = code;
                }
                else
                {
                    if (code < _tableIndex)
                    {
                        str = _stringTable[code];
                        outputStream.Write(str, 0, str.Length);
                        AddEntry(_stringTable[oldCode], str[0]);
                        oldCode = code;
                    }
                    else
                    {
                        str = _stringTable[oldCode];
                        outputStream.Write(str, 0, str.Length);
                        AddEntry(str, str[0]);
                        oldCode = code;
                    }
                }
            }

            outputStream.Capacity = (int)outputStream.Length;
            return outputStream.GetBuffer();
        }

        /// <summary>
        /// Initialize the dictionary.
        /// </summary>
        private void InitializeDictionary()
        {
            _stringTable = new byte[8192][];

            for (int i = 0; i < 256; i++)
            {
                _stringTable[i] = new byte[1];
                _stringTable[i][0] = (byte)i;
            }

            _tableIndex = 258;
            _bitsToGet = 9;
        }

        /// <summary>
        /// Add a new entry to the Dictionary.
        /// </summary>
        private void AddEntry(byte[] oldString, byte newString)
        {
            int length = oldString.Length;
            byte[] str = new byte[length + 1];
            Array.Copy(oldString, 0, str, 0, length);
            str[length] = newString;

            _stringTable[_tableIndex++] = str;

            _bitsToGet = _tableIndex switch {
                511 => 10,
                1023 => 11,
                2047 => 12,
                _ => _bitsToGet
            };
        }

        /// <summary>
        /// Returns the next set of bits.
        /// </summary>
        private int GetNextCode()
        {
            try
            {
                _nextData = (_nextData << 8) | (_data[_bytePointer++] & 0xff);
                _nextBits += 8;

                if (_nextBits < _bitsToGet)
                {
                    _nextData = (_nextData << 8) | (_data[_bytePointer++] & 0xff);
                    _nextBits += 8;
                }

                int code = (_nextData >> (_nextBits - _bitsToGet)) & _andTable[_bitsToGet - 9];
                _nextBits -= _bitsToGet;

                return code;
            }
            catch
            {
                return 257;
            }
        }
    }
}