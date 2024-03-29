﻿/* *********************************************************************
 * This Source Code Form is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 9 Greyfriars Rd, 
 * Reading, Berkshire, RG1 1NU.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 *
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 ********************************************************************** */

using System;
using System.IO;
using System.Text;

namespace DataFileHeader
{
    public class FileHeader
    {
        public string READER { get; set; }
        public string DataSetFormatVersion { get; set; }

        public DataSetString DataSetFormatName { get; set; }

        public DataSetString DataSetName { get; set; }
        public Guid DataSetGuid { get; set; }
        public Guid ExportTagGuid { get; set; }
        public DataSetString CopyrightNotice { get; set; }
        public ushort? LongestString { get; set; } = null;
        public DateTime PublishDate { get; set; }
        public DateTime NextExportDate { get; set; }
        public short? Age { get; set; } = null;

        public int? DeviceCombinations { get; set; } = null;
        public int? MaxUserAgentLength { get; set; } = null;

        public uint TotalStringValues { get; set; }

        public static FileHeader FromMemory(byte[] data)
        {
            FileHeader result = null;
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                result = ReadV3PatternV4Hash(reader);

            }
            if (result == null)
            {
                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    result = ReadV3Hash(reader);
                }
            }

            return result;

        }

        public static FileHeader FromFile(string path)
        {
            FileHeader result = null;

            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                result = ReadV3PatternV4Hash(reader);
            }
            if (result == null)
            {
                using (var reader = new BinaryReader(File.OpenRead(path)))
                {
                    result = ReadV3Hash(reader);
                }
            }

            return result;
        }

        private static FileHeader ReadV3PatternV4Hash(BinaryReader reader)
        {
            FileHeader result = new FileHeader();

            try
            {
                result.READER = "v3 Pattern or v4 Hash";

                int vMajor = reader.ReadInt32();
                int vMinor = reader.ReadInt32();
                result.DataSetFormatVersion = $"{vMajor}.{vMinor}.{reader.ReadInt32()}.{reader.ReadInt32()}";                

                var guidBytes = reader.ReadBytes(16);
                result.ExportTagGuid = new Guid(guidBytes);
                if (vMajor > 3 || (vMajor == 3 && vMinor > 1))
                {
                    guidBytes = reader.ReadBytes(16);
                    result.DataSetGuid = new Guid(guidBytes);
                }

                result.CopyrightNotice = new DataSetString() { Offset = reader.ReadInt32() };
                result.Age = reader.ReadInt16();

                reader.ReadInt32(); // min user agent count

                result.DataSetName = new DataSetString() { Offset = reader.ReadInt32() };
                result.DataSetFormatName = new DataSetString() { Offset = reader.ReadInt32() };

                result.PublishDate = ReadDate(reader);
                result.NextExportDate = ReadDate(reader);

                if (vMajor == 3)
                {
                    result.DeviceCombinations = reader.ReadInt32();
                    result.MaxUserAgentLength = reader.ReadInt16();
                    reader.ReadInt16(); // min useragent length
                    reader.ReadChar(); // lowest char
                    reader.ReadChar(); // highest char
                    reader.ReadInt32(); // max signatures
                    reader.ReadInt32(); // signature profiles count
                    reader.ReadInt32(); // signature nodes count
                    reader.ReadInt16(); // max values
                    reader.ReadInt32(); // csv buffer length
                    reader.ReadInt32(); // json buffer length
                    reader.ReadInt32(); // xml buffer length
                    reader.ReadInt32(); // max signatures closest
                    if (vMinor > 1)
                    {
                        reader.ReadInt32(); // max rank
                    }
                }

                var stringsStart = reader.ReadUInt32(); // strings start pos
                reader.ReadUInt32(); // strings byte length
                result.TotalStringValues = reader.ReadUInt32();

                var totalInts = 24;
                if (vMajor == 3 && vMinor == 1)
                {
                    totalInts = 30;
                } 
                else if (vMajor == 3 && vMinor == 2)
                {
                    totalInts = 36;
                }
                
                for (int i = 0; i < totalInts; i++)
                {
                    reader.ReadUInt32(); // start pos, byte length and item count for all other lists.
                                            // components, maps, properties, values, profiles
                                            // THEN
                                            // V4 = root nodes, nodes, profile offsets
                                            // V3 = (signatures, signature node offsets)[3.2 only]
                                            //   node ranked signature indexes, ranked signature indexes, 
                                            //   nodes, root nodes, profile offsets
                }

                var maxOffset = Math.Max(result.DataSetName.Offset,
                    Math.Max(result.CopyrightNotice.Offset, result.DataSetFormatName.Offset));

                while (reader.BaseStream.Position - stringsStart <= maxOffset)
                {
                    //if (reader.BaseStream.Position - stringsStart >= result.CopyrightNotice.Offset - 50 &&
                    //    reader.BaseStream.Position - stringsStart <= result.CopyrightNotice.Offset + 50)
                    //{
                    //    int x = 0;
                    //}
                    if (reader.BaseStream.Position - stringsStart == result.DataSetName.Offset)
                    {
                        result.DataSetName.Value = ReadString(reader);
                    }
                    else if (reader.BaseStream.Position - stringsStart == result.DataSetFormatName.Offset)
                    {
                        result.DataSetFormatName.Value = ReadString(reader);
                    }
                    else if (reader.BaseStream.Position - stringsStart == result.CopyrightNotice.Offset)
                    {
                        result.CopyrightNotice.Value = ReadString(reader);
                    }
                    else
                    {
                        var str = ReadString(reader);
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
        
        private static FileHeader ReadV3Hash(BinaryReader reader)
        {
            FileHeader result = new FileHeader();

            try
            {
                result.READER = "v3 Hash";

                result.DataSetFormatVersion = reader.ReadUInt16().ToString();
                result.DataSetFormatName = new DataSetString() { Offset = reader.ReadInt32() };
                result.DataSetName = new DataSetString() { Offset = reader.ReadInt32() };

                var guidBytes = reader.ReadBytes(16);
                result.DataSetGuid = new Guid(guidBytes);
                result.ExportTagGuid = Guid.Empty;

                result.PublishDate = ReadDate(reader);
                result.NextExportDate = ReadDate(reader);

                result.CopyrightNotice = new DataSetString() { Offset = reader.ReadInt32() };
                result.LongestString = reader.ReadUInt16();

                result.TotalStringValues = reader.ReadUInt32();
                var maxOffset = Math.Max(result.DataSetName.Offset,
                    Math.Max(result.CopyrightNotice.Offset, result.DataSetFormatName.Offset));
                var startPos = reader.BaseStream.Position;

                while (reader.BaseStream.Position - startPos <= maxOffset)
                {
                    if (reader.BaseStream.Position - startPos == result.DataSetName.Offset)
                    {
                        result.DataSetName.Value = ReadStringV3Hash(reader);
                    }
                    else if (reader.BaseStream.Position - startPos == result.DataSetFormatName.Offset)
                    {
                        result.DataSetFormatName.Value = ReadStringV3Hash(reader);
                    }
                    else if (reader.BaseStream.Position - startPos == result.CopyrightNotice.Offset)
                    {
                        result.CopyrightNotice.Value = ReadStringV3Hash(reader);
                    }
                    else
                    {
                        var str = ReadStringV3Hash(reader);
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        private static string ReadString(BinaryReader reader)
        {
            short length = reader.ReadInt16();
            byte[] bytes = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(bytes);
        }

        private static string ReadStringV3Hash(BinaryReader reader)
        {
            short length = reader.ReadInt16();
            byte[] bytes = reader.ReadBytes(length - 1);
            reader.ReadByte(); // null terminator
            return Encoding.ASCII.GetString(bytes);
        }

        private static DateTime ReadDate(BinaryReader reader)
        {
            var year = reader.ReadInt16();
            var month = reader.ReadByte();
            var day = reader.ReadByte();

            if (day >= 1 && day <= 31 &&
                month >= 1 && month <= 12 &&
                year >= 2000 && year <= 3000)
            {
                var date = new DateTime(year, month, day);
                return date;
            }

            throw new Exception("Invalid date");
        }
    }
}
