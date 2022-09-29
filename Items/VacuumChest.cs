using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VacuumChest.Items
{
	public class VacuumChest : ModItem
	{

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 26;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = 1;
			Item.consumable = true;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 6, 0);
			Item.createTile = ModContent.TileType<global::VacuumChest.Tiles.VacuumChest>();
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SoulofMight, 5);
			recipe.AddIngredient(ItemID.SoulofFright, 5);
			recipe.AddIngredient(ItemID.SoulofSight, 5);
			recipe.AddIngredient(ItemID.HallowedBar, 20);
			recipe.AddIngredient(ItemID.GoldChest);
			recipe.AddIngredient(ItemID.Wire, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

	}
}
