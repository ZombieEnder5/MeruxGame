using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merux
{
	internal class TextureSystem
	{
		static Dictionary<string, Texture2D> cache = new();

		public static Texture2D GetTexture(string path)
		{
			if (cache.ContainsKey(path)) return cache[path];
			var tex = new Texture2D(Merux.LoadStream(path));
			cache[path] = tex;
			return tex;
		}

		public static void Dispose()
		{
			foreach (var tex in cache.Values)
				tex.Dispose();
		}
	}
}
