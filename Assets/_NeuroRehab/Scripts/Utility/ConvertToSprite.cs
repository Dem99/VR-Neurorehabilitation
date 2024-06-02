using UnityEngine;

namespace NeuroRehab.Utility {
	public static class ConvertToSprite {
		// https://gist.github.com/nnm-t/0b826365eb66e7c7b61a3f2ecb2765f5
		public static Sprite ConvertTextureToSprite(this Texture2D texture) {
			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
		}
	}
}