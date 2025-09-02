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
		private RayHit? hit;
		private Part? last = null;
		private bool equipped = false;

		public override string GetIconPath()
		{
			return "Textures.deleteIcon.png";
		}

		protected override void onEquip()
		{
			base.onEquip();
			equipped = true;
			Merux.Game.MouseIcon = TextureSystem.GetTexture("Textures.cursor.trash.png");
		}

		protected override void onUnequip()
		{
			base.onUnequip();
			equipped = false;
			last = null;
			if (hit.HasValue)
				hit.Value.part.BorderColor = new Vector3(0, 0, 0);
			Merux.Game.MouseIcon = TextureSystem.GetTexture("Textures.cursor.pointer.png");
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);
			if (!equipped) return;
			hit = Merux.Game.GetMouseHit();
			if (hit.HasValue)
			{
				if (last == null && !hit.Value.part.Locked)
					hit.Value.part.BorderColor = new Vector3(1, 0, 0);
				else if (last != null && hit.Value.part != last)
				{
					last.BorderColor = Vector3.Zero;
					if (!hit.Value.part.Locked)
						hit.Value.part.BorderColor = new Vector3(1, 0, 0);
				}
				last = hit.Value.part;
			} else if (last != null)
				last.BorderColor = Vector3.Zero;
		}

		protected override void activate()
		{
			if (!hit.HasValue) return;
			var contact = hit.Value;
			if (contact.part.Locked) return;
			contact.part.Destroy();
			AL.SourcePlay(Merux.Game.DST_SND_SRC);
		}
	}
}
