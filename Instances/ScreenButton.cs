using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merux.Mathematics;

namespace Merux.Instances
{
	internal class ScreenButton : ScreenImage
	{
		bool didYouClickOnMe = false;
		bool hoveringOverMe = false;

		public event Action? OnLeftClick;
		public event Action? OnLeftRelease;

		public ScreenButton()
		{
			Merux.Game.MouseLeftClickEvent += onClick;
			Merux.Game.MouseLeftReleaseEvent += onRelease;
		}

		protected void onClick()
		{
			if (hoveringOverMe)
			{
				didYouClickOnMe = true;
				OnLeftClick?.Invoke();
			}
		}

		protected void onRelease()
		{
			if (hoveringOverMe || didYouClickOnMe)
			{
				didYouClickOnMe = false;
				OnLeftRelease?.Invoke();
			}
		}

		public override void Tick(float deltaTime)
		{
			hoveringOverMe = Merux.Game.guiHovering == this;
			TintAlpha = 0f;
			TintColor = new(0, 0, 0);
			if (hoveringOverMe)
			{
				TintAlpha = didYouClickOnMe ? .4f : .2f;
			}
			base.Tick(deltaTime);
		}

		public override void Dispose()
		{
			if (didYouClickOnMe)
				OnLeftRelease?.Invoke();
			Merux.Game.MouseLeftClickEvent -= onClick;
			Merux.Game.MouseLeftReleaseEvent -= onRelease;
			base.Dispose();
		}
	}
}
