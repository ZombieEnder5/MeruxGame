using Merux.Instances;
using Merux.Mathematics;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux
{
	public abstract class ToolScript : Instance
	{
		bool activated = false;
		bool equipped = false;
		internal readonly ScreenButton icon = new ScreenButton();
		ScreenImage number;

		public event Action? OnEquip;
		public event Action? OnUnequip;

		public ToolScript()
		{
			var pos = new GuiDim(0, 1, Merux.Game.GetToolCount() * Game.TOOL_ICON_SIZE, 0);
			icon.Parent = this;
			icon.Texture = TextureSystem.GetTexture(GetIconPath());
			icon.Position = pos;
			icon.Size = Vector2.One * Game.TOOL_ICON_SIZE;
			icon.AnchorPoint = Vector2.YAxis;
			icon.BackgroundColor = new(1, 1, 1);
			number = Merux.Game.textRenderer48.RenderScreenImage<ScreenImage>((Merux.Game.GetToolCount() + 1).ToString(), 3, 5);
			number.Parent = this;
			number.Position = pos;
			number.AnchorPoint = Vector2.YAxis;

			icon.OnLeftClick += () =>
			{
				Merux.Game.ToggleTool(this);
			};

			OnEquip += onEquip;
			OnUnequip += onUnequip;
		}
		public void Render()
		{
			//icon.Render();
			//number.Render();
		}
		protected virtual void activate()
		{
		}
		protected virtual void deactivate()
		{
		}

		public abstract string GetIconPath();

		protected virtual void onEquip()
		{
			equipped = true;
			icon.BackgroundTransparency = 0.5f;
			AL.SourcePlay(Merux.Game.CLK_SND_SRC);
			Merux.Game.MouseLeftClickEvent += Activate;
			Merux.Game.MouseLeftReleaseEvent += Deactivate;
		}
		protected virtual void onUnequip()
		{
			equipped = false;
			icon.BackgroundTransparency = 1f;
			Deactivate();
			Merux.Game.MouseLeftClickEvent -= Activate;
			Merux.Game.MouseLeftReleaseEvent -= Deactivate;
		}
		public void Activate()
		{
			if (!equipped) return;
			if (Merux.Game.HoveringOverGui()) return;
			activated = true;
			activate();
		}
		public void Deactivate()
		{
			if (!equipped) return;
			if (Merux.Game.HoveringOverGui()) return;
			if (!activated) return;
			activated = false;
			deactivate();
		}
		public override void Dispose()
		{
			base.Dispose();
		}
		internal void invokeEquip()
		{
			OnEquip?.Invoke();
		}
		internal void invokeUnequip()
		{
			OnUnequip?.Invoke();
		}
	}
}
