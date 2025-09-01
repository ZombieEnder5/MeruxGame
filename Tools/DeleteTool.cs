using Merux.Instances;
using Merux.Mathematics;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux.Tools
{
	internal class DeleteTool : ToolScript
	{
		RayHit? hit;

		public override string GetIconPath()
		{
			return "Textures.deleteIcon.png";
		}

		public override void Equip()
		{
			base.Equip();
		}

		public override void Unequip()
		{
			base.Unequip();
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);
			hit = Merux.Game.GetMouseHit();
		}

		public override void Activate()
		{
			if (!hit.HasValue) return;
			var contact = hit.Value;
			if (contact.part.Locked) return;
			contact.part.Destroy();
			AL.SourcePlay(Merux.Game.DST_SND_SRC);
		}
	}
}
