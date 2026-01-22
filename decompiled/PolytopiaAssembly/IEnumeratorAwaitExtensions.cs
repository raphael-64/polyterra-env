using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using UnityAsyncAwaitUtil;
using UnityEngine;

public static class IEnumeratorAwaitExtensions
{
	public class SimpleCoroutineAwaiter<T> : INotifyCompletion
	{
		private bool _isDone;

		private Exception _exception;

		private Action _continuation;

		private T _result;

		public bool IsCompleted => _isDone;

		public T GetResult()
		{
			Assert(_isDone, "Trying to get result when awaiter is already completed");
			if (_exception != null)
			{
				ExceptionDispatchInfo.Capture(_exception).Throw();
			}
			return _result;
		}

		public void Complete(T result, Exception e)
		{
			if (e != null)
			{
				Debug.LogError((object)("SimpleCoroutineAwaiter<T> Encountered error " + e.ToString()));
			}
			Assert(!_isDone, "Trying to complete when awaiter is already completed");
			_isDone = true;
			_exception = e;
			_result = result;
			if (_continuation != null)
			{
				RunOnUnityScheduler(_continuation);
			}
		}

		void INotifyCompletion.OnCompleted(Action continuation)
		{
			Assert(_continuation == null, "Continuation has already been set");
			Assert(!_isDone, "Can't set completion when already completed");
			_continuation = continuation;
		}
	}

	public class SimpleCoroutineAwaiter : INotifyCompletion
	{
		private bool _isDone;

		private Exception _exception;

		private Action _continuation;

		private bool _isContinuationSet;

		public bool IsCompleted
		{
			get
			{
				if (_isDone)
				{
					Debug.LogWarningFormat("Coroutine already done when starting. Id: {0} frame: {1}", new object[2]
					{
						GetHashCode(),
						Time.frameCount
					});
				}
				return _isDone;
			}
		}

		public bool IsCompletionSet => _isContinuationSet;

		public void GetResult()
		{
			Assert(_isDone, "Trying to get result when awaiter is already completed");
			if (_exception != null)
			{
				ExceptionDispatchInfo.Capture(_exception).Throw();
			}
		}

		public void Complete(Exception e)
		{
			if (e != null)
			{
				Debug.LogError((object)("SimpleCoroutineAwaiter Encountered error " + e.ToString()));
			}
			Assert(!_isDone, "Trying to complete when awaiter is already completed");
			_isDone = true;
			_exception = e;
			if (_continuation != null)
			{
				RunOnUnityScheduler(_continuation);
			}
		}

		void INotifyCompletion.OnCompleted(Action continuation)
		{
			Assert(_continuation == null, "Continuation has already been set");
			Assert(!_isDone, "Can't set completion when already completed");
			_continuation = continuation;
			_isContinuationSet = true;
		}
	}

	private class CoroutineWrapper<T>
	{
		private readonly SimpleCoroutineAwaiter<T> _awaiter;

		private readonly Stack<IEnumerator> _processStack;

		public CoroutineWrapper(IEnumerator coroutine, SimpleCoroutineAwaiter<T> awaiter)
		{
			_processStack = new Stack<IEnumerator>();
			_processStack.Push(coroutine);
			_awaiter = awaiter;
		}

		public IEnumerator Run()
		{
			IEnumerator enumerator;
			while (true)
			{
				enumerator = _processStack.Peek();
				bool flag;
				try
				{
					flag = !enumerator.MoveNext();
				}
				catch (Exception ex)
				{
					List<Type> list = GenerateObjectTrace(_processStack);
					if (list.Any())
					{
						_awaiter.Complete(default(T), new Exception(GenerateObjectTraceMessage(list), ex));
					}
					else
					{
						_awaiter.Complete(default(T), ex);
					}
					yield break;
				}
				if (flag)
				{
					_processStack.Pop();
					if (_processStack.Count == 0)
					{
						break;
					}
				}
				if (enumerator.Current is IEnumerator)
				{
					_processStack.Push((IEnumerator)enumerator.Current);
				}
				else
				{
					yield return enumerator.Current;
				}
			}
			_awaiter.Complete((T)enumerator.Current, null);
		}

		private string GenerateObjectTraceMessage(List<Type> objTrace)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Type item in objTrace)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(" -> ");
				}
				stringBuilder.Append(item.ToString());
			}
			stringBuilder.AppendLine();
			return "Unity Coroutine Object Trace: " + stringBuilder.ToString();
		}

		private static List<Type> GenerateObjectTrace(IEnumerable<IEnumerator> enumerators)
		{
			List<Type> list = new List<Type>();
			foreach (IEnumerator enumerator2 in enumerators)
			{
				FieldInfo field = enumerator2.GetType().GetField("$this", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null)
				{
					continue;
				}
				object value = field.GetValue(enumerator2);
				if (value != null)
				{
					Type type = value.GetType();
					if (!list.Any() || type != list.Last())
					{
						list.Add(type);
					}
				}
			}
			list.Reverse();
			return list;
		}
	}

	private static class InstructionWrappers
	{
		public static IEnumerator ReturnVoid(SimpleCoroutineAwaiter awaiter, object instruction)
		{
			yield return instruction;
			int loopBreaker = 100;
			while (loopBreaker-- > 0 && !awaiter.IsCompletionSet)
			{
				Log.Warning("Completion is not set yet, waiting", Array.Empty<object>());
				yield return null;
			}
			awaiter.Complete(null);
		}

		public static IEnumerator AssetBundleCreateRequest(SimpleCoroutineAwaiter<AssetBundle> awaiter, AssetBundleCreateRequest instruction)
		{
			yield return instruction;
			awaiter.Complete(instruction.assetBundle, null);
		}

		public static IEnumerator ReturnSelf<T>(SimpleCoroutineAwaiter<T> awaiter, T instruction)
		{
			yield return instruction;
			awaiter.Complete(instruction, null);
		}

		public static IEnumerator AssetBundleRequest(SimpleCoroutineAwaiter<Object> awaiter, AssetBundleRequest instruction)
		{
			yield return instruction;
			awaiter.Complete(instruction.asset, null);
		}

		public static IEnumerator ResourceRequest(SimpleCoroutineAwaiter<Object> awaiter, ResourceRequest instruction)
		{
			yield return instruction;
			awaiter.Complete(instruction.asset, null);
		}
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitForSeconds instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitForUpdate instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitForEndOfFrame instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitForFixedUpdate instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitForSecondsRealtime instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitUntil instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter GetAwaiter(this WaitWhile instruction)
	{
		return GetAwaiterReturnVoid(instruction);
	}

	public static SimpleCoroutineAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation instruction)
	{
		return GetAwaiterReturnSelf<AsyncOperation>(instruction);
	}

	public static SimpleCoroutineAwaiter<Object> GetAwaiter(this ResourceRequest instruction)
	{
		SimpleCoroutineAwaiter<Object> awaiter = new SimpleCoroutineAwaiter<Object>();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(InstructionWrappers.ResourceRequest(awaiter, instruction));
		});
		return awaiter;
	}

	public static SimpleCoroutineAwaiter<WWW> GetAwaiter(this WWW instruction)
	{
		return GetAwaiterReturnSelf<WWW>(instruction);
	}

	public static SimpleCoroutineAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
	{
		SimpleCoroutineAwaiter<AssetBundle> awaiter = new SimpleCoroutineAwaiter<AssetBundle>();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(InstructionWrappers.AssetBundleCreateRequest(awaiter, instruction));
		});
		return awaiter;
	}

	public static SimpleCoroutineAwaiter<Object> GetAwaiter(this AssetBundleRequest instruction)
	{
		SimpleCoroutineAwaiter<Object> awaiter = new SimpleCoroutineAwaiter<Object>();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(InstructionWrappers.AssetBundleRequest(awaiter, instruction));
		});
		return awaiter;
	}

	public static SimpleCoroutineAwaiter<T> GetAwaiter<T>(this IEnumerator<T> coroutine)
	{
		SimpleCoroutineAwaiter<T> awaiter = new SimpleCoroutineAwaiter<T>();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(new CoroutineWrapper<T>(coroutine, awaiter).Run());
		});
		return awaiter;
	}

	public static SimpleCoroutineAwaiter<object> GetAwaiter(this IEnumerator coroutine)
	{
		SimpleCoroutineAwaiter<object> awaiter = new SimpleCoroutineAwaiter<object>();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(new CoroutineWrapper<object>(coroutine, awaiter).Run());
		});
		return awaiter;
	}

	private static SimpleCoroutineAwaiter GetAwaiterReturnVoid(object instruction)
	{
		SimpleCoroutineAwaiter awaiter = new SimpleCoroutineAwaiter();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(InstructionWrappers.ReturnVoid(awaiter, instruction));
		});
		return awaiter;
	}

	private static SimpleCoroutineAwaiter<T> GetAwaiterReturnSelf<T>(T instruction)
	{
		SimpleCoroutineAwaiter<T> awaiter = new SimpleCoroutineAwaiter<T>();
		RunOnUnityScheduler(delegate
		{
			((MonoBehaviour)AsyncCoroutineRunner.Instance).StartCoroutine(InstructionWrappers.ReturnSelf(awaiter, instruction));
		});
		return awaiter;
	}

	private static void RunOnUnityScheduler(Action action)
	{
		if (SynchronizationContext.Current == SyncContextUtil.UnitySynchronizationContext)
		{
			action();
			return;
		}
		SyncContextUtil.UnitySynchronizationContext.Post(delegate
		{
			action();
		}, null);
	}

	private static void Assert(bool condition, string description)
	{
		if (!condition)
		{
			throw new Exception($"Assert hit in UnityAsyncUtil package! reason: {description} frame {Time.frameCount}");
		}
	}
}
