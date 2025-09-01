using Merux.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux
{
	public abstract class ToolScript : Instance
	{
		public abstract string GetIconPath();

		public virtual void Equip()
		{
			Merux.Game.MouseLeftClickEvent += Activate;
		}
		public virtual void Unequip()
		{
			Merux.Game.MouseLeftClickEvent -= Activate;
		}
		public virtual void Activate() { }
	}
}
