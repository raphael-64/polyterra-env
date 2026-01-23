using System;
using System.Collections;
using System.Collections.Generic;

public class CircularBuffer<T> : IEnumerable<T>, IEnumerable
{
	private readonly T[] elements;

	private int start;

	private int end;

	private int count;

	private readonly int capacity;

	public T this[int i] => elements[(start + 1) % capacity];

	public int Count => count;

	public T First => elements[start];

	public T Last => elements[(start + count - 1) % capacity];

	public bool IsFull => count == capacity;

	public CircularBuffer(int capacity)
	{
		elements = new T[capacity];
		this.capacity = capacity;
	}

	public void Add(T element)
	{
		if (count == capacity)
		{
			throw new ArgumentException();
		}
		elements[end] = element;
		end = (end + 1) % capacity;
		count++;
	}

	public void FastClear()
	{
		start = 0;
		end = 0;
		count = 0;
	}

	public void RemoveFromStart(int count)
	{
		if (count > capacity || count > this.count)
		{
			throw new ArgumentException();
		}
		start = (start + count) % capacity;
		this.count -= count;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int counter = start; counter != end; counter = (counter + 1) % capacity)
		{
			yield return elements[counter];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
