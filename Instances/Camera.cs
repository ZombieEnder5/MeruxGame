using System;
using System.Collections.Generic;
using System.Linq;
using Merux.Mathematics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;

namespace Merux.Instances
{
    public class Camera : Instance
    {
        static double ZOOM_STEP = 5.0;
        public CFrame CFrame = CFrame.Identity;
        Vector3 look = Vector3.Zero;
        Vector3 origin = Vector3.Zero;
        double zoom = 10.0;

        public float FieldOfView = MathF.PI * 70f / 180f;

        public void ZoomIn(int snd)
        {
            zoom -= ZOOM_STEP;
            zoom = Math.Max(zoom, ZOOM_STEP);
			AL.SourcePlay(snd);
		}

        public void ZoomOut(int snd)
        {
            zoom += ZOOM_STEP;
            zoom = Math.Max(zoom, ZOOM_STEP);
            AL.SourcePlay(snd);
        }

        public void TurnCamera(Vector2 mouseDelta)
        {
            Vector2 size = Merux.Game.windowExtents;
            int max = (int)Math.Max(size.X, size.Y);
            mouseDelta *= -5f;
            look += new Vector3(mouseDelta.Y / max, mouseDelta.X / max, 0);
            look.X = Math.Clamp(look.X, -1.4, 1.4);
            look.Y %= Math.PI * 2.0;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            CFrame = CFrame.FromEulerAnglesYXZ(look.X, look.Y, 0.0);
            CFrame += CFrame.ZVector * zoom + origin;
            CFrame.Orthonormalize();
        }

        public void SetLook(Vector3 look)
        {
            this.look = new Vector3(look);
        }
    }
}
