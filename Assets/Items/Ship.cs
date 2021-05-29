using BattleCity.Tanks;
using RotaryHeart.Lib.SerializableDictionary;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Ship : Item
	{
		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private SerializableDictionaryBase<Tank.Color, Sprite> sprites;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ChangeColor(Tank.Color color) => spriteRenderer.sprite = sprites[color];


		public override void OnCollision(Tank tank)
		{
			if (tank.ship)
			{
				Destroy(gameObject);
				return;
			}

			current = null;
			tank.ship = this;
			Destroy(GetComponent<Animator>());
			spriteRenderer.enabled = true;
			ChangeColor(tank.color);
			transform.SetParent(tank.transform);
			transform.localPosition = default;
		}
	}
}