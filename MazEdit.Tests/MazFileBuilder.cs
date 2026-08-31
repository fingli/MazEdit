using System.IO;
using System.Text;

namespace MazEdit.Tests;

internal static class MazFileBuilder
{
    public const int Scale = 10000;
    public const int BlockStart = 0x64;
    public const int BlockSize = 100;

    public static byte[] Header(
        string material = "CST IRN",
        float initialZ = 200,
        byte multiMode = 3,
        int blockCount = 0)
    {
        var data = new byte[BlockStart + blockCount * BlockSize];
        data[0x09] = multiMode;
        WriteRawCoord(data, 0x28, initialZ);
        Encoding.ASCII.GetBytes(material.PadRight(12, '\0')[..12]).CopyTo(data, 0x54);
        return data;
    }

    public static int Offset(int blockIndex) => BlockStart + blockIndex * BlockSize;

    public static void WriteBlock(byte[] data, int blockIndex, byte marker, short sequence)
    {
        int i = Offset(blockIndex);
        data[i] = marker;
        BitConverter.GetBytes(sequence).CopyTo(data, i + 2);
    }

    public static void WriteInt16(byte[] data, int blockIndex, int fieldOffset, short value)
        => BitConverter.GetBytes(value).CopyTo(data, Offset(blockIndex) + fieldOffset);

    public static void WriteInt32(byte[] data, int blockIndex, int fieldOffset, int value)
        => BitConverter.GetBytes(value).CopyTo(data, Offset(blockIndex) + fieldOffset);

    public static void WriteAscii(byte[] data, int blockIndex, int fieldOffset, string text, int length = 16)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        int n = Math.Min(bytes.Length, length);
        Buffer.BlockCopy(bytes, 0, data, Offset(blockIndex) + fieldOffset, n);
    }

    public static void WriteByte(byte[] data, int blockIndex, int fieldOffset, byte value)
        => data[Offset(blockIndex) + fieldOffset] = value;

    public static void WriteCoord(byte[] data, int blockIndex, int fieldOffset, float value)
        => WriteRawCoord(data, Offset(blockIndex) + fieldOffset, value);

    public static void WriteRawCoord(byte[] data, int absoluteOffset, float value)
        => BitConverter.GetBytes((int)Math.Round(value * Scale)).CopyTo(data, absoluteOffset);

    public static MazProgram Parse(byte[] data, string? fileName = null)
    {
        string path = Path.Combine(Path.GetTempPath(), fileName ?? $"mazedit-{Guid.NewGuid():N}.maz");
        File.WriteAllBytes(path, data);
        try
        {
            return new MazParser().ParseSubProgram(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    public static (MazProgram Program, string Path) ParseKeepingFile(byte[] data)
    {
        string path = Path.Combine(Path.GetTempPath(), $"mazedit-{Guid.NewGuid():N}.maz");
        File.WriteAllBytes(path, data);
        return (new MazParser().ParseSubProgram(path), path);
    }

    /// <summary>Minimal TEST.MAZ-shaped program used as a golden fixture.</summary>
    public static byte[] TestMazLayout()
    {
        var data = Header(blockCount: 10);

        WriteBlock(data, 0, 0xA0, 1);

        WriteBlock(data, 1, 0xA0, 2);
        WriteCoord(data, 1, 36, 205);

        WriteBlock(data, 2, 0x0C, 1);
        WriteByte(data, 2, 8, 2);
        WriteCoord(data, 2, 40, 180);

        WriteBlock(data, 3, 0x02, 2);
        WriteInt32(data, 3, 8, 2);
        WriteCoord(data, 3, 36, -102.5f);
        WriteCoord(data, 3, 40, -552f);
        WriteCoord(data, 3, 48, -541.5f);

        WriteBlock(data, 4, 0x03, 3);

        WriteBlock(data, 5, 0x40, 4);
        WriteByte(data, 5, 17, 3);
        WriteCoord(data, 5, 40, 3);
        WriteCoord(data, 5, 44, 30);

        WriteBlock(data, 6, 0xB1, 1);
        WriteByte(data, 6, 9, 15);
        WriteByte(data, 6, 11, 9);
        WriteInt16(data, 6, 20, 0x40);
        WriteInt16(data, 6, 22, 3);
        WriteInt16(data, 6, 24, 8);
        WriteInt16(data, 6, 26, 51);
        WriteCoord(data, 6, 36, 63);
        WriteCoord(data, 6, 48, 3);
        WriteInt32(data, 6, 60, 180);
        WriteCoord(data, 6, 64, 1.2f);

        WriteBlock(data, 7, 0xC2, 1);
        WriteByte(data, 7, 8, 0x20);
        WriteCoord(data, 7, 40, 53.75f);

        WriteBlock(data, 8, 0xC2, 2);
        WriteByte(data, 8, 8, 0x21);
        WriteCoord(data, 8, 40, 53.75f);
        WriteCoord(data, 8, 48, 53.75f);

        WriteBlock(data, 9, 0x04, 5);
        WriteByte(data, 9, 8, 2);
        WriteByte(data, 9, 9, 1);
        WriteByte(data, 9, 10, 1);

        return data;
    }
}
