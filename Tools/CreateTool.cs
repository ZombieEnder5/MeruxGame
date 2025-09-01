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
	internal class CreateTool : ToolScript
	{
		RayHit? hit;

		public override string GetIconPath()
		{
			return "Textures.createIcon.png";
		}

		public override void Equip()
		{
			base.Equip();
			Merux.Game.selector.Transparency = 0.5f;
		}

		public override void Unequip()
		{
			base.Unequip();
			Merux.Game.selector.Transparency = 1f;
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);
			hit = Merux.Game.GetMouseHit();
			if (!hit.HasValue) return;
			var contact = hit.Value;
			var half = Merux.Game.selector.Size * .5;
			Vector3 x = half.X * Vector3.XAxis, y = half.Y * Vector3.YAxis, z = half.Z * Vector3.ZAxis;
			var vert = Merux.Game.EpsilonMaxBy(new[] {
					-x - y - z,
					-x - y + z,
					-x + y - z,
					-x + y + z,
					x - y - z,
					x - y + z,
					x + y - z,
					x + y + z,
				}, v => v.Dot(contact.normal), 1e-2);
			Merux.Game.selector.Position = contact.point + vert;
		}

		public override void Activate()
		{
			if (!hit.HasValue) return;
			var inst = Create<Part>(Merux.Game.Workspace);
			inst.Position = Merux.Game.selector.Position;
			inst.Color = Merux.Game.selector.Color;
			inst.Size = new Vector3(4, 1, 2);
			Merux.Game.selector.Color = new Vector3(Merux.Game.RandomSource.NextDouble(), Merux.Game.RandomSource.NextDouble(), Merux.Game.RandomSource.NextDouble());
			AL.SourcePlay(Merux.Game.PUT_SND_SRC);
		}
	}
}
