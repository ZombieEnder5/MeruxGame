using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Merux.Instances
{
	public abstract class Instance: IDisposable
	{
		public string Name = "Instance";
		private List<Instance> children = new List<Instance>();
		private List<Instance> descendants = new List<Instance>();
		private List<Instance> ancestors = new List<Instance>();
		private Instance? _parent = null;
		public Instance? Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				if (value == _parent) return;
				var old = new List<Instance>(ancestors);
				if (_parent != null)
				{
					_parent.children.Remove(this);
					_parent.descendants.Remove(this);
					descendants.ForEach((o) =>
					{
						_parent.descendants.Remove(o);
						o.ancestors.RemoveAll(v => ancestors.Contains(v));
					});
				}
				_parent = value;
				if (_parent != null)
				{
					_parent.children.Add(this);
					_parent.descendants.Add(this);
					ancestors = new() { _parent };
					ancestors.AddRange(_parent.ancestors);
					descendants.ForEach((o) =>
					{
						_parent.descendants.Add(o);
						o.ancestors.AddRange(ancestors);
						foreach (var anc in old.Where(v => !ancestors.Contains(v)))
							anc.DescendantRemoving?.Invoke(null, o);
						foreach (var anc in ancestors.Where(o => !old.Contains(o)))
							anc.DescendantAdded?.Invoke(null, o);
					});
					foreach (var anc in old.Where(o => !ancestors.Contains(o)))
						anc.DescendantRemoving?.Invoke(null, this);
					foreach (var anc in ancestors.Where(o => !old.Contains(o)))
						anc.DescendantAdded?.Invoke(null, this);
				}
			}
		}

		public event EventHandler<Instance>? DescendantAdded;
		public event EventHandler<Instance>? DescendantRemoving;

		protected Instance()
		{
		}

		public static T Create<T>(Instance? parent) where T : Instance
		{
			Type type = typeof(T);
			ConstructorInfo? ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			if (ctor == null) throw new Exception("how did we even get here?");
			T inst = (T)ctor.Invoke(null)!;
			inst.Parent = parent;
			return inst;
		}

		public static T Create<T>() where T : Instance
		{
			return Create<T>(null);
		}

		public void Destroy()
		{
			Parent = null;
			Dispose();
		}

		public bool IsDescendantOf(Instance ancestor)
		{
			return ancestors.Contains(ancestor);
		}

		public Instance? FindFirstChild(string name)
		{
			foreach (var child in children)
				if (child.Name == name)
					return child;
			return null;
		}

		public Instance? FindFirstChild(Type type)
		{
			foreach (var child in children)
				if (child.GetType() == type)
					return child;
			return null;
		}

		public List<T> GetDescendantsOfClass<T>()
		{
			var list = new List<T>();
			foreach (var desc in descendants)
				if (desc is T match)
					list.Add(match);
			return list;
		}

		public List<Instance> GetDescendants()
		{
			return new List<Instance>(descendants);
		}

		public virtual void Tick(float deltaTime)
		{
			foreach (var child in children)
				child.Tick(deltaTime);
		}
		public virtual void Dispose()
		{
			foreach (var c in children)
				c.Dispose();
		}
	}
}
