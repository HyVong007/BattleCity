using BattleCity.Tanks;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;


namespace BattleCity.Items
{
	public sealed class Ship : Item
	{
		[SerializeField]
		[HideInInspector]
		private SpriteRenderer spriteRenderer;
		private void Reset()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}


		public override void OnCollision(Tank tank)
		{
			if (tank.ship)
			{
				Destroy(gameObject);
				return;
			}

			if (this == current) current = null;
			tank.ship = this;
			transform.parent = tank.transform;
			transform.localPosition = default;
			GetComponent<Animator>().enabled = false;
			spriteRenderer.enabled = true;
			ChangeColor(tank.color);
		}


		[SerializeField] private SerializableDictionaryBase<Color, Sprite> sprites;
		public void ChangeColor(Color color)
		{
			spriteRenderer.sprite = sprites[color];
		}
	}
}