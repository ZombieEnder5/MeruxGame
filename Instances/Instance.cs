using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux.Instances
{
	public abstract class Instance: IDisposable
	{
		public string Name;
		private List<Instance> children = new List<Instance>();
		private List<Instance> descendants = new List<Instance>();
		private List<Instance> ancestors = new List<Instance>();
		private Instance? parent = null;

		public event EventHandler<Instance> DescendantAdded;
		public event EventHandler<Instance> DescendantRemoving;

		public Instance()
		{
			Name = "Instance";
		}

		public void Destroy()
		{
			SetParent(null);
			Dispose();
		}

		public bool IsDescendantOf(Instance ancestor)
		{
			return ancestors.Contains(ancestor);
		}

		public void SetParent(Instance? parent)
		{
			if (parent == this.parent) return;
			var old = new List<Instance>(ancestors);
			if (this.parent != null)
			{
				this.parent.children.Remove(this);
				this.parent.descendants.Remove(this);
				descendants.ForEach((o) =>
				{
					this.parent.descendants.Remove(o);
					o.ancestors.RemoveAll(v => ancestors.Contains(v));
				});
			}
			this.parent = parent;
			if (parent != null)
			{
				parent.children.Add(this);
				parent.descendants.Add(this);
				ancestors = new() { parent };
				ancestors.AddRange(parent.ancestors);
				descendants.ForEach((o) =>
				{
					parent.descendants.Add(o);
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

		public Instance? GetParent()
		{
			return parent;
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
