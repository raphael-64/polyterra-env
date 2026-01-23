using System.IO;
using System.Text;

public class ByteArrayReader : BinaryReader
{
	public ByteArrayReader(Stream input)
		: base(input)
	{
	}

	public ByteArrayReader(Stream input, Encoding encoding)
		: base(input, encoding)
	{
	}

	public ByteArrayReader(Stream input, Encoding encoding, bool leaveOpen)
		: base(input, encoding, leaveOpen)
	{
	}

	public override short ReadInt16()
	{
		return (short)(((ReadByte() & 0xFF) << 8) + (ReadByte() & 0xFF));
	}

	public override int ReadInt32()
	{
		return ((ReadByte() & 0xFF) << 24) + ((ReadByte() & 0xFF) << 16) + ((ReadByte() & 0xFF) << 8) + (ReadByte() & 0xFF);
	}

	public override string ReadString()
	{
		int num = ReadInt16();
		if (num > 0)
		{
			byte[] bytes = ReadBytes(num);
			return Encoding.UTF8.GetString(bytes, 0, num);
		}
		return "";
	}

	public string[] ReadStringArray()
	{
		int num = ReadInt16();
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ReadString();
		}
		return array;
	}

	public byte[] ReadByteArray()
	{
		int num = ReadInt16();
		if (num == -1)
		{
			return null;
		}
		return ReadBytes(num);
	}
}
