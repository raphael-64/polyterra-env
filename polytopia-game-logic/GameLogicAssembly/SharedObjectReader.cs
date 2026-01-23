using System.IO;
using System.Text;

public class SharedObjectReader : BinaryReader
{
	public SharedObjectReader(Stream input)
		: base(input)
	{
	}

	public SharedObjectReader(Stream input, Encoding encoding)
		: base(input, encoding)
	{
	}

	public SharedObjectReader(Stream input, Encoding encoding, bool leaveOpen)
		: base(input, encoding, leaveOpen)
	{
	}

	public override short ReadInt16()
	{
		return (short)(((ReadByte() & 0xFF) << 8) | (ReadByte() & 0xFF));
	}

	public override int ReadInt32()
	{
		return ((ReadByte() & 0xFF) << 24) | ((ReadByte() & 0xFF) << 16) | ((ReadByte() & 0xFF) << 8) | (ReadByte() & 0xFF);
	}

	public int ReadCompressedInt32()
	{
		int num = 0;
		bool flag = true;
		int num2 = 0;
		for (int i = 0; i < 3; i++)
		{
			byte b = ReadByte();
			flag = (b & 0x80) == 0;
			num <<= 7;
			num |= b & 0x7F;
			num2 += 7;
			if (flag)
			{
				break;
			}
		}
		if (!flag)
		{
			byte b = ReadByte();
			num <<= 8;
			num |= b;
			num2 += 8;
		}
		if (num >> num2 - 1 == 1 && num2 == 29)
		{
			num = (int)(0L - (long)(uint)(~(num | (-1 << num2)) + 1));
		}
		return num;
	}

	public override string ReadString()
	{
		int length = ReadInt16();
		return ReadString(length);
	}

	public string ReadString(int length)
	{
		byte[] bytes = ReadBytes(length);
		return Encoding.UTF8.GetString(bytes, 0, length);
	}
}
