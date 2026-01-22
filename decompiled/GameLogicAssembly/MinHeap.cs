public class MinHeap<T>
{
	public class Node
	{
		public T Data { get; }

		public float ExpectedCost { get; set; }

		public Node Next { get; set; }

		public Node(T data, float expectedCost)
		{
			Data = data;
			ExpectedCost = expectedCost;
		}
	}

	private Node head;

	public bool HasNext()
	{
		return head != null;
	}

	public void Push(Node node)
	{
		if (head == null)
		{
			head = node;
			return;
		}
		if (node.ExpectedCost < head.ExpectedCost)
		{
			node.Next = head;
			head = node;
			return;
		}
		Node next = head;
		while (next.Next != null && next.Next.ExpectedCost <= node.ExpectedCost)
		{
			next = next.Next;
		}
		node.Next = next.Next;
		next.Next = node;
	}

	public Node Pop()
	{
		Node result = head;
		head = head.Next;
		return result;
	}
}
